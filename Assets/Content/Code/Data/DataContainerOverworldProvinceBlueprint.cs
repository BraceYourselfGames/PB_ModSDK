using System;
using System.Collections.Generic;
using Entitas;
using Entitas.CodeGeneration.Attributes;
using Entitas.VisualDebugging.Unity;
using PhantomBrigade.Functions;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;
using YamlDotNet.Serialization;
using Random = UnityEngine.Random;

#if UNITY_EDITOR
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using UnityEditor;
#endif

namespace PhantomBrigade.Data
{
    public enum PointDistancePriority
    {
        None,
        Closest,
        Furthest
    }
    
    [Overworld]
    public sealed class DataKeyOverworldProvince : IComponent
    {
        [EntityIndex]
        public string s;
    }
    
    [Overworld][DontDrawComponent]
    public sealed class DataLinkOverworldProvince : IComponent
    {
        public DataContainerOverworldProvinceBlueprint data;
    }

    [Serializable, HideReferenceObjectPicker]
    public class DataBlockOverworldProvincePoint
    {
        [HideLabel]
        public Vector3 position;
    }

    public class DataBlockProvinceScenarioChange
    {
        [ToggleLeft]
        public bool enabled = true;
        
        [DropdownReference, HideLabel] 
        public DataBlockComment comment;
        
        [DropdownReference (true)]
        public string conditionScenarioKey;
        
        [DropdownReference (false)]
        public SortedDictionary<string, bool> conditionScenarioTags;
        
        [DropdownReference (false)]
        public List<IOverworldEntityValidationFunction> conditionChecksBase;
        
        [DropdownReference (false)]
        public List<IOverworldEntityValidationFunction> conditionChecksSite;

        [DropdownReference (true)]
        [ValueDropdown ("@DataMultiLinkerScenario.data.Keys")]
        public string parentKey;

        [DropdownReference]
        public List<ICombatFunction> functionsOnStart;

        [DropdownReference]
        public List<DataBlockScenarioUnitsInjected> units;
        
        [DropdownReference (true)]
        public SortedDictionary<string, DataBlockOverworldPointRewardGroup> rewards;

        public bool IsChangeApplicable (DataContainerScenario scenario, PersistentEntity sitePersistent)
        {
            #if !PB_MODSDK
            if (!enabled)
                return false;

            if (sitePersistent == null || sitePersistent.isDestroyed)
                return false;

            if (scenario == null || string.IsNullOrEmpty (scenario.key))
                return false;
            
            if (!string.IsNullOrEmpty (conditionScenarioKey) && !string.Equals (scenario.key, conditionScenarioKey, StringComparison.Ordinal))
                return false;

            if (conditionScenarioTags != null)
            {
                bool match = true;
                foreach (var kvp in conditionScenarioTags)
                {
                    bool required = kvp.Value;
                    bool present = scenario.tagsProc != null && scenario.tagsProc.Contains (kvp.Key);

                    if (required != present)
                    {
                        match = false;
                        break;
                    }
                }

                if (!match)
                    return false;
            }

            if (conditionChecksBase != null)
            {
                var basePersistent = IDUtility.playerBasePersistent;
                foreach (var check in conditionChecksBase)
                {
                    if (check != null && !check.IsValid (basePersistent))
                        return false;
                }
            }
            
            if (conditionChecksSite != null)
            {
                foreach (var check in conditionChecksSite)
                {
                    if (check != null && !check.IsValid (sitePersistent))
                        return false;
                }
            }
            #endif
            
            return true;
        }
        
        #region Editor
	    #if UNITY_EDITOR
	    
	    [ShowInInspector, PropertyOrder (100)]
	    private DataEditor.DropdownReferenceHelper helper;

	    public DataBlockProvinceScenarioChange () => 
		    helper = new DataEditor.DropdownReferenceHelper (this);
	    
		#endif
	    #endregion
    }

    [Serializable][HideReferenceObjectPicker][LabelWidth (200f)]
    public class DataContainerOverworldProvinceBlueprint : DataContainerWithText, IDataContainerTagged
    {
        public bool hidden = false;
        
        public HashSet<string> tags = new HashSet<string> ();
        
        [LabelText ("Name / Desc.")]
        [YamlIgnore]
        public string textName;
        
        [TextArea][HideLabel]
        [YamlIgnore]
        public string textDesc;
        
        [InlineButton ("@LoadLandscape ()", "Apply")]
        [ValueDropdown ("@DataMultiLinkerOverworldLandscape.data.Keys")]
        public string landscapeKey;
        
        [ValueDropdown ("@DataMultiLinkerOverworldFactionBranch.data.Keys")]
        [DropdownReference (true)]
        public string factionBranchFixed;
        
        [DropdownReference (true)]
        public List<DataBlockProvinceScenarioChange> scenarioChanges;

        [OnInspectorGUI ("DrawMapPreview", false)]
        public Vector2 mapPosition;

        public bool IsHidden () => hidden;
        
        public HashSet<string> GetTags (bool processed) => 
            tags;
        
        public void LoadLandscape ()
        {
            var landscapeData = DataMultiLinkerOverworldLandscape.GetEntry (landscapeKey);
            if (landscapeData == null)
                return;

            DataMultiLinkerOverworldLandscape.selection = landscapeData;
            OverworldLandscapeManager.TryLoadingVisual 
            (
                landscapeData.assetKey, 
                true, 
                landscapeData.navSlopeLimit,
                landscapeData.propNormalDotRange
            );
        }
        
        public override void ResolveText ()
        {
            textName = DataManagerText.GetText (TextLibs.overworldProvinces, $"{key}_name");
            textDesc = DataManagerText.GetText (TextLibs.overworldProvinces, $"{key}_desc");
        }
        
        #if UNITY_EDITOR
        
        [ShowInInspector, PropertyOrder (100)]
        private DataEditor.DropdownReferenceHelper helper;

        public DataContainerOverworldProvinceBlueprint () => 
            helper = new DataEditor.DropdownReferenceHelper (this);
        
        public override void SaveText ()
        {
            if (!IsTextSavingPossible ())
                return;

            DataManagerText.TryAddingTextToLibrary (TextLibs.overworldProvinces, $"{key}_name", textName);
            DataManagerText.TryAddingTextToLibrary (TextLibs.overworldProvinces, $"{key}_desc", textDesc);
        }
        
        private Color GetElementColor (int index) => DataEditor.GetColorFromElementIndex (index);

        [ButtonGroup ("A"), Button ("Select"), PropertyOrder (-3), HideIf ("IsSelectedInInspector")]
        public void SelectToInspector ()
        {
            DataMultiLinkerOverworldProvinceBlueprints.selection = this;
        }
        
        [ButtonGroup ("A"), Button ("Deselect"), PropertyOrder (-3), ShowIf ("IsSelectedInInspector")]
        public void DeselectInInspector ()
        {
            DataMultiLinkerOverworldProvinceBlueprints.selection = null;
        }

        private bool IsSelectedInInspector ()
        {
            return DataMultiLinkerOverworldProvinceBlueprints.selection == this;
        }

        private static bool mapTexLoaded = false;
        private static Texture2D mapTex = null;
        private static bool mapTexFound = false;

        private void DrawMapPreview ()
        {
            var texKey = DataMultiLinkerOverworldProvinceBlueprints.Presentation.mapBackground;
            mapTex = TextureManager.GetTexture (TextureGroupKeys.OverworldMapBackgrounds, texKey);
            mapTexFound = mapTex != null;

            if (!mapTexFound)
                return;

            if (string.IsNullOrEmpty (landscapeKey))
            {
                GUILayout.Label ("No landscape, map hidden", EditorStyles.centeredGreyMiniLabel);
                return;
            }
            
            int texSize = mapTex.width;
            var drawSize = Mathf.Min (GUIHelper.ContextWidth - 88f - 15f, 800);
            var scaling = drawSize / texSize;

            using (var horizontalScope = new GUILayout.HorizontalScope ())
            {
                GUILayout.Space (15f);
                using ( var verticalScope = new GUILayout.VerticalScope ())
                {
                    GUILayout.Label (mapTex, GUILayout.Width (drawSize), GUILayout.Height (drawSize));
                    Rect rectLast = GUILayoutUtility.GetLastRect ();

                    var data = DataMultiLinkerOverworldProvinceBlueprints.GetDataList ();
                    if (data != null)
                    {
                        int i = 0;
                        foreach (var bp in data)
                        {
                            if (bp == null || bp.hidden || string.IsNullOrEmpty (bp.landscapeKey))
                                continue;

                            Vector2 mapPosNorm = new Vector2 (bp.mapPosition.x, -bp.mapPosition.y) / texSize;
                            DrawMapPoint (rectLast, mapPosNorm, i, drawSize, bp == this, bp.key);
                            i += 1;
                        }   
                    }
                }
            }
        }

        private void DrawMapPoint (Rect rectLast, Vector2 mapPositionNormalized, int index, float drawSize, bool selected, string name)
        {
            var rectPoint = new Rect (rectLast);
            rectPoint.width = 4f;
            rectPoint.height = 4f;
            
            bool center = mapPositionNormalized == Vector2.zero;
            if (center)
            {
                mapPositionNormalized += Random.insideUnitCircle * 0.01f;
                
                var rectText = new Rect (rectPoint);
                rectText.width = 200f;
                rectText.height = 16f;
                EditorGUI.DropShadowLabel (rectPoint, name);
            }

            rectPoint.x += drawSize * mapPositionNormalized.x + drawSize * 0.5f;
            rectPoint.y += drawSize * mapPositionNormalized.y + drawSize * 0.5f;
            
            if (selected)
            {
                float iconSize = 24f;
                Rect rectIcon = rectPoint.AlignCenter (iconSize, iconSize);
                SdfIcons.DrawIcon (rectIcon, SdfIconType.PlusCircleDotted);
                EditorGUI.DrawRect (rectPoint, Color.white.WithAlpha (0.2f));
            }
            else
            {
                var col = Color.HSVToRGB ((float)index * 0.09f, 0.5f, 1f).WithAlpha (0.7f);
                EditorGUI.DrawRect (rectPoint, col);
            }
        }

        #endif
    }
}
