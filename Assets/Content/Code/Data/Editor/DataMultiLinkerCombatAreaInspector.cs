using System.Collections.Generic;

using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;

using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;

using Area;
using PhantomBrigade;
using PhantomBrigade.Data;
#if PB_MODSDK
using PhantomBrigade.SDK.ModTools;
#endif

[CustomEditor (typeof(DataMultiLinkerCombatArea))]
public class DataMultiLinkerCombatAreaInspector : OdinEditor
{
    public void OnSceneGUI ()
    {
        if (DataMultiLinkerCombatArea.selectedArea == null)
        {
            InstructionPanelDrawer.Draw ();
            return;
        }
        if (CombatSceneHelper.ins == null)
        {
            return;
        }

        var am = CombatSceneHelper.ins.areaManager;
        var boundsFull = am.boundsFull;
        if (am == null || boundsFull == Vector3Int.size0x0x0)
        {
            return;
        }

        #if PB_MODSDK
        EnsureSegmentAndFieldVisibility ();
        #endif

        // Disable clicking on scene objects
        HandleUtility.AddDefaultControl (GUIUtility.GetControlID (FocusType.Passive));

        var area = DataMultiLinkerCombatArea.selectedArea;
        var circleRotation = Quaternion.Euler (90f, 0f, 0f);

        var e = Event.current;
        SuppressExecuteCommandEvents (e);

        var eventPresent = e.type == EventType.MouseDown;
        var buttonPressed = eventPresent && e.button == 0 ? KeyCode.Mouse0 : KeyCode.None;

        #if PB_MODSDK
        birdsEye.HandleInput (area, e);
        titlePanel.MeasureTitlePanel (area);
        spawnPointsDrawer.overlapsPanel = titlePanel.OverlapsUI;
        waypointsDrawer.overlapsPanel = titlePanel.OverlapsUI;
        #endif

        var worldRay = HandleUtility.GUIPointToWorldRay (e.mousePosition);
        var inputCheckSuccessful = buttonPressed == KeyCode.Mouse0 && e.type != EventType.Used;

        DrawAreaBorder (boundsFull);
        spawnPointsDrawer.Draw (area, worldRay, inputCheckSuccessful, circleRotation, e);
        waypointsDrawer.Draw (area, worldRay, inputCheckSuccessful, circleRotation, e);
        DrawLocations (area, worldRay, circleRotation, inputCheckSuccessful, e);
        DrawFields (area, worldRay, circleRotation, inputCheckSuccessful, e);
        DrawVolumes (area, am, boundsFull, worldRay, e, eventPresent, buttonPressed);

        if (DataMultiLinkerCombatArea.presentation.showSpawnGeneration) { }

        InstructionPanelDrawer.DrawFull (ref volumeEditingMode);

        #if PB_MODSDK
        titlePanel.DrawTitlePanel (area);
        #endif

        this.RepaintIfRequested ();
        if (GUI.changed)
        {
            EditorWindow view = EditorWindow.GetWindow<SceneView> ();
            view.Repaint ();
        }
    }

    protected override void OnEnable ()
    {
        Tools.hidden = true;
    }

    protected override void OnDisable ()
    {
        Tools.hidden = false;
    }

    void SuppressExecuteCommandEvents (Event e)
    {
        if (e.type != EventType.ExecuteCommand)
        {
            return;
        }

        // Killing some bad editor hotkeys
        switch (Event.current.commandName)
        {
            case "Copy":
                e.Use ();
                break;
            case "Cut":
                e.Use ();
                break;
            case "Paste":
                e.Use ();
                break;
            case "Delete":
                e.Use ();
                break;
            case "FrameSelected":
                e.Use ();
                break;
            case "Duplicate":
                e.Use ();
                break;
            case "SelectAll":
                e.Use ();
                break;
            // default:
            // Lets show any other commands that may come through
            // Debug.Log (Event.current.commandName);
            // break;
        }
    }

    void DrawAreaBorder (Vector3Int boundsFull)
    {

        var boundsScaled = boundsFull * TilesetUtility.blockAssetSize;
        var boundsMidY = -boundsScaled.y * 0.5f;
        var bc1 = new Vector3 (0f, boundsMidY, 0f);
        var bc2 = new Vector3 (boundsScaled.x, boundsMidY, 0f);
        var bc3 = new Vector3 (boundsScaled.x, boundsMidY, boundsScaled.z);
        var bc4 = new Vector3 (0f, boundsMidY, boundsScaled.z);

        Handles.color = colorUnused;
        Handles.DrawLine (bc1, bc2);
        Handles.DrawLine (bc2, bc3);
        Handles.DrawLine (bc3, bc4);
        Handles.DrawLine (bc4, bc1);
    }

    void DrawLocations (DataContainerCombatArea area, Ray worldRay, Quaternion circleRotation, bool inputCheckSuccessful, Event e)
    {
        if (area.locations == null || !DataMultiLinkerCombatArea.presentation.showLocations)
        {
            return;
        }

        foreach (var kvp in area.locations)
        {
            var locationKey = kvp.Key;
            var location = kvp.Value;

            if (location == null)
            {
                continue;
            }
            if (location.data == null)
            {
                location.data = new DataBlockAreaLocation ();
            }
            if (string.IsNullOrEmpty (location.key))
            {
                location.key = kvp.Key;
            }

            var ld = location.data;
            var selected = location == DataMultiLinkerCombatArea.selectedLocation;
            var colorFull = selected ? colorMainLocation : colorUnusedLocation;
            var colorSemi = colorFull.WithAlpha (0.25f);
            var position = ld.point;
            var rotationQ = Quaternion.Euler (0f, ld.rotation, 0f);

            Handles.color = colorFull;
            Handles.DrawLine (position, position + Vector3.up * 5f);

            Handles.Label (position + Vector3.up * 7f, $"{locationKey} (location)\n");

            if (ld.rect)
            {
                var sizeXHalf = ld.sizeX * 0.5f;
                var sizeYHalf = ld.sizeY * 0.5f;
                var pos1 = position + new Vector3 (-sizeXHalf, 0f, -sizeYHalf);
                var pos2 = position + new Vector3 (sizeXHalf, 0f, -sizeYHalf);
                var pos3 = position + new Vector3 (sizeXHalf, 0f, sizeYHalf);
                var pos4 = position + new Vector3 (-sizeXHalf, 0f, sizeYHalf);

                Handles.DrawLine (pos1, pos2);
                Handles.DrawLine (pos2, pos3);
                Handles.DrawLine (pos3, pos4);
                Handles.DrawLine (pos4, pos1);
            }
            else
            {
                var dir = rotationQ * Vector3.forward;
                var radius = location.data.sizeX;
                #if PB_MODSDK
                CircleHandleCap (position, circleRotation, radius);
                #else
                Handles.CircleHandleCap (0, position, circleRotation, radius, EventType.Repaint);
                #endif
                Handles.DrawLine (position + dir * radius, position + dir * (radius + 3));
            }

            Handles.color = colorSemi;
            var size = HandleUtility.GetHandleSize (position);

            #if PB_MODSDK
            var overlaps = titlePanel.OverlapsUI (position);
            var clicked = !overlaps && Handles.Button (position, Quaternion.identity, size * 0.05f, size * 0.2f, Handles.DotHandleCap);
            selected &= !overlaps;
            #else
            var clicked = Handles.Button (position, Quaternion.identity, size * 0.05f, size * 0.2f, Handles.DotHandleCap);
            #endif
            if (clicked)
            {
                DataMultiLinkerCombatArea.DeselectSelections();
                DataMultiLinkerCombatArea.selectedLocation = location;
                GUIHelper.RequestRepaint ();
            }

            #if PB_MODSDK
            if (!DataContainerModData.hasSelectedConfigs)
            {
                continue;
            }
            #endif

            if (selected)
            {
                EditorGUI.BeginChangeCheck ();
                position = Handles.DoPositionHandle (position, rotationQ);
                if (EditorGUI.EndChangeCheck ())
                {
                    location.data.point = position;

                    if (e.alt)
                    {
                        var groundingRayOrigin = position + Vector3.up * 100f;
                        var groundingRay = new Ray (groundingRayOrigin, Vector3.down);
                        if (Physics.Raycast (groundingRay, out var groundingHit, 200f, LayerMasks.environmentMask))
                        {
                            location.data.point = groundingHit.point;
                        }
                    }
                    if (e.shift)
                    {
                        location.SnapToGrid ();
                    }
                }

                EditorGUI.BeginChangeCheck ();
                rotationQ = Handles.DoRotationHandle (rotationQ, position);
                if (EditorGUI.EndChangeCheck ())
                {
                    var rotationNew = rotationQ.eulerAngles.y;
                    if (rotationNew < -180f)
                        rotationNew += 360f;
                    else if (rotationNew > 180f)
                        rotationNew -= 360f;
                    location.data.rotation = rotationNew;
                }
            }
        }

        #if PB_MODSDK
        if (!DataContainerModData.hasSelectedConfigs)
        {
            return;
        }
        #endif
        if (DataMultiLinkerCombatArea.selectedLocation == null)
        {
            return;
        }
        if (DataMultiLinkerCombatArea.selectedSpawnPoint != null)
        {
            return;
        }
        if (!DataMultiLinkerCombatArea.presentation.showLocations)
        {
            return;
        }

        if (!e.alt || !Physics.Raycast (worldRay, out var hitInfoLocation, 1000, LayerMasks.environmentMask))
        {
            return;
        }

        Handles.color = colorMainLocation;
        Handles.DrawLine (hitInfoLocation.point, hitInfoLocation.point + Vector3.up * 5f);

        const float targetCircleSize = 3f;

        var pos = hitInfoLocation.point;
        var posValid = true;
        #if PB_MODSDK
        var keyHit = "";
        #endif
        foreach (var kvp in area.locations)
        {
            #if PB_MODSDK
            var locationOther = kvp.Value;
            var flatPos = pos.Flatten2D ();
            var flatOrigin = locationOther.data.point.Flatten2D ();
            if (locationOther.data.rect)
            {
                var sizeX = locationOther.data.sizeX + targetCircleSize * 2f;
                var sizeY = locationOther.data.sizeY + targetCircleSize * 2f;
                var rect = new Rect (new Vector2 (flatOrigin.x - sizeX / 2f, flatOrigin.y - sizeY / 2f), new Vector2 (sizeX, sizeY));
                if (rect.Contains (flatPos))
                {
                    sizeX = locationOther.data.sizeX - 0.05f;
                    sizeY = locationOther.data.sizeY - 0.05f;
                    rect = new Rect (new Vector2 (flatOrigin.x - sizeX / 2f, flatOrigin.y - sizeY / 2f), new Vector2 (sizeX, sizeY));
                    if (rect.Contains (flatPos))
                    {
                        var y = locationOther.data.point.y;
                        var verts = new[]
                        {
                            new Vector3 (rect.xMin, y, rect.yMin),
                            new Vector3 (rect.xMin, y, rect.yMax),
                            new Vector3 (rect.xMax, y, rect.yMax),
                            new Vector3 (rect.xMax, y, rect.yMin),
                        };
                        var c = new Color (1f, 0f, 0.5f, 0.33f);
                        Handles.DrawSolidRectangleWithOutline(verts, c, c);
                        keyHit = kvp.Key;
                    }
                    posValid = false;
                    break;
                }
                continue;
            }

            var radius = locationOther.data.GetRadius () + targetCircleSize;
            #else
            var radius = 6f;
            #endif
            var sqrMagnitude = (pos - locationOther.data.point).sqrMagnitude;
            if (sqrMagnitude < radius * radius)
            {
                #if PB_MODSDK
                radius = locationOther.data.GetRadius ();
                sqrMagnitude = (flatPos - flatOrigin).sqrMagnitude;
                if (sqrMagnitude < radius * radius)
                {
                    var hc = Handles.color;
                    Handles.color = Color.magenta.WithAlpha (0.33f);
                    Handles.DrawSolidDisc (locationOther.data.point, Vector3.up, radius);
                    Handles.color = hc;
                    keyHit = kvp.Key;
                }
                #endif
                posValid = false;
                break;
            }
        }

        Handles.color = posValid ? colorMain : colorEnemy;
        #if PB_MODSDK
        CircleHandleCap (hitInfoLocation.point, circleRotation, targetCircleSize);
        #else
        Handles.CircleHandleCap (0, hitInfoLocation.point, circleRotation, targetCircleSize, EventType.Repaint);
        #endif

        #if PB_MODSDK
        if (!string.IsNullOrEmpty (keyHit) && e.OnEventType (EventType.MouseDown))
        {
            var loc = area.locations[keyHit];
            switch (e.button)
            {
                case 0:
                    if (loc == DataMultiLinkerCombatArea.selectedLocation)
                    {
                        DataMultiLinkerCombatArea.selectedLocation = null;
                    }
                    else
                    {
                        DataMultiLinkerCombatArea.selectedLocation = loc;
                    }
                    break;
                case 1:
                    area.locations.Remove (keyHit);
                    DataMultiLinkerCombatArea.unsavedChangesPossible = true;
                    if (loc == DataMultiLinkerCombatArea.selectedLocation)
                    {
                        DataMultiLinkerCombatArea.selectedLocation = null;
                    }
                    break;
                case 2:
                    if (e.shift)
                    {
                        loc.SnapToGrid ();
                    }
                    else
                    {
                        loc.Ground ();
                    }
                    DataMultiLinkerCombatArea.unsavedChangesPossible = true;
                    break;
            }
            GUIHelper.RequestRepaint ();
            e.Use ();
            return;
        }
        #endif

        if (!inputCheckSuccessful || !posValid)
        {
            return;
        }

        var (ok, key) = GetUniqueKey (area, "location_");
        if (!ok)
        {
            return;
        }

        var locationTagged = new DataBlockAreaLocationTagged ()
        {
            data = new DataBlockAreaLocation
            {
                point = pos
            }
        };
        locationTagged.SnapToGrid ();
        area.locations.Add (key, locationTagged);
        #if PB_MODSDK
        DataMultiLinkerCombatArea.selectedLocation = locationTagged;
        DataMultiLinkerCombatArea.unsavedChangesPossible = true;
        #endif
        GUIHelper.RequestRepaint ();
        e.Use ();
    }

    static (bool, string) GetUniqueKey (DataContainerCombatArea area, string prefix)
    {
        var loops = 0;
        var locationKeyIndex = 0;
        var key = prefix + locationKeyIndex;
        while (true)
        {
            if (!area.locations.ContainsKey (key))
            {
                break;
            }

            locationKeyIndex += 1;
            loops += 1;

            if (loops > 100)
            {
                Debug.LogWarning ("Failed to find any key for " + prefix);
                return (false, "");
            }

            key = prefix + locationKeyIndex;
        }
        return area.locations.ContainsKey (key) ? (false, "") : (true, key);
    }

    void DrawFields (DataContainerCombatArea area, Ray worldRay, Quaternion circleRotation, bool inputCheckSuccessful, Event e)
    {
        if (area.fields == null || !DataMultiLinkerCombatArea.presentation.showFields)
        {
            return;
        }

        var sourceColorMain = Color.HSVToRGB (0.58f, 0.5f, 0.8f).WithAlpha (0.25f);
        var sourceColorCulled = Color.HSVToRGB (0.65f, 0.4f, 0.6f).WithAlpha (0.025f);

        for (var i = 0; i < area.fields.Count; i += 1)
        {
            // var fieldKey = kvp.Key;
            var field = area.fields[i];
            if (field == null)
            {
                continue;
            }

            var type = !string.IsNullOrEmpty (field.type) ? field.type : "?";
            var selected = field == DataMultiLinkerCombatArea.selectedField;
            var colorFull = selected ? colorMainLocation : colorUnusedLocation;
            var colorSemi = colorFull.WithAlpha (0.25f);

            var position = field.origin;
            var rotationQ = Quaternion.Euler (0f, field.rotation, 0f);

            Handles.color = colorFull;
            Handles.DrawLine (position, position + Vector3.up * 5f);

            Handles.Label (position + Vector3.up * 7f, $"F{i} ({type})\n");
            DrawField (position, field.size, field.rotation, sourceColorMain, sourceColorCulled);

            Handles.color = colorSemi;
            var size = HandleUtility.GetHandleSize (position);

            #if PB_MODSDK
            var overlaps = titlePanel.OverlapsUI (position);
            var clicked = !overlaps && Handles.Button (position, Quaternion.identity, size * 0.05f, size * 0.2f, Handles.DotHandleCap);
            selected &= !overlaps;
            #else
            var clicked = Handles.Button (position, Quaternion.identity, size * 0.05f, size * 0.2f, Handles.DotHandleCap);
            #endif
            if (clicked)
            {
                DataMultiLinkerCombatArea.DeselectSelections ();
                DataMultiLinkerCombatArea.selectedField = field;
                GUIHelper.RequestRepaint ();
            }

            #if PB_MODSDK
            if (!DataContainerModData.hasSelectedConfigs)
            {
                continue;
            }
            #endif

            if (selected)
            {
                EditorGUI.BeginChangeCheck ();
                position = Handles.DoPositionHandle (position, rotationQ);
                if (EditorGUI.EndChangeCheck ())
                {
                    field.origin = position;
                    #if PB_MODSDK
                    DataMultiLinkerCombatArea.unsavedChangesPossible = true;
                    #endif
                }

                EditorGUI.BeginChangeCheck ();
                rotationQ = Handles.DoRotationHandle (rotationQ, position);
                if (EditorGUI.EndChangeCheck ())
                {
                    var rotationNew = rotationQ.eulerAngles.y;
                    if (rotationNew < -180f)
                    {
                        rotationNew += 360f;
                    }
                    else if (rotationNew > 180f)
                    {
                        rotationNew -= 360f;
                    }
                    field.rotation = rotationNew;
                    #if PB_MODSDK
                    DataMultiLinkerCombatArea.unsavedChangesPossible = true;
                    #endif
                }
            }
        }

        #if PB_MODSDK
        if (!DataContainerModData.hasSelectedConfigs)
        {
            return;
        }
        #endif
        if (DataMultiLinkerCombatArea.selectedField == null)
        {
            return;
        }
        if (DataMultiLinkerCombatArea.selectedSpawnPoint != null)
        {
            return;
        }
        if (!DataMultiLinkerCombatArea.presentation.showLocations)
        {
            return;
        }
        if (!e.alt || !Physics.Raycast (worldRay, out var hitInfoLocation, 1000, LayerMasks.environmentMask))
        {
            return;
        }

        Handles.color = colorMainLocation;
        Handles.DrawLine (hitInfoLocation.point, hitInfoLocation.point + Vector3.up * 5f);

        var pos = hitInfoLocation.point;
        var posValid = true;
        foreach (var fieldOther in area.fields)
        {
            var fieldBounds = new Bounds (fieldOther.origin, fieldOther.size);
            fieldBounds.Expand(12f);
            if (fieldBounds.Contains(pos))
            {
                posValid = false;
                break;
            }
        }

        Handles.color = posValid ? colorMain : colorEnemy;
        #if PB_MODSDK
        CircleHandleCap (hitInfoLocation.point, circleRotation, 3f);
        #else
        Handles.CircleHandleCap (0, hitInfoLocation.point, circleRotation, 3f, EventType.Repaint);
        #endif

        if (!inputCheckSuccessful || !posValid)
        {
            return;
        }

        var (ok, key) = GetUniqueKey (area, "field_");
        if (!ok)
        {
            return;
        }

        var f = new DataBlockAreaField ()
        {
            key = key,
            origin = pos,
            size = Vector3.one,
        };
        area.fields.Add (f);
        #if PB_MODSDK
        DataMultiLinkerCombatArea.selectedField = f;
        DataMultiLinkerCombatArea.unsavedChangesPossible = true;
        #endif
        GUIHelper.RequestRepaint ();
        e.Use ();
    }

    void DrawVolumes (DataContainerCombatArea area, AreaManager am, Vector3Int boundsFull, Ray worldRay, Event e, bool eventPresent, KeyCode buttonPressed)
    {
        if (area.volumes == null || !DataMultiLinkerCombatArea.presentation.showVolumes)
        {
            return;
        }

        var points = am.points;
        var pointsTotal = am.points.Count;

        cubeDescs.Clear ();

        foreach (var kvp in area.volumes)
        {
            var volumeKey = kvp.Key;
            var volume = kvp.Value;
            if (volume == null)
            {
                continue;
            }
            if (volume.data == null)
            {
                volume.data = new DataBlockAreaVolume ();
            }

            var vd = volume.data;
            var internalPositionA = vd.origin;
            var internalPositionB = vd.origin + vd.size - Vector3Int.size1x1x1;

            // Get Position
            var sourceCornerAIndex = AreaUtility.GetIndexFromVolumePosition (internalPositionA, boundsFull);
            var sourceCornerBIndex = AreaUtility.GetIndexFromVolumePosition (internalPositionB, boundsFull);
            var sourcePosA = new Vector3 (internalPositionA.x, -internalPositionA.y, internalPositionA.z) * TilesetUtility.blockAssetSize;
            var sourcePosB = new Vector3 (internalPositionB.x, -internalPositionB.y, internalPositionB.z) * TilesetUtility.blockAssetSize;

            sourcePosA += new Vector3 (-1f, 1f, -1f) * (TilesetUtility.blockAssetSize * 0.51f);
            sourcePosB += new Vector3 (1f, -1f, 1f) * (TilesetUtility.blockAssetSize * 0.51f);

            var sourceColorMain = new HSBColor (0.0f, 0.5f, 0.8f, 0.15f).ToColor ();
            var sourceColorCulled = new HSBColor (0.0f, 0.4f, 0.5f, 0.075f).ToColor ();

            if (sourceCornerAIndex != -1 && sourceCornerBIndex != -1)
            {
                sourceColorMain = new HSBColor (0.25f, 0.5f, 0.8f, 0.15f).ToColor ();
                sourceColorCulled = new HSBColor (0.1f, 0.4f, 0.5f, 0.075f).ToColor ();
            }

            DrawVolume (volumeKey, sourcePosA, sourcePosB, sourceColorMain, sourceColorCulled);

            var size = HandleUtility.GetHandleSize (sourcePosA);
            #if PB_MODSDK
            var overlaps = titlePanel.OverlapsUI (sourcePosA);
            var clicked = !overlaps && Handles.Button (sourcePosA, Quaternion.identity, size * 0.05f, size * 0.2f, Handles.DotHandleCap);
            #else
            var clicked = Handles.Button (sourcePosA, Quaternion.identity, size * 0.05f, size * 0.2f, Handles.DotHandleCap);
            #endif
            if (clicked)
            {
                DataMultiLinkerCombatArea.DeselectSelections ();
                DataMultiLinkerCombatArea.selectedVolume = volume;
                GUIHelper.RequestRepaint ();
            }
        }

        if (DataMultiLinkerCombatArea.selectedVolume == null)
        {
            return;
        }

        #if PB_MODSDK
        if (!DataContainerModData.hasSelectedConfigs)
        {
            return;
        }
        #endif

        var selectedVolumeData = DataMultiLinkerCombatArea.selectedVolume.data;
        if (e.alt)
        {
            if (Physics.Raycast (worldRay, out var hitInfo, 1000, LayerMasks.environmentMask))
            {
                Handles.color = Color.white.WithAlpha (1f);
                Handles.DrawLine (hitInfo.point, hitInfo.point + hitInfo.normal * 3f);
                #if PB_MODSDK
                CubeHandleCap (hitInfo.point, 0.5f);
                #else
                Handles.CubeHandleCap (0, hitInfo.point, Quaternion.identity, 0.5f, EventType.Repaint);
                #endif

                var hitPositionShiftedDeeper = hitInfo.point - hitInfo.normal * 0.5f;
                var index = AreaUtility.GetIndexFromWorldPosition (hitPositionShiftedDeeper, am.GetHolderColliders ().position, am.boundsFull);
                if (index != -1 && index.IsValidIndex (am.points))
                {
                    var point = am.points[index];
                    if (point != null)
                    {
                        Handles.color = Color.white.WithAlpha (0.25f);
                        #if PB_MODSDK
                        CubeHandleCap (point.pointPositionLocal, 2.8f);
                        #else
                        Handles.CubeHandleCap (0, point.pointPositionLocal, Quaternion.identity, 2.8f, EventType.Repaint);
                        #endif

                        var originAdjusted = new Vector3Int (point.pointPositionIndex.x, selectedVolumeData.origin.y, point.pointPositionIndex.z);
                        var internalPositionA = originAdjusted;
                        var internalPositionB = originAdjusted + selectedVolumeData.size - Vector3Int.size1x1x1;

                        // Get Position
                        int sourceCornerAIndex = AreaUtility.GetIndexFromVolumePosition (internalPositionA, boundsFull);
                        int sourceCornerBIndex = AreaUtility.GetIndexFromVolumePosition (internalPositionB, boundsFull);

                        var sourcePosA = new Vector3 (internalPositionA.x, -internalPositionA.y, internalPositionA.z) * TilesetUtility.blockAssetSize;
                        var sourcePosB = new Vector3 (internalPositionB.x, -internalPositionB.y, internalPositionB.z) * TilesetUtility.blockAssetSize;

                        sourcePosA += new Vector3 (-1f, 1f, -1f) * (TilesetUtility.blockAssetSize * 0.51f);
                        sourcePosB += new Vector3 (1f, -1f, 1f) * (TilesetUtility.blockAssetSize * 0.51f);

                        var internalPositionA1 = selectedVolumeData.origin;
                        var internalPositionB1 = selectedVolumeData.origin + selectedVolumeData.size - Vector3Int.size1x1x1;

                        var cOrigin = new Vector3 (internalPositionA1.x, -internalPositionA1.y, internalPositionA1.z) * TilesetUtility.blockAssetSize;
                        var cExtent = new Vector3 (internalPositionB1.x, -internalPositionB1.y, internalPositionB1.z) * TilesetUtility.blockAssetSize;

                        cOrigin += new Vector3 (-1f, 1f, -1f) * (TilesetUtility.blockAssetSize * 0.51f);
                        cExtent += new Vector3 (1f, -1f, 1f) * (TilesetUtility.blockAssetSize * 0.51f);

                        var sourceColorMain = new HSBColor (0.0f, 0.5f, 0.8f, 0.15f).ToColor ();
                        var sourceColorCulled = new HSBColor (0.0f, 0.4f, 0.5f, 0.075f).ToColor ();

                        if (sourceCornerAIndex != -1 && sourceCornerBIndex != -1)
                        {
                            sourceColorMain = new HSBColor (0.5f, 0.5f, 0.8f, 0.15f).ToColor ();
                            sourceColorCulled = new HSBColor (0.5f, 0.4f, 0.5f, 0.075f).ToColor ();
                        }

                        DrawVolume ("New origin", sourcePosA, sourcePosB, sourceColorMain, sourceColorCulled);

                        if (eventPresent && buttonPressed == KeyCode.Mouse0)
                        {
                            selectedVolumeData.origin = originAdjusted;
                        }

                        if (volumeEditingMode == VolumeEditingMode.PositionY)
                        {
                            var c1 = cOrigin;
                            var c2 = new Vector3 (cOrigin.x, cOrigin.y, cExtent.z);
                            var c3 = new Vector3 (cExtent.x, cOrigin.y, cOrigin.z);
                            var c4 = new Vector3 (cExtent.x, cOrigin.y, cExtent.z);
                            var up = Vector3.up * 3f;

                            DrawArrow (c1, c1 + up, Color.HSVToRGB (0.3f, 0.5f, 1f));
                            DrawArrow (c2, c2 + up, Color.HSVToRGB (0.3f, 0.5f, 1f));
                            DrawArrow (c3, c3 + up, Color.HSVToRGB (0.3f, 0.5f, 1f));
                            DrawArrow (c4, c4 + up, Color.HSVToRGB (0.3f, 0.5f, 1f));
                        }
                        else if (volumeEditingMode == VolumeEditingMode.SizeXLength)
                        {
                            var c1a = new Vector3 (cOrigin.x, cOrigin.y, cOrigin.z);
                            var c1b = new Vector3 (cExtent.x, cOrigin.y, cOrigin.z);
                            var c2a = new Vector3 (cOrigin.x, cOrigin.y, cExtent.z);
                            var c2b = new Vector3 (cExtent.x, cOrigin.y, cExtent.z);

                            DrawArrowBidirectional (c1a, c1b, Color.HSVToRGB (0f, 0.5f, 1f));
                            DrawArrowBidirectional (c2a, c2b, Color.HSVToRGB (0f, 0.5f, 1f));
                        }
                        else if (volumeEditingMode == VolumeEditingMode.SizeZWidth)
                        {
                            var c1a = new Vector3 (cOrigin.x, cOrigin.y, cOrigin.z);
                            var c1b = new Vector3 (cOrigin.x, cOrigin.y, cExtent.z);
                            var c2a = new Vector3 (cExtent.x, cOrigin.y, cOrigin.z);
                            var c2b = new Vector3 (cExtent.x, cOrigin.y, cExtent.z);

                            DrawArrowBidirectional (c1a, c1b, Color.HSVToRGB (0.55f, 0.5f, 1f));
                            DrawArrowBidirectional (c2a, c2b, Color.HSVToRGB (0.55f, 0.5f, 1f));
                        }
                        else if (volumeEditingMode == VolumeEditingMode.SizeYHeight)
                        {
                            var c1a = new Vector3 (cOrigin.x, cOrigin.y, cOrigin.z);
                            var c1b = new Vector3 (cOrigin.x, cExtent.y, cOrigin.z);

                            var c2a = new Vector3 (cOrigin.x, cOrigin.y, cExtent.z);
                            var c2b = new Vector3 (cOrigin.x, cExtent.y, cExtent.z);

                            var c3a = new Vector3 (cExtent.x, cOrigin.y, cOrigin.z);
                            var c3b = new Vector3 (cExtent.x, cExtent.y, cOrigin.z);

                            var c4a = new Vector3 (cExtent.x, cOrigin.y, cExtent.z);
                            var c4b = new Vector3 (cExtent.x, cExtent.y, cExtent.z);

                            DrawArrowBidirectional (c1a, c1b, Color.HSVToRGB (0.3f, 0.5f, 1f), true);
                            DrawArrowBidirectional (c2a, c2b, Color.HSVToRGB (0.3f, 0.5f, 1f), true);
                            DrawArrowBidirectional (c3a, c3b, Color.HSVToRGB (0.3f, 0.5f, 1f), true);
                            DrawArrowBidirectional (c4a, c4b, Color.HSVToRGB (0.3f, 0.5f, 1f), true);
                        }
                    }
                }
            }

            if (e.type == EventType.ScrollWheel)
            {
                e.Use ();
                var forward = e.delta.y > 0f;
                buttonPressed = e.shift ? (forward ? KeyCode.LeftBracket : KeyCode.RightBracket) : (forward ? KeyCode.PageDown : KeyCode.PageUp);

                if (e.shift)
                {
                    var editIndexCurrent = (int)volumeEditingMode;
                    var editIndexNew = editIndexCurrent.OffsetAndWrap (forward, 3);
                    var editModeNew = (VolumeEditingMode)editIndexNew;
                    volumeEditingMode = editModeNew;
                }
                else
                {
                    if (volumeEditingMode == VolumeEditingMode.PositionY)
                    {
                        selectedVolumeData.origin.y += forward ? 1 : -1;
                        selectedVolumeData.ValidateOrigin ();
                    }
                    else if (volumeEditingMode == VolumeEditingMode.SizeXLength)
                    {
                        selectedVolumeData.size.x += forward ? -1 : 1;
                        selectedVolumeData.ValidateSize ();
                    }
                    else if (volumeEditingMode == VolumeEditingMode.SizeZWidth)
                    {
                        selectedVolumeData.size.z += forward ? -1 : 1;
                        selectedVolumeData.ValidateSize ();
                    }
                    else if (volumeEditingMode == VolumeEditingMode.SizeYHeight)
                    {
                        selectedVolumeData.size.y += forward ? -1 : 1;
                        selectedVolumeData.ValidateSize ();
                    }
                }
            }
        }

        var boundsLocal = selectedVolumeData.size;
        var volumeLengthLocal = boundsLocal.x * boundsLocal.y * boundsLocal.z;

        for (var i = volumeLengthLocal - 1; i >= 0; i -= 1)
        {
            var internalPositionLocal = AreaUtility.GetVolumePositionFromIndex (i, boundsLocal);
            var internalPositionFull = internalPositionLocal + selectedVolumeData.origin;
            var sourcePointIndex = AreaUtility.GetIndexFromVolumePosition (internalPositionFull, boundsFull);

            // Skip if index is invalid
            if (sourcePointIndex < 0 || sourcePointIndex >= pointsTotal)
            {
                continue;
            }

            // Fetch the point, skip if it's empty
            var sourcePoint = points[sourcePointIndex];
            if (sourcePoint.pointState == AreaVolumePointState.Empty)
            {
                continue;
            }

            // Skip all points that designers specified as indestructible,
            // or points that are indestructible due to factors like height or tileset
            if (!sourcePoint.destructible || sourcePoint.indestructibleIndirect)
            {
                continue;
            }

            var gradientInterpolant = (float)internalPositionLocal.y / selectedVolumeData.size.y;

            // Increment count of destroyed points

            var colorPoint = sourcePoint.pointState == AreaVolumePointState.FullDestroyed
                ? Color.HSVToRGB (Mathf.Lerp (0.05f, 0f, gradientInterpolant), 0.5f, 1f)
                : Color.HSVToRGB (Mathf.Lerp (0.2f, 0.25f, gradientInterpolant), 0.5f, 1f);
            cubeDescs.Add (new CubeDesc
            {
                origin = sourcePoint.pointPositionLocal,
                size = 1.4f,
                color = colorPoint
            });
        }

        DrawCubeListSorted (cubeDescs);
    }

    void CircleHandleCap (Vector3 position, Quaternion circleRotation, float radius)
    {
        if (titlePanel.OverlapsUI (position))
        {
            return;
        }
        Handles.CircleHandleCap (0, position, circleRotation, radius, EventType.Repaint);
    }

    void CubeHandleCap (Vector3 position, float size, int controlID = 0)
    {
        if (titlePanel.OverlapsUI (position))
        {
            return;
        }
        Handles.CubeHandleCap (controlID, position, Quaternion.identity, size, EventType.Repaint);
    }

    void DrawCubeListSorted (List<CubeDesc> list, bool lighting = true, CompareFunction zTest = CompareFunction.Always, bool fadeUsed = true)
    {
        if (list == null || list.Count == 0)
            return;

        var scene = SceneView.lastActiveSceneView;
        if (scene == null || scene.camera == null)
            return;

        var c = scene.camera;
        var t = c.transform;
        var cameraPos = t.position;

        var distMin = 100000f;
        var distMax = 0f;

        for (int i = 0; i < list.Count; ++i)
        {
            var value = list[i];
            var valueNew = new CubeDesc
            {
                origin = value.origin,
                size = value.size,
                color = value.color,
                dist = Vector3.SqrMagnitude (value.origin - cameraPos)
            };

            list[i] = valueNew;
            if (distMax < valueNew.dist)
                distMax = valueNew.dist;

            if (distMin > valueNew.dist)
                distMin = valueNew.dist;
        }

        if (distMax <= 0f || distMax - distMin <= 0f)
            fadeUsed = false;

        #if PB_MODSDK
        if (list.Count < 3)
        {
            // No sense in fading a 2-block structure. One of the blocks will be at max dist and
            // therefore be invisible (alpha = 0).
            fadeUsed = false;
        }
        #endif

        var distSpan = distMax - distMin;

        list.Sort ((a, b) => b.dist.CompareTo (a.dist));

        var hc = Handles.color;
        var l = Handles.lighting;
        var zt = Handles.zTest;

        Handles.zTest = zTest;
        Handles.lighting = lighting;

        foreach (var cubeDesc in list)
        {
            if (fadeUsed)
            {
                var interpolant = Mathf.Clamp01 ((cubeDesc.dist - distMin) / distSpan);
                #if PB_MODSDK
                // Don't fade out the farthest block completely. Otherwise, for some structures it will look like the farthest block is missing.
                var fade = Mathf.Lerp (1f, 0.125f, interpolant);
                #else
                var fade = Mathf.Lerp (1f, 0f, interpolant);
                #endif
                Handles.color = cubeDesc.color.WithAlpha (fade);
            }
            else
            {
                Handles.color = cubeDesc.color;
            }
            #if PB_MODSDK
            CubeHandleCap (cubeDesc.origin, cubeDesc.size);
            #else
            Handles.CubeHandleCap (0, cubeDesc.origin, Quaternion.identity, cubeDesc.size, EventType.Repaint);
            #endif
        }

        Handles.color = hc;
        Handles.lighting = l;
        Handles.zTest = zt;
    }

    void DrawCube (Vector3 origin, float size, Color colorMain, bool lighting = true, CompareFunction zTest = CompareFunction.Always)
    {
        var hc = Handles.color;
        var l = Handles.lighting;
        var zt = Handles.zTest;
        Handles.zTest = zTest;
        Handles.lighting = lighting;
        Handles.color = colorMain;
        #if PB_MODSDK
        CubeHandleCap (origin, size);
        #else
        Handles.CubeHandleCap (0, origin, Quaternion.identity, size, EventType.Repaint);
        #endif
        Handles.color = hc;
        Handles.lighting = l;
        Handles.zTest = zt;
    }

    void DrawZCube (Vector3 origin, float size, Color colorMain, Color colorCulled)
    {
        var hc = Handles.color;
        var zt = Handles.zTest;
        Handles.zTest = CompareFunction.LessEqual;
        Handles.color = colorMain;
        #if PB_MODSDK
        CubeHandleCap (origin, size);
        #else
        Handles.CubeHandleCap (0, origin, Quaternion.identity, size, EventType.Repaint);
        #endif
        Handles.zTest = CompareFunction.Greater;
        Handles.color = colorCulled;
        #if PB_MODSDK
        CubeHandleCap (origin, size);
        #else
        Handles.CubeHandleCap (0, origin, Quaternion.identity, size, EventType.Repaint);
        #endif
        Handles.color = hc;
        Handles.zTest = zt;
    }

    void DrawArrow (Vector3 posOrigin, Vector3 posDestination, Color color)
    {
        if (posOrigin == posDestination)
            return;

        Vector3 difference = posDestination - posOrigin;
        difference.y = -difference.y;
        Vector3 dir = difference.normalized;

        Handles.color = color;
        Handles.DrawLine (posOrigin, posDestination, 2f);

        var sizeDestination = HandleUtility.GetHandleSize (posOrigin);
        #if PB_MODSDK
        if (!titlePanel.OverlapsUI (posDestination))
        {
            Handles.ConeHandleCap (0, posDestination, Quaternion.LookRotation (-dir), sizeDestination * 0.2f, Event.current.type);
        }
        #else
        Handles.ConeHandleCap (0, posDestination, Quaternion.LookRotation (-dir), sizeDestination * 0.2f, Event.current.type);
        #endif
    }

    void DrawArrowBidirectional (Vector3 posOrigin, Vector3 posDestination, Color color, bool flip = false)
    {
        if (posOrigin == posDestination)
            return;

        Vector3 difference = posDestination - posOrigin;
        difference.y = -difference.y;
        Vector3 dir = difference.normalized;

        Handles.color = color;
        Handles.DrawLine (posOrigin, posDestination, 2f);

        var sizeDestination = HandleUtility.GetHandleSize (posOrigin);
        #if PB_MODSDK
        if (!titlePanel.OverlapsUI (posDestination))
        {
            Handles.ConeHandleCap (0, posDestination, Quaternion.LookRotation (flip ? dir : -dir), sizeDestination * 0.2f, Event.current.type);
        }
        #else
        Handles.ConeHandleCap (0, posDestination, Quaternion.LookRotation (flip ? dir : -dir), sizeDestination * 0.2f, Event.current.type);
        #endif

        var sizeOrigin = HandleUtility.GetHandleSize (posOrigin);
        #if PB_MODSDK
        if (!titlePanel.OverlapsUI (posOrigin))
        {
            Handles.ConeHandleCap (0, posOrigin, Quaternion.LookRotation (flip ? -dir : dir), sizeOrigin * 0.2f, Event.current.type);
        }
        #else
        Handles.ConeHandleCap (0, posOrigin, Quaternion.LookRotation (flip ? -dir : dir), sizeOrigin * 0.2f, Event.current.type);
        #endif
    }

    void DrawVolume (string key, Vector3 posA, Vector3 posB, Color colorMain, Color colorCulled)
    {
        Vector3 difference = posB - posA;
        difference.y = -difference.y;
        Vector3 center = (posA + posB) / 2f;

        var hc = Handles.color;
        var zt = Handles.zTest;

        var colorMainTransparent = colorMain.WithAlpha (0.15f);
        var colorCulledTransparent = colorCulled.WithAlpha (0.15f);

        Handles.zTest = CompareFunction.LessEqual;
        Handles.color = colorMain;
        #if PB_MODSDK
        CubeHandleCap (posA, 0.5f);
        CubeHandleCap (posB, 0.5f, controlID: 1);
        #else
        Handles.CubeHandleCap (0, posA, Quaternion.identity, 0.5f, EventType.Repaint);
        Handles.CubeHandleCap (1, posB, Quaternion.identity, 0.5f, EventType.Repaint);
        #endif
        Handles.zTest = CompareFunction.Greater;
        Handles.color = colorCulled;
        #if PB_MODSDK
        CubeHandleCap (posA, 0.5f);
        CubeHandleCap (posB, 0.5f, controlID: 1);
        #else
        Handles.CubeHandleCap (0, posA, Quaternion.identity, 0.5f, EventType.Repaint);
        Handles.CubeHandleCap (1, posB, Quaternion.identity, 0.5f, EventType.Repaint);
        #endif

        Handles.color = Color.white.WithAlpha (1f);
        transferPreviewVerts[0] = new Vector3 (posA.x, posB.y, posA.z);
        transferPreviewVerts[1] = new Vector3 (posA.x, posA.y, posA.z);
        transferPreviewVerts[2] = new Vector3 (posB.x, posA.y, posA.z);
        transferPreviewVerts[3] = new Vector3 (posB.x, posB.y, posA.z);
        Handles.zTest = CompareFunction.LessEqual;
        Handles.DrawSolidRectangleWithOutline (transferPreviewVerts, colorMainTransparent, colorMain);
        Handles.zTest = CompareFunction.Greater;
        Handles.DrawSolidRectangleWithOutline (transferPreviewVerts, colorCulledTransparent, colorCulled);

        transferPreviewVerts[0] = new Vector3 (posB.x, posB.y, posA.z);
        transferPreviewVerts[1] = new Vector3 (posB.x, posA.y, posA.z);
        transferPreviewVerts[2] = new Vector3 (posB.x, posA.y, posB.z);
        transferPreviewVerts[3] = new Vector3 (posB.x, posB.y, posB.z);
        Handles.zTest = CompareFunction.LessEqual;
        Handles.DrawSolidRectangleWithOutline (transferPreviewVerts, colorMainTransparent, colorMain);
        Handles.zTest = CompareFunction.Greater;
        Handles.DrawSolidRectangleWithOutline (transferPreviewVerts, colorCulledTransparent, colorCulled);

        transferPreviewVerts[0] = new Vector3 (posB.x, posB.y, posB.z);
        transferPreviewVerts[1] = new Vector3 (posB.x, posA.y, posB.z);
        transferPreviewVerts[2] = new Vector3 (posA.x, posA.y, posB.z);
        transferPreviewVerts[3] = new Vector3 (posA.x, posB.y, posB.z);
        Handles.zTest = CompareFunction.LessEqual;
        Handles.DrawSolidRectangleWithOutline (transferPreviewVerts, colorMainTransparent, colorMain);
        Handles.zTest = CompareFunction.Greater;
        Handles.DrawSolidRectangleWithOutline (transferPreviewVerts, colorCulledTransparent, colorCulled);

        transferPreviewVerts[0] = new Vector3 (posA.x, posB.y, posB.z);
        transferPreviewVerts[1] = new Vector3 (posA.x, posA.y, posB.z);
        transferPreviewVerts[2] = new Vector3 (posA.x, posA.y, posA.z);
        transferPreviewVerts[3] = new Vector3 (posA.x, posB.y, posA.z);
        Handles.zTest = CompareFunction.LessEqual;
        Handles.DrawSolidRectangleWithOutline (transferPreviewVerts, colorMainTransparent, colorMain);
        Handles.zTest = CompareFunction.Greater;
        Handles.DrawSolidRectangleWithOutline (transferPreviewVerts, colorCulledTransparent, colorCulled);

        Handles.color = hc;
        Handles.zTest = zt;

        Handles.Label (center + Vector3.up * 7f, $"{key} (Volume)\n");
    }

    void DrawField (Vector3 origin, Vector3 size, float rotation, Color colorMain, Color colorCulled)
    {
        var hc = Handles.color;
        var zt = Handles.zTest;

        var colorMainTransparent = colorMain.WithAlpha (0.15f);
        var colorCulledTransparent = colorCulled.WithAlpha (0.15f);

        var rotationQ = Quaternion.Euler (0f, rotation, 0f);
        var sizeHalf = size * 0.5f;
        var offsetLow = new Vector3 (0f, -size.y, 0f);

        var posTop1 = origin + rotationQ * new Vector3 (sizeHalf.x, 0f, sizeHalf.z);
        var posTop2 = origin + rotationQ * new Vector3 (-sizeHalf.x, 0f, sizeHalf.z);
        var posTop3 = origin + rotationQ * new Vector3 (-sizeHalf.x, 0f, -sizeHalf.z);
        var posTop4 = origin + rotationQ * new Vector3 (sizeHalf.x, 0f, -sizeHalf.z);

        var posLow1 = posTop1 + offsetLow;
        var posLow2 = posTop2 + offsetLow;
        var posLow3 = posTop3 + offsetLow;
        var posLow4 = posTop4 + offsetLow;

        Handles.zTest = CompareFunction.LessEqual;
        Handles.color = colorMain;
        #if PB_MODSDK
        CubeHandleCap (origin, 0.5f);
        #else
        Handles.CubeHandleCap (0, origin, Quaternion.identity, 0.5f, EventType.Repaint);
        #endif
        Handles.DrawLine (posTop1, posLow1);
        Handles.DrawLine (posTop2, posLow2);
        Handles.DrawLine (posTop3, posLow3);
        Handles.DrawLine (posTop4, posLow4);

        Handles.color = Color.white.WithAlpha (1f);
        Handles.DrawLine (posTop1, posTop2);
        Handles.DrawLine (posTop2, posTop3);
        Handles.DrawLine (posTop3, posLow4);
        Handles.DrawLine (posLow4, posTop1);

        Handles.zTest = CompareFunction.Greater;
        Handles.color = colorCulled;
        #if PB_MODSDK
        CubeHandleCap (origin, 0.5f);
        #else
        Handles.CubeHandleCap (0, origin, Quaternion.identity, 0.5f, EventType.Repaint);
        #endif
        Handles.DrawLine (posTop1, posLow1);
        Handles.DrawLine (posTop2, posLow2);
        Handles.DrawLine (posTop3, posLow3);
        Handles.DrawLine (posTop4, posLow4);

        Handles.color = Color.white.WithAlpha (1f);
        transferPreviewVerts[0] = posTop1;
        transferPreviewVerts[1] = posTop2;
        transferPreviewVerts[2] = posTop3;
        transferPreviewVerts[3] = posTop4;

        Handles.zTest = CompareFunction.LessEqual;
        Handles.DrawSolidRectangleWithOutline (transferPreviewVerts, colorMainTransparent, colorMain);
        Handles.zTest = CompareFunction.Greater;
        Handles.DrawSolidRectangleWithOutline (transferPreviewVerts, colorCulledTransparent, colorCulled);

        transferPreviewVerts[0] = posLow1;
        transferPreviewVerts[1] = posLow2;
        transferPreviewVerts[2] = posLow3;
        transferPreviewVerts[3] = posLow4;
        Handles.zTest = CompareFunction.LessEqual;
        Handles.DrawSolidRectangleWithOutline (transferPreviewVerts, colorMainTransparent, colorMain);
        Handles.zTest = CompareFunction.Greater;
        Handles.DrawSolidRectangleWithOutline (transferPreviewVerts, colorCulledTransparent, colorCulled);

        Handles.color = hc;
        Handles.zTest = zt;
    }

    readonly SpawnGroupDrawer spawnPointsDrawer = new SpawnGroupDrawer (3f, 6f, colorMain, colorUnused);
    readonly WaypointGroupDrawer waypointsDrawer = new WaypointGroupDrawer (3f, 6f, colorMain, colorUnused);
    readonly List<CubeDesc> cubeDescs = new List<CubeDesc> ();
    VolumeEditingMode volumeEditingMode = VolumeEditingMode.PositionY;

    static Vector3[] transferPreviewVerts = new Vector3[4];

    static readonly Color colorUnused = Color.gray;
    static readonly Color colorMain = Color.white;
    static readonly Color colorEnemy = Color.Lerp (Color.red, Color.white, 0.5f);
    static readonly Color colorFriendly = Color.Lerp (Color.cyan, Color.white, 0.5f);
    static readonly Color colorEmpty = Color.yellow;

    static readonly Color colorMainLocation = Color.Lerp (Color.cyan, Color.white, 0.5f);
    static readonly Color colorUnusedLocation = Color.Lerp (Color.cyan, Color.gray, 0.5f);

    static readonly Color colorNeighbour = Color.red;
    static readonly Color colorSpawn = Color.red;
    static readonly Color colorLoot = Color.green;
    static readonly Color colorRetreat = Color.blue;

    public enum VolumeEditingMode
    {
        PositionY = 0,
        SizeXLength = 1,
        SizeZWidth = 2,
        SizeYHeight = 3
    }

    struct CubeDesc
    {
        public Vector3 origin;
        public float size;
        public Color color;
        public float dist;
    }

    #if PB_MODSDK
    void EnsureSegmentAndFieldVisibility ()
    {
        if (!CombatSceneHelper.ins.fieldHelper.gameObject.activeSelf)
            CombatSceneHelper.ins.fieldHelper.gameObject.SetActive (true);
        if (!CombatSceneHelper.ins.segmentHelper.gameObject.activeSelf)
            CombatSceneHelper.ins.segmentHelper.gameObject.SetActive (true);
    }

    readonly TitlePanelDrawer titlePanel = new TitlePanelDrawer ();
    readonly BirdsEyeView birdsEye = new BirdsEyeView ();
    #endif
}

abstract class PointGroupDrawer
{
    public void Draw (DataContainerCombatArea area, Ray worldRay, bool inputCheckSuccessful, Quaternion circleRotation, Event e)
    {
        DrawPoints (area, circleRotation, e.alt, e.shift);
        HandleInput (worldRay, inputCheckSuccessful, circleRotation, e);
    }

    public void HandleInput (Ray worldRay, bool inputCheckSuccessful, Quaternion circleRotation, Event e)
    {
        #if PB_MODSDK
        if (!DataContainerModData.hasSelectedConfigs)
        {
            return;
        }
        #endif
        if (!IsSelectionActive)
        {
            return;
        }
        if (!e.alt || !Physics.Raycast (worldRay, out var hitInfo, 1000, LayerMasks.environmentMask))
        {
            return;
        }
        #if PB_MODSDK
        if (overlapsPanel (hitInfo.point))
        {
            return;
        }
        #endif

        Handles.color = colorMain;
        Handles.DrawLine (hitInfo.point, hitInfo.point + Vector3.up * 5f);

        var pos = hitInfo.point;
        var posValid = true;
        #if PB_MODSDK
        var pointIndex = -1;
        #endif
        var points = GetPoints ();
        foreach (var pointExisting in points)
        {
            var sqrMagnitude = (pos - pointExisting.point).sqrMagnitude;
            if (sqrMagnitude < collisionRadiusSqr)
            {
                #if PB_MODSDK
                sqrMagnitude = (pos.Flatten2D () - pointExisting.point.Flatten2D ()).sqrMagnitude;
                if (sqrMagnitude < circleRadiusSqr)
                {
                    var hc = Handles.color;
                    Handles.color = colorHover;
                    Handles.DrawSolidDisc (pointExisting.point, Vector3.up, circleSize);
                    Handles.color = hc;
                    pointIndex = pointExisting.index;
                }
                #endif
                posValid = false;
                break;
            }
        }

        Handles.color = posValid ? colorMain : colorEnemy;
        Handles.CircleHandleCap (0, hitInfo.point, circleRotation, 3f, EventType.Repaint);

        #if PB_MODSDK
        if (pointIndex != -1 && e.OnEventType (EventType.MouseDown))
        {
            switch (e.button)
            {
                case 0:
                    if (IsSelected (points[pointIndex]))
                    {
                        DeselectGroup ();
                    }
                    else
                    {
                        UpdateSelection (points[pointIndex], changeGroup: false);
                    }
                    break;
                case 1:
                    points.RemoveAt (pointIndex);
                    OnRemovedPoint (pointIndex);
                    DataMultiLinkerCombatArea.unsavedChangesPossible = true;
                    break;
                case 2:
                    if (e.shift)
                    {
                        points[pointIndex].SnapToGrid ();
                        OnChangedPointPosition (true);
                    }
                    else
                    {
                        points[pointIndex].Ground ();
                    }
                    DataMultiLinkerCombatArea.unsavedChangesPossible = true;
                    break;
            }
            GUIHelper.RequestRepaint ();
            e.Use ();
            return;
        }
        #endif

        if (!inputCheckSuccessful || !posValid)
        {
            return;
        }

        var rot = points.Count > 0 ? points[points.Count - 1].rotation : Vector3.zero;
        var point = new DataBlockAreaPoint ()
        {
            point = pos,
            rotation = rot,
        };
        point.SnapToGrid ();

        points.Add (point);
        #if PB_MODSDK
        UpdateSelection (point, changeGroup: false);
        DataMultiLinkerCombatArea.unsavedChangesPossible = true;
        #endif
        OnChangedPointPosition (true);
        GUIHelper.RequestRepaint ();
        e.Use ();
    }

    public System.Func<Vector3, bool> overlapsPanel = NoOverlap;

    protected abstract void DrawPoints (DataContainerCombatArea area, Quaternion circleRotation, bool alt, bool shift);

    protected bool DrawPointsLoop (List<DataBlockAreaPoint> points, bool selectedGroup, Color colorFull, Color colorSemi, Quaternion circleRotation, bool alt, bool shift)
    {
        var errorInAny = false;
        for (var i = 0; i < points.Count; ++i)
        {
            var point = points[i];
            #if !PB_MODSDK
            if (point == null)
            {
                // It is possible the points collection has a null point. This is because the default value for DataBlockAreaPoint is null and
                // the ListDrawerSettings attribute on a points collection has AlwaysAddDefaultValue = true. So if the user adds a new point to
                // the collection through the inspector, the null will be seen on every update until the user selects DataBlockAreaPoint from
                // the object picker for the new entry in the points collection.
                continue;
            }
            #endif

            var position = point.point;
            var rotation = Quaternion.Euler (point.rotation);
            var forward = rotation * Vector3.forward;

            if (point.index == -1)
            {
                point.index = i;
            }

            var error = false;
            if (ShowRaycasts)
            {
                var groundingRayOrigin = position + Vector3.up;
                var groundingRay = new Ray (groundingRayOrigin, Vector3.down);
                var hit = Physics.Raycast (groundingRay, out var groundingHit, 100f, LayerMasks.environmentMask);
                if (hit && groundingHit.distance > 1.1f)
                {
                    errorInAny = true;
                    error = true;
                    Handles.color = colorEnemy;
                    Handles.DrawLine (groundingHit.point, position);
                }
            }

            Handles.color = colorFull;
            if (selectedGroup)
            {
                Handles.color = colorFull;
                Handles.DrawLine (position, position + Vector3.up * 5f);
                Handles.Label (position + Vector3.up * 7f, i + "\n");
            }

            Handles.color = colorSemi;
            if (i > 0)
            {
                Handles.DrawLine (position, points[i - 1].point);
            }

            Handles.color = error ? colorEnemy : colorFull;
            Handles.DrawLine (position + forward * 3f, position + forward * 6f);
            // Handles.DrawLine (position + forward, position - forward);
            // Handles.DrawLine (position + right, position - right);

            #if PB_MODSDK
            if (overlapsPanel (position))
            {
                continue;
            }
            #endif

            Handles.CircleHandleCap (0, position, circleRotation, circleSize, EventType.Repaint);
            var size = HandleUtility.GetHandleSize (position);
            if (Handles.Button (position, rotation, size * 0.05f, size * 0.2f, Handles.DotHandleCap))
            {
                UpdateSelection (point);
                GUIHelper.RequestRepaint ();
            }

            #if PB_MODSDK
            if (!DataContainerModData.hasSelectedConfigs)
            {
                continue;
            }
            #endif

            if (!IsSelected (point))
            {
                continue;
            }

            EditorGUI.BeginChangeCheck ();
            position = Handles.DoPositionHandle (position, rotation);
            if (EditorGUI.EndChangeCheck ())
            {
                point.point = position;
                if (alt)
                {
                    var groundingRayOrigin2 = position + Vector3.up * 100f;
                    var groundingRay2 = new Ray (groundingRayOrigin2, Vector3.down);
                    if (Physics.Raycast (groundingRay2, out var groundingHit2, 200f, LayerMasks.environmentMask))
                    {
                        point.point = groundingHit2.point;
                    }
                }
                if (shift)
                {
                    point.SnapToGrid ();
                }
                OnChangedPointPosition (selectedGroup);
                #if PB_MODSDK
                DataMultiLinkerCombatArea.unsavedChangesPossible = true;
                #endif
            }

            EditorGUI.BeginChangeCheck ();
            rotation = Handles.DoRotationHandle (rotation, position);
            if (EditorGUI.EndChangeCheck ())
            {
                point.rotation = new Vector3 (0f, rotation.eulerAngles.y, 0f);
                #if PB_MODSDK
                DataMultiLinkerCombatArea.unsavedChangesPossible = true;
                #endif
            }
        }

        return errorInAny;
    }

    protected abstract bool ShowRaycasts { get; }
    protected abstract bool IsSelectionActive { get; }
    protected abstract void UpdateSelection (DataBlockAreaPoint point, bool changeGroup = true);
    protected abstract bool IsSelected (DataBlockAreaPoint point);
    protected abstract void DeselectGroup ();
    protected abstract void OnChangedPointPosition (bool selectedGroup);
    protected abstract void OnRemovedPoint (int index);
    protected abstract List<DataBlockAreaPoint> GetPoints ();

    protected PointGroupDrawer (float circleSize, float collisionRadius, Color colorMain, Color colorUnused)
    {
        this.circleSize = circleSize;
        circleRadiusSqr = circleSize * circleSize;
        collisionRadiusSqr = collisionRadius * collisionRadius;
        this.colorMain = colorMain;
        this.colorUnused = colorUnused;
    }

    protected readonly Color colorMain;
    protected readonly Color colorUnused;

    protected static readonly Color colorEnemy = Color.Lerp (Color.red, Color.white, 0.5f);

    readonly float circleSize;
    readonly float circleRadiusSqr;
    readonly float collisionRadiusSqr;

    static bool NoOverlap (Vector3 _) => false;
    static readonly Color colorHover = Color.magenta.WithAlpha(0.33f);
}

sealed class SpawnGroupDrawer : PointGroupDrawer
{
    public SpawnGroupDrawer (float circleSize, float collisionRadius, Color colorMain, Color colorUnused)
        : base (circleSize, collisionRadius, colorMain, colorUnused)
    { }

    protected override void DrawPoints (DataContainerCombatArea area, Quaternion circleRotation, bool alt, bool shift)
    {
        if (area.spawnGroups == null || !DataMultiLinkerCombatArea.presentation.showSpawns)
        {
            return;
        }

        foreach (var kvp in area.spawnGroups)
        {
            var spawnGroupKey = kvp.Key;
            spawnGroup = kvp.Value;

            if (spawnGroup == null || spawnGroup.points == null)
            {
                continue;
            }

            var selectedGroup = spawnGroup == DataMultiLinkerCombatArea.selectedSpawnGroup;
            var colorFull = selectedGroup ? colorMain : colorUnused;
            var colorSemi = colorFull.WithAlpha (0.25f);
            var errorInAny = DrawPointsLoop (spawnGroup.points, selectedGroup, colorFull, colorSemi, circleRotation, alt, shift);
            Handles.color = errorInAny ? colorEnemy : colorUnused;
            Handles.Label (spawnGroup.averagePosition + Vector3.up * 7f, spawnGroupKey.Replace ("perimeter_", "p_"));
        }
    }

    protected override void UpdateSelection (DataBlockAreaPoint spawnPoint, bool changeGroup = true)
    {
        if (DataMultiLinkerCombatArea.selectedSpawnGroup == null)
        {
            DataMultiLinkerCombatArea.DeselectSelections ();
        }
        if (changeGroup)
        {
            DataMultiLinkerCombatArea.selectedSpawnGroup = spawnGroup;
        }
        DataMultiLinkerCombatArea.selectedSpawnPoint = spawnPoint;
    }

    protected override bool ShowRaycasts => DataMultiLinkerCombatArea.presentation.showSpawnRaycasts;
    protected override bool IsSelectionActive => DataMultiLinkerCombatArea.selectedSpawnGroup != null && DataMultiLinkerCombatArea.presentation.showSpawns;
    protected override bool IsSelected (DataBlockAreaPoint spawnPoint) => DataMultiLinkerCombatArea.selectedSpawnPoint == spawnPoint;
    protected override void OnChangedPointPosition (bool selectedGroup) => (selectedGroup ? DataMultiLinkerCombatArea.selectedSpawnGroup : spawnGroup).RefreshAveragePosition ();

    protected override void DeselectGroup ()
    {
        DataMultiLinkerCombatArea.selectedSpawnGroup = null;
        DataMultiLinkerCombatArea.selectedSpawnPoint = null;
    }

    protected override void OnRemovedPoint (int index)
    {
        if (DataMultiLinkerCombatArea.selectedSpawnGroup.points.Count == 0)
        {
            DataMultiLinkerCombatArea.selectedSpawnGroup = null;
            DataMultiLinkerCombatArea.selectedSpawnPoint = null;
            return;
        }
        DataMultiLinkerCombatArea.selectedSpawnGroup.RefreshAveragePosition ();
        var removedSelected = DataMultiLinkerCombatArea.selectedSpawnPoint.index == index;
        var points = DataMultiLinkerCombatArea.selectedSpawnGroup.points;
        for (var i = 0; i < points.Count; i += 1)
        {
            var spawnPoint = points[i];
            spawnPoint.index = i;
        }
        if (!removedSelected)
        {
            return;
        }
        if (points.Count <= index)
        {
            index = points.Count - 1;
        }
        DataMultiLinkerCombatArea.selectedSpawnPoint = points[index];
    }

    protected override List<DataBlockAreaPoint> GetPoints ()
    {
        if (DataMultiLinkerCombatArea.selectedSpawnGroup.points == null)
        {
            DataMultiLinkerCombatArea.selectedSpawnGroup.points = new List<DataBlockAreaPoint> ();
        }
        return DataMultiLinkerCombatArea.selectedSpawnGroup.points;
    }

    DataBlockAreaSpawnGroup spawnGroup;
}

sealed class WaypointGroupDrawer : PointGroupDrawer
{
    public WaypointGroupDrawer (float circleSize, float collisionRadius, Color colorMain, Color colorUnused)
        : base (circleSize, collisionRadius, colorMain, colorUnused)
    { }

    protected override void DrawPoints (DataContainerCombatArea area, Quaternion circleRotation, bool alt, bool shift)
    {
        if (area.waypointGroups == null || !DataMultiLinkerCombatArea.presentation.showWaypoints)
        {
            return;
        }

        foreach (var kvp in area.waypointGroups)
        {
            var waypointGroupKey = kvp.Key;
            waypointGroup = kvp.Value;

            if (waypointGroup == null || waypointGroup.points == null || waypointGroup.points.Count == 0)
            {
                continue;
            }

            if (string.IsNullOrEmpty (waypointGroup.key))
            {
                waypointGroup.key = waypointGroupKey;
            }

            var selectedGroup = waypointGroup == DataMultiLinkerCombatArea.selectedWaypointGroup;
            var colorFull = selectedGroup ? colorMainWaypoint : colorUnusedWaypoint;
            var colorSemi = colorFull.WithAlpha (0.25f);
            var errorInAny = DrawPointsLoop (waypointGroup.points, selectedGroup, colorFull, colorSemi, circleRotation, alt, shift);
            Handles.color = errorInAny ? colorEnemy : colorUnused;
            Handles.Label (waypointGroup.points[waypointGroup.points.Count - 1].point + Vector3.up * 7f, waypointGroupKey.Replace ("perimeter_", "p_"));
        }
    }

    protected override void UpdateSelection (DataBlockAreaPoint waypoint, bool changeGroup = true)
    {
        if (DataMultiLinkerCombatArea.selectedWaypointGroup == null)
        {
            DataMultiLinkerCombatArea.DeselectSelections ();
        }
        if (changeGroup)
        {
            DataMultiLinkerCombatArea.selectedWaypointGroup = waypointGroup;
        }
        DataMultiLinkerCombatArea.selectedWaypoint = waypoint;
    }

    protected override bool ShowRaycasts => DataMultiLinkerCombatArea.presentation.showSpawnRaycasts;
    protected override bool IsSelectionActive => DataMultiLinkerCombatArea.selectedSpawnGroup == null
        && DataMultiLinkerCombatArea.selectedWaypointGroup != null && DataMultiLinkerCombatArea.presentation.showWaypoints;
    protected override bool IsSelected (DataBlockAreaPoint waypoint) => DataMultiLinkerCombatArea.selectedWaypoint == waypoint;
    protected override void OnChangedPointPosition (bool selectedGroup) { }

    protected override void DeselectGroup ()
    {
        DataMultiLinkerCombatArea.selectedWaypointGroup = null;
        DataMultiLinkerCombatArea.selectedWaypoint = null;
    }

    protected override void OnRemovedPoint (int index)
    {
        if (DataMultiLinkerCombatArea.selectedWaypointGroup.points.Count == 0)
        {
            DataMultiLinkerCombatArea.selectedWaypointGroup = null;
            DataMultiLinkerCombatArea.selectedWaypoint = null;
            return;
        }
        var removedSelected = DataMultiLinkerCombatArea.selectedWaypoint.index == index;
        var points = DataMultiLinkerCombatArea.selectedWaypointGroup.points;
        for (var i = 0; i < points.Count; i += 1)
        {
            var waypoint = points[i];
            waypoint.index = i;
        }
        if (!removedSelected)
        {
            return;
        }
        if (points.Count <= index)
        {
            index = points.Count - 1;
        }
        DataMultiLinkerCombatArea.selectedWaypoint = points[index];
    }

    protected override List<DataBlockAreaPoint> GetPoints ()
    {
        if (DataMultiLinkerCombatArea.selectedWaypointGroup.points == null)
        {
            DataMultiLinkerCombatArea.selectedWaypointGroup.points = new List<DataBlockAreaPoint> ();
        }
        return DataMultiLinkerCombatArea.selectedWaypointGroup.points;
    }

    DataBlockAreaWaypointGroup waypointGroup;

    static readonly Color colorUnusedWaypoint = new Color (0.5f, 0.55f, 0.6f);
    static readonly Color colorMainWaypoint = new Color (0.7f, 0.8f, 1f);
}

static class InstructionPanelDrawer
{
    public static void Draw ()
    {
        Handles.BeginGUI ();
        GUILayout.BeginVertical (EditorStyles.helpBox, GUILayout.Width (250f));
        DrawLeader (full: false);
        GUILayout.EndVertical ();
        Handles.EndGUI ();
    }

    public static void DrawFull (ref DataMultiLinkerCombatAreaInspector.VolumeEditingMode volumeEditingMode)
    {
        Handles.BeginGUI ();
        GUILayout.BeginVertical (EditorStyles.helpBox, GUILayout.Width (250f));
        DrawLeader ();
        DrawExtraInstructions (ref volumeEditingMode);
        GUILayout.EndVertical ();
        Handles.EndGUI ();
    }

    static void DrawLeader (bool full = true)
    {
        GUILayout.Label ("Area data editor", EditorStyles.boldLabel);
        GUILayout.Space (8f);
        GUILayout.BeginHorizontal ("Box");
        GUILayout.Label (full ? fullLeader : terseLeader, EditorStyles.miniLabel);
        GUILayout.EndHorizontal ();
    }

    static void DrawExtraInstructions (ref DataMultiLinkerCombatAreaInspector.VolumeEditingMode volumeEditingMode)
    {
        DrawSpawnPointInstructions ();
        DrawWaypointInstructions ();
        DrawLocationInstructions ();
        DrawFieldInstructions ();
        DrawVolumeInstructions (ref volumeEditingMode);
    }

    static void DrawSpawnPointInstructions ()
    {
        var selected = DataMultiLinkerCombatArea.selectedSpawnPoint != null && DataMultiLinkerCombatArea.selectedSpawnGroup != null;
        if (!selected || !DataMultiLinkerCombatArea.presentation.showSpawns)
        {
            return;
        }

        GUILayout.Space (8f);
        GUILayout.BeginVertical ("Box");
        GUILayout.Label ("Spawn point");
        GUILayout.Label (string.Format ("{0} #{1}", DataMultiLinkerCombatArea.selectedSpawnGroup.key, DataMultiLinkerCombatArea.selectedSpawnPoint.index));
        #if PB_MODSDK
        if (!DataContainerModData.hasSelectedConfigs)
        {
            GUILayout.EndVertical ();
            return;
        }
        #endif
        GUILayout.Space (8f);
        GUILayout.BeginHorizontal ("Box");
        GUILayout.Label ("Alt+LMB\nAlt+LMB\nAlt+LMB\nAlt+MMB\nAlt+Shift+MMB\nAlt+RMB", EditorStyles.miniLabel);
        GUILayout.Label ("Place new spawn point (white target circle)\nSelect highlighted spawn point\nDeselect group (selected + highlighted)\nGround highlighted spawn point\nSnap highlighted spawn point to grid\nRemove highlighted spawn point", EditorStyles.miniLabel);
        GUILayout.EndHorizontal ();
        GUILayout.EndVertical ();
    }

    static void DrawWaypointInstructions ()
    {
        var selected = DataMultiLinkerCombatArea.selectedWaypoint != null && DataMultiLinkerCombatArea.selectedWaypointGroup != null;
        if (!selected || !DataMultiLinkerCombatArea.presentation.showWaypoints)
        {
            return;
        }

        GUILayout.Space (8f);
        GUILayout.BeginVertical ("Box");
        GUILayout.Label ("Waypoint");
        GUILayout.Label (string.Format ("{0} #{1}", DataMultiLinkerCombatArea.selectedWaypointGroup.key, DataMultiLinkerCombatArea.selectedWaypoint.index));
        #if PB_MODSDK
        if (!DataContainerModData.hasSelectedConfigs)
        {
            GUILayout.EndVertical ();
            return;
        }
        #endif
        if (DataMultiLinkerCombatArea.selectedSpawnGroup == null)
        {
            GUILayout.Space (8f);
            GUILayout.BeginHorizontal ("Box");
            GUILayout.Label ("Alt+LMB\nAlt+LMB\nAlt+LMB\nAlt+MMB\nAlt+Shift+MMB\nAlt+RMB", EditorStyles.miniLabel);
            GUILayout.Label ("Place new waypoint (white target circle)\nSelect highlighted waypoint\nDeselect group (selected + highlighted)\nGround highlighted waypoint\nSnap highlighted waypoint to grid\nRemove highlighted waypoint", EditorStyles.miniLabel);
            GUILayout.EndHorizontal ();
        }
        GUILayout.EndVertical ();
    }

    static void DrawLocationInstructions ()
    {
        if (DataMultiLinkerCombatArea.selectedLocation == null || !DataMultiLinkerCombatArea.presentation.showLocations)
        {
            return;
        }

        GUILayout.Space (8f);
        GUILayout.BeginVertical ("Box");
        GUILayout.Label ("Location");
        GUILayout.Label (DataMultiLinkerCombatArea.selectedLocation.key);
        #if PB_MODSDK
        if (!DataContainerModData.hasSelectedConfigs)
        {
            GUILayout.EndVertical ();
            return;
        }
        #endif
        if (DataMultiLinkerCombatArea.selectedSpawnGroup == null
            && DataMultiLinkerCombatArea.selectedWaypointGroup == null)
        {
            GUILayout.Space (8f);
            GUILayout.BeginHorizontal ("Box");
            GUILayout.Label ("Alt+LMB\nAlt+LMB\nAlt+LMB\nAlt+MMB\nAlt+Shift+MMB\nAlt+RMB", EditorStyles.miniLabel);
            GUILayout.Label ("Place new location (white target circle)\nSelect highlighted location\nDeselect location (selected + highlighted)\nGround highlighted location\nSnap highlighted location to grid\nRemove highlighted location", EditorStyles.miniLabel);
            GUILayout.EndHorizontal ();
        }
        GUILayout.EndVertical ();
    }

    static void DrawFieldInstructions ()
    {
        if (DataMultiLinkerCombatArea.selectedField == null || !DataMultiLinkerCombatArea.presentation.showFields)
        {
            return;
        }

        GUILayout.Space (8f);
        GUILayout.BeginVertical ("Box");
        GUILayout.Label ("Field");
        var index = DataMultiLinkerCombatArea.selectedArea.fields.IndexOf(DataMultiLinkerCombatArea.selectedField);
        GUILayout.Label ("F" + index + " (" + (string.IsNullOrEmpty (DataMultiLinkerCombatArea.selectedField.type) ? "?" : DataMultiLinkerCombatArea.selectedField.type) + ")");
        #if PB_MODSDK
        if (!DataContainerModData.hasSelectedConfigs)
        {
            GUILayout.EndVertical ();
            return;
        }
        #endif
        if (DataMultiLinkerCombatArea.selectedSpawnGroup == null
            && DataMultiLinkerCombatArea.selectedWaypointGroup == null
            && DataMultiLinkerCombatArea.selectedLocation == null)
        {
            GUILayout.Space (8f);
            GUILayout.BeginHorizontal ("Box");
            GUILayout.Label ("Alt+LMB", EditorStyles.miniLabel);
            GUILayout.Label ("Place new field", EditorStyles.miniLabel);
            GUILayout.EndHorizontal ();
        }
        GUILayout.EndVertical ();
    }

    static void DrawVolumeInstructions (ref DataMultiLinkerCombatAreaInspector.VolumeEditingMode volumeEditingMode)
    {
        if (DataMultiLinkerCombatArea.selectedVolume == null || !DataMultiLinkerCombatArea.presentation.showVolumes)
        {
            return;
        }

        GUILayout.Space (8f);
        GUILayout.BeginVertical ("Box");
        GUILayout.Label ("Volume");
        GUILayout.Label (DataMultiLinkerCombatArea.selectedVolume.key);

        #if PB_MODSDK
        if (!DataContainerModData.hasSelectedConfigs)
        {
            GUILayout.EndVertical ();
            return;
        }
        #endif

        if (DataMultiLinkerCombatArea.selectedSpawnGroup == null
            && DataMultiLinkerCombatArea.selectedWaypointGroup == null
            && DataMultiLinkerCombatArea.selectedLocation == null
            && DataMultiLinkerCombatArea.selectedField == null)
        {
            var modeText = string.Empty;
            switch (volumeEditingMode)
            {
                case DataMultiLinkerCombatAreaInspector.VolumeEditingMode.PositionY:
                    modeText = "Reposition volume on Y";
                    break;
                case DataMultiLinkerCombatAreaInspector.VolumeEditingMode.SizeXLength:
                    modeText = "Resize volume on X (length)";
                    break;
                case DataMultiLinkerCombatAreaInspector.VolumeEditingMode.SizeZWidth:
                    modeText = "Resize volume on Z (width)";
                    break;
                case DataMultiLinkerCombatAreaInspector.VolumeEditingMode.SizeYHeight:
                    modeText = "Resize volume on Y (height)";
                    break;
            }

            GUILayout.Space (8f);
            GUILayout.BeginHorizontal ("Box");
            GUILayout.Label ("Alt+LMB\nAlt+Scroll\nAlt+Shift+Scroll", EditorStyles.miniLabel);
            GUILayout.Label ($"Reposition volume on XZ\n{modeText}\nSwitch editing mode", EditorStyles.miniLabel);
            GUILayout.EndHorizontal ();
        }
        GUILayout.EndVertical ();
    }

    const string terseLeader = "- Choose area from dropdown list in inspector.\n- Click \"Select\" button in inspector to see the level in the viewport.";
    const string fullLeader = terseLeader + "\n- Click on a spawn point, location or volume in the scene to select it.\n- Select and edit spawn points, locations or volumes in inspector.\n- Toggle visibility of annotations in View options.";
}

#if PB_MODSDK
sealed class TitlePanelDrawer
{
    public void MeasureTitlePanel (DataContainerCombatArea area)
    {
        var levelKey = area.key;
        var titleStyle = SirenixGUIStyles.WhiteLabelCentered;
        var titleSize = titleStyle.CalcSize (GUIHelper.TempContent (levelKey));
        var width = titleSize.x;
        var height = titleSize.y + 8f;
        if (DataContainerModData.hasSelectedConfigs)
        {
            var modID = DataContainerModData.selectedMod.id;
            var modIDSize = titleStyle.CalcSize (GUIHelper.TempContent (modID));
            width = Mathf.Max (width, modIDSize.x);
            height += modIDSize.y;
        }
        var buttonSize = SirenixGUIStyles.Button.CalcSize (GUIHelper.TempContent (openLevelButtonText));
        width = Mathf.Max (width, buttonSize.x + 8f);
        height += buttonSize.y + 4f;
        var size = new Vector2 (width, height);
        var rect = new Rect (new Vector2 ((Screen.width - titleSize.x - 8f) / 2f, 4f), size);
        titlePanelRect = rect.Expand (4f, 0f);
        titlePanelScreenRect = titlePanelRect.Expand (10f, 10f, 4f, 10f);
    }

    public void DrawTitlePanel (DataContainerCombatArea area)
    {
        GUILayout.BeginArea (titlePanelRect);
        var rect = titlePanelRect.Expand (-4f, 0f);
        GUILayout.BeginVertical (EditorStyles.helpBox, GUILayoutOptions.Width (rect.width));
        var titleStyle = SirenixGUIStyles.WhiteLabelCentered;
        if (DataContainerModData.hasSelectedConfigs)
        {
            var c = GUI.contentColor;
            GUI.contentColor = DataContainerModData.colorSelected;
            var modID = DataContainerModData.selectedMod.id;
            GUILayout.Label (modID, titleStyle);
            GUI.contentColor = c;
        }
        var levelKey = area.key;
        GUILayout.Label (levelKey, titleStyle);
        var openLevelEditor = GUILayout.Button (openLevelButtonText, SirenixGUIStyles.Button);
        GUILayout.EndVertical ();
        GUILayout.EndArea ();

        if (openLevelEditor)
        {
            area.OpenLevelEditor ();
        }
    }

    public bool OverlapsUI (Vector3 worldPoint)
    {
        var guiPoint = HandleUtility.WorldToGUIPoint (worldPoint);
        return titlePanelScreenRect.Contains (guiPoint);
    }

    Rect titlePanelRect;
    Rect titlePanelScreenRect;

    const string openLevelButtonText = "Open Level Editor";
}

sealed class BirdsEyeView
{
    public void HandleInput (DataContainerCombatArea area, Event e)
    {
        if (!HasSelected (area))
        {
            Reset ();
            return;
        }
        if (!e.OnKeyUp(KeyCode.B))
        {
            return;
        }
        if (e.shift)
        {
            ReturnToLastView ();
            return;
        }
        BirdsEye (area);
    }

    void Reset ()
    {
        switchedToBirdsEye = false;
        lastAreaKey = "";
    }

    void ReturnToLastView ()
    {
        if (!switchedToBirdsEye || string.IsNullOrEmpty (lastAreaKey))
        {
            return;
        }

        Reset ();
        var sv = SceneView.lastActiveSceneView;
        sv.LookAt (lastSceneViewPivot, lastSceneViewRotation, lastSceneViewSize);
    }

    void BirdsEye (DataContainerCombatArea area)
    {
        var sv = SceneView.lastActiveSceneView;
        var areaBounds = CombatSceneHelper.ins.areaManager.boundsFull;
        var centerPoint = new Vector3 (areaBounds.x * TilesetUtility.blockAssetSize / 2f, 0f, areaBounds.z * TilesetUtility.blockAssetSize / 2f);
        var vert = Mathf.Max (centerPoint.x * 1.5f, centerPoint.z * 1.5f);
        var size = new Vector3 (areaBounds.x, vert, areaBounds.z);
        var bounds = new Bounds (centerPoint, size);
        lastSceneViewPivot = sv.pivot;
        lastSceneViewSize = sv.size;
        lastSceneViewRotation = sv.rotation;
        sv.Frame (bounds);
        var rot = Quaternion.LookRotation (Vector3.down, Vector3.forward);
        sv.LookAt (centerPoint, rot);
        switchedToBirdsEye = true;
        lastAreaKey = area.key;
    }

    static bool HasSelected (DataContainerCombatArea area) => area != null
        && CombatSceneHelper.ins != null
        && CombatSceneHelper.ins.areaManager != null
        && CombatSceneHelper.ins.areaManager.areaName == area.key;

    bool switchedToBirdsEye;
    Vector3 lastSceneViewPivot;
    float lastSceneViewSize;
    Quaternion lastSceneViewRotation;
    string lastAreaKey;
}
#endif
