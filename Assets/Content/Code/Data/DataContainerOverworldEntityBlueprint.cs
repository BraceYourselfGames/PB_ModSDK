using System;
using System.Collections.Generic;
using Entitas;
using Entitas.VisualDebugging.Unity;
using PhantomBrigade.Functions;
using PhantomBrigade.Overworld;
using PhantomBrigade.Overworld.Systems;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;
using YamlDotNet.Serialization;

#if UNITY_EDITOR
#endif

namespace PhantomBrigade.Data
{
    [Serializable]
    public class DataBlockOverworldEntityMovement
    {
        public float speed;
        public bool rotateToFacing;
    }
    
    [Serializable]
    public class DataBlockOverworldEntityObserver
    {
        public bool contactUsed = true;
        public float contactRange = 2.5f;
        
        [ValueDropdown ("GetCallKeysChecks"), InlineButtonClear]
        public List<string> contactChecks = new List<string> ();
        
        [ValueDropdown ("GetCallKeysEffects"), InlineButtonClear]
        public List<string> contactEffects = new List<string> ();
        
        private static List<string> GetCallKeysChecks () =>
            FieldReflectionUtility.GetConstantStringFieldValues (typeof (OverworldContactCheckKeys), false);
        
        private static List<string> GetCallKeysEffects () =>
            FieldReflectionUtility.GetConstantStringFieldValues (typeof (OverworldContactEffectsKeys), false);
    }

    [Serializable]
    public class DataBlockOverworldEntityDetection
    {
        public float pingInterval;
        public float detectionIncrementMin;
        public float detectionIncrementMax;
        public float detectionDecayTime;
    }

    public class DataBlockScenarioChangeCompositeSpawn
    {
        public string instanceNameOverride;
        
        [ValueDropdown ("@DataMultiLinkerUnitComposite.data.Keys")]
        public string blueprintKey;
    }

    [Serializable]
    public class DataBlockOverworldEntityScenarioChanges
    {
        [DropdownReference]
        public DataBlockScenarioChangeCompositeSpawn compositeOnStart;
        
        [DropdownReference]
        public List<ICombatFunction> functionsOnStart;
        
        #region Editor
        #if UNITY_EDITOR

        [ShowInInspector]
        private DataEditor.DropdownReferenceHelper helper;

        public DataBlockOverworldEntityScenarioChanges () =>
            helper = new DataEditor.DropdownReferenceHelper (this);

        #endif
        #endregion
    }
    
    // This interface is useful as it provides a way to stop PropertyTraversal at the right node without knowing exact type name
    // The DataBlockFilterLinked<T> is never used directly and can't be automatically constructed for IsAssignableFrom check in the DropdownUtils
    public interface ITagProvider
    {
        public IEnumerable<string> GetTags ();
    }

    // [HideReferenceObjectPicker]
    [LabelWidth (120f)]
    public class DataBlockFilterLinked<T> : ITagProvider where T : DataContainer, IDataContainerTagged, new()
    {
        [OnInspectorInit ("OnInspectorInit")]
        [ToggleLeft, OnValueChanged ("OnTagsUsedChange")]
        [InlineButton ("PrintFilteredKeysToLog", "Log", ShowIf = "tagsUsed")]
        public bool tagsUsed = true;
        
        [InlineButton ("@CreateKeyList ()", "+", ShowIf = "@keys == null")]
        [PropertySpace (4f), HideIf ("tagsUsed")]
        [ValueDropdown ("$GetKeys")]
        [GUIColor ("GetKeysColor")]
        [ListDrawerSettings ( DefaultExpandedState = true, ShowPaging = false)]
        public List<string> keys;
        
        [InlineButton ("@CreateTagFilter ()", "+", ShowIf = "@tags == null")]
        [PropertySpace (4f), ShowIf ("tagsUsed")]
        [PropertyTooltip("@GetTooltip ()")]
        [OnValueChanged ("Refresh", true)]
        [DictionaryKeyDropdown ("@DropdownUtils.ParentTypeMethod ($property, \"PhantomBrigade.Data.ITagProvider\", \"GetTags\", true)")]
        [DictionaryDrawerSettings (KeyColumnWidth = DataEditor.dictionaryKeyWidth)]
        public SortedDictionary<string, bool> tags;        
        
        [HideIf ("@configsFiltered == null || !tagsUsed")]
        [PropertyOrder (1), ShowIf ("tagsUsed")]
        [GUIColor ("GetFilteredColor")]
        [LabelText ("Filter Output")]
        [YamlIgnore, ListDrawerSettings (DefaultExpandedState = false, ShowPaging = true, DraggableItems = false, HideAddButton = true, HideRemoveButton = true)]
        public List<DataContainerLink<T>> configsFiltered;
        
        private static Color colorWarning = Color.HSVToRGB (0.1f, 0.4f, 1f);
        private static Color colorError = Color.HSVToRGB (0.0f, 0.6f, 1f);
        private static Color colorNeutral = Color.HSVToRGB (0.55f, 0.2f, 1f);
        private bool initialized = false;

        private void OnTagsUsedChange ()
        {
            if (tagsUsed)
            {
                keys = null;
                if (tags == null)
                    tags = new SortedDictionary<string, bool> ();
            }
            else
            {
                tags = null;
                if (keys == null)
                    keys = new List<string> ();
            }
        }

        private Color GetKeysColor () => keys == null || keys.Count == 0 ? colorError : colorNeutral;

        private Color GetFilteredColor ()
        {
            if (configsFiltered == null || configsFiltered.Count == 0)
                return colorError;

            if (configsFiltered.Count > 8)
                return colorWarning;

            return colorNeutral;
        }

        public override string ToString ()
        {
            if (tagsUsed)
                return $"{GetType ().Name} | Tags: {tags?.ToStringFormattedKeyValuePairs ()}";
            else
                return $"{GetType ().Name} | Keys: {keys?.ToString ()}";
        }

        protected virtual string GetTooltip ()
        {
            return string.Empty;
        }

        private void CreateKeyList ()
        {
            if (keys == null)
            {
                keys = new List<string> { string.Empty };
            }
        }

        private void CreateTagFilter ()
        {
            if (tags == null)
            {
                tags = new SortedDictionary<string, bool> { { string.Empty, true } };
            }
        }

        public virtual IEnumerable<string> GetTags ()
        {
            return null;
        }

        protected IEnumerable<string> GetKeys ()
        {
            var dataTemp = GetData ();
            return dataTemp != null ? dataTemp.Keys : null;
        }
        
        public virtual SortedDictionary<string, T> GetData ()
        {
            return null;
        }

        private void OnInspectorInit ()
        { 
            if (!initialized)
            {
                initialized = true;
                Refresh ();
            }
        }
        
        public virtual void Refresh ()
        {
            RefreshInternal (GetData ());
        }

        protected void RefreshInternal (SortedDictionary<string, T> data)
        {
            /*
            if (configsFiltered == null)
                configsFiltered = new List<DataContainerLink<T>> ();
            else
                configsFiltered.Clear ();
                    
            if (tags == null || tags.Count == 0 || data == null)
                return;

            var keysWithTags = DataTagUtility.GetKeysWithTags (data, tags);
            if (keysWithTags == null || keysWithTags.Count == 0)
                return;

            foreach (var key in keysWithTags)
                configsFiltered.Add (new DataContainerLink<T> (key));
            */

            if (!tagsUsed)
            {
                if (configsFiltered != null)
                    configsFiltered.Clear ();
            }
            else
            {
                if (configsFiltered == null)
                    configsFiltered = new List<DataContainerLink<T>> ();
                else
                    configsFiltered.Clear ();
            
                var keysFound = GetFilteredKeys (data);
                if (keysFound == null)
                    return;
                
                foreach (var key in keysFound)
                    configsFiltered.Add (new DataContainerLink<T> (key));
            }
        }

        private static List<string> keysInitial = new List<string> ();
        private static List<string> keysFiltered = new List<string> ();

        public virtual bool IsCandidateValid (T candidate)
        {
            if (candidate == null)
                return false;

            if (tagsUsed)
            {
                if (tags == null || tags.Count == 0)
                    return false;
                
                var tagsInContainer = candidate.GetTags (true);
                bool tagsInContainerPresent = tagsInContainer != null && tagsInContainer.Count > 0;
                
                if (!tagsInContainerPresent)
                    return false;

                bool invalid = false;
                foreach (var kvp in tags)
                {
                    string tag = kvp.Key;
                    bool required = kvp.Value;
                    bool present = tagsInContainer.Contains (tag);
                    
                    if (present != required)
                    {
                        invalid = true;
                        break;
                    }
                }

                if (invalid)
                    return false;
            }
            else
            {
                if (keys == null || keys.Count == 0)
                    return false;

                if (string.IsNullOrEmpty (candidate.key))
                    return false;

                if (!keys.Contains (candidate.key))
                    return false;
            }

            return true;
        }

        public string GetRandomKey (SortedDictionary<string, T> data, HashSet<string> keysExempt = null)
        {
            var k = GetFilteredKeys (data, keysExempt);
            if (k == null || k.Count == 0)
                return null;
                        
            return k.GetRandomEntry ();
        }
        
        public List<string> GetFilteredKeys (SortedDictionary<string, T> data, HashSet<string> keysExempt = null)
        {
            keysInitial.Clear ();
                
            if (tagsUsed)
            {
                var keysWithTags = DataTagUtility.GetKeysWithTags (data, tags);
                keysInitial.AddRange (keysWithTags);
            }
            else if (keys != null && keys.Count > 0)
                keysInitial.AddRange (keys);
                        
            if (keysInitial.Count == 0)
                return null;
                        
            keysFiltered.Clear ();
            foreach (var keyCandidate in keysInitial)
            {
                if (keysExempt != null && keysExempt.Contains (keyCandidate))
                    continue;
                
                var configCandidate = data.TryGetValue (keyCandidate, out var v) ? v : null;
                if (configCandidate == null)
                    continue;

                bool valid = IsCandidateValid (configCandidate);
                if (!valid)
                    continue;
                            
                keysFiltered.Add (keyCandidate);
            }

            if (keysFiltered.Count == 0)
                return null;
                        
            return keysFiltered;
        }

        private void PrintFilteredKeysToLog ()
        {
            var results = GetFilteredKeys (GetData ());
            if (results == null)
                return;
            
            Debug.Log ($"{this.GetType ().GetNiceTypeName ()} ({results.Count})\n{results.ToStringMultilineDash ()}");
        }
    }

    public class DataBlockFilterLinkedCounted<T> : DataBlockFilterLinked<T> where T : DataContainer, IDataContainerTagged, new ()
    {
        /*
        [LabelText ("Count"), PropertyOrder (-2)]
        [YamlIgnore, ShowInInspector, MinMaxSlider (1, 5, true)]
        public Vector2Int countRange
        {
            get
            {
                return new Vector2Int (countMin, countMax);
            }
            set
            {
                countMin = Mathf.Max (1, value.x);
                countMax = Mathf.Max (countMin, value.y);
            }
        }
        */
        
        [PropertyOrder (-3)]
        [HorizontalGroup]
        [LabelText ("Count"), SuffixLabel ("$GetLabelText", true)]
        public int countMin = 1;
        
        [PropertyOrder (-3)]
        [HorizontalGroup (0.2f)]
        [HideLabel, ShowIf (nameof(countRandom)), SuffixLabel ("max", true)]
        public int countMax = 1;
        
        [HideInInspector]
        public bool countRandom = false;

        [Button ("$GetButtonText", ButtonHeight = 21), HorizontalGroup (120f)]
        private void ToggleRandom ()
        {
            countRandom = !countRandom;
            if (!countRandom)
                countMax = countMin;
        }

        private string GetLabelText () => countRandom ? "min" : string.Empty;

        private string GetButtonText () => countRandom ? "Switch to fixed" : "Switch to random";
        
        public override void Refresh ()
        {            
            countMin = Mathf.Max (1, countMin);
            countMax = Mathf.Max (countMin, countMax);
            base.Refresh ();
        }
    }
    
    [Serializable]
    public class DataBlockOverworldEntityScenarios : DataBlockFilterLinked<DataContainerScenario>
    {
        public override IEnumerable<string> GetTags () => DataMultiLinkerScenario.GetTags ();
        public override SortedDictionary<string, DataContainerScenario> GetData () => DataMultiLinkerScenario.data;

        protected override string GetTooltip () => "Optional scenario constraints.";
    }
    
    [Serializable]
    public class DataBlockOverworldEntityAreas : DataBlockFilterLinked<DataContainerCombatArea>
    {
        public override IEnumerable<string> GetTags () => DataMultiLinkerCombatArea.GetTags ();
        public override SortedDictionary<string, DataContainerCombatArea> GetData () => DataMultiLinkerCombatArea.data;

        protected override string GetTooltip () => "Optional area constraints. These usually shouldn't include mission type and should typically just specify location type etc.";
    }
    
    [HideReferenceObjectPicker]
    public class DataBlockStringNonSerialized
    {
        [YamlIgnore]
        [HideLabel]
        public string s;
    }
    
    [HideReferenceObjectPicker]
    public class DataBlockStringNonSerializedLong
    {
        [YamlIgnore]
        [HideLabel, TextArea (1, 10)]
        public string s;
    }

    public static class ScenarioBlockTags
    {
        public const string Start = "start";
        public const string Reinforcement1 = "reinforcement1";
        public const string Reinforcement2 = "reinforcement2";
        public const string Reinforcement3 = "reinforcement3";
    }

    public class DataBlockScenarioUnitsInjected
    {
        public bool step = true;
        
        [ValueDropdown ("@FieldReflectionUtility.GetConstantStringFieldValues (typeof (ScenarioBlockTags), false)")]
        public HashSet<string> tags = new HashSet<string> ();

        [ListDrawerSettings (AlwaysAddDefaultValue = true, DefaultExpandedState = false)]
        public List<DataBlockScenarioUnitGroup> unitGroups = new List<DataBlockScenarioUnitGroup> ();
    }
}
