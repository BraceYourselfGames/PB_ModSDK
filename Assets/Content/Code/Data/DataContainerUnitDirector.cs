using System;
using System.Collections.Generic;
using System.Text;
using PhantomBrigade.Functions;
using Sirenix.OdinInspector;
using UnityEngine;
using YamlDotNet.Serialization;

namespace PhantomBrigade.Data
{
    public class DataBlockUnitDirectorSelfChange
    {
	    [InfoBox ("All downstream checks and effects on \"self\" will target the specified composite unit. This includes checks and effects of this very node.")]
	    [ValueDropdown ("@DropdownUtils.ParentTypeProperty ($property, \"DataContainerUnitComposite\", \"GetUnitKeys\")")]
	    public string unitKey = string.Empty;
    }
    
    public class DataBlockUnitDirectorChecked
    {
	    [PropertyOrder (-18), HorizontalGroup ("Header", 16f), ToggleLeft, HideLabel]
	    public bool enabled = true;
    
	    [DropdownReference (true)]
	    public DataBlockScenarioSubcheckTurn turn;
	    
	    [DropdownReference (true)]
	    public DataBlockScenarioSubcheckTurnModulus turnModulus;
	    
	    [DropdownReference (true)]
	    public DataBlockScenarioSubcheckUnit unitSelfCheck;
	    
	    [InfoBox ("Connected unit saved to list for targeted function use", VisibleIf = "@unitConnectedCheck != null && unitFilterCheck == null")]
	    [InfoBox ("Connected unit won't be saved to for targeted function use: unit filter check takes over that output", VisibleIf = "@unitConnectedCheck != null && unitFilterCheck != null")]
	    [DropdownReference (true)]
	    public DataBlockScenarioSubcheckUnitCompositeConnected unitConnectedCheck;
	    
	    [InfoBox ("Filtered units saved to list for targeted function use", VisibleIf = "@unitFilterCheck != null")]
	    [DropdownReference (true)]
	    public DataBlockScenarioSubcheckUnitFilter unitFilterCheck;
	    
	    [DropdownReference (true)]
	    public DataBlockOverworldEventSubcheckInt unitFilterCount;
	    
	    [DropdownReference (true)]
	    public DataBlockOverworldMemoryCheckGroup memoryBase;
	    
	    #region Editor
	    #if UNITY_EDITOR
	    
	    [ShowInInspector, PropertyOrder (100)]
	    private DataEditor.DropdownReferenceHelper helper;

	    public DataBlockUnitDirectorChecked () => 
		    helper = new DataEditor.DropdownReferenceHelper (this);
	    
		#endif
	    #endregion
    }
    
    public class DataBlockUnitDirectorFacing
    {
	    [PropertyOrder (-18), HorizontalGroup ("Header", 16f), ToggleLeft, HideLabel]
	    public bool enabled = true;
	    
	    [ListDrawerSettings (CustomAddFunction = "@new DataBlockUnitDirectorFacingOption ()", ElementColor = "GetElementColor")]
	    public List<DataBlockUnitDirectorFacingOption> options = 
		    new List<DataBlockUnitDirectorFacingOption> { new DataBlockUnitDirectorFacingOption () };
	    
	    #region Editor
	    #if UNITY_EDITOR
	    
	    private Color GetElementColor (int index) => DataEditor.GetColorFromElementIndex (index);
	    
		#endif
	    #endregion
    }
    
    public class DataBlockUnitDirectorFacingOption : DataBlockUnitDirectorChecked
    {
	    [DropdownReference (true)]
	    public TargetFromSource target;
	    
	    [DropdownReference (true)]
	    public DataBlockScenarioUnitTargetFiltered targetUnitFiltered;
    }

    public class DataBlockUnitDirectorNavigation
    {
	    [PropertyOrder (-18), HorizontalGroup ("Header", 16f), ToggleLeft, HideLabel]
	    public bool enabled = true;
	    
	    [ValueDropdown ("@DataMultiLinkerAction.data.Keys")]
	    [BoxGroup ("A", false)]
	    public string actionKey;
	    
	    [BoxGroup ("A")]
	    public float speed = 1f;
	    
	    [BoxGroup ("A")]
	    public float distanceThreshold = 9f;

	    [ListDrawerSettings (CustomAddFunction = "@new DataBlockUnitDirectorNavigationOption ()", ElementColor = "GetElementColor")]
	    public List<DataBlockUnitDirectorNavigationOption> options = 
		    new List<DataBlockUnitDirectorNavigationOption> { new DataBlockUnitDirectorNavigationOption () };
	    
	    #region Editor
	    #if UNITY_EDITOR
	    
	    private Color GetElementColor (int index) => DataEditor.GetColorFromElementIndex (index);
	    
		#endif
	    #endregion
    }
    
    public class DataBlockUnitDirectorNavigationOption : DataBlockUnitDirectorChecked
    {
	    public string pointGroupFromArea = string.Empty;
	    
	    [ListDrawerSettings (CustomAddFunction = "@new DataBlockUnitDirectorNavigationPoint ()")]
	    public List<DataBlockAreaPoint> points = new List<DataBlockAreaPoint> ();
	    
	    #region Editor
	    #if UNITY_EDITOR
	    
	    private string GetSelectLabel => DataMultiLinkerUnitComposite.selectedNavOption == this ? "Deselect" : "Select";

	    [Button ("@GetSelectLabel"), PropertyOrder (-10)]
	    private void Select ()
	    {
		    if (DataMultiLinkerUnitComposite.selectedNavOption != this)
		    {
			    DataMultiLinkerUnitComposite.selectedNavOption = this;
			    DataMultiLinkerUnitComposite.selectedNavPoint = null;
		    }
		    else
			    DataMultiLinkerUnitComposite.selectedNavOption = null;
	    }

	    #endif
	    #endregion
    }

    public class DataBlockUnitDirectorNavigationPoint
    {
	    public Vector3 position;
    }

    public enum UnitDirectorChildMode
    {
	    ExecuteAll,
	    ExecuteFirstValid,
	    ExecuteUntilBlocked,
	    ExecuteOneRandom,
	    ExecuteOneRandomValid
    }

    [HideReferenceObjectPicker]
    public class DataBlockColor
    {
	    [HideLabel, ColorUsage (true, false)]
	    public Color v = new Color (1f, 1f, 1f, 1f);
    }

    public class DataBlockUnitDirectorNodeRoot : DataBlockUnitDirectorNode
    {
	    [PropertyOrder (-3), HorizontalGroup ("L")]
	    [LabelText ("Looping"), ToggleLeft]
	    public bool looping = true;
        
	    [PropertyOrder (-3), HorizontalGroup ("L"), ShowIf ("looping")]
	    [HideLabel, SuffixLabel ("turns to repeat")]
	    public int durationInTurns = 1;
	    
	    [PropertyOrder (-3), PropertyRange (0, 300)]
	    public int priority = 0;
	    
	    [HideInInspector, NonSerialized, YamlIgnore]
	    public bool removedInProcessing = false;
    }

    public enum DirectorTargetedFunctionContext
    {
	    Self,
	    FilteredUnits,
	    FilteredUnitsInParent
    }

    [HideReferenceObjectPicker]
    public class DataBlockUnitDirectorFunctionGroup
    {
	    [ShowIf ("@functionsTargeted != null")]
	    [LabelText ("Context")]
	    public DirectorTargetedFunctionContext functionsTargetedContext = DirectorTargetedFunctionContext.Self;
	    
	    [DropdownReference]
	    public List<ICombatFunctionTargeted> functionsTargeted;
	    
	    [DropdownReference]
	    public List<ICombatFunction> functionsGlobal;
	    
	    #region Editor
	    #if UNITY_EDITOR
	    
	    [ShowInInspector]
	    private DataEditor.DropdownReferenceHelper helper;

	    public DataBlockUnitDirectorFunctionGroup () => 
		    helper = new DataEditor.DropdownReferenceHelper (this);
	    
	    [GUIColor (1f, 0.75f, 0.5f), ShowIf ("IsCommsUpdateAvailable")]
	    [Button ("Update comms", ButtonSizes.Medium), PropertyOrder (-1)]
	    private void UpdateUnlocalizedComms (string key)
	    {
		    DataMultiLinkerUnitComposite.UpdateUnlocalizedCommsInFunctions (functionsGlobal, key);
	    }

	    private bool IsCommsUpdateAvailable ()
	    {
		    if (functionsGlobal != null)
		    {
			    foreach (var fn in functionsGlobal)
			    {
				    if (fn != null && fn is CombatLogMessageUnlocalized fnMessage)
					    return true;
			    }
		    }
		    return false;
	    }

	    #endif
	    #endregion
    }

    // [GUIColor ("@GetColorFromProperty ($property)")]
    public class DataBlockUnitDirectorNode : DataBlockUnitDirectorChecked
    {
	    // [InfoBox ("@GetHierarchyText", InfoMessageType.None, VisibleIf = "@!string.IsNullOrEmpty (hierarchyTextCached)")]
	    [PropertyOrder (-17), HorizontalGroup ("Header")]
	    [HideLabel]
	    [SuffixLabel ("@GetHierarchyText", true)]
	    public string name = string.Empty;
	    
	    [PropertyOrder (-10)]
	    [DropdownReference, HideLabel] 
	    public DataBlockColor color;
	    
	    [PropertyOrder (-8)]
	    [DropdownReference, HideLabel] 
	    public DataBlockComment comment;

	    [PropertyOrder (-2)]
	    [DropdownReference (true)]
	    [LabelText ("Select Unit (Self)")]
	    public DataBlockUnitDirectorSelfChange selfChange;

	    [DropdownReference]
	    [ListDrawerSettings (ElementColor = "GetElementColor", DefaultExpandedState = false)]
	    [LabelText ("Function Groups")]
	    public List<DataBlockUnitDirectorFunctionGroup> functionGroups;

	    [FoldoutGroup ("Children", false)]
	    [ShowIf ("@children != null && children.Count > 0")]
	    public UnitDirectorChildMode childMode = UnitDirectorChildMode.ExecuteAll;

	    [FoldoutGroup ("Children", false)]
	    [DropdownReference]
	    [ListDrawerSettings (CustomAddFunction = "@new DataBlockUnitDirectorNode ()", ElementColor = "GetElementColor", OnBeginListElementGUI = "DrawEntryGUI")]
	    public List<DataBlockUnitDirectorNode> children;

	    [YamlIgnore, HideInInspector]
	    public DataBlockUnitDirectorNode treeParent;
	    
	    [YamlIgnore, HideInInspector]
	    public int treeDepth;
	    
	    [YamlIgnore, HideInInspector]
	    public int treeIndex;

	    [YamlIgnore, HideInInspector]
	    public string hierarchyTextCached;

	    private List<int> childrenIndexesValid = new List<int> ();

	    public void OnDataChanges ()
	    {
		    // if (IsUpdateBlackboardTargetAvailable ())
			//     UpdateBlackboardTargets ();
	    }
	    
	    public void UpdateChildRecursive (DataBlockUnitDirectorNode n, Action<DataBlockUnitDirectorNode> action)
	    {
		    if (n == null)
			    return;
		    
		    action.Invoke (n);

		    if (n.children != null)
		    {
			    foreach (var child in n.children)
				    UpdateChildRecursive (child, action);
		    }
	    }

	    #region Editor
	    #if UNITY_EDITOR
	    
	    [ShowInInspector]
	    private DataEditor.DropdownReferenceHelper helper;

	    public DataBlockUnitDirectorNode () => 
		    helper = new DataEditor.DropdownReferenceHelper (this);
	    
	    private Color GetElementColor (int index, Color defaultColor)
	    {
		    var value = children != null && index >= 0 && index < children.Count ? children[index] : null;
		    if (value != null)
		    { 
			    if (!value.enabled)
				    return Color.gray.WithAlpha (0.2f);
			    if (value.color != null)
				    return value.color.v.WithAlpha (0.2f);
		    }
		    return DataEditor.GetColorFromElementIndex (index);
	    }

	    private static StringBuilder sb = new StringBuilder ();
	    
	    private void DrawEntryGUI (int index)
	    {
		    if (children == null || !index.IsValidIndex (children))
			    return;
            
		    var node = children[index];
		    if (node == null)
			    return;
            
		    // GUI.backgroundColor = bgColor;

		    sb.Clear ();
		    sb.Append (!string.IsNullOrEmpty (node.name) ? node.name : "Unnamed");

		    if (node.selfChange != null)
		    {
			    sb.Append (" | Unit: ");
			    sb.Append (node.selfChange.unitKey);
		    }

		    if (node.children != null)
		    {
			    sb.Append (" | ");
			    sb.Append (node.children.Count);
			    sb.Append (" nodes");
		    }
		    
		    GUILayout.BeginHorizontal ();
		    GUILayout.Label (sb.ToString (), UnityEditor.EditorStyles.miniLabel);
		    GUILayout.EndHorizontal ();
            
		    // GUI.backgroundColor = bgColorOld;
	    }
	    
	    private string GetHierarchyText => hierarchyTextCached;
	    private Color commentColor = Color.HSVToRGB (0.3f, 0f, 1f).WithAlpha (0.66f);

	    [ShowIf ("IsConvertBlackboardTargetAvailable")]
	    [Button (ButtonSizes.Medium), PropertyOrder (-1), ButtonGroup]
	    private void ConvertBlackboardTargets ()
	    {
		    string tgtName = null;
		    if (functionGroups != null)
		    {
			    foreach (var fg in functionGroups)
			    {
				    if (fg.functionsTargeted == null || fg.functionsTargetedContext != DirectorTargetedFunctionContext.FilteredUnits)
					    continue;

				    for (int i = fg.functionsTargeted.Count - 1; i >= 0; --i)
				    {
					    var ft = fg.functionsTargeted[i];
					    if (ft != null && ft is CombatUnitBlackboardSet ftbs)
					    {
						    Debug.Log ($"Found CombatUnitBlackboardSet with key {ftbs.key}");
						    tgtName = ftbs.key;
						    fg.functionsTargeted.RemoveAt (i);
					    }
				    }

				    if (fg.functionsTargeted.Count == 0)
					    fg.functionsTargeted = null;
			    }
		    }

		    unitFilterCheck.exportEntitiesToBlackboardLimited = new DataBlockUnitFilterBlackboardExport
		    {
			    indexed = false,
			    key = tgtName
		    };
	    }

	    [ShowIf ("IsUpdateBlackboardTargetAvailable")]
	    [Button (ButtonSizes.Medium), PropertyOrder (-1), ButtonGroup]
	    private void UpdateBlackboardTargets ()
	    {
		    string tgtName = null;
		    if (unitFilterCheck != null && unitFilterCheck.exportEntitiesToBlackboardLimited != null)
			    tgtName = unitFilterCheck.exportEntitiesToBlackboardLimited.key;
		    else if (functionGroups != null)
		    {
			    foreach (var fg in functionGroups)
			    {
				    if (fg.functionsTargeted == null || fg.functionsTargetedContext != DirectorTargetedFunctionContext.FilteredUnits)
					    continue;

				    foreach (var ft in fg.functionsTargeted)
				    {
					    if (ft != null && ft is CombatUnitBlackboardSet ftbs)
					    {
						    Debug.Log ($"Found CombatUnitBlackboardSet with key {ftbs.key}");
						    tgtName = ftbs.key;
					    }
				    }
			    }
		    }

		    if (string.IsNullOrEmpty (tgtName))
			    return;

		    UpdateChildRecursive (this, (n) =>
		    {
			    if (n == null)
				    return;

			    var tgtNameUsed = tgtName;
			    if (n.functionGroups != null)
			    {
				    foreach (var fg in n.functionGroups)
				    {
					    if (fg.functionsTargeted == null)
						    continue;

					    foreach (var ft in fg.functionsTargeted)
						    UpdateBlackboardTargetName (ft, tgtNameUsed);
				    }
			    }
		    });
	    }

	    private void UpdateBlackboardTargetName (ICombatFunctionTargeted ft, string tgtNameUsed)
	    {
		    if (ft == null)
			    return;
		    
		    if (ft is CombatUnitActionsCreate ftac && ftac.actions != null)
		    {
			    foreach (var a in ftac.actions)
			    {
				    if (a.target != null)
				    {
					    var tgt = a.target;
					    if (tgt.type == CombatTargetSource.UnitBlackboard || tgt.type == CombatTargetSource.UnitBlackboardRelative)
					    {
						    if (tgt.name != tgtNameUsed)
							    Debug.Log ($"Updated blackboard target (P) to {tgtNameUsed}");
						    tgt.name = tgtNameUsed;
					    }
				    }
								    
				    if (a.targetSecondary != null)
				    {
					    var tgt = a.targetSecondary;
					    if (tgt.type == CombatTargetSource.UnitBlackboard || tgt.type == CombatTargetSource.UnitBlackboardRelative)
					    {
						    if (tgt.name != tgtNameUsed)
							    Debug.Log ($"Updated blackboard target (P) to {tgtNameUsed}");
						    tgt.name = tgtNameUsed;
					    }
				    }
			    }
		    }
		    else if (ft is CombatUnitTargetCompositeConnected ftcs && ftcs.functionsTargeted != null)
		    {
			    foreach (var ftc in ftcs.functionsTargeted)
				    UpdateBlackboardTargetName (ftc, tgtNameUsed);
		    }
	    }

	    private bool IsUpdateBlackboardTargetAvailable ()
	    {
		    if (unitFilterCheck != null && unitFilterCheck.exportEntitiesToBlackboardLimited != null)
			    return true;
		    
		    if (functionGroups == null)
			    return false;

		    foreach (var fg in functionGroups)
		    {
			    if (fg.functionsTargeted == null || fg.functionsTargetedContext != DirectorTargetedFunctionContext.FilteredUnits)
				    continue;

			    foreach (var ft in fg.functionsTargeted)
			    {
				    if (ft != null && ft is CombatUnitBlackboardSet)
						return true;
			    }
		    }

		    return false;
	    }
	    
	    private bool IsConvertBlackboardTargetAvailable ()
	    {
		    if (unitFilterCheck == null || unitFilterCheck.exportEntitiesToBlackboardLimited != null)
			    return false;
		    
		    if (functionGroups == null)
			    return false;

		    foreach (var fg in functionGroups)
		    {
			    if (fg.functionsTargeted == null || fg.functionsTargetedContext != DirectorTargetedFunctionContext.FilteredUnits)
				    continue;

			    foreach (var ft in fg.functionsTargeted)
			    {
				    if (ft != null && ft is CombatUnitBlackboardSet)
					    return true;
			    }
		    }

		    return false;
	    }

	    #endif

	    #endregion
    }
}