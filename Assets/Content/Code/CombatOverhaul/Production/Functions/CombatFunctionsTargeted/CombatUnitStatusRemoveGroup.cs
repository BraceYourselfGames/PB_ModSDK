using System;
using System.Collections.Generic;
using PhantomBrigade.Data;
using Sirenix.OdinInspector;

namespace PhantomBrigade.Functions
{
    [Serializable]
    public class CombatUnitStatusRemoveGroup : ICombatFunctionTargeted
    {
        [ValueDropdown ("@DataMultiLinkerUnitStatus.data.Keys")]
        public HashSet<string> keys = new HashSet<string> ();

        [PropertyRange (1, 10)]
        public int limit = 1;
        
        [InlineButtonClear]
        [ValueDropdown("@AudioEvents.GetKeys ()")]
        public string fxAudio;

        public void Run (PersistentEntity unitPersistent)
        {
            #if !PB_MODSDK

            bool success = false;
            var unitCombat = IDUtility.GetLinkedCombatEntity (unitPersistent);
            
            foreach (var key in keys)
            {
                if (string.IsNullOrEmpty (key))
                    continue;
                
                UnitStatusUtility.RemoveStatus (unitCombat, key, out bool successLocal, limit);
                if (successLocal)
                    success = true;
            }

            if (!string.IsNullOrEmpty (fxAudio) && success)
                AudioUtility.CreateAudioEvent (fxAudio, unitCombat.GetCenterPoint ());

            #endif
        }
    }
}