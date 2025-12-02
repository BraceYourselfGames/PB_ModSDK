using System.Collections.Generic;
using PhantomBrigade;
using PhantomBrigade.Data;
using UnityEngine;
using UnityEditor;
using Sirenix.OdinInspector.Editor;
using Area;
using UnityEngine.Rendering;

[CustomEditor(typeof(DataMultiLinkerCombatArea))]
public class DataMultiLinkerCombatAreaInspector : OdinEditor
{
    public enum VolumeEditingMode
    {
        PositionY = 0,
        SizeXLength = 1,
        SizeZWidth = 2,
        SizeYHeight = 3
    }
    
    private VolumeEditingMode volumeEditingMode = VolumeEditingMode.PositionY;

    private Color colorUnusedWaypoint = new Color (0.5f, 0.55f, 0.6f);
    private Color colorMainWaypoint = new Color (0.7f, 0.8f, 1f);
    
    private Color colorUnused = Color.gray;
    private Color colorMain = Color.white;
    private Color colorEnemy = Color.Lerp (Color.red, Color.white, 0.5f);
    private Color colorFriendly = Color.Lerp (Color.cyan, Color.white, 0.5f);
    private Color colorEmpty = Color.yellow;
    
    private Color colorMainLocation = Color.Lerp (Color.cyan, Color.white, 0.5f);
    private Color colorUnusedLocation = Color.Lerp (Color.cyan, Color.gray, 0.5f);

    private static Vector3[] transferPreviewVerts = new Vector3[4];
    private Color colorNeighbour = Color.red;
    private Color colorSpawn = Color.red;
    private Color colorLoot = Color.green;
    private Color colorRetreat = Color.blue;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI ();
        // You can also call base.OnInspectorGUI(); instead if you simply want to prepend or append GUI to the editor.
    }

    public void OnSceneGUI ()
    {
        if (DataMultiLinkerCombatArea.selectedArea == null)
            return;

        // Disable clicking on scene objects
        HandleUtility.AddDefaultControl (GUIUtility.GetControlID (FocusType.Passive));

        var area = DataMultiLinkerCombatArea.selectedArea;
        var circleRotation = Quaternion.Euler (90f, 0f, 0f);

        var e = Event.current;
        // Killing some bad editor hotkeys
        if (Event.current.type == EventType.ExecuteCommand)
        {
            switch (Event.current.commandName)
            {
                case "Copy":
                    Event.current.Use ();
                    break;
                case "Cut":
                    Event.current.Use ();
                    break;
                case "Paste":
                    Event.current.Use ();
                    break;
                case "Delete":
                    Event.current.Use ();
                    break;
                case "FrameSelected":
                    Event.current.Use ();
                    break;
                case "Duplicate":
                    Event.current.Use ();
                    break;
                case "SelectAll":
                    Event.current.Use ();
                    break;
                default:
                    // Lets show any other commands that may come through
                    // Debug.Log (Event.current.commandName);
                    break;
            }
        }
        
        bool eventPresent = e.type == EventType.MouseDown;
        bool alt = e.alt;
        bool shift = e.shift;

        var buttonPressed = KeyCode.None;
        if (eventPresent)
        {
            if (e.button == 0)
            {
                buttonPressed = KeyCode.Mouse0; 
                // e.Use ();
            }
            
            /*
            if (e.button == 1)
            {
                buttonPressed = KeyCode.Mouse1;
                e.Use ();
            }

            if (e.button == 2)
            {
                buttonPressed = KeyCode.Mouse2;
                e.Use ();
            }
            */
        }
        
        // if (shift || alt)
        //     e.Use ();
        
        var worldRay = HandleUtility.GUIPointToWorldRay (e.mousePosition);
        bool inputCheckSuccessful = buttonPressed == KeyCode.Mouse0 && e.type != EventType.Used;
        
        var am = CombatSceneHelper.ins.areaManager;
        var boundsFull = am.boundsFull;
        if (am == null || am.boundsFull == Vector3Int.size0x0x0)
            return;

        var boundsScaled = am.boundsFull * TilesetUtility.blockAssetSize;
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

        if (area.spawnGroups != null && DataMultiLinkerCombatArea.Presentation.showSpawns)
        {
            foreach (var kvp in area.spawnGroups)
            {
                var spawnGroupKey = kvp.Key;
                var spawnGroup = kvp.Value;
                
                if (spawnGroup == null || spawnGroup.points == null)
                    continue;

                bool selectedGroup = spawnGroup == DataMultiLinkerCombatArea.selectedSpawnGroup;
                var colorFull = selectedGroup ? colorMain : colorUnused;
                var colorSemi = colorFull.WithAlpha (0.25f);

                bool errorInAny = false;
                for (int i = 0; i < spawnGroup.points.Count; ++i)
                {
                    var spawnPoint = spawnGroup.points[i];
                    var position = spawnPoint.point;
                    var rotation = Quaternion.Euler (spawnPoint.rotation);
                    var forward = rotation * Vector3.forward;
                    var right = rotation * Vector3.right;

                    bool error = false;
                    if (DataMultiLinkerCombatArea.Presentation.showSpawnRaycasts)
                    {
                        var groundingRayOrigin = position + Vector3.up;
                        var groundingRay = new Ray (groundingRayOrigin, Vector3.down);
                        bool hit = Physics.Raycast (groundingRay, out var groundingHit, 100f, LayerMasks.environmentMask);
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
                        Handles.Label (position + Vector3.up * 7f, $"{i}\n");
                    }

                    Handles.color = error ? colorEnemy : colorFull;
                    Handles.CircleHandleCap (0, position, circleRotation, 3f, EventType.Repaint);
                    Handles.DrawLine (position + forward * 3f, position + forward * 6f);

                    // Handles.DrawLine (position + forward, position - forward);
                    // Handles.DrawLine (position + right, position - right);

                    Handles.color = colorSemi;
                    if (i > 0)
                        Handles.DrawLine (position, spawnGroup.points[i - 1].point);

                    var size = HandleUtility.GetHandleSize (position);
                    if (Handles.Button (position, rotation, size * 0.05f, size * 0.2f, Handles.DotHandleCap))
                    {
                        DataMultiLinkerCombatArea.selectedLocation = null;
                        DataMultiLinkerCombatArea.selectedField = null;
                        DataMultiLinkerCombatArea.selectedVolume = null;
                        DataMultiLinkerCombatArea.selectedWaypointGroup = null;
                        DataMultiLinkerCombatArea.selectedWaypoint = null;
                        DataMultiLinkerCombatArea.selectedSpawnGroup = spawnGroup;
                        DataMultiLinkerCombatArea.selectedSpawnPoint = spawnPoint;
                        Repaint ();
                    }

                    if (DataMultiLinkerCombatArea.selectedSpawnPoint == spawnPoint)
                    {
                        EditorGUI.BeginChangeCheck ();
                        position = Handles.DoPositionHandle (position, rotation);
                        if (EditorGUI.EndChangeCheck ())
                        {
                            spawnPoint.point = position;

                            if (alt)
                            {
                                var groundingRayOrigin2 = position + Vector3.up * 100f;
                                var groundingRay2 = new Ray (groundingRayOrigin2, Vector3.down);

                                if (Physics.Raycast (groundingRay2, out var groundingHit2, 200f, LayerMasks.environmentMask))
                                    spawnPoint.point = groundingHit2.point;
                            }

                            if (shift)
                                spawnPoint.SnapToGrid ();
                            
                            spawnGroup.RefreshAveragePosition ();
                        }

                        EditorGUI.BeginChangeCheck ();
                        rotation = Handles.DoRotationHandle (rotation, position);
                        if (EditorGUI.EndChangeCheck ())
                            spawnPoint.rotation = new Vector3 (0f, rotation.eulerAngles.y, 0f);
                    }
                }
                
                Handles.color = errorInAny ? colorEnemy : colorUnused;
                Handles.Label (spawnGroup.averagePosition + Vector3.up * 7f, spawnGroupKey.Replace ("perimeter_", "p_"));
            }
        }

        if (DataMultiLinkerCombatArea.selectedSpawnGroup != null && DataMultiLinkerCombatArea.Presentation.showSpawns)
        {
            if (alt && Physics.Raycast (worldRay, out var hitInfo, 1000, LayerMasks.environmentMask))
            {
                Handles.color = colorMain;
                Handles.DrawLine (hitInfo.point, hitInfo.point + Vector3.up * 5f);

                var pos = hitInfo.point;
                bool posValid = true;

                var points = DataMultiLinkerCombatArea.selectedSpawnGroup.points != null ? DataMultiLinkerCombatArea.selectedSpawnGroup.points : null;

                if (points != null)
                {
                    foreach (var pointExisting in points)
                    {
                        var sqrMagnitude = (pos - pointExisting.point).sqrMagnitude;
                        if (sqrMagnitude < 36)
                        {
                            posValid = false;
                            break;
                        }
                    }
                }

                Handles.color = posValid ? colorMain : colorEnemy;
                Handles.CircleHandleCap (0, hitInfo.point, circleRotation, 3f, EventType.Repaint);

                if (inputCheckSuccessful && posValid)
                {
                    if (points == null)
                    {
                        DataMultiLinkerCombatArea.selectedSpawnGroup.points = new List<DataBlockAreaPoint> ();
                        points = DataMultiLinkerCombatArea.selectedSpawnGroup.points;
                    }

                    var rot = points.Count > 0 ? points[points.Count - 1].rotation : Vector3.zero;
                    var point = new DataBlockAreaPoint { point = pos, rotation = rot };
                    point.SnapToGrid ();
                    
                    points.Add (point);
                    Repaint ();
                }
            }
        }
        
        if (area.waypointGroups != null && DataMultiLinkerCombatArea.Presentation.showWaypoints)
        {
            foreach (var kvp in area.waypointGroups)
            {
                var waypointGroupKey = kvp.Key;
                var waypointGroup = kvp.Value;
                
                if (waypointGroup == null || waypointGroup.points == null || waypointGroup.points.Count == 0)
                    continue;

                bool selectedGroup = waypointGroup == DataMultiLinkerCombatArea.selectedWaypointGroup;
                var colorFull = selectedGroup ? colorMainWaypoint : colorUnusedWaypoint;
                var colorSemi = colorFull.WithAlpha (0.25f);

                bool errorInAny = false;
                for (int i = 0; i < waypointGroup.points.Count; ++i)
                {
                    var waypoint = waypointGroup.points[i];
                    var position = waypoint.point;
                    var rotation = Quaternion.Euler (waypoint.rotation);
                    var forward = rotation * Vector3.forward;
                    var right = rotation * Vector3.right;

                    bool error = false;
                    if (DataMultiLinkerCombatArea.Presentation.showSpawnRaycasts)
                    {
                        var groundingRayOrigin = position + Vector3.up;
                        var groundingRay = new Ray (groundingRayOrigin, Vector3.down);
                        bool hit = Physics.Raycast (groundingRay, out var groundingHit, 100f, LayerMasks.environmentMask);
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
                        Handles.Label (position + Vector3.up * 7f, $"{i}\n");
                    }

                    Handles.color = error ? colorEnemy : colorFull;
                    Handles.CircleHandleCap (0, position, circleRotation, 3f, EventType.Repaint);
                    Handles.DrawLine (position + forward * 3f, position + forward * 6f);

                    // Handles.DrawLine (position + forward, position - forward);
                    // Handles.DrawLine (position + right, position - right);

                    Handles.color = colorSemi;
                    if (i > 0)
                        Handles.DrawLine (position, waypointGroup.points[i - 1].point);

                    var size = HandleUtility.GetHandleSize (position);
                    if (Handles.Button (position, rotation, size * 0.05f, size * 0.2f, Handles.DotHandleCap))
                    {
                        DataMultiLinkerCombatArea.selectedLocation = null;
                        DataMultiLinkerCombatArea.selectedField = null;
                        DataMultiLinkerCombatArea.selectedVolume = null;
                        DataMultiLinkerCombatArea.selectedSpawnGroup = null;
                        DataMultiLinkerCombatArea.selectedSpawnPoint = null;
                        DataMultiLinkerCombatArea.selectedWaypointGroup = waypointGroup;
                        DataMultiLinkerCombatArea.selectedWaypoint = waypoint;
                        Repaint ();
                    }

                    if (DataMultiLinkerCombatArea.selectedWaypoint == waypoint)
                    {
                        EditorGUI.BeginChangeCheck ();
                        position = Handles.DoPositionHandle (position, rotation);
                        if (EditorGUI.EndChangeCheck ())
                        {
                            waypoint.point = position;

                            if (alt)
                            {
                                var groundingRayOrigin2 = position + Vector3.up * 100f;
                                var groundingRay2 = new Ray (groundingRayOrigin2, Vector3.down);

                                if (Physics.Raycast (groundingRay2, out var groundingHit2, 200f, LayerMasks.environmentMask))
                                    waypoint.point = groundingHit2.point;
                            }

                            if (shift)
                                waypoint.SnapToGrid ();
                        }

                        EditorGUI.BeginChangeCheck ();
                        rotation = Handles.DoRotationHandle (rotation, position);
                        if (EditorGUI.EndChangeCheck ())
                            waypoint.rotation = new Vector3 (0f, rotation.eulerAngles.y, 0f);
                    }
                }
                
                Handles.color = errorInAny ? colorEnemy : colorUnused;
                Handles.Label (waypointGroup.points[waypointGroup.points.Count - 1].point + Vector3.up * 7f, waypointGroupKey.Replace ("perimeter_", "p_"));
            }
        }

        if (DataMultiLinkerCombatArea.selectedWaypointGroup != null && DataMultiLinkerCombatArea.Presentation.showWaypoints)
        {
            if (alt && Physics.Raycast (worldRay, out var hitInfo, 1000, LayerMasks.environmentMask))
            {
                Handles.color = colorMain;
                Handles.DrawLine (hitInfo.point, hitInfo.point + Vector3.up * 5f);

                var pos = hitInfo.point;
                bool posValid = true;

                var points = DataMultiLinkerCombatArea.selectedWaypointGroup.points != null ? DataMultiLinkerCombatArea.selectedWaypointGroup.points : null;

                if (points != null)
                {
                    foreach (var pointExisting in points)
                    {
                        var sqrMagnitude = (pos - pointExisting.point).sqrMagnitude;
                        if (sqrMagnitude < 36)
                        {
                            posValid = false;
                            break;
                        }
                    }
                }

                Handles.color = posValid ? colorMain : colorEnemy;
                Handles.CircleHandleCap (0, hitInfo.point, circleRotation, 3f, EventType.Repaint);

                if (inputCheckSuccessful && posValid)
                {
                    if (points == null)
                    {
                        DataMultiLinkerCombatArea.selectedWaypointGroup.points = new List<DataBlockAreaPoint> ();
                        points = DataMultiLinkerCombatArea.selectedWaypointGroup.points;
                    }

                    var rot = points.Count > 0 ? points[points.Count - 1].rotation : Vector3.zero;
                    var point = new DataBlockAreaPoint { point = pos, rotation = rot };
                    point.SnapToGrid ();
                    
                    points.Add (point);
                    Repaint ();
                }
            }
        }
        
        if (area.locations != null && DataMultiLinkerCombatArea.Presentation.showLocations)
        {
            foreach (var kvp in area.locations)
            {
                var locationKey = kvp.Key;
                var location = kvp.Value;
                
                if (location == null)
                    continue;
                
                if (location.data == null)
                    location.data = new DataBlockAreaLocation ();

                var ld = location.data;
                bool selected = location == DataMultiLinkerCombatArea.selectedLocation;
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
                    
                    Handles.CircleHandleCap (0, position, circleRotation, radius, EventType.Repaint);
                    Handles.DrawLine (position + dir * radius, position + dir * (radius + 3));
                }

                Handles.color = colorSemi;
                var size = HandleUtility.GetHandleSize (position);
                
                if (Handles.Button (position, Quaternion.identity, size * 0.05f, size * 0.2f, Handles.DotHandleCap))
                {
                    DataMultiLinkerCombatArea.selectedLocation = location;
                    DataMultiLinkerCombatArea.selectedField = null;
                    DataMultiLinkerCombatArea.selectedVolume = null;
                    DataMultiLinkerCombatArea.selectedWaypointGroup = null;
                    DataMultiLinkerCombatArea.selectedWaypoint = null;
                    DataMultiLinkerCombatArea.selectedSpawnGroup = null;
                    DataMultiLinkerCombatArea.selectedSpawnPoint = null;
                    Repaint ();
                }

                if (selected)
                {
                    EditorGUI.BeginChangeCheck ();
                    position = Handles.DoPositionHandle (position, rotationQ);
                    if (EditorGUI.EndChangeCheck ())
                    {
                        location.data.point = position;

                        if (alt)
                        {
                            var groundingRayOrigin = position + Vector3.up * 100f;
                            var groundingRay = new Ray (groundingRayOrigin, Vector3.down);

                            if (Physics.Raycast (groundingRay, out var groundingHit, 200f, LayerMasks.environmentMask))
                                location.data.point = groundingHit.point;
                        }

                        if (shift)
                            location.SnapToGrid ();
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
            
            if (DataMultiLinkerCombatArea.selectedLocation != null && DataMultiLinkerCombatArea.selectedSpawnPoint == null && DataMultiLinkerCombatArea.Presentation.showLocations)
            {
                if (alt && Physics.Raycast (worldRay, out var hitInfoLocation, 1000, LayerMasks.environmentMask))
                {
                    Handles.color = colorMainLocation;
                    Handles.DrawLine (hitInfoLocation.point, hitInfoLocation.point + Vector3.up * 5f);

                    var pos = hitInfoLocation.point;
                    bool posValid = true;

                    foreach (var kvp in area.locations)
                    {
                        var locationOther = kvp.Value;
                        var sqrMagnitude = (pos - locationOther.data.point).sqrMagnitude;
                        if (sqrMagnitude < 36)
                        {
                            posValid = false;
                            break;
                        }
                    }

                    Handles.color = posValid ? colorMain : colorEnemy;
                    Handles.CircleHandleCap (0, hitInfoLocation.point, circleRotation, 3f, EventType.Repaint);

                    if (inputCheckSuccessful && posValid)
                    {
                        int loops = 0;
                        int locationKeyIndex = 0;
                        string key = null;
                        
                        while (true)
                        {
                            key = $"location_{locationKeyIndex}";
                            if (!area.locations.ContainsKey (key))
                                break;
                            
                            locationKeyIndex += 1;
                            loops += 1;

                            if (loops > 100)
                            {
                                Debug.LogWarning ($"Failed to find any key for location");
                                break;
                            }
                        }
                        
                        if (!string.IsNullOrEmpty (key) && !area.locations.ContainsKey (key))
                        {
                            var location = new DataBlockAreaLocationTagged () { data = new DataBlockAreaLocation { point = pos } };
                            location.SnapToGrid ();

                            area.locations.Add (key, location);
                            Repaint ();
                        }
                    }
                }
            }
        }
        
        if (area.fields != null && DataMultiLinkerCombatArea.Presentation.showFields)
        {
            var sourceColorMain = Color.HSVToRGB (0.58f, 0.5f, 0.8f).WithAlpha (0.25f);
            var sourceColorCulled = Color.HSVToRGB (0.65f, 0.4f, 0.6f).WithAlpha (0.025f);
            
            for (int i = 0; i < area.fields.Count; ++i)
            {
                // var fieldKey = kvp.Key;
                var field = area.fields[i];
                
                if (field == null)
                    continue;

                var type = !string.IsNullOrEmpty (field.type) ? field.type : "?";
                bool selected = field == DataMultiLinkerCombatArea.selectedField;
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
                
                if (Handles.Button (position, Quaternion.identity, size * 0.05f, size * 0.2f, Handles.DotHandleCap))
                {
                    DataMultiLinkerCombatArea.selectedLocation = null;
                    DataMultiLinkerCombatArea.selectedField = field;
                    DataMultiLinkerCombatArea.selectedVolume = null;
                    DataMultiLinkerCombatArea.selectedWaypointGroup = null;
                    DataMultiLinkerCombatArea.selectedWaypoint = null;
                    DataMultiLinkerCombatArea.selectedSpawnGroup = null;
                    DataMultiLinkerCombatArea.selectedSpawnPoint = null;
                    Repaint ();
                }

                if (selected)
                {
                    EditorGUI.BeginChangeCheck ();
                    position = Handles.DoPositionHandle (position, rotationQ);
                    if (EditorGUI.EndChangeCheck ())
                    {
                        field.origin = position;
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
                        field.rotation = rotationNew;
                    }
                }
            }
            
            if (DataMultiLinkerCombatArea.selectedLocation != null && DataMultiLinkerCombatArea.selectedSpawnPoint == null && DataMultiLinkerCombatArea.Presentation.showLocations)
            {
                if (alt && Physics.Raycast (worldRay, out var hitInfoLocation, 1000, LayerMasks.environmentMask))
                {
                    Handles.color = colorMainLocation;
                    Handles.DrawLine (hitInfoLocation.point, hitInfoLocation.point + Vector3.up * 5f);

                    var pos = hitInfoLocation.point;
                    bool posValid = true;

                    foreach (var kvp in area.locations)
                    {
                        var locationOther = kvp.Value;
                        var sqrMagnitude = (pos - locationOther.data.point).sqrMagnitude;
                        if (sqrMagnitude < 36)
                        {
                            posValid = false;
                            break;
                        }
                    }

                    Handles.color = posValid ? colorMain : colorEnemy;
                    Handles.CircleHandleCap (0, hitInfoLocation.point, circleRotation, 3f, EventType.Repaint);

                    if (inputCheckSuccessful && posValid)
                    {
                        int loops = 0;
                        int locationKeyIndex = 0;
                        string key = null;
                        
                        while (true)
                        {
                            key = $"location_{locationKeyIndex}";
                            if (!area.locations.ContainsKey (key))
                                break;
                            
                            locationKeyIndex += 1;
                            loops += 1;

                            if (loops > 100)
                            {
                                Debug.LogWarning ($"Failed to find any key for location");
                                break;
                            }
                        }
                        
                        if (!string.IsNullOrEmpty (key) && !area.locations.ContainsKey (key))
                        {
                            var location = new DataBlockAreaLocationTagged () { data = new DataBlockAreaLocation { point = pos } };
                            location.SnapToGrid ();

                            area.locations.Add (key, location);
                            Repaint ();
                        }
                    }
                }
            }
        }

        if (area.volumes != null && DataMultiLinkerCombatArea.Presentation.showVolumes)
        {
            var points = am.points;
            int pointsTotal = am.points.Count;
            
            cubeDescs.Clear ();
            
            foreach (var kvp in area.volumes)
            {
                var volumeKey = kvp.Key;
                var volume = kvp.Value;

                if (volume == null)
                    continue;

                if (volume.data == null)
                    volume.data = new DataBlockAreaVolume ();

                var vd = volume.data;
                var internalPositionA = vd.origin;
                var internalPositionB = vd.origin + vd.size - Vector3Int.size1x1x1;

                // Get Position
                int sourceCornerAIndex = AreaUtility.GetIndexFromInternalPosition (internalPositionA, boundsFull);
                int sourceCornerBIndex = AreaUtility.GetIndexFromInternalPosition (internalPositionB, boundsFull);
                
                var sourcePosA = new Vector3 (internalPositionA.x, -internalPositionA.y, internalPositionA.z) * TilesetUtility.blockAssetSize;
                var sourcePosB = new Vector3 (internalPositionB.x, -internalPositionB.y, internalPositionB.z) * TilesetUtility.blockAssetSize;

                sourcePosA += new Vector3 (-1f, 1f, -1f) * (TilesetUtility.blockAssetSize * 0.51f);
                sourcePosB += new Vector3 (1f, -1f, 1f) * (TilesetUtility.blockAssetSize * 0.51f);

                var sourceColorMain = new HSBColor (0.0f, 0.5f, 0.8f, 0.15f).ToColor();
                var sourceColorCulled = new HSBColor (0.0f, 0.4f, 0.5f, 0.075f).ToColor ();
                
                if (sourceCornerAIndex != -1 && sourceCornerBIndex != -1)
                {
                    sourceColorMain = new HSBColor (0.25f, 0.5f, 0.8f, 0.15f).ToColor();
                    sourceColorCulled = new HSBColor (0.1f, 0.4f, 0.5f, 0.075f).ToColor ();
                }

                DrawVolume (volumeKey, sourcePosA, sourcePosB, sourceColorMain, sourceColorCulled);
                
                var size = HandleUtility.GetHandleSize (sourcePosA);
                if (Handles.Button (sourcePosA, Quaternion.identity, size * 0.05f, size * 0.2f, Handles.DotHandleCap))
                {
                    DataMultiLinkerCombatArea.selectedLocation = null;
                    DataMultiLinkerCombatArea.selectedField = null;
                    DataMultiLinkerCombatArea.selectedVolume = volume;
                    DataMultiLinkerCombatArea.selectedWaypointGroup = null;
                    DataMultiLinkerCombatArea.selectedWaypoint = null;
                    DataMultiLinkerCombatArea.selectedSpawnGroup = null;
                    DataMultiLinkerCombatArea.selectedSpawnPoint = null;
                    Repaint ();
                }
            }

            if (DataMultiLinkerCombatArea.selectedVolume != null)
            {
                var vd = DataMultiLinkerCombatArea.selectedVolume.data;
            
                if (alt)
                {
                    if (Physics.Raycast (worldRay, out var hitInfo, 1000, LayerMasks.environmentMask))
                    {
                        Handles.color = Color.white.WithAlpha (1f);
                        Handles.DrawLine (hitInfo.point, hitInfo.point + hitInfo.normal * 3f);
                        Handles.CubeHandleCap (0, hitInfo.point, Quaternion.identity, 0.5f, EventType.Repaint);
                        
                        var hitPositionShiftedDeeper = hitInfo.point - hitInfo.normal * 0.5f;
                        int index = AreaUtility.GetIndexFromWorldPosition (hitPositionShiftedDeeper, am.GetHolderColliders ().position, am.boundsFull);
                        if (index != -1 && index.IsValidIndex (am.points))
                        {
                            var point = am.points[index];
                            if (point != null)
                            {
                                Handles.color = Color.white.WithAlpha (0.25f);
                                Handles.CubeHandleCap (0, point.pointPositionLocal, Quaternion.identity, 2.8f, EventType.Repaint);

                                var originAdjusted = new Vector3Int (point.pointPositionIndex.x, vd.origin.y, point.pointPositionIndex.z);
                                var internalPositionA = originAdjusted;
                                var internalPositionB = originAdjusted + vd.size - Vector3Int.size1x1x1;

                                // Get Position
                                int sourceCornerAIndex = AreaUtility.GetIndexFromInternalPosition (internalPositionA, boundsFull);
                                int sourceCornerBIndex = AreaUtility.GetIndexFromInternalPosition (internalPositionB, boundsFull);
                
                                var sourcePosA = new Vector3 (internalPositionA.x, -internalPositionA.y, internalPositionA.z) * TilesetUtility.blockAssetSize;
                                var sourcePosB = new Vector3 (internalPositionB.x, -internalPositionB.y, internalPositionB.z) * TilesetUtility.blockAssetSize;

                                sourcePosA += new Vector3 (-1f, 1f, -1f) * (TilesetUtility.blockAssetSize * 0.51f);
                                sourcePosB += new Vector3 (1f, -1f, 1f) * (TilesetUtility.blockAssetSize * 0.51f);
                                
                                var internalPositionA1 = vd.origin;
                                var internalPositionB1 = vd.origin + vd.size - Vector3Int.size1x1x1;
                                
                                var cOrigin = new Vector3 (internalPositionA1.x, -internalPositionA1.y, internalPositionA1.z) * TilesetUtility.blockAssetSize;
                                var cExtent = new Vector3 (internalPositionB1.x, -internalPositionB1.y, internalPositionB1.z) * TilesetUtility.blockAssetSize;

                                cOrigin += new Vector3 (-1f, 1f, -1f) * (TilesetUtility.blockAssetSize * 0.51f);
                                cExtent += new Vector3 (1f, -1f, 1f) * (TilesetUtility.blockAssetSize * 0.51f);

                                var sourceColorMain = new HSBColor (0.0f, 0.5f, 0.8f, 0.15f).ToColor();
                                var sourceColorCulled = new HSBColor (0.0f, 0.4f, 0.5f, 0.075f).ToColor ();
                
                                if (sourceCornerAIndex != -1 && sourceCornerBIndex != -1)
                                {
                                    sourceColorMain = new HSBColor (0.5f, 0.5f, 0.8f, 0.15f).ToColor();
                                    sourceColorCulled = new HSBColor (0.5f, 0.4f, 0.5f, 0.075f).ToColor ();
                                }

                                DrawVolume ("New origin", sourcePosA, sourcePosB, sourceColorMain, sourceColorCulled);
                                
                                if (eventPresent && buttonPressed == KeyCode.Mouse0)
                                {
                                    vd.origin = originAdjusted;
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
                        bool forward = e.delta.y > 0f;
                        buttonPressed = shift ? (forward ? KeyCode.LeftBracket : KeyCode.RightBracket) : (forward ? KeyCode.PageDown : KeyCode.PageUp);

                        if (shift)
                        {
                            int editIndexCurrent = (int)volumeEditingMode;
                            int editIndexNew = editIndexCurrent.OffsetAndWrap (forward, 3);
                            var editModeNew = (VolumeEditingMode)editIndexNew;
                            volumeEditingMode = editModeNew;
                        }
                        else
                        {
                            if (volumeEditingMode == VolumeEditingMode.PositionY)
                            {
                                vd.origin.y += forward ? 1 : -1;
                                vd.ValidateOrigin ();
                            }
                            else if (volumeEditingMode == VolumeEditingMode.SizeXLength)
                            {
                                vd.size.x += forward ? -1 : 1;
                                vd.ValidateSize ();
                            }
                            else if (volumeEditingMode == VolumeEditingMode.SizeZWidth)
                            {
                                vd.size.z += forward ? -1 : 1;
                                vd.ValidateSize ();
                            }
                            else if (volumeEditingMode == VolumeEditingMode.SizeYHeight)
                            {
                                vd.size.y += forward ? -1 : 1;
                                vd.ValidateSize ();
                            }
                        }
                    }
                }

                Vector3Int boundsLocal = vd.size;
                int volumeLengthLocal = boundsLocal.x * boundsLocal.y * boundsLocal.z;
                
                for (int i = volumeLengthLocal - 1; i >= 0; --i)
                {
                    Vector3Int internalPositionLocal = AreaUtility.GetVolumePositionFromIndex (i, boundsLocal);
                    Vector3Int internalPositionFull = internalPositionLocal + vd.origin;
                    int sourcePointIndex = AreaUtility.GetIndexFromInternalPosition (internalPositionFull, boundsFull);

                    // Skip if index is invalid
                    if (sourcePointIndex < 0 || sourcePointIndex >= pointsTotal)
                        continue;

                    // Fetch the point, skip if it's empty
                    var sourcePoint = points[sourcePointIndex];
                    if (sourcePoint.pointState == AreaVolumePointState.Empty)
                        continue;

                    // Skip all points that designers specified as indestructible,
                    // or points that are indestructible due to factors like height or tileset
                    if (!sourcePoint.destructible || sourcePoint.indestructibleIndirect)
                        continue;

                    var gradientInterpolant = (float)internalPositionLocal.y / vd.size.y;

                    // Increment count of destroyed points
                    
                    Color colorPoint;
                    if (sourcePoint.pointState == AreaVolumePointState.FullDestroyed)
                        colorPoint = Color.HSVToRGB (Mathf.Lerp (0.05f, 0f, gradientInterpolant), 0.5f, 1f);
                    else
                        colorPoint = Color.HSVToRGB (Mathf.Lerp (0.2f, 0.25f, gradientInterpolant), 0.5f, 1f);
                    
                    cubeDescs.Add (new CubeDesc { origin = sourcePoint.pointPositionLocal, size = 1.4f, color = colorPoint });
                }
                
                DrawCubeListSorted (cubeDescs);
            }
        }

        if (DataMultiLinkerCombatArea.Presentation.showSpawnGeneration)
        {
            
        }
        
        if (DataMultiLinkerCombatArea.selectedVolume != null && DataMultiLinkerCombatArea.Presentation.showVolumes)
        {
            Handles.BeginGUI ();

            GUILayout.BeginVertical (EditorStyles.helpBox, GUILayout.Width (250f));
            GUILayout.Label ("Area data editor", EditorStyles.boldLabel);

            GUILayout.Space (8f);
            GUILayout.Label ("- Use \"Load Area\" in inspector to see the level in the viewport.\n- Select spawn group, location or volume in inspector to edit it in scene.\n- Set visibility of each channel from inspector.", EditorStyles.miniLabel);
            GUILayout.Space (8f);

            string modeText = string.Empty;
            if (volumeEditingMode == VolumeEditingMode.PositionY)
                modeText = "Reposition volume on Y";
            else if (volumeEditingMode == VolumeEditingMode.SizeXLength)
                modeText = "Resize volume on X (length)";
            else if (volumeEditingMode == VolumeEditingMode.SizeZWidth)
                modeText = "Resize volume on Z (width)";
            else if (volumeEditingMode == VolumeEditingMode.SizeYHeight)
                modeText = "Resize volume on Y (height)";

            GUILayout.BeginHorizontal ();
            GUILayout.Label ("Alt+LMB\nAlt+Scroll\nAlt+Shift+Scroll", EditorStyles.miniLabel);
            GUILayout.Label ($"Reposition volume on XZ\n{modeText}\nSwitch editing mode", EditorStyles.miniLabel);
            GUILayout.EndHorizontal ();

            GUILayout.Space (8f);
            GUILayout.EndVertical ();

            Handles.EndGUI ();
        }

        if (GUI.changed)
        {
            EditorWindow view = EditorWindow.GetWindow<SceneView> ();
            view.Repaint ();
        }

        SceneView.RepaintAll ();
    }

    void OnEnable()
    {
        Tools.hidden = true;
    }
 
    void OnDisable()
    {
        Tools.hidden = false;
    }

    private List<CubeDesc> cubeDescs = new List<CubeDesc> ();

    public struct CubeDesc
    {
        public Vector3 origin;
        public float size;
        public Color color;
        public float dist;
    }
    
    private void DrawCubeListSorted (List<CubeDesc> list, bool lighting = true, CompareFunction zTest = CompareFunction.Always, bool fadeUsed = true)
    {
        if (list == null || list.Count == 0)
            return;
    
        var scene = UnityEditor.SceneView.lastActiveSceneView;
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
                var fade = Mathf.Lerp (1f, 0f, interpolant);
                Handles.color = cubeDesc.color.WithAlpha (fade);
            }
            else
            {
                Handles.color = cubeDesc.color;
            }
        

            Handles.CubeHandleCap (0, cubeDesc.origin, Quaternion.identity, cubeDesc.size, EventType.Repaint);
        }

        Handles.color = hc;
        Handles.lighting = l;
        Handles.zTest = zt;
    }
    
    private void DrawCube (Vector3 origin, float size, Color colorMain, bool lighting = true, CompareFunction zTest = CompareFunction.Always)
    {
        var hc = Handles.color;
        var l = Handles.lighting;
        var zt = Handles.zTest;

        Handles.zTest = zTest;
        Handles.lighting = lighting;
        Handles.color = colorMain;
        
        Handles.CubeHandleCap (0, origin, Quaternion.identity, size, EventType.Repaint);
        
        Handles.color = hc;
        Handles.lighting = l;
        Handles.zTest = zt;
    }

    private void DrawZCube (Vector3 origin, float size, Color colorMain, Color colorCulled)
    {
        var hc = Handles.color;
        var zt = Handles.zTest;

        Handles.zTest = CompareFunction.LessEqual;
        Handles.color = colorMain;
        Handles.CubeHandleCap (0, origin, Quaternion.identity, size, EventType.Repaint);

        Handles.zTest = CompareFunction.Greater;
        Handles.color = colorCulled;
        Handles.CubeHandleCap (0, origin, Quaternion.identity, size, EventType.Repaint);
        
        Handles.color = hc;
        Handles.zTest = zt;
    }

    private void DrawArrow (Vector3 posOrigin, Vector3 posDestination, Color color)
    {
        if (posOrigin == posDestination)
            return;
        
        Vector3 difference = posDestination - posOrigin;
        difference.y = -difference.y;
        Vector3 dir = difference.normalized;

        Handles.color = color;
        Handles.DrawLine (posOrigin, posDestination, 2f);
        
        var sizeDestination = HandleUtility.GetHandleSize (posOrigin);
        Handles.ConeHandleCap (0, posDestination, Quaternion.LookRotation (-dir), sizeDestination * 0.2f, Event.current.type);
    }
    
    private void DrawArrowBidirectional (Vector3 posOrigin, Vector3 posDestination, Color color, bool flip = false)
    {
        if (posOrigin == posDestination)
            return;
        
        Vector3 difference = posDestination - posOrigin;
        difference.y = -difference.y;
        Vector3 dir = difference.normalized;

        Handles.color = color;
        Handles.DrawLine (posOrigin, posDestination, 2f);
        
        var sizeDestination = HandleUtility.GetHandleSize (posOrigin);
        Handles.ConeHandleCap (0, posDestination, Quaternion.LookRotation (flip ? dir : -dir), sizeDestination * 0.2f, Event.current.type);
        
        var sizeOrigin = HandleUtility.GetHandleSize (posOrigin);
        Handles.ConeHandleCap (0, posOrigin, Quaternion.LookRotation (flip ? -dir : dir), sizeOrigin * 0.2f, Event.current.type);
    }

    private void DrawVolume(string key, Vector3 posA, Vector3 posB, Color colorMain, Color colorCulled)
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
        Handles.CubeHandleCap (0, posA, Quaternion.identity, 0.5f, EventType.Repaint);
        Handles.CubeHandleCap (1, posB, Quaternion.identity, 0.5f, EventType.Repaint);

        Handles.zTest = CompareFunction.Greater;
        Handles.color = colorCulled;
        Handles.CubeHandleCap (0, posA, Quaternion.identity, 0.5f, EventType.Repaint);
        Handles.CubeHandleCap (1, posB, Quaternion.identity, 0.5f, EventType.Repaint);

        Handles.color = Color.white.WithAlpha(1f);
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
    
    private void DrawField (Vector3 origin, Vector3 size, float rotation, Color colorMain, Color colorCulled)
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
        Handles.CubeHandleCap (0, origin, Quaternion.identity, 0.5f, EventType.Repaint);
        Handles.DrawLine (posTop1, posLow1);
        Handles.DrawLine (posTop2, posLow2);
        Handles.DrawLine (posTop3, posLow3);
        Handles.DrawLine (posTop4, posLow4);
        
        Handles.color = Color.white.WithAlpha (1f);
        Handles.DrawLine (posTop1, posTop2);
        Handles.DrawLine (posTop2, posTop3);
        Handles.DrawLine (posTop3, posTop4);
        Handles.DrawLine (posTop4, posTop1);

        Handles.zTest = CompareFunction.Greater;
        Handles.color = colorCulled;
        Handles.CubeHandleCap (0, origin, Quaternion.identity, 0.5f, EventType.Repaint);
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
}
