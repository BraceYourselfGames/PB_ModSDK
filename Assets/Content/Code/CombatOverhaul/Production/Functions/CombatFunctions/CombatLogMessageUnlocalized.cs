using System;
using PhantomBrigade.Data;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PhantomBrigade.Functions
{
    [Serializable, GUIColor (1f, 0.75f, 0.5f)]
    public class CombatLogMessageUnlocalized : ICombatFunction
    {
        [InlineButtonClear]
        [ValueDropdown ("@DataMultiLinkerUIColor.data.Keys")]
        [GUIColor ("GetColorPreview")]
        public string colorKey = null;
        
        [HideLabel, TextArea (0, 10)]
        public string text;

        public void Run ()
        {
            #if !PB_MODSDK
            
            if (string.IsNullOrEmpty (text))
                return;

            CIViewCombatEventLog.AddMessageFromCommSource ("enemy_boss_intercepted", text, colorKey);
            
            #endif
        }
        
        private Color colorFallback = Color.white.WithAlpha (1f);

        private Color GetColorPreview ()
        {
            var colorInfo = DataMultiLinkerUIColor.GetEntry (colorKey, false);
            if (colorInfo != null && colorInfo.colorCache != null)
                return colorInfo.colorCache.colorHover;
            else
                return colorFallback;
        }
    }
}