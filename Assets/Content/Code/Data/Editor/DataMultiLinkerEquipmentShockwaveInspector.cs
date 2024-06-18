using PhantomBrigade;
using PhantomBrigade.Data;
using UnityEngine;
using UnityEditor;
using Sirenix.OdinInspector.Editor;

[CustomEditor(typeof(DataMultiLinkerEquipmentShockwave))]
public class DataMultiLinkerEquipmentShockwaveInspector : OdinEditor
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
        
        
        if (DataContainerEquipmentShockwave.selection != null)
        {
            var s = DataContainerEquipmentShockwave.selection;
            if (s.points != null && s.points.Count > 2)
            {
                var rootPos = new Vector3 (150f, 0f, 150f);
                var rootRot = Quaternion.identity;
                var rootDistance = s.unitPositionTarget;

                var positionRootStart = rootPos;
                var positionRootEnd = rootPos + (rootRot * Vector3.forward) * rootDistance;
                var distance = Vector3.Distance (positionRootStart, positionRootEnd);
                var directionForward = (positionRootEnd - positionRootStart).normalized;
                
                var timeNormalized = Mathf.Clamp01 (s.time);
                
                var boxDepthHalf = s.hitboxDepth;
                var boxRoundingOffset = s.hitboxDepth * s.hitboxRounding;
                var boxRoundingOffsetHalf = boxRoundingOffset * 0.5f;
                
                var timeRemapCurve = s.unitTimeRemap ? DataShortcuts.anim.timeRemapMeleeStandard : null;
                bool timeRemapUsed = timeRemapCurve != null;
                var rotationRoot = Quaternion.identity;
                
                var progressUnit = timeNormalized;
                if (timeRemapUsed)
                    progressUnit = timeRemapCurve.Evaluate (progressUnit);
                var positionRoot = Vector3.Lerp (positionRootStart, positionRootEnd, progressUnit);
                
                Handles.color = Color.white.WithAlpha (0.5f);
                Handles.DrawLine (positionRoot, positionRoot + Vector3.up * 3f);
                Handles.CircleHandleCap (0, positionRoot, circleRotation, 3f, EventType.Repaint);
                
                Handles.color = Color.white.WithAlpha (0.3f);
                Handles.CircleHandleCap (0, positionRoot, circleRotation, 5f, EventType.Repaint);
                // Handles.Label (position + Vector3.up * 30f, $"   {i}");

                for (int i = s.points.Count - 1; i >= 1; --i)
                {
                    var point = s.points[i];
                    var pointPrev = s.points[i - 1];
                    var selected = point == DataBlockShockwavePoint.selection;
                    
                    bool timeEarly = timeNormalized < point.lifetime.x;
                    bool timeLate = timeNormalized > point.lifetime.y;
                    bool timeActive = !timeEarly && !timeLate;
                    var timeLocal = Mathf.Max (0f, timeNormalized - point.lifetime.x);
                    var timeInterval = point.lifetime.y - point.lifetime.x;

                    // Get base position for current point
                    var progressAtSpawn = point.lifetime.x;
                    if (timeRemapUsed)
                        progressAtSpawn = timeRemapCurve.Evaluate (progressAtSpawn);
                    
                    var positionRootAtSpawn = Vector3.Lerp (positionRootStart, positionRootEnd, progressAtSpawn);
                    var positionFrom = point.positionFrom.TransformLocalToWorld (positionRootAtSpawn, rotationRoot);

                    // Get offsets based on velocity and damping. To prevent backtracking from damping, clamp time based on calculated peak time.
                    var offsetForward = UtilityMath.GetDampedTravelDistance (point.velocityForward + s.velocityForward, s.dampingForward, timeLocal);
                    var offsetRadial = UtilityMath.GetDampedTravelDistance (point.velocityRadial + s.velocityRadial, s.dampingRadial, timeLocal);
                    
                    // Construct animated position of current point from the two offsets
                    var directionRadial = point.positionFrom.normalized;
                    var positionAnim = positionFrom + directionForward * offsetForward + directionRadial * offsetRadial;
                    
                    // Get base position for previous point
                    var timeLocalPrev = Mathf.Max (0f, timeNormalized - pointPrev.lifetime.x);
                    var progressAtSpawnPrev = pointPrev.lifetime.x;
                    if (timeRemapUsed)
                        progressAtSpawnPrev = timeRemapCurve.Evaluate (progressAtSpawnPrev);
                    
                    var positionRootAtSpawnPrev = Vector3.Lerp (positionRootStart, positionRootEnd, progressAtSpawnPrev);
                    var positionFromPrev = pointPrev.positionFrom.TransformLocalToWorld (positionRootAtSpawnPrev, rotationRoot);
                    
                    // Repeat offset calculations
                    var offsetForwardPrev = UtilityMath.GetDampedTravelDistance (pointPrev.velocityForward + s.velocityForward, s.dampingForward, timeLocalPrev); 
                    var offsetRadialPrev = UtilityMath.GetDampedTravelDistance (pointPrev.velocityRadial + s.velocityRadial, s.dampingRadial, timeLocalPrev);
                    
                    // Construct animated position of current point from the two offsets
                    var directionRadialPrev = pointPrev.positionFrom.normalized;
                    var positionAnimPrev = positionFromPrev + directionForward * offsetForwardPrev + directionRadialPrev * offsetRadialPrev;

                    Handles.color = colorTrajectoryFrom;
                    Handles.DrawLine (positionFrom, positionFromPrev);

                    Handles.color = colorTrajectoryAnim;
                    Handles.DrawLine (positionAnim, positionAnimPrev);
                    
                    Handles.color = selected ? colorTrajectoryAnim : colorTrajectoryHint;
                    var size = HandleUtility.GetHandleSize (positionFrom);
                    if (Handles.Button (positionFrom, Quaternion.identity, size * 0.05f, size * 0.2f, Handles.DotHandleCap))
                    {
                        Debug.Log ($"Point clicked with BP: {buttonPressed} | {e.type} | Shift: {e.shift} | Alt: {e.alt}");
                        if (e.shift)
                        {
                            s.points.RemoveAt (i);
                            DataBlockShockwavePoint.selection = null;
                        }
                        else
                        {
                            if (DataBlockShockwavePoint.selection == point)
                                DataBlockShockwavePoint.selection = null;
                            else
                                DataBlockShockwavePoint.selection = point;
                        }

                        Repaint ();
                    }
                    
                    if (selected)
                    {
                        Handles.color = colorTrajectoryHint;
                        var rotationImplicit = Quaternion.LookRotation (directionRadial);

                        EditorGUI.BeginChangeCheck ();
                        positionFrom = Handles.DoPositionHandle (positionFrom, rotationImplicit);
                        if (EditorGUI.EndChangeCheck ())
                        {
                            // Debug.Log ($"Adjusted position of point {i} to {position} | Shift: {e.shift} | Alt: {e.alt}");
                            point.positionFrom = positionFrom - rootPos - positionRootAtSpawn;
                            Repaint ();
                        }
                    }
                    
                    var segmentCenter = (positionAnim + positionAnimPrev) * 0.5f;
                    var segmentDelta = (positionAnim - positionAnimPrev);
                    var segmentDirectionForward = segmentDelta.normalized;
                    var segmentLength = segmentDelta.magnitude;

                    var axisPitch = Vector3.Cross (segmentDirectionForward, Vector3.up).normalized;
                    var segmentDirectionSide = s.extendRight ? axisPitch : -axisPitch;

                    if (timeActive)
                    {
                        var timeFactorIn = 1f;
                        var timeFactorOut = 1f;
                        var timeFactorCombined = 1f;

                        if (s.hitboxScaleTime > 0f)
                        {
                            timeFactorIn = Mathf.Clamp01 (1f - timeLocal / s.hitboxScaleTime);
                            timeFactorOut = Mathf.Clamp01 (1f + (timeLocal - timeInterval + Mathf.Clamp01 (s.hitboxFadeoutTime)) / s.hitboxScaleTime);
                            timeFactorCombined = 1f - Mathf.Clamp01 (timeFactorIn + timeFactorOut);
                        }

                        if (timeFactorCombined > 0f)
                        {
                            var boxLength = segmentLength + boxRoundingOffset * timeFactorCombined;
                            var boxLengthHalf = boxLength * 0.5f;
                            var boxDepthHalfAnim = boxDepthHalf * timeFactorCombined * s.hitboxCollisionScale;
                            
                            var boxOrigin = segmentCenter;
                            boxOrigin = Vector3.Lerp (boxOrigin, positionAnimPrev - segmentDirectionForward * boxRoundingOffsetHalf, timeFactorIn);
                            boxOrigin = Vector3.Lerp (boxOrigin, positionAnim + segmentDirectionForward * boxRoundingOffsetHalf, timeFactorOut);
                            
                            if (!s.hitboxCollisionScale.RoughlyEqual (1f))
                            {
                                var boxDepthHalfUnscaled = boxDepthHalf * timeFactorCombined;
                                var boxLengthHalfUnscaled = segmentLength * timeFactorCombined * 0.5f;
                                
                                var cornerOutForward1 = boxOrigin + segmentDirectionForward * boxLengthHalfUnscaled + segmentDirectionSide * boxDepthHalfUnscaled;
                                var cornerOutBack1 = boxOrigin - segmentDirectionForward * boxLengthHalfUnscaled + segmentDirectionSide * boxDepthHalfUnscaled;
                                var cornerInForward1 = boxOrigin + segmentDirectionForward * boxLengthHalfUnscaled - segmentDirectionSide * boxDepthHalfUnscaled;
                                var cornerInBack1 = boxOrigin - segmentDirectionForward * boxLengthHalfUnscaled - segmentDirectionSide * boxDepthHalfUnscaled;

                                Handles.DrawLine (cornerOutForward1, cornerOutBack1);
                                Handles.DrawLine (cornerInForward1, cornerInBack1);
                                // Handles.DrawLine (cornerInForward1, cornerOutForward1);
                                // Handles.DrawLine (cornerInBack1, cornerOutBack1);
                            }
                            
                            Handles.color = colorTrajectoryHint;
                            Handles.DrawLine (segmentCenter, segmentCenter + Vector3.up);

                            var cornerOutForward = boxOrigin + segmentDirectionForward * boxLengthHalf + segmentDirectionSide * boxDepthHalfAnim;
                            var cornerOutBack = boxOrigin - segmentDirectionForward * boxLengthHalf + segmentDirectionSide * boxDepthHalfAnim;
                            var cornerInForward = boxOrigin + segmentDirectionForward * boxLengthHalf - segmentDirectionSide * boxDepthHalfAnim;
                            var cornerInBack = boxOrigin - segmentDirectionForward * boxLengthHalf - segmentDirectionSide * boxDepthHalfAnim;
                            
                            Handles.color = colorTrajectoryHint;
                            Handles.DrawLine (cornerInBack, cornerOutForward);

                            Handles.color = colorTrajectoryCollision;
                            Handles.DrawLine (cornerOutForward, cornerOutBack);
                            Handles.DrawLine (cornerInForward, cornerInBack);
                            Handles.DrawLine (cornerInForward, cornerOutForward);
                            Handles.DrawLine (cornerInBack, cornerOutBack);
                            Handles.CircleHandleCap (0, segmentCenter, circleRotation, 0.25f, EventType.Repaint);
                            
                            
                        }
                    }
                }
            }
        }
    }
}
