using Sirenix.Utilities;

namespace Area
{
    sealed class AreaSceneRoadCurveModePanel : AreaSceneModePanel
    {
        public GUILayoutOptions.GUILayoutOptionsInstance Width => AreaScenePanelDrawer.ModePanelWidth;
        public string Title => "Road Curve tool";

        public void OnDisable () { }

        public void Draw () { }

        public AreaSceneModeHints Hints => hints;

        public AreaSceneRoadCurveModePanel () { }

        static readonly AreaSceneModeHints hints = new AreaSceneModeHints ()
        {
            LeaderText = "Click on a turn to smooth it",
            HintText = "[LMB] - Smooth     [RMB] - Angled",
        };
    }
}
