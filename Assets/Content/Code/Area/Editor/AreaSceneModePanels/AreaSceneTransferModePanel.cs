using System.Collections.Generic;
using System.Linq;

using Sirenix.OdinInspector;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;

using UnityEditor;
using UnityEngine;

#if PB_MODSDK
using PhantomBrigade.SDK.ModTools;
#endif

namespace Area
{
    using Scene;

    sealed class AreaSceneTransferModePanel : AreaSceneModePanel
    {
        public static AreaSceneModePanel Create (AreaSceneBlackboard bb) => new AreaSceneTransferModePanel (bb);

        public GUILayoutOptions.GUILayoutOptionsInstance Width => GUILayoutOptions.Width (270f);
        public string Title => "Copy/paste mode";

        public void OnDisable ()
        {
            pointDisplay.OnDisable ();
            controls.OnDisable ();
            snippets.OnDisable ();
        }

        public void Draw ()
        {
            if (Event.current.OnLayout ())
            {
                pointCached = bb.lastPointHovered;
                cachedHover = bb.hoverActive;
            }

            GUILayout.Space (4f);
            pointDisplay.Draw (pointCached, cachedHover);

            GUILayout.Space (4f);
            GUILayout.BeginVertical ("Box");
            controls.Draw ();
            GUILayout.EndVertical ();

            #if PB_MODSDK
            if (!DataContainerModData.hasSelectedConfigs)
            {
                return;
            }
            #endif

            GUILayout.Space (4f);
            GUILayout.BeginVertical ("Box");
            snippets.Draw ();
            GUILayout.EndVertical ();

            // XXX not sure what to do with export. The combineExports field doesn't seem to be used
            // XXX in the AreaManager and I'm not sure what use modders have for the generated mesh.
            // var am = bb.am;
            // GUILayout.BeginVertical ("Box");
            // if (GUILayout.Button ("Export mesh"))
            // {
            //     am.ExportVolume (am.clipboardOrigin, am.clipboardOrigin + am.clipboardBoundsRequested + Vector3Int.size1x1x1Neg);
            // }
            // UtilityCustomInspector.DrawField ("Combine", () => am.combineExports = EditorGUILayout.Toggle (am.combineExports));
            // GUILayout.EndVertical ();
        }

        #if PB_MODSDK
        public AreaSceneModeHints Hints
        {
            get
            {
                for (var i = 0; i < hintColors.Length; i += 1)
                {
                    hintColors[i] = "";
                }
                if (AreaSceneBlackboard.clipboardSource == ClipboardSource.Snippet)
                {
                    hintColors[0] = gs;
                    hintColors[1] = ge;
                }
                var levelMatch = string.IsNullOrEmpty (AreaSceneBlackboard.clipboardAreaKey) || AreaSceneBlackboard.clipboardAreaKey == bb.am.areaName;
                if (!levelMatch)
                {
                    hintColors[0] = gs;
                    hintColors[1] = ge;
                }
                if (bb.targetOriginYCursorLock)
                {
                    hintColors[4] = gs;
                    hintColors[5] = ge;
                }
                #if PB_MODSDK
                if (!DataContainerModData.hasSelectedConfigs)
                {
                    hintColors[2] = gs;
                    hintColors[3] = ge;
                }
                else if (levelMatch
                    && !string.IsNullOrEmpty (AreaSceneBlackboard.clipboardModID)
                    && AreaSceneBlackboard.clipboardModID != DataContainerModData.selectedMod.id)
                {
                    hintColors[0] = gs;
                    hintColors[1] = ge;
                }
                #endif
                hints.HintText = string.Format (hintTextFormat, hintColors);
                return hints;
            }
        }
        #else
        public AreaSceneModeHints Hints => hints;
        #endif

        AreaSceneTransferModePanel (AreaSceneBlackboard bb)
        {
            this.bb = bb;
            controls = new TransferModePanelControls (bb);
            snippets = new TransferModePanelSnippets (bb);
        }

        readonly AreaSceneBlackboard bb;
        readonly TransferModePanelPointDisplay pointDisplay = new TransferModePanelPointDisplay ();
        readonly TransferModePanelControls controls;
        readonly TransferModePanelSnippets snippets;

        AreaVolumePoint pointCached;
        bool cachedHover;

        const string gs = "<color=#66666699>";
        const string ge = "</color>";
        const string hintTextFormat = "{0}[LMB] - Set source origin     [MMB] - Set source bounds     [MW▲▼] - Adjust source origin height{1}"
            + "\n[RMB] - Set target origin     {4}[Shift + MMB] - Ground target volume     [Shift + MW▲▼] - Adjust target origin height{5}"
            + "\n{0}[S] - Shrinkwrap source{1}     [Shift] - Drag target volume     [Shift + Z] - Lock/unlock target Y"
            + "\n{2}[Shift + LMB] - Paste (overwrite)     [Shift + RMB] - Paste (additive){3}"
            + "\n{0}[X] - Copy{1}     {2}[V] - Paste (overwrite)     [B] - Paste (additive){3}"
            + "\n[Delete] - Clear clipboard";

        static readonly object[] hintColors = {"", "", "", "", "", ""};
        static readonly AreaSceneModeHints hints = new AreaSceneModeHints ();
    }


    sealed class TransferModePanelPointDisplay : SelfDrawnGUI
    {
        [HorizontalGroup ("h")]
        [GUIColor (nameof(color))]
        [MiniLabel]
        public string pointDisplay () => hasPoint && hover
            ? "Point: " + point.spotIndex
            : "Point: —";

        [HorizontalGroup ("h")]
        [GUIColor (nameof(color))]
        [MiniLabel]
        public string pointPosition () => hasPoint && hover
            ? string.Format ("Position: ({0}, {1}, {2})", point.pointPositionIndex.x, point.pointPositionIndex.z, point.pointPositionIndex.y)
            : "Position: —";

        public void Draw (AreaVolumePoint pointHovered, bool hoverActive)
        {
            point = pointHovered;
            hover = hoverActive;

            GUILayout.BeginVertical ("Box");
            base.Draw ();
            GUILayout.EndVertical ();
        }

        Color color => hasPoint && hover ? Color.white : Color.gray;
        bool hasPoint => point != null;

        AreaVolumePoint point;
        bool hover;
    }

    sealed class TransferModePanelControls : SelfDrawnGUI
    {
        [OnInspectorGUI]
        public void Update ()
        {
            targetOriginErrorField = BoundsCheck (targetOrigin, true);
            if (targetOriginErrorField != -1)
            {
                var bounds = bb.am.boundsFull + Vector3Int.size1x1x1Neg;
                targetOriginErrorMessage = string.Format (positionErrorMessageFormat, bounds.x, bounds.z, bounds.y);
            }
            if (AreaSceneBlackboard.clipboardSource == ClipboardSource.Scene)
            {
                sourceOriginErrorField = BoundsCheck (sourceOrigin, true);
                if (sourceOriginErrorField != -1)
                {
                    var bounds = bb.am.boundsFull + Vector3Int.size1x1x1Neg;
                    sourceOriginErrorMessage = string.Format (positionErrorMessageFormat, bounds.x, bounds.z, bounds.y);
                }
                sourceBoundsErrorField = BoundsCheck (clipboardBoundsRequested, false);
                if (sourceBoundsErrorField != -1)
                {
                    var bounds = bb.am.boundsFull;
                    sourceBoundsErrorMessage = string.Format (boundsErrorMessageFormat, bounds.x, bounds.z, bounds.y);
                }
                return;
            }
            sourceOriginErrorField = -1;
            sourceBoundsErrorField = -1;
            sourceOriginErrorMessage = "";
            sourceBoundsErrorMessage = "";
        }

        [ShowInInspector]
        [BoxTitleGroup (OdinGroup.Name.Source, "$" + nameof(sourceTitle), Order = OdinGroup.Order.Source)]
        [VerticalGroup (OdinGroup.Name.SourceScene, VisibleIf = nameof(isSourceScene))]
        [ShowIf (nameof(showLevelSource))]
        [PropertySpace (0f, 2f)]
        [HideLabel, DisplayAsString]
        #if PB_MODSDK
        public string clipboardLevelSource
        {
            get
            {
                if (!DataContainerModData.hasSelectedConfigs && AreaSceneBlackboard.clipboardModID == AreaSceneModePanelHelper.SDKModPlaceHolder)
                {
                    return AreaSceneBlackboard.clipboardAreaKey;
                }
                if (DataContainerModData.hasSelectedConfigs && AreaSceneBlackboard.clipboardModID == DataContainerModData.selectedMod.id)
                {
                    return AreaSceneBlackboard.clipboardAreaKey;
                }
                return AreaSceneBlackboard.clipboardModID + " / " + AreaSceneBlackboard.clipboardAreaKey;
            }
        }
        #else
        public string clipboardLevelSource => AreaSceneBlackboard.clipboardAreaKey;
        #endif

        [ShowInInspector]
        [VerticalGroup (OdinGroup.Name.SourceScene)]
        [InfoBox ("$" + nameof(sourceOriginErrorMessage), InfoMessageType.Warning, VisibleIf = nameof(showSourceOriginError))]
        [EnableIf (nameof(enableSourceSceneControls))]
        [BoundsCheck (nameof(sourceOriginErrorField))]
        [LabelText ("Origin"), LabelWidth (55f)]
        public Vector3Int sourceOrigin
        {
            get => bb.am.clipboardOrigin;
            set => bb.am.clipboardOrigin = value;
        }

        [ShowInInspector]
        [BoxTitleGroup (OdinGroup.Name.Source)]
        [HideIf (nameof(isSourceScene))]
        [HideLabel, DisplayAsString, EnableGUI]
        #if PB_MODSDK
        public string sourceSnippet
        {
            get
            {
                var ss = AreaSceneBlackboard.clipboardSnippetKey;
                if (DataContainerModData.hasSelectedConfigs && DataContainerModData.selectedMod.id == AreaSceneBlackboard.clipboardModID)
                {
                    return ss;
                }
                if (!DataContainerModData.hasSelectedConfigs && AreaSceneBlackboard.clipboardModID == AreaSceneModePanelHelper.SDKModPlaceHolder)
                {
                    return ss;
                }
                return AreaSceneBlackboard.clipboardModID + " / " + ss;
            }
        }
        #else
        public string sourceSnippet => AreaSceneBlackboard.keySnippetSource;
        #endif

        [ShowInInspector]
        [BoxTitleGroup (OdinGroup.Name.Source)]
        [EnableIf (nameof(enableSourceSceneControls))]
        [InfoBox ("$" + nameof(sourceBoundsErrorMessage), InfoMessageType.Warning, VisibleIf = nameof(showSourceBoundsError))]
        [BoundsCheck (nameof(sourceBoundsErrorField))]
        [LabelText ("Bounds"), LabelWidth (55f)]
        public Vector3Int clipboardBoundsRequested
        {
            get => bb.am.clipboardBoundsRequested;
            set => bb.am.clipboardBoundsRequested = value;
        }

        [ShowInInspector]
        [BoxTitleGroup (OdinGroup.Name.Source)]
        [ShowIf (nameof(isSourceScene))]
        [EnableIf (nameof(enableSourceSceneControls))]
        [ToggleLeft]
        public bool groundOriginOnClick
        {
            get => bb.groundSourceOriginOnClick;
            set => bb.groundSourceOriginOnClick = value;
        }

        [BoxTitleGroup (OdinGroup.Name.Source)]
        [ButtonGroup (OdinGroup.Name.SourceVolume)]
        [Button ("$" + nameof(clearUnloadButtonName))]
        public void ClearSource ()
        {
            if (isSourceScene)
            {
                AreaSceneModeHelper.ClearClipboard (bb);
                return;
            }
            bb.am.clipboard.Reset ();
            AreaSceneBlackboard.ClearPersistentClipboardInfo ();
            ClipboardCopyOperation.RestoreClipboardOriginAndBounds (bb.am);
        }

        [ButtonGroup (OdinGroup.Name.SourceVolume)]
        [EnableIf (nameof(enableSourceSceneControls))]
        [Button]
        public void Shrinkwrap () => ClipboardCopyOperation.ShrinkwrapSource (bb.am);

        [BoxTitleGroup (OdinGroup.Name.Source)]
        [EnableIf (nameof(enableCopyButton))]
        [PropertySpace (2f)]
        [GUIColor ("lightgreen")]
        [Button (ButtonSizes.Large)]
        public void Copy () => AreaSceneModeHelper.CopyClipboardScene (bb);

        [ShowInInspector]
        [BoxTitleGroup (OdinGroup.Name.Target, "Target", SpaceBefore = 4f, VisibleIf = nameof(hasCopy), Order = OdinGroup.Order.Target)]
        [BoxGroup (OdinGroup.Name.TargetOptions, false, Order = OdinGroup.Order.TargetOptions)]
        [ToggleLeft]
        public bool transferVolume
        {
            get => bb.am.transferVolume;
            set => bb.am.transferVolume = value;
        }

        [ShowInInspector]
        [BoxGroup (OdinGroup.Name.TargetOptions)]
        [EnableIf (nameof(transferVolume))]
        [PropertyTooltip ("If pasting a volume inside terrain, setting this option will punch a hole from the surface of the terrain down to the paste volume. This will punch out only spots with terrain tiles.")]
        [ToggleLeft]
        public bool punchDownTerrain
        {
            get => bb.punchDownTerrainOnPaste;
            set => bb.punchDownTerrainOnPaste = value;
        }

        [ShowInInspector]
        [BoxGroup (OdinGroup.Name.TargetOptions)]
        [ToggleLeft]
        public bool transferProps
        {
            get => bb.am.transferProps;
            set => bb.am.transferProps = value;
        }

        [ShowInInspector]
        [BoxGroup (OdinGroup.Name.TargetOptions)]
        [EnableIf (nameof(transferProps))]
        [PropertyTooltip ("Check if props are compatible with the spots they're being pasted onto. Props that aren't compatible won't be pasted. Unset this option only if you are willing to clean up some messy prop placements.")]
        [ToggleLeft]
        public bool checkPropCompatibility
        {
            get => bb.checkPropCompatibilityOnPaste;
            set => bb.checkPropCompatibilityOnPaste = value;
        }

        [BoxGroup (OdinGroup.Name.TargetInfo, false)]
        [MiniLabel]
        public string points () => "Points: " + bb.am.clipboard.clipboardPointsSaved.Count;

        [BoxGroup (OdinGroup.Name.TargetInfo, Order = OdinGroup.Order.TargetInfo)]
        [MiniLabel]
        public string targetBounds ()
        {
            var bounds = bb.am.clipboard.clipboardBoundsSaved;
            return string.Format ("Bounds: {0} x {1} x {2}", bounds.x, bounds.z, bounds.y);
        }

        [ShowInInspector]
        [VerticalGroup (OdinGroup.Name.TargetOrigin, Order = OdinGroup.Order.TargetOrigin)]
        [InfoBox ("$" + nameof(targetOriginErrorMessage), InfoMessageType.Warning, VisibleIf = nameof(showTargetOriginError))]
        [BoundsCheck (nameof(targetOriginErrorField))]
        [EnableFieldIf (Y = "@!" + nameof(lockYToCursor))]
        [LabelText ("Origin"), LabelWidth (50f)]
        public Vector3Int targetOrigin
        {
            get => bb.am.targetOrigin;
            set => bb.am.targetOrigin = value;
        }

        [ShowInInspector]
        [VerticalGroup (OdinGroup.Name.TargetOrigin)]
        [ToggleLeft]
        public bool lockYToCursor
        {
            get => bb.targetOriginYCursorLock;
            set => bb.targetOriginYCursorLock = value;
        }

        [VerticalGroup (OdinGroup.Name.TargetButtons, Order = OdinGroup.Order.TargetButtons)]
        [HorizontalGroup (OdinGroup.Name.TargetVolume, Order = OdinGroup.Order.TargetVolume)]
        [OnCommand (nameof(Rotate))]
        [LabelText ("Rotate"), LabelWidth (50f)]
        public Rotation rotateVolume;

        [HorizontalGroup (OdinGroup.Name.TargetVolume, 80f)]
        [DisableIf (nameof(lockYToCursor))]
        [PropertySpace (2f)]
        [Button]
        public void Ground () => ClipboardCopyOperation.ShrinkwrapTarget (bb.am);

        [VerticalGroup (OdinGroup.Name.TargetPaste, Order = OdinGroup.Order.TargetPaste)]
        #if PB_MODSDK
        [EnableIf (nameof(enablePasteButton))]
        #else
        [DisableIf (nameof(showTargetOriginError))]
        #endif
        [PropertySpace (0f, 2f)]
        [GUIColor ("lightblue")]
        [Title ("Paste", "", TitleAlignments.Centered, false)]
        [Button ("Overwrite", ButtonSizes.Large)]
        public void PasteOverwrite () => Paste (ClipboardPasteOperation.ApplicationMode.Overwrite);

        [ButtonGroup (OdinGroup.Name.TargetPasteAddSub)]
        #if PB_MODSDK
        [EnableIf (nameof(enablePasteButton))]
        #else
        [DisableIf (nameof(showTargetOriginError))]
        #endif
        [GUIColor ("lightblue")]
        [Button ("Additive", ButtonSizes.Medium)]
        public void PasteAdditive () => Paste (ClipboardPasteOperation.ApplicationMode.Additive);

        [ButtonGroup (OdinGroup.Name.TargetPasteAddSub)]
        #if PB_MODSDK
        [EnableIf (nameof(enablePasteButton))]
        #else
        [DisableIf (nameof(showTargetOriginError))]
        #endif
        [Button ("Subtractive")]
        public void PasteSubtractive () => Paste (ClipboardPasteOperation.ApplicationMode.Subtractive);

        void Paste (ClipboardPasteOperation.ApplicationMode mode)
        {
            ClipboardPasteOperation.applicationMode = mode;
            ClipboardPasteOperation.PasteVolume (bb.am, punchDownTerrain, checkPropCompatibility, bb.enableTransferLogging);
        }

        void Rotate ()
        {
            switch (rotateVolume)
            {
                case Rotation.Clockwise:
                    bb.am.clipboard.Rotate (true);
                    break;
                case Rotation.Anticlockwise:
                    bb.am.clipboard.Rotate (false);
                    break;
                case Rotation.Flip:
                    bb.am.clipboard.Rotate (false);
                    bb.am.clipboard.Rotate (false);
                    break;
            }
        }

        int BoundsCheck (Vector3Int v, bool position)
        {
            var bounds = bb.am.boundsFull;
            if (position)
            {
                bounds += Vector3Int.size1x1x1Neg;
            }
            if (v.x < 0 || v.x > bounds.x)
            {
                return 0;
            }
            if (v.z < 0 || v.z > bounds.z)
            {
                return 1;
            }
            if (v.y < 0 || v.y > bounds.y)
            {
                return 2;
            }
            return -1;
        }

        string sourceTitle => "Source" + (AreaSceneBlackboard.clipboardSource == ClipboardSource.Snippet ? " (snippet)" : "");

        bool hasCopy => bb.am.clipboard.IsValid;
        bool isSourceScene => AreaSceneBlackboard.clipboardSource == ClipboardSource.Scene;
        #if PB_MODSDK
        bool isSourceLevelSame => string.IsNullOrEmpty(AreaSceneBlackboard.clipboardAreaKey)
            || (AreaSceneBlackboard.clipboardModID == (DataContainerModData.hasSelectedConfigs ? DataContainerModData.selectedMod.id : AreaSceneModePanelHelper.SDKModPlaceHolder)
            && AreaSceneBlackboard.clipboardAreaKey == bb.am.areaName);
        #else
        bool isSourceLevelSame => string.IsNullOrEmpty(AreaSceneBlackboard.clipboardAreaKey) || AreaSceneBlackboard.clipboardAreaKey == bb.am.areaName;
        #endif
        bool showLevelSource => hasCopy && !isSourceLevelSame;
        bool enableSourceSceneControls => isSourceScene && isSourceLevelSame;
        bool enableCopyButton => !sourceError && enableSourceSceneControls;

        int sourceOriginErrorField = -1;
        string sourceOriginErrorMessage = "";
        bool showSourceOriginError => sourceOriginErrorField != -1;
        int sourceBoundsErrorField = -1;
        string sourceBoundsErrorMessage = "";
        bool showSourceBoundsError => sourceBoundsErrorField != -1;
        bool sourceError => showSourceOriginError || showSourceBoundsError;
        int targetOriginErrorField = -1;
        string targetOriginErrorMessage = "";
        bool showTargetOriginError => targetOriginErrorField != -1;

        string clearUnloadButtonName => isSourceScene ? "Clear" : "Unload";
        #if PB_MODSDK
        bool enablePasteButton => DataContainerModData.hasSelectedConfigs && targetOriginErrorField == -1;
        #endif

        public TransferModePanelControls (AreaSceneBlackboard bb)
        {
            this.bb = bb;
        }

        readonly AreaSceneBlackboard bb;

        const string positionErrorMessageFormat = "The coordinate entered is outside the level bounds. Coordinates must be between (0, 0, 0) and ({0}, {1}, {2})";
        const string boundsErrorMessageFormat = "The bounds entered are larger than the level bounds. Bounds must be between (0, 0, 0) and ({0}, {1}, {2})";

        public enum Rotation
        {
            Clockwise = 0,
            Anticlockwise,
            Flip,
        }

        static class OdinGroup
        {
            public static class Name
            {
                public const string Source = nameof(Source);
                public const string SourceScene = Source + "/Scene";
                public const string SourceVolume = Source + "/Volume";
                public const string Target = nameof(Target);
                public const string TargetButtons = Target + "/Buttons";
                public const string TargetInfo = Target + "/Info";
                public const string TargetOptions = Target + "/Options";
                public const string TargetOrigin = Target + "/Origin";
                public const string TargetPaste = TargetButtons + "/Paste";
                public const string TargetPasteAddSub = TargetPaste + "/AddSub";
                public const string TargetVolume = TargetButtons + "/Volume";
            }

            public static class Order
            {
                public const float Source = 0f;
                public const float Target = 1f;

                public const float TargetOptions = 0f;
                public const float TargetInfo = 1f;
                public const float TargetOrigin = 2f;
                public const float TargetButtons = 3f;

                public const float TargetVolume = 0f;
                public const float TargetPaste = 1f;
            }
        }
    }

    sealed class TransferModePanelSnippets : SelfDrawnGUI
    {
        #if PB_MODSDK
        [OnInspectorGUI]
        public void Update ()
        {
            if (!DataContainerModData.hasSelectedConfigs)
            {
                if (!string.IsNullOrEmpty (lastModID))
                {
                    snippets.Clear ();
                    snippetKeys.Clear ();
                    contentLoad.Clear ();
                    Reset ();
                    lastModID = "";
                }
                return;
            }

            if (lastModID == DataContainerModData.selectedMod.id)
            {
                return;
            }

            LevelSnippetManager.LoadData (force: true);
            snippets.Clear();
            foreach (var kvp in LevelSnippetManager.data)
            {
                snippets[kvp.Key] = kvp.Value;
            }
            snippetKeys.Clear();
            snippetKeys.AddRange(snippets.Keys.ToList ());
            if (snippetKeys.Any ())
            {
                keySnippetLoad = snippetKeys.First ();
                ChooseSnippet ();
            }
            else
            {
                contentLoad.Clear ();
                Reset ();
            }
            lastModID = DataContainerModData.selectedMod.id;
        }

        static string lastModID;
        #endif

        [FoldoutGroup (OdinGroup.Name.Snippets, false)]
        [TabGroup (OdinGroup.Name.LoadSave, "Load")]
        #if PB_MODSDK
        [EnableIf (nameof(isMod))]
        #endif
        [OnValueChanged (nameof(ChooseSnippet))]
        [ValueDropdown (nameof(snippetKeys))]
        [HideLabel, SuffixLabel ("$" + nameof(snippetCount))]
        public string keySnippetLoad;

        [TabGroup (OdinGroup.Name.LoadSave, "Load")]
        [PropertySpace (2f)]
        [GUIColor ("$" + nameof(colorLoad))]
        [MiniLabel]
        public string size () => "Size: " + sizeDisplay;

        [TabGroup (OdinGroup.Name.LoadSave, "Load")]
        [GUIColor ("$" + nameof(colorLoad))]
        [MiniLabel]
        public string points () => "Points: " + (pointCount == 0 ? blank : pointCount.ToString ());

        #if PB_MODSDK
        [TabGroup (OdinGroup.Name.LoadSave, "Load")]
        [GUIColor ("$" + nameof(colorLoad))]
        [MiniLabel (VisibleIf = nameof(showOriginalLevel))]
        public string sourceLevelLoad () => "Original area: " + originalLevel;
        #else
        [TabGroup (OdinGroup.Name.LoadSave, "Load")]
        [GUIColor ("$" + nameof(colorLoad))]
        [MiniLabel (VisibleIf = nameof(showOriginalArea))]
        public string sourceLevelLoad () => "Original area: " + originalAreaLoad;
        #endif

        [TabGroup (OdinGroup.Name.LoadSave, "Load")]
        [PropertySpace (2f, 2f)]
        [MiniLabel (Multiline = true, VisibleIf = nameof(showDescriptionLoad))]
        public string descriptionLoad () => snippetLoad.description;

        [ShowInInspector]
        [TabGroup (OdinGroup.Name.LoadSave, "Load")]
        [ShowIf (nameof(isSnippetLoadValid))]
        [PropertyOrder (1f)]
        [PropertySpace (2f)]
        [ListDrawerSettings (DraggableItems = false, HideAddButton = true, HideRemoveButton = true, IsReadOnly = true, ShowFoldout = true)]
        [LabelText ("Content"), DisplayAsString]
        public readonly List<string> contentLoad = new List<string> ();

        [TabGroup (OdinGroup.Name.LoadSave, "Load")]
        [EnableIf (nameof(isSnippetLoadValid))]
        [PropertyOrder (2f)]
        [PropertySpace (2f)]
        [ConditionalInfoBox ("$" + nameof(loadMessage), MessageType = nameof(loadMessageType), VisibleIf = nameof(showLoadMessage))]
        [Button ("Load to clipboard")]
        public void Load ()
        {
            var ok = LevelSnippetHelper.LoadToClipboard (snippetLoad, bb.am);
            if (!ok)
            {
                hasLoadError = true;
                return;
            }
            hasLoadError = false;
            if (AreaSceneBlackboard.clipboardSource == ClipboardSource.Scene)
            {
                ClipboardCopyOperation.StashClipboardOriginAndBounds (bb.am);
            }
            bb.am.clipboardOrigin = Vector3Int.size0x0x0;
            bb.am.clipboardBoundsRequested = snippetLoad.size;
            AreaSceneBlackboard.clipboardSource = ClipboardSource.Snippet;
            AreaSceneBlackboard.clipboardSnippetKey = snippetLoad.key;
            #if PB_MODSDK
            AreaSceneBlackboard.clipboardModID = DataContainerModData.hasSelectedConfigs ? DataContainerModData.selectedMod.id : AreaSceneModePanelHelper.SDKModPlaceHolder;
            #endif
        }

        [TabGroup (OdinGroup.Name.LoadSave, "Save", TextColor = "@" + nameof(colorSaveTab))]
        [EnableIf (nameof(hasCopy))]
        [OnValueChanged (nameof(Validate))]
        [InfoBox ("$" + nameof(keyError), InfoMessageType.Error, VisibleIf = nameof(showKeyError))]
        [LabelText ("Key"), LabelWidth (35f)]
        public string keySnippetSave;

        [TabGroup (OdinGroup.Name.LoadSave, "Save")]
        [EnableIf (nameof(hasCopy))]
        [PropertySpace (2f)]
        [OnValueChanged (nameof(Validate))]
        [LabelText ("Overwrite"), LabelWidth (64f)]
        public bool overwriteExisting;

        [TabGroup (OdinGroup.Name.LoadSave, "Save")]
        [EnableIf (nameof(hasCopy))]
        [Title ("Description (optional)", HorizontalLine = false, Bold = false)]
        [HideLabel, TextArea (1, 3)]
        public string descriptionSave;

        [ShowInInspector]
        [TabGroup (OdinGroup.Name.LoadSave, "Save")]
        [EnableIf (nameof(hasCopy))]
        [Title ("Content", null, TitleAlignments.Centered, HorizontalLine = false, Bold = false)]
        [TableList (AlwaysExpanded = true, HideToolbar = true, IsReadOnly = true)]
        [HideLabel]
        public readonly List<ChunkEntry> contentSave;

        [TabGroup (OdinGroup.Name.LoadSave, "Save")]
        [EnableIf (nameof(enableSaveButton))]
        [PropertySpace (4f)]
        [InfoBox ("$" + nameof(saveError), InfoMessageType.Error, VisibleIf = nameof(showSaveError))]
        [Button ("Save from clipboard")]
        public void Save ()
        {
            var spec = new LevelSnippetManager.SaveSpec ()
            {
                Key = keySnippetSave,
                Description = descriptionSave,
                AreaKey = bb.am.areaName,
            };
            foreach (var entry in contentSave)
            {
                if (entry.save == ChunkEntry.SaveOption.No)
                {
                    continue;
                }
                var chunk = entry.create ();
                spec.Content.Add ((ILevelSnippetContent)chunk);
            }
            var (ok, snippet) = LevelSnippetHelper.SaveFromClipboard (bb.am.clipboard, spec);
            if (!ok)
            {
                saveError = "There was an error saving the snippet. Check the console for details.";
                return;
            }
            snippetKeys.Clear ();
            snippetKeys.AddRange (snippets.Keys);
            keySnippetLoad = snippet.key;
            keySnippetSave = "";
            descriptionSave = "";
            saveError = "";
        }

        void ChooseSnippet ()
        {
            contentLoad.Clear ();
            if (!snippets.TryGetValue (keySnippetLoad, out var snippet))
            {
                Reset ();
                return;
            }
            snippetLoad = snippet;
            var size = snippetLoad.size;
            sizeDisplay = string.Format ("{0} x {1} x {2}", snippetLoad.size.x, snippetLoad.size.z, snippetLoad.size.y);
            pointCount = size.x * size.z * size.y;
            var content = snippetLoad.content
                .Where (chunk => LevelExtensionManager.SnippetContentRegistry.ContainsKey (chunk.GetType ()))
                .Select (chunk => LevelExtensionManager.SnippetContentRegistry[chunk.GetType ()])
                .OrderBy (entry => entry.Priority)
                .ThenBy (entry => entry.DisplayText)
                .Select (entry => entry.DisplayText)
                .ToList ();
            contentLoad.AddRange (content);
        }

        void Reset ()
        {
            keySnippetLoad = nullKey;
            snippetLoad = null;
            sizeDisplay = blank;
            pointCount = 0;
        }

        bool ValidateKey (string key, out string errorDesc)
        {
            if (string.IsNullOrWhiteSpace (key))
            {
                errorDesc = "Snippet key should not be null, empty or whitespace";
                return false;
            }
            if (!UtilitiesYAML.IsDirectoryNameValid (key, out errorDesc))
            {
                if (errorDesc.StartsWith ("Input"))
                {
                    errorDesc = errorDesc.Replace ("Input", "Snippet key");
                }
                return false;
            }
            if (key.Any(char.IsWhiteSpace))
            {
                errorDesc = "Snippet key should not contain any whitespace characters";
                return false;
            }
            if (!overwriteExisting && snippets.ContainsKey (key))
            {
                errorDesc = "A snippet with that key already exists. Toggle overwrite existing if you want to replace the snippet.";
                return false;
            }
            errorDesc = "";
            return true;
        }

        void Validate () => isKeyValid = ValidateKey (keySnippetSave, out keyError);

        int snippetCount => snippetKeys.Count;
        #if PB_MODSDK
        bool isMod => DataContainerModData.hasSelectedConfigs;
        bool isSnippetLoadValid => isMod && snippetLoad != null;
        bool isSameMod => isSnippetLoadValid && DataContainerModData.selectedMod.id == snippetLoad.originalModID;
        bool showOriginalLevel => isSnippetLoadValid && !string.IsNullOrWhiteSpace (snippetLoad.originalAreaKey);
        string originalLevel => (isSameMod ? snippetLoad.originalModID + " / " : "") + snippetLoad.originalAreaKey;
        string loadMessage => hasLoadError
            ? "There was an error loading the snippet to the clipboard. Check the console for details."
            : isMod && snippetCount == 0
                ? "No snippets? You can create one by selecting a volume in the level editor and copying it to the clipboard. Then switch to the Save tab to make your first snippet!"
                : "";
        #else
        bool isSnippetLoadValid => snippetLoad != null;
        bool showOriginalArea => isSnippetLoadValid && !string.IsNullOrWhiteSpace (snippetLoad.originalAreaKey);
        string originalAreaLoad => showOriginalArea ? snippetLoad.originalAreaKey : blank;
        string loadMessage => hasLoadError
            ? "There was an error loading the snippet to the clipboard. Check the console for details."
            : snippetCount == 0
                ? "No snippets? You can create one by selecting a volume in the level editor and copying it to the clipboard. Then switch to the Save tab to make your first snippet!"
                : "";
        #endif
        bool showDescriptionLoad => isSnippetLoadValid && !string.IsNullOrWhiteSpace (snippetLoad.description);
        bool hasLoadError;
        bool showLoadMessage => !string.IsNullOrEmpty (loadMessage);
        MessageType loadMessageType => hasLoadError ? MessageType.Error : MessageType.Info;
        Color colorLoad => isSnippetLoadValid ? Color.white : Color.gray;

        bool hasCopy => bb.am.clipboard.IsValid;
        Color colorSaveTab => hasCopy ? Color.white : Color.gray;
        bool isKeyValid;
        string keyError;
        bool showKeyError => !string.IsNullOrEmpty (keyError);
        bool enableSaveButton => hasCopy && isKeyValid;
        string saveError;
        bool showSaveError => !string.IsNullOrEmpty (saveError);

        public TransferModePanelSnippets (AreaSceneBlackboard bb)
        {
            this.bb = bb;
            LevelSnippetManager.LoadData ();
            foreach (var kvp in LevelSnippetManager.data)
            {
                snippets[kvp.Key] = kvp.Value;
            }
            snippetKeys = snippets.Keys.ToList ();
            if (snippetKeys.Any ())
            {
                keySnippetLoad = snippetKeys.First ();
                ChooseSnippet ();
            }
            else
            {
                keySnippetLoad = nullKey;
            }
            contentSave = LevelExtensionManager.SnippetContentRegistry
                .Values
                .OrderBy (registered => registered.Priority)
                .ThenBy (registered => registered.DisplayText)
                .Select (registered => new ChunkEntry ()
                {
                    name = registered.DisplayText,
                    create = registered.Create,
                })
                .ToList ();
            #if PB_MODSDK
            lastModID ??= DataContainerModData.hasSelectedConfigs ? DataContainerModData.selectedMod.id : "";
            #endif
        }

        readonly AreaSceneBlackboard bb;
        readonly IDictionary<string, LevelSnippet> snippets = new Dictionary<string, LevelSnippet> ();
        readonly List<string> snippetKeys;
        LevelSnippet snippetLoad;
        string sizeDisplay = blank;
        int pointCount;

        const string nullKey = "—";
        const string blank = "—";

        static class OdinGroup
        {
            public static class Name
            {
                public const string LoadSave = Snippets + "/" + nameof(LoadSave);
                public const string Snippets = nameof(Snippets);
            }
        }
    }

    [System.Serializable]
    sealed class ChunkEntry
    {
        [EnumToggleButtons]
        public SaveOption save = SaveOption.Yes;

        [DisplayAsString, EnableGUI]
        public string name;

        [HideInInspector]
        public System.Func<ILevelExtension> create;

        public enum SaveOption
        {
            No = 0,
            Yes,
        }
    }
}
