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
        
        [FoldoutGroup ("Utilities", false), PropertyOrder (-20)]
        [Button]
        public void AddChargeRequirementToFiltered (int count)
        {
            foreach (var kvp in data)
            {
                if (!string.IsNullOrEmpty (filter) && !kvp.Key.Contains (filter))
                    continue;
                
                var c = kvp.Value;
                if (c == null)
                    continue;

                c.unlockCharges = new DataBlockInt { i = Mathf.Max (1, count) };
            }
        }
        
        [FoldoutGroup ("Utilities", false), PropertyOrder (-20)]
        [Button]
        public void AddWeightTags ()
        {
            foreach (var kvp in data)
            {
                if (!string.IsNullOrEmpty (filter) && !kvp.Key.Contains (filter))
                    continue;
                
                var c = kvp.Value;
                if (c == null)
                    continue;

                if (!c.key.StartsWith ("prt_body_"))
                    continue;

                if (c.key.EndsWith ("_h"))
                    c.tags.Add ("weight_heavy");
                else if (c.key.EndsWith ("_m"))
                    c.tags.Add ("weight_medium");
                else if (c.key.EndsWith ("_l"))
                    c.tags.Add ("weight_light");
            }
        }
        
        [FoldoutGroup ("Utilities", false), PropertyOrder (-20)]
        [Button]
        public void AddFactionTagToFiltered ([ValueDropdown ("@DataMultiLinkerOverworldFactionBranch.GetKeys ()")] string branchKey)
        {
            foreach (var kvp in data)
            {
                if (!string.IsNullOrEmpty (filter) && !kvp.Key.Contains (filter))
                    continue;
                
                var c = kvp.Value;
                if (c == null)
                    continue;

                if (c.tags == null)
                    c.tags = new HashSet<string> ();

                if (!c.tags.Contains (EquipmentUtility.workshopTagFactionExclusive))
                {
                    c.tags.Add (EquipmentUtility.workshopTagFactionExclusive);
                }

                if (!c.tags.Contains (branchKey))
                {
                    Debug.Log ($"{c.key}: added tag {branchKey}");
                    c.tags.Add (branchKey);
                }
            }
        }

        /*
        [FoldoutGroup ("Utilities", false), PropertyOrder (-2)]
        [Button]
        public void CheckOutputDoubling ()
        {
            foreach (var kvp in data)
            {
                var c = kvp.Value;
                if (c.outputParts != null)
                {
                    foreach (var block in c.outputParts)
                    {
                        bool keyPresent = !string.IsNullOrEmpty (block.key);
                        var tagsPresent = block.tagsUsed || block.tags != null;
                        if (keyPresent && tagsPresent)
                        {
                            Debug.LogWarning ($"{kvp.Key} | Double output data | Key: {block.key} | Tags used: {block.tagsUsed} | Tags:\n{block.tags.ToStringFormattedKeyValuePairs (true, multilinePrefix: "-")}");
                        }
                    }
                }
            }
        }
        */

        [FoldoutGroup ("Utilities", false), PropertyOrder (-2)]
        [Button]
        public void CombineBodyProjectRatings () => CombineProjects ("prt_body", "prt_body_ratings_01");
        
        [FoldoutGroup ("Utilities", false), PropertyOrder (-2)]
        [Button]
        public void CombineItemProjectRatings () => CombineProjects ("prt_item", "prt_item_ratings_01");
        
        private void CombineProjects (string projectKeyStart, string variantKeySecondary)
        {
            var entriesToAdd = new Dictionary<string, DataContainerWorkshopProject> ();
            
            foreach (var kvp in data)
            {
                var projectKeyRating1 = kvp.Key;
                if (!projectKeyRating1.StartsWith (projectKeyStart) || !projectKeyRating1.EndsWith ("_r1"))
                    continue;

                var projectKeyRoot = projectKeyRating1.Replace ("_r1", "");
                if (data.ContainsKey (projectKeyRoot))
                {
                    Debug.LogWarning ($"Skipping root {projectKeyRoot} | Same key already registered");
                    continue;
                }
                
                var projectKeyRating2 = $"{projectKeyRoot}_r2";
                var projectKeyRating3 = $"{projectKeyRoot}_r3";

                var projectRating1 = kvp.Value;
                var projectRating2 = GetEntry (projectKeyRating2, false);
                var projectRating3 = GetEntry (projectKeyRating3, false);

                if (projectRating2 == null || projectRating3 == null)
                {
                    Debug.LogWarning ($"Skipping root {projectKeyRoot} | R2: {projectRating2.ToStringNullCheck ()} | R3: {projectRating3.ToStringNullCheck ()}");
                    continue;
                }

                var projectNew = new DataContainerWorkshopProject ();
                entriesToAdd.Add (projectKeyRoot, projectNew);
                
                projectNew.key = projectKeyRoot;
                projectNew.icon = projectRating1.icon;
                projectNew.textName = projectRating1.textName;
                projectNew.textDesc = projectRating1.textDesc;

                projectNew.textSourceName = projectRating1.textSourceName;
                projectNew.textSourceDesc = projectRating1.textSourceDesc;
                projectNew.textSourceSubtitle = projectRating1.textSourceSubtitle;

                if (projectRating1.tags != null)
                {
                    projectNew.tags = new HashSet<string> ();
                    foreach (var tag in projectRating1.tags)
                    {
                        if (!tag.Contains ("rating"))
                            projectNew.tags.Add (tag);
                    }
                }

                projectNew.duration = new DataBlockFloat { f = 1f };
                projectNew.inputCharges = new DataBlockInt { i = 1 };

                projectNew.inputResources = new List<DataBlockResourceCost>
                {
                    new DataBlockResourceCost { key = "supplies", amount = 1 },
                    new DataBlockResourceCost { key = "components_r2", amount = 1 },
                    new DataBlockResourceCost { key = "components_r3", amount = 1 },
                };

                if (projectRating1.outputParts != null)
                {
                    projectNew.outputParts = new List<DataBlockWorkshopPart> ();
                    foreach (var outputPart in projectRating1.outputParts)
                    {
                        var outputPartNew = new DataBlockWorkshopPart ();
                        projectNew.outputParts.Add (outputPartNew);
                        
                        outputPartNew.count = outputPart.count;
                        outputPartNew.key = outputPart.key;

                        if (outputPart.tags != null)
                        {
                            outputPartNew.tags = new SortedDictionary<string, bool> ();
                            foreach (var kvp2 in outputPart.tags)
                            {
                                if (!kvp2.Key.Contains ("rating"))
                                    outputPartNew.tags.Add (kvp2.Key, kvp2.Value);
                            }
                        }
                    }
                }

                projectNew.variantLinkPrimary = projectRating1.variantLinkPrimary;
                projectNew.variantLinkSecondary = new DataBlockWorkshopVariantLink { key = variantKeySecondary };
            }

            foreach (var kvp in entriesToAdd)
                data.Add (kvp.Key, kvp.Value);
        }
        
        [FoldoutGroup ("Utilities", false), PropertyOrder (-2)]
        [Button]
        public void UpgradeHistoryFromRatingOne ()
        {
            var history = DataLinkerHistory.data;
            KeyReplacementGroup group;
            if (!history.keyReplacementGroups.ContainsKey (dataType.Name))
            {
                group = new KeyReplacementGroup { keyReplacements = new List<KeyReplacement> () };
                history.keyReplacementGroups.Add (dataType.Name, group);
            }
            else
                group = history.keyReplacementGroups[dataType.Name];
            
            foreach (var kvp in data)
            {
                var key = kvp.Key;
                if (key.EndsWith ("_r1") || (!key.StartsWith ("prt_body") && !key.StartsWith ("prt_item")))
                    continue;

                var keyExtended = key + "_r1";
                group.keyReplacements.Add (new KeyReplacement { keyOld = keyExtended, keyNew = key });
            }
        }
        
        /*
        [FoldoutGroup ("Utilities", false), PropertyOrder (-2)]
        [Button]
        public void FixAuxProjects ()
        {
            foreach (var kvp in data)
            {
                var key = kvp.Key;
                if (!key.Contains ("aux"))
                    continue;

                var c = kvp.Value;
                if (!string.IsNullOrEmpty (c.textFromSubsystem))
                {
                    Debug.Log ($"{key}: text from subsystem {c.textFromSubsystem}");
                }

                if (!string.IsNullOrEmpty (c.textFromGroup))
                {
                    string subsystemKey = null;
                    if (c.outputSubsystems != null)
                    {
                        foreach (var o in c.outputSubsystems)
                        {
                            if (o.tagsUsed && o.tags != null)
                                subsystemKey = o.tags.Keys.First ();
                            else
                                subsystemKey = o.key;
                        }
                    }

                    var subsystem = DataMultiLinkerSubsystem.GetEntry (subsystemKey, false);
                    bool subsystemFound = subsystem != null;
                    Debug.LogWarning ($"{key}: text from group {c.textFromGroup} | Replacement subsystem key generated: {subsystemKey} | Replacement found: {subsystemFound}");

                    if (subsystemFound)
                    {
                        c.textFromSubsystem = subsystemKey;
                        c.textFromGroup = string.Empty;
                    }
                }
            }
        }
        
        [FoldoutGroup ("Utilities", false), PropertyOrder (-2)]
        [Button]
        public void FixWorkshopText ()
        {
            foreach (var kvp in data)
            {
                var c = kvp.Value;

                if (!string.IsNullOrEmpty (c.textFromSubsystem))
                {
                    c.textSourceName = new DataBlockWorkshopTextSourceName { key = c.textFromSubsystem, source = WorkshopTextSource.Subsystem };

                    var s = DataMultiLinkerSubsystem.GetEntry (c.textSourceName.key);
                    var hp = s.hardpointsProcessed.FirstOrDefault ();
            
                    c.textSourceSubtitle = new DataBlockWorkshopTextSourceSubtitle { key = hp, source = WorkshopTextSource.Hardpoint };
                    c.textSourceDesc = new DataBlockWorkshopTextSourceDesc { key = hp, source = WorkshopTextSource.Hardpoint };
                }
                
                else if (!string.IsNullOrEmpty (c.textFromPartPreset))
                {
                    c.textSourceName = new DataBlockWorkshopTextSourceName { key = c.textFromPartPreset, source = WorkshopTextSource.Part };

                    var p = DataMultiLinkerPartPreset.GetEntry (c.textSourceName.key);
                    var g = p.groupMainKey;
            
                    c.textSourceSubtitle = new DataBlockWorkshopTextSourceSubtitle { key = g, source = WorkshopTextSource.Group };
                    c.textSourceDesc = new DataBlockWorkshopTextSourceDesc { key = c.textFromPartPreset, source = WorkshopTextSource.Part };
                }
                
                else if (!string.IsNullOrEmpty (c.textFromGroup))
                {
                    c.textSourceName = new DataBlockWorkshopTextSourceName { key = c.textFromGroup, source = WorkshopTextSource.Group };
                    c.textSourceSubtitle = new DataBlockWorkshopTextSourceSubtitle { key = c.textFromGroup, source = WorkshopTextSource.Group };
                    c.textSourceDesc = new DataBlockWorkshopTextSourceDesc { key = c.textFromGroup, source = WorkshopTextSource.Group };
                }
                
                else if (!string.IsNullOrEmpty (c.textFromHardpoint))
                {
                    c.textSourceName = new DataBlockWorkshopTextSourceName { key = c.textFromHardpoint, source = WorkshopTextSource.Hardpoint };
                    c.textSourceSubtitle = new DataBlockWorkshopTextSourceSubtitle { key = c.textFromHardpoint, source = WorkshopTextSource.Hardpoint };
                    c.textSourceDesc = new DataBlockWorkshopTextSourceDesc { key = c.textFromHardpoint, source = WorkshopTextSource.Hardpoint };
                }

                if (!string.IsNullOrEmpty (c.textSubtitleKey) && c.textSourceSubtitle == null)
                    c.textSourceSubtitle = new DataBlockWorkshopTextSourceSubtitle { key = c.textSubtitleKey, source = WorkshopTextSource.Shared };
                
                if (!string.IsNullOrEmpty (c.textDescKey) && c.textSourceDesc == null)
                    c.textSourceDesc = new DataBlockWorkshopTextSourceDesc { key = c.textDescKey, source = WorkshopTextSource.Shared };
            }
        }
        */

        [FoldoutGroup ("Utilities", false), PropertyOrder (-2)]
        [Button]
        public void FixArmorText ()
        {
            foreach (var kvp in data)
            {
                var c = kvp.Value;
                if (!c.key.Contains ("prt_body"))
                    continue;

                c.textName = string.Empty;
                var root = c.key.Replace ("prt_body_", string.Empty);
                var partKey1 = $"body_set_{root}";
                var partKey2 = $"body_set_{root}_top";

                c.textSourceName = new DataBlockWorkshopTextSourceName { key = partKey1, source = WorkshopTextSource.Part };
                c.textSourceDesc = new DataBlockWorkshopTextSourceDesc { key = partKey2, source = WorkshopTextSource.Part };
                c.textSourceSubtitle = new DataBlockWorkshopTextSourceSubtitle { key = "body_sub", source = WorkshopTextSource.Shared };
            }
        }

        #endif
    }
}


