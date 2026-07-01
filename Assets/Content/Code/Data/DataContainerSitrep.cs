using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using YamlDotNet.Serialization;

namespace PhantomBrigade.Data
{
    [HideReferenceObjectPicker]
    public class DataBlockScenarioMarkerSize
    {
        [HideLabel, HorizontalGroup ("B"), SuffixLabel ("X  ", true)]
        [Min (4)]
        public int x = 16;

        [HideLabel, HorizontalGroup ("B"), SuffixLabel ("Y  ", true)]
        [Min (4)]
        public int y = 16;
    }
    
    [HideReferenceObjectPicker]
    public class DataBlockScenarioSitrepMarker
    {
        [DropdownReference]
        [HideLabel, SuffixLabel ("Custom key", true)]
        public string key;
        
        [HideLabel, HorizontalGroup ("B"), SuffixLabel ("Time  ", true)]
        public float time = 0f;
        
        [HideLabel, HorizontalGroup ("B"), SuffixLabel ("X  ", true)]
        public float x = 0f;

        [HideLabel, HorizontalGroup ("B"), SuffixLabel ("Y  ", true)]
        public float y = 0f;
        
        [HideLabel, HorizontalGroup ("B", 64f)]
        [DropdownReference, InlineProperty]
        public DataBlockColor color;

        [DropdownReference, InlineProperty, PropertySpace (4f)]
        public DataBlockScenarioMarkerSize size;
        
        [DropdownReference, InlineProperty]
        public DataBlockFloat rotation;
        
        [DropdownReference, InlineProperty]
        public DataBlockFloat duration;
        
        [DropdownReference]
        [ValueDropdown ("styleKeysBuiltin")]
        public string style;
        
        [DropdownReference]
        [DataEditor.SpriteNameAttribute (true, 32f)]
        public string icon;

        [DropdownReference (true)]
        [HideLabel]
        public DataBlockTextTrimodal text;
        
        #region Editor
        #if UNITY_EDITOR
        
        private static List<string> styleKeysBuiltin = new List<string>
        {
            "base_s_l",
            "base_s_r",
            "base_s_c",
            "base_m_l",
            "base_m_r",
            "base_m_c",
            "base_l_c",
            "grid1",
            "grid2",
            "shape_round_l",
            "shape_round_r",
            "shape_round_c",
            "shape_diamond_c",
            "shape_arrow1",
            "shape_line1"
        };
        
        [ShowInInspector, PropertyOrder (100)]
        private DataEditor.DropdownReferenceHelper helper;
        
        public DataBlockScenarioSitrepMarker () => 
            helper = new DataEditor.DropdownReferenceHelper (this);
        
        #endif
        #endregion
    }

    [HideReferenceObjectPicker]
    public class DataBlockScenarioSitrepMap
    {
        [InlineButtonClear]
        [ValueDropdown ("GetTextureKeys")]
        [OnInspectorGUI ("@DropdownUtils.DrawTexturePreview ($value, TextureGroupKeys.OverworldMapBackgrounds, 256)", false)]
        [LabelText ("Custom BG")]
        public string image;
        
        [HideLabel, HorizontalGroup, SuffixLabel ("X  ", true)]
        public float x = 0f;
        
        [HideLabel, HorizontalGroup, SuffixLabel ("Y  ", true)]
        public float y = 0f;
        
        [HideLabel, HorizontalGroup, SuffixLabel ("Scale  ", true)]
        public float scale = 1f;
        
        [InlineButtonClear]
        [ListDrawerSettings (DefaultExpandedState = true, ShowPaging = false, CustomAddFunction = "@new DataBlockScenarioSitrepMarker ()")]
        public List<DataBlockScenarioSitrepMarker> markers;
        
        #if UNITY_EDITOR
        
        private IEnumerable<string> GetTextureKeys () => TextureManager.GetExposedTextureKeys (TextureGroupKeys.OverworldMapBackgrounds);
        
        #endif
    }
    
    
    [HideReferenceObjectPicker]
    public class DataBlockScenarioSitrepSlide
    {
        [ValueDropdown ("GetTextureKeys")]
        [OnInspectorGUI ("@DropdownUtils.DrawTexturePreview ($value, group, 256)", false)]
        public string image;
        
        [ValueDropdown ("GetGroupKeys")]
        [InlineButton ("ReloadTextureGroup", "Reload")]
        public string group = TextureGroupKeys.OverworldEvents;
        
        #if UNITY_EDITOR
        
        private IEnumerable<string> GetGroupKeys () => FieldReflectionUtility.GetConstantStringFieldValues (typeof (TextureGroupKeys));
        private IEnumerable<string> GetTextureKeys () => TextureManager.GetExposedTextureKeys (group);
        private void ReloadTextureGroup () => TextureManager.LoadGroup (group);
        
        #endif
    }
    
    
    [HideReferenceObjectPicker]
    public class DataBlockScenarioSitrepBackground
    {
        [ValueDropdown ("GetTextureKeys")]
        [OnInspectorGUI ("@DropdownUtils.DrawTexturePreview ($value, TextureGroupKeys.OverworldEvents, 256)", false)]
        public string image;
        
        [PropertyRange (0f, 1f), HorizontalGroup]
        [LabelText ("Brightness / Color")]
        public float brightness = 1f;
        
        [HideLabel, HorizontalGroup (0.25f)]
        public Color color = Color.white.WithAlpha (0f);

        public bool focus;
        
        #if UNITY_EDITOR
        
        private IEnumerable<string> GetTextureKeys () => TextureManager.GetExposedTextureKeys (TextureGroupKeys.OverworldEvents);
        
        #endif
    }
    
    [HideReferenceObjectPicker, LabelWidth (120f)]
    public class DataBlockScenarioSitrepStep
    {
        [OnValueChanged ("OnChange", true), HideLabel, PropertyOrder (1)]
        public Color color = new Color (1f, 0.5f, 0.5f);
        
        [OnValueChanged ("OnChange", true)]
        [DropdownReference (true)]
        public DataBlockScenarioSitrepSlide slide;
        
        [OnValueChanged ("OnChange", true)]
        [DropdownReference (true)]
        public DataBlockScenarioSitrepBackground background;
        
        [OnValueChanged ("OnChange", true)]
        [DropdownReference (true)]
        public DataBlockScenarioSitrepMap map;

        [OnValueChanged ("OnChange", true), PropertyOrder (2)]
        [DropdownReference, InlineProperty, HideLabel]
        public DataBlockTextTrimodal textBody;
        
        [YamlIgnore, HideInInspector]
        public string id;
        
        [YamlIgnore, ShowInInspector, ReadOnly, PropertyOrder (-1)]
        public int index;
        
        #region Editor
        #if UNITY_EDITOR
        
        [ShowInInspector, PropertyOrder (100)]
        private DataEditor.DropdownReferenceHelper helper;
        
        public DataBlockScenarioSitrepStep () => 
            helper = new DataEditor.DropdownReferenceHelper (this);

        [Button ("Preview"), HideInEditorMode, PropertyOrder (-1)]
        private void OnPreview () => OnPreview (false);
        private void OnChange () => OnPreview (true);
        private void OnPreview (bool forceEnd)
        {
            #if !PB_MODSDK
            if (!Application.isPlaying || CIViewCombatSitrep.ins == null || !CIViewCombatSitrep.ins.IsEntered ())
                return;

            if (CIViewCombatSitrep.stepIDLast != id)
                return;
            
            CIViewCombatSitrep.ins.OnStepChange (this, forceEnd);
            #endif
        }
        
        #endif
        #endregion
    }
    
    public class DataContainerSitrep : DataContainerWithText
    {
        [ToggleLeft]
        public bool hidden;
        
        [OnValueChanged ("RefreshParentsInPages")]
        [ListDrawerSettings (DefaultExpandedState = true, ShowPaging = false, CustomAddFunction = "@new DataBlockScenarioSitrepStep ()")]
        public List<DataBlockScenarioSitrepStep> steps = new List<DataBlockScenarioSitrepStep> ();
        
        public override void OnAfterDeserialization (string key)
        {
            // Set key manually so that it is in place for RefreshParentsInPages
            this.key = key;
            
            // Run this method first, before base implementation invokes ResolveText, which depends on IDs set here
            RefreshParentsInPages ();
            
            // Now it's safe to run this
            base.OnAfterDeserialization (key);
        }
        
        private void RefreshParentsInPages ()
        {
            if (steps != null)
            {
                for (int i = 0, count = steps.Count; i < count; ++i)
                {
                    var page = steps[i];
                    if (page == null)
                        continue;

                    page.index = i;
                    page.id = $"{key}__p{i:D2}";
                }
            }
        }
        
        public override void ResolveText ()
        {
            if (steps != null)
            {
                for (int i = 0; i < steps.Count; i++)
                {
                    var step = steps[i];
                    if (step == null)
                        continue;

                    var stepKey = $"{key}_s{i:00}";
                    if (step.textBody != null)
                        step.textBody.ResolveText (TextLibs.combatSitreps, stepKey + "_body");

                    if (step.map != null && step.map.markers != null && step.map.markers.Count > 0)
                    {
                        for (int m = 0, mLimit = step.map.markers.Count; m < mLimit; ++m)
                        {
                            var marker = step.map.markers[m];
                            if (marker == null)
                                continue;
                            
                            if (marker.text != null)
                            {
                                var markerKey = !string.IsNullOrEmpty (marker.key) ? marker.key : m.ToString ("00");
                                marker.text.ResolveText (TextLibs.combatSitreps, $"{stepKey}_m_{markerKey}");
                            }
                        }
                    }
                }
            }
        }

        #if UNITY_EDITOR 

        public override void SaveText ()
        {
            if (!IsTextSavingPossible ())
                return;

            if (steps != null)
            {
                for (int i = 0; i < steps.Count; i++)
                {
                    var step = steps[i];
                    if (step == null)
                        continue;

                    var stepKey = $"{key}_s{i:00}";
                    if (step.textBody != null)
                        step.textBody.SaveText (TextLibs.combatSitreps, stepKey + "_body");

                    if (step.map != null && step.map.markers != null && step.map.markers.Count > 0)
                    {
                        for (int m = 0, mLimit = step.map.markers.Count; m < mLimit; ++m)
                        {
                            var marker = step.map.markers[m];
                            if (marker == null)
                                continue;
                            
                            if (marker.text != null)
                            {
                                var markerKey = !string.IsNullOrEmpty (marker.key) ? marker.key : m.ToString ("00");
                                marker.text.SaveText (TextLibs.combatSitreps, $"{stepKey}_m_{markerKey}");
                            }
                        }
                    }
                }
            }
        }

        [HideInEditorMode, Button, PropertyOrder (-1)]
        private void Test ()
        {
            #if !PB_MODSDK
            if (CIViewCombatSitrep.ins == null)
                return;
            
            CIViewCombatSitrep.ins.TryEntryFromSitrep (this);
            #endif
        }

        #endif
    }
}

