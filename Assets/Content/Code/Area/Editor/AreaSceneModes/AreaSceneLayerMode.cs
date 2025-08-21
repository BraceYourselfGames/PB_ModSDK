using System.Collections.Generic;
using System.Linq;

using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Area
{
    using Scene;

    sealed class AreaSceneLayerMode : AreaSceneMode
    {
        public EditingMode EditingMode => EditingMode.Layers;

        public AreaSceneModePanel Panel { get; }

        public void OnDisable () => Panel.OnDisable ();
        public void OnDestroy ()
        {
            if (useSlice)
            {
                CombatSceneHelper.ins.materialHelper.RestoreSliceSettings ();
            }
            cellVisuals.OnDestroy ();
            floor.OnDestroy ();
            bb.am.ApplyPropVisibilityEverywhere(0);
            EnableBlockColliders ();
            bb.selectionLayer = -1;
            bb.selectedSpots.Clear ();
            bb.markedSpotsByLayer.Clear ();
            bb.onLevelLoaded -= OnLevelLoaded;
            bb.onEditingModeChanged -= OnEditingModeChanged;
        }

        public int LayerMask => AreaSceneCamera.volumeCollidersLayerMask;

        public bool Hover (Event e, RaycastHit hitInfo)
        {
            if (!allSystemsGo)
            {
                return false;
            }
            if (!floor.HitTest (hitInfo))
            {
                return false;
            }

            var spotIndex = GetSpotIndex (currentLayer, hitInfo.point, bb.am.boundsFull);
            bb.spotInfo.Update (spotIndex);
            DisplayCellCursor (spotIndex, hitInfo);
            switch (e.type)
            {
                case EventType.MouseDown:
                    HandleMouseDown (spotIndex, e);
                    return true;
                case EventType.MouseUp:
                    HandleMouseUp (e);
                    return true;
                case EventType.MouseDrag:
                    HandleMouseDrag (spotIndex, e);
                    break;
                case EventType.ScrollWheel:
                    HandleScrollWheel (e);
                    return true;
                case EventType.KeyUp:
                    return HandleSceneUserInput (e);
            }
            return false;
        }

        public void OnHoverEnd ()
        {
            if (!allSystemsGo)
            {
                return;
            }
            bb.gizmos.cursor.HideCursor ();
            cellVisuals.marking = CellObjectOperation.None;
        }

        public bool HandleSceneUserInput (Event e)
        {
            if (!allSystemsGo)
            {
                return false;
            }

            var isKeyUp = !e.shift && !e.control && e.type == EventType.KeyUp;
            if (!isKeyUp)
            {
                return false;
            }
            switch (e.keyCode)
            {
                case KeyCode.Minus:
                case KeyCode.Equals:
                    return TryChangeLayer(e.keyCode);
                case KeyCode.Z when e.alt:
                    AreaSceneModeHelper.LookDownNorthAligned (bb.lastCellHovered);
                    return true;
                case KeyCode.X when e.alt && currentLayer != -1 && nudge != 0f:
                    nudge = 0f;
                    ChangeSpotVisiblity (currentLayer);
                    return true;
                case KeyCode.V when e.alt && currentLayer != -1 && nudge == 0f:
                    nudge = -0.000001f;
                    ChangeSpotVisiblity (currentLayer);
                    return true;
            }
            return false;
        }

        public void DrawSceneMarkup (Event e, System.Action repaint)
        {
            if (!allSystemsGo)
            {
                return;
            }

            ops.TryRebuild (ref currentLayer);
            floor.Draw ();

            if (bb.layer == currentLayer)
            {
                if (bb.showPropsOnLayer != lastShowPropsOnLayer)
                {
                    lastShowPropsOnLayer = bb.showPropsOnLayer;
                    bb.am.ApplyPropVisibilityEverywhere(currentLayer + (lastShowPropsOnLayer ? 0 : 1));
                }
                cellVisuals.DisplayCellObjects (currentLayer);
                return;
            }

            currentLayer = bb.layer;
            ChangeSpotVisiblity (currentLayer);
            cellVisuals.DisplayCellObjects (currentLayer);
            floor.Move (currentLayer);
        }

        static int GetSpotIndex (int layer, Vector3 hitPoint, Vector3Int bounds)
        {
            var offset = WorldSpace.SpotOffsetFromPoint;
            offset.y = 0f;
            hitPoint.y = layer * -WorldSpace.BlockSize;
            return AreaUtility.GetIndexFromWorldPosition (hitPoint, offset, bounds);
        }

        void HandleMouseDown (int spotIndex, Event e)
        {
            if (e.button != 0)
            {
                cellVisuals.selecting = CellObjectOperation.None;
                cellVisuals.marking = CellObjectOperation.None;
                return;
            }

            if (e.control && bb.enableLayerDiagnostics)
            {
                cellVisuals.HandleCellMark (currentLayer, spotIndex);
                return;
            }

            if (bb.cellEmpty)
            {
                if (!e.shift)
                {
                    cellVisuals.ClearSelection ();
                }
                return;
            }
            if (bb.selectedSpots.Count != 0 && currentLayer != bb.selectionLayer)
            {
                cellVisuals.ClearSelection ();
            }
            else if (currentLayer == bb.selectionLayer)
            {
                var found = bb.selectedSpots.Contains (spotIndex);
                if (!e.shift && !found)
                {
                    cellVisuals.ClearSelection ();
                }
                else if (found)
                {
                    bb.selectedSpots.Remove (spotIndex);
                    cellVisuals.selecting = CellObjectOperation.Remove;
                    return;
                }
            }
            bb.selectionLayer = currentLayer;
            bb.selectedSpots.Add (spotIndex);
            bb.selectedSpots.Sort ();
            cellVisuals.selecting = CellObjectOperation.Add;
        }

        void HandleMouseUp (Event e)
        {
            if (e.button != 0)
            {
                return;
            }
            cellVisuals.selecting = CellObjectOperation.None;
            cellVisuals.marking = CellObjectOperation.None;
        }

        void HandleMouseDrag (int spotIndex, Event e)
        {
            cellVisuals.HandleDragSelecting (spotIndex);
            cellVisuals.HandleDragMarking (currentLayer, spotIndex, e);
        }

        void HandleScrollWheel (Event e)
        {
            var keyCode = e.delta.y > 0f ? KeyCode.Minus : KeyCode.Equals;
            TryChangeLayer (keyCode);
        }

        void DisplayCellCursor (int spotIndex, RaycastHit hitInfo)
        {
            if (!spotIndex.IsValidIndex (bb.am.points))
            {
                return;
            }
            var point = bb.am.points[spotIndex];
            bb.gizmos.cursor.SetCursor (cursorID);
            bb.gizmos.cursor.ShowCursor(point, hitInfo);
            bb.lastCellHovered = point;
            bb.lastCellInspected = null;
        }

        bool TryChangeLayer (KeyCode keyCode)
        {
            var buttonPressed = KeyCode.None;
            ref var layer = ref bb.layer;
            if (keyCode == KeyCode.Minus && layer > 0)
            {
                layer -= 1;
                buttonPressed = keyCode;
                bb.lastCellHovered = null;
            }
            else if (keyCode == KeyCode.Equals && layer < bb.am.boundsFull.y - 1)
            {
                layer += 1;
                buttonPressed = keyCode;
                bb.lastCellHovered = null;
            }
            return buttonPressed != KeyCode.None;
        }

        void EnableBlockColliders ()
        {
            if (blockCollidersActive)
            {
                return;
            }

            var am = bb.am;
            foreach (var point in am.points)
            {
                if (point.pointState == AreaVolumePointState.Empty)
                {
                    continue;
                }
                if (point.instanceCollider == null)
                {
                    continue;
                }
                if (point.instanceCollider.activeSelf)
                {
                    continue;
                }
                point.instanceCollider.SetActive (true);
            }
            blockCollidersActive = true;
        }

        void DisableBlockColliders ()
        {
            if (!blockCollidersActive)
            {
                return;
            }

            var am = bb.am;
            foreach (var point in am.points)
            {
                if (point.instanceCollider == null)
                {
                    continue;
                }
                if (!point.instanceCollider.activeSelf)
                {
                    continue;
                }
                point.instanceCollider.SetActive (false);
            }
            blockCollidersActive = false;
        }

        void ChangeSpotVisiblity (int layer)
        {
            var am = bb.am;
            if (am == null)
            {
                return;
            }
            am.ApplyPropVisibilityEverywhere(layer + (lastShowPropsOnLayer ? 0 : 1));
            var sliceDepth = -layer * 3f + nudge;
            CombatSceneHelper.ins.materialHelper.SetupSlicingForLayerMode(true, sliceDepth, sliceColor);
        }

        void OnLevelLoaded ()
        {
            if (bb.editingMode != EditingMode.Layers && lastEditingMode != EditingMode.Layers)
            {
                return;
            }
            if (bb.editingMode != EditingMode.Layers && lastEditingMode == EditingMode.Layers)
            {
                ExitMode ();
                lastEditingMode = bb.editingMode;
                return;
            }
            if (bb.editingMode == EditingMode.Layers && lastEditingMode != EditingMode.Layers)
            {
                EnterMode ();
                lastEditingMode = bb.editingMode;
                return;
            }

            if (!allSystemsGo)
            {
                return;
            }
            bb.spotChange = SpotChange.None;
            bb.lastCellInspected = null;
            bb.selectionLayer = -1;
            if (bb.selectedSpots.Count != 0)
            {
                bb.selectedSpots.Clear ();
            }
            bb.manualSpotMarking = false;
            if (bb.markedSpotsByLayer.Count != 0)
            {
                bb.markedSpotsByLayer.Clear ();
            }

            if (currentLayer == -1)
            {
                return;
            }
            ChangeSpotVisiblity (currentLayer);
        }

        void OnEditingModeChanged ()
        {
            if (bb.editingMode != EditingMode.Layers && lastEditingMode != EditingMode.Layers)
            {
                return;
            }
            if (bb.editingMode == EditingMode.Layers && lastEditingMode == EditingMode.Layers)
            {
                return;
            }
            lastEditingMode = bb.editingMode;
            if (bb.editingMode == EditingMode.Layers)
            {
                EnterMode ();
                return;
            }
            ExitMode ();
        }

        void EnterMode ()
        {
            if (useSlice)
            {
                CombatSceneHelper.ins.materialHelper.CacheSliceSettings ();
            }
            allSystemsGo = allSystemsGo && floor.Load (currentLayer);
            if (!allSystemsGo)
            {
                return;
            }
            cellVisuals.Load ();
            DisableBlockColliders ();
            lastShowPropsOnLayer = bb.showPropsOnLayer;
            ChangeSpotVisiblity (currentLayer == -1 ? 0 : currentLayer);
        }

        void ExitMode ()
        {
            if (useSlice)
            {
                CombatSceneHelper.ins.materialHelper.RestoreSliceSettings ();
            }
            if (!allSystemsGo)
            {
                return;
            }
            floor.Hide ();
            bb.am.ApplyPropVisibilityEverywhere(0);
            EnableBlockColliders ();
            cellVisuals.Hide ();
        }

        public static AreaSceneMode CreateInstance (AreaSceneBlackboard bb) => new AreaSceneLayerMode (bb);

        AreaSceneLayerMode (AreaSceneBlackboard bb)
        {
            this.bb = bb;
            cellVisuals = new AreaSceneLayerModeCellVisuals (bb);
            floor = new AreaSceneLayerModeFloor (bb);
            ops = new AreaSceneLayerModeOperations (bb);
            var cursor = new AreaSceneLayerCellCursor (bb);
            cursorID = cursor.ID;
            Panel = AreaSceneLayerModePanel.Create (bb);
            bb.onLevelLoaded += OnLevelLoaded;
            bb.onEditingModeChanged += OnEditingModeChanged;
            lastEditingMode = bb.editingMode;
            useSlice = CombatSceneHelper.ins != null && CombatSceneHelper.ins.materialHelper != null;
            if (useSlice)
            {
                CombatSceneHelper.ins.materialHelper.CacheSliceSettings ();
            }
            allSystemsGo = useSlice;
            if (!allSystemsGo)
            {
                Debug.LogWarning ("Layer mode is not operational -- requires CombatMaterialHelper which is null");
            }
        }

        readonly AreaSceneBlackboard bb;
        readonly AreaSceneLayerModeCellVisuals cellVisuals;
        readonly AreaSceneLayerModeFloor floor;
        readonly AreaSceneLayerModeOperations ops;
        readonly int cursorID;
        readonly bool useSlice;

        bool allSystemsGo;
        int currentLayer = -1;
        float nudge;
        bool blockCollidersActive = true;
        EditingMode lastEditingMode;
        bool lastShowPropsOnLayer;

        static readonly HSBColor sliceColor = new HSBColor (0f, 0f, 0.18f, 1f);
    }

    enum CellObjectOperation
    {
        None = 0,
        Add,
        Remove,
    }

    sealed class AreaSceneLayerModeCellVisuals
    {
        public void OnDestroy ()
        {
            if (holderSelection != null)
            {
                Object.DestroyImmediate (holderSelection);
            }
            if (cellSelectionMaterial != null)
            {
                Object.DestroyImmediate (cellSelectionMaterial);
            }
            if (holderMarks != null)
            {
                Object.DestroyImmediate (holderMarks);
            }
            if (cellMarkMaterial != null)
            {
                Object.DestroyImmediate (cellMarkMaterial);
            }
        }

        public void Load ()
        {
            LoadCellSelectionObject ();
            LoadCellMarkObjects ();
        }

        public void Hide ()
        {
            HideCellObjects (holderSelection);
            HideCellObjects (holderMarks);
        }

        public void HandleCellMark (int currentLayer, int spotIndex)
        {
            if (bb.markedSpotsByLayer.TryGetValue (currentLayer, out var spots))
            {
                if (marking == CellObjectOperation.None && spots.Remove (spotIndex))
                {
                    marking = CellObjectOperation.Remove;
                    return;
                }
                if (marking == CellObjectOperation.Remove)
                {
                    spots.Remove (spotIndex);
                    marking = CellObjectOperation.Remove;
                    return;
                }
                if (spots.Contains (spotIndex))
                {
                    bb.manualSpotMarking = true;
                    marking = CellObjectOperation.Add;
                    return;
                }
            }
            else if (marking == CellObjectOperation.Remove)
            {
                return;
            }
            else
            {
                spots = new HashSet<int> ();
                bb.markedSpotsByLayer[currentLayer] = spots;
            }
            spots.Add (spotIndex);
            bb.manualSpotMarking = true;
            marking = CellObjectOperation.Add;
        }

        public void DisplayCellObjects (int currentLayer)
        {
            DisplaySelection (currentLayer);
            DisplayCellMarks (currentLayer);
        }

        public void ClearSelection ()
        {
            var t = holderSelection.transform;
            for (var i = 0; i < t.childCount; i += 1)
            {
                var child = t.GetChild (i).gameObject;
                if (child.activeSelf)
                {
                    child.SetActive (false);
                }
            }
            bb.selectedSpots.Clear ();
        }

        public void HandleDragSelecting (int spotIndex)
        {
            if (bb.cellEmpty)
            {
                return;
            }
            switch (selecting)
            {
                case CellObjectOperation.Add:
                    if (!bb.selectedSpots.Contains (spotIndex))
                    {
                        bb.selectedSpots.Add (spotIndex);
                        bb.selectedSpots.Sort ();
                    }
                    return;
                case CellObjectOperation.Remove:
                    bb.selectedSpots.Remove (spotIndex);
                    break;
            }
        }

        public void HandleDragMarking (int currentLayer, int spotIndex, Event e)
        {
            if (!e.control || !bb.enableLayerDiagnostics)
            {
                marking = CellObjectOperation.None;
            }
            if (marking == CellObjectOperation.None)
            {
                return;
            }
            HandleCellMark (currentLayer, spotIndex);
        }

        void DisplaySelection (int currentLayer)
        {
            if (holderSelection == null)
            {
                Debug.LogWarning ("Missing holder for cell selection");
                return;
            }
            if (prefabSelection == null)
            {
                Debug.LogWarning ("Cell selection prefab not loaded");
                return;
            }
            if (currentLayer != bb.selectionLayer && bb.selectedSpots.Count != 0)
            {
                HideCellObjects (holderSelection);
                return;
            }
            if (bb.selectedSpots.Count == 0)
            {
                HideCellObjects (holderSelection);
                return;
            }
            DisplayCellObjects (holderSelection, bb.selectedSpots, InstantiateSelectionObject);
        }

        static void HideCellObjects (GameObject holder)
        {
            if (holder == null || !holder.activeSelf)
            {
                return;
            }
            holder.SetActive (false);
        }

        void DisplayCellObjects (GameObject holder, IEnumerable<int> spotIndexes, System.Func<GameObject> instantiateCellObject)
        {
            if (!holder.activeSelf)
            {
                holder.SetActive (true);
            }

            var t = holder.transform;
            var points = bb.am.points;
            var cellObjectIndex = 0;
            foreach (var spotIndex in spotIndexes)
            {
                var cellObject = cellObjectIndex + 1 > t.childCount
                    ? instantiateCellObject ()
                    : t.GetChild (cellObjectIndex).gameObject;
                var spot = points[spotIndex];
                cellObject.transform.position = spot.instancePosition;
                if (!cellObject.activeSelf)
                {
                    cellObject.SetActive (true);
                }
                cellObjectIndex += 1;
            }
            for (; cellObjectIndex < t.childCount; cellObjectIndex += 1)
            {
                var cellObject = t.GetChild(cellObjectIndex).gameObject;
                if (cellObject.activeSelf)
                {
                    cellObject.SetActive (false);
                }
            }
        }

        void DisplayCellMarks (int currentLayer)
        {
            if (holderMarks == null)
            {
                Debug.LogWarning ("Missing holder for cell marks");
                return;
            }
            if (prefabCellMark == null)
            {
                Debug.LogWarning ("Cell mark prefab not loaded");
                return;
            }
            if (!bb.enableLayerDiagnostics || !bb.markedSpotsByLayer.TryGetValue(currentLayer, out var marks) || marks.Count == 0)
            {
                HideCellObjects (holderMarks);
                return;
            }
            DisplayCellObjects(holderMarks, marks, InstantiateCellMarkObject);
        }

        void LoadCellSelectionObject ()
        {
            prefabSelection = Resources.Load<GameObject> (cellObjectPrefabName);
            if (prefabSelection == null)
            {
                Debug.LogWarning ("Failed to load resource for cell object: " + cellObjectPrefabName);
            }

            var holder = bb.am.transform;
            var selectionTransform = holder.Find (holderSelectionName);
            if (selectionTransform != null)
            {
                holderSelection = selectionTransform.gameObject;
                return;
            }

            holderSelection = new GameObject (holderSelectionName);
            holderSelection.transform.parent = holder;
            holderSelection.SetActive (false);
        }

        GameObject InstantiateSelectionObject ()
        {
            var cellSelectionObject = Object.Instantiate (prefabSelection, holderSelection.transform);
            cellSelectionObject.name = cellSelectionObjectName;
            cellSelectionObject.SetActive (false);

            var mr = cellSelectionObject.GetComponentInChildren<MeshRenderer> ();
            if (mr == null)
            {
                return cellSelectionObject;
            }

            cellSelectionMaterial = new Material (mr.sharedMaterial);
            cellSelectionMaterial.SetColor (tintColorShaderID, Color.magenta);
            mr.sharedMaterial = cellSelectionMaterial;
            return cellSelectionObject;
        }

        void LoadCellMarkObjects ()
        {
            prefabCellMark = Resources.Load<GameObject> (cellObjectPrefabName);
            if (prefabCellMark == null)
            {
                Debug.LogWarning ("Failed to load resource for cell object to be used for cell marks: " + cellObjectPrefabName);
            }

            var holder = bb.am.transform;
            var marksTransform = holder.Find (holderMarksName);
            if (marksTransform != null)
            {
                holderMarks = marksTransform.gameObject;
                return;
            }

            holderMarks = new GameObject (holderMarksName);
            holderMarks.transform.parent = holder;
            holderMarks.SetActive (false);
        }

        GameObject InstantiateCellMarkObject ()
        {
            var t = holderMarks.transform;
            var cellMarkObject = Object.Instantiate (prefabCellMark, t);
            cellMarkObject.name = cellMarkObjectNamePrefix + t.childCount.ToString("00");
            cellMarkObject.SetActive (false);

            var mr = cellMarkObject.GetComponentInChildren<MeshRenderer> ();
            if (mr == null)
            {
                return cellMarkObject;
            }

            cellMarkMaterial = new Material (mr.sharedMaterial);
            cellMarkMaterial.SetColor (tintColorShaderID, Color.red);
            mr.sharedMaterial = cellMarkMaterial;

            return cellMarkObject;
        }

        public CellObjectOperation selecting;
        public CellObjectOperation marking;

        public AreaSceneLayerModeCellVisuals (AreaSceneBlackboard bb)
        {
            this.bb = bb;
        }

        readonly AreaSceneBlackboard bb;

        GameObject holderSelection;
        GameObject prefabSelection;
        Material cellSelectionMaterial;

        GameObject holderMarks;
        GameObject prefabCellMark;
        Material cellMarkMaterial;

        const string holderSelectionName = "holder_cell_selection";
        const string cellSelectionObjectName = "cell_selection";
        const string cellObjectPrefabName = "Editor/AreaVisuals/block_cell";
        const string holderMarksName = "holder_cell_marks";
        const string cellMarkObjectNamePrefix = "cell_mark_";

        static readonly int tintColorShaderID = Shader.PropertyToID ("_TintColor");
    }

    sealed class AreaSceneLayerModeFloor
    {
        public void OnDestroy ()
        {
            if (collider != null)
            {
                Object.DestroyImmediate (collider);
            }
            if (layerFloor != null)
            {
                Object.DestroyImmediate (layerFloor);
            }
            if (layerPlaneMaterial != null)
            {
                Object.DestroyImmediate (layerPlaneMaterial);
            }
        }

        public bool Load (int layer)
        {
            currentLayer = layer;
            (sizeX, sizeZ) = InstantiateLayerFloor ();
            if (sizeX == 0f || sizeZ == 0f)
            {
                return false;
            }
            InstantiateCollider (sizeX, sizeZ);
            return true;
        }

        public void Hide ()
        {
            if (layerFloor != null && layerFloor.activeSelf)
            {
                layerFloor.SetActive (false);
            }
            if (collider != null && collider.activeSelf)
            {
                collider.SetActive (false);
            }
        }

        public bool HitTest (RaycastHit hitInfo) => hitInfo.collider.gameObject == collider;

        public void Draw ()
        {
            var hc = Handles.color;
            Handles.color = gridLineColor;
            Handles.DrawLines (gridlines);
            Handles.color = hc;
        }

        public void Move (int layer)
        {
            currentLayer = layer;
            var position = new Vector3(sizeX / 2f, FloorVertical (), sizeZ / 2f);
            layerFloor.transform.position = position;
            MoveCollider ();
            MoveGridLines ();
        }

        float FloorVertical () => (currentLayer + 1) * -WorldSpace.BlockSize - floorOffset;

        (float, float) InstantiateLayerFloor ()
        {
            if (layerFloor == null)
            {
                var floorTransform = bb.am.transform.Find (layerFloorObjectName);
                if (floorTransform == null)
                {
                    layerFloor = PrimitiveHelper.CreatePrimitive (PrimitiveType.Plane, false);
                    layerFloor.name = layerFloorObjectName;
                    layerFloor.transform.parent = bb.am.transform;
                    var mr = layerFloor.GetComponentInChildren<MeshRenderer> ();
                    if (mr != null)
                    {
                        layerFloorInitialSize = mr.bounds.size;
                        if (layerPlaneMaterial == null)
                        {
                            var shader = Shader.Find ("Standard");
                            var mat = new Material (shader);
                            EnableTransparentMode (mat);
                            // Reduce glare/reflections from lighting.
                            mat.SetFloat (metallicShaderID, 0.1f);
                            mat.SetFloat (smoothnessShaderID, 0.1f);
                            mat.color = new HSBColor (0f, 0f, 0.8f, 0.2f).ToColor ();
                            layerPlaneMaterial = mat;
                        }
                        mr.sharedMaterial = layerPlaneMaterial;
                    }
                }
                else
                {
                    layerFloor = floorTransform.gameObject;
                }
            }

            if (!layerFloor.activeSelf)
            {
                layerFloor.SetActive (true);
            }

            var bounds = bb.am.boundsFull;
            var sizeX = (bounds.x - 1) * WorldSpace.BlockSize;
            var sizeZ = (bounds.z - 1) * WorldSpace.BlockSize;
            if (bounds == areaBounds)
            {
                return (sizeX, sizeZ);
            }
            areaBounds = bounds;

            if (layerFloorInitialSize == Vector3.zero)
            {
                var mr = layerFloor.GetComponentInChildren<MeshRenderer> ();
                if (mr == null)
                {
                    Debug.LogWarning ("Unable to properly size layer floor\nGame object should have a MeshRenderer component");
                    return (0f, 0f);
                }

                layerFloor.transform.localScale = Vector3.one;
                layerFloorInitialSize = mr.bounds.size;
            }

            layerFloor.transform.localScale = new Vector3 (sizeX / layerFloorInitialSize.x, 1f, sizeZ / layerFloorInitialSize.z);
            BuildGridLines (sizeX, sizeZ);
            return (sizeX, sizeZ);
        }

        void EnableTransparentMode (Material mat)
        {
            // Cribbed from the Unity shader code download.
            // See https://docs.unity3d.com/2020.3/Documentation/Manual/StandardShaderMakeYourOwn.html
            mat.SetOverrideTag("RenderType", "Transparent");
            mat.SetInt("_SrcBlend", (int)BlendMode.One);
            mat.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 0);
            mat.DisableKeyword("_ALPHATEST_ON");
            mat.DisableKeyword("_ALPHABLEND_ON");
            mat.EnableKeyword("_ALPHAPREMULTIPLY_ON");
            if (mat.renderQueue < (int)RenderQueue.GeometryLast + 1 || mat.renderQueue > (int)RenderQueue.Overlay - 1)
            {
                mat.renderQueue = (int)RenderQueue.Transparent;
            }
        }

        void BuildGridLines (float sizeX, float sizeZ)
        {
            var boundsX = areaBounds.x - 1;
            var boundsZ = areaBounds.z - 1;
            gridlines = new Vector3[2 * (boundsX + boundsZ) - 4];

            var lineIndex = 0;
            var y = FloorVertical ();
            var x = WorldSpace.BlockSize;
            for (var i = 1; i < boundsX; i += 1, x += WorldSpace.BlockSize)
            {
                gridlines[lineIndex++] = new Vector3 (x, y, 0f);
                gridlines[lineIndex++] = new Vector3 (x, y, sizeZ);
            }

            var z = WorldSpace.BlockSize;
            for (var i = 1; i < boundsZ; i += 1, z += WorldSpace.BlockSize)
            {
                gridlines[lineIndex++] = new Vector3 (0f, y, z);
                gridlines[lineIndex++] = new Vector3 (sizeX, y, z);
            }
        }

        void InstantiateCollider (float sizeX, float sizeZ)
        {
            if (collider == null)
            {
                var holder = bb.am.GetHolderColliders ();
                if (holder == null)
                {
                    return;
                }

                var colliderTransform = holder.Find (colliderName);
                if (colliderTransform == null)
                {
                    collider = new GameObject (colliderName)
                    {
                        hideFlags = HideFlags.DontSave,
                    };
                    collider.transform.parent = holder;
                    collider.layer = Constants.volumeCollidersLayer;
                    collider.AddComponent<BoxCollider> ();
                }
                else
                {
                    collider = colliderTransform.gameObject;
                }
            }

            if (!collider.activeSelf)
            {
                collider.SetActive (true);
            }

            var bc = collider.GetComponent<BoxCollider> ();
            var colliderSize = bc.size;
            var visualTransform = InstantiateColliderVisual (colliderSize);
            if (colliderSize.x.RoughlyEqual (sizeX) && colliderSize.z.RoughlyEqual (sizeZ))
            {
                return;
            }

            bc.size = new Vector3 (sizeX, WorldSpace.BlockSize, sizeZ);
            collider.transform.localPosition = new Vector3 (sizeX / 2f, ColliderVertical (), sizeZ / 2f);

            if (visualTransform != null)
            {
                var scale = bc.size;
                scale.y = WorldSpace.BlockSize;
                visualTransform.localScale = scale;
            }
        }

        Transform InstantiateColliderVisual (Vector3 size)
        {
            var visualize = bb.am.visualizeCollisions;
            var visualTransform = collider.transform.Find (colliderVisualName);
            if (visualTransform == null && visualize)
            {
                var vis = PrimitiveHelper.CreatePrimitive (PrimitiveType.Cube, false);
                vis.name = colliderVisualName;
                vis.hideFlags = HideFlags.DontSave;
                visualTransform = vis.transform;
                visualTransform.parent = collider.transform;
                visualTransform.localPosition = Vector3.zero;
                var scale = size;
                scale.y = WorldSpace.BlockSize;
                visualTransform.localScale = scale;
                var mr = vis.GetComponent<MeshRenderer> ();
                if (mr != null)
                {
                    mr.shadowCastingMode = ShadowCastingMode.Off;
                    mr.sharedMaterial = Resources.Load<Material> (colliderMaterialName);
                }
            }
            else if (visualize && !visualTransform.gameObject.activeSelf)
            {
                visualTransform.gameObject.SetActive (true);
            }
            else if (!bb.am.visualizeCollisions && visualTransform != null)
            {
                visualTransform.gameObject.SetActive (false);
            }
            return visualTransform;
        }

        float ColliderVertical () => currentLayer * -WorldSpace.BlockSize - WorldSpace.HalfBlockSize;

        void MoveCollider ()
        {
            if (collider == null)
            {
                return;

            }

            var t = collider.transform;
            var position = t.localPosition;
            position.y = ColliderVertical ();
            t.localPosition = position;
        }

        void MoveGridLines ()
        {
            var y = FloorVertical ();
            for (var i = 0; i < gridlines.Length; i += 1)
            {
                gridlines[i].y = y;
            }
        }

        public AreaSceneLayerModeFloor (AreaSceneBlackboard bb)
        {
            this.bb = bb;
        }

        readonly AreaSceneBlackboard bb;

        int currentLayer = -1;
        float sizeX;
        float sizeZ;

        Vector3Int areaBounds;
        Vector3 layerFloorInitialSize;

        GameObject layerFloor;
        Material layerPlaneMaterial;

        GameObject collider;

        Vector3[] gridlines;

        const float floorOffset = -0.1f;

        const string layerFloorObjectName = "layer_floor";
        const string colliderName = "ColliderForLayer";
        const string colliderVisualName = "ColliderVisual";
        const string colliderMaterialName = "Content/Debug/AreaCollision";

        static readonly Color gridLineColor = new HSBColor (0f, 0f, 0.25f, 0.4f).ToColor ();
        static readonly int smoothnessShaderID = Shader.PropertyToID ("_Glossiness");
        static readonly int metallicShaderID = Shader.PropertyToID ("_Metallic");
    }

    sealed class AreaSceneLayerModeOperations
    {
        public void TryRebuild (ref int currentLayer)
        {
            TryChangeMarked (currentLayer);
            TryChangeSelected ();
            if (bb.rebuildTerrain)
            {
                bb.am.UpdateTerrain (true, true);
                bb.rebuildTerrain = false;
                currentLayer = -1;
            }
        }

        void TryChangeMarked (int currentLayer)
        {
            var changeType = bb.spotChange;
            System.Action<AreaVolumePoint> spotOp;
            switch (changeType)
            {
                case SpotChange.InteriorMarkedAll:
                case SpotChange.InteriorMarkedLayer:
                    spotOp = SetInterior;
                    break;
                case SpotChange.FullMarkedAll:
                case SpotChange.FullMarkedLayer:
                    spotOp = SetPointFull;
                    break;
                default:
                    return;
            }

            var am = bb.am;
            var points = am.points;
            var spotIndexes = changeType == SpotChange.InteriorMarkedLayer || changeType == SpotChange.FullMarkedLayer
                ? bb.markedSpotsByLayer.Where (kvp => kvp.Key == currentLayer).SelectMany (kvp => kvp.Value)
                : bb.markedSpotsByLayer.SelectMany (kvp => kvp.Value);
            foreach (var spotIndex in spotIndexes.ToList ())
            {
                var spot = points[spotIndex];
                spotOp (spot);
                AreaSceneModeHelper.UpdateNeighborState(am, spotIndex);
                am.RebuildBlocksAroundIndex (spot.spotIndex);
            }
            bb.spotChange = SpotChange.None;
        }

        void TryChangeSelected ()
        {
            var am = bb.am;
            var points = bb.am.points;
            var changeType = bb.spotChange;
            var terrainModified = false;
            foreach (var spotIndex in bb.selectedSpots)
            {
                var spot = points[spotIndex];
                var changed = false;
                switch (changeType)
                {
                    case SpotChange.Interior:
                        SetInterior (spot);
                        changed = true;
                        break;
                    case SpotChange.Empty:
                        SetEmpty (spot);
                        changed = true;
                        break;
                    case SpotChange.State:
                        spot.pointState = bb.spotChangeState;
                        changed = true;
                        break;
                    case SpotChange.Tileset:
                        terrainModified |= SetTileset (spot);
                        changed = true;
                        break;
                    case SpotChange.StateAndTileset:
                        spot.pointState = bb.spotChangeState;
                        terrainModified |= SetTileset (spot);
                        changed = true;
                        break;
                }
                if (!changed)
                {
                    continue;
                }
                AreaSceneModeHelper.UpdateNeighborState (am, spotIndex);
                am.RebuildBlocksAroundIndex (spot.spotIndex);
            }
            if (terrainModified)
            {
                bb.rebuildTerrain = true;
            }
            bb.spotChange = SpotChange.None;
        }

        void SetInterior (AreaVolumePoint cell)
        {
            cell.pointState = AreaVolumePointState.Full;
            cell.spotConfiguration = cell.spotPresent ? TilesetUtility.configurationFull : TilesetUtility.configurationEmpty;
            cell.spotConfigurationWithDamage = cell.spotConfiguration;
            ClearTileset (cell);
            if (bb.markedSpotsByLayer.TryGetValue (cell.pointPositionIndex.y, out var marks))
            {
                marks.Remove (cell.spotIndex);
            }
        }

        void SetEmpty (AreaVolumePoint cell)
        {
            cell.pointState = AreaVolumePointState.Empty;
            cell.spotConfiguration = TilesetUtility.configurationEmpty;
            cell.spotConfigurationWithDamage = TilesetUtility.configurationEmpty;
            ClearTileset (cell);
        }

        void SetPointFull (AreaVolumePoint cell)
        {
            cell.pointState = AreaVolumePointState.Full;
            if (bb.markedSpotsByLayer.TryGetValue (cell.pointPositionIndex.y, out var marks))
            {
                marks.Remove (cell.spotIndex);
            }
        }

        bool SetTileset (AreaVolumePoint cell)
        {
            var tileset = bb.spotChangeTileset;
            if (tileset == cell.blockTileset)
            {
                return false;
            }
            var terrainModified = AreaManager.IsPointTerrain (cell) || tileset == AreaTilesetHelper.idOfTerrain;
            cell.blockTileset = tileset;
            if (tileset == 0)
            {
                AreaManager.ClearTileData (cell);
            }
            return terrainModified;
        }

        void ClearTileset (AreaVolumePoint cell)
        {
            cell.blockTileset = 0;
            cell.blockGroup = 0;
            cell.blockSubtype = 0;
            cell.blockRotation = 0;
            cell.blockFlippedHorizontally = false;
            cell.customization = TilesetVertexProperties.defaults;
        }

        public AreaSceneLayerModeOperations (AreaSceneBlackboard bb)
        {
            this.bb = bb;
        }

        readonly AreaSceneBlackboard bb;
    }
}
