using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using YamlDotNet.Serialization;

namespace PhantomBrigade.Data
{
    [Serializable, HideReferenceObjectPicker, LabelWidth (160f)]
    public class DataContainerPilotPersonality : DataContainerWithText
    {
        [LabelText ("Name")][YamlIgnore]
        public string textName;
    
        [YamlIgnore, HideReferenceObjectPicker, OnValueChanged ("OnBeforeSerialization")]
        [InlineButton ("OnBeforeSerialization", "Update path")]
        [DictionaryKeyDropdown ("@PilotView.bodyKeys")]
        public Dictionary<string, RuntimeAnimatorController> overrideControllers = new Dictionary<string, RuntimeAnimatorController> ();

        [ReadOnly]
        public Dictionary<string, string> overrideControllerPaths = new Dictionary<string, string> ();

        [LabelText ("Block On Players")]
        public bool excludeFromPlayerPilots;
        
        [LabelText ("Block On Enemies")]
        public bool excludeFromEnemyPilots;

        public int idlePersonalityIndex = 0;
        public int basePersonalityIndex = 0;

        // todo: mech animator params

        public override void OnBeforeSerialization ()
        {
            #if UNITY_EDITOR
            overrideControllerPaths.Clear ();
            
            foreach (var kvp in overrideControllers)
            {
                var fullPath = UnityEditor.AssetDatabase.GetAssetPath (kvp.Value);
                var extension = System.IO.Path.GetExtension (fullPath);

                fullPath = fullPath.ReplaceFirst ("Assets/Resources/", string.Empty);
                fullPath = fullPath.Substring (0, fullPath.Length - extension.Length);
                overrideControllerPaths[kvp.Key] = fullPath;
            }
            #endif
            
            base.OnBeforeSerialization ();
        }

        public override void OnAfterDeserialization (string key)
        {
            base.OnAfterDeserialization (key);

            foreach (var kvp in overrideControllerPaths)
            {
                overrideControllers[kvp.Key] = Resources.Load<RuntimeAnimatorController> (kvp.Value);
            }
        }
        
        public override void ResolveText ()
        {
            textName = DataManagerText.GetText (TextLibs.pilotPersonalities, $"{key}_name");
        }
        
        #if UNITY_EDITOR
        
        public override void SaveText ()
        {
            if (!IsTextSavingPossible ())
                return;

            DataManagerText.TryAddingTextToLibrary (TextLibs.pilotPersonalities, $"{key}_name", textName);
        }
        
        #endif
    }
}