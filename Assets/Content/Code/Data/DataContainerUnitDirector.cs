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

	    [NonSerialized]
	    private List<CombatEntity> unitsFilteredCopy = new List<CombatEntity> ();

	    #if !PB_MODSDK
	    public virtual bool IsPassed (PersistentEntity unitPersistentSelf, CombatEntity unitCombatSelf, out List<CombatEntity> unitsFilteredOut)
	    {
		    if (unitsFilteredCopy == null)
			    unitsFilteredCopy = new List<CombatEntity> ();
		    
		    bool passed = true;
		    unitsFilteredOut = unitsFilteredCopy;
		    unitsFilteredOut.Clear ();
		    
		    if (!enabled)
			    return false;

		    var originPosition = unitCombatSelf.position.v;
		    var originDirection = unitCombatSelf.directionTargetPrimary.v;
		    
		    if (turn != null)
		    {
			    var combat = Contexts.sharedInstance.combat;
			    var turnChecked = combat.currentTurn.i;
			    passed = turn.IsPassed (true, turnChecked);
		    }
		    
		    if (passed && turnModulus != null)
		    {
			    var combat = Contexts.sharedInstance.combat;
			    var turnChecked = combat.currentTurn.i;
			    var output = turnChecked == 0 ? 0 : turnChecked % turnModulus.factor;
			    passed = turnModulus.IsPassed (true, output);
                    
			    // Debug.Log ($"Turn checked: {turnChecked} | {turnChecked} % {definition.turnModulus.factor} = {output} | Passed: {turnModuloValid}");
		    }
		    
		    if (passed && unitFilterCheck != null)
		    {
			    var unitsFiltered = unitFilterCheck.GetFilteredUnitsUsingSettings (originPosition, originDirection);
			    int unitsFilteredCount = unitsFiltered.Count;
			    bool unitsFilterPassed = unitFilterCount != null ? unitFilterCount.IsPassed (true, unitsFilteredCount) : unitsFilteredCount > 0;
			    if (!unitsFilterPassed)
				    passed = false;
			    else
			    {
				    unitsFilteredOut.Clear ();
				    unitsFilteredOut.AddRange (unitsFiltered);
			    }
		    }

		    if (passed)
		    {
			    if (unitSelfCheck != null)
			    {
				    bool unitSelfCheckPassed = ScenarioUtility.IsUnitMatchingCheck
				    (
					    unitPersistentSelf, unitCombatSelf, unitSelfCheck, true, true,
					    originPosition: originPosition,
					    originDirection: originDirection
				    );

				    if (!unitSelfCheckPassed)
					    passed = false;
			    }
		    }

		    if (passed && unitConnectedCheck != null && unitConnectedCheck.unitKeys != null && unitConnectedCheck.unitKeys.Count > 0)
		    {
			    if (unitFilterCheck == null)
				    unitsFilteredOut.Clear ();

			    if (!unitCombatSelf.hasUnitCompositeLink)
			    {
				    Debug.LogWarning ($"Can't check composite children {unitConnectedCheck.unitKeys.ToStringFormatted ()} using unit {unitPersistentSelf.ToLog ()} - no composite link found");
				    passed = false;
			    }
			    else
			    {
				    var link = unitCombatSelf.unitCompositeLink;
				    var unitsInComposite = UnitUtilities.GetUnitsInComposite (link.compositeInstanceKey);
				    foreach (var unitKeyChecked in unitConnectedCheck.unitKeys)
				    {
					    if (unitsInComposite == null || !unitsInComposite.TryGetValue (unitKeyChecked, out var unitCombatConnected))
					    {
						    Debug.LogWarning ($"Can't check composite child {unitKeyChecked} using unit {unitPersistentSelf.ToLog ()} with instance {link.compositeInstanceKey} - couldn't find a child unit with that key under composite {link.compositeInstanceKey}");
						    passed = false;
						    break;
					    }
					    
					    var unitPersistentConnected = IDUtility.GetLinkedPersistentEntity (unitCombatConnected);
					    bool unitConnectedCheckPassed = ScenarioUtility.IsUnitMatchingCheck 
					    (
						    unitPersistentConnected, unitCombatConnected, unitConnectedCheck, true, true,
						    originPosition: originPosition,
						    originDirection: originDirection
					    );
				    
					    if (!unitConnectedCheckPassed)
					    {
						    passed = false;
						    break;
					    }
					    
					    if (unitFilterCheck == null)
						    unitsFilteredOut.Add (unitCombatConnected);
				    }
				    
				    
			    }
		    }
                        
		    if (passed && memoryBase != null)
		    {
			    var basePersistent = IDUtility.playerBasePersistent;
			    bool memoryBaseValid = memoryBase.IsPassed (basePersistent);
			    if (!memoryBaseValid)
				    passed = false;
		    }

		    return passed;
	    }
	    #endif
	    
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

	    #if !PB_MODSDK
	    public void ExecuteEffects (PersistentEntity unitPersistentSelf, CombatEntity unitCombatSelf, List<CombatEntity> unitsFiltered, List<CombatEntity> unitsFilteredParent)
	    {
		    if (functionGroups == null)
			    return;

		    foreach (var group in functionGroups)
		    {
			    if (group.functionsGlobal != null && group.functionsGlobal.Count > 0)
			    {
				    var functions = group.functionsGlobal;
				    foreach (var function in functions)
					    function.Run ();
			    }

			    if (group.functionsTargeted != null && group.functionsTargeted.Count > 0)
			    {
				    var functions = group.functionsTargeted;
				    if (group.functionsTargetedContext == DirectorTargetedFunctionContext.Self)
				    {
					    foreach (var functionTargeted in functions)
						    functionTargeted.Run (unitPersistentSelf);
				    }
				    else if (group.functionsTargetedContext == DirectorTargetedFunctionContext.FilteredUnits)
				    {
					    foreach (var unitCombatFiltered in unitsFiltered)
					    {
						    var unitPersistentFiltered = IDUtility.GetLinkedPersistentEntity (unitCombatFiltered);
						    if (unitPersistentFiltered != null)
						    {
							    foreach (var functionTargeted in functions)
								    functionTargeted.Run (unitPersistentFiltered);
						    }
					    }
				    }
				    else if (group.functionsTargetedContext == DirectorTargetedFunctionContext.FilteredUnitsInParent)
				    {
					    foreach (var unitCombatFiltered in unitsFilteredParent)
					    {
						    var unitPersistentFiltered = IDUtility.GetLinkedPersistentEntity (unitCombatFiltered);
						    if (unitPersistentFiltered != null)
						    {
							    foreach (var functionTargeted in functions)
								    functionTargeted.Run (unitPersistentFiltered);
						    }
					    }
				    }
			    }
		    }
	    }
	    
	    public bool EvaluateSelfChange (ref PersistentEntity unitPersistentSelf, ref CombatEntity unitCombatSelf)
	    {
		    if (selfChange == null)
			    return true;
		    
		    if (unitCombatSelf == null || !unitCombatSelf.hasUnitCompositeLink)
		    {
			    Debug.LogWarning ($"Can't change self to unit {selfChange.unitKey} using unit {unitPersistentSelf.ToLog ()}, missing composite link or null unit");
			    return false;
		    }
		    
		    var link = unitCombatSelf.unitCompositeLink;
		    var unitsInComposite = UnitUtilities.GetUnitsInComposite (link.compositeInstanceKey);
		    if (unitsInComposite == null || !unitsInComposite.TryGetValue (selfChange.unitKey, out var unitCombatConnected))
		    {
			    Debug.LogWarning ($"Can't change self to unit {selfChange.unitKey} using unit {unitPersistentSelf.ToLog ()} with instance {link.compositeInstanceKey} - couldn't find a child unit with that key under composite {link.compositeInstanceKey}");
			    return false;
		    }

		    var unitPersistentConnected = IDUtility.GetLinkedPersistentEntity (unitCombatConnected);
		    unitPersistentSelf = unitPersistentConnected;
		    unitCombatSelf = unitCombatConnected;
		    return true;
	    }

	    private List<int> childrenIndexesValid = new List<int> ();
	    
	    public void EvaluateChildren 
		(
			PersistentEntity unitPersistentSelf, 
			CombatEntity unitCombatSelf, 
			string context, 
			List<CombatEntity> stepUnitsFilteredParent
		)
	    {
		    if (children == null || children.Count == 0)
			    return;
		    
		    if (childMode == UnitDirectorChildMode.ExecuteOneRandom)
		    {
			    var c = UnityEngine.Random.Range (0, children.Count);
			    EvaluateChild (unitPersistentSelf, unitCombatSelf, context, stepUnitsFilteredParent, c, true);

		    }
		    else if (childMode == UnitDirectorChildMode.ExecuteOneRandomValid)
		    {
			    childrenIndexesValid.Clear ();
			    for (int c = 0, cLimit = children.Count; c < cLimit; ++c)
			    {
				    bool stepChildValid = EvaluateChild (unitPersistentSelf, unitCombatSelf, context, stepUnitsFilteredParent, c, false);
				    if (stepChildValid)
					    childrenIndexesValid.Add (c);
			    }

			    if (childrenIndexesValid.Count > 0)
			    {
				    var c = childrenIndexesValid.GetRandomEntry ();
				    EvaluateChild (unitPersistentSelf, unitCombatSelf, context, stepUnitsFilteredParent, c, true);
			    }
		    }
            else
		    {
			    for (int c = 0, cLimit = children.Count; c < cLimit; ++c)
			    {
				    bool stepChildValid = EvaluateChild (unitPersistentSelf, unitCombatSelf, context, stepUnitsFilteredParent, c, true);
				    if (c < cLimit - 1)
				    {
					    if (childMode == UnitDirectorChildMode.ExecuteFirstValid)
					    {
						    if (stepChildValid)
						    {
							    bool log = DataShortcuts.sim.logCompositeBehavior;
							    if (log)
									Debug.Log ($"- Child node {context}/{c} ends evaluation of {context} children, as first valid child");
							    break;
						    }
					    }
					    else if (childMode == UnitDirectorChildMode.ExecuteUntilBlocked)
					    {
						    if (!stepChildValid)
						    {
							    bool log = DataShortcuts.sim.logCompositeBehavior;
							    if (log)
									Debug.Log ($"- Child node {context}/{c} ends evaluation of {context} children, as first invalid child");
							    break;
						    }
					    }
				    }
			    }
		    }
	    }

	    public bool EvaluateChild 
	    (
		    PersistentEntity unitPersistentSelf, 
		    CombatEntity unitCombatSelf, 
		    string context, 
		    List<CombatEntity> stepUnitsFilteredParent,
		    int childIndex,
		    bool executeEffects
		)
	    {
		    bool log = DataShortcuts.sim.logCompositeBehavior;
		    if (children == null || childIndex < 0 || childIndex >= children.Count)
		    {
			    if (log)
					Debug.Log ($"- Child node {childIndex} can't be executed, invalid index");
			    return false;
		    }
		    
		    var nodeChild = children[childIndex];
		    if (nodeChild == null || !nodeChild.enabled)
			    return false;
		    
		    bool selfChangeValidated = nodeChild.EvaluateSelfChange (ref unitPersistentSelf, ref unitCombatSelf);
		    if (!selfChangeValidated)
			    return false;
		    
		    var contextChild = $"{context}/{childIndex} ({nodeChild.name})";

		    bool stepChildValid = nodeChild.IsPassed (unitPersistentSelf, unitCombatSelf, out var stepChildUnitsFiltered);
		    if (stepChildValid && executeEffects)
		    {
			    if (log)
					Debug.Log ($"- Child node {contextChild} validated, executing effects...\n  - {(nodeChild.comment != null ? nodeChild.comment.comment : "...")}");
			    nodeChild.ExecuteEffects (unitPersistentSelf, unitCombatSelf, stepChildUnitsFiltered, stepUnitsFilteredParent);
			    nodeChild.EvaluateChildren (unitPersistentSelf, unitCombatSelf, contextChild, stepChildUnitsFiltered);
		    }
		    else
		    {
			    if (log)
					Debug.Log ($"- Child node {contextChild} {(stepChildValid ? "valid" : "invalid")}, skipping effects\n  - {(nodeChild.comment != null ? nodeChild.comment.comment : "...")}");
		    }

		    return stepChildValid;
	    }
	    
	    #endif
	    
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