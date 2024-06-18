using PhantomBrigade.Data.UI;

namespace PhantomBrigade.Data
{
    /// <summary>
    /// Short class working as an alias for various unique data containers
    /// </summary>
    
    public static class DataShortcuts
    {
        public static DataContainerSettingsAnimation anim => DataLinkerSettingsAnimation.data;
        public static DataContainerSettingsArea area => DataLinkerSettingsArea.data;
        public static DataContainerSettingsSimulation sim => DataLinkerSettingsSimulation.data;
        public static DataContainerSettingsDebug debug => DataLinkerSettingsDebug.dataOverridden;
        public static DataContainerSettingsCamera cam => DataLinkerSettingsCamera.data;
        public static DataContainerRendering render => DataLinkerRendering.data;
        public static DataContainerSettingsAI ai => DataLinkerSettingsAI.data;
        public static DataContainerUI ui => DataLinkerUI.data;
        public static DataBlockDebugDrawing uiDebug => DataLinkerUI.data.debugDrawing;

        public static DataContainerSettingsMusic music => DataLinkerSettingsMusic.data;

        public static DataContainerSettingsAudio audio => DataLinkerSettingsAudio.data;

        public static DataContainerSettingsOverworld overworld => DataLinkerSettingsOverworld.data;

        public static DataContainerSettingsEscalation escalation => DataLinkerSettingsEscalation.data;
        
        public static DataContainerSettingsPilot pilots => DataLinkerSettingsPilot.data;
    }
}

