using System;
using PhantomBrigade.Data;
using Sirenix.OdinInspector;

namespace PhantomBrigade.Functions
{
    [Serializable]
    public class CombatUnitStatusRemove : ICombatFunctionTargeted
    {
        [ValueDropdown ("@DataMultiLinkerUnitStatus.data.Keys")]
        public string key = string.Empty;

        [PropertyRange (1, 10)]
        public int limit = 1;
        
        [InlineButtonClear]
        [ValueDropdown("@AudioEvents.GetKeys ()")]
        public string fxAudio;

        public void Run (PersistentEntity unitPersistent)
        {
            #if !PB_MODSDK
            
            var unitCombat = IDUtility.GetLinkedCombatEntity (unitPersistent);
            UnitStatusUtility.RemoveStatus (unitCombat, key, out bool success, limit);
            
            if (!string.IsNullOrEmpty (fxAudio) && success)
                AudioUtility.CreateAudioEvent (fxAudio, unitCombat.GetCenterPoint ());
            
            #endif
        }
    }
}