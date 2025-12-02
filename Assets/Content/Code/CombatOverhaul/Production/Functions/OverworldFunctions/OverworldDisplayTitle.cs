using System;
using PhantomBrigade.Data;

namespace PhantomBrigade.Functions
{
    [Serializable]
    public class OverworldDisplayTitle : IOverworldFunction
    {
        public float hue = 0f;
        public DataBlockLocString textPrimary = new DataBlockLocString ();
        public DataBlockLocString textSecondary = new DataBlockLocString ();
        public DataBlockLocString textTertiary = new DataBlockLocString ();

        public void Run ()
        {
            #if !PB_MODSDK

            var textPrimaryFinal = textPrimary != null ? textPrimary.text : string.Empty;
            var textSecondaryFinal = textSecondary != null ? textSecondary.text : string.Empty;
            var textTertiaryFinal = textTertiary != null ? textTertiary.text : string.Empty;
            
            CIViewOverworldTitle.ins.BeginAnimation (textPrimaryFinal, textSecondaryFinal, textTertiaryFinal, hue);
            
            #endif
        }
    }
}