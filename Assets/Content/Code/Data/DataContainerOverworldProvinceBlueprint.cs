using System;
using System.Collections.Generic;
using PhantomBrigade.Functions;
using Sirenix.OdinInspector;
using UnityEngine;
using YamlDotNet.Serialization;
using Random = UnityEngine.Random;

namespace PhantomBrigade.Data
{
    [Serializable, HideReferenceObjectPicker]
    public class DataBlockOverworldProvincePoint
    {
        [HideLabel]
        public Vector3 position;
    }

    [Serializable, HideReferenceObjectPicker][LabelWidth (200f)]
    public class DataBlockOverworldProvinceNeighbor
    {
        public bool allowActorSearch = true;
        public bool allowConvoys = true;
        public bool allowCounterAttacks = true;
        public bool allowWar = true;
    }

    [Serializable, HideReferenceObjectPicker][LabelWidth (200f)]
    public class DataBlockOverworldProvinceEntityGenerator
    {
        [ValueDropdown ("GetSpawnGroupKeys")]
        public string spawnGroup = "general";

        [ValueDropdown ("GetProfileKeys")]
        public string profile;

        [HorizontalGroup ("A")]
        [LabelText (@"Count (min/max) / Random")]
        public int countMin = 1;

        [HorizontalGroup ("A", 0.2f)]
        [ShowIf ("countRandom")]
        [HideLabel]
        public int countMax = 1;

        [HorizontalGroup ("A", 18f)]
        [HideLabel]
        public bool countRandom = false;

        #region Editor
        #if UNITY_EDITOR

        [YamlIgnore, HideInInspector]
        public DataContainerOverworldProvinceBlueprint parent;

        private IEnumerable<string> GetSpawnGroupKeys =>
            parent != null && parent.spawns != null ? parent.spawns.Keys : null;

        private IEnumerable<string> GetProfileKeys =>
            DataMultiLinkerOverworldSiteGenerationSettings.data.Keys;

        #endif
        #endregion
    }

    public static class UnitDifficultyTags
    {
        public static string[] tags =
        {
            "difficulty_easy",
            "difficulty_normal",
            "difficulty_hard"
        };

        public static string[] text =
        {
            "Easy",
            "Normal",
            "Hard"
        };

        public static string GetDifficultyTag (int index)
        {
            if (index.IsValidIndex (tags))
                return tags[index];
            return null;
        }

        public static string GetDifficultyText (int index)
        {
            if (index.IsValidIndex (text))
                return text[index];
            return null;
        }
    }

    public class DataBlockProvinceWarObjectiveDecisive
    {
        public bool tagsUsed = true;

        [ShowIf ("tagsUsed")]
        [DictionaryKeyDropdown ("@DataMultiLinkerOverworldEntityBlueprint.tags")]
        public Dictionary<string, bool> tags = new Dictionary<string, bool> ();

        [HideIf ("tagsUsed")]
        [ValueDropdown ("@DataMultiLinkerOverworldEntityBlueprint.data.Keys")]
        [GUIColor ("GetBlueprintColor")]
        public string blueprintKey;

        public string nameInternalSuffix;

        [YamlIgnore, ReadOnly]
        public string nameInternal;

        [YamlIgnore, ReadOnly]
        public string memoryKeyOnClear;

        public void OnAfterDeserialization (string key)
        {
            if (!string.IsNullOrEmpty (key) && !string.IsNullOrEmpty (nameInternalSuffix))
            {
                nameInternal = $"{key}_{nameInternalSuffix}";
                memoryKeyOnClear = $"province_auto_cleared_{nameInternalSuffix}";
            }
        }

        #if UNITY_EDITOR

        private Color GetBlueprintColor ()
        {
            var bp = DataMultiLinkerOverworldEntityBlueprint.GetEntry (blueprintKey, false);
            if (bp == null)
                return Color.HSVToRGB (0f, 0.5f, 1f);
            return Color.white;
        }

        #endif
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
        public List<IOverworldValidationFunction> conditionFunctions;

        [DropdownReference (true)]
        [ValueDropdown ("@DataMultiLinkerScenario.data.Keys")]
        public string parentKey;

        [DropdownReference]
        public List<ICombatFunction> functionsOnStart;

        [DropdownReference]
        public List<DataBlockScenarioUnits> units;

        public bool IsChangeApplicable (DataContainerScenario scenario)
        {
            #if !PB_MODSDK

            if (!enabled)
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

            if (conditionFunctions != null)
            {
                var basePersistent = IDUtility.playerBasePersistent;
                foreach (var function in conditionFunctions)
                {
                    if (function != null && !function.IsValid (basePersistent))
                        return false;
                }
            }

            return true;

            #else
            return false;
            #endif
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
    public class DataContainerOverworldProvinceBlueprint : DataContainerWithText
    {
        public bool used = false;

        [FoldoutGroup ("Core", false)]
        [LabelText ("Name / Desc.")]
        [YamlIgnore]
        public string textName;

        [FoldoutGroup ("Core")]
        [TextArea][HideLabel]
        [YamlIgnore]
        public string textDesc;

        [FoldoutGroup ("Core")]
        public int warVFXContainerIndex;

        [FoldoutGroup ("Core")]
        public bool alwaysVulnerable = false;

        [FoldoutGroup ("Core")]
        public int minLevelOffset;

        [FoldoutGroup ("Core")]
        public int maxLevelOffset;

        [FoldoutGroup ("Core")]
        public bool escalationDisabled = false;

        [FoldoutGroup ("Core")]
        public bool warDisabled = false;

        [FoldoutGroup ("Core")]
        public bool warAutomated = true;

        [FoldoutGroup ("Core")]
        public bool warDefeatScoreDecay = true;

        [FoldoutGroup ("Core")]
        [ValueDropdown ("@DataMultiLinkerOverworldFactionBranch.data.Keys")]
        public string factionBranch;

        [FoldoutGroup ("Core")]
        public Vector3 position;

        [PropertyOrder (11)]
        [ShowIf ("AreObjectivesDecisiveVisible")]
        public bool warObjectivesOld = false;

        [PropertyOrder (11)]
        [ShowIf ("AreObjectivesDecisiveVisible")]
        [PropertyRange (0, 5)]
        public int warObjectivesDecisiveRequired = 0;

        [PropertyOrder (11)]
        [ShowIf ("AreObjectivesDecisiveVisible")]
        [ListDrawerSettings (DefaultExpandedState = false, ShowPaging = false)]
        public List<DataBlockProvinceWarObjectiveDecisive> warObjectivesDecisive = new List<DataBlockProvinceWarObjectiveDecisive> ();

        [ShowIf ("AreNeighborsVisible")]
        [DictionaryKeyDropdown ("@DataMultiLinkerOverworldProvinceBlueprints.data.Keys")]
        [OnValueChanged ("CheckNeighbours", true)]
        public SortedDictionary<string, DataBlockOverworldProvinceNeighbor> neighbourData = new SortedDictionary<string, DataBlockOverworldProvinceNeighbor> ();

        [ShowIf ("AreNeighborsVisible")]
        [YamlIgnore, ReadOnly]
        [ListDrawerSettings (DefaultExpandedState = false, ShowPaging = false)]
        public Dictionary<string, int> neighbourAdjacencyForActors = new Dictionary<string, int> ();

        [ShowIf ("AreBordersVisible")]
        [ListDrawerSettings (DefaultExpandedState = false, ShowPaging = false)]
        public List<Vector3> border = new List<Vector3> ();

        [ShowIf ("AreSpawnsVisible")]
        [ListDrawerSettings (DefaultExpandedState = false, ShowPaging = false)]
        public Dictionary<string, List<DataBlockOverworldProvincePoint>> spawns;

        [YamlIgnore, HideInInspector]
        [ListDrawerSettings (DefaultExpandedState = false, ShowPaging = false)]
        public Dictionary<string, List<DataBlockOverworldProvincePoint>> spawnsShuffled;

        [ShowIf ("AreEntitiesVisible")]
        [ListDrawerSettings (DefaultExpandedState = false, ShowPaging = false)]
        public List<DataBlockOverworldProvinceEntityGenerator> entities = new List<DataBlockOverworldProvinceEntityGenerator> ();

        [DropdownReference]
        [ListDrawerSettings (DefaultExpandedState = false, ShowPaging = false)]
        [DictionaryKeyDropdown ("@DataMultiLinkerOverworldMemory.data.Keys")]
        public SortedDictionary<string, float> startingMemory;

        [DropdownReference (true)]
        public DataBlockCombatBiomeFilter biomeFilter;

        [DropdownReference (true)]
        public List<DataBlockProvinceScenarioChange> scenarioChanges;

        [DropdownReference (true)]
        public SortedDictionary<string, bool> tagFilterPatrol;

        [YamlIgnore, ReadOnly]
        public Bounds borderBounds;

        public struct BorderTriangleInfo
        {
            public Vector3 a;
            public Vector3 b;
            public Vector3 c;
        }

        [YamlIgnore, ReadOnly, HideInInspector]
        public List<BorderTriangleInfo> borderTriangles = new List<BorderTriangleInfo>();

        //Stores the normalized CDF of the triangles, so that we can look up a randomly weighted triangle faster
        [YamlIgnore, ReadOnly, HideInInspector]
        public List<float> borderTrianglesIndex = new List<float>();

        #if !PB_MODSDK

        public Vector3 GetRandomPointNavigable (int nTries, out bool success)
        {
            var pointLast = Vector3.zero;
            nTries = Mathf.Clamp (nTries, 1, 100);
            success = false;

            for (int i = 0; i < nTries; ++i)
            {
                pointLast = GetRandomPoint ();

                var nnInfo = AstarPath.active.GetNearest (pointLast, new NNConstraint { walkable = true, constrainWalkability = true });
                if (nnInfo.node == null)
                    continue;

                if ((nnInfo.position.Flatten () - pointLast.Flatten ()).sqrMagnitude > 10f * 10f)
                    continue;

                success = true;
                return nnInfo.position;
            }

            return pointLast;
        }

        #endif

        public Vector3 GetRandomPoint ()
        {
            if (borderTrianglesIndex.Count <= 0)
				return position;

			float tgtVal = UnityEngine.Random.value;

            //generate point in parallelogram and flip if we're not inside the triangle
            Vector3 GetRandomPointInTriangle(int index)
            {
				var tri = borderTriangles[index];

                var d1 = tri.b - tri.a;
                var d2 = tri.c - tri.a;

                var r1 = UnityEngine.Random.value;
                var r2 = UnityEngine.Random.value;

                if(r1 + r2 > 1f)
                {
                    r1 = 1f - r1;
                    r2 = 1f - r2;
                }

                return tri.a + r1 * d1 + r2 * d2;
            }

            for(int i = 0;i < borderTrianglesIndex.Count;++i)
            {
				if(borderTrianglesIndex[i] >= tgtVal)
				{
					return GetRandomPointInTriangle(i);
				}
            }

            return GetRandomPointInTriangle(borderTrianglesIndex.Count-1);
        }

        void ComputeBorderTriangles()
        {
            borderTriangles.Clear();
            borderTrianglesIndex.Clear();

            if(border.Count < 3)
				return;

            List<Vector3> flatBorder = new List<Vector3>();

			foreach (var v in border)
				flatBorder.Add(v.Flatten());

	        //make sure the polygon is clockwise
            float areaSum = 0;
            for(int i = 0;i < flatBorder.Count;++i)
            {
				Vector3 curV = flatBorder[i];
                Vector3 nextV = flatBorder[(i+1) % flatBorder.Count];

				areaSum += -(curV.x * nextV.z - curV.z*nextV.x);

            }

            areaSum *= 0.5f;

            if(areaSum < 0f)
            {
	            flatBorder.Reverse();
                areaSum *= -1;
            }

            float runningNormalizedAreaSum = 0f;

            float GetTriangleArea(Vector3 a, Vector3 b, Vector3 c)
            {
				var delta1 = b - a;
                var delta2 = c - a;
                float triAreaX2 = -(delta1.x * delta2.z - delta1.z * delta2.x);

                return triAreaX2 / 2f;
            }

			bool TryClipTriangle(int index1, int index2, int index3)
			{
				var a = flatBorder[index1];
                var b = flatBorder[index2];
                var c = flatBorder[index3];

                float triArea = GetTriangleArea(a,b,c);

                //Check that the vertex is convex
                if(triArea < 0f)
					return false;

                //Check if any other vertices are inside the triangle
				for(int i = 0;i < flatBorder.Count;++i)
	            {
					if(i == index1 || i == index2 || i == index3)
						continue;

                    //is p outside the triangle
                    var p = flatBorder[i];
                    if( GetTriangleArea(p, a, b) < 0f ||
						GetTriangleArea(p, b, c) < 0f ||
                        GetTriangleArea(p, c, a) < 0f)
                    {
                        continue;
                    }

                    return false;
	            }

                borderTriangles.Add(new BorderTriangleInfo
                {
					a = a,
                    b = b,
                    c = c
                });

                runningNormalizedAreaSum += triArea / areaSum;
                borderTrianglesIndex.Add(runningNormalizedAreaSum);

                return true;
			}

            //Ear clipping algorithm for triangulation
			while(flatBorder.Count >= 3)
            {
				int numVertices = flatBorder.Count;

                //Go around the polygon and try clipping off a triangle
	            for(int i = 0;i < flatBorder.Count;++i)
	            {
	                int pivotVertexIndex = (flatBorder.Count - 1 - i + flatBorder.Count) % flatBorder.Count;
	                int nextVertexIndex = (pivotVertexIndex + 1) % flatBorder.Count;
	                int prevVertexIndex = (pivotVertexIndex - 1 + flatBorder.Count) % flatBorder.Count;

                    if(!TryClipTriangle(prevVertexIndex, pivotVertexIndex, nextVertexIndex))
	                    continue;

                    flatBorder.RemoveAt(pivotVertexIndex);
                    break;
	            }

                //Infinite loop guard
                if(numVertices == flatBorder.Count)
                {
					Debug.LogError("Could not triangulate polygon");
	                break;
                }
            }

            //Fix any floating point errors by guaranteeing the last triangle to end at 1f
            if(borderTrianglesIndex.Count >= 1)
				borderTrianglesIndex[borderTrianglesIndex.Count-1] = 1f;
        }

        public override void OnAfterDeserialization (string key)
        {
            base.OnAfterDeserialization (key);

            if (warObjectivesDecisive != null)
            {
                foreach (var obj in warObjectivesDecisive)
                    obj.OnAfterDeserialization (key);
            }

            CheckNeighbours ();

            if (spawns == null)
                spawns = new Dictionary<string, List<DataBlockOverworldProvincePoint>> ();
            spawnsShuffled = new Dictionary<string, List<DataBlockOverworldProvincePoint>> ();

            #if UNITY_EDITOR
            if (entities != null)
            {

                foreach (var generator in entities)
                    generator.parent = this;
            }
            #endif

            foreach (var kvp in spawns)
            {
                var spawnGroup = kvp.Key;
                var listOrdered = kvp.Value;

                if (listOrdered == null)
                    continue;

                var listShuffled = new List<DataBlockOverworldProvincePoint> (listOrdered.Count);
                spawnsShuffled.Add (spawnGroup, listShuffled);

                foreach (var block in listOrdered)
                    listShuffled.Add (block);
                listShuffled.Shuffle ();
            }

            borderBounds = new Bounds(Vector3.zero, Vector3.zero);
            if(border.Count > 0)
            {
				borderBounds = new Bounds(border[0], Vector3.zero);

                foreach (var borderPos in border)
                {
	                borderBounds.Encapsulate(borderPos);
                }
            }

            ComputeBorderTriangles();

            #if UNITY_EDITOR
            DataMultiLinkerOverworldProvinceBlueprints.selection = null;
            // visible.Clear ();
            #endif
        }

        private void CheckNeighbours ()
        {
            if (neighbourData == null)
                neighbourData = new SortedDictionary<string, DataBlockOverworldProvinceNeighbor> ();

            foreach (var kvp in neighbourData)
            {
                var neighbourName = kvp.Key;
                if (!DataMultiLinkerOverworldProvinceBlueprints.data.ContainsKey (neighbourName))
                {
                    Debug.LogWarning ($"Encountered an invalid neighbour key {neighbourName} in province {key}");
                    continue;
                }

                var neighbour = DataMultiLinkerOverworldProvinceBlueprints.GetEntry (neighbourName);
                if (neighbour.neighbourData == null)
                    neighbour.neighbourData = new SortedDictionary<string, DataBlockOverworldProvinceNeighbor> ();

                if (!neighbour.neighbourData.ContainsKey (key))
                    neighbour.neighbourData.Add (key, new DataBlockOverworldProvinceNeighbor ());

                var neighborBlock = kvp.Value;
                var neighborBlockOpposite = neighbour.neighbourData[key];

                neighborBlockOpposite.allowActorSearch = neighborBlock.allowActorSearch;
                neighborBlockOpposite.allowConvoys = neighborBlock.allowConvoys;
                neighborBlockOpposite.allowCounterAttacks = neighborBlock.allowCounterAttacks;
                neighborBlockOpposite.allowWar = neighborBlock.allowWar;
            }
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

        private bool AreBordersVisible () => DataMultiLinkerOverworldProvinceBlueprints.Presentation.showBorder;
        private bool AreEntitiesVisible () => DataMultiLinkerOverworldProvinceBlueprints.Presentation.showEntities;
        private bool AreNeighborsVisible () => DataMultiLinkerOverworldProvinceBlueprints.Presentation.showNeighbours;
        private bool AreSpawnsVisible () => DataMultiLinkerOverworldProvinceBlueprints.Presentation.showSpawns;
        private bool AreObjectivesDecisiveVisible () => DataMultiLinkerOverworldProvinceBlueprints.Presentation.showObjectivesDecisive;

        public override void SaveText ()
        {
            if (!IsTextSavingPossible ())
                return;

            DataManagerText.TryAddingTextToLibrary (TextLibs.overworldProvinces, $"{key}_name", textName);
            DataManagerText.TryAddingTextToLibrary (TextLibs.overworldProvinces, $"{key}_desc", textDesc);
        }

        private Color GetElementColor (int index) => DataEditor.GetColorFromElementIndex (index);

        [PropertyOrder (-2), ShowIf ("@spawns != null")]
        [ValueDropdown ("GetSpawnGroupKeys")]
        [YamlIgnore]
        public string spawnKeyEditable = "general";

        private IEnumerable<string> GetSpawnGroupKeys =>
            spawns != null ? spawns.Keys : null;

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

        [ButtonGroup ("A"), Button ("Reverse"), PropertyOrder (-3), ShowIf ("IsSelectedInInspector")]
        public void ReverseBorderPoints ()
        {
            border.Reverse ();
        }

        [ShowIf ("@IsSelectedInInspector () && spawns != null && !spawns.ContainsKey (\"obj_decisive\")")]
        [ButtonGroup ("A"), Button ("Add obj. spawn"), PropertyOrder (-3)]
        public void AddObjectiveSpawn ()
        {
            spawns["obj_decisive"] = new List<DataBlockOverworldProvincePoint>
            {
                new DataBlockOverworldProvincePoint
                {
                    position = position + Quaternion.Euler (0f, Random.Range (0f, 360f), 0f) * Vector3.forward
                }
            };
        }

        private Color GetHighlightColor (float hue) => Color.HSVToRGB (hue, 0.2f, 1f);


        [ShowIf ("AreObjectivesDecisiveVisible")]
        [ButtonGroup ("B2"), Button ("+ CR / Executor", ButtonSizes.Large), PropertyOrder (10), GUIColor ("@GetHighlightColor (0.0f)")]
        public void InsertBossCruiserExecutor () =>
            InsertObjectiveDecisive ("obj_boss", "squad_boss_cruiser_executor");

        [ShowIf ("AreObjectivesDecisiveVisible")]
        [ButtonGroup ("B2"), Button ("+ CR / Interdictor", ButtonSizes.Large), PropertyOrder (10), GUIColor ("@GetHighlightColor (0.08f)")]
        public void InsertBossCruiserInterdictor () =>
            InsertObjectiveDecisive ("obj_boss", "squad_boss_cruiser_interdictor");


        [ShowIf ("AreObjectivesDecisiveVisible")]
        [ButtonGroup ("B1"), Button ("+ FR / Tutorial", ButtonSizes.Large), PropertyOrder (10), GUIColor ("@GetHighlightColor (0.30f)")]
        public void InsertBossFrigate1 () =>
            InsertObjectiveDecisive ("obj_boss", "squad_boss_frigate_tutorial");

        [ShowIf ("AreObjectivesDecisiveVisible")]
        [ButtonGroup ("B1"), Button ("+ FR / Artillery", ButtonSizes.Large), PropertyOrder (10), GUIColor ("@GetHighlightColor (0.35f)")]
        public void InsertBossFrigate2 () =>
            InsertObjectiveDecisive ("obj_boss", "squad_boss_frigate_artillery");

        [ShowIf ("AreObjectivesDecisiveVisible")]
        [ButtonGroup ("B1"), Button ("+ FR / Striker", ButtonSizes.Large), PropertyOrder (10), GUIColor ("@GetHighlightColor (0.4f)")]
        public void InsertBossFrigate3 () =>
            InsertObjectiveDecisive ("obj_boss", "squad_boss_frigate_striker");


        [ShowIf ("AreObjectivesDecisiveVisible")]
        [ButtonGroup ("B"), Button ("+ SH / Beam", ButtonSizes.Large), PropertyOrder (10), GUIColor ("@GetHighlightColor (0.55f)")]
        public void InsertBossShieldbearerBeam () =>
            InsertObjectiveDecisive ("obj_boss", "squad_boss_shieldbearer_beam");

        [ShowIf ("AreObjectivesDecisiveVisible")]
        [ButtonGroup ("B"), Button ("+ SH / AC", ButtonSizes.Large), PropertyOrder (10), GUIColor ("@GetHighlightColor (0.62f)")]
        public void InsertBossShieldbearerCannon () =>
            InsertObjectiveDecisive ("obj_boss", "squad_boss_shieldbearer_cannon");

        [ShowIf ("AreObjectivesDecisiveVisible")]
        [ButtonGroup ("B"), Button ("+ SH / MG", ButtonSizes.Large), PropertyOrder (10), GUIColor ("@GetHighlightColor (0.7f)")]
        public void InsertBossShieldbearerMG () =>
            InsertObjectiveDecisive ("obj_boss", "squad_boss_shieldbearer_mg");


        [ShowIf ("@AreObjectivesDecisiveVisible () && IsObjectiveDecisivePresent ()")]
        [Button ("- Clear key objectives", ButtonSizes.Large), PropertyOrder (10)]
        public void ClearObjectives ()
        {
            warObjectivesDecisive = null;
            warObjectivesDecisiveRequired = 0;
        }

        private bool IsObjectiveDecisivePresent ()
        {
            return warObjectivesDecisive != null && warObjectivesDecisive.Count > 0;
        }

        // [Button ("Insert key objective", ButtonSizes.Large), PropertyOrder (10)]
        private void InsertObjectiveDecisive ([ValueDropdown ("@DataMultiLinkerUnitComposite.data.Keys")] string blueprintKey)
        {
            InsertObjectiveDecisive ("obj_boss", blueprintKey);
        }

        private void InsertObjectiveDecisive (string nameInternal, [ValueDropdown ("@DataTagUtility.GetKeys (DataMultiLinkerUnitComposite.data, true, true, 0)")] string blueprintKey)
        {
            warObjectivesDecisiveRequired = 1;
            warObjectivesDecisive = new List<DataBlockProvinceWarObjectiveDecisive>
            {
                new DataBlockProvinceWarObjectiveDecisive ()
                {
                    nameInternalSuffix = nameInternal,
                    tagsUsed = false,
                    tags = null,
                    blueprintKey = blueprintKey
                }
            };
        }

        public string GetFirstDecisiveObjectiveDesc ()
        {
            if (warObjectivesDecisiveRequired < 1 || warObjectivesDecisive == null || warObjectivesDecisive.Count == 0)
                return null;

            var entry = warObjectivesDecisive[0];
            if (entry == null)
                return null;

            if (entry.tagsUsed)
            {
                if (entry.tags != null)
                {
                    foreach (var kvp in entry.tags)
                        return $"{kvp.Key} (tag)";
                }
            }
            else
                return entry.blueprintKey;

            return null;
        }

        private bool IsSelectedInInspector ()
        {
            return DataMultiLinkerOverworldProvinceBlueprints.selection == this;
        }

        /*
        [ButtonGroup ("A"), Button ("Show"), PropertyOrder (-2), HideIf ("IsVisibleInInspector")]
        public void ShowInInspector ()
        {
            if (!visible.Contains (this))
                visible.Add (this);
        }

        [ButtonGroup ("A"), Button ("Hide"), PropertyOrder (-2), ShowIf ("IsVisibleInInspector")]
        public void HideInInspector ()
        {
            if (visible.Contains (this))
                visible.Remove (this);
        }

        private bool IsVisibleInInspector ()
        {
            return visible != null && visible.Contains (this);
        }
        */

        #endif
    }
}
