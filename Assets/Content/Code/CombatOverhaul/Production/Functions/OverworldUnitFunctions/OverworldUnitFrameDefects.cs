using System;
using PhantomBrigade.Data;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PhantomBrigade.Functions
{
    public class OverworldUnitFrameDefectsSet : IOverworldUnitFunction, IFunctionLocalizedText
    {
        [InfoBox ("@GetLocalizedText ()", InfoMessageType.None)]
        public int value;

        public void Run (PersistentEntity unitPersistent)
        {
            #if !PB_MODSDK

            if (unitPersistent == null || !unitPersistent.isUnitTag)
                return;
            
            int defectLimit = DataShortcuts.sim.mechDamageLimit;
            unitPersistent.ReplaceUnitFrameDefects (Mathf.Clamp (value, 0, defectLimit));

            #endif
        }
        
        public string GetLocalizedText ()
        {
            #if !PB_MODSDK

            var statInfo = DataMultiLinkerUnitStats.GetEntry (UnitStats.frameFractures, false);
            if (statInfo == null)
                return string.Empty;

            var text = TextUtility.GetPercentageFromNormalizedValue (value);
            return $"[cc]{statInfo.textName}:[ff] {text}";

            #else
            return null;
            #endif
        }
    }
    
    public class OverworldUnitFrameDefectsOffset : IOverworldUnitFunction, IFunctionLocalizedText
    {
        [InfoBox ("@GetLocalizedText ()", InfoMessageType.None)]
        public int value;

        public void Run (PersistentEntity unitPersistent)
        {
            #if !PB_MODSDK

            if (unitPersistent == null || !unitPersistent.isUnitTag)
                return;
            
            int defectLimit = DataShortcuts.sim.mechDamageLimit;
            int defectsCurrent = unitPersistent.hasUnitFrameDefects ? unitPersistent.unitFrameDefects.i : 0;
            unitPersistent.ReplaceUnitFrameDefects (Mathf.Clamp (defectsCurrent + value, 0, defectLimit));

            #endif
        }
        
        public string GetLocalizedText ()
        {
            #if !PB_MODSDK

            var statInfo = DataMultiLinkerUnitStats.GetEntry (UnitStats.frameFractures, false);
            if (statInfo == null)
                return string.Empty;

            var text = TextUtility.GetPercentageFromNormalizedOffset (value);
            return $"[cc]{statInfo.textName}:[ff] {text}";

            #else
            return null;
            #endif
        }
    }
}