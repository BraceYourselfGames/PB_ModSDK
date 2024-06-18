using System;
using System.Collections.Generic;
using System.Text;
using PhantomBrigade.Data.UI;
using PhantomBrigade.Functions;
using PhantomBrigade.Overworld;
using Sirenix.OdinInspector;
using UnityEngine;
using YamlDotNet.Serialization;

namespace PhantomBrigade.Data
{
    public enum OverworldMemoryType
    {
        Int,
        Float
    }
    
    public enum OverworldMemoryHost
    {
        World,
        Province,
        Unit,
        Pilot
    }
    
    [HideReferenceObjectPicker]
    public class DataBlockOverworldMemoryUI
    {
        [YamlIgnore, LabelText ("Name")]
        public string textName;

        public string format = "0.##";
        public string suffix = null;
        public float multiplier = 1f;
        public bool showInEventOptions = true;
        
        [ValueDropdown ("@DataShortcuts.ui.colors.Keys"), InlineButtonClear]
        [GUIColor ("GetColorOverride")]
        public string combatColorOverride = null;
        public DataBlockVector2 combatDisplayRange;

        private Color GetColorOverride ()
        {
            if (string.IsNullOrEmpty (combatColorOverride))
                return new Color (1f, 1f, 1f, 1f);
            else
                return DataHelperUI.GetColor (combatColorOverride);
        }
    }

    [Serializable] [LabelWidth (200f)]
    public class DataContainerOverworldMemory : DataContainerWithText
    {
        [DropdownReference (true)]
        public DataBlockOverworldMemoryUI ui;

        public OverworldMemoryHost host = OverworldMemoryHost.World;
        public OverworldMemoryType type = OverworldMemoryType.Int;
        
        [LabelText ("Discard on reset")]
        public bool discardOnWorldChange = true;
        
        [LabelText ("Old key")]
        public string keyOld;

        public override void OnAfterDeserialization (string key)
        {
            base.OnAfterDeserialization (key);
        }

        public override void ResolveText ()
        {
            if (ui == null)
                return;

            ui.textName = DataManagerText.GetText (TextLibs.overworldMemories, $"{key}__name", true);
        }
        
        public override void OnKeyReplacement (string keyOld, string keyNew)
        {
            base.OnKeyReplacement (keyOld, keyNew);

            FunctionUtility.ReplaceInFunction (typeof (ModifyMemoryBase), keyOld, keyNew, (function, context) =>
            {
                var functionTyped = (ModifyMemoryBase)function;
                if (functionTyped.changes != null)
                {
                    foreach (var change in functionTyped.changes)
                    {
                        if (change != null)
                            FunctionUtility.TryReplaceInString (ref change.key, keyOld, keyNew, context);
                    }
                }
            });
        }

        #region Editor
        #if UNITY_EDITOR

        public override void SaveText ()
        {
            if (!IsTextSavingPossible () || ui == null)
                return;
    
            DataManagerText.TryAddingTextToLibrary (TextLibs.overworldMemories, $"{key}__name", ui.textName);
        }
       
        [ShowInInspector]
        private DataEditor.DropdownReferenceHelper helper;
        
        public DataContainerOverworldMemory () => 
            helper = new DataEditor.DropdownReferenceHelper (this);

        #endif
        #endregion
    }
    
    /// <summary>
    /// Class declaring a group of checks on persistent object memory.
    /// Used instead of various places directly using lists of DataBlockOverworldMemoryCheck.
    /// This allows for one centralized implementation of any/all cases when iterating over a memory check list.
    /// </summary>
    [Serializable][HideReferenceObjectPicker]
    public class DataBlockOverworldMemoryCheckGroup
    {
        public EntityCheckMethod method = EntityCheckMethod.RequireAll;
        
        [ListDrawerSettings (ShowPaging = false, CustomAddFunction = "@new DataBlockOverworldMemoryCheck ()")]
        public List<DataBlockOverworldMemoryCheck> checks = new List<DataBlockOverworldMemoryCheck> ();

        private static StringBuilder sb = new StringBuilder ();
        private static bool old = false;
        
        public override string ToString ()
        {
            sb.Clear ();
            old = false;

            if (checks != null && checks.Count > 0)
            {
                sb.Append ("Conditions (");
                sb.Append (method == EntityCheckMethod.RequireOne ? "one match required" : "all required");
                sb.Append (")");

                foreach (var check in checks)
                {
                    if (check == null)
                        continue;

                    sb.Append ("\n  - ");
                    sb.Append (check.ToString ());
                }
            }
            else
            {
                sb.Append ("No memory checks declared");
                old = true;
            }

            return sb.ToString ();
        }

        public string GetStringAndWarning (out bool warning)
        {
            var text = this.ToString ();
            warning = old;
            return text;
        }

        #region Editor
        #if UNITY_EDITOR

        [ShowInInspector]
        private DataEditor.DropdownReferenceHelper helper;
        
        public DataBlockOverworldMemoryCheckGroup () => 
            helper = new DataEditor.DropdownReferenceHelper (this);

        #endif
        #endregion
        
    }
    
    public enum MemoryValueCheckMode
    {
        NoValueCheck,
        Less,
        LessEqual,
        Equal,
        GreaterEqual,
        Greater
    }

    /// <summary>
    /// Class declaring a check against memory of a persistent entity.
    /// Points to a specific key, checks its presence and optionally its value.
    /// </summary>
    [Serializable][HideReferenceObjectPicker]
    public class DataBlockOverworldMemoryCheck
    {
        // [OnInspectorGUI ("OnInspectorGUI", false)]
        [PropertyOrder (-2)]
        [HideLabel, ValueDropdown ("GetKeys")]
        public string key;
        
        [HideInInspector]
        public bool presenceDesired = true;
        
        [HideInInspector]
        public bool valueFromMemory = false;
        
        [PropertyOrder (6)]
        [ShowIf ("valueFromMemory")]
        [HideLabel, ValueDropdown ("GetKeys")]
        public string valueFromMemoryKey;

        [HorizontalGroup ("Check", 0.3f)]
        [HideLabel, GUIColor ("GetCheckColor")]
        public MemoryValueCheckMode valueCheck = MemoryValueCheckMode.NoValueCheck;

        [HorizontalGroup ("Check")]
        [HideLabel]
        [ShowIf ("IsValueVisible")]
        public float value;

        public bool IsPassed (bool valuePresent, float valueCurrent, SortedDictionary<string, float> memoryData)
        {
            bool valueValid = true;
            var valueChecked = value;

            if (valueFromMemory)
            {
                float valueFromProvider = 0f;
                bool memoryRetrieved = memoryData != null && memoryData.TryGetValue (valueFromMemoryKey, out valueFromProvider);
                valueChecked += valueFromProvider;
            }

            // Only check value if it's requested, and it's possible to do so
            if (valuePresent && valueCheck != MemoryValueCheckMode.NoValueCheck)
            {
                bool roughlyEqual = valueCurrent.RoughlyEqual (valueChecked);
                // If we don't use RoughlyEqual, the equality zone is infinitely small and imprecise,
                // but with it, the result splits neatly into 3 zones with non-zero width middle
                // [   <   ][ == ][   >   ]
                // [       <=    ]
                //          [    >=       ]

                if (valueCheck == MemoryValueCheckMode.Less)
                    valueValid = valueCurrent < valueChecked && !roughlyEqual;
                else if (valueCheck == MemoryValueCheckMode.LessEqual)
                    valueValid = valueCurrent < valueChecked || roughlyEqual;
                else if (valueCheck == MemoryValueCheckMode.Equal)
                    valueValid = roughlyEqual;
                else if (valueCheck == MemoryValueCheckMode.GreaterEqual)
                    valueValid = valueCurrent > valueChecked || roughlyEqual;
                else if (valueCheck == MemoryValueCheckMode.Greater)
                    valueValid = valueCurrent > valueChecked && !roughlyEqual;
                else
                    valueValid = false;
            }
            
            // Only two cases are useful
            // - value must be present and match condition
            // - value must be absent or match condition
            // Here are all 4 possible permutations to illustrate the issue with 2:

            // [  Value Present  ]  AND   [   Value Matches Condition   ]
            // - makes sense, e.g. "reputation is present and above 100"
            
            // [   Value Absent  ]  AND   [   Value Matches Condition   ]
            // - nonsense, it's impossible to ensure value is valid if it's always absent
            
            // [  Value Present  ]   OR   [   Value Matches Condition   ]
            // - nonsense, left half is a superset of right half
            
            // [   Value Absent  ]   OR   [   Value Matches Condition   ]
            // - makes some limited sense, e.g. "infamy not present or below 25"
            
            // Value must be present and match condition
            if (presenceDesired)
                return valuePresent && valueValid;
            else
            {
                // If we don't have a value check, that doesn't mean we want to return "true" when any value is present,
                // that would be in conflict with "value must be absent" use case. However, if a value check is present,
                // allowing absence OR passage of that check to return true is a valid use case - hence the split here:
                if (valueCheck != MemoryValueCheckMode.NoValueCheck)
                    return !valuePresent || valueValid;
                else
                    return !valuePresent;
            }
        }

        public override string ToString ()
        {
            string keyText = !string.IsNullOrEmpty (key) ? key : "[no key]";
            if (valueCheck != MemoryValueCheckMode.NoValueCheck)
            {
                if (valueCheck == MemoryValueCheckMode.Less)
                    return $"{keyText}: {(presenceDesired ? "must be present and" : "must be absent or")} less than {value}";
                else if (valueCheck == MemoryValueCheckMode.LessEqual)
                    return $"{keyText}: {(presenceDesired ? "must be present and" : "must be absent or")} less/equal to {value}";
                else if (valueCheck == MemoryValueCheckMode.Equal)
                    return $"{keyText}: {(presenceDesired ? "must be present and" : "must be absent or")} equal to {value}";
                else if (valueCheck == MemoryValueCheckMode.GreaterEqual)
                    return $"{keyText}: {(presenceDesired ? "must be present and" : "must be absent or")} equal/greater than {value}";
                else if (valueCheck == MemoryValueCheckMode.Greater)
                    return $"{keyText}: {(presenceDesired ? "must be present and" : "must be absent or")} greater than {value}";
                else
                    return $"{keyText}: {(presenceDesired ? "must be present and" : "must be absent or")} [unknown value check!]";
            }
            else
            {
                if (presenceDesired)
                    return $"{keyText}: must be present";
                else
                    return $"{keyText}: must be absent";
            }
        }

        #region Editor
        #if UNITY_EDITOR

        [PropertyOrder (-1)]
        [HorizontalGroup ("Check", 0.3f)]
        [Button ("@GetBoolLabel"), GUIColor ("GetBoolColor")]
        private void TogglePresenceDesired ()
        {
            presenceDesired = !presenceDesired;
        }
        
        [PropertyOrder (5)]
        [HorizontalGroup ("Check", 64f)]
        [ShowIf ("IsValueVisible")]
        [Button ("@GetMemoryLabel"), GUIColor ("GetMemoryColor")]
        private void ToggleValueFromMemory ()
        {
            valueFromMemory = !valueFromMemory;
        }

        private void OnInspectorGUI ()
        {
            GUILayout.Label (this.ToString (), UnityEditor.EditorStyles.miniLabel);
        }

        private IEnumerable<string> GetKeys => DataMultiLinkerOverworldMemory.data.Keys;

        private bool IsValueVisible => valueCheck != MemoryValueCheckMode.NoValueCheck;
        private string GetBoolLabel => valueCheck != MemoryValueCheckMode.NoValueCheck ? (presenceDesired ? "Present And" : "Absent Or") : (presenceDesired ? "Present" : "Absent");
        private string GetMemoryLabel => valueFromMemory ? "◄ + ▼" : "◄";

        private Color GetBoolColor => Color.HSVToRGB (presenceDesired ? 0.55f : 0f, 0.5f, 1f);
        private Color GetCheckColor => new Color (1f, 1f, 1f, valueCheck != MemoryValueCheckMode.NoValueCheck ? 1f : 0.5f);
        private Color GetMemoryColor => new Color (1f, 1f, 1f, valueFromMemory ? 1f : 0.5f);

        #endif
        #endregion
    }
}