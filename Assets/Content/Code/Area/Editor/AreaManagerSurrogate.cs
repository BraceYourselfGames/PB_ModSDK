using System.Collections.Generic;
using System.IO;
using System.Linq;

using PhantomBrigade.Data;

using Sirenix.OdinInspector;

using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

#if PB_MODSDK
using PhantomBrigade.SDK.ModTools;
#endif

namespace Area
{
    using Scene;

    sealed class AreaManagerSurrogate
    {
        #if PB_MODSDK
        bool hasSelectedMod => DataContainerModData.selectedMod != null;

        [ShowInInspector, PropertyOrder (-100), HideReferenceObjectPicker, HideLabel, HideDuplicateReferenceBox]
        private static ModConfigStatus status = new ModConfigStatus ();

        public static string configsPath => DataContainerModData.selectedMod.GetModPathConfigs ();
        #endif

        [ShowInInspector]
        [BoxGroup (OdinGroup.Name.LevelSelector, false, Order = OdinGroup.Order.LevelSelector)]
        [GUIColor (nameof(selectionColor))]
        [InfoBox ("$" + nameof(areaHint))]
        [LabelText ("Combat Area")]
        public string areaKeyCurrent => dataLoaded ? DataMultiLinkerCombatArea.selectedArea.key : selectionNone;

        [BoxGroup (OdinGroup.Name.LevelSelector)]
        [Button (SdfIconType.BoxArrowUp, "$" + nameof(combatAreaButtonName), ButtonHeight = 32)]
        public void OpenParentInspector () => AreaSceneHelper.ReturnToAreaDB ();

        [ButtonGroup (OdinGroup.Name.LevelSelectorButtons, VisibleIf = nameof(dataLoaded))]
        [Button (SdfIconType.XSquare, "Unload", ButtonHeight = 32)]
        public void UnloadSelectedArea ()
        {
            var dataSelected = DataMultiLinkerCombatArea.selectedArea;
            dataSelected.UnloadFromScene ();
        }

        [ButtonGroup (OdinGroup.Name.LevelSelectorButtons)]
        [Button (SdfIconType.ArrowRepeat, "Reload", ButtonHeight = 32)]
        public void ReloadSelectedArea ()
        {
            var dataSelected = DataMultiLinkerCombatArea.selectedArea;
            var entryReloaded = DataMultiLinkerCombatArea.LoadDataIsolated (dataSelected.key);
            entryReloaded?.SelectAndApplyToScene ();
            bb.onLevelLoaded?.Invoke ();
        }

        [ButtonGroup (OdinGroup.Name.LevelSelectorButtons)]
        [EnableIf (nameof(editable))]
        [Button (SdfIconType.Save, "Save", ButtonHeight = 32)]
        public void SaveSelectedArea () => AreaSceneHelper.SaveSelectedLevel ();

        [FoldoutGroup (OdinGroup.Name.BoundsAndStats, false, VisibleIf = nameof(dataLoaded), Order = OdinGroup.Order.BoundsAndStats)]
        #if PB_MODSDK
        [HorizontalGroup (OdinGroup.Name.BoundsAndStatsBoundsReadonly, VisibleIf = "@!" + nameof(showBoundsChanges))]
        #else
        [HorizontalGroup (OdinGroup.Name.BoundsAndStatsBoundsReadonly)]
        #endif
        [LabelText ("Bounds:"), LabelWidth (50f), DisplayAsString]
        public string boundsDisplay;

        #if PB_MODSDK
        [HorizontalGroup (OdinGroup.Name.BoundsAndStatsBounds, Gap = 15, VisibleIf = nameof(showBoundsChanges))]
        [OnValueChanged (nameof(UpdateBounds))]
        [LabelText ("X"), LabelWidth (15)]
        public int boundsX;

        [HorizontalGroup (OdinGroup.Name.BoundsAndStatsBounds)]
        [OnValueChanged (nameof(UpdateBounds))]
        [LabelText ("Z"), LabelWidth (15)]
        public int boundsZ;

        [HorizontalGroup (OdinGroup.Name.BoundsAndStatsBounds)]
        [OnValueChanged (nameof(UpdateBounds))]
        [LabelText ("Y"), LabelWidth (15)]
        public int boundsY;

        [HorizontalGroup (OdinGroup.Name.BoundsAndStatsBoundsButtons)]
        [EnableIf (nameof(boundsChanged))]
        [PropertySpace (2f)]
        [Button ("Reset")]
        public void ResetBounds ()
        {
            var bounds = bb.am.boundsFull;
            boundsX = bounds.x;
            boundsY = bounds.y;
            boundsZ = bounds.z;
            boundsUpdated = bounds;
            boundsChanged = false;
        }

        [HorizontalGroup (OdinGroup.Name.BoundsAndStatsBoundsButtons)]
        [EnableIf (nameof(boundsChanged))]
        [PropertySpace (2f)]
        [Button ("Apply")]
        public void ApplyBounds ()
        {
            bb.am.RemapLevel (boundsUpdated);
            boundsDisplay = AreaSceneHelper.FormatBounds (bb.am.boundsFull);
        }

        bool boundsChanged;
        Vector3Int boundsUpdated;

        void UpdateBounds ()
        {
            var bounds = bb.am.boundsFull;
            var boundsNew = new Vector3Int (boundsX, boundsY, boundsZ);
            if (!AreaSceneHelper.ValidateBounds (boundsNew))
            {
                boundsChanged = false;
                boundsUpdated = bounds;
                return;
            }
            boundsChanged = boundsNew != bounds;
            boundsUpdated = boundsChanged ? boundsNew : bounds;
        }
        #endif

        [FoldoutGroup (OdinGroup.Name.BoundsAndStats)]
        [PropertySpace (0f, 4f)]
        [HideLabel, DisplayAsMultilineString]
        public string stats;

        [ShowInInspector]
        [FoldoutGroup (OdinGroup.Name.BoundsAndStats)]
        [ListDrawerSettings (DraggableItems = false, HideAddButton = true, HideRemoveButton = true, IsReadOnly = true, ShowFoldout = true, ListElementLabelName = nameof(LayerStats.Name))]
        public List<LayerStats> layers = new List<LayerStats> ();

        [FoldoutGroup (OdinGroup.Name.Updates, false, VisibleIf = nameof(dataLoaded), Order = OdinGroup.Order.Updates)]
        [HorizontalGroup (OdinGroup.Name.UpdatesButtons)]
        [VerticalGroup (OdinGroup.Name.UpdatesButtonsLeft)]
        [Button ("Rebuild everything (1-6)\n►►►", 118)]
        public void RebuildEverything ()
        {
            bb.am.rebuildCount = 0;
            bb.am.RebuildEverything ();
        }

        [VerticalGroup (OdinGroup.Name.UpdatesButtonsRight)]
        [Button ("Update volume (1)")]
        public void UpdateVolume () => bb.am.UpdateVolume (false);

        [VerticalGroup (OdinGroup.Name.UpdatesButtonsRight)]
        [PropertySpace (2)]
        [Button ("Update spots (2)")]
        public void UpdateSpots () => bb.am.UpdateAllSpots (false);

        [VerticalGroup (OdinGroup.Name.UpdatesButtonsRight)]
        [PropertySpace (2)]
        [Button ("Update damage (3)")]
        public void UpdateDamage () => bb.am.RebuildAllBlockDamage ();

        [VerticalGroup (OdinGroup.Name.UpdatesButtonsRight)]
        [PropertySpace (2)]
        [Button ("Apply vertex colors (4)")]
        public void ApplyVertexColors () => bb.am.ApplyShaderPropertiesEverywhere ();

        [VerticalGroup (OdinGroup.Name.UpdatesButtonsRight)]
        [PropertySpace (2)]
        [Button ("Update collisions (5)")]
        public void UpdateCollisions () => bb.am.RebuildCollisions ();

        [VerticalGroup (OdinGroup.Name.UpdatesButtonsRight)]
        [PropertySpace (2)]
        [Button ("Update globals (6)")]
        public void UpdateGlobals () => bb.am.UpdateShaderGlobals ();

        [ShowInInspector]
        [FoldoutGroup (OdinGroup.Name.View, false, VisibleIf = nameof(dataLoaded), Order = OdinGroup.Order.View)]
        public readonly SliceControls sliceControls;

        [FoldoutGroup (OdinGroup.Name.Cleanup, false, VisibleIf = nameof(dataLoaded), Order = OdinGroup.Order.Cleanup)]
        [HorizontalGroup (OdinGroup.Name.CleanupButtons)]
        [Button ("Reset prop\noffsets", ButtonHeight = 40)]
        public void ResetPropOffsets () => bb.am.ResetPropOffsets ();

        [HorizontalGroup (OdinGroup.Name.CleanupButtons)]
        [Button ("Reset\nsubtyping", ButtonHeight = 40)]
        public void ResetBlockSubtyping () => bb.am.ResetSubtyping ();

        [HorizontalGroup (OdinGroup.Name.CleanupButtons)]
        [Button ("Reset\ndamage", ButtonHeight = 40)]
        public void ResetVolumeDamage () => bb.am.ResetVolumeDamage ();

        [HorizontalGroup (OdinGroup.Name.CleanupButtons)]
        [Button ("Erase & reset\neverything", ButtonHeight = 40)]
        public void EraseAndResetAll () => bb.am.EraseAndResetScene ();

        [ShowInInspector]
        [FoldoutGroup (OdinGroup.Name.EditingMode, Expanded = true, VisibleIf = nameof(dataLoaded), Order = OdinGroup.Order.EditingMode)]
        [PropertyOrder (OdinGroup.SubGroupOrder.EditingModeToggle)]
        [PropertySpace (0f, 2f)]
        [ToggleLeft]
        public bool showEditingModeButtonsInScene
        {
            get => bb.showModeToolbar;
            set
            {
                bb.showModeToolbar = value;
                bb.repaintScene = true;
            }
        }

        [FoldoutGroup (OdinGroup.Name.EditingMode)]
        [PropertyOrder (OdinGroup.SubGroupOrder.EditingModeButtons)]
        [InspectorSurrogateGroup (3)]
        public ModeButtons modeButtons;

        [FoldoutGroup (OdinGroup.Name.Tilesets, false, VisibleIf = nameof(dataLoaded), Order = OdinGroup.Order.Tilesets)]
        [Button ("Reload DB")]
        public void ReloadTilesetDatabase ()
        {
            AreaTilesetHelper.LoadDatabase ();
            PopulateTilesetLists ();
        }

        void PopulateTilesetLists ()
        {
            mainTilesets.Clear ();
            interiorTilesets.Clear ();
            if (AreaTilesetHelper.database == null)
            {
                return;
            }
            if (AreaTilesetHelper.database.tilesets == null)
            {
                return;
            }

            foreach (var value in AreaTilesetHelper.database.tilesets.Values)
            {
                if (bb.editingMode == EditingMode.Volume && !AreaSceneHelper.volumeTilesetIDs.Contains (value.id))
                {
                    continue;
                }

                var tte = new TilesetTableEntry ()
                {
                    am = bb.am,
                    bb = bb,
                    tileset = value,
                    name = value.name,
                };

                if (value.usedAsInterior)
                {
                    if (bb.editingMode != EditingMode.Volume)
                    {
                        interiorTilesets.Add (tte);
                    }
                    continue;
                }
                mainTilesets.Add (tte);
            }
        }

        [FoldoutGroup (OdinGroup.Name.Tilesets)]
        [HideIf (nameof(mainTilesetsEmpty))]
        [PropertyOrder (1)]
        [TableList (AlwaysExpanded = true, IsReadOnly = true)]
        public List<TilesetTableEntry> mainTilesets = new List<TilesetTableEntry> ();

        bool mainTilesetsEmpty => mainTilesets.Count == 0;

        [FoldoutGroup (OdinGroup.Name.Tilesets)]
        [HideIf (nameof(interiorTilesetsEmpty))]
        [PropertyOrder (1)]
        [TableList (AlwaysExpanded = true, IsReadOnly = true)]
        public List<TilesetTableEntry> interiorTilesets = new List<TilesetTableEntry> ();

        bool interiorTilesetsEmpty => interiorTilesets.Count == 0;

        [ShowInInspector]
        [FoldoutGroup (OdinGroup.Name.TilesetsMaterialOverrides, VisibleIf = nameof(hasTilesetSelected), Order = 2)]
        [PropertyOrder (-1)]
        [LabelText ("Selected tileset"), LabelWidth (100f)]
        public string tilesetMaterialOverride => bb.editingMode == EditingMode.Volume
            ? bb.volumeTilesetSelected.name
            : bb.spotTilesetSelected.name;

        [HorizontalGroup (OdinGroup.Name.TilesetsMaterialOverridesInput, Width = 80f)]
        [HideLabel]
        public int tilesetMaterialOverrideIndexFrom;

        [ShowInInspector]
        [HorizontalGroup (OdinGroup.Name.TilesetsMaterialOverridesInput, Width = 20f)]
        [DisplayAsString, HideLabel]
        public static string fromToArrow = " →";

        [HorizontalGroup (OdinGroup.Name.TilesetsMaterialOverridesInput, Width = 80f)]
        [HideLabel]
        public int tilesetMaterialOverrideIndexTo;

        [HorizontalGroup (OdinGroup.Name.TilesetsMaterialOverridesInput)]
        [PropertySpace (2)]
        [Button ("Change material overrides")]
        public void ChangeTilesetMaterialOverrides ()
        {
            if (tilesetMaterialOverrideIndexFrom != tilesetMaterialOverrideIndexTo)
            {
                return;
            }

            var am = bb.am;
            var tilesetID = bb.editingMode == EditingMode.Volume ? bb.volumeTilesetSelected.id : bb.spotTilesetSelected.id;
            var count = 0;
            foreach (var point in am.points)
            {
                if (point == null)
                {
                    continue;
                }
                if (!point.spotPresent)
                {
                    continue;
                }
                if (point.blockTileset != tilesetID)
                {
                    continue;
                }
                if (!point.customization.overrideIndex.RoughlyEqual (tilesetMaterialOverrideIndexFrom))
                {
                    continue;
                }

                point.customization.overrideIndex = tilesetMaterialOverrideIndexTo;
                count += 1;
            }

            Debug.LogFormat
            (
                "Adjusted material overrides from {0} to {1} on {2} spots",
                tilesetMaterialOverrideIndexFrom,
                tilesetMaterialOverrideIndexTo,
                count
            );
            am.RebuildEverything ();
        }

        [ShowInInspector]
        [FoldoutGroup (OdinGroup.Name.Props, false, VisibleIf = nameof(dataLoaded), Order = OdinGroup.Order.Props)]
        public readonly InspectorPropSelector props;

        [FoldoutGroup (OdinGroup.Name.PropGeneration, false, VisibleIf = nameof(dataLoaded), Order = OdinGroup.Order.PropGeneration)]
        [PropertyRange (0f, 1f)]
        [LabelText ("Density")]
        public float propGenChance;

        [FoldoutGroup (OdinGroup.Name.PropGeneration)]
        [PropertyRange (0f, 1f)]
        [LabelText ("Offset range")]
        public float propGenOffsetRange;

        [FoldoutGroup (OdinGroup.Name.PropGeneration)]
        [LabelText ("Require flat surroundings")]
        public bool propGenRequireFlatSurroundings;

        [HorizontalGroup (OdinGroup.Name.PropGenerationButtonsRow1)]
        [PropertySpace (2)]
        [Button ("Generate\nProps", ButtonHeight = 40)]
        public void GenerateProps ()
        {
            var am = bb.am;
            if (am == null || propGenerationIDs.Count == 0)
            {
                return;
            }

            var offsetUsed = propGenOffsetRange > 0f;

            foreach (var point in am.points)
            {
                if (!point.spotPresent)
                {
                    continue;
                }
                if (point.spotConfiguration != AreaNavUtility.configFloor)
                {
                    continue;
                }
                if (am.indexesOccupiedByProps.ContainsKey (point.spotIndex))
                {
                    continue;
                }
                if (point.blockTileset != AreaTilesetHelper.idOfForest && !AreaManager.IsPointTerrain (point))
                {
                    continue;
                }

                if (propGenRequireFlatSurroundings)
                {
                    var pointNeighbourXPos = point.pointsInSpot[1];
                    var pointNeighbourZPos = point.pointsInSpot[2];
                    var pointNeighbourXNeg = point.pointsWithSurroundingSpots[6];
                    var pointNeighbourZNeg = point.pointsWithSurroundingSpots[5];

                    var surroundedByFloor =
                        (pointNeighbourXPos == null || pointNeighbourXPos.spotPresent && pointNeighbourXPos.spotConfiguration == AreaNavUtility.configFloor)
                        && (pointNeighbourZPos == null || pointNeighbourZPos.spotPresent && pointNeighbourZPos.spotConfiguration == AreaNavUtility.configFloor)
                        && (pointNeighbourXNeg == null || pointNeighbourXNeg.spotPresent && pointNeighbourXNeg.spotConfiguration == AreaNavUtility.configFloor)
                        && (pointNeighbourZNeg == null || pointNeighbourZNeg.spotPresent && pointNeighbourZNeg.spotConfiguration == AreaNavUtility.configFloor);

                    if (!surroundedByFloor)
                    {
                        continue;
                    }
                }

                var propId = propGenerationIDs[Random.Range (0, propGenerationIDs.Count)];
                var prototype = AreaAssetHelper.GetPropPrototype (propId);
                if (prototype == null)
                {
                    continue;
                }

                var chance = Random.Range (0f, 1f);
                if (chance > propGenChance)
                {
                    continue;
                }

                var placement = new AreaPlacementProp
                {
                    id = prototype.id,
                    pivotIndex = point.spotIndex,
                    offsetX = 0f,
                    offsetZ = 0f,
                    rotation = bb.propEditInfo.Rotation,
                    flipped = bb.propEditInfo.Flipped,
                    hsbPrimary = Constants.defaultHSBOffset,
                    hsbSecondary = Constants.defaultHSBOffset
                };

                if (offsetUsed)
                {
                    placement.offsetX = Random.Range (-propGenOffsetRange, propGenOffsetRange);
                    placement.offsetZ = Random.Range (-propGenOffsetRange, propGenOffsetRange);
                }

                if (!am.indexesOccupiedByProps.ContainsKey (point.spotIndex))
                {
                    am.indexesOccupiedByProps.Add (point.spotIndex, new List<AreaPlacementProp> ());
                }

                am.indexesOccupiedByProps[point.spotIndex].Add (placement);
                am.placementsProps.Add (placement);
                am.ExecutePropPlacement (placement);
            }
        }

        [HorizontalGroup (OdinGroup.Name.PropGenerationButtonsRow1)]
        [PropertySpace (2)]
        [Button ("Remove\nall props", ButtonHeight = 40)]
        public void RemoveAllProps () => bb.am.RemoveAllProps ();

        [HorizontalGroup (OdinGroup.Name.PropGenerationButtonsRow1)]
        [PropertySpace (2)]
        [DisableIf (nameof(propGenerationIDsEmpty))]
        [Button ("Remove\nlisted props", ButtonHeight = 40)]
        public void RemoveListedProps () => bb.am.RemovePropsWithIds (new HashSet<int> (propGenerationIDs));

        [HorizontalGroup (OdinGroup.Name.PropGenerationButtonsRow2)]
        [PropertySpace (2)]
        [Button ("Load\ngrass props", ButtonHeight = 40)]
        public void LoadGrassProps () => LoadPropIDs (AreaAssetHelper.grassIDs);

        void LoadPropIDs (IEnumerable<int> propIDs)
        {
            propGenerationIDs.Clear ();
            propGenerationIDs.AddRange (propIDs);
        }

        [HorizontalGroup (OdinGroup.Name.PropGenerationButtonsRow2)]
        [PropertySpace (2)]
        [Button ("Load\nbush props", ButtonHeight = 40)]
        public void LoadBushProps () => LoadPropIDs (AreaAssetHelper.bushIDs);

        [HorizontalGroup (OdinGroup.Name.PropGenerationButtonsRow2)]
        [PropertySpace (2)]
        [Button ("Load\ntree props", ButtonHeight = 40)]
        public void LoadTreeProps () => LoadPropIDs (AreaAssetHelper.treeIDs);

        [HorizontalGroup (OdinGroup.Name.PropGenerationButtonsRow3)]
        [PropertySpace (2)]
        [Button ("Load\nprop file", ButtonHeight = 40)]
        public void LoadPropListFromFile ()
        {
            #if PB_MODSDK
            var dirPath = hasSelectedMod ? dirLevelData.FullName : DataPathHelper.GetApplicationFolder ();
            #else
            var dirPath = DataPathHelper.GetApplicationFolder ();
            #endif
            var filename = EditorUtility.OpenFilePanel ("Load Prop List", dirPath, "yaml");
            if (filename.Length == 0)
            {
                return;
            }

            var list = UtilitiesYAML.LoadDataFromFile<List<int>> (filename);
            if (list != null)
            {
                propGenerationIDs.Clear ();
                propGenerationIDs.AddRange (list);
            }
            else
            {
                Debug.LogErrorFormat ("Could not load '{0}'", filename);
            }
        }

        [HorizontalGroup (OdinGroup.Name.PropGenerationButtonsRow3)]
        [PropertySpace (2)]
        [DisableIf (nameof(propGenerationIDsEmpty))]
        [Button ("Save\nprop file", ButtonHeight = 40)]
        public void SavePropListToFile ()
        {
            #if PB_MODSDK
            var dirPath = hasSelectedMod ? dirLevelData.FullName : DataPathHelper.GetApplicationFolder ();
            #else
            var dirPath = DataPathHelper.GetApplicationFolder ();
            #endif
            var filename = EditorUtility.SaveFilePanel ("Save prop List", dirPath, "proplist", "yaml");
            if (filename.Length != 0)
            {
                UtilitiesYAML.SaveDataToFile (filename, propGenerationIDs, false);
            }
        }

        [FoldoutGroup (OdinGroup.Name.PropGeneration)]
        [PropertyOrder (1)]
        [PropertySpace (2)]
        [LabelText ("Prop IDs")]
        [ListDrawerSettings (ShowFoldout = false, DraggableItems = false)]
        public List<int> propGenerationIDs = new List<int> ();

        bool propGenerationIDsEmpty => propGenerationIDs.Count == 0;

        [FoldoutGroup (OdinGroup.Name.ColorPalette, false, VisibleIf = nameof(dataLoaded), Order = OdinGroup.Order.ColorPalette)]
        [HorizontalGroup (OdinGroup.Name.ColorPaletteButtons, Order = 0)]
        [Button ("Save")]
        // XXX tileset and prop color palette may be different; check which tool is in use?
        public void SaveColorPalette () => UtilitiesYAML.SaveDataToFile ("Configs/LevelEditor", "colorPalette.yaml", colorPalette);

        [HorizontalGroup (OdinGroup.Name.ColorPaletteButtons)]
        [Button ("Load")]
        public void LoadColorPalette ()
        {
            var list = UtilitiesYAML.LoadDataFromFile<List<PaletteEntry>> ("Configs/LevelEditor/colorPalette.yaml");
            if (list != null)
            {
                colorPalette.Clear ();
                foreach (var cpe in list)
                {
                    cpe.SetContext (UpdateColorInfo);
                    colorPalette.Add (cpe);
                }
            }
            else
            {
                Debug.LogWarning ("Could not load color palette");
            }
        }

        void UpdateColorInfo (PaletteEntry cpe)
        {
            if (bb.editingMode == EditingMode.Color)
            {
                bb.tilesetColor.SelectedTilesetId = cpe.tilesetId;
                bb.tilesetColor.SelectedPrimaryColor = cpe.primaryColor;
                bb.tilesetColor.SelectedSecondaryColor = cpe.secondaryColor;
                return;
            }

            if (bb.editingMode != EditingMode.Props)
            {
                return;
            }

            var am = bb.am;
            var propEditInfo = bb.propEditInfo;
            if (am.indexesOccupiedByProps.ContainsKey (propEditInfo.PlacementListIndex))
            {
                foreach (var placement in am.placementsProps)
                {
                    if (placement != propEditInfo.PlacementHandled)
                    {
                        continue;
                    }

                    placement.hsbPrimary.x = cpe.primaryColor.h;
                    placement.hsbPrimary.y = cpe.primaryColor.s;
                    placement.hsbPrimary.z = cpe.primaryColor.b;

                    placement.hsbSecondary.x = cpe.secondaryColor.h;
                    placement.hsbSecondary.y = cpe.secondaryColor.s;
                    placement.hsbSecondary.z = cpe.secondaryColor.b;

                    placement.UpdateMaterial_HSBOffsets (placement.hsbPrimary, placement.hsbSecondary);
                }
                return;
            }

            propEditInfo.HSBPrimary.x = cpe.primaryColor.h;
            propEditInfo.HSBPrimary.y = cpe.primaryColor.s;
            propEditInfo.HSBPrimary.z = cpe.primaryColor.b;

            propEditInfo.HSBSecondary.x = cpe.secondaryColor.h;
            propEditInfo.HSBSecondary.y = cpe.secondaryColor.s;
            propEditInfo.HSBSecondary.z = cpe.secondaryColor.b;
        }

        [FoldoutGroup (OdinGroup.Name.ColorPalette)]
        [HorizontalGroup (OdinGroup.Name.ColorPaletteNewSwatch, Width = 120f, Order = 1)]
        [ValueDropdown (nameof(tilesetsByID))]
        [HideLabel]
        public int selectedTilesetID = -1;

        bool invalidSelectedTilesetID => selectedTilesetID == -1;

        [HorizontalGroup (OdinGroup.Name.ColorPaletteNewSwatch)]
        [HideLabel]
        public string newSwatchDescription;

        [HorizontalGroup (OdinGroup.Name.ColorPaletteNewSwatch, Width = 45f)]
        [CustomValueDrawer (nameof(SwatchDrawer))]
        public HSBColor newSwatchPrimaryColor = HSBColor.FromColor (Color.gray);

        static void SwatchDrawer (HSBColor hsbc, GUIContent label) => PaletteEntry.SwatchDrawer (hsbc, label);

        [HorizontalGroup (OdinGroup.Name.ColorPaletteNewSwatch, Width = 45f)]
        [CustomValueDrawer (nameof(SwatchDrawer))]
        public HSBColor newSwatchSecondaryColor = HSBColor.FromColor (Color.gray);

        [HorizontalGroup (OdinGroup.Name.ColorPaletteNewSwatch, Width = 45f)]
        [PropertySpace (2)]
        [DisableIf (nameof(invalidSelectedTilesetID))]
        [Button ("Add"), PropertyTooltip ("Add Swatch")]
        public void AddNewSwatch () => colorPalette.Add (new PaletteEntry (UpdateColorInfo)
        {
            tilesetId = selectedTilesetID,
            tilesetDescription = newSwatchDescription,
            primaryColor = newSwatchPrimaryColor,
            secondaryColor = newSwatchSecondaryColor,
        });

        [HorizontalGroup (OdinGroup.Name.ColorPaletteNewSwatch, Width = 45f)]
        [PropertySpace (2)]
        [Button ("Copy"), PropertyTooltip ("Copy From Color Tool")]
        public void CopySwatchColorsFromTool ()
        {
            selectedTilesetID = bb.tilesetColor.SelectedTilesetId;
            newSwatchPrimaryColor = bb.tilesetColor.SelectedPrimaryColor;
            newSwatchSecondaryColor = bb.tilesetColor.SelectedSecondaryColor;
        }

        [FoldoutGroup (OdinGroup.Name.ColorPalette)]
        [PropertyOrder (1)]
        [LabelText ("Swatches")]
        [ListDrawerSettings (HideAddButton = true, ShowFoldout = false)]
        public List<PaletteEntry> colorPalette = new List<PaletteEntry> ();

        [FoldoutGroup (OdinGroup.Name.VolumeSnippets, false, VisibleIf = nameof(dataLoaded), Order = OdinGroup.Order.VolumeSnippets)]
        [Button ("Load")]
        public void LoadVolumeSnippets ()
        {
            // XXX this is broken; see comments in VolumeSnippetEntry.Load()
            var keys = UtilitiesYAML.GetDirectoryList (VolumeSnippetEntry.snippetsPath);

            volumeSnippets.Clear ();

            if (keys == null)
            {
                return;
            }

            foreach (var key in keys)
            {
                volumeSnippets.Add (new VolumeSnippetEntry ()
                {
                    am = bb.am,
                    key = key,
                });
            }
        }

        [FoldoutGroup (OdinGroup.Name.VolumeSnippets)]
        [PropertyOrder (1)]
        [LabelText ("Snippets")]
        [ListDrawerSettings (HideAddButton = true, HideRemoveButton = true, ShowFoldout = false, DraggableItems = false, ShowPaging = true)]
        public List<VolumeSnippetEntry> volumeSnippets = new List<VolumeSnippetEntry> ();

        [FoldoutGroup (OdinGroup.Name.Other, false, VisibleIf = nameof(dataLoaded), Order = OdinGroup.Order.Other)]
        [HorizontalGroup (OdinGroup.Name.OtherButtonsRow1)]
        [Button ("Regenerate\nnav graph", ButtonHeight = 40)]
        public void RegenerateNavGraph () => AreaNavUtility.GetNavigationNodes (bb.am);

        [HorizontalGroup (OdinGroup.Name.OtherButtonsRow1, Width = 95f)]
        [Button ("Regenerate\nnav overrides", ButtonHeight = 40)]
        public void RegenerateNavOverrides () => bb.am.RecheckNavOverrides ();

        [HorizontalGroup (OdinGroup.Name.OtherButtonsRow1, Width = 60f)]
        [Button ("Fill all\nvolume", ButtonHeight = 40)]
        public void FillAllVolumePoints ()
        {
            var am = bb.am;
            foreach (var point in am.points)
            {
                if (point.pointPositionIndex.y > 0)
                {
                    point.pointState = AreaVolumePointState.Full;
                }
            }
            am.RebuildEverything ();
        }

        [HorizontalGroup (OdinGroup.Name.OtherButtonsRow1, Width = 100f)]
        [Button ("Mark roads\nindestructible", ButtonHeight = 40)]
        public void MarkRoadsIndestructible ()
        {
            var am = bb.am;
            var pointsSwitched = 0;
            foreach (var point in am.points)
            {
                if (point.pointState == AreaVolumePointState.Empty)
                {
                    continue;
                }

                var pointAbove = point.pointsWithSurroundingSpots[3];
                if (pointAbove == null || pointAbove.pointState != AreaVolumePointState.Empty)
                {
                    continue;
                }
                if (AreaManager.IsPointIndestructible (point, true, true, true, true, true))
                {
                    continue;
                }

                var match = AreaManager.IsPointInvolvingTileset (point, AreaTilesetHelper.idOfRoad);
                if (!match)
                {
                    continue;
                }

                point.destructible = false;
                pointsSwitched += 1;
            }

            Debug.LogWarning ("Points now indestructibe: " + pointsSwitched);
            am.RebuildEverything ();
        }

        [HorizontalGroup (OdinGroup.Name.OtherButtonsRow1, Width = 75f)]
        [Button ("Fix rural\ntextures", ButtonHeight = 40)]
        public void FixRuralTextureOverrides () => bb.am.ResetTextureOverrides (20);

        [HorizontalGroup (OdinGroup.Name.OtherButtonsRow2)]
        [PropertySpace (2)]
        [Button ("Reset prop\nHSB offsets", ButtonHeight = 40)]
        public void ResetPropColorOffsets ()
        {
            var am = bb.am;
            foreach (var placement in am.placementsProps)
            {
                placement.hsbPrimary = Constants.defaultHSBOffset;
                placement.hsbSecondary = Constants.defaultHSBOffset;
            }
            am.ExecuteAllPropPlacements ();
        }

        [HorizontalGroup (OdinGroup.Name.OtherButtonsRow2)]
        [PropertySpace (2)]
        [Button ("Process\nisolated", ButtonHeight = 40)]
        public void ProcessIsolatedStructures () => bb.am.SimulateIsolatedStructures ();

        [HorizontalGroup (OdinGroup.Name.OtherButtonsRow2)]
        [PropertySpace (2)]
        [Button ("Replace\ncity grass", ButtonHeight = 40)]
        public void SwapRoadGrass ()
        {
            var am = bb.am;
            if (am == null)
            {
                return;
            }

            foreach (var point in am.points)
            {
                if (!point.spotPresent || point.spotConfiguration != AreaNavUtility.configFloor)
                {
                    continue;
                }
                if (point.blockTileset != AreaTilesetHelper.idOfRoad)
                {
                    continue;
                }
                if (point.blockGroup != 1 || point.blockSubtype != 0)
                {
                    continue;
                }

                point.blockTileset = AreaTilesetHelper.idOfForest;
                point.blockGroup = 0;
                point.blockSubtype = 0;
                am.RebuildBlock (point, false);
            }
        }

        [HorizontalGroup (OdinGroup.Name.OtherButtonsRow2)]
        [PropertySpace (2)]
        [Button ("Remove\ncity crossings", ButtonHeight = 40)]
        public void RemoveCityCrossings ()
        {
            var am = bb.am;
            for (var i = am.placementsProps.Count - 1; i >= 0; i -= 1)
            {
                var placement = am.placementsProps[i];
                if (placement.id == 6703 || placement.id == 6702)
                {
                    am.RemovePropPlacement (placement);
                }
            }
            am.RebuildEverything ();
        }

        [HorizontalGroup (OdinGroup.Name.OtherButtonsRow3)]
        [PropertySpace (2)]
        [Button ("Remove float.\nprops", ButtonHeight = 40)]
        public void RemoveFloatingProps () => bb.am.RemoveAllFloatingProps ();

        [HorizontalGroup ("Other/Buttons Row 3")]
        [PropertySpace (2)]
        [Button ("Remove all\nprops", ButtonHeight = 40)]
        public void RemoveAllProps2 () => bb.am.RemoveAllProps ();

        [HorizontalGroup (OdinGroup.Name.OtherButtonsRow3)]
        [PropertySpace (2)]
        [Button ("Remove all\nfoliage", ButtonHeight = 40)]
        public void RemoveAllFoliage () => bb.am.RemoveAllProps ("vegetation_");

        [HorizontalGroup (OdinGroup.Name.OtherButtonsRow3)]
        [PropertySpace (2)]
        [Button ("Remove all\nemission", ButtonHeight = 40)]
        public void RemoveEmissions () => bb.am.ResetTextureOverrides ();

        #if PB_MODSDK
        [FoldoutGroup (OdinGroup.Name.OtherAdvancedFeatures, false, VisibleIf = nameof(dataLoaded), Order = OdinGroup.SubGroupOrder.OtherAdvancedFeatures)]
        [PropertyTooltip ("Enable UI to change area bounds")]
        public bool enableBoundsChanges;

        bool showBoundsChanges => dataLoaded && enableBoundsChanges;
        #endif

        [FoldoutGroup (OdinGroup.Name.Ramps, false, VisibleIf = nameof(dataLoaded), Order = OdinGroup.Order.Ramps)]
        [HorizontalGroup (OdinGroup.Name.RampsButtons)]
        [Button ("Remove all", ButtonHeight = 40)]
        public void RemoveAllRamps () => bb.am.RemoveRampsEverywhere ();

        [HorizontalGroup (OdinGroup.Name.RampsButtons)]
        [Button ("Generate\neverywhere", ButtonHeight = 40)]
        public void GenerateRampsEverywhere () => GenerateRamps (AreaManager.SlopeProximityCheck.None);

        void GenerateRamps (AreaManager.SlopeProximityCheck proximityCheck)
        {
            if (!dirTextureMaps.Exists)
            {
                return;
            }
            var filePath = DataPathHelper.GetCombinedCleanPath (dirTextureMaps.FullName, AreaManager.standardHeightmapFileName);
            bb.am.SetRampsEverywhere (filePath, proximityCheck);
        }

        [HorizontalGroup (OdinGroup.Name.RampsButtons)]
        [Button ("Generate\nstraight", ButtonHeight = 40)]
        public void GenerateStraightRamps () => GenerateRamps (AreaManager.SlopeProximityCheck.LateralSingle);

        [HorizontalGroup (OdinGroup.Name.RampsButtons)]
        [Button ("Generate\nwide straight", ButtonHeight = 40)]
        public void GenerateWideRamps () => GenerateRamps (AreaManager.SlopeProximityCheck.LateralDouble);

        [ShowInInspector]
        [FoldoutGroup (OdinGroup.Name.Ramps)]
        [PropertyOrder (1)]
        [LabelText ("Import ramps from texture on generation"), ToggleLeft]
        public bool importRampsFromTexture
        {
            get => bb.am.rampImportOnGeneration;
            set => bb.am.rampImportOnGeneration = value;
        }

        [FoldoutGroup (OdinGroup.Name.Heightmap, false, VisibleIf = nameof(dataLoaded), Order = OdinGroup.Order.Heightmap)]
        #if PB_MODSDK
        [HorizontalGroup (OdinGroup.Name.HeightmapButtonsRow1, Order = OdinGroup.SubGroupOrder.HeightmapButtonsRow1)]
        [EnableIf (nameof(hasSelectedMod))]
        [PropertyTooltip ("Make a starter heightmap that drops terrain to lowest height while preserving the boundary curves.")]
        [Button ("Create starter\nheightmap", ButtonHeight = 40)]
        public void CreateStarterHeightmap ()
        {
            if (!dirTextureMaps.Exists)
            {
                dirTextureMaps.Create ();
            }
            var filePath = DataPathHelper.GetCombinedCleanPath (dirTextureMaps.FullName, starterHeightmapFileName);
            bb.am.CreateHeightmap (BasinHeightmap.CalculateValues, filePath);
            heightmapExported = true;
        }

        [HorizontalGroup (OdinGroup.Name.HeightmapButtonsRow1)]
        [PropertyTooltip ("Erase level and generate new terrain from starter heightmap")]
        [Button ("Import from\nstarter heightmap", ButtonHeight = 40)]
        public void ImportDepthFromStarter ()
        {
            if (!dirTextureMaps.Exists)
            {
                return;
            }
            var filePath = DataPathHelper.GetCombinedCleanPath (dirTextureMaps.FullName, starterHeightmapFileName);
            bb.am.ImportHeightFromTexture (filePath);
            bb.am.ImportRoadsFromTexture (filePath);
        }

        const string starterHeightmapFileName = "starter_heightmap.png";
        #endif

        [HorizontalGroup (OdinGroup.Name.HeightmapButtonsRow2, Order = OdinGroup.SubGroupOrder.HeightmapButtonsRow2)]
        [PropertySpace (2f)]
        [Button ("Export\ndepth/ramps/roads", ButtonHeight = 40)]
        public void ExportDepthRampRoadToTexture ()
        {
            if (!dirTextureMaps.Exists)
            {
                dirTextureMaps.Create ();
            }
            var filePath = DataPathHelper.GetCombinedCleanPath (dirTextureMaps.FullName, AreaManager.standardHeightmapFileName);
            bb.am.CreateHeightmap (bb.am.CalculateStandardHeightmapValues, filePath);
            heightmapExported = true;
        }

        [HorizontalGroup (OdinGroup.Name.HeightmapButtonsRow3, Order = OdinGroup.SubGroupOrder.HeightmapButtonsRow3)]
        [PropertySpace (2f)]
        [Button ("Import\ndepth", ButtonHeight = 40)]
        public void ImportDepthFromTexture () => ImportFeatureFromTexture (bb.am.ImportHeightFromTexture);

        void ImportFeatureFromTexture (System.Action<string> importFeature)
        {
            if (!dirTextureMaps.Exists)
            {
                return;
            }
            var filePath = DataPathHelper.GetCombinedCleanPath (dirTextureMaps.FullName, AreaManager.standardHeightmapFileName);
            importFeature (filePath);
        }

        [HorizontalGroup (OdinGroup.Name.HeightmapButtonsRow3)]
        [PropertySpace (2f)]
        [Button ("Import\nramps", ButtonHeight = 40)]
        public void ImportRampsFromTexture () => ImportFeatureFromTexture (bb.am.ImportRampsFromTexture);

        [HorizontalGroup (OdinGroup.Name.HeightmapButtonsRow3)]
        [PropertySpace (2f)]
        [Button ("Import\nroads", ButtonHeight = 40)]
        public void ImportRoadsFromTexture () => ImportFeatureFromTexture (bb.am.ImportRoadsFromTexture);

        [BoxGroup (OdinGroup.Name.HeightmapDepthColors, false, VisibleIf = nameof(showDepthColors), Order = OdinGroup.SubGroupOrder.HeightmapDepthColors)]
        [TableList (AlwaysExpanded = true, IsReadOnly = true)]
        [OnInspectorInit (nameof(InitializeDepthColors))]
        [LabelText ("Exported color to depth")]
        public List<AreaManager.DepthColorLink> depthColors;

        bool heightmapExported;
        void InitializeDepthColors () => depthColors = bb.am.heightfieldPalette;
        bool showDepthColors => heightmapExported && bb.am.heightfieldPalette.Count != 0;

        [FoldoutGroup (OdinGroup.Name.Textures, false, VisibleIf = nameof(dataLoaded), Order = OdinGroup.Order.Textures)]
        [HorizontalGroup (OdinGroup.Name.TexturesButtons, Order = OdinGroup.SubGroupOrder.TextureButtons)]
        [PropertySpace (2f)]
        [Button ("Import props")]
        public void ImportPropsFromTexture ()
        {
            if (!dirTextureMaps.Exists)
            {
                return;
            }
            var filePath = DataPathHelper.GetCombinedCleanPath (dirTextureMaps.FullName, AreaManager.standardPropMaskVegetationFileName);
            bb.am.ImportPropsFromTexture (filePath);
        }

        [ToggleGroup (OdinGroup.Name.TexturesOverrideImportedProps, "Override imported props", Order = OdinGroup.SubGroupOrder.TextureOverrides)]
        [ShowIf (nameof(hasBiomeProps))]
        [OnValueChanged (nameof(ChangePropImportOverride))]
        [OnInspectorInit (nameof(InitiliazeOverrideImportedProps))]
        public bool overrideImportedProps;

        void InitiliazeOverrideImportedProps () => overrideImportedProps = bb.am.propImportOverrides;

        void ChangePropImportOverride () => bb.am.propImportOverrides = overrideImportedProps;

        [ToggleGroup (OdinGroup.Name.TexturesOverrideImportedProps)]
        [ValueDropdown (nameof(biomePropKeys))]
        [OnValueChanged (nameof(OverridePropImportsRed))]
        [OnInspectorInit (nameof(InitializePropImportOverrideRed))]
        [LabelText ("R (tall)"), LabelWidth (80f)]
        public string propImportOverrideRed = "";

        void InitializePropImportOverrideRed () => propImportOverrideRed = bb.am.propImportOverrideRed;
        void OverridePropImportsRed () => bb.am.propImportOverrideRed = propImportOverrideRed;

        [ToggleGroup (OdinGroup.Name.TexturesOverrideImportedProps)]
        [ValueDropdown (nameof(biomePropKeys))]
        [OnValueChanged (nameof(OverridePropImportsYellow))]
        [OnInspectorInit (nameof(InitializePropImportOverrideYellow))]
        [LabelText ("Y (mid)"), LabelWidth (80f)]
        public string propImportOverrideYellow = "";

        void InitializePropImportOverrideYellow () => propImportOverrideYellow = bb.am.propImportOverrideYellow;
        void OverridePropImportsYellow () => bb.am.propImportOverrideRed = propImportOverrideRed;

        [ToggleGroup (OdinGroup.Name.TexturesOverrideImportedProps)]
        [ValueDropdown (nameof(biomePropKeys))]
        [OnValueChanged (nameof(OverridePropImportsGreen))]
        [OnInspectorInit (nameof(InitializePropImportOverrideGreen))]
        [LabelText ("G (low)"), LabelWidth (80f)]
        public string propImportOverrideGreen = "";

        void InitializePropImportOverrideGreen () => propImportOverrideGreen = bb.am.propImportOverrideGreen;
        void OverridePropImportsGreen () => bb.am.propImportOverrideRed = propImportOverrideRed;

        [FoldoutGroup (OdinGroup.Name.VertexColors, false, VisibleIf = nameof(dataLoaded), Order = OdinGroup.Order.VertexColors)]
        [Button ("Set default")]
        public void SetDefaultVertexColors ()
        {
            bb.am.vertexPropertiesSelected = TilesetVertexProperties.defaults;
            SetVertexColorPrimary ();
            SetVertexColorSecondary ();
            SetVertexColorIntensity ();
        }

        [ShowInInspector]
        [HorizontalGroup (OdinGroup.Name.VertexColorsColors)]
        [VerticalGroup (OdinGroup.Name.VertexColorsPrimary)]
        [DisplayAsString, HideLabel]
        public static string vertexColorPrimaryLabel = "Primary";

        [VerticalGroup (OdinGroup.Name.VertexColorsPrimary)]
        [OnInspectorInit (nameof(SetVertexColorPrimary))]
        [OnValueChanged (nameof(UpdateVertexColorPrimaryFields))]
        [HideLabel]
        public Color vertexColorPrimary;

        void SetVertexColorPrimary ()
        {
            huePrimary = bb.am.vertexPropertiesSelected.huePrimary;
            saturationPrimary = bb.am.vertexPropertiesSelected.saturationPrimary;
            brightnessPrimary = bb.am.vertexPropertiesSelected.brightnessPrimary;
            vertexColorPrimary = new HSBColor (huePrimary, saturationPrimary, brightnessPrimary).ToColor ();
        }

        void UpdateVertexColorPrimaryFields ()
        {
            var hsbc = new HSBColor (vertexColorPrimary);
            huePrimary = hsbc.h;
            saturationPrimary = hsbc.s;
            brightnessPrimary = hsbc.b;
            bb.vertexColorChanged = true;
        }

        [VerticalGroup (OdinGroup.Name.VertexColorsPrimary)]
        [PropertyRange (0f, 1f)]
        [OnValueChanged (nameof(ChangeVertexColorPrimary))]
        [LabelText ("H")]
        public float huePrimary;

        void ChangeVertexColorPrimary ()
        {
            vertexColorPrimary = new HSBColor (huePrimary, saturationPrimary, brightnessPrimary).ToColor ();
            bb.vertexColorChanged = true;
        }

        [VerticalGroup (OdinGroup.Name.VertexColorsPrimary)]
        [PropertyRange (0f, 1f)]
        [OnValueChanged (nameof(ChangeVertexColorPrimary))]
        [LabelText ("S")]
        public float saturationPrimary;

        [VerticalGroup (OdinGroup.Name.VertexColorsPrimary)]
        [PropertyRange (0f, 1f)]
        [OnValueChanged (nameof(ChangeVertexColorPrimary))]
        [LabelText ("B")]
        public float brightnessPrimary;

        [ShowInInspector]
        [VerticalGroup (OdinGroup.Name.VertexColorsSecondary)]
        [DisplayAsString, HideLabel]
        public static string vertexColorSecondaryLabel = "Secondary";

        [VerticalGroup (OdinGroup.Name.VertexColorsSecondary)]
        [OnInspectorInit (nameof(SetVertexColorSecondary))]
        [OnValueChanged (nameof(UpdateVertexColorSecondaryFields))]
        [HideLabel]
        public Color vertexColorSecondary;

        void SetVertexColorSecondary ()
        {
            hueSecondary = bb.am.vertexPropertiesSelected.hueSecondary;
            saturationSecondary = bb.am.vertexPropertiesSelected.saturationSecondary;
            brightnessSecondary = bb.am.vertexPropertiesSelected.brightnessSecondary;
            vertexColorSecondary = new HSBColor (hueSecondary, saturationSecondary, brightnessSecondary).ToColor ();
        }

        void UpdateVertexColorSecondaryFields ()
        {
            var hsbc = new HSBColor (vertexColorSecondary);
            hueSecondary = hsbc.h;
            saturationSecondary = hsbc.s;
            brightnessSecondary = hsbc.b;
            bb.vertexColorChanged = true;
        }

        [VerticalGroup (OdinGroup.Name.VertexColorsSecondary)]
        [PropertyRange (0f, 1f)]
        [OnValueChanged (nameof(ChangeVertexColorSecondary))]
        [LabelText ("H")]
        public float hueSecondary;

        void ChangeVertexColorSecondary ()
        {
            vertexColorSecondary = new HSBColor (hueSecondary, saturationSecondary, brightnessSecondary).ToColor ();
            bb.vertexColorChanged = true;
        }

        [VerticalGroup (OdinGroup.Name.VertexColorsSecondary)]
        [PropertyRange (0f, 1f)]
        [OnValueChanged (nameof(ChangeVertexColorSecondary))]
        [LabelText ("S")]
        public float saturationSecondary;

        [VerticalGroup (OdinGroup.Name.VertexColorsSecondary)]
        [PropertyRange (0f, 1f)]
        [OnValueChanged (nameof(ChangeVertexColorSecondary))]
        [LabelText ("B")]
        public float brightnessSecondary;

        [ShowInInspector]
        [VerticalGroup (OdinGroup.Name.VertexColorsIntensity)]
        [DisplayAsString, HideLabel]
        public static string vertexColorIntensityLabel = "Intensities";

        [VerticalGroup (OdinGroup.Name.VertexColorsIntensity)]
        [OnInspectorInit (nameof(SetVertexColorIntensity))]
        [ReadOnly]
        [HideLabel]
        public Color vertexColorIntensity;

        void SetVertexColorIntensity ()
        {
            emissionIntensity = bb.am.vertexPropertiesSelected.overrideIndex;
            damageIntensity = bb.am.vertexPropertiesSelected.damageIntensity;
            vertexColorIntensity = new Color (emissionIntensity, emissionIntensity, emissionIntensity, damageIntensity);
        }

        [VerticalGroup (OdinGroup.Name.VertexColorsIntensity)]
        [PropertyRange (0f, 1f)]
        [OnValueChanged (nameof(ChangeVertexColorIntensity))]
        [LabelText ("Emission")]
        public float emissionIntensity;

        void ChangeVertexColorIntensity ()
        {
            vertexColorIntensity = new Color (emissionIntensity, emissionIntensity, emissionIntensity, damageIntensity);
            bb.vertexColorChanged = true;
        }

        [VerticalGroup (OdinGroup.Name.VertexColorsIntensity)]
        [PropertyRange (0f, 1f)]
        [OnValueChanged (nameof(ChangeVertexColorIntensity))]
        [LabelText ("Damage")]
        public float damageIntensity;

        [ShowInInspector]
        [ShowIf (nameof(dataLoaded))]
        [PropertyOrder (OdinGroup.Order.ConfigurationData)]
        [HideLabel, HideReferenceObjectPicker]
        public ConfigurationData configurationData = new ConfigurationData ();

        [FoldoutGroup (OdinGroup.Name.Debug, false, VisibleIf = nameof(dataLoaded), Order = OdinGroup.Order.Debug)]
        [HorizontalGroup (OdinGroup.Name.DebugButtons, Order = OdinGroup.SubGroupOrder.DebugButtons)]
        [Button ("Validate flags", ButtonHeight = 40)]
        public void DebugValidateFlags ()
        {
            var counter = 0;
            foreach (var kvp in AreaTilesetHelper.database.tilesets)
            {
                foreach (var blockDefinition in kvp.Value.blocks)
                {
                    foreach (var kvpGroup in blockDefinition.subtypeGroups)
                    {
                        var group = kvpGroup.Value;
                        foreach (var kvpSubtype in group)
                        {
                            var subtype = kvpSubtype.Value;
                            if (subtype.hideFlags != HideFlags.None)
                            {
                                counter += 1;
                                Debug.Log ("Found a bad flag " + counter);
                                subtype.hideFlags = HideFlags.None;
                            }
                        }
                    }
                }
            }
        }

        [HorizontalGroup (OdinGroup.Name.DebugButtons)]
        [Button ("Fix brightness", ButtonHeight = 40)]
        public void DebugFixBrightness () => bb.am.FixBrightnessValues ();

        [HorizontalGroup (OdinGroup.Name.DebugButtons)]
        [Button ("Reapply defaults", ButtonHeight = 40)]
        public void DebugReapplyDefaults () => bb.am.SetAllBlocksToDefault ();

        [ShowInInspector]
        [FoldoutGroup (OdinGroup.Name.Debug)]
        [PropertyOrder (OdinGroup.SubGroupOrder.DebugToggles)]
        public AreaSceneDebugToggles debugToggles;

        [ShowInInspector]
        [FoldoutGroup (OdinGroup.Name.Debug)]
        [PropertyOrder (OdinGroup.SubGroupOrder.DebugInteractionDistance)]
        [PropertyRange (50f, 300f)]
        [PropertyTooltip ("Maximum distance to hit test for volumes")]
        public float volumeInteractionDistance
        {
            get => bb.volumeInteractionDistance;
            set => bb.volumeInteractionDistance = value;
        }

        [ShowInInspector]
        [FoldoutGroup (OdinGroup.Name.Debug)]
        [PropertyOrder (OdinGroup.SubGroupOrder.DebugNavigationDistance)]
        [PropertyRange (500f, 5000f)]
        [PropertyTooltip ("Maximum distance to display nav graph")]
        public float navigationInteractionDistance
        {
            get => bb.navigationInteractionDistance;
            set
            {
                bb.navigationInteractionDistance = value;
                bb.repaintScene = true;
            }
        }

        void Prepare()
        {
            var am = bb.am;
            var bounds = am.boundsFull;
            boundsDisplay = AreaSceneHelper.FormatBounds(bounds);
            boundsX = bounds.x;
            boundsY = bounds.y;
            boundsZ = bounds.z;

            stats = string.Format
            (
                "Points: {0} ({1} destructible)\nProps: {2}",
                am.points.Count,
                am.destructiblePointCount,
                am.placementsProps == null ? "null" : am.placementsProps.Count.ToString ()
            );

            if (mainTilesets.Count == 0)
            {
                PopulateTilesetLists ();
            }

            props.Populate ();
            heightmapExported = false;
            PopulateLayerStats ();

            bb.propEditInfo.Reset ();
            bb.lastPointHovered = default;
            bb.lastSpotType = default;
            bb.lastCellHovered = default;
            bb.onLevelLoaded?.Invoke ();
        }

        void PopulateLayerStats ()
        {
            var am = bb.am;

            layers.Clear ();
            layers.Capacity = boundsY;

            var bounds = bb.am.boundsFull;
            var boundsPosition = new Vector3Int ();
            for (var y = 0; y < bounds.y; y += 1)
            {
                boundsPosition.y = y;
                var layerStats = new LayerStats (y);
                for (var z = 0; z < bounds.z; z += 1)
                {
                    for (var x = 0; x < bounds.x; x += 1)
                    {
                        boundsPosition.x = x;
                        boundsPosition.z = z;
                        var index = AreaUtility.GetIndexFromVolumePosition (boundsPosition, bounds, skipBoundsCheck: true);
                        var point = am.points[index];
                        layerStats.Points += 1;
                        switch (point.pointState)
                        {
                            case AreaVolumePointState.Empty:
                                layerStats.EmptyPoints += 1;
                                break;
                            case AreaVolumePointState.Full:
                                layerStats.FullPoints += 1;
                                break;
                            case AreaVolumePointState.FullDestroyed:
                                layerStats.DestroyedPoints += 1;
                                break;
                        }
                        if (point.destructible)
                        {
                            layerStats.MarkedDestructible += 1;
                        }
                        if (point.pointState != AreaVolumePointState.Empty && !AreaManager.IsPointIndestructible (point, true, true, true, true, true))
                        {
                            layerStats.ComputedDestructible += 1;
                        }
                        if (point.instanceCollider != null)
                        {
                            layerStats.Colliders += 1;
                        }
                        if (point.spotConfiguration == TilesetUtility.configurationFull)
                        {
                            layerStats.FullConfigurations += 1;
                        }
                        if (point.spotConfiguration == TilesetUtility.configurationEmpty)
                        {
                            layerStats.EmptyConfigurations += 1;
                        }
                        if (point.road)
                        {
                            layerStats.MarkedRoads += 1;
                        }
                        if (point.spotPresent)
                        {
                            layerStats.Spots += 1;
                        }
                        if (point.blockTileset == 0)
                        {
                            layerStats.UntiledSpots += 1;
                        }
                        else if (point.blockTileset == AreaTilesetHelper.idOfRoad)
                        {
                            layerStats.RoadSpots += 1;
                        }
                        else if (AreaManager.IsPointTerrain(point))
                        {
                            layerStats.TerrainSpots += 1;
                        }
                        if (point.terrainOffset != 0f)
                        {
                            layerStats.ModifiedTerrain += 1;
                        }
                    }
                }
                layers.Add (layerStats);
            }
        }

        public void Initialize (AreaManager am, bool setupPerformed)
        {
            bb.am = am;

            if (!dataLoaded)
                return;

            if (setupPerformed)
                return;

            Prepare ();
        }

        public void OnVertexColorChange ()
        {
            bb.am.vertexPropertiesSelected = new TilesetVertexProperties
            (
                huePrimary,
                saturationPrimary,
                brightnessPrimary,
                hueSecondary,
                saturationSecondary,
                brightnessSecondary,
                emissionIntensity,
                damageIntensity
            );
            bb.vertexColorChanged = false;
        }

        public void OnEditingModeChanged ()
        {
            if ((lastEditingMode == EditingMode.Volume && bb.editingMode != EditingMode.Volume)
                || (lastEditingMode != EditingMode.Volume && bb.editingMode == EditingMode.Volume))
            {
                PopulateTilesetLists ();
            }
            lastEditingMode = bb.editingMode;
        }

        public bool dataLoaded
        {
            get
            {
                var dataSelected = DataMultiLinkerCombatArea.selectedArea;
                return dataSelected?.content?.core != null && dataSelected.content.channels != null && bb.am.points != null && bb.am.points.Count > 0;
            }
        }

        public AreaManagerSurrogate (AreaSceneBlackboard bb)
        {
            this.bb = bb;
            modeButtons = new ModeButtons (bb);
            debugToggles = new AreaSceneDebugToggles (bb);
            props = new InspectorPropSelector (bb);
            sliceControls = new SliceControls (bb);
            lastEditingMode = bb.editingMode;
        }

        readonly AreaSceneBlackboard bb;
        EditingMode lastEditingMode;

        bool editable => dataLoaded && hasSelectedMod;

        string combatAreaButtonName => dataLoaded ? "Edit Combat Area" : "Go to Combat Area DB";
        Color selectionColor => dataLoaded ? colorSelection : colorNormal;
        bool hasTilesetSelected => bb.spotTilesetSelected != null;

        #if PB_MODSDK
        DirectoryInfo dirLevelData => new DirectoryInfo (DataPathHelper.GetCombinedCleanPath (DataContainerModData.selectedMod.GetModPathProject (), "LevelData"));
        DirectoryInfo dirTextureMaps => new DirectoryInfo (DataPathHelper.GetCombinedCleanPath (dirLevelData.FullName, "TextureMaps", DataMultiLinkerCombatArea.selectedArea.key));
        #else
        DirectoryInfo dirTextureMaps => new DirectoryInfo (DataPathHelper.GetCombinedCleanPath (DataMultiLinkerCombatArea.selectedArea.path, DataMultiLinkerCombatArea.selectedArea.key));
        #endif

        static ValueDropdownList<int> tilesetsByID
        {
            get
            {
                var vdl = new ValueDropdownList<int> ()
                {
                    {"", -1},
                };
                var tsets = AreaTilesetHelper.database.tilesets.Values
                    .Select (t => new
                    {
                        Name = t.name,
                        TilesetID = t.id,
                    });
                foreach (var ts in tsets)
                {
                    vdl.Add (ts.Name, ts.TilesetID);
                }
                return vdl;
            }
        }

        static DataContainerCombatBiomes biomeData => DataLinkerCombatBiomes.data;
        static List<string> biomePropKeys => biomeData.propGroups.Keys.ToList ();
        static bool hasBiomeProps
        {
            get
            {
                var bd = biomeData;
                if (bd == null)
                {
                    return false;
                }
                var pg = bd.propGroups;
                if (pg == null)
                {
                    return false;
                }
                return pg.Count != 0;
            }
        }

        static readonly Color colorNormal = new Color (1f, 1f, 1f, 1f);
        static readonly Color colorSelection = new Color (0.7f, 0.9f, 1f, 1f);

        const string selectionNone = "—";
        const string areaHint = "This is a sub-editor allowing you to view and edit level content stored in the Area DB.";

        public static class OdinGroup
        {
            public static class Name
            {
                public const string BoundsAndStats = "Bounds & Stats";
                public const string BoundsAndStatsBoundsReadonly = BoundsAndStats + "/Bounds (ReadOnly)";
                public const string BoundsAndStatsBounds = BoundsAndStats + "/Bounds";
                public const string BoundsAndStatsBoundsButtons = BoundsAndStatsBounds + "/Buttons";
                public const string Cleanup = nameof(Cleanup);
                public const string CleanupButtons = Cleanup + "/Buttons";
                public const string ColorPalette = "Color Palette";
                public const string ColorPaletteButtons = ColorPalette + "/Buttons";
                public const string ColorPaletteNewSwatch = ColorPalette + "/NewSwatch";
                public const string Debug = nameof(Debug);
                public const string DebugButtons = Debug + "/Buttons";
                public const string EditingMode = "Editing Mode";
                public const string Heightmap = nameof(Heightmap);
                public const string HeightmapButtonsRow1 = Heightmap + "/Buttons Row 1";
                public const string HeightmapButtonsRow2 = Heightmap + "/Buttons Row 2";
                public const string HeightmapButtonsRow3 = Heightmap + "/Buttons Row 3";
                public const string HeightmapDepthColors = Heightmap + "/DepthColor";
                public const string LevelSelector = nameof(LevelSelector);
                public const string LevelSelectorButtons = LevelSelector + "/Buttons";
                public const string Other = nameof(Other);
                public const string OtherButtonsRow1 = Other + "/Buttons Row 1";
                public const string OtherButtonsRow2 = Other + "/Buttons Row 2";
                public const string OtherButtonsRow3 = Other + "/Buttons Row 3";
                public const string OtherAdvancedFeatures = Other + "/Advanced Features";
                public const string PropGeneration = "Prop Generation";
                public const string PropGenerationButtonsRow1 = PropGeneration + "/Buttons Row 1";
                public const string PropGenerationButtonsRow2 = PropGeneration + "/Buttons Row 2";
                public const string PropGenerationButtonsRow3 = PropGeneration + "/Buttons Row 3";
                public const string Props = nameof(Props);
                public const string Ramps = nameof(Ramps);
                public const string RampsButtons = Ramps + "/Buttons";
                public const string Textures = nameof(Textures);
                public const string TexturesButtons = Textures + "/Buttons";
                public const string TexturesOverrideImportedProps = Textures + "/overrideImportedProps";
                public const string Tilesets = nameof(Tilesets);
                public const string TilesetsMaterialOverrides = Tilesets + "/Material overrides";
                public const string TilesetsMaterialOverridesInput = TilesetsMaterialOverrides + "/Input";
                public const string Updates = nameof(Updates);
                public const string UpdatesButtons = Updates + "/Buttons";
                public const string UpdatesButtonsLeft = UpdatesButtons + "/Left";
                public const string UpdatesButtonsRight = UpdatesButtons + "/Right";
                public const string VertexColors = "Vertex Colors";
                public const string VertexColorsColors = VertexColors + "/Colors";
                public const string VertexColorsPrimary = VertexColorsColors + "/Primary";
                public const string VertexColorsSecondary = VertexColorsColors + "/Colors";
                public const string VertexColorsIntensity = VertexColorsColors + "/Intensity";
                public const string View = nameof(View);
                public const string VolumeSnippets = "Volume Snippets";
            }

            public static class Order
            {
                public const float LevelSelector = 1f;
                public const float BoundsAndStats = 2f;
                public const float View = 2f;
                public const float Updates = 3f;
                public const float Cleanup = 4f;
                public const float EditingMode = 5f;
                public const float Tilesets = 6f;
                public const float Props = 7f;
                public const float PropGeneration = 8f;
                public const float ColorPalette = 9f;
                public const float VolumeSnippets = 10f;
                public const float Other = 11f;
                public const float Ramps = 12f;
                public const float Heightmap = 13f;
                public const float Textures = 14f;
                public const float VertexColors = 15f;
                public const float ConfigurationData = 16f;
                public const float Debug = 17f;
            }

            public static class SubGroupOrder
            {
                public const float DebugButtons = 0f;
                public const float DebugToggles = 1f;
                public const float DebugInteractionDistance = 2f;
                public const float DebugNavigationDistance = 3f;
                public const float EditingModeToggle = 0f;
                public const float EditingModeButtons = 1f;
                public const float HeightmapButtonsRow1 = 0f;
                public const float HeightmapButtonsRow2 = 1f;
                public const float HeightmapButtonsRow3 = 2f;
                public const float HeightmapDepthColors = 3f;
                public const float OtherAdvancedFeatures = 1f;
                public const float TextureButtons = 0f;
                public const float TextureOverrides = 1f;
            }
        }
    }
}
