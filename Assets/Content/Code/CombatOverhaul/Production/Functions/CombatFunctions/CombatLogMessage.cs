using System;
using PhantomBrigade.Data;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PhantomBrigade.Functions
{
    [Serializable]
    public class CombatLogMessage : ICombatFunction
    {
        public DataBlockLocString data = new DataBlockLocString ();
        
        [InlineButtonClear]
        [ValueDropdown ("@DataMultiLinkerUIColor.data.Keys")]
        [GUIColor ("GetColorPreview")]
        public string colorKey = null;
        
        public void Run ()
        {
            #if !PB_MODSDK
            
            if (data == null)
                return;

            var text = DataManagerText.GetText (data.textSector, data.textKey);
            if (string.IsNullOrEmpty (colorKey))
                CIViewOverworldLog.AddMessage (text);
            else
                CIViewOverworldLog.AddMessage (text, colorKey);
            
            #endif
        }
        
        private Color colorFallback = Color.white.WithAlpha (1f);

        private Color GetColorPreview ()
        {
            var colorInfo = DataMultiLinkerUIColor.GetEntry (colorKey);
            if (colorInfo != null && colorInfo.colorCache != null)
                return colorInfo.colorCache.colorHover;
            else
                return colorFallback;
        }
    }
}