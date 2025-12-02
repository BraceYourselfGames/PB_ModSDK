using System.Collections.Generic;
using PhantomBrigade.Data;
using Sirenix.OdinInspector;

namespace PhantomBrigade.Functions
{
    public class UnlockCodex : IOverworldFunction
    {
        [ValueDropdown("@DataMultiLinkerCodex.GetKeys ()")]
        public string key;

        public void Run ()
        {
            #if !PB_MODSDK
            
            CodexUtility.TryUnlockEntry (key);
            
            #endif
        }
    }
    
    public class UnlockCodexGroup : IOverworldFunction
    {
        [ValueDropdown("@DataMultiLinkerCodex.GetKeys ()")]
        public HashSet<string> keys = new HashSet<string> ();

        public void Run ()
        {
            #if !PB_MODSDK
            
            CodexUtility.TryUnlockEntries (keys);
            
            #endif
        }
    }
}