using System.Collections.Generic;
using PhantomBrigade.Combat.Components;
using PhantomBrigade.Functions;
using PhantomBrigade.Game;
using Sirenix.OdinInspector;
using UnityEngine;
using YamlDotNet.Serialization;

namespace PhantomBrigade.Data
{
    public class DataBlockUnitStatusStackable
    {
        [PropertyRange (1, 10)]
        public int limit = 1;

        public bool composite;
    }
    
    public class DataBlockUnitStatusAudio
    {
        [ValueDropdown ("GetAudioKeys")]
        [InlineButtonClear]
        public string audioOnStart;
        
        [ValueDropdown ("GetAudioKeys")]
        [InlineButtonClear]
        public string audioOnStop;

        private IEnumerable<string> GetAudioKeys () => AudioEvents.GetKeys ();
    }

    public class DataBlockUnitStatusEffectBlackboard
    {
        public string key;
        public bool indexed;

        [DropdownReference (true)]
        public CombatUnitDamageEvent damageEventDirectional;
        
        [LabelText ("Effects")]
        [DropdownReference]
        public List<DataBlockAssetInterpolated> fx;
        
        [DropdownReference]
        public List<ICombatFunctionTargeted> functions;
        
        #region Editor
        #if UNITY_EDITOR
        
        [ShowInInspector, PropertyOrder (10)]
        private DataEditor.DropdownReferenceHelper helper;
        
        public DataBlockUnitStatusEffectBlackboard () => 
            helper = new DataEditor.DropdownReferenceHelper (this);
        
        #endif
        #endregion
    }
    
    [LabelWidth (160f)]
    public class DataBlockAssetInterpolated : DataBlockAsset
    {
        [InfoBox ("@GetTransformPreviewText ()", InfoMessageType.None)]
        [LabelText ("Spawn Distance")]
        [YamlIgnore, ShowInInspector, PropertyOrder (-2)]
        [PropertyRange (0f, 100f), SuffixLabel ("%")]
        private float distanceProp
        {
            get
            {
                return Mathf.Clamp (distance * 100f, 0f, 100f);
            }
            set
            {
                distance = Mathf.Clamp01 (value * 0.01f);
            }
        }
        
        [HideInInspector]
        public float distance = 0f;

        [PropertyOrder (-1)]
        [LabelText ("Spawn Offset"), SuffixLabel ("m")]
        public float offsetLocal = 0f;
        
        [PropertyOrder (-1)]
        public Vector3 rotation = new Vector3 (0f, 0f, 0f);
        
        [ShowIf ("IsDistanceMode"), GUIColor ("GetScaleColor")]
        [LabelText ("Scale (XY)")]
        [YamlIgnore, ShowInInspector, PropertyOrder (1)]
        private Vector2 scaleXY
        {
            get
            {
                return new Vector2 (scale.x, scale.y);
            }
            set
            {
                scale = new Vector3 (value.x, value.y, scale.z);
            }
        }

        [GUIColor ("GetScaleColor")]
        [LabelText ("Scale (Z) From Distance")]
        [YamlIgnore, ShowInInspector, PropertyOrder (1)]
        private bool scaleFromDistanceUsed
        {
            get
            {
                return !scaleFromDistance.RoughlyEqual (0f);
            }
            set
            {
                if (value)
                {
                    scaleFromDistance = 1f;
                    scale = new Vector3 (scale.x, scale.y, 0f);
                }
                else
                {
                    scaleFromDistance = 0f;
                    scale = new Vector3 (scale.x, scale.y, scale.y);
                }
            }
        }
        
        [ShowIf ("IsDistanceMode"), GUIColor ("GetScaleColor")]
        [LabelText ("Scale Multiplier")]
        [YamlIgnore, ShowInInspector, PropertyOrder (1)]
        [PropertyRange (0f, 100f), SuffixLabel ("%")]
        private float scaleFromDistanceFactorProp
        {
            get
            {
                return Mathf.Clamp (scaleFromDistance * 100f, 0f, 100f);
            }
            set
            {
                scaleFromDistance = Mathf.Clamp01 (value * 0.01f);
            }
        }
        
        [ShowIf ("IsDistanceMode"), GUIColor ("GetScaleColor")]
        [LabelText ("Scale Offset")]
        [YamlIgnore, ShowInInspector, PropertyOrder (1)]
        [SuffixLabel ("m")]
        private float scaleFromDistanceOffsetProp
        {
            get
            {
                return scale.z;
            }
            set
            {
                scale = new Vector3 (scale.x, scale.y, value);
            }
        }
        
        [HideInInspector]
        public float scaleFromDistance = 0f;
            
        [PropertyOrder (-3)]
        [Min (0f), SuffixLabel ("s")]
        public float delay = 0f;

        public void GetFXTransform 
        (
            Vector3 positionSource, Vector3 positionTarget, 
            out Vector3 positionFinal, out Vector3 directionFinal, out Vector3 scaleFinal
        )
        {
            positionFinal = positionSource;
            directionFinal = Vector3.forward;
            scaleFinal = scale;
            
            if (positionSource == positionTarget)
                return;

            var deltaTarget = (positionTarget - positionSource);
            var distanceTarget = deltaTarget.magnitude;
            
            directionFinal = deltaTarget.normalized;
            positionFinal = positionSource + directionFinal * (offsetLocal + Mathf.Clamp01 (distance) * distanceTarget);
            scaleFinal = new Vector3 (scale.x, scale.y, scale.z + scaleFromDistance * distanceTarget);
            
            if (rotation != Vector3.zero)
                directionFinal = Quaternion.Euler (rotation) * directionFinal;
        }
        
        #if UNITY_EDITOR

        private bool IsDistanceMode () => !scaleFromDistance.RoughlyEqual (0f);
        protected override bool IsScaleXYZVisible () => !IsDistanceMode ();

        private Color colorDistanceMode = new Color (0.75f, 0.85f, 1f, 1f);
        protected Color GetScaleColor => IsDistanceMode () ? colorDistanceMode : Color.white;

        private string GetTransformPreviewText ()
        {
            var distanceFull = 30f;
            var pos = distance * 30f + offsetLocal;
            
            var textPrefix = $"Assuming distance of {distanceFull}m, the effect will be at";
            
            var textPos = $"\n- Position (Z): {pos:0.#}m";
            if (pos.RoughlyEqual (0f))
                textPos += " (origin)";
            else if (pos.RoughlyEqual (distanceFull))
                textPos += " (destination)";

            var scaleFull = new Vector3 (scale.x, scale.y, scale.z + distanceFull * scaleFromDistance);
            var textScale = $"\n- Scale (XYZ): {scaleFull.x:0.#} x {scaleFull.y:0.#} x {scaleFull.z:0.#}";

            return $"{textPrefix}{textPos}{textScale}";
        }
        
        #endif
    }
    
    public class DataBlockUnitStatusEffect
    {
        [PropertyOrder (-10)]
        [DropdownReference (true), HideLabel] 
        public DataBlockComment comment;
        
        [PropertyOrder (1)]
        [DropdownReference]
        public List<ICombatFunctionTargeted> functionsOnHost;
        
        [PropertyOrder (1)]
        [DropdownReference]
        public List<IPilotTargetedFunction> functionsOnPilot;
        
        [PropertyOrder (1)]
        [DropdownReference]
        public List<ICombatFunction> functionsGlobal;

        [PropertyOrder (1)]
        [DropdownReference]
        public List<DataBlockUnitStatusEffectBlackboard> functionsOnBlackboard;
        
        #region Editor
        #if UNITY_EDITOR
        
        [ShowInInspector, PropertyOrder (10)]
        private DataEditor.DropdownReferenceHelper helper;
        
        public DataBlockUnitStatusEffect () => 
            helper = new DataEditor.DropdownReferenceHelper (this);
        
        #endif
        #endregion
    }
    
    public class DataBlockUnitStatusEffectConditional : DataBlockUnitStatusEffect
    {
        [ToggleLeft, PropertyOrder (-20)]
        public bool enabled = true;

        [LabelText ("Unit Check"), PropertyOrder (-1)]
        [DropdownReference]
        [ListDrawerSettings (DefaultExpandedState = true, ElementColor = "GetElementColor")]
        public List<ICombatUnitValidationFunction> triggerCheckUnit;
        
        [LabelText ("Pilot Check"), PropertyOrder (-1)]
        [DropdownReference]
        [ListDrawerSettings (DefaultExpandedState = true, ElementColor = "GetElementColor")]
        public List<IPilotValidationFunction> triggerCheckPilot;
        
        #region Editor
        #if UNITY_EDITOR
        
        private Color GetElementColor (int index) => DataEditor.GetColorFromElementIndex (index);
        
        #endif
        #endregion
    }
    
    public class DataBlockUnitStatusEffectOnUpdate : DataBlockUnitStatusEffectConditional
    {
        [ToggleLeft, PropertyOrder (-2)]
        public bool triggerRemoval = false;

        [ToggleLeft, PropertyOrder (-2)]
        public bool triggerInStack = false;
        
        [ToggleLeft, PropertyOrder (-2)]
        public bool applyMissedUpdates = false;

        #region Editor
        #if UNITY_EDITOR
        
        private Color GetElementColor (int index) => DataEditor.GetColorFromElementIndex (index);
        
        #endif
        #endregion
    }
    
    public class DataBlockUnitStatusEffectOnEnd : DataBlockUnitStatusEffectConditional
    {
        [ToggleLeft, PropertyOrder (-2)]
        public bool triggerOnCompletion = true;

        [ToggleLeft, PropertyOrder (-2)]
        public bool triggerOnCancel = false;

        #region Editor
        #if UNITY_EDITOR
        
        private Color GetElementColor (int index) => DataEditor.GetColorFromElementIndex (index);
        
        #endif
        #endregion
    }
    
    [HideReferenceObjectPicker]
    public class DataBlockFloatComposite
    {
        [LabelText ("Value")]
        public float f;

        [LabelText ("Boss multiplier")]
        [PropertyRange (0f, 10f)]
        public float compMultiplier = 1f;
    }

    public static class UnitStatusTags
    {
        public const string PilotAbility = "ability";
    }

    public class DataBlockAssetStatusRepeat : DataBlockAsset
    {
        public bool parented = true;
        
        [PropertyOrder (-1)]
        public float interval = 0.5f;
        
        [PropertyOrder (-1)]
        public float velocityDecayTime = 1f;
    }
    
    [LabelWidth (180f)]
    public class DataContainerUnitStatus : DataContainerWithText
    {
        [YamlIgnore, ReadOnly, LabelText ("ID")]
        public int keyHash;

        [DataEditor.SpriteNameAttribute (true, 32f)]
        public string icon;
        
        [HorizontalGroup ("Color")]
        [OnValueChanged ("OnColorChange", true), ColorUsage (false)]
        public Color color = new Color (0.75f, 0.3f, 0.3f, 1f);
        
        [HorizontalGroup ("Color", 40f)]
        [YamlIgnore, ReadOnly, HideLabel, ColorUsage (false)]
        public Color colorHighlightDark;
        
        [HorizontalGroup ("Color", 40f)]
        [YamlIgnore, ReadOnly, HideLabel, ColorUsage (false)]
        public Color colorHighlight;
        
        [HorizontalGroup ("Color", 40f)]
        [YamlIgnore, ReadOnly, HideLabel, ColorUsage (false)]
        public Color colorHighlightMax;
        
        [HorizontalGroup ("Color", 56f)]
        [YamlIgnore, ReadOnly, HideLabel]
        public string colorTagNormal;
        
        [HorizontalGroup ("Color", 56f)]
        [YamlIgnore, ReadOnly, HideLabel]
        public string colorTagHighlight;
        
        [YamlIgnore]
        [LabelText ("Header / Desc.")]
        public string textName;
        
        [YamlIgnore]
        [TextArea][HideLabel]
        public string textDesc;

        public bool hidden = false;
        public bool debug = false;
        public bool showOverlay = true;
        public bool showNotifications = true;
        public bool showTutorial = true;
        public bool restartOnRefresh = false;
        public bool turnLinked = false;

        [DropdownReference (true)]
        public DataBlockCombatActionLink actionLink;
        
        [DropdownReference (false)]
        public HashSet<string> tags;

        [DropdownReference (true)]
        public DataBlockAsset fxAttached;
        
        [DropdownReference (true)]
        public DataBlockAssetStatusRepeat fxRepeated;

        [DropdownReference (true)]
        public DataBlockFloat durationFull;
        
        [DropdownReference (true)]
        public DataBlockFloat durationUpdate;
        
        [DropdownReference (true)]
        public DataBlockFloatComposite buildupDecayRate;
        
        [DropdownReference (true)]
        public DataBlockFloatComposite buildupThreshold;
        
        [DropdownReference (true)]
        public DataBlockFloatComposite buildupOverflow;
        
        [DropdownReference (true)]
        public DataBlockFloat buildupShieldMultiplier;

        [DropdownReference (true)]
        public DataBlockUnitStatusStackable stackable;
        
        [DropdownReference (true)]
        public DataBlockUnitStatusAudio audio;

        [DropdownReference (true)]
        public List<DataBlockUnitStatusEffectConditional> effectsOnStart;
        
        [DropdownReference]
        [ListDrawerSettings (ElementColor = "GetEffectsOnUpdateElementColor")]
        public List<DataBlockUnitStatusEffectOnUpdate> effectsOnUpdate;
        
        [DropdownReference (true)]
        public List<DataBlockUnitStatusEffectOnEnd> effectsOnEnd;
        
        [DropdownReference]
        [DictionaryKeyDropdown ("@DataMultiLinkerUnitStats.data.Keys")]
        [DictionaryDrawerSettings (KeyColumnWidth = DataEditor.dictionaryKeyWidth)]
        public SortedDictionary<string, float> statOffsets;

        public bool IsTagPresent (string tag)
        {
            if (tags == null)
                return false;

            if (string.IsNullOrEmpty (tag))
                return false;
            
            return tags.Contains (tag);
        }
        
        public override void OnAfterDeserialization (string key)
        {
            base.OnAfterDeserialization (key);
            keyHash = key.GetHashCode ();
            OnColorChange ();
        }

        public virtual void OnKeyReplacement (string keyOld, string keyNew)
        {
            base.OnKeyReplacement (keyOld, keyNew);
            keyHash = key.GetHashCode ();
        }
        

        public void GetUIData (out string icon, out string textName, out string textDesc)
        {
            icon = this.icon;
            textName = this.textName;
            textDesc = this.textDesc;

            if (actionLink != null && !string.IsNullOrEmpty (actionLink.key))
            {
                var actionData = DataMultiLinkerAction.GetEntry (actionLink.key, false);
                if (actionData != null && actionData.dataUI != null)
                {
                    if (string.IsNullOrEmpty (icon))
                        icon = actionData.dataUI.icon;
                    
                    if (string.IsNullOrEmpty (textName))
                        textName = actionData.dataUI.textName;
                    
                    if (string.IsNullOrEmpty (textDesc))
                        textDesc = actionData.dataUI.textDesc;
                }
            }
        }
        
        
        
        

        public override void ResolveText ()
        {
            textName = DataManagerText.GetText (TextLibs.unitStatus, $"{key}__header", true);
            textDesc = DataManagerText.GetText (TextLibs.unitStatus, $"{key}__text", true);
        }

        private void OnColorChange ()
        {
            colorTagNormal = UtilityColor.ToHexRGB (color);
            colorTagHighlight = UtilityColor.ToHexRGB (Color.Lerp (color, Color.white, 0.5f));
            colorHighlight = Color.HSVToRGB (new HSBColor (color).h, 0.32f, 1f).WithAlpha (1f);
            colorHighlightMax = Color.Lerp (colorHighlight, Color.white, 0.5f).WithAlpha (1f);
            colorHighlightDark = Color.Lerp (colorHighlight, Color.black, 0.5f).WithAlpha (1f);
        }

        #region Editor
        #if UNITY_EDITOR
        
        [ShowInInspector]
        private DataEditor.DropdownReferenceHelper helper;
        
        public DataContainerUnitStatus () => 
            helper = new DataEditor.DropdownReferenceHelper (this);

        public override void SaveText ()
        {
            if (!IsTextSavingPossible ())
                return;

            if (!string.IsNullOrEmpty (textName))
                DataManagerText.TryAddingTextToLibrary (TextLibs.unitStatus, $"{key}__header", textName);
            
            if (!string.IsNullOrEmpty (textDesc))
                DataManagerText.TryAddingTextToLibrary (TextLibs.unitStatus, $"{key}__text", textDesc);
        }

        #if !PB_MODSDK
        [HideInEditorMode]
        [Button ("Build on selected"), ButtonGroup, PropertyOrder (-1)]
        private void BuildOnSelectedUnit ()
        {
            var unitSelected = IDUtility.GetSelectedCombatEntity ();
            if (unitSelected != null)
                UnitStatusUtility.OffsetBuildup (unitSelected, key, 0.25f, UnitStatusSource.Function);
        }
        
        [HideInEditorMode]
        [Button ("Add to selected"), ButtonGroup, PropertyOrder (-1)]
        private void AddToSelectedUnit ()
        {
            var unitSelected = IDUtility.GetSelectedCombatEntity ();
            if (unitSelected != null)
                UnitStatusUtility.AddStatus (unitSelected, key, UnitStatusSource.Function);
        }
        
        [HideInEditorMode]
        [Button ("Remove from selected"), ButtonGroup, PropertyOrder (-1)]
        private void RemoveFromToSelectedUnit ()
        {
            var unitSelected = IDUtility.GetSelectedCombatEntity ();
            if (unitSelected != null)
                UnitStatusUtility.RemoveStatus (unitSelected, key);
        }
        #endif
        
        private Color GetEffectsOnUpdateElementColor (int index, Color defaultColor)
        {
            var value = effectsOnUpdate != null && index >= 0 && index < effectsOnUpdate.Count ? effectsOnUpdate[index] : null;
            if (value != null)
            { 
                if (!value.enabled)
                    return Color.gray.WithAlpha (0.2f);
            }
            return DataEditor.GetColorFromElementIndex (index);
        }
        
        #endif
        #endregion
    }
}

