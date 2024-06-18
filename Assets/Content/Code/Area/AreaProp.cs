using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Area
{
    [ExecuteInEditMode][SelectionBase]
    public class AreaProp : MonoBehaviour
    {
        public enum DestructionType
        {
            Indestructible = 0,
            BasicDestruction = 10,
            FallSimulated = 20,
            FallAnimated = 21,
        }

        public enum Compatibility
        {
            Floor = 0,
            WallStraightMiddle = 10,
            WallStraightBottomToFloor = 11,
            WallStraightTopToFloor = 12,
        }
        
        public enum RendererMode
        {
            ActiveConstantly,
            ActiveWhenIntact,
            ActiveWhenDestroyed
        }

        public enum PropertyTarget
        {
            All,
            OnlyIntact,
            OnlyDestroyed
        }

        [System.Serializable]
        public class PropRenderer
        { 
            public RendererMode mode = RendererMode.ActiveWhenIntact;
            public Renderer renderer;

            //public bool isPrefab = false; // that is a very strange feature, investigate and remove if possible - safe to deprecate.
            // Was introduced at the time when Unity did not support prefab nesting.

            public bool rotateRandomly = false;
            [ShowIf ("rotateRandomly")] 
            public float randomRotationMin = 0.0f;
            [ShowIf ("rotateRandomly")]
            public float randomRotationMax = 360.0f;
            public bool scaleRandomly = false;
            [ShowIf ("scaleRandomly")]
            public Vector3 scaleMin = Vector3.one;
            [ShowIf ("scaleRandomly")]
            public Vector3 scaleMax = Vector3.one;

            public bool offsetRandomly = false;
            [ShowIf ("offsetRandomly")]
            public Vector3 offsetScale = Vector3.zero;

            [HideInInspector]
            public List<PropBoneGroup> randomizedBoneGroups;
        }

        [System.Serializable]
        public class PropBoneGroup
        {
            public List<Transform> bones;
            public List<PropBoneGroupState> states;
            public bool modifyPositions = false;
        }

        [System.Serializable]
        public class PropBoneGroupState
        {
            public Vector3 position;
            public Vector3 rotation;
        }

        public AreaPropPrototypeData prototype;

        // Yes, this is the only way to change the info box icon that I've found to be working
        [InfoBox("@this.CheckID()", InfoMessageType.Info, "@resultMessageIcon == InfoMessageType.Info")]
        [InfoBox("@this.CheckID()", InfoMessageType.Warning, "@resultMessageIcon == InfoMessageType.Warning")]
        [InfoBox("@this.CheckID()", InfoMessageType.Error, "@resultMessageIcon == InfoMessageType.Error")]
        public int id = -1;
        
        public List<PropRenderer> renderers;

        public const float crushParameterOffset = -0.25f;
        public const float crushParameterMultiplier = 1.25f;
        public const float crushIntensityOnDeparture = 0.5f;

        public List<Vector3Int> pointsToCheck;

        [Header("Placement"), PropertySpace (4f)]
        public Compatibility compatibility = Compatibility.Floor;
        public bool linkRotationToConfiguration = false;
        public bool blockNavigation = false;
        public bool allowPositionOffset = false;
        public bool allowTinting = false;

        [Header("Destruction"), PropertySpace (4f)]
        public DestructionType destructionType = DestructionType.Indestructible;
        
        [ShowIf("DestructionEnabled")]
        public float destructionDuration = 0.5f;
        
        [ShowIf("DestructionEnabled")]
        public bool destructionEffectCustom = false;
        
        [ShowIf ("destructionEffectCustom"), ValueDropdown ("@DataMultiLinkerAssetPools.data.Keys")]
        public string destructionEffect;

        
        [ShowIf("DestructionEnabled"), ValueDropdown ("@AudioEvents.GetKeys()")]
        public string destructionAudioEvent;
        
        [ShowIf("DestructionEnabled")]
        public Vector3 offsetReveal = new Vector3 (0f, 1f, 0f);

        [ShowIf("DestructionEnabled")]
        public Vector3 offsetRemoval = new Vector3 (0f, -1f, 0f);
    
        [ShowIf("DestructionEnabled")]
        public bool compressOnDestruction = false;

        [ShowIf("DestructionEnabled")]
        public bool fadeOnDestruction = false;

        [ShowIf("DestructionEnabled")]
        public bool kickAboveCenter;

        [ShowIf("DestructionEnabled")]
        public bool disableAreaDamage = false;

        [Header("Destruction - Colliders"), PropertySpace (2f)]
        public Collider colliderMain;
        public List<Collider> collidersSecondary;

        [Header("Destruction - Fall"), PropertySpace (4f), ShowIf("DesctructionIsAnimatedOrSimulatedFall")]
        public bool destructionFallCurveCustom = false;
        
        [ShowIf ("destructionFallCurveCustom")]
        public AnimationCurve destructionFallCurve = new AnimationCurve 
        (
            new Keyframe (0f, 0f),
            new Keyframe (0.75f, 1f),
            new Keyframe (0.8f, 0.8f),
            new Keyframe (0.85f, 1f),
            new Keyframe (0.9f, 0.9f),
            new Keyframe (1f, 1f)
        );

        [Header("Destruction - Fall - Rigidbody"), PropertySpace (2f), ShowIf("DesctructionIsSimulatedFall")]
        [InfoBox("Currently in a deprecated state", InfoMessageType.Warning)]
        public Rigidbody rigidbodyMain;

        [ShowIf("DesctructionIsSimulatedFall")]
        public Vector3 centerOfMass = new Vector3 (0f, 0f, 0f);

        [ShowIf("DesctructionIsSimulatedFall")]
        private float fallEndGroundHeight = 0f;

        [ShowIf("DesctructionIsSimulatedFall")]
        public float fallEndFeelerTriggerHeight = 2f;

        [ShowIf("DesctructionIsSimulatedFall")]
        public Transform fallEndFeeler;

        [Header("Destruction - Fall - Animated"), PropertySpace (2f), ShowIf("DesctructionIsAnimatedFall")]
        [InfoBox("Currently in a deprecated state", InfoMessageType.Warning)]
        public float animatedFallDuration = 1f;

        [ShowIf("DesctructionIsAnimatedFall")]
        public Vector2 animatedFallRotationHorizontal = new Vector2 (15f, 30f);

        [ShowIf("DesctructionIsAnimatedFall")]
        public Vector2 animatedFallRotationPitch = new Vector2 (70f, 90f);

        [ShowIf("DesctructionIsAnimatedFall")]
        public float animatedFallVerticalOffset = 0.35f;

        [ShowIf("DesctructionIsAnimatedFall")]
        public float animatedFallDirectionalOffset = 2f;

        [ShowIf("DesctructionIsAnimatedFall")]
        public bool animatedFallVelocityOffset = true;

        [ShowIf("DesctructionIsAnimatedFall")]
        public float animatedFallVelocityScale = 0.2f;

        private Quaternion animatedFallRotationFrom;
        private Quaternion animatedFallRotationTo;
        private Vector3 animatedFallPositionFrom;
        private Vector3 animatedFallPositionTo;
        private Vector3 animatedFallAxis;


        [Header("Transform"), PropertySpace (4f)]
        public bool rotateRandomly = false;

        [ShowIf ("rotateRandomly")]
        public bool useAllAxesForRandomRotation = false;

        [ShowIf ("rotateRandomly")]
        public float randomRotationMin = 0.0f;

        [ShowIf ("rotateRandomly")]
        public float randomRotationMax = 360.0f;
        public bool scaleRandomly = false;

        [ShowIf ("scaleRandomly")]
        public Vector3 scaleMin = Vector3.one;

        [ShowIf ("scaleRandomly")]
        public Vector3 scaleMax = Vector3.one;
        public bool flipRandomly = false;
        public bool mirrorOnZAxis = false;
        [Header("Lights"), PropertySpace (4f)]
        public bool activeLightsOnlyAtNight = false;
        public List<Light> activeLights;
        private List<float> activeLightIntensities;

        // public List<Projection> decals;

        /*private bool kickRequired = false;
        private bool kickRotationRequired = false;
        private float kickForce = 100f;
        private Vector3 kickDirection;
        private Vector3 kickPoint;

        [PropertySpace (4f)]]
        public bool testKick = false;
        public bool testReset = false;

        public float testKickHeight = 6f;
        public Vector3 testKickDirection = new Vector3 (0f, 0f, 1f);
        private Vector3 testPositionStart;*/

        private bool DestructionEnabled()
        {
            return (destructionType != DestructionType.Indestructible);
        }

        private bool DesctructionIsAnimatedOrSimulatedFall()
        {
            return (destructionType == DestructionType.FallAnimated) || (destructionType == DestructionType.FallSimulated);
        }

        private bool DesctructionIsSimulatedFall()
        {
            return (destructionType == DestructionType.FallSimulated);
        }

        private bool DesctructionIsAnimatedFall()
        {
            return (destructionType == DestructionType.FallAnimated);
        }

        private int idCached = -100;
        private string resultMessage = "";
        private InfoMessageType resultMessageIcon = InfoMessageType.Info;

        private string CheckID ()
        {
            if (id != idCached)
            {
                AreaAssetHelper.CheckResources ();

                if (AreaAssetHelper.propsPrototypesList != null)
                {
                    if (AreaAssetHelper.propsPrototypes.ContainsKey (id) && this != AreaAssetHelper.propsPrototypes[id].prefab)
                    {
                        resultMessage = "ID is already in use by " + AreaAssetHelper.propsPrototypes[id].name;
                        resultMessageIcon = InfoMessageType.Error;
                    }
                    else if (id < 0)
                    {
                        resultMessage = "This ID (" + id + ") is not valid";
                        resultMessageIcon = InfoMessageType.Warning;
                    }
                    else
                    {
                        resultMessage = "ID is unique";
                        resultMessageIcon = InfoMessageType.Info;
                    }
                }
                idCached = id;
            }
            return resultMessage;
        }

        [HorizontalGroup("ButtonsAtTheBottom", 0.5f)]
        [Button("Fill Renderer References", ButtonSizes.Large)]
        public void FillReferences ()
        {
            renderers = new List<PropRenderer> ();
            List<Renderer> rendererComponents = new List<Renderer> (gameObject.GetComponentsInChildren<Renderer> ());
            for (int i = 0; i < rendererComponents.Count; ++i)
            {
                Renderer mr = rendererComponents[i];
                PropRenderer pr = new PropRenderer ();
                pr.renderer = mr;
                pr.mode = RendererMode.ActiveWhenIntact;
                renderers.Add (pr);
            }

            activeLights = new List<Light> (gameObject.GetComponentsInChildren<Light> ());
            // decals = new List<Projection> (gameObject.GetComponentsInChildren<Projection> ());
        }

        [HorizontalGroup("ButtonsAtTheBottom", 0.5f)]
        [Button("Collect collider", ButtonSizes.Large)]
        public void CollectColliderAndRigidbody ()
        {
            colliderMain = gameObject.GetComponent<Collider> ();
            rigidbodyMain = gameObject.GetComponent<Rigidbody> ();
        }

        [Button("Set Center of Mass from Collider", ButtonSizes.Medium)]
        public void SetCenterOfMassFromCollider ()
        {
            if (colliderMain != null)
                centerOfMass = colliderMain.bounds.center;
        }
    }
}