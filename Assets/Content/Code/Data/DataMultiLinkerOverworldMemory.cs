using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PhantomBrigade.Data
{
    [ExecuteInEditMode]
    public class DataMultiLinkerOverworldMemory : DataMultiLinker<DataContainerOverworldMemory>
    {
        public DataMultiLinkerOverworldMemory ()
        {
            textSectorKeys = new List<string> { TextLibs.overworldMemories };
            DataMultiLinkerUtility.RegisterOnAfterDeserialization (dataType, OnAfterDeserialization);
            DataMultiLinkerUtility.RegisterOnTextExport 
            (
                dataType, 
                () => TextLibraryHelper.OnBeforeTextExport (dataType, TextLibs.overworldMemories),
                () => TextLibraryHelper.OnAfterTextExport (dataType, TextLibs.overworldMemories)
            );
        }

        [HideReferenceObjectPicker]
        public class Presentation
        {
            [ShowInInspector]
            public static bool showLookups = false;
        }

        [ShowInInspector, HideLabel, FoldoutGroup ("View options")]
        public Presentation presentation = new Presentation ();

        [ShowIf ("@DataMultiLinkerOverworldMemory.Presentation.showLookups")]
        [ShowInInspector]
        public static HashSet<string> keysInt = new HashSet<string> ();

        [ShowIf ("@DataMultiLinkerOverworldMemory.Presentation.showLookups")]
        [ShowInInspector, ReadOnly]
        public static HashSet<string> keysFloat = new HashSet<string> ();
        
        [ShowIf ("@DataMultiLinkerOverworldMemory.Presentation.showLookups")]
        [ShowInInspector, ReadOnly]
        public static HashSet<string> keysPersistedOnGeneration = new HashSet<string> ();
        
        [ShowIf ("@DataMultiLinkerOverworldMemory.Presentation.showLookups")]
        [ShowInInspector, ReadOnly]
        public static HashSet<string> keysDiscardedOnTravel = new HashSet<string> ();
        
        [ShowIf ("@DataMultiLinkerOverworldMemory.Presentation.showLookups")]
        [ShowInInspector, ReadOnly]
        public static Dictionary<string, string> keysOldToNew = new Dictionary<string, string> ();
        
        [ShowIf ("@DataMultiLinkerOverworldMemory.Presentation.showLookups")]
        [ShowInInspector, ReadOnly]
        [ListDrawerSettings (DefaultExpandedState = false)]
        public static List<string> keysEditableSorted = new List<string> ();

        public static void OnAfterDeserialization ()
        {
            keysFloat.Clear ();
            keysInt.Clear ();
            keysPersistedOnGeneration.Clear ();
            keysDiscardedOnTravel.Clear ();
            keysOldToNew.Clear ();
            keysEditableSorted.Clear ();

            foreach (var kvp in data)
            {
                var key = kvp.Key;
                var config = kvp.Value;

                if (config == null)
                    continue;
                
                keysEditableSorted.Add (key);

                if (config.type == OverworldMemoryType.Int)
                    keysInt.Add (key);
                
                else if (config.type == OverworldMemoryType.Float)
                    keysFloat.Add (key);

                if (!config.discardOnWorldChange) 
                    keysPersistedOnGeneration.Add (key);
                
                if (config.discardOnTravel) 
                    keysDiscardedOnTravel.Add (key);

                if (!string.IsNullOrEmpty (config.keyOld))
                {
                    if (!keysOldToNew.ContainsKey (config.keyOld))
                        keysOldToNew.Add (config.keyOld, key);
                    else
                        Debug.LogWarning ($"Detected a conflict in overworld memory key renaming history: old key {config.keyOld} is already registered from config {keysOldToNew[config.keyOld]}, but config {key} also claims the same old key");
                }
            }

            #if UNITY_EDITOR
            
            var keysFeatures = FieldReflectionUtility.GetConstantStringFieldValues (typeof (EventMemoryFeatureFlag));
            if (keysFeatures != null)
            {
                foreach (var key in keysFeatures)
                {
                    if (!keysEditableSorted.Contains (key))
                        keysEditableSorted.Add (key);
                }
            }
            
            #endif
            
            keysEditableSorted.Sort ();
        }

        [FoldoutGroup ("Utilities", false)]
        [Button]
        private static void LogPersistedOnWorldReset ()
        {
            var listPreserved = new List<string> ();
            var listDiscarded = new List<string> ();
            
            foreach (var kvp in data)
            {
                var memory = kvp.Value;
                var report = memory.ui != null && !string.IsNullOrEmpty (memory.ui.textName) ? $"{kvp.Key} ({memory.ui.textName})" : kvp.Key;
                
                if (memory.discardOnWorldChange)
                    listDiscarded.Add (report);
                else
                    listPreserved.Add (report);
            }
            
            Debug.Log ($"Preserved memories ({listPreserved.Count}):\n{listPreserved.ToStringFormatted (true, multilinePrefix: "- ")}");
            Debug.Log ($"Discarded memories ({listDiscarded.Count}):\n{listDiscarded.ToStringFormatted (true, multilinePrefix: "- ")}");
        }
    }
}