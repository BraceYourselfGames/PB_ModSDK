using System;
using System.Collections.Generic;
using System.Text;
using PhantomBrigade.Functions;
using Sirenix.OdinInspector;
using UnityEngine;
using YamlDotNet.Serialization;

namespace PhantomBrigade.Data
{
    public class DataBlockBaseEffectPilot
    {
        [DropdownReference]
        public List<IPilotValidationFunction> checksPilot;
        
        [DropdownReference]
        public List<IPilotTargetedFunction> functionsPilot;
        
        #if UNITY_EDITOR
        
        [ShowInInspector, PropertyOrder (100)]
        private DataEditor.DropdownReferenceHelper helper;
        
        public DataBlockBaseEffectPilot () => 
            helper = new DataEditor.DropdownReferenceHelper (this);
        
        #endif
    }
    
    public class DataBlockBaseEffectUnit
    {
        [DropdownReference]
        public List<IOverworldUnitValidationFunction> checksUnit;
        
        [DropdownReference]
        public List<IOverworldUnitFunction> functionsUnit;
        
        #if UNITY_EDITOR
        
        [ShowInInspector, PropertyOrder (100)]
        private DataEditor.DropdownReferenceHelper helper;
        
        public DataBlockBaseEffectUnit () => 
            helper = new DataEditor.DropdownReferenceHelper (this);
        
        #endif
    }

    /*
    public enum BaseEffectContext
    {
        None,
        PilotRecovery,
        UnitRecovery,
        CampGeneral
    }
    */
    
    public static class BaseEffectTags
    {
        public const string ResupplyGeneral = "resupply_general";
        public const string TravelGeneral = "travel_general";
        
        public const string CampGeneral = "camp_general";
        public const string CampPilot = "camp_pilot";
        public const string CampUnit = "camp_unit";
        
        public const string RedrawAllSelections = "redraw_all_selections";

        /*
        public static HashSet<string> tagsLocalized = new HashSet<string>
        {
            ResupplyGeneral,
            TravelGeneral,
            CampGeneral,
            CampPilot,
            CampUnit
        };
        */
    }

    public class DataBlockBaseEffectSourceText
    {
        [YamlIgnore]
        [LabelText ("Source hint")]
        public string text = string.Empty;

        [DropdownReference]
        [ValueDropdown("@DataMultiLinkerBaseEffect.tags")]
        public string tagAssociated = string.Empty;
        
        [DropdownReference]
        [ValueDropdown("@DataMultiLinkerPilotEvent.data.Keys")]
        public string pilotEventAssociated = string.Empty;
        
        #region Editor
        #if UNITY_EDITOR
        
        [ShowInInspector, PropertyOrder (100)]
        private DataEditor.DropdownReferenceHelper helper;
        
        public DataBlockBaseEffectSourceText () => 
            helper = new DataEditor.DropdownReferenceHelper (this);
        
        #endif
        #endregion
    }

    public enum BaseEffectTextMode
    {
        FunctionTextOmitted,
        FunctionTextInSecondary,
        FunctionTextInTooltip
    }

    [Serializable][LabelWidth (180f)]
    public class DataContainerBaseEffect : DataContainerWithText, IDataContainerTagged
    {
        public bool hidden = false;
        public bool unique = false;
        public bool quickIntro = false;
        
        [HorizontalGroup ("A"), LabelText ("Priority/Rating/Mood")]
        public int priority = 0;
        
        [HorizontalGroup ("A", 0.25f), HideLabel]
        public int rating = 0;
        
        [HorizontalGroup ("A", 0.25f), HideLabel]
        public int mood = 0;
        
        public string group;
        
        [GUIColor ("GetHueBasedColor"), SuffixLabel ("███")]
        [PropertyRange (0f, 1f)]
        public float hue = 0f;
        
        [PropertySpace (8f)]
        [DataEditor.SpriteNameAttribute (false, 180f)]
        public string icon = "s_icon_m_link_squad";

        [YamlIgnore]
        [LabelText ("Name")]
        public string textName = string.Empty;
        
        [YamlIgnore]
        [LabelText ("Effect / Desc.")]
        public string textEffect = string.Empty;
        
        [YamlIgnore]
        [TextArea (1, 10)]
        [HideLabel]
        public string textDesc = string.Empty;
        
        [HideLabel]
        public BaseEffectTextMode textFromFunctions = BaseEffectTextMode.FunctionTextOmitted;
        
        [DropdownReference (true)]
        public DataBlockBaseEffectSourceText textSource;
        
        [PropertySpace (8f)]
        [DropdownReference]
        [ValueDropdown("@DataMultiLinkerBaseEffect.tags")]
        public HashSet<string> tags = new HashSet<string> ();
        
        [DropdownReference]
        public List<IOverworldGlobalValidationFunction> checksGlobal;
        
        [DropdownReference]
        public List<IOverworldEntityValidationFunction> checksBase;
        
        [DropdownReference]
        public List<IOverworldFunction> functionsGlobal;
        
        [DropdownReference]
        public List<IOverworldTargetedFunction> functionsBase;
        
        [DropdownReference (true)]
        public DataBlockBaseEffectPilot pilot;
        
        [DropdownReference (true)]
        public DataBlockBaseEffectUnit unit;

        public HashSet<string> GetTags (bool processed) =>
            tags;
        
        public bool IsHidden () => hidden;
        
        public override void ResolveText ()
        {
            textName = DataManagerText.GetText (TextLibs.baseEffects, $"{key}__name", true);
            textEffect = DataManagerText.GetText (TextLibs.baseEffects, $"{key}__effect", true);
            textDesc = DataManagerText.GetText (TextLibs.baseEffects, $"{key}__text", true);
            
            if (textSource != null)
                textSource.text = DataManagerText.GetText (TextLibs.baseEffects, $"{key}__source", true);
        }

        private static StringBuilder sb = new StringBuilder ();
        private static List<string> textFromFunctionsList = new List<string> ();

        #if !PB_MODSDK
        public bool AreChecksPassed (PersistentEntity unitTargeted, PersistentEntity pilotTargeted)
        {
            if (!Application.isPlaying || !IDUtility.IsGameLoaded ())
                return false;
            
            if (checksGlobal != null)
            {
                bool valid = true;
                foreach (var check in checksGlobal)
                {
                    valid = check.IsValid ();
                    if (!valid)
                        break;
                }
                    
                if (!valid)
                    return false;
            }
                
            if (checksBase != null)
            {
                var basePersistent = IDUtility.playerBasePersistent;
                bool valid = true;
                foreach (var check in checksBase)
                {
                    valid = check.IsValid (basePersistent);
                    if (!valid)
                        break;
                }
                    
                if (!valid)
                    return false;
            }

            if (pilotTargeted != null && pilot?.checksPilot != null)
            {
                bool valid = true;
                foreach (var check in pilot.checksPilot)
                {
                    valid = check.IsValid (pilotTargeted, null);
                    if (!valid)
                        break;
                }
                
                if (!valid)
                    return false;
            }
            
            if (unitTargeted != null && unit?.checksUnit != null)
            {
                bool valid = true;
                foreach (var check in unit.checksUnit)
                {
                    valid = check.IsValid (unitTargeted);
                    if (!valid)
                        break;
                }
                
                if (!valid)
                    return false;
            }
            
            return true;
        }

        public void GetTextDependencies (PersistentEntity unitTargeted, PersistentEntity pilotTargeted, out string textPrimary, out string textSecondary, out string textTooltip)
        {
            textPrimary = textName;
            textSecondary = string.Empty;
            textTooltip = string.Empty;
            var mode = textFromFunctions;

            if (!string.IsNullOrEmpty (textEffect))
            {
                textSecondary = textEffect;
            }

            if (!string.IsNullOrEmpty (textDesc))
            {
                textTooltip = textDesc;
            }

            textFromFunctionsList.Clear ();
            
            if (functionsGlobal != null)
            {
                foreach (var function in functionsGlobal)
                {
                    if (function != null && function is IFunctionLocalizedText functionLoc)
                    {
                        var textFunction = functionLoc.GetLocalizedText ();
                        if (!string.IsNullOrEmpty (textFunction))
                            textFromFunctionsList.Add (textFunction);
                    }
                }
            }

            if (functionsBase != null)
            {
                foreach (var function in functionsBase)
                {
                    if (function != null && function is IFunctionLocalizedText functionLoc)
                    {
                        var textFunction = functionLoc.GetLocalizedText ();
                        if (!string.IsNullOrEmpty (textFunction))
                            textFromFunctionsList.Add (textFunction);
                    }
                }
            }
        
            if (unit?.functionsUnit != null && unitTargeted != null)
            {
                foreach (var function in unit.functionsUnit)
                {
                    if (function != null && function is IFunctionLocalizedText functionLoc)
                    {
                        var textFunction = functionLoc.GetLocalizedText ();
                        if (!string.IsNullOrEmpty (textFunction))
                            textFromFunctionsList.Add (textFunction);
                    }
                }
            }
        
            if (pilot?.functionsPilot != null && pilotTargeted != null)
            {
                foreach (var function in pilot.functionsPilot)
                {
                    if (function != null && function is IFunctionLocalizedText functionLoc)
                    {
                        var textFunction = functionLoc.GetLocalizedText ();
                        if (!string.IsNullOrEmpty (textFunction))
                            textFromFunctionsList.Add (textFunction);
                    }
                }
            }
            
            if (textFromFunctionsList.Count > 0)
            {
                if (mode == BaseEffectTextMode.FunctionTextInSecondary)
                {
                    if (textFromFunctionsList.Count > 1)
                        mode = BaseEffectTextMode.FunctionTextInTooltip;
                    else
                        textSecondary = textFromFunctionsList[0];
                }
                
                if (textFromFunctions == BaseEffectTextMode.FunctionTextInTooltip)
                {
                    if (!string.IsNullOrEmpty (textTooltip))
                    {
                        if (textFromFunctionsList.Count > 1)
                            textTooltip = $"{textTooltip}\n\n{textFromFunctionsList.ToStringMultilineDash ()}";
                        else
                            textTooltip = $"{textTooltip}\n\n{textFromFunctionsList[0]}";
                    }
                    else
                    {
                        if (textFromFunctionsList.Count > 1)
                            textTooltip = textFromFunctionsList.ToStringMultilineDash ();
                        else
                            textTooltip = textFromFunctionsList[0];
                    }
                }
            }
            
            if (!string.IsNullOrEmpty (textTooltip))
            {
                if (string.IsNullOrEmpty (textSecondary))
                    textSecondary = Txt.Get (TextLibs.uiOverworld, "lock_effect_multiple");
                
                if (!textSecondary.EndsWith (" *"))
                    textSecondary += " *";

                
            }

            if (textSource != null)
            {
                string insertedText = null;
                if (!string.IsNullOrEmpty (textSource.text))
                    insertedText = textSource.text;
                else if (!string.IsNullOrEmpty (textSource.pilotEventAssociated))
                {
                    var eventData = DataMultiLinkerPilotEvent.GetEntry (textSource.pilotEventAssociated, false);
                    if (eventData != null && eventData.ui != null)
                        insertedText = $"{Txt.Get (TextLibs.uiOverworld, "lock_effect_pilot_event")}: {eventData.ui.textName.ToLower ()}";
                }

                if (!string.IsNullOrEmpty (insertedText))
                {
                    var hexTag = UtilityColor.ToHexTagRGB (GetHueBasedColor);
                    if (!string.IsNullOrEmpty (textTooltip))
                        textTooltip = $"{textTooltip}\n\n[i]{hexTag}{insertedText}[-][/i]";
                    else
                        textTooltip = $"[i]{hexTag}{insertedText}[-][/i]";
                }
            }
            
            if (tags != null)
            {
                var data = DataMultiLinkerBaseEffect.data;
                var tagsWithLocText = DataMultiLinkerBaseEffect.tagsWithLocText;
                bool textSourceWithTag = textSource != null && !string.IsNullOrEmpty (textSource.tagAssociated);
                
                foreach (var tag in tags)
                {
                    if (!tagsWithLocText.TryGetValue (tag, out var insertedText))
                        continue;
                    
                    if (textSourceWithTag && string.Equals (tag, textSource.tagAssociated, StringComparison.Ordinal))
                        continue;
                        
                    // Txt.Get (TextLibs.uiOverworld, "lock_effect_tag_" + tag, true);
                    if (string.IsNullOrEmpty (insertedText))
                        continue;

                    var hexTag = UtilityColor.ToHexTagRGB (GetHueBasedColor);
                    if (!string.IsNullOrEmpty (textTooltip))
                        textTooltip = $"{textTooltip}\n\n[i]{hexTag}{insertedText}[-][/i]";
                    else
                        textTooltip = $"[i]{hexTag}{insertedText}[-][/i]";
                }
            }
        }

        public void Run (PersistentEntity unitTargeted, PersistentEntity pilotTargeted)
        {
            if (!Application.isPlaying || !IDUtility.IsGameLoaded ())
                return;

            if (functionsGlobal != null)
            {
                foreach (var function in functionsGlobal)
                {
                    if (function != null)
                        function.Run ();
                }
            }
            
            if (functionsBase != null)
            {
                var baseOverworld = IDUtility.playerBaseOverworld;
                foreach (var function in functionsBase)
                {
                    if (function != null)
                        function.Run (baseOverworld);
                }
            }

            if (pilot != null && pilotTargeted != null)
            {
                if (pilot.functionsPilot != null)
                {
                    foreach (var function in pilot.functionsPilot)
                    {
                        if (function != null)
                            function.Run (pilotTargeted, null);
                    }
                }
            }
            
            if (unit != null && unitTargeted != null)
            {
                if (unit.functionsUnit != null)
                {
                    foreach (var function in unit.functionsUnit)
                    {
                        if (function != null)
                            function.Run (unitTargeted);
                    }
                }
            }
        }
        #endif
        
        private Color GetHueBasedColor => Color.HSVToRGB (hue, 0.3f, 1f);

        #if UNITY_EDITOR
        
        [ShowInInspector, PropertyOrder (100)]
        private DataEditor.DropdownReferenceHelper helper;
        
        public DataContainerBaseEffect () => 
            helper = new DataEditor.DropdownReferenceHelper (this);

        public override void SaveText ()
        {
            if (!IsTextSavingPossible ())
                return;

            if (!string.IsNullOrEmpty (textName))
                DataManagerText.TryAddingTextToLibrary (TextLibs.baseEffects, $"{key}__name", textName);
            
            if (!string.IsNullOrEmpty (textEffect))
                DataManagerText.TryAddingTextToLibrary (TextLibs.baseEffects, $"{key}__effect", textEffect);
            
            if (!string.IsNullOrEmpty (textDesc))
                DataManagerText.TryAddingTextToLibrary (TextLibs.baseEffects, $"{key}__text", textDesc);
            
            if (textSource != null && !string.IsNullOrEmpty (textSource.text))
                DataManagerText.TryAddingTextToLibrary (TextLibs.baseEffects, $"{key}__source", textSource.text);
        }

        #endif
    }
}

