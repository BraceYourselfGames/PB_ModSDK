using System;
using System.Collections.Generic;
using System.Linq;

using PhantomBrigade.Data;

using Sirenix.OdinInspector;

using UnityEditor;
using UnityEngine;

namespace Area
{
    using Scene;

    [Serializable]
    class TilesetTableEntry
    {
        [DisplayAsString, HideLabel]
        public string name;

        [ShowInInspector]
        [HorizontalGroup ("Select")]
        [ShowIf (nameof(isSelected))]
        [DisplayAsString, HideLabel]
        public static readonly string selected = "Selected";

        [HorizontalGroup ("Select")]
        [HideIf (nameof(isSelected))]
        [Button]
        void Select ()
        {
            SetSelectedTileset ();
            bb.spotTilesetSelected = tileset;
            var view = EditorWindow.GetWindow<SceneView>();
            view.Repaint();
        }

        [HorizontalGroup ("Select")]
        [Button]
        void Fill ()
        {
            SetSelectedTileset ();
            foreach (var point in am.points)
            {
                point.blockTileset = tileset.id;
            }
            am.RebuildEverything ();
        }

        [HideInInspector]
        public AreaManager am;
        [HideInInspector]
        public AreaSceneBlackboard bb;
        [HideInInspector]
        public AreaTileset tileset;

        bool isSelected => bb.editingMode == EditingMode.Volume
            ? bb.volumeTilesetSelected != null && tileset == bb.volumeTilesetSelected
            : bb.spotTilesetSelected != null && tileset == bb.spotTilesetSelected;

        void SetSelectedTileset ()
        {
            if (bb.editingMode == EditingMode.Volume)
            {
                bb.volumeTilesetSelected = tileset;
            }
            else
            {
                bb.spotTilesetSelected = tileset;
            }
        }
    }

    [Serializable]
    class PropTableEntry
    {
        [ShowInInspector]
        [TableColumnWidth (45, Resizable = false)]
        [DisplayAsString, HideLabel, EnableGUI]
        public int ID => prototype?.id ?? 0;

        [ShowInInspector]
        [DisplayAsString, HideLabel, EnableGUI]
        public string name => prototype?.prefab.name ?? "";

        [ShowIf (nameof(showSelectButton))]
        [HorizontalGroup ("Select")]
        [TableColumnWidth (66, Resizable = false)]
        [Button]
        void Select ()
        {
            if (propEditInfo == null || prototype == null)
            {
                return;
            }

            propEditInfo.SelectionID = ID;
            propEditInfo.Index = AreaAssetHelper.propsPrototypesList.IndexOf (prototype);
            var view = EditorWindow.GetWindow<SceneView>();
            view.Repaint();
        }

        [ShowInInspector]
        [HorizontalGroup ("Select")]
        [PropertyOrder (1)]
        [ShowIf (nameof(isSelected))]
        [DisplayAsString, HideLabel]
        public static readonly string selected = "Selected";

        [HideInInspector]
        public AreaPropPrototypeData prototype;
        [HideInInspector]
        public PropEditInfo propEditInfo;

        bool isSelected => prototype != null && propEditInfo.SelectionID == prototype.id;
        bool showSelectButton => !isSelected && prototype != null;
    }

    [Serializable]
    class PaletteEntry
    {
        [HideInInspector]
        public int tilesetId;

        [ShowInInspector]
        [HorizontalGroup ("PaleteEntry", Width = 95f)]
        [PropertyOrder (-1)]
        [DisplayAsString, HideLabel, EnableGUI]
        public string tilesetName => AreaTilesetHelper.database.tilesets[tilesetId].name;

        [HorizontalGroup ("PaleteEntry")]
        [HideLabel]
        public string tilesetDescription;

        [HorizontalGroup ("PaleteEntry", Width = 45f)]
        [CustomValueDrawer ("SwatchDrawer")]
        [HideLabel]
        public HSBColor primaryColor;

        [HorizontalGroup ("PaleteEntry", Width = 45f)]
        [CustomValueDrawer ("SwatchDrawer")]
        [HideLabel]
        public HSBColor secondaryColor;

        [HorizontalGroup ("PaleteEntry", Width = 45f)]
        [PropertySpace (2)]
        [Button]
        public void Apply () => updateColorInfo (this);

        public PaletteEntry()
        {
            tilesetId = 0;
            tilesetDescription = "";
            primaryColor = HSBColor.FromColor(Color.gray);
            secondaryColor = HSBColor.FromColor(Color.gray);
        }

        public PaletteEntry (Action<PaletteEntry> updateColorInfo)
            : this ()
        {
            SetContext (updateColorInfo);
        }

        public PaletteEntry(PaletteEntry other)
        {
            tilesetId = other.tilesetId;
            tilesetDescription = other.tilesetDescription;
            primaryColor = other.primaryColor;
            secondaryColor = other.secondaryColor;
        }

        public void SetContext (Action<PaletteEntry> updateColorInfo)
        {
            this.updateColorInfo = updateColorInfo;
        }

        public static HSBColor SwatchDrawer (HSBColor hsbc, GUIContent label) =>
            HSBColor.FromColor (EditorGUILayout.ColorField (hsbc.ToColor ()));

        Action<PaletteEntry> updateColorInfo;
    }

    [Serializable]
    class VolumeSnippetEntry
    {
        // XXX this is wrong -- see Load method below
        public const string snippetsPath = "Configs/LevelSnippets";

        [HorizontalGroup ("VolumeSnippet")]
        [DisplayAsString, HideLabel]
        public string key;

        [HorizontalGroup ("VolumeSnippet", Width = 25f)]
        [PropertyTooltip ("Load volume snippet to clipboard")]
        [Button ("◄")]
        public void Load ()
        {
            // XXX I broke this because I got confused with LevelSnippets. LevelSnippets are miniature levels.
            // XXX clipboard snippets are a different serialization format. See AreaClipboardSerialized
            var path = DataPathHelper.GetCombinedCleanPath (DataPathHelper.GetApplicationFolder (), snippetsPath);
            var filename = DataPathHelper.GetCombinedCleanPath (path, key);
            if (filename.Length != 0)
            {
                //am.clipboard?.LoadFromYAML (filename);
            }
        }

        [HideInInspector]
        public AreaManager am;
    }

    [Serializable]
    struct NavigationLegendEntry
    {
        [GUIColor ("$" + nameof(sceneColor))]
        [TableColumnWidth (55, false), DisplayAsString]
        public string color;
        [DisplayAsString]
        public string feature;
        [HideInInspector]
        public Color sceneColor;
    }

    [HideReferenceObjectPicker]
    public sealed class LayerStats
    {
        public LayerStats (int layer)
        {
            Layer = layer;
        }

        [HideInInspector]
        public readonly int Layer;
        public string Name => "Layer " + Layer;

        [DisplayAsString]
        public int Points;
        [DisplayAsString]
        public int EmptyPoints;
        [DisplayAsString]
        public int FullPoints;
        [DisplayAsString]
        public int DestroyedPoints;
        [DisplayAsString]
        public int MarkedDestructible;
        [DisplayAsString]
        public int ComputedDestructible;
        [DisplayAsString]
        public int Colliders;
        [DisplayAsString]
        public int FullConfigurations;
        [DisplayAsString]
        public int EmptyConfigurations;
        [DisplayAsString]
        public int MarkedRoads;
        [DisplayAsString]
        public int Spots;
        [DisplayAsString]
        public int UntiledSpots;
        [DisplayAsString]
        public int RoadSpots;
        [DisplayAsString]
        public int TerrainSpots;
        [DisplayAsString]
        public int ModifiedTerrain;
    }

    sealed class ConfigurationData
    {
        [FoldoutGroup (OdinGroup.Name.ConfigurationData)]
        [PropertyOrder (OdinGroup.Order.Load)]
        [Button ("Load configuration data")]
        public void Load ()
        {
            data = AreaTilesetHelper.database.configurationDataForBlocks.Where (cfg => cfg != null).ToList ();
            PopulateList ();
        }

        void PopulateList ()
        {
            if (!filterUsedInternal || filterBy == FilterType.None)
            {
                dataFiltered.Clear ();
                dataFiltered.AddRange (data);
                return;
            }
            if (filterBy == FilterType.Configuration)
            {
                FilterByConfiguration ();
                return;
            }
            FilterByReference ();
        }

        [ShowInInspector]
        [ToggleGroup (OdinGroup.Name.Filter, "Filter", VisibleIf = nameof(ready), Order = OdinGroup.Order.Filter)]
        public bool filterUsed
        {
            get => filterUsedInternal;
            set
            {
                filterUsedInternal = value;
                PopulateList ();
            }
        }
        bool filterUsedInternal;

        [ToggleGroup (OdinGroup.Name.Filter)]
        [HorizontalGroup (OdinGroup.Name.FilterInput)]
        [GUIColor (nameof(filterButtonColor))]
        [LabelText ("Filter"), LabelWidth (50f)]
        public string filter = string.Empty;

        [HorizontalGroup (OdinGroup.Name.FilterInput)]
        [GUIColor (nameof(filterButtonColor))]
        [LabelText ("Bit string")]
        public bool filterByBitString;

        [ButtonGroup (OdinGroup.Name.FilterButtons)]
        [EnableIf (nameof(enableFilterButtons))]
        [GUIColor (nameof(filterButtonColor))]
        public void FilterByConfiguration ()
        {
            filterBy = FilterType.Configuration;
            dataFiltered.Clear ();

            if (filterByBitString)
            {
                dataFiltered.AddRange(data.Where (acd => acd.configurationAsString.StartsWith (filter)));
                return;
            }

            var v = byte.Parse (filter);
            dataFiltered.AddRange(data.Where (acd => acd.configuration == v));
        }

        [ButtonGroup (OdinGroup.Name.FilterButtons)]
        [EnableIf (nameof(enableFilterButtons))]
        [GUIColor (nameof(filterButtonColor))]
        public void FilterByReference()
        {
            filterBy = FilterType.Reference;
            dataFiltered.Clear ();

            IEnumerable<byte> transformed;
            if (filterByBitString)
            {
                var references = AreaTilesetHelper.configurationOrder.Where (co => TilesetUtility.GetStringFromConfiguration (co).StartsWith (filter));
                transformed = references.SelectMany(co =>
                    Enumerable.Range (0, 8).Select (r =>
                    {
                        var requiredRotation = r % 4;
                        var requiredFlipping = r > 3;
                        return TilesetUtility.GetConfigurationTransformed (co, requiredRotation, requiredFlipping);
                    }));
            }
            else
            {
                var v = byte.Parse (filter);
                var idx = Array.BinarySearch (AreaTilesetHelper.configurationOrder, 0, AreaTilesetHelper.configurationOrder.Length, v);
                if (idx < 0)
                {
                    dataFiltered.Clear();
                    return;
                }
                transformed = Enumerable.Range (0, 8).Select (r =>
                {
                    var requiredRotation = r % 4;
                    var requiredFlipping = r > 3;
                    return TilesetUtility.GetConfigurationTransformed (v, requiredRotation, requiredFlipping);
                });
            }
            var transformedSet = new HashSet<byte> (transformed);
            dataFiltered.AddRange(data.Where (acd => transformedSet.Contains (acd.configuration)));
        }

        bool enableFilterButtons
        {
            get
            {
                if (string.IsNullOrEmpty (filter))
                {
                    return false;
                }
                return filterByBitString
                    ? filter.Length <= 8 && filter.ToCharArray ().All (c => c == '0' || c == '1')
                    : int.TryParse (filter, out var v) && v >= byte.MinValue && v <= byte.MaxValue;
            }
        }

        Color filterButtonColor => filterByBitString ? colorFilterBitString : colorFilterNormal;
        static readonly Color colorFilterNormal = new Color (1f, 1f, 1f, 1f);
        static readonly Color colorFilterBitString = new Color (0.7f, 0.9f, 1f, 1f);

        [FoldoutGroup (OdinGroup.Name.ConfigurationData)]
        [ShowIf (nameof(ready))]
        [PropertyOrder (OdinGroup.Order.List)]
        [TableList (AlwaysExpanded = true, IsReadOnly = true, ShowPaging = true)]
        [CompactFormat]
        [LabelText ("Configurations")]
        public List<AreaConfigurationData> dataFiltered = new List<AreaConfigurationData> ();

        // Keep a separate backing list of all the data. If you try to show this list in the UI by
        // using an Odin TableList attribute on it and then toggle between it and the dataFiltered
        // list with ShowIf attributes, the TableListAttributeDrawer does strange things.
        List<AreaConfigurationData> data;
        bool ready => data != null;
        FilterType filterBy;

        enum FilterType
        {
            None = 0,
            Configuration,
            Reference,
        }

        public static class OdinGroup
        {
            public static class Name
            {
                public const string ConfigurationData = "Configuration data";
                public const string Filter = ConfigurationData + "/" + nameof(filterUsed);
                public const string FilterInput = Filter + "/Input";
                public const string FilterButtons = Filter + "/Buttons";
                public const string List = Filter + "/List";
                public const string ListFiltered = Filter + "/List (filtered)";
            }

            public static class Order
            {
                public const float Load = 0f;
                public const float Filter = 1f;
                public const float List = 2f;
            }
        }

        [AttributeUsage (AttributeTargets.Field | AttributeTargets.Property)]
        public sealed class CompactFormatAttribute : ShowInInspectorAttribute { }
    }

    [Serializable]
    [HideLabel, HideReferenceObjectPicker]
    sealed class InspectorPropSelector
    {
        [Button ("Reload DB")]
        [PropertyOrder (0f)]
        public void ReloadAssetDatabase ()
        {
            AreaAssetHelper.LoadResources ();
            Populate ();
        }

        [ShowInInspector]
        [PropertyOrder (1f)]
        public string propFilter
        {
            get => bb.propFilter;
            set => bb.propFilter = value;
        }

        [ShowInInspector]
        [PropertyOrder (2f)]
        [TableList (AlwaysExpanded = true, IsReadOnly = true, ShowPaging = true)]
        public readonly List<PropTableEntry> props;

        public void Populate () => AreaSceneModeHelper.PopulatePropList (bb);

        public InspectorPropSelector (AreaSceneBlackboard bb)
        {
            this.bb = bb;
            props = bb.props;
            bb.onPropListChanged += Populate;
        }

        readonly AreaSceneBlackboard bb;
    }

    [Serializable]
    [HideLabel, HideReferenceObjectPicker]
    sealed class SliceControls
    {
        [ShowInInspector]
        [ShowIf (nameof(showSliceControls))]
        [EnableIf (nameof(enable))]
        [PropertyRange (0, nameof(sliceDepthLimit))]
        public int sliceDepth
        {
            get => AreaManager.sliceDepth;
            set
            {
                AreaManager.sliceDepth = Mathf.Clamp (value, 0, sliceDepthLimit);
                bb.am.UpdateSlicing ();
                RedrawSceneView ();
            }
        }

        [ShowInInspector]
        [ShowIf (nameof(showSliceControls))]
        [EnableIf (nameof(enable))]
        [PropertyRange (0f, 1f)]
        public float sliceOpacity
        {
            get => CombatSceneHelper.ins != null && CombatSceneHelper.ins.materialHelper != null ? CombatSceneHelper.ins.materialHelper.sliceColor.a : 0f;
            set
            {
                if (CombatSceneHelper.ins == null || CombatSceneHelper.ins.materialHelper == null)
                {
                    return;
                }
                var m = CombatSceneHelper.ins.materialHelper;
                var sliceAlpha = Mathf.Clamp (value, 0f, 1f);
                m.sliceColor = new Color (m.sliceColor.r, m.sliceColor.g, m.sliceColor.b, sliceAlpha);
                bb.am.UpdateSlicing ();
                RedrawSceneView ();
            }
        }

        [HorizontalGroup]
        [EnableIf (nameof(enable))]
        [Button ("$" + nameof(buttonLabelVolumeView), ButtonHeight = 24)]
        public void ToggleVolumeView ()
        {
            bb.am.SetVolumeDisplayMode (!AreaManager.displayOnlyVolume);
            RedrawSceneView ();
        }

        [HorizontalGroup]
        [EnableIf (nameof(enable))]
        [Button ("$" + nameof(buttonLabelPropView), ButtonHeight = 24)]
        public void TogglePropView ()
        {
            AreaManager.displayProps = !AreaManager.displayProps;
            bb.am.RebuildEverything ();
            RedrawSceneView ();
        }

        [HorizontalGroup]
        [EnableIf (nameof(enable))]
        [Button ("$" + nameof(buttonLabelSlicing), ButtonHeight = 24)]
        public void ToggleSlicing ()
        {
            AreaManager.sliceEnabled = !AreaManager.sliceEnabled;
            bb.am.UpdateSlicing ();
            RedrawSceneView ();
        }

        bool showSliceControls => AreaManager.sliceEnabled;
        bool enable => bb.editingMode != EditingMode.Layers;
        string buttonLabelVolumeView => AreaManager.displayOnlyVolume ? "Show everything" : "Show only volume";
        string buttonLabelPropView => AreaManager.displayProps ? "Hide props" : "Show props";
        string buttonLabelSlicing => AreaManager.sliceEnabled ? "Disable slicing" : "Enable slicing";
        int sliceDepthLimit => bb?.am != null ? Mathf.Max (0, bb.am.boundsFull.y - 2) : 0;

        static void RedrawSceneView ()
        {
            EditorWindow view = EditorWindow.GetWindow<SceneView>();
            if (view != null)
            {
                view.Repaint ();
            }
        }

        public SliceControls (AreaSceneBlackboard bb)
        {
            this.bb = bb;
        }

        readonly AreaSceneBlackboard bb;
    }

    [AttributeUsage (AttributeTargets.Field | AttributeTargets.Property)]
    sealed class DisplayAsMultilineStringAttribute : ShowInInspectorAttribute { }

    [AttributeUsage (AttributeTargets.Field | AttributeTargets.Property)]
    sealed class InspectorSurrogateGroupAttribute : ShowInInspectorAttribute
    {
        public readonly int ButtonsPerRow;

        public InspectorSurrogateGroupAttribute (int buttonsPerRow)
        {
            ButtonsPerRow = buttonsPerRow;
        }
    }

    [AttributeUsage (AttributeTargets.Field | AttributeTargets.Property)]
    sealed class InspectorSurrogateGroupButtonAttribute : ShowInInspectorAttribute
    {
        public readonly string LabelText;
        public readonly string CurrentMode;
        public readonly string SetMode;
        public string EnableIf;

        public InspectorSurrogateGroupButtonAttribute (string labelText, string modeProperty)
        {
            LabelText = labelText;
            CurrentMode = modeProperty;
            SetMode = "@" + modeProperty + " = $value";
        }
    }
}
