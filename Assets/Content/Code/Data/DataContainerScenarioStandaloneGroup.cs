using System;
using System.Collections.Generic;
using System.Text;
using Sirenix.OdinInspector;
using UnityEngine;
using YamlDotNet.Serialization;

namespace PhantomBrigade.Data
{
    public class DataContainerScenarioStandaloneGroup : DataContainerWithText
    {
        // [ValueDropdown ("@DataMultiLinkerScenarioGroup.missionLinkGroups")][InlineButtonClear]
        // public string group;

        public bool hidden;
        public bool generatorGroup;
        
        public int priority;
        
        [PropertyRange (0f, 1f)]
        public float hue = 0.388f;
        
        [DataEditor.SpriteNameAttribute (true, 32f)]
        public string icon;
        
        [DropdownReference (true)]
        [DataEditor.SpriteNameAttribute (false, 48f)]
        public string pattern;

        [DropdownReference (true)]
        public DataBlockTextTrimodal textName;

        [DropdownReference (true)]
        public DataBlockInt generatorLimit;
        
        [YamlIgnore]
        [HideInInspector]
        public List<DataContainerScenario> scenariosSorted = new List<DataContainerScenario> ();
        
        [YamlIgnore]
        [HideInInspector]
        public List<DataContainerScenario> scenariosSortedVisible = new List<DataContainerScenario> ();
        
        /*
        [YamlIgnore]
        [HideInInspector]
        public List<DataContainerScenarioGenerator> generatorsSorted = new List<DataContainerScenarioGenerator> ();
        
        [YamlIgnore]
        [HideInInspector]
        public List<DataContainerScenarioGenerator> generatorsSortedVisible = new List<DataContainerScenarioGenerator> ();
        */
        
        [YamlIgnore, HideInInspector]
        private string report;
        
        private static StringBuilder sb = new StringBuilder ();

        public override void OnAfterDeserialization (string key)
        {
            base.OnAfterDeserialization (key);
            RefreshScenarios ();
        }

        public override void ResolveText ()
        {
            if (textName != null)
                textName.ResolveText (TextLibs.overworldMissionLinks, key + "__name");
        }

        [FoldoutGroup ("Debug", false)]
        [Button ("Refresh"), InfoBox ("$report", InfoMessageType.None)]
        public void RefreshScenarios ()
        {
            scenariosSorted.Clear ();
            scenariosSortedVisible.Clear ();
            // generatorsSorted.Clear ();
            // generatorsSortedVisible.Clear ();
            report = string.Empty;
            
            if (hidden)
                return;
            
            bool developerMode = DataShortcuts.debug.developerMode;
            sb.Clear ();

            if (generatorGroup)
            {
                /*
                var generators = DataMultiLinkerScenarioGenerator.GetDataList ();
                foreach (var generator in generators)
                {
                    if (generator == null || generator.hidden || generator.coreProc == null)
                        continue;
                    
                    if (!string.Equals (generator.coreProc.group, key, StringComparison.Ordinal))
                        continue;
            
                    generatorsSorted.Add (generator);
                }
        
                if (!Application.isPlaying)
                    sb.Append ("Tested outside play mode, conditions unchecked!\n");
            
                foreach (var generator in generatorsSorted)
                {
                    if (sb.Length > 0)
                        sb.Append ("\n");
                    sb.Append ("- ");
                    sb.Append (generator.key);
                
                    if (Application.isPlaying)
                    {
                        if (generator.checksGlobalProc != null)
                        {
                            bool valid = true;
                            foreach (var check in generator.checksGlobalProc)
                            {
                                if (check != null && !check.IsValid ())
                                {
                                    valid = false;
                                    break;
                                }
                            }

                            if (!valid)
                                continue;
                        }

                        if (generator.checksBaseProc != null)
                        {
                            var basePersistent = IDUtility.playerBasePersistent;
                            bool valid = true;
                            foreach (var check in generator.checksBaseProc)
                            {
                                if (check != null && !check.IsValid (basePersistent))
                                {
                                    valid = false;
                                    break;
                                }
                            }

                            if (!valid)
                                continue;
                        }
                    }
                    
                    generatorsSortedVisible.Add (generator);
                    sb.Append (" (vis.)");
                }

                if (generatorsSortedVisible.Count == 0)
                    sb.Append ("No matched generators!");
                */
            }
            else
            {
                var scenarios = DataMultiLinkerScenario.GetDataList ();
                foreach (var scenario in scenarios)
                {
                    if (scenario == null || scenario.hidden || scenario.standaloneProc == null)
                        continue;
                
                    if (!string.Equals (scenario.standaloneProc.group, key, StringComparison.Ordinal))
                        continue;
                
                    if (scenario.standaloneProc.developerOnly && !developerMode)
                        continue;
            
                    scenariosSorted.Add (scenario);
                }
        
                scenariosSorted.Sort ((x, y) =>
                    x.standaloneProc.priority.CompareTo (y.standaloneProc.priority));

                

                if (!Application.isPlaying)
                    sb.Append ("Tested outside play mode, conditions unchecked!\n");
            
                foreach (var scenario in scenariosSorted)
                {
                    if (sb.Length > 0)
                        sb.Append ("\n");
                    sb.Append ("- ");
                    sb.Append (scenario.key);
                
                    if (Application.isPlaying)
                    {
                        var st = scenario.standaloneProc;
                        bool visible = st.conditionVisible == null || st.conditionVisible.IsValid ();
                        if (visible)
                        {
                            scenariosSortedVisible.Add (scenario);
                            sb.Append (" (vis.)");
                        }
                    }
                }

                if (scenariosSorted.Count == 0)
                    sb.Append ("No matched scenarios!");
            }
            
            report = sb.ToString ();
        }

        #if UNITY_EDITOR 

        public override void SaveText ()
        {
            if (!IsTextSavingPossible ())
                return;
            
            if (textName != null)
                textName.SaveText (TextLibs.overworldMissionLinks, key + "__name");
        }
        
        [ShowInInspector, PropertyOrder (100)]
        private DataEditor.DropdownReferenceHelper helper;
        
        public DataContainerScenarioStandaloneGroup () => 
            helper = new DataEditor.DropdownReferenceHelper (this);
        
        #endif
    }
}