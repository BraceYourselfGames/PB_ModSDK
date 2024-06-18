using PhantomBrigade;
using PhantomBrigade.Data;
using UnityEngine;
using UnityEditor;
using Sirenix.OdinInspector.Editor;
using Area;

[CustomEditor(typeof(DataMultiLinkerUnitComposite))]
public class DataMultiLinkerUnitCompositeInspector : OdinEditor
{
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

    private Vector3 boundsDefault = new Vector3 (100, 10, 100);

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI ();
        // You can also call base.OnInspectorGUI(); instead if you simply want to prepend or append GUI to the editor.
    }

    public void OnSceneGUI ()
    {
        if (DataMultiLinkerUnitComposite.selectedDirector == null)
            return;

        // Disable clicking on scene objects
        HandleUtility.AddDefaultControl (GUIUtility.GetControlID (FocusType.Passive));

        var director = DataMultiLinkerUnitComposite.selectedDirector;
        var selectedNavOption = DataMultiLinkerUnitComposite.selectedNavOption;
        
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
        }

        // if (shift || alt)
        //     e.Use ();

        var worldRay = HandleUtility.GUIPointToWorldRay (e.mousePosition);
        bool inputCheckSuccessful = buttonPressed == KeyCode.Mouse0 && e.type != EventType.Used;

        var boundsScaled = boundsDefault * TilesetUtility.blockAssetSize;
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

        if (selectedNavOption != null && selectedNavOption.points != null && selectedNavOption.points.Count >= 2)
        {
            var colorFull = colorMain;
            var colorSemi = colorFull.WithAlpha (0.25f);

            var points = selectedNavOption.points;
            for (int i = 0; i < points.Count; ++i)
            {
                var point = points[i];
                var position = point.point;

                Handles.color = colorFull;

                Handles.DrawLine (position, position + Vector3.up * 5f);
                Handles.Label (position + Vector3.up * 7f, $"{i}\n");

                Handles.CircleHandleCap (0, position, circleRotation, 3f, EventType.Repaint);

                Handles.color = colorSemi;
                if (i > 0)
                    Handles.DrawLine (position, points[i - 1].point);
                else
                    Handles.DrawLine (position, points[points.Count - 1].point);

                var size = HandleUtility.GetHandleSize (position);
                if (Handles.Button (position, Quaternion.identity, size * 0.05f, size * 0.2f, Handles.DotHandleCap))
                {
                    DataMultiLinkerUnitComposite.selectedNavPoint = point;
                    Repaint ();
                }

                if (DataMultiLinkerUnitComposite.selectedNavPoint == point)
                {
                    var groundingRayOrigin = position + Vector3.up * 100f;
                    var groundingRay = new Ray (groundingRayOrigin, Vector3.down);
                    var groundingCast = Physics.Raycast (groundingRay, out var groundingHit, 200f, LayerMasks.environmentMask);
                    if (groundingCast && groundingHit.distance > 0f)
                    {
                        Handles.color = colorFriendly;
                        Handles.DrawLine (position, groundingHit.point);
                    }
                    
                    EditorGUI.BeginChangeCheck ();
                    position = Handles.DoPositionHandle (position, Quaternion.identity);
                    if (EditorGUI.EndChangeCheck ())
                    {
                        point.point = position;

                        if (alt && groundingCast)
                            point.point = groundingHit.point;

                        if (shift)
                            point.point = AreaUtility.SnapPointToGrid (point.point);
                    }
                }
            }
        }
    }
}
