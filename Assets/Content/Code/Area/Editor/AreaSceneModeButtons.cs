using System;

using Sirenix.OdinInspector;

namespace Area
{
    using Scene;

    [HideLabel, HideReferenceObjectPicker]
    sealed class ModeButtons
    {
        [ButtonLabels ("Stucture shape")]
        public EditingMode modeVolume => EditingMode.Volume;

        [ButtonLabels ("Terrain shape")]
        public EditingMode modeTerrainShape => EditingMode.TerrainShape;

        [ButtonLabels ("Copy/paste", "Copy\nPaste")]
        public EditingMode modeTransfer => EditingMode.Transfer;

        [ButtonLabels ("Spot tileset")]
        public EditingMode modeTileset => EditingMode.Tileset;

        [ButtonLabels ("Spot type & transform", "Spot type\n& transform")]
        public EditingMode modeSpot => EditingMode.Spot;

        [ButtonLabels ("Spot color")]
        public EditingMode modeColor => EditingMode.Color;

        [ButtonLabels ("Prop tool")]
        public EditingMode modeProp => EditingMode.Props;

        [ButtonLabels ("Road tool")]
        public EditingMode modeRoad => EditingMode.Roads;

        [ButtonLabels ("Road curve tool", "Road curve\ntool")]
        public EditingMode modeRoadCurve => EditingMode.RoadCurves;

        [ButtonLabels ("Nav tool")]
        public EditingMode modeNavigation => EditingMode.Navigation;

        [ButtonLabels ("Terrain ramp")]
        public EditingMode modeRamp => EditingMode.TerrainRamp;

        [ButtonLabels ("Volume damage")]
        public EditingMode modeDamage => EditingMode.Damage;

        [ButtonLabels ("Layers", "Layers", EnableIf = nameof(enableLayerMode))]
        public EditingMode modeLayers => EditingMode.Layers;

        public EditingMode mode
        {
            get => bb.editingMode;
            set => bb.editingMode = value;
        }

        bool enableLayerMode => !AreaManager.displayOnlyVolume;

        public ModeButtons (AreaSceneBlackboard bb)
        {
            this.bb = bb;
        }

        readonly AreaSceneBlackboard bb;
    }

    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Property, Inherited = false)]
    sealed class ButtonLabelsAttribute : Attribute
    {
        public readonly string Oneline;
        public readonly string Multiline;
        public string EnableIf;

        public ButtonLabelsAttribute (string oneline)
        {
            Oneline = oneline;
            Multiline = oneline.ReplaceFirst(" ", "\n");
        }

        public ButtonLabelsAttribute (string oneline, string multiline)
        {
            Oneline = oneline;
            Multiline = multiline;
        }
    }
}
