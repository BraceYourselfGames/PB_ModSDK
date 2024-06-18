using System.Collections.Generic;

using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;

using UnityEditor;
using UnityEngine;

namespace Area
{
    using Scene;

    [CustomEditor (typeof (AreaManager))]
    public class AreaManagerInspector : OdinEditor
    {
        public override void OnInspectorGUI ()
        {
            var am = target as AreaManager;
            if (am == null)
            {
                am = CombatSceneHelper.ins.areaManager;
            }
            if (am == null)
            {
                EditorGUILayout.HelpBox ("No inspector instance", MessageType.Warning);
                return;
            }

            CheckSetup (am);
            if (!ValidateTilesets ())
            {
                return;
            }
            if (!ValidateProps ())
            {
                return;
            }

            base.OnInspectorGUI ();

            if (!surrogate.dataLoaded)
            {
                return;
            }

            foldoutDefault = EditorGUILayout.Foldout (foldoutDefault, "Other options");
            if (foldoutDefault)
            {
                defaultTree?.Draw ();
            }

            if (bb.vertexColorChanged)
            {
                surrogate.OnVertexColorChange ();
            }

            if (bb.repaintScene)
            {
                bb.repaintScene = false;
                var view = EditorWindow.GetWindow<SceneView>();
                view.Repaint();
            }
        }

        protected override void DrawTree () => surrogateTree.Draw(false);

        protected override void OnEnable ()
        {
            base.OnEnable ();
            EditorApplication.update -= UpdateInEditorApplication;
            EditorApplication.update += UpdateInEditorApplication;
            surrogateTree = new PropertyTree<AreaManagerSurrogate> (new[] { surrogate });
            bb.onEditingModeChanged += surrogate.OnEditingModeChanged;
            lastEditingMode = bb.editingMode;
        }

        protected override void OnDisable ()
        {
            base.OnDisable ();
            EditorApplication.update -= UpdateInEditorApplication;
            ClearSurrogate ();
            sceneModeToolbar.OnDisable ();
            foreach (var mode in modes.Values)
            {
                mode.OnDisable ();
            }
            setupPerformed = false;
        }

        void OnDestroy ()
        {
            EditorApplication.update -= UpdateInEditorApplication;
            foreach (var mode in modes.Values)
            {
                mode.OnDestroy ();
            }
            bb.gizmos.OnDestroy ();
            bb.spotInfo.OnDestroy ();
        }

        void OnSceneGUI ()
        {
            // Disable clicking on scene objects
            HandleUtility.AddDefaultControl (GUIUtility.GetControlID (FocusType.Passive));

            var am = target as AreaManager;
            if (am == null)
            {
                return;
            }

            var e = Event.current;
            if (e.type == EventType.Ignore)
            {
                Debug.Log ("Ignored event");
                return;
            }
            if (e.type == EventType.Used)
            {
                Debug.Log ("Used event");
                return;
            }

            CheckSetup (am);
            bb.am.UpdateShaderGlobals ();

            if (!surrogate.dataLoaded)
            {
                return;
            }

            ExamineEvent (e);
            bb.hoverActive = e.alt;
            if (bb.hoverActive)
            {
                BeginHover (e);
            }
            else
            {
                EndHover ();
                HandleSceneUserInput (e);
            }

            Draw (e);

            this.RepaintIfRequested ();
            if (GUI.changed)
            {
                var view = EditorWindow.GetWindow<SceneView>();
                view.Repaint();
            }
        }

        void CheckSetup (AreaManager am)
        {
            if (lastEditingMode != bb.editingMode)
            {
                lastEditingMode = bb.editingMode;
                activeMode = modes[(int)lastEditingMode];
                bb.repaintScene = true;
            }

            surrogate.Initialize (am, setupPerformed);
            defaultTree ??= new PropertyTree<AreaManager> (new[] { bb.am });

            if (setupPerformed)
            {
                return;
            }

            // AreaNavUtility.GetNavigationNodes (ref PhantomNavGraph.areaNodes, target as AreaManager);

            AreaTilesetHelper.CheckResources ();
            AreaAssetHelper.CheckResources ();

            if (AreaAssetHelper.propsPrototypesList.Count > 0)
            {
                bb.propEditInfo.SelectionID = AreaAssetHelper.propsPrototypesList[0].id;
            }

            AreaSceneModePanelHelper.InitializeStyles();
            bb.volumeTilesetSelected = AreaTilesetHelper.database.tilesetFallback;
            bb.spotTilesetSelected = AreaTilesetHelper.database.tilesetFallback;

            var defaultConfig = TilesetVertexProperties.defaults;
            bb.tilesetColor.SelectedPrimaryColor = new HSBColor(defaultConfig.huePrimary, defaultConfig.saturationPrimary, defaultConfig.brightnessPrimary);
            bb.tilesetColor.SelectedSecondaryColor = new HSBColor(defaultConfig.hueSecondary, defaultConfig.saturationSecondary, defaultConfig.brightnessSecondary);

            surrogate.LoadColorPalette();
            foreach (var create in modeFactories)
            {
                var mode = create (bb);
                modes[(int)mode.EditingMode] = mode;
            }
            activeMode = modes[(int)lastEditingMode];
            bb.gizmos.CheckSetup ();

            setupPerformed = true;
        }

        void ClearSurrogate ()
        {
            bb.onEditingModeChanged -= surrogate.OnEditingModeChanged;
            surrogateTree?.Dispose ();
            surrogateTree = null;
            defaultTree?.Dispose ();
            defaultTree = null;
        }

        bool ValidateTilesets ()
        {
            var tilesetsPresent = AreaTilesetHelper.AreAssetsPresent ();
            if (tilesetsPresent)
            {
                return true;
            }

            EditorGUIUtility.labelWidth = 180f;

            GUILayout.BeginVertical ("Box");
            EditorGUILayout.HelpBox (AssetPackageHelper.levelAssetTilesetsWarning, MessageType.Error);

            if (GUILayout.Button (AssetPackageHelper.levelAssetURLCaption))
            {
                Application.OpenURL (AssetPackageHelper.levelAssetURL);
            }

            GUILayout.EndVertical ();
            return false;
        }

        bool ValidateProps ()
        {
            var propsPresent = AreaAssetHelper.AreAssetsPresent ();
            if (propsPresent)
            {
                return true;
            }

            EditorGUIUtility.labelWidth = 180f;

            GUILayout.BeginVertical ("Box");
            EditorGUILayout.HelpBox (AssetPackageHelper.levelAssetPropsWarning, MessageType.Error);

            if (GUILayout.Button (AssetPackageHelper.levelAssetURLCaption))
            {
                Application.OpenURL (AssetPackageHelper.levelAssetURL);
            }

            GUILayout.EndVertical ();
            return false;
        }

        void UpdateInEditorApplication ()
        {
            bb.gizmos.Update ();
        }

        void ExamineEvent (Event e)
        {
            SuppressExecuteCommandEvents (e);
            if (e.type == EventType.KeyDown && e.isKey)
            {
                if (e.keyCode == KeyCode.Alpha2 || e.keyCode == KeyCode.F)
                {
                    e.Use (); // if you don't use the event, the default action will still take place.
                }
            }
            SwitchModeByKeypad (e);
        }

        void SuppressExecuteCommandEvents (Event e)
        {
            if (e.type != EventType.ExecuteCommand)
            {
                return;
            }

            // Killing some bad editor hotkeys
            switch (Event.current.commandName)
            {
                case "Copy":
                    e.Use ();
                    break;
                case "Cut":
                    e.Use ();
                    break;
                case "Paste":
                    e.Use ();
                    break;
                case "Delete":
                    e.Use ();
                    break;
                case "FrameSelected":
                    e.Use ();
                    break;
                case "Duplicate":
                    e.Use ();
                    break;
                case "SelectAll":
                    e.Use ();
                    break;
                // default:
                    // Lets show any other commands that may come through
                    // Debug.Log (Event.current.commandName);
                    // break;
            }
        }

        void SwitchModeByKeypad (Event e)
        {
            if (e.type != EventType.KeyDown)
            {
                return;
            }

            switch (e.keyCode)
            {
                case KeyCode.Keypad0:
                    bb.editingMode = EditingMode.Volume;
                    break;
                case KeyCode.Keypad1:
                    bb.editingMode = EditingMode.Tileset;
                    break;
                case KeyCode.Keypad2:
                    bb.editingMode = EditingMode.Spot;
                    break;
                case KeyCode.Keypad6:
                    bb.editingMode = EditingMode.Props;
                    break;
            }
        }

        void BeginHover (Event e)
        {
            var worldRay = HandleUtility.GUIPointToWorldRay (e.mousePosition);
            if (!Physics.Raycast (worldRay, out var hitInfo, bb.volumeInteractionDistance, activeMode.LayerMask))
            {
                EndHover ();
                return;
            }
            if (activeMode.Hover(e, hitInfo))
            {
                e.Use ();
            }
            lastHover = true;
        }

        void EndHover ()
        {
            if (!lastHover)
            {
                return;
            }
            activeMode.OnHoverEnd ();
            lastHover = false;
        }

        void HandleSceneUserInput (Event e)
        {
            if (activeMode.HandleSceneUserInput (e))
            {
                e.Use ();
            }
        }

        void Draw (Event e)
        {
            activeMode.DrawSceneMarkup (e, GUIHelper.RequestRepaint);
            bb.gizmos.DrawSceneMarkup ();
            sceneModeToolbar.Draw ();
            scenePanelDrawer.Draw (activeMode.Panel);
            DrawDebugOutlines ();
        }

        void DrawDebugOutlines ()
        {
            if (!bb.showSceneUIDebugOutlines)
            {
                return;
            }
            scenePanelDrawer.DrawUIOutline ();
            sceneModeToolbar.DrawUIOutline ();
        }

        public AreaManagerInspector ()
        {
            surrogate = new AreaManagerSurrogate (bb);
            scenePanelDrawer = new AreaScenePanelDrawer (bb);
            sceneModeToolbar = new AreaSceneModeToolbar (bb);
            AreaSceneGizmos.CreateInstance (bb);
            AreaSceneSpotInfo.CreateInstance (bb);
        }

        readonly AreaSceneBlackboard bb = new AreaSceneBlackboard ();
        readonly Dictionary<int, AreaSceneMode> modes = new Dictionary<int, AreaSceneMode> ();

        readonly AreaManagerSurrogate surrogate;
        PropertyTree surrogateTree;
        PropertyTree defaultTree;

        readonly AreaScenePanelDrawer scenePanelDrawer;
        readonly AreaSceneModeToolbar sceneModeToolbar;

        bool setupPerformed;
        bool foldoutDefault;
        EditingMode lastEditingMode;
        AreaSceneMode activeMode;
        bool lastHover;

        static readonly List<System.Func<AreaSceneBlackboard, AreaSceneMode>> modeFactories = new List<System.Func<AreaSceneBlackboard, AreaSceneMode>> ()
        {
            AreaSceneVolumeShapeMode.CreateInstance,
            AreaSceneDamageMode.CreateInstance,
            AreaSceneTransferMode.CreateInstance,
            AreaSceneTilesetMode.CreateInstance,
            AreaSceneSpotMode.CreateInstance,
            AreaSceneColorMode.CreateInstance,
            AreaSceneLayerMode.CreateInstance,
            AreaScenePropMode.CreateInstance,
            AreaSceneNavigationMode.CreateInstance,
            AreaSceneRoadMode.CreateInstance,
            AreaSceneRoadCurveMode.CreateInstance,
            AreaSceneTerrainRampMode.CreateInstance,
            AreaSceneTerrainShapeMode.CreateInstance,
        };
    }
}
