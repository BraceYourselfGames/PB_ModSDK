using System;
using Entitas;
using System.Collections.Generic;
using System.Reflection;
using PhantomBrigade.Data;
using PhantomBrigade.Functions;
using Sirenix.OdinInspector;
using UnityEngine;
using YamlDotNet.Serialization;

#if UNITY_EDITOR
using UnityEditor;
using Sirenix.Utilities;
using Sirenix.OdinInspector.Editor;
#endif

namespace PhantomBrigade.Data
{
    public static class UnitGroupDifficulty
    {
        public static string[] text = 
        {
            "Easy",
            "Medium",
            "Hard"
        };
        
        public static string[] chevrons = 
        {
            "›",
            "››",
            "›››"
        };
        
        // public static string[] chevrons = 
        // {
        //     "♦",
        //     "♦♦",
        //     "♦♦♦"
        // };
        
        public static Color[] inspectorColors = 
        {
            new Color (1f, 1f, 1f, 1f),
            new Color (0.85f, 1f, 0.976f, 1f),
            new Color (0.955f, 1f, 0.85f, 1f)
        };
        
        public static string[] tags = 
        {
            "grade_0",
            "grade_1",
            "grade_2"
        };
        
        public const string tagPrefix = "grade_";
    }
    
    public static class UnitEquipmentQuality
    {
        public static string[] text = 
        {
            "Training",
            "Common",
            "Uncommon",
            "Rare",
            "Legendary"
        };

        public static Color[] colors =
        {
            Color.gray,
            Color.white,
            new Color (0.53f, 0.83f, 0.53f, 1f), 
            new Color (0.53f, 0.53f, 0.83f, 1f),
            new Color (1f, 0.75f, 0.03f, 1f)
        };

        public const string tagPrefix = "rating_";
    }

    [Persistent]
    public sealed class DataKeyUnitGroup : IComponent
    {
        public string s;
    }

    [Serializable, HideReferenceObjectPicker]
    public class DataBlockUnitGroupGrade
    {
        [ReadOnly, YamlIgnore, LabelText ("Threat"), LabelWidth (120f), PropertyTooltip ("Threat Rating: Used to estimate how difficult this unit group is.")]
        public int threat = 0;
        
        [ValueDropdown ("@DataMultiLinkerUnitLiveryPreset.data.Keys")]
        [HorizontalGroup ("C"), LabelWidth (120f), OnValueChanged ("RefreshLiveryPresetPreview")]
        public string liveryPreset;
        
        [OnValueChanged ("RefreshLiveryPresetPreview")]
        [ShowIf ("@liveryPresetPreview != null")]
        [NonSerialized, YamlIgnore, ShowInInspector, HideLabel, HorizontalGroup ("C", EquipmentLiveryPreview.width)]
        [InlineButton ("RemoveLiveryPreset", "-")]
        public EquipmentLiveryPreview liveryPresetPreview;
        
        [ListDrawerSettings (AlwaysAddDefaultValue = true)]
        [OnValueChanged ("RefreshOnChanges", true)]
        public List<DataBlockScenarioUnitResolver> units = new List<DataBlockScenarioUnitResolver> ();
        
        [YamlIgnore, HideInInspector]
        public DataContainerCombatUnitGroup parentContainer;
        
        private void RefreshOnChanges ()
        {
            RefreshThreatRating ();
            RefreshUnitParentLink (parentContainer);
        }
        
        public void RefreshUnitParentLink (DataContainerCombatUnitGroup parentContainer)
        {
            this.parentContainer = parentContainer;
            if (units != null)
            {
                foreach (var resolver in units)
                {
                    if (resolver != null)
                        resolver.parentContainer = parentContainer;
                }
            }
        }
        
        public void RefreshLiveryPresetPreview ()
        {
            bool used = false;
            if (!string.IsNullOrEmpty (liveryPreset))
            {
                var lp = DataMultiLinkerUnitLiveryPreset.GetEntry (liveryPreset, false);
                if (lp != null)
                {
                    var liveryRoot = DataMultiLinkerEquipmentLivery.GetEntry (lp.livery, false);
                    if (liveryRoot != null)
                    {
                        used = true;
                        if (liveryPresetPreview == null) 
                            liveryPresetPreview = new EquipmentLiveryPreview ();
                        liveryPresetPreview.Refresh (lp.livery);
                        liveryPresetPreview.showSelectButton = false;
                    }
                }
            }

            if (!used && liveryPresetPreview != null)
                liveryPresetPreview = null;
        }
        
        private void RemoveLiveryPreset ()
        {
            liveryPreset = null;
            liveryPresetPreview = null;
        }
        
        public void RefreshThreatRating ()
        {
            threat = 0;
            if (units == null || parentContainer == null || parentContainer.unitPresets == null)
                return;

            foreach (var unitInstance in units)
            {
                if (unitInstance == null || string.IsNullOrEmpty (unitInstance.key) || !parentContainer.unitPresets.ContainsKey (unitInstance.key))
                    continue;

                var presetResolver = parentContainer.unitPresets[unitInstance.key];
                if (presetResolver == null)
                    continue;

                var unitCount = unitInstance.countRandom ? Mathf.RoundToInt ((unitInstance.countMin + unitInstance.countMax) * 0.5f) : unitInstance.countMin;
                var unitLevel = unitInstance.levelOffsetRandom ? Mathf.RoundToInt ((unitInstance.levelOffsetMin + unitInstance.levelOffsetMax) * 0.5f) : unitInstance.levelOffsetMin;

                var threatOffsetFromQuality = 0f; //Mathf.Max (0, unitInstance.quality - 1) * 0.25f;
                if(!string.IsNullOrEmpty(unitInstance.qualityTableKey) && DataMultiLinkerQualityTable.data.TryGetValue(unitInstance.qualityTableKey, out var qualityTable))
	                threatOffsetFromQuality = qualityTable.threatOffset;

                var threatFromUnits = (threatOffsetFromQuality + unitLevel * 0.25f + 1) * unitCount;

                if (presetResolver is DataBlockScenarioUnitFilter presetResolverFilter)
                {
                    var tags = presetResolverFilter.tags;
                    if (tags == null)
                        continue;
                    
                    if (tags.ContainsKey ("type_mech"))
                        threat += Mathf.RoundToInt (20 * threatFromUnits);
                    else if (tags.ContainsKey ("type_tank_elevated"))
                        threat += Mathf.RoundToInt (15 * threatFromUnits);
                    else if (tags.ContainsKey ("type_tank_standard"))
                        threat += Mathf.RoundToInt (10 * threatFromUnits);
                    else if (tags.ContainsKey ("type_turret"))
                        threat += Mathf.RoundToInt (5 * threatFromUnits);
                }

                if (presetResolver is DataBlockScenarioUnitPresetLink presetResolverLink)
                {
                    var preset = DataMultiLinkerUnitPreset.GetEntry (presetResolverLink.preset);
                    if (preset == null)
                        continue;
                    
                    if (preset.blueprintProcessed == UnitBlueprintKeys.mech)
                        threat += Mathf.RoundToInt (20 * threatFromUnits);
                    else if (preset.blueprintProcessed == UnitBlueprintKeys.tankElevated)
                        threat += Mathf.RoundToInt (15 * threatFromUnits);
                    else if (preset.blueprintProcessed == UnitBlueprintKeys.tankStandard)
                        threat += Mathf.RoundToInt (10 * threatFromUnits);
                    else if (preset.blueprintProcessed == UnitBlueprintKeys.turret)
                        threat += Mathf.RoundToInt (5 * threatFromUnits);
                }
            }
        }
    }
    
    [Serializable][LabelWidth (180f)]
    public class DataContainerCombatUnitGroup : DataContainerWithText, IDataContainerTagged
    {
        [DropdownReference, HideLabel] 
        public DataBlockComment comment;
        
        [OnInspectorGUI ("DrawHeaderGUI", false)]
        [ShowIf ("IsCoreVisible")]
        public bool hidden = false;
        
        [ShowIf ("IsCoreVisible")]
        public bool listed = false;
        
        [ShowIf ("IsTextVisible")]
        [LabelText ("Name / Desc"), YamlIgnore]
        public string textName;
        
        [ShowIf ("IsTextVisible")]
        [HideLabel, TextArea (1, 10), YamlIgnore]
        public string textDesc;
        
        public bool factionAllied = false;

        [DropdownReference]
        [ValueDropdown ("@DataMultiLinkerOverworldFactionBranch.data.Keys")]
        [LabelText ("Default Branch")]
        public string factionBranchDefault = string.Empty;

        [PropertySpace (4f)]
        [ShowIf ("AreTagsVisible")]
        [ValueDropdown ("@DataMultiLinkerCombatUnitGroup.tags")]
        public HashSet<string> tags = new HashSet<string> ();
        
        [ShowIf ("AreGradesVisible")]
        [ListDrawerSettings (AlwaysAddDefaultValue = true, ShowPaging = false, OnBeginListElementGUI = "DrawGradeEntryBeginGUI", OnEndListElementGUI = "DrawGradeEntryEndGUI", CustomAddFunction = "AddGrade")]
        [OnValueChanged ("OnGradeListChanged")]
        public List<DataBlockUnitGroupGrade> unitsPerGrade = new List<DataBlockUnitGroupGrade> ();

        [DropdownReference]
        [ShowIf ("AreUnitsVisible")]
        [DictionaryDrawerSettings (DisplayMode = DictionaryDisplayOptions.CollapsedFoldout)]
        public SortedDictionary<string, DataBlockScenarioUnit> unitPresets;

        public HashSet<string> GetTags (bool processed) => 
            tags;
        
        public bool IsHidden () => hidden;

        public override void OnAfterDeserialization (string key)
        {
            base.OnAfterDeserialization (key);
            RefreshUnitParentLink ();
            RefreshUnitPresets ();

            if (unitsPerGrade != null)
            {
                foreach (var unitsInGrade in unitsPerGrade)
                {
                    if (unitsInGrade == null)
                        continue;
                    
                    unitsInGrade.RefreshLiveryPresetPreview ();
                    unitsInGrade.RefreshThreatRating ();

                    if (unitsInGrade.units != null)
                    {
                        foreach (var unitInstance in unitsInGrade.units)
                        {
                            if (unitInstance != null)
                                unitInstance.OnAfterDeserialization ();
                        }
                    }
                }
            }
        }
        
        public override void OnBeforeSerialization ()
        {
            base.OnBeforeSerialization ();

            if (unitsPerGrade != null)
            {
                foreach (var unitsInGrade in unitsPerGrade)
                {
                    if (unitsInGrade == null)
                        continue;
                    
                    unitsInGrade.RefreshLiveryPresetPreview ();
                    unitsInGrade.RefreshThreatRating ();

                    if (unitsInGrade.units != null)
                    {
                        foreach (var unitInstance in unitsInGrade.units)
                        {
                            if (unitInstance != null)
                                unitInstance.OnBeforeSerialization ();
                        }
                    }
                }
            }
        }

        private void RefreshUnitParentLink ()
        {
            if (unitsPerGrade != null)
            {
                foreach (var entry in unitsPerGrade)
                {
                    if (entry == null)
                        continue;

                    entry.RefreshUnitParentLink (this);
                }
            }
        }
        
        [Button ("Discover presets, refresh usage stats")]
        private void RefreshUnitPresets ()
        {
            if (unitPresets == null)
                return;
            
            int presetEmbeddedIndex = 0;
            foreach (var kvp in unitPresets)
            {
                var presetResolver = kvp.Value;
                
                if (presetResolver is DataBlockScenarioUnitPresetEmbedded presetEmbedded)
                {
                    if (presetEmbedded.preset != null)
                    {
                        presetEmbedded.preset.OnAfterDeserialization ($"{key}_embedded_{presetEmbeddedIndex}");
                        presetEmbedded.preset.OnAfterDeserializationEmbedded ();
                        presetEmbeddedIndex += 1;
                    }
                }
                
                #if UNITY_EDITOR
                if (presetResolver is DataBlockScenarioUnitFilter presetFilter)
                {
                    presetFilter.Refresh (factionBranchDefault);

                    if (presetFilter.presetsFiltered != null)
                    {
                        foreach (var link in presetFilter.presetsFiltered)
                        {
                            var preset = DataMultiLinkerUnitPreset.GetEntry (link.key, false);
                            if (preset != null)
                                preset.OnUnitGroupFilterDetection (key);
                        }
                    }
                }
                else if (presetResolver is DataBlockScenarioUnitPresetLink presetLink)
                {
                    var unitPresetKey = presetLink.preset;
                    var unitPreset = DataMultiLinkerUnitPreset.GetEntry (unitPresetKey);
                    if (unitPreset != null)
                        unitPreset.OnUnitGroupLinkDetection (key);
                }
                #endif    
            }
        }
        
        public override void ResolveText ()
        {
            textName = DataManagerText.GetText (TextLibs.unitGroups, $"{key}__name", suppressWarning: true);
            textDesc = DataManagerText.GetText (TextLibs.unitGroups, $"{key}__text", suppressWarning: true);

            if (unitsPerGrade != null)
            {
                for (int g = 0; g < unitsPerGrade.Count; ++g)
                {
                    var block = unitsPerGrade[g];
                    if (block != null && block.units != null)
                    {
                        for (int u = 0; u < block.units.Count; ++u)
                        {
                            var resolver = block.units[u];
                            if (resolver == null || resolver.custom == null)
                                continue;
                            
                            if (resolver.custom.id != null)
                            {
                                var textKey = $"{key}__g{g}_u{u}_unit";
                                resolver.custom.id.nameOverride = DataManagerText.GetText (TextLibs.scenarioEmbedded, textKey, true);
                            }

                            if (resolver.custom.idPilot != null)
                            {
                                var textKeyCallsign = $"{key}__g{g}_u{u}_pilot_callsign";
                                resolver.custom.idPilot.callsignOverride = DataManagerText.GetText (TextLibs.scenarioEmbedded, textKeyCallsign, true);
                                            
                                var textKeyName = $"{key}__g{g}_u{u}_pilot_name";
                                resolver.custom.idPilot.nameOverride = DataManagerText.GetText (TextLibs.scenarioEmbedded, textKeyName, true);
                            }
                        }
                    }
                }
            }
        }

        #if UNITY_EDITOR
        
        [ShowInInspector]
        private DataEditor.DropdownReferenceHelper helper;
        
        public DataContainerCombatUnitGroup () => 
            helper = new DataEditor.DropdownReferenceHelper (this);
        
        public override void SaveText ()
        {
            if (!IsTextSavingPossible ())
                return;

            if (!string.IsNullOrEmpty (textName))
                DataManagerText.TryAddingTextToLibrary (TextLibs.unitGroups, $"{key}__name", textName);
            
            if (!string.IsNullOrEmpty (textDesc))
                DataManagerText.TryAddingTextToLibrary (TextLibs.unitGroups, $"{key}__text", textDesc);
            
            if (unitsPerGrade != null)
            {
                for (int g = 0; g < unitsPerGrade.Count; ++g)
                {
                    var block = unitsPerGrade[g];
                    if (block != null && block.units != null)
                    {
                        for (int u = 0; u < block.units.Count; ++u)
                        {
                            var resolver = block.units[u];
                            if (resolver == null || resolver.custom == null)
                                continue;
                            
                            if (resolver.custom.id != null)
                            {
                                var textKey = $"{key}__g{g}_u{u}_unit";
                                DataManagerText.TryAddingTextToLibrary (TextLibs.unitGroups, textKey, resolver.custom.id.nameOverride);
                            }

                            if (resolver.custom.idPilot != null)
                            {
                                var textKeyCallsign = $"{key}__g{g}_u{u}_pilot_callsign";
                                DataManagerText.TryAddingTextToLibrary (TextLibs.unitGroups, textKeyCallsign, resolver.custom.idPilot.callsignOverride);
                                            
                                var textKeyName = $"{key}__g{g}_u{u}_pilot_name";
                                DataManagerText.TryAddingTextToLibrary (TextLibs.unitGroups, textKeyName, resolver.custom.idPilot.nameOverride);
                            }
                        }
                    }
                }
            }
        }
        
        private static bool IsCoreVisible => DataMultiLinkerCombatUnitGroup.Presentation.showCore;
        private static bool IsTextVisible => DataMultiLinkerCombatUnitGroup.Presentation.showText;
        private static bool AreTagsVisible => DataMultiLinkerCombatUnitGroup.Presentation.showTags;
        private static bool AreGradesVisible => DataMultiLinkerCombatUnitGroup.Presentation.showGrades;
        private static bool AreUnitsVisible => DataMultiLinkerCombatUnitGroup.Presentation.showUnits;

        private static GUIStyle miniLabelStyled;

        private void DrawHeaderGUI ()
        {
            if (hidden)
                return;
            
            if (miniLabelStyled == null)
            {
                miniLabelStyled = new GUIStyle (EditorStyles.miniLabel);
                miniLabelStyled.richText = true;
            }
            
            var rect = UnityEditor.EditorGUILayout.BeginVertical ();
            GUILayout.Label (" ", GUILayout.Height (32));
            UnityEditor.EditorGUILayout.EndVertical ();
            var rectAligned = rect.AlignLeft (32).AddX (2);

            string textName = " ? ";
            string textDesc = "custom\nsquad";
            SdfIconType icon = SdfIconType.Triangle;
            Color col = Color.white.WithAlpha (0.1f);
            bool recognized = false;
            
            if (tags != null)
            {
                if (tags.Contains ("type_attacker"))
                {
                    recognized = true;
                    icon = SdfIconType.PlusSquare;
                    textName = "ATK";
                    textDesc = "••<color=black>•</color>  medium weight\n••<color=black>•</color>  medium range";
                    col = Color.HSVToRGB (0f, 0.45f, 0.5f).WithAlpha (0.25f);
                }
                else if (tags.Contains ("type_charger"))
                {
                    recognized = true;
                    icon = SdfIconType.ArrowDownLeftSquare;
                    textName = "CHR";
                    textDesc = "•<color=black>••</color>  light weight\n•<color=black>••</color>  close range";
                    col = Color.HSVToRGB (0.1f, 0.45f, 0.5f).WithAlpha (0.25f);
                }
                else if (tags.Contains ("type_defender"))
                {
                    recognized = true;
                    icon = SdfIconType.ArrowUpLeftSquare;
                    textName = "DEF";
                    textDesc = "•••  heavy weight\n•<color=black>••</color>  close range";
                    col = Color.HSVToRGB (0.25f, 0.45f, 0.5f).WithAlpha (0.25f);
                }
                else if (tags.Contains ("type_berserker"))
                {
                    recognized = true;
                    icon = SdfIconType.ArrowUpRightSquare;
                    textName = "BRS";
                    textDesc = "•••  heavy weight\n•••  long range";
                    col = Color.HSVToRGB (0.4f, 0.45f, 0.5f).WithAlpha (0.25f);
                }
                else if (tags.Contains ("type_ranger"))
                {
                    recognized = true;
                    icon = SdfIconType.ArrowDownRightSquare;
                    textName = "RAN";
                    textDesc = "•<color=black>••</color>  light weight\n•••  long range";
                    col = Color.HSVToRGB (0.5f, 0.45f, 0.5f).WithAlpha (0.25f);
                }
            }

            var gc = GUI.color;
            GUI.color = col;
            GUI.DrawTexture (rect.Expand (3), Texture2D.whiteTexture);
            GUI.color = gc;
            
            SdfIcons.DrawIcon (rectAligned, icon);
            if (recognized)
            {
                GUI.Label (rect.AddX (40), textName, EditorStyles.boldLabel);
                GUI.Label (rect.AddX (72), textDesc, miniLabelStyled);
            }
            else
            {
                GUI.Label (rect.AddX (40), textDesc, EditorStyles.miniLabel);
            }
            GUILayout.Space (8f);
        }

        private void DrawGradeEntryEndGUI (int index)
        {
            GUI.backgroundColor = new Color (1f, 1f, 1f, 1f);
        }

        private void DrawGradeEntryBeginGUI (int index)
        {
            var textIndex = index.IsValidIndex (UnitGroupDifficulty.text) ? index : 0;
            var difficultyText = $"{UnitGroupDifficulty.chevrons[textIndex]} {UnitGroupDifficulty.text[textIndex]}";
            var bgColor = UnitGroupDifficulty.inspectorColors[textIndex];
            GUI.backgroundColor = bgColor;
            
            GUILayout.BeginHorizontal ();
            GUILayout.Label (difficultyText, UnityEditor.EditorStyles.boldLabel);
            GUILayout.FlexibleSpace ();
            
            if (index > 0)
            {
                if (GUILayout.Button ("Copy from above ▲"))
                {
                    var entryPrev = unitsPerGrade[index - 1];
                    if (entryPrev == null || entryPrev.units == null)
                        unitsPerGrade[index] = null;
                    else
                    {
                        var entry = UtilitiesYAML.CloneThroughYaml (entryPrev);
                        unitsPerGrade[index] = entry;
                    }
                }
            }
            
            GUILayout.EndHorizontal ();
            // GUI.backgroundColor = bgColorOld;
        }
        
        private DataBlockUnitGroupGrade AddGrade ()
        {
            if (unitsPerGrade == null)
                return null;
            
            if (unitsPerGrade.Count == 3)
                return null;

            var entry = new DataBlockUnitGroupGrade ();
            if (unitsPerGrade.Count == 0)
                return entry;

            var entryPrev = unitsPerGrade[unitsPerGrade.Count - 1];
            if (entryPrev == null || entryPrev.units == null)
                return entry;

            entry.units = UtilitiesYAML.CloneThroughYaml (entryPrev.units);
            return entry;
        }

        private void OnGradeListChanged ()
        {
            if (unitsPerGrade == null)
                unitsPerGrade = new List<DataBlockUnitGroupGrade> ();

            if (unitsPerGrade.Count > 3)
                unitsPerGrade.RemoveRange (3, unitsPerGrade.Count - 3);

            RefreshUnitParentLink ();
        }

        private IEnumerable<string> GetDifficultyKeys => UnitGroupDifficulty.text;

        #endif
    }
}

public struct DataContainerLink<T> where T : DataContainer
{
    [HideLabel]
    [InlineButton ("TryOpeningLinker", "Open")]
    public string key;

    public override string ToString ()
    {
        return key;
    }

    public DataContainerLink (T input)
    {
        key = input != null ? input.key : null;
    }
    
    public DataContainerLink (string key)
    {
        this.key = key;
    }

    private static Dictionary<Type, Type> containerToLinkerLookup = new Dictionary<Type, Type> ();
    
    private void TryOpeningLinker ()
    {
        if (string.IsNullOrEmpty (key))
            return;
        
        var containerType = typeof (T);
        if (!containerToLinkerLookup.TryGetValue (containerType, out var linkerType))
        {
            linkerType = typeof (DataMultiLinker<>).MakeGenericType (new[] { containerType });
            containerToLinkerLookup.Add (containerType, linkerType);
        }
            
        var obj = GameObject.FindObjectOfType (linkerType);
        if (obj == null)
        {
            Debug.Log ($"Failed to find any GameObjects with component of type {linkerType}");
            return;
        }

        var bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy;
        var methodInfo = linkerType.GetMethod ("SetFilterAndSelect", bindingFlags);
        if (methodInfo == null)
        {
            var ms = linkerType.GetMethods (bindingFlags);
            Debug.Log ($"Failed to find SetAndApplyFilter method on type {linkerType} | Methods:\n{ms.ToStringFormatted (true, multilinePrefix: "- ", toStringOverride: (x) => x.Name)}");
            return;
        }

        methodInfo.Invoke (obj, new object[] { key });
    }
}