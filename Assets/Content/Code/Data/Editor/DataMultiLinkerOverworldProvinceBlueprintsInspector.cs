using System.Collections.Generic;
using PhantomBrigade;
using PhantomBrigade.Data;
using UnityEngine;
using UnityEditor;
using Sirenix.OdinInspector.Editor;

[CustomEditor(typeof(DataMultiLinkerOverworldProvinceBlueprints))]
public class DataMultiLinkerOverworldProvinceBlueprintsInspector : OdinEditor
{
    private Color colorUnused = Color.gray;
    private Color colorMain = Color.white;
    private Color colorEnemy = Color.Lerp (Color.red, Color.white, 0.5f);
    private Color colorFriendly = Color.Lerp (Color.cyan, Color.white, 0.5f);

    private Color colorLinkActors = Color.HSVToRGB (0.6f, 0.5f, 1f);
    private Color colorLinkConvoys = Color.HSVToRGB (0.25f, 0.5f, 0.75f);
    private Color colorLinkCounterAttacks = Color.HSVToRGB (0.1f, 0.5f, 1f);
    private Color colorLinkWar = Color.HSVToRGB (0f, 0.5f, 1f);
    
    private Color colorNoDecisiveObjectives = Color.HSVToRGB (0.25f, 0.5f, 1f);

    private Vector3 posLast = Vector3.zero;
    private int selectedBorderPoint = -1;
    private bool selectedBorderPointPresent = false;
    private int selectedPointOfInterest = -1;
    private bool pointMode = true;
    
    private string provinceKeyLast = string.Empty;

    private bool snapRequested = false;
    private bool snapFound = false;
    private Vector3 snapPosLast = Vector3.zero;
    


    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI ();
        // You can also call base.OnInspectorGUI(); instead if you simply want to prepend or append GUI to the editor.
    }

    public void OnSceneGUI ()
    {
        // Disable clicking on scene objects
        HandleUtility.AddDefaultControl (GUIUtility.GetControlID (FocusType.Passive));

        var provinces = DataMultiLinkerOverworldProvinceBlueprints.data;
        var selected = DataMultiLinkerOverworldProvinceBlueprints.selection;
        var circleRotation = Quaternion.Euler (90f, 0f, 0f);
        
        bool showDecisiveObj = DataMultiLinkerOverworldProvinceBlueprints.Presentation.showObjectivesDecisive;
        bool showSpawns = DataMultiLinkerOverworldProvinceBlueprints.Presentation.showSpawns;
        bool showSpawnKeys = DataMultiLinkerOverworldProvinceBlueprints.Presentation.showSpawnKeys;
        
        var e = Event.current;
        bool eventPresent = e.type == EventType.MouseDown;
        // if (eventPresent)
        //     e.Use ();

        var buttonPressed = KeyCode.None;
        if (eventPresent)
        {
            if (e.button == 0)
                buttonPressed = KeyCode.Mouse0;
            if (e.button == 1)
                buttonPressed = KeyCode.Mouse1;
            if (e.button == 2)
                buttonPressed = KeyCode.Mouse2;
        }

        // selectedPointProximitySearch = e.alt && selected != null && selectedPoint.IsValidIndex (selected.border) && !selectedPointSnapFound;
        var snapSearch = e.alt && selected != null && selectedBorderPoint.IsValidIndex (selected.border) && !snapFound && snapRequested;
        var selectedPointPos = Vector3.zero;
        if (snapSearch)
        {
            snapFound = false;
            selectedPointPos = selected.border[selectedBorderPoint];
            Debug.Log ($"{Time.frameCount} | Looking for snap candidate for point {selectedBorderPoint} that has position {selectedPointPos.ToStringDetailed ()}");
        }
        
        string spawnGroupHighlightedKey = DataMultiLinkerOverworldProvinceBlueprints.Presentation.spawnGroupHighlighted;
        bool spawnGroupHighlighted = !string.IsNullOrEmpty (spawnGroupHighlightedKey);

        foreach (var kvp in provinces)
        {
            var province = kvp.Value;
            bool isSelected = selected == province;
            
            Color colorProvinceGeneral = colorEnemy;
            if (province.used)
            {
                if (isSelected)
                    colorProvinceGeneral = colorMain;
                else
                {
                    colorProvinceGeneral = colorEnemy;
                    if (showDecisiveObj && (province.warObjectivesDecisiveRequired <= 0 || province.warObjectivesDecisive == null || province.warObjectivesDecisive.Count == 0))
                        colorProvinceGeneral = colorNoDecisiveObjectives;
                }
            }
            else
                colorProvinceGeneral = colorUnused;

            Handles.color = colorProvinceGeneral;
            
            var size = HandleUtility.GetHandleSize (province.position);
            if (Handles.Button (province.position, Quaternion.identity, size * 0.05f, size * 0.2f, Handles.DotHandleCap))
            {
                DataMultiLinkerOverworldProvinceBlueprints.selection = province;
                selected = province;
                isSelected = true;

                selectedBorderPoint = -1;
                selectedPointOfInterest = -1;
            }

            if (showSpawns && !isSelected && pointMode)
            {
                if (province.spawns != null && spawnGroupHighlighted && province.spawns.TryGetValue (spawnGroupHighlightedKey, out var spawnList))
                {
                    Handles.color = Color.HSVToRGB (0f, 0.1f, 1f);

                    for (int i = spawnList.Count - 1; i >= 0; --i)
                    {
                        var point = spawnList[i];
                        var pos = point.position;

                        Handles.DrawLine (pos, pos + Vector3.up * 25f);
                        Handles.CircleHandleCap (0, pos, circleRotation, 12f, EventType.Repaint);

                        var c = Handles.color;
                        Handles.color = Color.HSVToRGB (0f, 0.25f, 0.9f);
                        Handles.CircleHandleCap (0, pos, circleRotation, 14f, EventType.Repaint);

                        Handles.color = Color.HSVToRGB (0f, 0.35f, 0.8f);
                        Handles.CircleHandleCap (0, pos, circleRotation, 16f, EventType.Repaint);

                        Handles.color = c;
                    }
                }
            }

            if (isSelected && pointMode)
            {
                EditorGUI.BeginChangeCheck ();
                var positionOfProvince = Handles.DoPositionHandle (province.position, Quaternion.identity);
                if (EditorGUI.EndChangeCheck ())
                {
                    var groundingRayOrigin = new Vector3 (positionOfProvince.x, 200f, positionOfProvince.z);
                    var groundingRay = new Ray (groundingRayOrigin, Vector3.down);
                    if (Physics.Raycast (groundingRay, out var hit, 400f, LayerMasks.environmentMask))
                        positionOfProvince = hit.point;
                    
                    selected.position = positionOfProvince;
                    Repaint ();
                }

                int spawnGroupIndex = -1;
                int spawnGroupCount = province.spawns.Count;
                
                foreach (var kvp2 in province.spawns)
                {
                    spawnGroupIndex += 1;
                    var spawnGroupKey = kvp2.Key;
                    var spawnList = kvp2.Value;
                    var spawnGroupSelected = spawnGroupKey == province.spawnKeyEditable;
                    
                    Handles.color = Color.HSVToRGB ((float)spawnGroupIndex / spawnGroupCount, 0.25f, spawnGroupSelected ? 1f : 0.65f);

                    bool groupHighlighted = spawnGroupKey == spawnGroupHighlightedKey;
                    if (groupHighlighted)
                        Handles.color = Color.HSVToRGB (0f, 0.1f, 1f);
                    
                    for (int i = spawnList.Count - 1; i >= 0; --i)
                    {
                        var point = spawnList[i];
                        var pos = point.position;
                        bool pointIsSelected = i == selectedPointOfInterest;

                        Handles.DrawLine (pos, pos + Vector3.up * 25f);
                        Handles.CircleHandleCap (0, pos, circleRotation, 12f, EventType.Repaint);
                        
                        if (groupHighlighted)
                        {
                            var c = Handles.color;
                            Handles.color = Color.HSVToRGB (0f, 0.25f, 0.9f);
                            Handles.CircleHandleCap (0, pos, circleRotation, 14f, EventType.Repaint);
                            
                            Handles.color = Color.HSVToRGB (0f, 0.35f, 0.8f);
                            Handles.CircleHandleCap (0, pos, circleRotation, 16f, EventType.Repaint);

                            Handles.color = c;
                        }
                        
                        if (spawnGroupSelected)
                        {
                            Handles.Label (pos + Vector3.up * 30f, i == 0 && showSpawnKeys ? $"   {spawnGroupKey}\n   {i}" : $"   {i}");

                            if (i > 0)
                            {
                                var c = Handles.color;
                                Handles.color = colorUnused;
                                var pointPrev = spawnList[i - 1];
                                Handles.DrawLine (pos, pointPrev.position);
                                Handles.color = c;
                            }
                        }
                        
                        size = HandleUtility.GetHandleSize (pos);
                        if (Handles.Button (pos, Quaternion.identity, size * 0.05f, size * 0.2f, Handles.DotHandleCap))
                        {
                            if (spawnGroupSelected)
                            {
                                if (e.alt)
                                    spawnList.RemoveAt (i);
                                else
                                    selectedPointOfInterest = i;
                            }
                            else
                            {
                                province.spawnKeyEditable = spawnGroupKey;
                                selectedPointOfInterest = -1;
                            }
                            Repaint ();
                        }

                        if (spawnGroupSelected && pointIsSelected)
                        {
                            EditorGUI.BeginChangeCheck ();
                            var positionModified = Handles.DoPositionHandle (point.position, Quaternion.identity);
                            if (EditorGUI.EndChangeCheck ())
                            {
                                var groundingRayOrigin = new Vector3 (positionModified.x, 200f, positionModified.z);
                                var groundingRay = new Ray (groundingRayOrigin, Vector3.down);
                                if (Physics.Raycast (groundingRay, out var hit, 400f, LayerMasks.environmentMask))
                                    positionModified = hit.point;

                                // Debug.Log ($"Adjusted position of point {i} to {position} | Shift: {e.shift} | Alt: {e.alt}");
                                point.position = positionModified;
                                Repaint ();
                            }
                        }
                    }
                }
            }

            if (pointMode)
            {
                var hc2 = Handles.color;

                bool showLinksWar = DataMultiLinkerOverworldProvinceBlueprints.Presentation.showLinksWar;
                bool showLinksCounterAttacks = DataMultiLinkerOverworldProvinceBlueprints.Presentation.showLinksCounterAttacks;
                bool showLinksConvoy = DataMultiLinkerOverworldProvinceBlueprints.Presentation.showLinksConvoy;
                bool showLinksActorSearch = DataMultiLinkerOverworldProvinceBlueprints.Presentation.showLinksActorSearch;

                foreach (var kvp2 in province.neighbourData)
                {
                    var neighbourName = kvp2.Key;
                    if (string.IsNullOrEmpty (neighbourName) || !provinces.ContainsKey (neighbourName))
                        continue;

                    var link = kvp2.Value;
                    var neighbour = provinces[neighbourName];
                    var dir = province.position.GetDirection (neighbour.position);
                    var shift = Vector3.Cross (dir, Vector3.up);

                    if (showLinksWar && link.allowWar)
                    {
                        Handles.color = colorLinkWar;
                        Handles.DrawLine (province.position + dir * 25f + shift * 2f, neighbour.position - dir * 25f + shift * 2f);
                    }

                    if (showLinksCounterAttacks && link.allowCounterAttacks)
                    {
                        Handles.color = colorLinkCounterAttacks;
                        Handles.DrawLine (province.position + dir * 24.5f + shift * 4f, neighbour.position - dir * 24.5f + shift * 4f);
                    }

                    if (showLinksConvoy && link.allowConvoys)
                    {
                        Handles.color = colorLinkConvoys;
                        Handles.DrawLine (province.position + dir * 24f + shift * 6f, neighbour.position - dir * 24f + shift * 6f);
                    }

                    if (showLinksActorSearch && link.allowActorSearch)
                    {
                        Handles.color = colorLinkActors;
                        Handles.DrawLine (province.position + dir * 23.5f + shift * 8f, neighbour.position - dir * 23.5f + shift * 8f);
                    }
                }

                Handles.color = hc2;
            }



            for (int i = province.border.Count - 1; i >= 0; --i)
            {
                var position = province.border[i];
                var positionPrev = i > 0 ? province.border[i - 1] : province.border[province.border.Count - 1];

                if (isSelected)
                    Handles.color = i == selectedBorderPoint ? colorFriendly : colorMain;
                else
                    Handles.color = colorProvinceGeneral;
                Handles.DrawLine (position, positionPrev);
                Handles.DrawLine (position, position + Vector3.up);

                if (snapSearch && province != selected)
                {
                    var sqrDistance = (position - selectedPointPos).sqrMagnitude;
                    if (sqrDistance < 16f)
                    {
                        Debug.Log ($"{Time.frameCount} | Found a close snap position to point {selectedBorderPoint} currently at {selectedPointPos.ToStringDetailed ()}: new position would be {position.ToStringDetailed ()} | Sqr.: {sqrDistance}");
                        snapSearch = false;
                        snapFound = true;
                        selected.border[selectedBorderPoint] = position;
                        snapPosLast = position;
                    }
                }
            }
            
            var text = $"      {province.key}";
            if (showDecisiveObj)
            {
                var tag = province.GetFirstDecisiveObjectiveDesc ();
                if (!string.IsNullOrEmpty (tag))
                    text = $"   ▲ {tag}\n{text}";
                else
                    text = $"   ---\n{text}";
            }

            Handles.color = colorProvinceGeneral;
            Handles.DrawLine (province.position, province.position + Vector3.up * 25f);
            Handles.Label (province.position + Vector3.up * 30f, text);
            Handles.CircleHandleCap (0, province.position, circleRotation, 25f, EventType.Repaint);
        }

        if (snapSearch && !snapFound)
        {
            Debug.Log ($"{Time.frameCount} | Failed to locate snap candidate");
            snapRequested = false;
        }

        if (selected != null)
        {
            if (selected.border == null)
                selected.border = new List<Vector3> ();
            
            if (selectedBorderPoint != -1 && !selectedBorderPoint.IsValidIndex (selected.border))
                selectedBorderPoint = -1;

            if (!pointMode)
            {
                for (int i = selected.border.Count - 1; i >= 0; --i)
                {
                    var position = selected.border[i];
                    var positionPrev = i > 0 ? selected.border[i - 1] : selected.border[selected.border.Count - 1];
                    var pointIsSelected = selectedBorderPoint == i;

                    var ringSize = pointIsSelected ? 6f : 3f;
                    Handles.color = pointIsSelected ? colorFriendly : colorMain;

                    Handles.DrawLine (position, position + Vector3.up * 25f);
                    Handles.Label (position + Vector3.up * 30f, $"   {i}");
                    Handles.CircleHandleCap (0, position, circleRotation, ringSize, EventType.Repaint);

                    var size = HandleUtility.GetHandleSize (position);
                    if (Handles.Button (position, Quaternion.identity, size * 0.05f, size * 0.2f, Handles.DotHandleCap))
                    {
                        Debug.Log ($"Point clicked with BP: {buttonPressed} | {e.type} | Shift: {e.shift} | Alt: {e.alt}");
                        if (e.shift)
                            selected.border.RemoveAt (i);
                        else
                            selectedBorderPoint = i;

                        Repaint ();
                        snapFound = false;
                        snapRequested = false;
                    }

                    if (pointIsSelected && !snapFound)
                    {
                        var posMid = Vector3.Lerp (position, positionPrev, 0.5f);
                        if (Handles.Button (posMid, Quaternion.identity, size * 0.05f, size * 0.2f, Handles.DotHandleCap))
                        {
                            Debug.Log ($"Midpoint clicked with BP: {buttonPressed} | {e.type} | Shift: {e.shift} | Alt: {e.alt}");
                            if (e.shift)
                            {
                                var groundingRayOrigin = new Vector3 (posMid.x, 200f, posMid.z);
                                var groundingRay = new Ray (groundingRayOrigin, Vector3.down);
                                if (Physics.Raycast (groundingRay, out var hit, 400f, LayerMasks.environmentMask))
                                    posMid = hit.point;
                                
                                selected.border.Insert (selectedBorderPoint, posMid);
                                break;
                            }
                        }
                        
                        EditorGUI.BeginChangeCheck ();
                        position = Handles.DoPositionHandle (position, Quaternion.identity);
                        if (EditorGUI.EndChangeCheck ())
                        {
                            if (e.alt)
                            {
                                snapRequested = true;

                                /*
                                if (snapFound)
                                {
                                    Debug.Log ($"{Time.frameCount} | Moving current point {selectedPoint} to position {selectedPointSnapPos.ToStringDetailed ()}");
                                    position = selectedPointSnapPos;
                                    selectedPointSnapFound = false;
                                }
                                else
                                {
                                    Debug.Log ($"{Time.frameCount} | Issuing request to snap current point {selectedPoint} currently at position {position}");
                                    selectedPointRequestsSnapPos = true;
                                    selectedPointPos = position;
                                }
                                */
                            }
                            else
                            {
                                var groundingRayOrigin = new Vector3 (position.x, 200f, position.z);
                                var groundingRay = new Ray (groundingRayOrigin, Vector3.down);
                                if (Physics.Raycast (groundingRay, out var hit, 400f, LayerMasks.environmentMask))
                                    position = hit.point;
                            }

                            // Debug.Log ($"Adjusted position of point {i} to {position} | Shift: {e.shift} | Alt: {e.alt}");
                            selected.border[i] = position;
                            Repaint ();
                        }
                    }
                }
            }
        }
        else
        {
            if (selectedBorderPoint != -1)
                selectedBorderPoint = -1;
        }
        
        var worldRay = HandleUtility.GUIPointToWorldRay (e.mousePosition);
        bool inputCheckSuccessful = buttonPressed == KeyCode.Mouse0 && e.type != EventType.Used;
        bool alt = e.alt;
        
        if (Physics.Raycast (worldRay, out var hitInfo, 1000, LayerMasks.environmentMask) && alt)
        {
            posLast = hitInfo.point;
            provinceKeyLast = DataHelperProvince.GetProvinceKeyAtPositionExpensive (hitInfo.point);
            
            Handles.color = colorMain;
            Handles.DrawLine (hitInfo.point, hitInfo.point + Vector3.up * 5f);
            
            if (pointMode)
            {
                var province = DataMultiLinkerOverworldProvinceBlueprints.GetEntry (provinceKeyLast);
                var point = hitInfo.point;

                bool addToSelected = !e.shift;
                List<DataBlockOverworldProvincePoint> pointsModified = null;
                if (e.shift)
                {
                    if (province.spawns == null)
                        province.spawns = new Dictionary<string, List<DataBlockOverworldProvincePoint>> ();
                    
                    if (province.spawns.TryGetValue (spawnGroupHighlightedKey, out var val))
                        pointsModified = val;
                    else
                    {
                        pointsModified = new List<DataBlockOverworldProvincePoint> ();
                        province.spawns.Add (spawnGroupHighlightedKey, pointsModified);
                    }
                }
                else
                {
                    if (province.spawns != null && province.spawns.TryGetValue (province.spawnKeyEditable, out var val))
                        pointsModified = val;
                }

                bool pointValid = pointsModified != null;
                if (pointValid)
                {
                    foreach (var pointExisting in pointsModified)
                    {
                        var sqrMagnitude = (point - pointExisting.position).sqrMagnitude;
                        if (sqrMagnitude < 900)
                        {
                            // Debug.LogWarning ($"Can't place a new point at {hitInfo.point}: too close to existing spawn");
                            pointValid = false;
                            break;
                        }
                    }
                }
                
                Handles.color = pointValid ? colorMain : colorEnemy;
                Handles.CircleHandleCap (0, hitInfo.point, circleRotation, 12f, EventType.Repaint);
                
                if (inputCheckSuccessful && pointValid)
                {
                    pointsModified.Add (new DataBlockOverworldProvincePoint { position = point });
                    Repaint ();
                }
            }
            else
            {
                if (inputCheckSuccessful)
                {
                    if (selected != null)
                    {
                        if (selectedBorderPoint == -1)
                        {
                            var province = DataMultiLinkerOverworldProvinceBlueprints.GetEntry (provinceKeyLast);
                            if (e.alt)
                            {
                                selected.border.Add (posLast);
                            }
                            else if (province != selected || province == null)
                            {
                                Debug.Log ($"Deselecting current province {selected.key}");
                                DataMultiLinkerOverworldProvinceBlueprints.selection = null;
                            }
                        }
                        else if (selectedBorderPoint.IsValidIndex (selected.border))
                        {
                            if (e.alt)
                            {
                                Debug.Log ($"Attempting to insert point after index {selectedBorderPoint}");
                                selected.border.Insert (selectedBorderPoint, posLast);
                            }
                            else
                            {
                                Debug.Log ($"Attempting to deselect point");
                                selectedBorderPoint = -1;
                            }
                        }
                    }
                    else
                    {
                        var province = DataMultiLinkerOverworldProvinceBlueprints.GetEntry (provinceKeyLast);
                        if (province != null)
                        {
                            Debug.Log ($"Selecting province {provinceKeyLast}");
                            DataMultiLinkerOverworldProvinceBlueprints.selection = province;
                        }
                    }
                }
            }
        }
        
        Handles.BeginGUI();

        GUILayout.BeginVertical (EditorStyles.helpBox, GUILayout.Width (200f));
        GUILayout.Label ("Province editor", EditorStyles.boldLabel);
        GUILayout.Space (8f);
        GUILayout.Label (provinceKeyLast, EditorStyles.miniLabel);
        GUILayout.Label (posLast.ToString (), EditorStyles.miniLabel);
        pointMode = EditorGUILayout.Toggle (pointMode ? "Point mode" : "Border mode", pointMode);
        GUILayout.BeginHorizontal ();
        GUILayout.Label ("LMB (point)\nAlt+LMB (point)\nLMB (empty)\nAlt+LMB (empty)", EditorStyles.miniLabel);
        GUILayout.Label ("Select point\nDelete point\nSelect province\nInsert point", EditorStyles.miniLabel);
        GUILayout.EndHorizontal ();
        GUILayout.Space (8f);
        GUILayout.EndVertical ();
        
        Handles.EndGUI ();
        
        if (GUI.changed)
        {
            EditorWindow view = EditorWindow.GetWindow<SceneView>();
            view.Repaint ();
        }
        
        SceneView.RepaintAll ();
    }
    
    protected override void OnEnable ()
    {
        Tools.hidden = true;
    }
    
    protected override void OnDisable ()
    {
        Tools.hidden = false;
    }

    /*
    private Vector3 ShowEditablePoint (int index)
    {
        Vector3 point = handleTransform.TransformPoint (spline.GetControlPoint (index));
        float size = HandleUtility.GetHandleSize (point);
        if (index == 0)
        {
            size *= 2f;
        }
        Handles.color = modeColors[(int)spline.GetControlPointMode (index)];
        if (Handles.Button (point, handleRotation, size * handleSize, size * pickSize, Handles.DotCap))
        {
            selectedIndex = index;
            Repaint ();
        }
        if (selectedIndex == index)
        {
            EditorGUI.BeginChangeCheck ();
            point = Handles.DoPositionHandle (point, handleRotation);
            if (EditorGUI.EndChangeCheck ())
            {
                Undo.RecordObject (spline, "Move Point");
                EditorUtility.SetDirty (spline);
                spline.SetControlPoint (index, handleTransform.InverseTransformPoint (point));
            }
        }
        return point;
    }
    */
}
