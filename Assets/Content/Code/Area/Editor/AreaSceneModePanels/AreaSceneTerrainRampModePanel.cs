using Sirenix.Utilities;

namespace Area
{
    sealed class AreaSceneTerrainRampModePanel : AreaSceneModePanel
    {
        public GUILayoutOptions.GUILayoutOptionsInstance Width => AreaScenePanelDrawer.ModePanelWidth;
        public string Title => "Terrain Ramp mode";

        public void OnDisable () { }

        public void Draw () { }

        public AreaSceneModeHints Hints => hints;

        public AreaSceneTerrainRampModePanel () { }

        static readonly AreaSceneModeHints hints = new AreaSceneModeHints ()
        {
            LeaderText = "Click on the top point of a ramp",
            HintText = "[LMB] - Rampify     [RMB] - Un-rampify     [Shift] - Strict eligibility check (block corner ramps)",
        };
    }
}
