using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Entitas;
using Entitas.CodeGeneration.Attributes;
using Entitas.VisualDebugging.Unity;
using PhantomBrigade.Data.UI;
using PhantomBrigade.Functions;
using PhantomBrigade.Overworld;
using PhantomBrigade.Overworld.Components;
using Sirenix.OdinInspector;
using UnityEngine;
using YamlDotNet.Serialization;

namespace PhantomBrigade.Data
{
    [HideReferenceObjectPicker, HideDuplicateReferenceBox]
    public class DataContainerParent
    {
        [GUIColor ("GetKeyColor")]
        [ValueDropdown (nameof(GetKeys))]
        [SuffixLabel ("$" + nameof(hierarchyProperty)), HideLabel]
        public string key;

        [YamlIgnore, ReadOnly, HideInInspector]
        private string hierarchyProperty => DataMultiLinkerScenario.Presentation.showHierarchy ? hierarchy : string.Empty;
        
        [YamlIgnore, ReadOnly, HideInInspector]
        public string hierarchy;
        
        protected virtual IEnumerable<string> GetKeys () => null;

        #region Editor
        #if UNITY_EDITOR

        private static Color colorError = Color.Lerp (Color.red, Color.white, 0.5f);
        private static Color colorNormal = Color.white;
        
        private Color GetKeyColor ()
        {
            if (string.IsNullOrEmpty (key))
                return colorError;

            var keys = GetKeys ();
            bool present = keys != null && keys.Contains (key);
            return present ? colorNormal : colorError;
        }

        #endif

        #endregion
    }
    
    public class DataBlockOverworldPointParent : DataContainerParent
    {
        protected override IEnumerable<string> GetKeys () => DataMultiLinkerOverworldPointPreset.data.Keys;
    }
    
    public class OverworldPointState
    {
        public float timeAtSpawn;
    }
    
    [Overworld]
    public sealed class DataKeyPointPreset : IComponent
    {
        [EntityIndex]
        public string s;
    }

    [Overworld][DontDrawComponent]
    public sealed class DataLinkPointPreset : IComponent
    { 
        public DataContainerOverworldPointPreset data;
    }

    public class DataBlockOverworldPointInteraction
    {
        [DropdownReference]
        [ValueDropdown ("GetInteractionKeys")]
        public List<string> keys;
        
        [DropdownReference]
        public SortedDictionary<string, bool> tags;
        
        #region Editor
        #if UNITY_EDITOR

        [ShowInInspector]
        private DataEditor.DropdownReferenceHelper helper;

        public DataBlockOverworldPointInteraction () =>
            helper = new DataEditor.DropdownReferenceHelper (this);

        private IEnumerable<string> GetInteractionKeys => DataMultiLinkerOverworldInteraction.GetKeys ();

        #endif
        #endregion
    }

    public class DataBlockOverworldPointCore
    {
        [DropdownReference (true)]
        [LabelText ("Name Group")]
        public string textIdentifierGroup;
        
        [DropdownReference (true)]
        [LabelText ("Name")]
        public DataBlockStringNonSerialized textName;
        
        [DropdownReference (true)]
        [LabelText ("Desc.")]
        public DataBlockStringNonSerializedLong textDesc;

        [DropdownReference (true)]
        [ValueDropdown ("@TextureManager.GetExposedTextureKeys (TextureGroupKeys.OverworldEntities)")]
        [OnInspectorGUI ("@DropdownUtils.DrawTexturePreview ($value, TextureGroupKeys.OverworldEntities, 128)", false)]
        public string image;
        
        [DropdownReference (true)]
        [DataEditor.SpriteNameAttribute (true, 40f)]
        public string icon;

        [DropdownReference (true)]
        [LabelText ("Vision color")]
        public string visionColor;
        
        [DropdownReference (true)]
        public DataContainerResourcePrefab visual;
        
        [DropdownReference (true)]
        public DataContainerResourcePrefab visualLiberated;
        
        [DropdownReference (true)]
        public DataBlockOverworldEntityObserver observer;
        
        [DropdownReference (true)]
        public DataBlockOverworldEntityMovement movement;
        
        [DropdownReference (true)]
        public DataBlockFloat rangeVision;
        
        [DropdownReference (true)]
        public DataBlockFloat rangeInteraction;
        
        [DropdownReference (true)]
        public DataBlockOverworldEntityDetection detection;
        
        #region Editor
        #if UNITY_EDITOR

        [ShowInInspector]
        private DataEditor.DropdownReferenceHelper helper;

        public DataBlockOverworldPointCore () =>
            helper = new DataEditor.DropdownReferenceHelper (this);

        #endif
        #endregion
    }
    
    public class DataBlockOverworldPointCompletion
    {
        public bool destroyed = false;
        
        [DropdownReference]
        public List<IOverworldFunction> functionsGlobal;
        
        [DropdownReference]
        public List<IOverworldTargetedFunction> functionsBase;
        
        [DropdownReference]
        public List<IOverworldTargetedFunction> functionsTarget;
        
        #region Editor
        #if UNITY_EDITOR

        [ShowInInspector]
        private DataEditor.DropdownReferenceHelper helper;

        public DataBlockOverworldPointCompletion () =>
            helper = new DataEditor.DropdownReferenceHelper (this);

        #endif
        #endregion
    }

    /*
    [HideReferenceObjectPicker, HideDuplicateReferenceBox]
    public class DataBlockOverworldPointGenerationCondition
    {
        public string tagChecked;
        
        [DropdownReference (true), InlineProperty, LabelWidth (280f)]
        public DataBlockInt spawnLimit;

        [DropdownReference (true), InlineProperty, LabelWidth (280f)]
        public DataBlockInt instanceLimit;

        [DropdownReference (true), InlineProperty, LabelWidth (280f)]
        public DataBlockInt completionLimit;

        [DropdownReference (true), InlineProperty, LabelWidth (280f)]
        public DataBlockInt completionSeparation;
        
        #region Editor
        #if UNITY_EDITOR

        [ShowInInspector]
        private DataEditor.DropdownReferenceHelper helper;

        public DataBlockOverworldPointGenerationCondition () =>
            helper = new DataEditor.DropdownReferenceHelper (this);

        #endif
        #endregion
    }
    */
    
    public class DataBlockOverworldPointEffect
    {
        [DropdownReference]
        public List<IOverworldFunction> functionsGlobal;
        
        [DropdownReference]
        public List<IOverworldTargetedFunction> functionsBase;
        
        [DropdownReference]
        public List<IOverworldTargetedFunction> functionsSelf;
        
        #region Editor
        #if UNITY_EDITOR

        [ShowInInspector]
        private DataEditor.DropdownReferenceHelper helper;

        public DataBlockOverworldPointEffect () =>
            helper = new DataEditor.DropdownReferenceHelper (this);

        #endif
        #endregion
    }

    public class DataBlockOverworldPointSpawnPrefs
    {
        public string groupOverride;
        public bool groupReusable;
        public bool rangeIgnored;
        [HideIf ("rangeIgnored")]
        public DataBlockVector2 rangeOverride;

        public PointDistancePriority distancePriority = PointDistancePriority.None;
    }

    public class DataBlockOverworldPointGeneration
    {
        [DropdownReference (true)]
        public DataBlockOverworldPointSpawnPrefs spawnPrefs;

        // [DropdownReference, ListDrawerSettings (CustomAddFunction = ("@new DataBlockOverworldPointGenerationCondition ()"))] 
        // public List<DataBlockOverworldPointGenerationCondition> conditions;
        
        // TODO: Consider limits and separations by tags rather than just the specific key of this preset
        // E.g. public HashSet<string> spawnLimitByTag;
        // With this, you'd e.g. be able to define base_01 and base_02 and have them subject to shared restrictions
        
        [DropdownReference]
        public List<IOverworldEntityValidationFunction> checksBase;
        
        [DropdownReference]
        public List<IOverworldTargetedFunction> functionsOnSpawn;
        
        #region Editor
        #if UNITY_EDITOR

        [ShowInInspector]
        private DataEditor.DropdownReferenceHelper helper;

        public DataBlockOverworldPointGeneration () =>
            helper = new DataEditor.DropdownReferenceHelper (this);

        #endif
        #endregion
    }

    public enum PointRewardType
    {
        Hidden,
        CombatVictory,
        CombatConditional,
    }
    
    public class DataBlockOverworldPointRewardGroup
    {
        public PointRewardType type = PointRewardType.CombatVictory;
        
        public bool collapsed = true;

        [ListDrawerSettings (DefaultExpandedState = true, ShowPaging = false, CustomAddFunction = "@new DataBlockOverworldPointReward ()")]
        public List<DataBlockOverworldPointReward> rewards = new List<DataBlockOverworldPointReward> ();
        
        public void Refresh ()
        {
            if (rewards != null)
            {
                foreach (var reward in rewards)
                {
                    if (reward != null)
                        reward.Refresh ();
                }
            }
        }
        
        #region Editor
        #if UNITY_EDITOR

        [ShowInInspector]
        private DataEditor.DropdownReferenceHelper helper;

        public DataBlockOverworldPointRewardGroup () =>
            helper = new DataEditor.DropdownReferenceHelper (this);

        #endif
        #endregion
    }
    
    [HideReferenceObjectPicker]
    public class DataBlockOverworldPointReward : DataBlockFilterLinked<DataContainerOverworldReward>
    {
        [PropertyOrder (-1)]
        [PropertyRange (0f, 1f)]
        public float chance = 1f;
        
        [PropertyOrder (-1)]
        [PropertyRange (1, 8)]
        public int repeats = 1;

        public override IEnumerable<string> GetTags () => DataMultiLinkerOverworldReward.GetTags ();
        public override SortedDictionary<string, DataContainerOverworldReward> GetData () => DataMultiLinkerOverworldReward.data;
    }

    public class DataBlockOverworldPointCombat
    {
        [ValueDropdown ("@GetScenarioKeys")]
        [DropdownReference]
        public List<string> scenarioKeys;

        [DropdownReference (true)]
        public DataBlockOverworldEntityScenarios scenarioFilter;
        
        [DropdownReference (true)]
        public DataBlockOverworldEntityScenarioChanges scenarioChanges;
        
        [ValueDropdown ("@GetAreaKeys")]
        [DropdownReference]
        public List<string> areaKeys;

        [DropdownReference (true)]
        public DataBlockOverworldEntityAreas areaFilter;
        
        [DropdownReference (true)]
        public DataBlockOverworldPointCompletion completion;
        
        [DropdownReference (true)]
        public DataBlockFloat salvageBudget;
        
        [DropdownReference (true)]
        public SortedDictionary<string, DataBlockOverworldPointRewardGroup> rewards;

        #region Editor
        #if UNITY_EDITOR

        [ShowInInspector]
        private DataEditor.DropdownReferenceHelper helper;

        public DataBlockOverworldPointCombat () =>
            helper = new DataEditor.DropdownReferenceHelper (this);
        
        private IEnumerable<string> GetScenarioKeys => DataMultiLinkerScenario.GetKeys ();
        private IEnumerable<string> GetAreaKeys => DataMultiLinkerCombatArea.GetKeys ();

        #endif
        #endregion
    }

    [LabelWidth (160f)]
    public class DataContainerOverworldPointPreset : DataContainerWithText, IDataContainerTagged
    {
        [DropdownReference, HideLabel] 
        public DataBlockComment comment;
        
        [ToggleLeft]
        public bool hidden;
        
        [DropdownReference]
        [ListDrawerSettings (DefaultExpandedState = true, ShowPaging = false, CustomAddFunction = "@new DataBlockOverworldPointParent ()")]
        public List<DataBlockOverworldPointParent> parents;
        
        [YamlIgnore, ReadOnly]
        [ShowIf ("@children != null && children.Count > 0")]
        [ListDrawerSettings (DefaultExpandedState = false)]
        public List<string> children;
        
        
        [DropdownReference (true)]
        public DataBlockOverworldPointCore core;
        
        [ShowIf ("IsInheritanceVisible")]
        [YamlIgnore, ReadOnly]
        public DataBlockOverworldPointCore coreProc;
        
        
        [DropdownReference]
        public HashSet<string> tags;
        
        [ShowIf ("IsInheritanceVisible")]
        [YamlIgnore, ReadOnly]
        public HashSet<string> tagsProc;

        
        [DropdownReference (true)]
        public DataBlockOverworldPointGeneration generation;
        
        [ShowIf ("IsInheritanceVisible")]
        [YamlIgnore, ReadOnly]
        public DataBlockOverworldPointGeneration generationProc;

        
        [DropdownReference (true)]
        public DataBlockOverworldPointInteraction interaction;
        
        [ShowIf ("IsInheritanceVisible")]
        [YamlIgnore, ReadOnly]
        public DataBlockOverworldPointInteraction interactionProc;
        
        
        [DropdownReference (true)]
        public SortedDictionary<PointEventType, DataBlockOverworldPointEffect> effectsOnEvents;
        
        [ShowIf ("IsInheritanceVisible")]
        [YamlIgnore, ReadOnly]
        public SortedDictionary<PointEventType, DataBlockOverworldPointEffect> effectsOnEventsProc;
        
        
        [DropdownReference (true)]
        public DataBlockOverworldPointCombat combat;
        
        [ShowIf ("IsInheritanceVisible")]
        [YamlIgnore, ReadOnly]
        public DataBlockOverworldPointCombat combatProc;
        
        
        public bool IsHidden () => hidden;
        
        public HashSet<string> GetTags (bool processed) => 
            processed ? tagsProc : tags;

        public bool IsTagPresent (string tag)
        {
            if (tagsProc == null)
                return false;

            if (string.IsNullOrEmpty (tag))
                return false;
            
            return tagsProc.Contains (tag);
        }
        
        public override void OnBeforeSerialization ()
        {
            base.OnBeforeSerialization ();
            
            if (core != null)
            {
                if (core.visual != null)
                    core.visual.OnBeforeSerialization ();
                
                if (core.visualLiberated != null)
                    core.visualLiberated.OnBeforeSerialization ();
            }
        }

        public override void OnAfterDeserialization (string key)
        {
            base.OnAfterDeserialization (key);

            if (core != null)
            {
                if (core.visual != null)
                    core.visual.OnAfterDeserialization (key);
                
                if (core.visualLiberated != null)
                    core.visualLiberated.OnAfterDeserialization (key);
            }

            if (combat != null)
            {
                if (combat.scenarioFilter != null)
                    combat.scenarioFilter.Refresh ();
                
                if (combat.areaFilter != null)
                    combat.areaFilter.Refresh ();

                if (combat.rewards != null)
                {
                    foreach (var kvp in combat.rewards)
                    {
                        if (kvp.Value != null)
                            kvp.Value.Refresh ();
                    }
                }
            }
        }

        public override void OnKeyReplacement (string keyOld, string keyNew)
        {
            base.OnKeyReplacement (keyOld, keyNew);
            
            foreach (var kvp in DataMultiLinkerOverworldPointPreset.data)
            {
                var entry = kvp.Value;
                if (entry.parents != null)
                {
                    for (int i = 0; i < entry.parents.Count; ++i)
                    {
                        var parent = entry.parents[i];
                        if (parent != null && parent.key == keyOld)
                        {
                            Debug.LogWarning ($"Point preset {kvp.Key}, parent block {i} | Replacing entity key: {keyOld} -> {keyNew})");
                            parent.key = keyNew;
                        }
                    }
                }
            }
        }
        
        public override void ResolveText ()
        {
            if (core != null)
            {
                if (core.textName != null)
                    core.textName.s = DataManagerText.GetText (TextLibs.overworldPoints, $"{key}__name");
            
                if (core.textDesc != null)
                    core.textDesc.s = DataManagerText.GetText (TextLibs.overworldPoints, $"{key}__text");
            }
        }

        public float GetVisionRange ()
        {
            return coreProc?.rangeVision != null ? coreProc.rangeVision.f : 100f;
        }
        
        public float GetInteractionRange ()
        {
            return coreProc?.rangeInteraction != null ? coreProc.rangeInteraction.f : 5f;
        }
        
        #region Editor
        #if UNITY_EDITOR

        [ShowInInspector]
        private DataEditor.DropdownReferenceHelper helper;

        public DataContainerOverworldPointPreset () =>
            helper = new DataEditor.DropdownReferenceHelper (this);
        
        private static bool IsInheritanceVisible => DataMultiLinkerOverworldPointPreset.Presentation.showInheritance;
        
        public override void SaveText ()
        {
            if (!IsTextSavingPossible ())
                return;
            
            if (core != null)
            {
                if (core.textName != null)
                    DataManagerText.TryAddingTextToLibrary (TextLibs.overworldPoints, $"{key}__name", core.textName.s);
            
                if (core.textDesc != null)
                    DataManagerText.TryAddingTextToLibrary (TextLibs.overworldPoints, $"{key}__text", core.textDesc.s);
            }
        }

        #endif
        #endregion
    }
}

