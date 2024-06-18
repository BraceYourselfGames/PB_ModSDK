using System;
using System.Collections.Generic;
using PhantomBrigade.Data;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PhantomBrigade.Functions
{
    [Serializable]
    public class CombatValidateUnitNode : ICombatUnitValidationFunction
    {
	    public enum ChildMode
	    {
		    ExecuteAll,
		    ExecuteFirstValid,
		    ExecuteUntilBlocked,
		    ExecuteOneRandom,
		    ExecuteOneRandomValid
	    }	    
	    
	    [DropdownReference, HideLabel] 
	    public DataBlockComment comment;
	    
	    [ToggleLeft]
        public bool enabled = true;
        
	    [ToggleLeft]
	    public bool debug = false;
        
        [DropdownReference (true)]
        public DataBlockOverworldEventSubcheckInt validationsCount;
        
        [DropdownReference]
        public List<ICombatUnitValidationFunction> validations;
        
        [DropdownReference]
        public List<ICombatFunctionTargeted> functions;
        
        [ShowIf ("@children != null && children.Count > 0")]
        public ChildMode childMode = ChildMode.ExecuteAll;
        
        [DropdownReference]
        [ListDrawerSettings (DefaultExpandedState = true, ElementColor = "GetElementColor")]
        public List<CombatValidateUnitNode> children;

        private List<int> childrenIndexesValid = new List<int> ();
        
        public bool IsValid (PersistentEntity unitPersistent)
        {
	        #if !PB_MODSDK
	        
            // In this function, the returned bool has a different meaning, "evaluated successfully":
            // to keep the logic easier to track, we switch to a more appropriately named method
            return IsEvaluationSuccessful (unitPersistent, IDUtility.GetLinkedCombatEntity (unitPersistent), true);

            #else
            return false;
            #endif
        }

        public void ExecuteEffects (PersistentEntity unitPersistent, CombatEntity unitCombat)
        {
	        
        }

        public bool IsValidated (PersistentEntity unitPersistent, CombatEntity unitCombat)
        {
	        #if !PB_MODSDK

	        bool valid = true;
	        if (validations != null)
	        {
		        int countPassed = 0;
		        int countTotal = validations.Count;

		        foreach (var child in validations)
		        {
			        if (child == null)
				        continue;

			        bool childValid = child.IsValid (unitPersistent);
			        if (childValid)
				        countPassed += 1;
		        }

		        if (validationsCount != null)
		        {
			        valid = validationsCount.IsPassed (true, countPassed);
			        if (debug)
				        Debug.Log ($"Unit {unitPersistent.ToLog ()} val. node: {(valid ? "Validated" : "Not validated")} | Passed: {countPassed}/{countTotal} | Check: {validationsCount}");
		        }
				else
				{
					valid = countTotal == countPassed;
			        if (debug)
				        Debug.Log ($"Unit {unitPersistent.ToLog ()} val. node: {(valid ? "Validated" : "Not validated")} | Passed: {countPassed}/{countTotal}");
		        }
	        }

	        return valid;

	        #else
            return false;
	        #endif
        }

        public bool IsEvaluationSuccessful (PersistentEntity unitPersistent, CombatEntity unitCombat, bool validationUsed)
        {
	        #if !PB_MODSDK

	        if (unitPersistent == null || unitCombat == null)
		        return false;

	        if (!enabled)
		        return false;

	        if (validationUsed)
	        {
		        bool valid = IsValidated (unitPersistent, unitCombat);
		        if (!valid)
			        return false;
	        }

	        if (functions != null && functions.Count > 0)
	        {
		        if (debug)
			        Debug.Log ($"Unit {unitPersistent.ToLog ()} val. node: Executing {functions.Count} functions");
		        
		        foreach (var function in functions)
		        {
			        if (function != null)
				        function.Run (unitPersistent);
		        }
	        }

	        // If there are no children, block execution here
	        if (children == null)
		        return true;
	        
	        if (childMode == ChildMode.ExecuteOneRandom)
	        {
		        var c = UnityEngine.Random.Range (0, children.Count);
		        var child = children[c];
		        
		        if (debug)
			        Debug.Log ($"Unit {unitPersistent.ToLog ()} val. node: Executing one random child at index {c}");
		        
		        bool childSuccessful = child.IsEvaluationSuccessful (unitPersistent, unitCombat, true);
		        return childSuccessful;
	        }
	        else if (childMode == ChildMode.ExecuteOneRandomValid)
	        {
		        childrenIndexesValid.Clear ();
		        for (int c = 0, cLimit = children.Count; c < cLimit; ++c)
		        {
			        var child = children[c];
			        bool childValid = child.IsValidated (unitPersistent, unitCombat);
			        if (childValid)
				        childrenIndexesValid.Add (c);
		        }

		        if (childrenIndexesValid.Count > 0)
		        {
			        var c = childrenIndexesValid.GetRandomEntry ();
			        var child = children[c];
			        
			        if (debug)
				        Debug.Log ($"Unit {unitPersistent.ToLog ()} val. node: Executing one random validated child at index {c} | All validated children: {childrenIndexesValid.ToStringFormatted ()}");
			        
			        bool childSuccessful = child.IsEvaluationSuccessful (unitPersistent, unitCombat, false);
			        return childSuccessful;
		        }
				else
					return false;
	        }
	        else
	        {
		        int childCount = children.Count;
		        int childIndexLast = childCount - 1;
		        bool childSetSuccessful = true;
		        
		        for (int c = 0, cLimit = children.Count; c < cLimit; ++c)
		        {
			        var child = children[c];
			        bool childValid = child.IsEvaluationSuccessful (unitPersistent, unitCombat, true);
			        childSetSuccessful = childSetSuccessful && childValid;

			        if (c < childIndexLast)
			        {
				        if (childMode == ChildMode.ExecuteFirstValid)
				        {
					        if (childValid)
					        {
						        if (debug)
							        Debug.Log ($"Unit {unitPersistent.ToLog ()} val. node: Breaking after one successful child evaluation at index {c}/{childCount}");
						        childSetSuccessful = true;
						        break;
					        }
				        }
				        else if (childMode == ChildMode.ExecuteUntilBlocked)
				        {
					        if (!childValid)
					        {
						        if (debug)
							        Debug.Log ($"Unit {unitPersistent.ToLog ()} val. node: Breaking after one blocked child evaluation at index {c}/{childCount}");
						        break;
					        }
				        }
			        }
		        }
		        
		        if (childMode == ChildMode.ExecuteAll && debug)
			        Debug.Log ($"Unit {unitPersistent.ToLog ()} val. node: Completed evaluation of {childCount} children | Final output: {childSetSuccessful}");

		        return childSetSuccessful;
	        }

	        return false;

	        #else
            return false;
	        #endif
        }
        
        #region Editor
        #if UNITY_EDITOR

        [ShowInInspector]
        private DataEditor.DropdownReferenceHelper helper;
        
        public CombatValidateUnitNode () => 
            helper = new DataEditor.DropdownReferenceHelper (this);
        
        private Color GetElementColor (int index, Color defaultColor)
        {
	        var value = children != null && index >= 0 && index < children.Count ? children[index] : null;
	        if (value != null)
	        { 
		        if (!value.enabled)
			        return Color.gray.WithAlpha (0.2f);
	        }
	        return DataEditor.GetColorFromElementIndex (index);
        }

        #endif
        #endregion
    }
}