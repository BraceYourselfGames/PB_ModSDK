using System.Collections.Generic;

using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;

using UnityEditor;
using UnityEngine;

namespace Area
{
    using Scene;

    sealed class AreaScenePropModePanel : AreaSceneModePanel
    {
        public static AreaSceneModePanel Create (AreaSceneBlackboard bb) => new AreaScenePropModePanel (bb);

        public GUILayoutOptions.GUILayoutOptionsInstance Width => GUILayoutOptions.MinWidth (panelStartingWidth);
        public string Title => "Prop tool";

        public void OnDisable ()
        {
            propList.OnDisable ();
            panelOptions.OnDisable ();
        }

        public void Draw ()
        {
            DrawSpotInfo ();
            DrawEditOptions ();
            DrawPropList ();
            DrawNewPropSetup ();

            if (!bb.hoverActive
                && bb.propEditInfo.PlacementListIndex != -1
                && bb.am.indexesOccupiedByProps.ContainsKey (bb.propEditInfo.PlacementListIndex))
            {
                GUILayout.Space (5f);
                GUILayout.BeginVertical ("Box");
                GUILayout.Space (5f);
                GUILayout.Label ("Selected Props", EditorStyles.boldLabel);
                GUILayout.Space (3f);
                DrawSelectedProps ();
                GUILayout.EndVertical ();
            }
        }

        public AreaSceneModeHints Hints => bb.propEditingMode == PropEditingMode.Color ? colorHints : hints;

        void DrawSpotInfo ()
        {
            if (Event.current.type == EventType.Layout)
            {
                if (bb.hoverActive)
                {
                    spotCached = bb.lastSpotHovered;
                }
                else if (bb.propEditInfo.PlacementListIndex.IsValidIndex(bb.am.points))
                {
                    spotCached = bb.am.points[bb.propEditInfo.PlacementListIndex];
                }
                else
                {
                    spotCached = bb.lastSpotHovered;
                }
            }

            if (spotCached == null)
            {
                AreaSceneModePanelHelper.DrawSpotUnknown ();
                return;
            }

            GUILayout.BeginHorizontal ("Box");
            GUILayout.Label ("Spot: " + spotCached.spotIndex, EditorStyles.miniLabel, GUILayoutOptions.MinWidth(35f).MaxWidth(70f));
            var position = spotCached.pointPositionIndex;
            GUILayout.Label (string.Format ("Position: ({0}, {1}, {2})", position.x, position.z, position.y), EditorStyles.miniLabel);
            GUILayout.Space (35f);
            GUILayout.EndHorizontal ();
        }

        void DrawEditOptions ()
        {
            GUILayout.Space (4f);
            GUILayout.BeginVertical ("Box");
            panelOptions.Draw ();
            GUILayout.EndVertical ();
        }

        void DrawPropList ()
        {
            if (!bb.showPropListInPanel)
            {
                return;
            }

            GUILayout.Space (5f);
            GUILayout.BeginVertical ("Box");
            if (bb.hoverActive)
            {
                UtilityCustomInspector.DrawFoldout (propListSectionName, false, GUILayoutOptions.MinWidth (panelStartingWidth).ExpandWidth ());
                GUILayout.EndVertical ();
                return;
            }
            expandPropList = UtilityCustomInspector.DrawFoldout (propListSectionName, expandPropList, GUILayoutOptions.MinWidth (panelStartingWidth).ExpandWidth ());
            if (expandPropList)
            {
                propList.Draw ();
            }
            GUILayout.EndVertical ();
        }

        void DrawNewPropSetup ()
        {
            var propEditInfo = bb.propEditInfo;
            var selectedPrototype = AreaAssetHelper.GetPropPrototype (propEditInfo.SelectionID);
            GUILayout.Space (5f);
            GUILayout.BeginVertical ("Box");
            if (!bb.hoverActive && bb.propEditInfo.PlacementListIndex != -1)
            {
                expandNewPropSection = UtilityCustomInspector.DrawFoldout (newPropSectionName, expandNewPropSection, GUILayoutOptions.MinWidth (panelStartingWidth).ExpandWidth());
                if (!expandNewPropSection)
                {
                    GUILayout.EndVertical ();
                    return;
                }
            }
            else
            {
                GUILayout.Label (newPropSectionName, EditorStyles.boldLabel);
            }
            GUILayout.Space (3f);
            GUILayout.BeginHorizontal ();

            GUILayout.BeginVertical ("Box");
            var propName = selectedPrototype != null ? selectedPrototype.name : "null";
            GUILayout.Label (propEditInfo.SelectionID + " - " + propName, EditorStyles.boldLabel);

            DrawOrientationSection (npf, false);

            if (selectedPrototype != null && selectedPrototype.prefab.allowPositionOffset)
            {
                DrawPositionOffsetSection (npf);
            }
            if (selectedPrototype != null && selectedPrototype.prefab.allowTinting)
            {
                DrawColorSection (npf);
            }
            GUILayout.EndVertical ();

            GUILayout.BeginHorizontal ();
            GUILayout.Space (35f);
            GUILayout.EndHorizontal ();

            GUILayout.EndHorizontal ();
            GUILayout.EndVertical ();
        }

        void DrawOrientationSection (PropEditFunctions pf, bool showSnap)
        {
            GUILayout.BeginHorizontal ("Box");
            var (rotation, flipped) = pf.GetOrientation ();
            GUILayout.Label ("Orientation: " + rotation + (flipped ? "-" : "+"), EditorStyles.miniLabel);
            GUILayout.FlexibleSpace ();
            var snap = showSnap;
            if (showSnap)
            {
                snap = GUILayout.Button (new GUIContent ("S", "Snap rotation to tile"), GUILayout.Width (31f));
            }
            var rotateLeft = GUILayout.Button ("←", GUILayout.Width (31f));
            var flip = GUILayout.Button ("↔", GUILayout.Width (31f));
            var rotateRight = GUILayout.Button ("→", GUILayout.Width (31f));
            var reset = GUILayout.Button ("reset", EditorStyles.miniButton, GUILayout.MinWidth (45f));
            GUILayout.EndHorizontal ();

            pf.ChangeOrientation (snap, rotateLeft, rotateRight, flip, reset);
        }

        void DrawPositionOffsetSection (PropEditFunctions pf)
        {
            var lw = EditorGUIUtility.labelWidth;
            var fw = EditorGUIUtility.fieldWidth;
            EditorGUIUtility.labelWidth = 40f;
            EditorGUIUtility.fieldWidth = 30f;

            GUILayout.BeginHorizontal ("Box");
            GUILayout.BeginVertical ();
            GUILayout.Label ("Position offsets", EditorStyles.miniLabel);
            GUILayout.BeginHorizontal ();
            var offset = pf.GetOffset ();
            var offsetX = EditorGUILayout.FloatField ("X ↔", offset.x, GUILayout.MinWidth (80f));
            GUILayout.Space (5f);
            var offsetZ = EditorGUILayout.FloatField ("Z ↔", offset.z, GUILayout.MinWidth (80f));
            pf.ChangeOffset (offsetX, offsetZ);

            GUILayout.Space (30f);
            GUILayout.FlexibleSpace ();

            var cpr = DrawCopyPasteButtons (BeginHorizontal, GUILayout.EndHorizontal, GUILayout.MinWidth (45f));
            pf.CopyPastePosition (cpr);

            GUILayout.EndHorizontal ();
            GUILayout.EndVertical ();
            GUILayout.EndHorizontal ();

            EditorGUIUtility.labelWidth = lw;
            EditorGUIUtility.fieldWidth = fw;
        }

        PropCopyPasteReset DrawCopyPasteButtons (System.Action beginLayout, System.Action endLayout, GUILayoutOption layoutWidth)
        {
            beginLayout ();
            var cpr = PropCopyPasteReset.None;
            cpr = GUILayout.Button ("copy", EditorStyles.miniButton, layoutWidth) ? PropCopyPasteReset.Copy : cpr;
            cpr = GUILayout.Button ("paste", EditorStyles.miniButton, layoutWidth) ? PropCopyPasteReset.Paste : cpr;
            cpr = GUILayout.Button ("reset", EditorStyles.miniButton, layoutWidth) ? PropCopyPasteReset.Reset : cpr;
            endLayout ();
            return cpr;
        }

        void DrawColorSection (PropEditFunctions pf)
        {
            GUILayout.BeginHorizontal ("Box");
            GUILayout.BeginVertical ();
            GUILayout.Label ("HSV offsets", EditorStyles.miniLabel);
            DrawChangeColor (ref pf.PrimaryColor);
            DrawChangeColor (ref pf.SecondaryColor);
            GUILayout.EndVertical ();

            if (GUI.changed)
            {
                pf.OnColorGUIChanged ();
            }
            GUI.changed = false;

            GUILayout.Space (30f);
            GUILayout.FlexibleSpace ();
            var cpr = DrawCopyPasteButtons (BeginVertical, GUILayout.EndVertical, GUILayout.Width (50f));
            pf.CopyPasteColor (cpr);
            GUILayout.EndHorizontal ();
        }

        void DrawChangeColor (ref Vector4 hsb)
        {
            var lw = EditorGUIUtility.labelWidth;
            var fw = EditorGUIUtility.fieldWidth;
            EditorGUIUtility.labelWidth = 40f;
            EditorGUIUtility.fieldWidth = 30f;

            GUILayout.BeginHorizontal ();
            hsb.x = Mathf.Clamp01 (EditorGUILayout.FloatField ("H ↔", hsb.x));
            hsb.y = Mathf.Clamp01 (EditorGUILayout.FloatField ("S ↔", hsb.y));
            hsb.z = Mathf.Clamp01 (EditorGUILayout.FloatField ("V ↔", hsb.z));

            EditorGUI.BeginChangeCheck ();
            var colPrimary = EditorGUILayout.ColorField (new HSBColor (hsb.x, hsb.y, hsb.z).ToColor ());
            if (EditorGUI.EndChangeCheck ())
            {
                var colPrimaryHSB = new HSBColor (colPrimary);
                hsb.x = colPrimaryHSB.h;
                hsb.y = colPrimaryHSB.s;
                hsb.z = colPrimaryHSB.b;
            }
            GUILayout.EndHorizontal ();

            EditorGUIUtility.labelWidth = lw;
            EditorGUIUtility.fieldWidth = fw;
        }

        void DrawSelectedProps ()
        {
            var am = bb.am;
            var propEditInfo = bb.propEditInfo;
            var indexToRemove = -1;

            spf.am = am;
            spf.command = bb.propEditCommand;

            GUILayout.BeginVertical ();

            var placements = am.indexesOccupiedByProps[propEditInfo.PlacementListIndex];
            for (var i = 0; i < placements.Count; ++i)
            {
                var placement = placements[i];
                spf.placement = placement;

                var colorPrevious = GUI.backgroundColor;
                if (propEditInfo.PlacementHandled == placement)
                {
                    // Set blue background color for selected prop's UI elements
                    GUI.backgroundColor = Color.Lerp (GUI.backgroundColor, Color.cyan, 0.35f);
                    GUILayout.BeginVertical ("Box");
                }
                else
                {
                    GUILayout.BeginVertical ("Box");
                }

                var prototype = AreaAssetHelper.propsPrototypes[placement.id];
                // Prop ID and name
                var headerText = $"{placement.id} - {prototype.name}";
                if (prototype.prefab == null)
                {
                    headerText += " !NP";
                }

                GUILayout.Label (headerText, EditorStyles.boldLabel);
                GUILayout.Space (5f);
                GUILayout.BeginHorizontal ();

                GUILayout.BeginVertical ();

                DrawOrientationSection(spf, true);

                if (placement.state != null && placement.prototype != null && placement.prototype.prefab.allowPositionOffset)
                {
                    EditorGUI.BeginChangeCheck ();
                    DrawPositionOffsetSection (spf);
                    if (EditorGUI.EndChangeCheck ())
                    {
                        placement.UpdateOffsets (am);
                        UtilityECS.ScheduleUpdate ();
                    }

                    /*
                    if (GUI.changed)
                    {
                        Debug.LogWarning ("More than one update of entity positions might have unintended consequences");
                        placement.UpdateTransformations (am);
                    }
                    GUI.changed = false;
                    */
                }

                if (placement.prototype != null && placement.prototype.prefab.allowTinting)
                {
                    DrawColorSection(spf);
                }

                // Commented this out to remove list of subojbects from the UI and save on vertical space. Still useful for debug purposes.
                /*if (placement.prototype != null)
                {
                    for (int s = 0; s < placement.prototype.subObjects.Count; ++s)
                    {
                        var subObject = placement.prototype.subObjects[s];
                        var propRenderer = placement.prototype.prefab.renderers[subObject.contextIndex];
                        GUILayout.Label ($"{s}: {propRenderer?.renderer?.name}", EditorStyles.miniLabel);
                    }
                }*/
                GUILayout.EndVertical ();

                indexToRemove = DrawAddRemovePropButtons (i, placement, propEditInfo, indexToRemove);

                EditorGUILayout.EndHorizontal ();
                EditorGUILayout.EndVertical ();

                // Reset background color for UI elements that don't belong to selected prop
                GUI.backgroundColor = colorPrevious;

                GUILayout.Space (10f);
            }
            GUILayout.EndVertical ();

            if (indexToRemove != -1)
            {
                var placement = placements[indexToRemove];
                am.RemovePropPlacement (placement);
            }

            bb.propEditCommand = PropEditCommand.None;
        }

        int DrawAddRemovePropButtons (int index, AreaPlacementProp placement, PropEditInfo propEditInfo, int indexToRemove)
        {
            GUILayout.BeginVertical ();
            var isSelected = propEditInfo.PlacementHandled == placement;
            if (placement.prototype != null && placement.prototype.prefab.allowPositionOffset)
            {
                var text = isSelected ? "¤" : "┌ ┐\n└ ┘";
                if (GUILayout.Button (text, GUILayout.Width (35f), GUILayout.Height (35f)))
                {
                    if (isSelected)
                    {
                        propEditInfo.PlacementHandled = null;
                        propEditInfo.PlacementListIndex = -1;
                    }
                    else
                    {
                        propEditInfo.PlacementHandled = placement;
                    }
                }
            }
            if (GUILayout.Button ("×", GUILayout.Width (35f), GUILayout.Height (35f)))
            {
                indexToRemove = index;
            }
            else if (isSelected && bb.propEditCommand == PropEditCommand.DeleteSelected)
            {
                indexToRemove = index;
            }
            GUILayout.EndVertical ();

            return indexToRemove;
        }

        static void BeginHorizontal () => GUILayout.BeginHorizontal ();
        static void BeginVertical () => GUILayout.BeginVertical ();

        AreaScenePropModePanel (AreaSceneBlackboard bb)
        {
            this.bb = bb;
            npf = new NewPropFunctions (bb);
            spf = new SelectedPropFunctions (bb);
            propList = new PanelPropSelector (bb);
            panelOptions = new PropModePanelOptions (bb);
        }

        readonly AreaSceneBlackboard bb;
        readonly NewPropFunctions npf;
        readonly SelectedPropFunctions spf;
        readonly PanelPropSelector propList;
        readonly PropModePanelOptions panelOptions;
        bool expandPropList;
        bool expandNewPropSection;
        AreaVolumePoint spotCached;

        const string propListSectionName = "Props";
        const string newPropSectionName = "New Prop Spawn Setup";
        const float panelStartingWidth = 300f;

        static readonly AreaSceneModeHints hints = new AreaSceneModeHints ()
        {
            HintText = "[LMB] - Add prop     [Shift + RMB] - Copy prop     [MMB] - Remove prop     [MW▲▼] - Change preview prop"
                + "\n[Right Square Bracket] - Rotate preview prop     [Left Square Bracket] - Flip preview prop",
            NonAltHintText = "[Shift + F] - Rotate selected prop     [Shift + G] - Flip selected prop"
                + "\n[Shift + Z] - Copy selected prop HSV     [Shift + X] - Paste selected prop HSV"
                + "\n[Delete] - Remove selected prop",
        };
        static readonly AreaSceneModeHints colorHints = new AreaSceneModeHints ()
        {
            LeaderText = "<color=#ffea04bb>Applying prop color</color>\nUse the Mode dropdown in the Prop tool panel to switch back to placing props",
            HintText = "[LMB] - Apply picked color     [RMB] - Apply default color     [MMB] - Pick color",
        };
    }

    [HideLabel, HideReferenceObjectPicker]
    sealed class PanelPropSelector : SelfDrawnGUI
    {
        [ShowInInspector]
        [PropertyOrder (0f)]
        [LabelWidth (72f)]
        public string propFilter
        {
            get => bb.propFilter;
            set => bb.propFilter = value;
        }

        [ShowInInspector]
        [PropertyOrder (1f)]
        [TableList (AlwaysExpanded = true, IsReadOnly = true, NumberOfItemsPerPage = itemsPerPage, ShowPaging = true)]
        public readonly List<PropTableEntry> props;

        public override void OnDisable ()
        {
            bb.onPropListChanged -= Populate;
            base.OnDisable ();
        }

        void Populate ()
        {
            AreaSceneModeHelper.PopulatePropList (bb, altList: props);
            // Stuff dummies to avoid the table list stealing the focus from the
            // filter text field when it changes or hides its toolbar, scrollbar
            // or hides itself when there are no rows.
            while (props.Count < itemsPerPage + 1)
            {
                props.Add (new PropTableEntry ());
            }
        }

        public PanelPropSelector (AreaSceneBlackboard bb)
        {
            this.bb = bb;
            props = new List<PropTableEntry> ();
            props.AddRange (bb.props);
            bb.onPropListChanged += Populate;
        }

        readonly AreaSceneBlackboard bb;

        const int itemsPerPage = 10;
    }

    [HideLabel, HideReferenceObjectPicker]
    sealed class PropModePanelOptions : SelfDrawnGUI
    {
        [ShowInInspector]
        public PropEditingMode mode
        {
            get => bb.propEditingMode;
            set => bb.propEditingMode = value;
        }

        [ShowInInspector]
        [LabelText ("Check spot compatibility")]
        public bool checkCompatibility
        {
            get => bb.checkPropConfiguration;
            set => bb.checkPropConfiguration = value;
        }

        [ShowInInspector]
        public bool showOccludedHandles
        {
            get => bb.showOccludedPropHandles;
            set => bb.showOccludedPropHandles = value;
        }

        [ShowInInspector]
        public bool showPropListInPanel
        {
            get => bb.showPropListInPanel;
            set => bb.showPropListInPanel = value;
        }

        public PropModePanelOptions (AreaSceneBlackboard bb)
        {
            this.bb = bb;
        }

        readonly AreaSceneBlackboard bb;
    }
}
