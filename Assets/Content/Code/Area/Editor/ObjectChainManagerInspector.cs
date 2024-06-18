using System.Linq;
using Area;
using PhantomBrigade.Data;
using UnityEngine;
using UnityEditor;
using Sirenix.OdinInspector.Editor;

[CustomEditor (typeof(ObjectChainManager))]
public class ObjectChainManagerInspector : OdinEditor
{
    private Color colorUnused = Color.gray;
    private Color colorMain = Color.white;
    private Color colorEnemy = Color.Lerp (Color.red, Color.white, 0.5f);
    private Color colorFriendly = Color.Lerp (Color.cyan, Color.white, 0.5f);
    private Color colorEmpty = Color.yellow;
    
    private Color colorRedPrimary = Color.Lerp (Color.red, Color.white, 0.5f);
    private Color colorRedSecondary = Color.Lerp (Color.red, Color.white, 0.7f);

    private ObjectChainLink selectedLink;

    private Material previewMaterialLast;
    private GameObject previewHolder;
    private ObjectChainHelper previewHelper;
    private GUIStyle labelStyle;
    private GUIStyle labelStyleHelp;
    private Quaternion circleRotation = Quaternion.Euler (90f, 0f, 0f);

    private bool showGizmoInfo = true;
    private GUIStyle gizmoGUIStyle = null;

    private string prefabSubsetTypeLast = string.Empty;
    private static bool prefabSelectedFlipped = false;

    public override void OnInspectorGUI ()
    {
        var manager = target as ObjectChainManager;
        if (manager != null && manager.prefabs != null && manager.prefabs.Count > 0 && manager.placementHelper != null)
        {
            if (manager.instanceLinkSelected != null)
            {
                EditorGUILayout.BeginVertical ("Box");

                for (int i = 0, count = manager.instanceLinkSelected.component.links.Count; i < count; ++i)
                {
                    var link = manager.instanceLinkSelected.component.links[i];
                    if (link == null || link.transform == null)
                        continue;
                    
                    bool linkUsed = link.attachment != null;
                    
                    GUILayout.BeginHorizontal ();
                    GUILayout.Label (i.ToString (), EditorStyles.miniLabel, GUILayout.Width (30f));
                    GUILayout.Label (linkUsed ? $"connected to {link.attachment.name}" : $"{i}: —");
                    
                    if (linkUsed)
                    {
                        if (GUILayout.Button ("Clear from this point", GUILayout.Width (180f)))
                        {
                            manager.RemoveInstanceAttachedToLink (link, true);
                        }
                    }
                    GUILayout.EndHorizontal ();
                }

                EditorGUILayout.EndVertical ();
            }
        }

        prefabSelectedFlipped = EditorGUILayout.Toggle ("Flip new objects", prefabSelectedFlipped);
        GUILayout.Space (10f);
        EditorGUILayout.HelpBox("- When starting from scratch, manually add a chain prefab into the scene.\n- Parent it to this object with Chain Manager component.\n- Add chain prefabs by drag-n-dropping a selection of prefabs from 'Project' tab into 'Prefab drop-off', NOT 'Prefabs'.", MessageType.Info, true); 
        
        base.OnInspectorGUI ();
        // You can also call base.OnInspectorGUI(); instead if you simply want to prepend or append GUI to the editor.
    }

    public void OnSceneGUI ()
    {
        var manager = target as ObjectChainManager;
        if (manager == null || manager.prefabs == null || manager.prefabs.Count == 0 || manager.placementHelper == null)
            return;
        
        if (ObjectChainManager.prefabSubset.Count == 0)
            RefreshPrefabSubset (manager);
        
        if (ObjectChainManager.prefabSubset.Count == 0)
            return;

        if (manager.prefabSelected == null)
            manager.prefabSelected = ObjectChainManager.prefabSubset[0];
        
        if (labelStyle == null)
            labelStyle = new GUIStyle (EditorStyles.textArea);

        if (labelStyleHelp == null)
            labelStyleHelp = new GUIStyle (EditorStyles.boldLabel);

        // Disable clicking on scene objects
        HandleUtility.AddDefaultControl (GUIUtility.GetControlID (FocusType.Passive));

        var area = DataMultiLinkerCombatArea.selectedArea;
        var e = Event.current;
        bool eventPresent = e.type == EventType.MouseDown || e.type == EventType.ScrollWheel;
        
        bool alt = e.alt;
        bool shift = e.shift;
        bool ctrl = e.control;
        
        var buttonPressed = KeyCode.None;
        if (eventPresent)
        {
            if (e.type == EventType.MouseDown)
            {
                if (e.button == 0)
                    buttonPressed = KeyCode.Mouse0;
                if (e.button == 1)
                    buttonPressed = KeyCode.Mouse1;
                if (e.button == 2)
                    buttonPressed = KeyCode.Mouse2;
            }
            else if (e.type == EventType.ScrollWheel)
            {
                bool forward = e.delta.y > 0f;
                if (e.shift)
                    buttonPressed = forward ? KeyCode.LeftBracket : KeyCode.RightBracket;
                else if (e.control)
                    buttonPressed = forward ? KeyCode.LeftArrow : KeyCode.RightArrow;
                else if (e.alt)
                    buttonPressed = forward ? KeyCode.PageDown : KeyCode.PageUp;
            }
        }
        
        var positionMouse = e.mousePosition;
        bool hovered = false;
        
        if (previewHolder == null)
            previewHolder = GameObject.Find ("ObjectChainPreview");

        if (previewHolder == null)
        {
            previewHolder = new GameObject ("ObjectChainPreview");
            previewHolder.hideFlags = HideFlags.HideAndDontSave;
        }

        if (previewHolder.transform.parent != manager.transform)
            previewHolder.transform.parent = manager.transform;

        if (manager != null && manager.instances != null)
        {
            for (int i = 0; i < manager.instances.Count; ++i)
            {
                var helperInstanceLink = manager.instances[i];
                if (helperInstanceLink == null)
                    continue;

                var helperInstance = helperInstanceLink.component;
                if (helperInstance == null || helperInstance.links == null)
                    continue;

                bool selected = manager.instanceLinkSelected == helperInstanceLink;
                var helperTransform = helperInstance.transform;
                var helperPos = helperTransform.TransformPoint (helperInstance.center);
                var helperRot = helperTransform.rotation;

                Handles.color = selected ? colorEnemy : colorUnused;
                if (selected)
                {
                    Handles.DrawLine (helperPos, helperPos + helperRot * (Vector3.up * 4f));
                    Handles.CircleHandleCap (0, helperPos, helperRot * circleRotation, 1.5f, EventType.Repaint);
                }

                var linkOriginPosGui = HandleUtility.WorldToGUIPoint (helperPos);
                if (!hovered)
                {
                    float dist = Vector2.Distance (positionMouse, linkOriginPosGui);
                    if (dist < 10f)
                    {
                        Handles.Label (helperPos + Vector3.up * 5f, "Shift + LMB - Delete segment\n(Warning: unpack prefab before deleting)", labelStyleHelp);
                        hovered = true;
                        Repaint ();
                    }
                }
                
                var helperBtnSize = HandleUtility.GetHandleSize (helperPos);
                if (Handles.Button (helperPos, Quaternion.identity, helperBtnSize * 0.05f, helperBtnSize * 0.2f, Handles.DotHandleCap))
                {
                    if (shift)
                    {
                        manager.RemoveInstance (helperInstanceLink);
                        break;
                    }
                    else
                    {
                        if (!selected)
                            manager.instanceLinkSelected = helperInstanceLink;
                        else
                            manager.instanceLinkSelected = null;
                        Repaint ();
                        break;
                    }
                }
                
                if (!selected)
                    continue;
                    
                var prefabSelected = manager.prefabSelected;
                var prefabSelectedAsset = prefabSelected.asset;
                var prefabSelectedName = prefabSelectedAsset != null ? prefabSelectedAsset.name : null;
                var prefabSelectedIndex = ObjectChainManager.prefabSubset.IndexOf (prefabSelected);
                    
                var originIndex = prefabSelected.originIndex;
                var linkOrigin = originIndex.IsValidIndex (prefabSelectedAsset.links) ? prefabSelectedAsset.links[originIndex] : null;
                if (linkOrigin == null || linkOrigin.transform == null)
                    continue;

                                
                var linkOriginName = linkOrigin.transform.name;
                bool interactionWithLink = false;

                for (int l = 0; l < helperInstance.links.Count; ++l)
                {
                    var link = helperInstance.links[l];
                    if (link == null || link.transform == null)
                        continue;

                    var linkPos = link.transform.position;
                    var linkRot = link.transform.rotation;
                    var linkForward = link.transform.forward;
                    bool linkUsed = link.attachment != null;
                    
                    Handles.color = linkUsed ? colorFriendly : colorUnused;
                    Handles.DrawLine (linkPos + linkForward * 1f, linkPos + linkForward * 2f);
                    Handles.CircleHandleCap (0, linkPos, link.transform.rotation * circleRotation, 1f, EventType.Repaint);
                    Handles.DrawLine (linkPos, linkPos + Vector3.up * 0.1f);
                    
                    // Press
                    var linkBtnSize = HandleUtility.GetHandleSize (linkPos);
                    var linkBtnSizePick = linkBtnSize * 0.2f; // linkBtnSize * linkBtnSize;
                    var linkBtnSizeSqr = linkBtnSize * linkBtnSize;
                    var linkPosGui = HandleUtility.WorldToGUIPoint (linkPos);
                    
                    // Hover
                    if (!hovered)
                    {
                        float dist = Vector2.Distance (positionMouse, linkPosGui);
                        if (dist < 10f)
                        {
                            Handles.DrawLine (linkPos, linkPos + Vector3.up * 2f);
                            var linkCompatible = link.type == linkOrigin.type;
                            
                            if (linkUsed)
                            {
                                var linkText = $"{link.attachment.name}\n{l} ({link.transform.name}): linked";
                                Handles.Label (linkPos + Vector3.up * 5f, linkText, labelStyle);
                            }
                            else
                            {
                                var typeText = !linkCompatible ? $"incompatible ({link.type} =/= {linkOrigin.type})" : (prefabSelectedFlipped ? "X-" : "X+");
                                var helpText = "\n\n\n\n\n\nCtrl + Scroll - Select asset\nShift + Scroll - Flip asset\nAlt + Scroll - Cycle through connectors\n";

                                var linkText = $"{prefabSelectedAsset.name} ({linkOriginName}) ({typeText})\n{l} ({link.transform.name}): open";
                                Handles.Label (linkPos + Vector3.up * 5f, linkText, labelStyle);
                                Handles.Label (linkPos + Vector3.up * 5f, helpText, labelStyleHelp);
                                
                            }

                            if (previewHelper != null)
                            {
                                bool previewRemoval =
                                    linkUsed ||
                                    prefabSelectedAsset == null ||
                                    (prefabSelectedAsset != null && prefabSelectedAsset.name != previewHelper.name);

                                if (previewRemoval)
                                {
                                    // Debug.LogWarning ($"Removing preview for {previewHelper.name} due to used link, null selected asset or new name");
                                    UtilityGameObjects.ClearChildren (previewHolder);
                                    previewHelper = null;
                                }
                            }

                            if (!linkUsed && prefabSelectedAsset != null && previewHelper == null)
                            {
                                UtilityGameObjects.ClearChildren (previewHolder);
                                previewHelper = GameObject.Instantiate (prefabSelectedAsset, previewHolder.transform) as ObjectChainHelper;
                                previewHelper.hideFlags = HideFlags.HideAndDontSave;
                                previewHelper.name = prefabSelectedAsset.name;
                                previewMaterialLast = null;
                                // Debug.LogWarning ($"Creating preview asset {prefabSelectedAsset.name}");
                            }
                            else if (linkUsed)
                            {
                                DrawRemovalPreview (link, shift);
                                if (previewHelper != null)
                                {
                                    UtilityGameObjects.ClearChildren (previewHolder);
                                    previewHelper = null;
                                }
                            }

                            if (previewHelper != null)
                            {
                                var placementHelper = manager.placementHelper;
                                linkOrigin = previewHelper.links[originIndex];

                                var previewHelperTransform = previewHelper.transform;

                                previewHelperTransform.parent = previewHolder.transform;
                                previewHelperTransform.SetLocalTransformationToZero ();
                                if (prefabSelectedFlipped)
                                    previewHelperTransform.localScale = new Vector3 (-1f, 1f, 1f);

                                placementHelper.position = linkOrigin.transform.position;
                                placementHelper.forward = -linkOrigin.transform.forward;
                                previewHelperTransform.parent = placementHelper;
            
                                placementHelper.position = linkPos;
                                placementHelper.rotation = linkRot;
                                previewHelperTransform.parent = previewHolder.transform;
                                
                                var material = 
                                    linkCompatible ? 
                                    AreaManagerEditorHelper.GetMaterialMultiblockValid () : 
                                    AreaManagerEditorHelper.GetMaterialMultiblockInvalid ();
                                
                                if (previewMaterialLast != material)
                                {
                                    previewMaterialLast = material;
                                    UtilityMaterial.SetMaterialsOnRenderers (previewHelper.gameObject, material);
                                }
                            }
                            
                            RefreshPrefabSubset (manager, link.type);

                            if (buttonPressed == KeyCode.PageDown)
                            {
                                prefabSelected.originIndex = 
                                    prefabSelected.originIndex.OffsetAndWrap (false, prefabSelected.GetOriginIndexLimit ());
                            }
                            else if (buttonPressed == KeyCode.PageUp)
                            {
                                prefabSelected.originIndex = 
                                    prefabSelected.originIndex.OffsetAndWrap (true, prefabSelected.GetOriginIndexLimit ());
                            }

                            if (buttonPressed == KeyCode.LeftArrow)
                            {
                                prefabSelectedIndex = prefabSelectedIndex.OffsetAndWrap (false, ObjectChainManager.prefabSubset.Count - 1);
                                manager.prefabSelected = ObjectChainManager.prefabSubset[prefabSelectedIndex];
                            }
                            else if (buttonPressed == KeyCode.RightArrow)
                            {
                                prefabSelectedIndex = prefabSelectedIndex.OffsetAndWrap (true, ObjectChainManager.prefabSubset.Count - 1);
                                manager.prefabSelected = ObjectChainManager.prefabSubset[prefabSelectedIndex];
                            }
                            
                            if (buttonPressed == KeyCode.LeftBracket)
                            {
                                prefabSelectedFlipped = !prefabSelectedFlipped;
                            }

                            if (Handles.Button (linkPos, Quaternion.identity, linkBtnSize * 0.05f, linkBtnSizePick, Handles.DotHandleCap))
                            {
                                if (linkUsed)
                                {
                                    var attachment = link.attachment.gameObject;
                                    if (e.control)
                                    {
                                        Undo.RecordObject (helperInstance, "Removing connected object");
                                        PrefabUtility.RecordPrefabInstancePropertyModifications (helperInstance);
                                        manager.RemoveInstanceAttachedToLink (link, shift);
                                    }
                                    else
                                    {
                                        manager.TrySelectInstance (link.attachment);
                                    }
                                }
                                else
                                {
                                    Undo.RecordObject (helperInstance, "Adding connected object");
                                    PrefabUtility.RecordPrefabInstancePropertyModifications (helperInstance);
                                    var instance = manager.SpawnSelectedPrefab (link, prefabSelectedFlipped);
                                    if (instance != null)
                                    {
                                        manager.UpdateInstanceList ();
                                        manager.instanceLinkSelected = manager.instances.LastOrDefault ();
                                    }
                                }
                        
                                Repaint ();
                                interactionWithLink = true;
                                break;
                            }

                            hovered = true;
                            Repaint ();
                        }
                    }
                }
                
                if (interactionWithLink)
                    break;

                if (!hovered)
                {
                    if (previewHelper != null)
                    {
                        // Debug.LogWarning ($"Removing preview for {previewHelper.name} due to end of hover");
                        UtilityGameObjects.ClearChildren (previewHolder);
                        previewHelper = null;
                    }
                }
            }
        }

        if (eventPresent)
        {
            if (e.alt || e.control || e.shift)
                e.Use ();
        }
        
        if (GUI.changed)
        {
            EditorWindow view = EditorWindow.GetWindow<SceneView> ();
            view.Repaint ();
        }

        SceneView.RepaintAll ();
    }

    private void DrawRemovalPreview (ObjectChainLink linkStart, bool continueRecursively)
    {
        if (linkStart == null || linkStart.attachment == null)
            return;

        var helperStart = linkStart.parent;
        if (helperStart == null)
            return;

        var helperOther = linkStart.attachment;
        var linkStartPos = linkStart.transform.position;
        var helperOtherPos = helperOther.transform.TransformPoint (helperOther.center);

        Handles.color = colorRedPrimary;
        Handles.DrawLine (linkStartPos, helperOtherPos);
        Handles.CircleHandleCap (0, linkStartPos, linkStart.transform.rotation * circleRotation, 0.75f, EventType.Repaint);
        Handles.CircleHandleCap (0, helperOtherPos, helperOther.transform.rotation * circleRotation, 1.25f, EventType.Repaint);
        
        if (helperOther.links != null)
        {
            Handles.color = colorRedSecondary;
            foreach (var linkOther in helperOther.links)
            {
                // Avoid going back
                if (linkOther == null || linkOther.attachment == null || linkOther.attachment == helperStart)
                    continue;
                
                Handles.DrawLine (helperOtherPos, linkOther.transform.position);
                if (continueRecursively)
                    DrawRemovalPreview (linkOther, true); 
            }
        }
    }

    private void RefreshPrefabSubset (ObjectChainManager manager, string linkType = null)
    {
        if (linkType == prefabSubsetTypeLast)
            return;
            
        if (manager == null || manager.prefabs == null)
            return;

        prefabSubsetTypeLast = linkType;
        ObjectChainManager.prefabSubset.Clear ();
        
        bool linkTypeChecked = linkType != null;
        foreach (var prefabLink in manager.prefabs)
        {
            if (prefabLink == null || prefabLink.asset == null || prefabLink.asset.links == null)
                continue;

            if (!linkTypeChecked)
                ObjectChainManager.prefabSubset.Add (prefabLink);
            else
            {
                foreach (var link in prefabLink.asset.links)
                {
                    if (link == null || link.type != linkType)
                        continue;

                    // Debug.Log ($"Adding prefab {prefabLink.asset.name} due to link {link.transform.name} having type {link.type}");
                    ObjectChainManager.prefabSubset.Add (prefabLink);
                    break;
                }
            }
        }

        var prefabLinkSelected = manager.prefabSelected;
        bool selectionFound = false;
        
        foreach (var prefabLink in ObjectChainManager.prefabSubset)
        {
            if (prefabLink == prefabLinkSelected)
            {
                selectionFound = true;
                break;
            }
        }

        if (!selectionFound)
            manager.prefabSelected = ObjectChainManager.prefabSubset.FirstOrDefault ();
    }

    protected override void OnEnable ()
    {
        Tools.hidden = true;
    }
    
    protected override void OnDisable ()
    {
        Tools.hidden = false;
    }

    private void OnDestroy ()
    {
        if (previewHelper != null)
            DestroyImmediate (previewHelper.gameObject);
    }
}
