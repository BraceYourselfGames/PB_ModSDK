using PhantomBrigade.Data;
using UnityEngine;
using UnityEditor;
using Sirenix.OdinInspector.Editor;

[CustomEditor(typeof(DataLinkerSettingsAnimation))]
public class DataLinkerSettingsAnimationInspector : OdinEditor
{
    private Color colorUnused = Color.gray;
    private Color colorMain = Color.white;
    private Color colorEnemy = Color.Lerp (Color.red, Color.white, 0.5f);
    private Color colorFriendly = Color.Lerp (Color.cyan, Color.white, 0.5f);

    private Color colorTrajectoryTo = Color.HSVToRGB (0.65f, 0.5f, 1f).WithAlpha (0.85f);
    private Color colorTrajectoryMid = Color.HSVToRGB (0.9f, 0.4f, 1f).WithAlpha (0.85f);
    private Color colorTrajectoryFrom = Color.HSVToRGB (0f, 0.3f, 1f).WithAlpha (0.85f);
    private Color colorTrajectoryCollision = Color.HSVToRGB (0.3f, 0.3f, 1f).WithAlpha (0.85f);
    private Color colorTrajectoryAnim = Color.HSVToRGB (0f, 0f, 1f).WithAlpha (0.85f);
    private Color colorTrajectoryHint = Color.HSVToRGB (0f, 0f, 1f).WithAlpha (0.5f);
    
    private int selectedIndex = -1;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI ();
        // You can also call base.OnInspectorGUI(); instead if you simply want to prepend or append GUI to the editor.
    }

    public void OnSceneGUI ()
    {
        // Disable clicking on scene objects
        HandleUtility.AddDefaultControl (GUIUtility.GetControlID (FocusType.Passive));

        var circleRotation = Quaternion.Euler (90f, 0f, 0f);

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

        if (DataBlockTransformSequence.selection != null)
        {
            var s = DataBlockTransformSequence.selection;
            if (s.snapshots != null && s.snapshots.Count > 2)
            {
                var offset = new Vector3 (150f, 0f, 150f);
            
                if (selectedIndex != -1 && !selectedIndex.IsValidIndex (s.snapshots))
                    selectedIndex = -1;

                for (int i = s.snapshots.Count - 1; i >= 1; --i)
                {
                    var snapshot = s.snapshots[i];
                    var snapshotPrev = s.snapshots[i - 1];
                    var selected = selectedIndex == i;

                    var snapshotPosition = snapshot.position + offset;
                    var snapshotPositionPrev = snapshotPrev.position + offset;
                    
                    Handles.color = colorFriendly;
                    Handles.DrawLine (snapshotPosition, snapshotPositionPrev);

                    Handles.color = colorMain;
                    Handles.DrawLine (snapshotPosition, snapshot.position + offset + snapshot.rotation * Vector3.forward);

                    var ringSize = selected ? 6f : 3f;
                    Handles.color = selected ? colorFriendly : colorMain;
                
                    var size = HandleUtility.GetHandleSize (snapshotPosition);
                    if (Handles.Button (snapshotPosition, Quaternion.identity, size * 0.05f, size * 0.2f, Handles.DotHandleCap))
                    {
                        Debug.Log ($"Point clicked with BP: {buttonPressed} | {e.type} | Shift: {e.shift} | Alt: {e.alt}");
                        if (e.shift)
                           s.snapshots.RemoveAt (i);
                        else
                            selectedIndex = i;

                        Repaint ();
                    }
                    
                    if (selected)
                    {
                        var posMid = Vector3.Lerp (snapshotPosition, snapshotPositionPrev, 0.5f);
                        if (Handles.Button (posMid, Quaternion.identity, size * 0.05f, size * 0.2f, Handles.DotHandleCap))
                        {
                            Debug.Log ($"Midpoint clicked with BP: {buttonPressed} | {e.type} | Shift: {e.shift} | Alt: {e.alt}");
                            if (e.shift)
                            {
                                s.snapshots.Insert (selectedIndex, new DataBlockTransformSnapshot
                                {
                                    time = Mathf.Lerp (snapshotPrev.time, snapshot.time, 0.5f),
                                    position = Vector3.Lerp (snapshotPrev.position, snapshot.position, 0.5f),
                                    rotation = Quaternion.Lerp (snapshotPrev.rotation, snapshot.rotation, 0.5f),
                                });
                                
                                break;
                            }
                        }
                        
                        EditorGUI.BeginChangeCheck ();
                        snapshotPosition = Handles.DoPositionHandle (snapshotPosition, Quaternion.identity);
                        if (EditorGUI.EndChangeCheck ())
                        {
                            // Debug.Log ($"Adjusted position of point {i} to {position} | Shift: {e.shift} | Alt: {e.alt}");
                            snapshot.position = snapshotPosition - offset;
                            Repaint ();
                        }
                    }
                }
            }
        }
    }
}
