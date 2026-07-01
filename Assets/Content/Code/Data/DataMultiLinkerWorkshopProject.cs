using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace PhantomBrigade.Data
{
    [ExecuteInEditMode]
    public class DataMultiLinkerWorkshopProject : DataMultiLinker<DataContainerWorkshopProject>
    {
        public DataMultiLinkerWorkshopProject ()
        {
            textSectorKeys = new List<string> { TextLibs.workshopEmbedded };
            DataMultiLinkerUtility.RegisterOnAfterDeserialization (dataType, OnAfterDeserialization);
            DataMultiLinkerUtility.RegisterOnTextExport 
            (
                dataType, 
                () => TextLibraryHelper.OnBeforeTextExport (dataType, TextLibs.workshopEmbedded),
                () => TextLibraryHelper.OnAfterTextExport (dataType, TextLibs.workshopEmbedded)
            );
        }
        
        [HideReferenceObjectPicker]
        public class Presentation
        {
            [ShowInInspector, LabelText ("Show Core/UI")]
            public static bool showCore = true;
            
            [ShowInInspector, LabelText ("Show Text Preview")]
            public static bool showTextPreview = false;
            
            [ShowInInspector, LabelText ("Show Inputs/Outputs")]
            public static bool showInputsOutputs = true;

            [ShowInInspector]
            public static bool showTags = true;
            
            [ShowInInspector]
            public static bool showTagCollections = false;
        }

        [ShowInInspector, HideLabel, FoldoutGroup ("View options")]
        public Presentation presentation = new Presentation ();
        
        [ShowIf ("@DataMultiLinkerWorkshopProject.Presentation.showTagCollections")]
        [ShowInInspector] //, ReadOnly]
        public static HashSet<string> tags = new HashSet<string> ();
        
        [ShowIf ("@DataMultiLinkerWorkshopProject.Presentation.showTagCollections")]
        [ShowInInspector, ReadOnly]
        public static Dictionary<string, HashSet<string>> tagsMap = new Dictionary<string, HashSet<string>> ();
        
        public static HashSet<string> GetTags ()
        {
            LoadDataChecked ();
            return tags;
        }
        
        public static void OnAfterDeserialization ()
        {
            DataTagUtility.RegisterTags (data, ref tags, ref tagsMap);
            
            #if UNITY_EDITOR
            
            var textKeysShared = DataManagerText.GetLibraryTextKeys (TextLibs.workshopShared);
            var textKeySuffixHeader = "_header";
            var textKeySuffixDescription = "_text";
            var textKeySuffixSubtitle = "_sub";
            
            textKeysSharedHeaders.Clear ();
            textKeysSharedSubtitles.Clear ();
            textKeysSharedDescriptions.Clear ();
            
            if (textKeysShared != null)
            {
                foreach (var key in textKeysShared)
                {
                    if (key.EndsWith (textKeySuffixHeader))
                        textKeysSharedHeaders.Add (key);
                    else if (key.EndsWith (textKeySuffixSubtitle))
                        textKeysSharedSubtitles.Add (key);
                    else if (key.EndsWith (textKeySuffixDescription))
                        textKeysSharedDescriptions.Add (key);
                }
            }
            
            #endif
        }
        
        public static List<string> textKeysSharedHeaders = new List<string> ();
        public static List<string> textKeysSharedSubtitles = new List<string> ();
        public static List<string> textKeysSharedDescriptions = new List<string> ();
        
        #if UNITY_EDITOR && !PB_MODSDK
        
        [FoldoutGroup ("Utilities", false), PropertyOrder (-20)]
        [LabelText ("Project")]
        public string utilityFilterProjects = string.Empty;

        [HideInEditorMode]
        [FoldoutGroup ("Utilities", false), PropertyOrder (-20)]
        [Button]
        public void IssueResources ()
        {
            var playerBase = IDUtility.playerBasePersistent;
            if (playerBase == null || !playerBase.hasInventoryWorkshopCharges)
                return;
            
            var resources = playerBase.inventoryResources.s;
            bool filterProjects = !string.IsNullOrEmpty (utilityFilterProjects);

            foreach (var kvp in data)
            {
                if (filterProjects && !kvp.Key.Contains (filter))
                    continue;

                var c = kvp.Value;
                if (c == null || c.inputResources == null || c.inputResources.Count == 0)
                    continue;

                foreach (var block in c.inputResources)
                {
                    if (resources.ContainsKey (block.key))
                        resources[block.key] += block.amount;
                    else
                        resources.Add (block.key, block.amount);
                }
            }
            
            if (CIViewBaseWorkshopV2.ins != null && CIViewBaseWorkshopV2.ins.IsEntered ())
                CIViewBaseWorkshopV2.ins.RefreshList ();
        }
        
        [HideInEditorMode]
        [FoldoutGroup ("Utilities", false), PropertyOrder (-20)]
        [Button]
        public void IssueCharges ()
        {
            var playerBase = IDUtility.playerBasePersistent;
            if (playerBase == null || !playerBase.hasInventoryWorkshopCharges)
                return;
            
            var playerCharges = playerBase.inventoryWorkshopCharges.s;
            bool filterProjects = !string.IsNullOrEmpty (utilityFilterProjects);
            
            foreach (var kvp in data)
            {
                if (filterProjects && !kvp.Key.Contains (filter))
                    continue;
                
                var c = kvp.Value;
                if (c == null)
                    continue;

                var key = kvp.Key;
                if (playerCharges.ContainsKey (key))
                    playerCharges[key] += 1;
                else
                    playerCharges.Add (key, 1);
            }
            
            if (CIViewBaseWorkshopV2.ins != null && CIViewBaseWorkshopV2.ins.IsEntered ())
                CIViewBaseWorkshopV2.ins.RefreshList ();
        }

        #endif
    }
}


