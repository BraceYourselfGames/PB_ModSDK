using System;
using PhantomBrigade.Data;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PhantomBrigade.Functions
{
    public class OverworldUnitFrameIntegritySet : IOverworldUnitFunction, IFunctionLocalizedText
    {
        [InfoBox ("@GetLocalizedText ()", InfoMessageType.None)]
        [PropertyRange (0f, 1f)]
        public float value;

        public void Run (PersistentEntity unitPersistent)
        {
            #if !PB_MODSDK

            if (unitPersistent == null || !unitPersistent.isUnitTag)
                return;
            
            unitPersistent.ReplaceUnitFrameIntegrity (Mathf.Clamp01 (value));

            #endif
        }

        public string GetLocalizedText ()
        {
            #if !PB_MODSDK

            var statInfo = DataMultiLinkerUnitStats.GetEntry (UnitStats.frameHealth, false);
            if (statInfo == null)
                return string.Empty;

            var text = TextUtility.GetPercentageFromNormalizedValue (value);
            return $"[cc]{statInfo.textName}:[ff] {text}";

            #else
            return null;
            #endif
        }
    }
    
    public class OverworldUnitFrameIntegrityOffset : IOverworldUnitFunction, IFunctionLocalizedText
    {
        [InfoBox ("@GetLocalizedText ()", InfoMessageType.None)]
        public float value;

        public void Run (PersistentEntity unitPersistent)
        {
            #if !PB_MODSDK

            if (unitPersistent == null || !unitPersistent.isUnitTag)
                return;

            float hp = unitPersistent.hasUnitFrameIntegrity ? unitPersistent.unitFrameIntegrity.f : 1f;
            unitPersistent.ReplaceUnitFrameIntegrity (Mathf.Clamp01 (hp + value));

            #endif
        }
        
        public string GetLocalizedText ()
        {
            #if !PB_MODSDK

            var statInfo = DataMultiLinkerUnitStats.GetEntry (UnitStats.frameHealth, false);
            if (statInfo == null)
                return string.Empty;

            var text = TextUtility.GetPercentageFromNormalizedOffset (Mathf.Clamp01 (value));
            return $"[cc]{statInfo.textName}:[ff] {text}";

            #else
            return null;
            #endif
        }
    }
    
    public class OverworldUnitFrameIntegrityFromDifficulty : IOverworldUnitFunction, IFunctionLocalizedText
    {
        [InfoBox ("@GetLocalizedText ()", InfoMessageType.None)]
        [ValueDropdown ("@DataMultiLinkerDifficultySetting.data.Keys")]
        [LabelText ("Difficulty")]
        public string multiplier;

        public float value;

        public void Run (PersistentEntity unitPersistent)
        {
            #if !PB_MODSDK

            if (unitPersistent == null || !unitPersistent.isUnitTag)
                return;

            float hp = unitPersistent.hasUnitFrameIntegrity ? unitPersistent.unitFrameIntegrity.f : 1f;
            float valueFinal = value * DifficultyUtility.GetMultiplier (multiplier);
            unitPersistent.ReplaceUnitFrameIntegrity (Mathf.Clamp01 (hp + valueFinal));

            #endif
        }
        
        public string GetLocalizedText ()
        {
            #if !PB_MODSDK

            var statInfo = DataMultiLinkerUnitStats.GetEntry (UnitStats.frameHealth, false);
            if (statInfo == null)
                return string.Empty;

            float valueFinal = value * DifficultyUtility.GetMultiplier (multiplier);
            var text = TextUtility.GetPercentageFromNormalizedOffset (valueFinal);
            return $"[cc]{statInfo.textName}:[ff] {text}";

            #else
            return null;
            #endif
        }
    }
}