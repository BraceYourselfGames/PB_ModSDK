using System.Collections.Generic;
using CustomRendering;
using PhantomBrigade.Data;
using UnityEngine;

namespace Area
{
    public static class AreaAssetHelper
    {
        // Used for GameObject prototypes like prop collision objects, which are not prefabs
        private static GameObject prototypeHolder;
        private static string prototypeHolderName = "PrototypeHolder";

        private static readonly string resourcesDirPathProps = "Content/Props";
        private static readonly string resourcesDirPathAreaPreviews = "Content/Textures/Scenarios";

        private static List<ResourceDatabaseEntryRuntime> propsResources;
        public static Dictionary<int, AreaPropPrototypeData> propsPrototypes;
        public static List<AreaPropPrototypeData> propsPrototypesList;

        private static Dictionary<long, InstancedMeshRenderer> propsInstancedModels;
        private static InstancedMeshRenderer propsInstancedModelFallback;
        private static List<long> propsInstancedIDFailures = new List<long> ();

        public static List<ResourceDatabaseEntryRuntime> imageResources;
        public static Dictionary<string, Texture> imageReferences;
        public static List<Texture> imageReferenceList;

        public const int propIDDebrisMain = 103;
        public const int propIDDebrisPile = 100;

        private static bool autoloadAttempted = false;
        private static bool log = false;

        public static void CheckResources ()
        {
            if (!autoloadAttempted)
            {
                autoloadAttempted = true;
                if (propsResources == null)
                {
                    LoadResources ();
                    #if UNITY_EDITOR
                    propsHiddenWithECS.UnionWith (bushIDs);
                    propsHiddenWithECS.UnionWith (grassIDs);
                    propsHiddenWithECS.UnionWith (treeIDs);
                    #endif
                }
            }
        }

        public static bool AreAssetsPresent ()
        {
            return propsPrototypesList != null && propsPrototypesList.Count > 0;
        }

        public static void LoadResources ()
        {
            if (Utilities.isPlaymodeChanging)
                return;

            if (Application.isPlaying)
            {
                if (prototypeHolder == null)
                {
                    prototypeHolder = new GameObject ();
                    GameObject.DontDestroyOnLoad (prototypeHolder);
                    prototypeHolder.name = prototypeHolderName;
                }
            }

            ResourceDatabaseContainer resourceDatabase = ResourceDatabaseManager.GetDatabase ();
            if (resourceDatabase == null || resourceDatabase.entries == null || resourceDatabase.entries.Count == 0)
                return;

            propsPrototypes = new Dictionary<int, AreaPropPrototypeData> ();
            propsPrototypesList = new List<AreaPropPrototypeData> ();
            propsInstancedIDFailures = new List<long> ();
            propsInstancedModels = new Dictionary<long, InstancedMeshRenderer> ();
            propsInstancedModelFallback = new InstancedMeshRenderer
            {
                mesh = PrimitiveHelper.GetPrimitiveMesh (PrimitiveType.Cube),
                material = PrimitiveHelper.GetDefaultMaterial (),
                instanceLimit = DataLinkerRendering.data.defaultComputeBufferSize,
                subMesh = 0,
                castShadows = UnityEngine.Rendering.ShadowCastingMode.On,
                receiveShadows = true
            };

            propsResources = new List<ResourceDatabaseEntryRuntime> ();

            if (resourceDatabase.entries.ContainsKey (resourcesDirPathProps))
            {
                ResourceDatabaseEntryRuntime infoDir = ResourceDatabaseManager.GetEntryByPath (resourcesDirPathProps);
                ResourceDatabaseManager.FindResourcesRecursive (propsResources, infoDir, 1, ResourceDatabaseEntrySerialized.Filetype.Prefab);
            }

            if (log)
                Debug.Log ("AreaAssetHelper | LoadResources | Gathered props: " + propsResources.Count);

            for (int i = 0; i < propsResources.Count; ++i)
            {
                ResourceDatabaseEntryRuntime entry = propsResources[i];
                GameObject prefab = entry.GetContent<GameObject> ();

                #if UNITY_EDITOR
                if (prefab != null)
                {
                    UnityEditor.PrefabType prefabType = UnityEditor.PrefabUtility.GetPrefabType(prefab);
                    if (prefabType == UnityEditor.PrefabType.ModelPrefab)
                        continue;
                }
                #endif

                if (prefab != null)
                {
                    AreaProp prop = prefab.GetComponent<AreaProp> ();
                    if (prop != null)
                        RegisterPropPrototype (prop);
                }
            }

            if (Application.isPlaying)
            {
                if (log)
                    Debug.Log (string.Format ("AreaAssetHelper | LoadResources | Registered prop models: {0}", propsInstancedModels.Count));
            }

            imageResources = new List<ResourceDatabaseEntryRuntime> ();
            if (resourceDatabase.entries.ContainsKey (resourcesDirPathAreaPreviews))
            {
                ResourceDatabaseEntryRuntime infoDir = ResourceDatabaseManager.GetEntryByPath (resourcesDirPathAreaPreviews);
                ResourceDatabaseManager.FindResourcesRecursive (imageResources, infoDir, 1, ResourceDatabaseEntrySerialized.Filetype.Texture);
            }

            if (log)
                Debug.Log ("AreaAssetHelper | LoadResources | Gathered scenario preview images: " + imageResources.Count);

            imageReferenceList = new List<Texture> ();
            imageReferences = new Dictionary<string, Texture> ();
            for (int i = 0; i < imageResources.Count; ++i)
            {
                ResourceDatabaseEntryRuntime entry = imageResources[i];
                Texture image = entry.GetContent<Texture> ();
                if (image != null)
                {
                    string key = image.name;
                    if (!imageReferences.ContainsKey (key))
                    {
                        imageReferences.Add (key, image);
                        imageReferenceList.Add (image);
                    }
                }
            }
        }

        public static AreaPropPrototypeData GetPropPrototype (int id)
        {
            CheckResources ();
            if (propsPrototypes.ContainsKey (id))
                return propsPrototypes[id];
            else
                return null;
        }

        public static readonly List<int> grassIDs = new List<int>
        {
            20501,
            20502,
            20503,
            20504,
            20505,
            20506,
            20507,

            20521,
            20522,
            20523,
            20524,
            20525,
            20526,
            20527,
            20528,
            20529
        };

        public static readonly List<int> bushIDs = new List<int>
        {
            20101,
            20102,

            20201,
            20202,
            20203,
            20204,
            20205,

            20305,
            20306,
            20307,

            20701,
            20702,
            20703,
            20704,
            20705,
            20706,
        };

        public static readonly List<int> treeIDs = new List<int>
		{
			20001,
			20002,
			20003,
			20004,
			20005,
			20006,
			20007,
			20008,
            20009,
            20010,
            20011,
            20012,

            20301,
            20302,
            20303,
            20304,

            20401,
			20405,

            20601,
            20602,
        };

		public static Texture GetAreaImage (string key)
        {
            CheckResources ();

            if (imageReferences.ContainsKey (key))
                return imageReferences[key];
            else
            {
                Debug.LogWarning ("AreaAssetHelper | GetAreaImage | Couldn't find a scenario preview image with with key: " + key);
                return null;
            }
        }

        private static List<byte> maskEmpty = new List<byte> (new byte[] { 0 });
        private static List<byte> maskFloor = new List<byte> (new byte[] { 15 });
        private static List<byte> maskWallStraight_Middle = new List<byte> (new byte[] { 204, 102, 51, 153 });
        private static List<byte> maskWallStraight_BottomToFloor = new List<byte> (new byte[] { 63, 111, 159, 207 });
        private static List<byte> maskWallStraight_TopToFloor = new List<byte> (new byte[] { 3, 6, 9, 12 });

        private static List<byte> maskFloorToWallStraight = new List<byte> (new byte[] { 3, 6, 9, 12 });
        private static List<byte> maskFloorToWallCorner = new List<byte> (new byte[] { 1, 2, 4, 8 });
        private static List<byte> maskWallToFloorStraight = new List<byte> (new byte[] { 60, 105, 150, 195, 61, 107, 158, 199, 62, 109, 151, 203, 63, 111, 159, 207 });
        private static List<byte> maskWallToFloorCorner = new List<byte> (new byte[] { 30, 45, 75, 135, 31, 47, 79, 143 });
        private static List<byte> maskVerticalClearance = new List<byte> (new byte[] { 0, 1, 2, 4, 8, 3, 6, 9, 12 });

        private static Dictionary<AreaProp.Compatibility, List<byte>> masks;

        public static List<byte> GetPropMask (AreaProp.Compatibility compatibility)
        {
            if (masks == null)
            {
                masks = new Dictionary<AreaProp.Compatibility, List<byte>> ();
                masks.Add (AreaProp.Compatibility.Floor, maskFloor);
                masks.Add (AreaProp.Compatibility.WallStraightMiddle, maskWallStraight_Middle);
                masks.Add (AreaProp.Compatibility.WallStraightBottomToFloor, maskWallStraight_BottomToFloor);
                masks.Add (AreaProp.Compatibility.WallStraightTopToFloor, maskWallStraight_TopToFloor);
            }

            return masks[compatibility];
        }

        private static Vector3 GetDirectionFromAngle (float yaw, float pitch = 0f)
        {
            return Quaternion.Euler (pitch, yaw, 0f) * Vector3.forward;
        }

        private static Dictionary<byte, Vector3> surfaceDirections = new Dictionary<byte, Vector3>
        {
            { 12, GetDirectionFromAngle (0f) },
            { 204, GetDirectionFromAngle (0f) },
            { 207, GetDirectionFromAngle (0f, -45f) },

            { 8, GetDirectionFromAngle (45f) },
            { 136, GetDirectionFromAngle (45f) },
            { 143, GetDirectionFromAngle (45f, -45f) },

            { 9, GetDirectionFromAngle (90f) },
            { 153, GetDirectionFromAngle (90f) },
            { 159, GetDirectionFromAngle (90f, -45f) },

            { 1, GetDirectionFromAngle (135f) },
            { 17, GetDirectionFromAngle (135f) },
            { 31, GetDirectionFromAngle (135f, -45f) },

            { 3, GetDirectionFromAngle (180f) },
            { 51, GetDirectionFromAngle (180f) },
            { 63, GetDirectionFromAngle (180f, -45f) },

            { 2, GetDirectionFromAngle (225f) },
            { 34, GetDirectionFromAngle (225f) },
            { 47, GetDirectionFromAngle (225f, -45f) },

            { 6, GetDirectionFromAngle (270f) },
            { 102, GetDirectionFromAngle (270f) },
            { 111, GetDirectionFromAngle (270f, -45f) },

            { 4, GetDirectionFromAngle (315f) },
            { 68, GetDirectionFromAngle (315f) },
            { 79, GetDirectionFromAngle (315f, -45f) },
        };

        public static Vector3 GetSurfaceDirection (byte spotConfiguration)
        {
            if (surfaceDirections.TryGetValue (spotConfiguration, out var direction))
                return direction;
            else
                return Vector3.up;
        }

        private static string lodFilter = "_lod1";

        private static void RegisterPropPrototype (AreaProp prop)
        {
            if (prop == null || propsPrototypes.ContainsKey (prop.id) || prop.id < 1)
                return;

            AreaPropPrototypeData prototype = new AreaPropPrototypeData ();
            propsPrototypes.Add (prop.id, prototype);
            propsPrototypesList.Add (prototype);

            prototype.id = prop.id;
            prototype.name = prop.gameObject.name;
            prototype.prefab = prop;
            prototype.subObjects = new List<AreaPropSubObject> (prop.renderers.Count);

            for (int i = 0; i < prop.renderers.Count; ++i)
            {
                AreaProp.PropRenderer pr = prop.renderers[i];
                if (pr.renderer == null || !pr.renderer.enabled)
                    continue;

                if (pr.renderer.gameObject.name.EndsWith (lodFilter))
                    continue;

                /*
                // Uncomment to check which props use special per-sub-object randomization
                if (pr.offsetRandomly || pr.rotateRandomly || pr.scaleRandomly)
                {
                    Debug.Log
                    (
                        "Prop " + prop.name +
                        " sub-object " + i +
                        " named " + pr.renderer.gameObject.name +
                        " has S/O randomization | P: " + pr.offsetRandomly +
                        " | R: " + pr.rotateRandomly +
                        " | S: " + pr.scaleRandomly,
                        prop.gameObject
                    );
                }
                */

                Mesh mesh = null;
                if (pr.renderer is SkinnedMeshRenderer)
                {
                    mesh = ((SkinnedMeshRenderer)pr.renderer).sharedMesh;
                }
                else if (pr.renderer is MeshRenderer)
                {
                    MeshFilter mf = pr.renderer.gameObject.GetComponent<MeshFilter> ();
                    if (mf != null)
                        mesh = mf.sharedMesh;
                }

                if (mesh == null)
                    continue;

                bool root = pr.renderer.gameObject == prop.gameObject;
                int customTransformsUsed = 0;

                for (int m = 0; m < pr.renderer.sharedMaterials.Length; ++m)
                {
                    Material sharedMaterial = pr.renderer.sharedMaterials[m];
                    if (sharedMaterial == null)
                    {
                        Debug.LogWarning ($"Material {m} in object {pr.renderer.name} in prop {prop.name} is null");
                        continue;
                    }

                    if (m >= mesh.subMeshCount)
                    {
                        // Debug.LogWarning (string.Format ("Invalid material count {0} for a mesh {1} containing {2} submeshes",
                        //     m, pr.renderer.name, mesh.subMeshCount), pr.renderer.gameObject);
                        continue;
                    }

                    if (sharedMaterial.HasProperty ("_PackedProp"))
                    {
                        sharedMaterial.SetVector ("_PackedProp", new Vector4 (1f, 0f, 1f, 0f));
                    }

                    // Fixing scale since fast path doesn't support non-uniform scaling
                    Vector3 scale = pr.renderer.transform.localScale;
                    if (scale.x != scale.y || scale.y != scale.z || scale.x != scale.z)
                        scale = Vector3.one * ((scale.x + scale.y + scale.z) / 3);

                    long instancedModelID = GetInstancedModelID (prop.id, (byte)i, (byte)m, false);
                    if (!propsInstancedModels.ContainsKey (instancedModelID))
                    {
                        InstancedMeshRenderer instancedModel = new InstancedMeshRenderer
                        {
                            mesh = mesh,
                            material = sharedMaterial,
                            instanceLimit = DataLinkerRendering.data.defaultComputeBufferSize,
                            subMesh = m,
                            castShadows = pr.renderer.shadowCastingMode,
                            receiveShadows = true,
                            id = instancedModelID
                        };

                        propsInstancedModels.Add (instancedModelID, instancedModel);

                        int customTransformIndex = -1;
                        if (pr.offsetRandomly || pr.rotateRandomly || pr.scaleRandomly)
                        {
                            customTransformIndex = customTransformsUsed;
                            customTransformsUsed += 1;
                        }

                        prototype.subObjects.Add (new AreaPropSubObject
                        {
                            name = string.Format ("{0} (material {1}: {2})", pr.renderer.name, m, sharedMaterial.name),
                            contextIndex = i,
                            modelID = instancedModelID,
                            position = root ? Vector3.zero : pr.renderer.transform.localPosition,
                            rotation = root ? Quaternion.identity : pr.renderer.transform.localRotation,
                            scale = root ? Vector3.one : scale,
                            customTransformIndex = customTransformIndex
                        });
                    }
                    else
                    {
                        Debug.LogWarning
                        (
                            string.Format ("AAH | RegisterInstancedModel | Hash collision: {0} | Prop ID: {1} | Sub-object: {2}",
                            instancedModelID, prop.id, i)
                        );
                    }
                }

                // Prop collider is a basic GameObject hosting only a collider
                // for registering impacts (no legacy prop component, meshes etc.)

                if (Application.isPlaying)
                {
                    Collider colliderSource = prop.GetComponent<Collider> ();
                    if (colliderSource != null)
                    {
                        GameObject colliderObject = new GameObject ();
                        colliderObject.name = prop.name + "_Collider";
                        colliderObject.transform.parent = prototypeHolder.transform;

                        if (colliderSource is BoxCollider)
                        {
                            BoxCollider colliderSourceBox = colliderSource as BoxCollider;
                            BoxCollider colliderPrefabBox = colliderObject.AddComponent<BoxCollider> ();

                            colliderPrefabBox.center = colliderSourceBox.center;
                            colliderPrefabBox.size = colliderSourceBox.size;
                            colliderPrefabBox.isTrigger = true;
                            prototype.prefabCollider = colliderPrefabBox;
                        }
                        else if (colliderSource is SphereCollider)
                        {
                            SphereCollider colliderSourceSphere = colliderSource as SphereCollider;
                            SphereCollider colliderPrefabSphere = colliderObject.AddComponent<SphereCollider> ();

                            colliderPrefabSphere.center = colliderSourceSphere.center;
                            colliderPrefabSphere.radius = colliderSourceSphere.radius;
                            colliderPrefabSphere.isTrigger = true;
                            prototype.prefabCollider = colliderPrefabSphere;
                        }
                        else if (colliderSource is CapsuleCollider)
                        {
                            CapsuleCollider colliderSourceCapsule = colliderSource as CapsuleCollider;
                            CapsuleCollider colliderPrefabCapsule = colliderObject.AddComponent<CapsuleCollider> ();

                            colliderPrefabCapsule.center = colliderSourceCapsule.center;
                            colliderPrefabCapsule.radius = colliderSourceCapsule.radius;
                            colliderPrefabCapsule.height = colliderSourceCapsule.height;
                            colliderPrefabCapsule.direction = colliderSourceCapsule.direction;
                            colliderPrefabCapsule.isTrigger = true;
                            prototype.prefabCollider = colliderPrefabCapsule;
                        }
                        else
                        {
                            Debug.LogWarning ("Unsupported collider type detected on root of prop " + prop.name, prop.gameObject);
                        }
                    }
                }
            }
        }

        public static long GetInstancedModelID (int propID, byte subObject, byte material, bool check)
        {
            PackIntegerAndBytesToLong pack = new PackIntegerAndBytesToLong
            {
                integer = propID,
                byte0 = subObject,
                byte1 = material,
                byte2 = 0,
                byte3 = 0
            };

            long instanceRendererID = pack.result;
            if (check && !propsInstancedModels.ContainsKey (instanceRendererID) && !propsInstancedIDFailures.Contains (instanceRendererID))
            {
                propsInstancedIDFailures.Add (instanceRendererID);
                Debug.LogWarning
                (
                    string.Format ("Failed to find the model for prop ID {0} and sub-object {1} | Total fails: {2}",
                    instanceRendererID, subObject, propsInstancedIDFailures.Count)
                );
            }

            return instanceRendererID;
        }

        public static InstancedMeshRenderer GetInstancedModel (long id)
        {
            if (propsInstancedModels.ContainsKey (id))
                return propsInstancedModels[id];
            else
                return propsInstancedModelFallback;
        }

        #if UNITY_EDITOR
        // Some props use a shader that lets you control the visibility of the props. Others don't,
        // namely the vegetation props. Keep a list of the ones that can't be hidden through the
        // shader so that when layer editing mode asks to hide/show props, the routine in AreaManager
        // can look up which strategy to use.
        public static readonly HashSet<int> propsHiddenWithECS = new HashSet<int> ()
        {
            20801, // crop patch
            20802, // crop patch
            20803, // crop patch
            20804, // crop patch
            20805, // crop patch
            20806, // crop patch
            20807, // crop patch
            20808, // crop patch
            20809, // crop patch
            20901, // creeper ivy
            20902, // creeper ivy
            20903, // creeper ivy
            20904, // creeper ivy
            20905, // creeper ivy
            20906, // creeper ivy
            20907, // creeper ivy
            20908, // creeper ivy
        };
        #endif
    }
}
