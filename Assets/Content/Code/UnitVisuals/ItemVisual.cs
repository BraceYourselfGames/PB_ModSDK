using System;
using System.Collections.Generic;
using System.Text;
using PhantomBrigade;
using PhantomBrigade.Data;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public class ItemAttachmentLanding
{
    [ValueDropdown ("@DataMultiLinkerAssetPools.data.Keys")]
    [InlineButtonClear]
    public string visualKey = "acs_landing_engine_mech";

    public Transform holder;
}

[Serializable]
public class ItemVisualCycle
{
    public AnimationClip clip;
    public GameObject root;
    public Vector3 positionContact;
    public float strideLength;
    public string fxContact = "fx_podejection_shockwave";
}

[Serializable]
public class ItemVisualLegStepConfig
{
    public bool update = true;
    public int liftoffLimit = 1;
    
    public float threshold = 5f;
    public float elevation = 5f;
    public float duration = 0.5f;
    
    public float velocityLimit = 1f;
    public float velocityInfluence = 1f;
    public float raycastDistance = 0f;
    
    public AnimationCurve curveElevation = new AnimationCurve (new [] { new Keyframe (0f, 0f), new Keyframe (0.5f, 1f), new Keyframe (1f, 0f) });
    public AnimationCurve curveTranslation = AnimationCurve.EaseInOut (0f, 0f, 1f, 1f);
    
    [PropertySpace (4)]
    [ValueDropdown ("@DataMultiLinkerAssetPools.data.Keys")]
    public string fxContact = "fx_mech_movement_foot_brake";

    [PropertySpace (4)]
    [ValueDropdown ("@DataMultiLinkerAssetPools.data.Keys")]
    public string fxLiftoff = "fx_mech_movement_foot_launch";

    public string audioContact = null;
    public string audioLiftoff = null;
    
    public void FillFromSource (ItemVisualLegStepConfig source)
    {
        if (source == null)
            return;

        update = source.update;
        liftoffLimit = source.liftoffLimit;
        
        threshold = source.threshold;
        elevation = source.elevation;
        duration = source.duration;
        
        velocityLimit = source.velocityLimit;
        velocityInfluence = source.velocityInfluence;
        raycastDistance = source.raycastDistance;
        
        curveElevation = source.curveElevation;
        curveTranslation = source.curveTranslation;
        
        fxContact = source.fxContact;
        fxLiftoff = source.fxLiftoff;
        
        audioContact = source.audioContact;
        audioLiftoff = source.audioLiftoff;
    }
}

[Serializable]
public class ItemVisualLegGroup
{
    public string key = "default";
    
    [BoxGroup]
    public ItemVisualLegStepConfig step = new ItemVisualLegStepConfig ();
    
    [ListDrawerSettings (DefaultExpandedState = false, ShowIndexLabels = true)]
    public List<ItemVisualLeg> legs = new List<ItemVisualLeg> ();

    [NonSerialized, HideInInspector]
    public ItemVisual parent;
}

[Serializable]
public class ItemVisualLeg
{
    private const string bgMain = "Main";
    private const string bgValues = "AreValuesVisible";

    public bool enabled = true;

    [LabelText ("Use 3 Pitch Joints")]
    public bool tripleMode = false;

    [InlineButton ("FillTransforms", "Fill")]
    public Transform root;
    public Transform jointYawRoot;
    public Transform jointPitchRoot;
    public Transform jointPitchMid;
    public Transform jointPitchLow;
    public Transform locatorTargetTip;
    public Transform locatorTargetAim;
    public Transform locatorTargetRest;
    
    public Transform fxContactParent;
    public Transform fxLiftoffParent;

    [ShowInInspector, ReadOnly]
    public float reach 
    {
        get
        {
            if (!AreDependenciesPresent ())
                return 0f;
            
            var dist = 0f; 
            dist += Vector3.Distance (jointPitchRoot.position, jointPitchMid.position);
            
            if (tripleMode)
            {
                dist += Vector3.Distance (jointPitchMid.position, jointPitchLow.position);
                dist += Vector3.Distance (jointPitchLow.position, locatorTargetTip.position);
            }
            else
                dist += Vector3.Distance (jointPitchMid.position, locatorTargetTip.position);

            return dist;
        }
    }
    
    [ShowIfGroup (bgValues), NonSerialized, ShowInInspector, ReadOnly]
    public bool log;

    private bool AreValuesVisible => Application.isPlaying;

    public void FillFromSource (ItemVisualLeg source)
    {
        if (source == null)
            return;

        enabled = source.enabled;
        tripleMode = source.tripleMode;
        root = source.root;
        jointYawRoot = source.jointYawRoot;
        jointPitchRoot = source.jointPitchRoot;
        jointPitchMid = source.jointPitchMid;
        jointPitchLow = source.jointPitchLow;
        locatorTargetTip = source.locatorTargetTip;
        locatorTargetAim = source.locatorTargetAim;
        locatorTargetRest = source.locatorTargetRest;
    }
    
    public void FillTransforms ()
    {
        if (root == null)
            return;

        jointYawRoot = root.FindChildDeep ("joint_y", true);
        jointPitchRoot = root.FindChildDeep ("joint_x_01", true);
        jointPitchMid = root.FindChildDeep ("joint_x_02", true);
        jointPitchLow = root.FindChildDeep ("joint_x_03", true);
        locatorTargetTip = root.FindChildDeep ("target_tip", true);
        locatorTargetAim = root.FindChildDeep ("target_aim", true);
        locatorTargetRest = root.FindChildDeep ("target_rest", true);
    }

    public void Evaluate (bool stepAllowed, float timeCurrent, ItemVisualLegStepConfig stepConfig, Vector3 velocity = default)
    {
        
    }
    
    private bool AreDependenciesPresent ()
    {
        bool transformsPresent =
            jointYawRoot != null &&
            jointPitchRoot != null &&
            jointPitchMid != null &&
            locatorTargetTip != null &&
            locatorTargetAim != null && 
            locatorTargetRest != null;

        if (tripleMode)
            transformsPresent = transformsPresent && jointPitchLow != null;

        return transformsPresent;
    }

    public void ResetToRest ()
    {
        
    }
    
    public void OnDrawGizmos (ItemVisualLegStepConfig stepConfig, Vector3 velocity = default)
    {
        
    }
}

[Serializable]
public class ItemPoseData
{
    public Transform root;
    public Transform parent;
    public AnimationClip clip;
    
    [ListDrawerSettings (DefaultExpandedState = false, ShowPaging = false)]
    public List<Transform> joints;
    
    [OnValueChanged ("RegisterPoses")]
    public List<ItemPose> poses;

    [Button, ButtonGroup, PropertyOrder (-1)]
    private void RegisterPoses ()
    {
        if (poses == null)
            return;

        foreach (var pose in poses)
        {
            if (pose == null)
                continue;

            pose.parent = this;
        }
    }
    
    [Button, ButtonGroup, PropertyOrder (-1)]
    private void FindJoints ()
    {
        if (parent == null)
            return;

        var children = parent.GetComponentsInChildren<Transform> (true);
        joints = new List<Transform> (children.Length);
        joints.AddRange (children);
    }
}

[Serializable]
public class ItemPose
{
    public string key;
    
    [ListDrawerSettings (DefaultExpandedState = false)]
    public ItemPoseTransform[] pose;

    [NonSerialized]
    public ItemPoseData parent;
    
    [Button, ButtonGroup, PropertyOrder (-1)]
    private void Save ()
    {
        if (parent == null || parent.joints == null || parent.joints.Count == 0)
        {
            Debug.LogWarning ($"Nothing to save - no parent or registered joints");
            return;
        }

        var count = parent.joints.Count;
        if (pose.Length != count)
            pose = new ItemPoseTransform[count];

        for (int i = 0; i < count; ++i)
        {
            var joint = parent.joints[i];
            if (joint == null)
            {
                Debug.LogWarning ($"Joint {i} is null, skipping saving from it");
                continue;
            }

            pose[i] = new ItemPoseTransform
            {
                position = joint.localPosition,
                rotation = joint.localRotation
            };
        }
    }

    [Button, ButtonGroup, PropertyOrder (-1)]
    private void Apply ()
    {
        if (parent == null || parent.joints == null || parent.joints.Count == 0)
        {
            Debug.LogWarning ($"Nothing to apply - no parent or registered joints");
            return;
        }

        var count = parent.joints.Count;
        if (pose.Length != count)
        {
            Debug.LogWarning ($"Pose {key} can't be applied - invalid collection size");
            return;
        }

        for (int i = 0; i < count; ++i)
        {
            var joint = parent.joints[i];
            if (joint == null)
            {
                Debug.LogWarning ($"Joint {i} is null, skipping applying to it");
                continue;
            }

            var p = pose[i];
            joint.localPosition = p.position;
            joint.localRotation = p.rotation;
        }
    }

    [Button, ButtonGroup, PropertyOrder (-1)]
    private void Write ()
    {
        if (parent == null || parent.joints == null || parent.joints.Count == 0 || parent.clip == null || parent.root == null)
        {
            Debug.LogWarning ($"Nothing to write - no parent or registered joints or clip");
            return;
        }

        var count = parent.joints.Count;
        if (pose.Length != count)
        {
            Debug.LogWarning ($"Pose {key} can't be applied - invalid collection size");
            return;
        }

        var root = parent.root;
        var c = parent.clip;
        var type = typeof (Transform);

        c.ClearCurves ();
        var pathList = new List<Transform> ();
        var sb = new StringBuilder ();
        
        var propertyNamePositionX = "localPosition.x";
        var propertyNamePositionY = "localPosition.y";
        var propertyNamePositionZ = "localPosition.z";
        
        var propertyNameRotationX = "localRotation.x";
        var propertyNameRotationY = "localRotation.y";
        var propertyNameRotationZ = "localRotation.z";
        var propertyNameRotationW = "localRotation.w";
        
        for (int i = 0; i < count; ++i)
        {
            var joint = parent.joints[i];
            if (joint == null)
            {
                Debug.LogWarning ($"Joint {i} is null, skipping applying to it");
                continue;
            }

            int a = 0;
            
            pathList.Clear ();
            sb.Clear ();
            
            Transform t = joint;
            while (a < 100)
            {
                pathList.Add (t);
                if (t == root)
                    break;

                t = t.parent;
                a += 1;
            }

            for (int x = pathList.Count - 1; x >= 0; --x)
            {
                t = pathList[x];
                sb.Append (t.name);
                if (x > 0)
                    sb.Append ("/");
            }
            
            var pos = joint.localPosition;
            var rot = joint.localRotation;
            
            var path = sb.ToString ();
            Debug.Log ($"{i} / {pathList.Count} size / {path}");
            
            var curvePositionX = new AnimationCurve (new Keyframe (0f, pos.x), new Keyframe (1f, pos.x));
            var curvePositionY = new AnimationCurve (new Keyframe (0f, pos.y), new Keyframe (1f, pos.y));
            var curvePositionZ = new AnimationCurve (new Keyframe (0f, pos.z), new Keyframe (1f, pos.z));
            
            c.SetCurve (path, type, propertyNamePositionX, curvePositionX);
            c.SetCurve (path, type, propertyNamePositionY, curvePositionY);
            c.SetCurve (path, type, propertyNamePositionZ, curvePositionZ);
            
            var curveRotationX = new AnimationCurve (new Keyframe (0f, rot.x), new Keyframe (1f, rot.x));
            var curveRotationY = new AnimationCurve (new Keyframe (0f, rot.y), new Keyframe (1f, rot.y));
            var curveRotationZ = new AnimationCurve (new Keyframe (0f, rot.z), new Keyframe (1f, rot.z));
            var curveRotationW = new AnimationCurve (new Keyframe (0f, rot.w), new Keyframe (1f, rot.w));
            
            c.SetCurve (path, type, propertyNameRotationX, curveRotationX);
            c.SetCurve (path, type, propertyNameRotationY, curveRotationY);
            c.SetCurve (path, type, propertyNameRotationZ, curveRotationZ);
            c.SetCurve (path, type, propertyNameRotationW, curveRotationW);
        }
    }
}

[Serializable]
public struct ItemPoseTransform
{
    [HorizontalGroup, HideLabel]
    public Vector3 position;
    
    [HorizontalGroup, HideLabel]
    public Quaternion rotation;
}

[Serializable]
public class ItemActivationLink
{
    public Transform visualTransform;
    public Transform recoilTransform;
    
    [ShowIf ("IsRecoilDataVisible")]
    public Vector3 recoilPositionFrom;
    
    [ShowIf ("IsRecoilDataVisible")]
    public Vector3 recoilPositionTo;

    private bool IsRecoilDataVisible => recoilTransform != null;
}

[Serializable]
public class ItemVisualJoint
{
    public string key;
    public Transform transform;
}

[Serializable]
public class ItemVisualHolderOverride
{
    [ValueDropdown ("@DataHelperUnitEquipment.GetSockets ()")]
    public string socket;

    [ValueDropdown ("@DataMultiLinkerSubsystemHardpoint.groups")]
    public string hardpointGroup;
    
    public List<Transform> visualHolders = new List<Transform> ();
}

public class ItemVisual : MonoBehaviour
{
    [NonSerialized, ShowInInspector]
    public Color colorPrimaryLast;
    
    [NonSerialized, ShowInInspector]
    public Color colorSecondaryLast;
    
    [NonSerialized, ShowInInspector]
    public Vector4 hsvOffsetPrimaryLast;
    
    [NonSerialized, ShowInInspector]
    public Vector4 hsvOffsetSecondaryLast;
    
    [NonSerialized, ShowInInspector]
    public Vector4 materialPrimaryLast;

    [NonSerialized, ShowInInspector]
    public Vector4 materialSecondaryLast;
    
    [PropertyOrder (-10)]
    public bool instantiateAtRoot = false;

    [PropertyOrder (-10)]
    public bool includesBase = false;
    
    [PropertyOrder (-9)]
    public bool customTransform = false;
    
    [PropertyOrder (-9)]
    public int shaderType = 0;

    [PropertyOrder (-9)]
    public bool shaderDestructionLimitLocked = false;
    
    [ShowIf ("customTransform"), PropertyOrder (-9)]
    public Vector3 customPosition = new Vector3 (-0.465f, 0f, 0f);
    
    [ShowIf ("customTransform"), PropertyOrder (-9)]
    public Vector3 customRotation = new Vector3 (90f, 0f, 0f);

    [PropertySpace]
    [Title("Holder overrides"), PropertyOrder (-7)]
    public Transform centerOfMass;
    
    [PropertyOrder (-7)]
    public List<ItemVisualHolderOverride> holderOverrides;
    
    [Title("Mesh Visuals (Renderers)"), PropertyOrder (-7)]
    public List<MeshRenderer> renderers;

    public Transform headlightOverrideTransform;

    public Transform podOverrideTransform;

    public List<ItemAttachmentLanding> attachmentsLanding;
    
    [PropertySpace]
    [Title ("Collider overrides")]
    public BoxCollider colliderImpactOverride;
    public List<Collider> colliderHitOverrides;
    
    

    [Button ("Collect renderers"), PropertyOrder (-7)]
    public void CollectRenderers (bool clearList = true)
    {
        if (renderers == null)
        {
            renderers = new List<MeshRenderer> ();
        }
        else
        {
            // When user clicks Collect Renderers button
            // it's useful to clear the list first
            if (clearList) renderers.Clear();
        }

        renderers.AddRange(this.GetComponentsInChildren<MeshRenderer> ());
    }
    
    [PropertySpace]
    [Title("Animation")]
    public Animator animator;
    
    [FormerlySerializedAs ("skinnedJoints")] 
    public List<ItemVisualJoint> joints;
    
    public List<ItemVisualLegGroup> legGroups = null;

    [SerializeReference]
    public ItemVisualCycle cycle = null;

    [PropertySpace]
    [Title("Mesh Visuals (General)")]
    public List<UnitRendererSocketMapped> renderersSocketMapped;
    public List<UnitRendererOutline> renderersOutlined;
    
    public List<GameObject> effectHolders;
    public List<ParticleSystem> effectSystems;

    [PropertySpace]
    [Title("FX Activation (muzzle flashes, recoil, blade trails, etc.)")]
    public Transform fxTransform;

    [InlineButtonClear]
    [ValueDropdown("@DataMultiLinkerAssetPools.data.Keys")]
    public string fxDestructionOverride;

    public bool activationUsed = false;
    
    [NonSerialized, ShowInInspector, HideInEditorMode]
    public int activationIndex;
    
    [ShowIf ("activationUsed")]
    public List<ItemActivationLink> activationLinks;
    
    [NonSerialized]
    [ShowInInspector][HideInEditorMode]
    public Dictionary<string, Transform> jointsLookup = new Dictionary<string, Transform> ();
    
    

    private static MaterialPropertyBlock propertyBlock;
    private static int propertyIDVisibility;
    private float visibilityValue = 0f;
    private bool initialized = false;

    [Button ("Convert to activation link")]
    private void ConvertToActivationLink ()
    {
        if (fxTransform == null)
            return;

        if (activationUsed && activationLinks.Count > 0)
        {
            Debug.LogWarning ($"Skipped {gameObject.name} as it already has activation links");
            return;
        }

        activationUsed = true;
        AddActivationLink(fxTransform);
    }

    public void AddActivationLink (Transform fxTransform)
    {
        if (activationLinks == null)
            activationLinks = new List<ItemActivationLink> ();

        var link = new ItemActivationLink ();
        activationLinks.Add (link);

        link.visualTransform = fxTransform;
    }

    public void CheckInitialization ()
    {
        if (initialized)
            return;

        initialized = true;
        jointsLookup.Clear ();

        if (joints != null)
        {
            foreach (var jointInfo in joints)
            {
                if (jointInfo != null && jointInfo.transform != null && !string.IsNullOrEmpty (jointInfo.key) && !jointsLookup.ContainsKey (jointInfo.key))
                    jointsLookup.Add (jointInfo.key, jointInfo.transform);
            }
        }
    }
    
    private void StopAnimation ()
    {
        
    }

    [ContextMenu ("Set visible")]
    public void SetVisibleTrue () { SetVisible (true); }

    [ContextMenu ("Set hidden")]
    public void SetVisibleFalse () { SetVisible (false); }

    public void SetVisibleInstant (bool visible)
    {
        StopAnimation ();
        AnimateVisibility (visible ? 1f : 0f);
        visibilityValue = visible ? 1f : 0f;
    }

    public void SetVisible (bool visible)
    {
        if (propertyBlock == null)
        {
            propertyBlock = new MaterialPropertyBlock ();
            propertyIDVisibility = Shader.PropertyToID ("_Visibility");
        }
        
        StopAnimation ();

        if (visible)
        {
            for (int i = 0; i < renderers.Count; ++i)
            {
                MeshRenderer mr = renderers[i];
                mr.enabled = true;
            }
        }
    }

    private void AnimateVisibility (float value)
    {
        visibilityValue = value;
        propertyBlock.SetFloat (propertyIDVisibility, value);
        for (int i = 0; i < renderers.Count; ++i)
        {
            MeshRenderer mr = renderers[i];
            mr.SetPropertyBlock (propertyBlock);
        }
    }

    private void CompleteVisibility ()
    {
        for (int i = 0; i < renderers.Count; ++i)
        {
            MeshRenderer mr = renderers[i];
            if (visibilityValue < 0.1f)
                mr.enabled = false;
        }
    }
    
    [ShowIf ("customTransform"), PropertyOrder (-1)]
    [ButtonGroup ("A"), Button ("Zero")]
    public void SetDefaultZero ()
    {
        customPosition = new Vector3 (0f, 0f, 0f);
        customRotation = new Vector3 (0f, 0f, 0f);
    }

    [ShowIf ("customTransform"), PropertyOrder (-1)]
    [ButtonGroup ("A"), Button ("Shoulder")]
    public void SetDefaultShoulder ()
    {
        customPosition = new Vector3 (-0.465f, 0f, 0f);
        customRotation = new Vector3 (90f, 0f, 0f);
    }

    [ShowIf ("customTransform"), PropertyOrder (-1)]
    [ButtonGroup ("A"), Button ("Forearm")]
    public void SetDefaultForearm ()
    {
        customPosition = new Vector3 (0f, 0f, 0f);
        customRotation = new Vector3 (90f, 0f, 0f);
    }

    [ShowIf ("customTransform"), PropertyOrder (-1)]
    [ButtonGroup ("A"), Button ("Thigh")]
    public void SetDefaultThigh ()
    {
        customPosition = new Vector3 (-0.465f, 0f, 0f);
        customRotation = new Vector3 (0f, 0f, 0f);
    }

    [ShowIf ("customTransform"), PropertyOrder (-1)]
    [ButtonGroup ("A"), Button ("Foot")]
    public void SetDefaultFoot ()
    {
        customPosition = new Vector3 (0f, -0.415f, 0f);
        customRotation = new Vector3 (0f, 0f, 0f);
    }
    
    private static MaterialPropertyBlock mpb;

    private static int propertyID_ArrayOverrideMode = Shader.PropertyToID ("_ArrayOverrideMode");
    private static int propertyID_ArrayOverrideIndex = Shader.PropertyToID ("_ArrayOverrideIndex");
    private static int propertyID_ArrayForColorPrimary = Shader.PropertyToID ("_ArrayForColorPrimary");
    private static int propertyID_ArrayForColorSecondary = Shader.PropertyToID ("_ArrayForColorSecondary");
    private static int propertyID_ArrayForColorTertiary = Shader.PropertyToID ("_ArrayForColorTertiary");
    private static int propertyID_ArrayForSmoothnessPrimary = Shader.PropertyToID ("_ArrayForSmoothnessPrimary");
    private static int propertyID_ArrayForSmoothnessSecondary = Shader.PropertyToID ("_ArrayForSmoothnessSecondary");
    private static int propertyID_ArrayForSmoothnessTertiary = Shader.PropertyToID ("_ArrayForSmoothnessTertiary");
    private static int propertyID_ArrayForMetalness = Shader.PropertyToID ("_ArrayForMetalness");
    private static int propertyID_ArrayForEffect = Shader.PropertyToID ("_ArrayForEffect");
    private static int propertyID_ArrayForDamage = Shader.PropertyToID ("_ArrayForDamage");
    private static int propertyID_PixelOverlayIntensity = Shader.PropertyToID("_PixelOverlayIntensity");
    
    private const string ppt = "propertyPreview";
    private const string prf = "RefreshAndApplyPropertyBlock";
    
    [Title ("Shaders")]
    [ShowInInspector, ToggleGroup (ppt, ToggleGroupTitle = "Shader Property Preview"), LabelText ("Preview")]
    [OnValueChanged (prf)]
    private bool propertyPreview = false;
    
    [ShowInInspector, ToggleGroup (ppt), LabelText ("Color 1")]
    [OnValueChanged (prf)]
    private Color propertyValue_ColorPrimary = new Color (0.7f, 0.7f, 0.7f);
    
    [ShowInInspector, ToggleGroup (ppt), LabelText ("Color 2")]
    [OnValueChanged (prf)]
    private Color propertyValue_ColorSecondary = new Color (0.75f, 0.35f, 0.25f);
    
    [ShowInInspector, ToggleGroup (ppt), LabelText ("Color 3")]
    [OnValueChanged (prf)]
    private Color propertyValue_ColorTertiary = new Color (0.3f, 0.3f, 0.3f);
    
    [ShowInInspector, ToggleGroup (ppt), LabelText ("Smoothness 1")]
    [OnValueChanged (prf)]
    private Vector4 propertyValue_SmoothnessPrimary = new Vector4 (0f, 0.4f, 0.8f, 0f);
    
    [ShowInInspector, ToggleGroup (ppt), LabelText ("Smoothness 2")]
    [OnValueChanged (prf)]
    private Vector4 propertyValue_SmoothnessSecondary = new Vector4 (0f, 0.4f, 0.8f, 0f);
    
    [ShowInInspector, ToggleGroup (ppt), LabelText ("Smoothness 3")]
    [OnValueChanged (prf)]
    private Vector4 propertyValue_SmoothnessTertiary = new Vector4 (0f, 0.4f, 0.8f, 0f);
    
    [ShowInInspector, ToggleGroup (ppt), LabelText ("Metalness 1/2/3")]
    [OnValueChanged (prf)]
    private Vector4 propertyValue_Metalness = new Vector4 (0f, 0f, 0f, 0f);
    
    [ShowInInspector, ToggleGroup (ppt), LabelText ("Effects")]
    [OnValueChanged (prf)]
    private Vector4 propertyValue_Effect = new Vector4 (0f, 0f, 0f, 0f);
    
    [ShowInInspector, ToggleGroup (ppt), LabelText ("Damage")]
    [OnValueChanged (prf)]
    private Vector4 propertyValue_Damage = new Vector4 (0f, 0f, 0f, 1f);
    
    private List<Vector4> propertyValue_ArrayForColorPrimary;
    private List<Vector4> propertyValue_ArrayForColorSecondary;
    private List<Vector4> propertyValue_ArrayForColorTertiary;
    private List<Vector4> propertyValue_ArrayForSmoothnessPrimary;
    private List<Vector4> propertyValue_ArrayForSmoothnessSecondary;
    private List<Vector4> propertyValue_ArrayForSmoothnessTertiary;
    private List<Vector4> propertyValue_ArrayForMetalness;
    private List<Vector4> propertyValue_ArrayForEffect;
    private List<Vector4> propertyValue_ArrayForDamage;
    private bool propertyValue_ArraysInitialized = false;

    [ToggleGroup (ppt)]
    [Button ("Apply MPB")]
    public void ForceMaterialUpdate ()
    {
        propertyPreview = true;
        RefreshAndApplyPropertyBlock ();
    }
    
    private void RefreshAndApplyPropertyBlock ()
    {
        if (mpb == null)
            mpb = new MaterialPropertyBlock ();
        
        mpb.Clear ();
        
        if (propertyPreview)
        {
            if (shaderType == 0)
            {
                mpb.SetFloat (propertyID_ArrayOverrideMode, 1f);
                mpb.SetFloat (propertyID_ArrayOverrideIndex, 0f);

                if (!propertyValue_ArraysInitialized)
                {
                    propertyValue_ArraysInitialized = true;
                    int propertyCount = UnitHelper.locationShaderArraySize;

                    propertyValue_ArrayForColorPrimary = new List<Vector4> (new Vector4[propertyCount]);
                    propertyValue_ArrayForColorSecondary = new List<Vector4> (new Vector4[propertyCount]);
                    propertyValue_ArrayForColorTertiary = new List<Vector4> (new Vector4[propertyCount]);
                    propertyValue_ArrayForSmoothnessPrimary = new List<Vector4> (new Vector4[propertyCount]);
                    propertyValue_ArrayForSmoothnessSecondary = new List<Vector4> (new Vector4[propertyCount]);
                    propertyValue_ArrayForSmoothnessTertiary = new List<Vector4> (new Vector4[propertyCount]);
                    propertyValue_ArrayForMetalness = new List<Vector4> (new Vector4[propertyCount]);
                    propertyValue_ArrayForEffect = new List<Vector4> (new Vector4[propertyCount]);
                    propertyValue_ArrayForDamage = new List<Vector4> (new Vector4[propertyCount]);
                }

                propertyValue_ArrayForColorPrimary[0] = propertyValue_ColorPrimary;
                propertyValue_ArrayForColorSecondary[0] = propertyValue_ColorSecondary;
                propertyValue_ArrayForColorTertiary[0] = propertyValue_ColorTertiary;
                propertyValue_ArrayForSmoothnessPrimary[0] = propertyValue_SmoothnessPrimary;
                propertyValue_ArrayForSmoothnessSecondary[0] = propertyValue_SmoothnessSecondary;
                propertyValue_ArrayForSmoothnessTertiary[0] = propertyValue_SmoothnessTertiary;
                propertyValue_ArrayForMetalness[0] = propertyValue_Metalness;
                propertyValue_ArrayForEffect[0] = propertyValue_Effect;
                propertyValue_ArrayForDamage[0] = propertyValue_Damage;

                mpb.SetVectorArray (propertyID_ArrayForColorPrimary, propertyValue_ArrayForColorPrimary);
                mpb.SetVectorArray (propertyID_ArrayForColorSecondary, propertyValue_ArrayForColorSecondary);
                mpb.SetVectorArray (propertyID_ArrayForColorTertiary, propertyValue_ArrayForColorTertiary);
                mpb.SetVectorArray (propertyID_ArrayForSmoothnessPrimary, propertyValue_ArrayForSmoothnessPrimary);
                mpb.SetVectorArray (propertyID_ArrayForSmoothnessSecondary, propertyValue_ArrayForSmoothnessSecondary);
                mpb.SetVectorArray (propertyID_ArrayForSmoothnessTertiary, propertyValue_ArrayForSmoothnessTertiary);
                mpb.SetVectorArray (propertyID_ArrayForMetalness, propertyValue_ArrayForMetalness);
                mpb.SetVectorArray (propertyID_ArrayForEffect, propertyValue_ArrayForEffect);
                mpb.SetVectorArray (propertyID_ArrayForDamage, propertyValue_ArrayForDamage);

                mpb.SetFloat (propertyID_PixelOverlayIntensity, 0f);
            }
            else
            {
                // ...
            }
        }

        foreach (var mr in renderers)
        {
            if (mr != null)
                mr.SetPropertyBlock (mpb);
        }
    }
    
    public Bounds GetRendererBounds ()
    {
        var bounds = new Bounds (Vector3.zero, Vector3.zero);
        if (renderers == null || renderers.Count == 0)
            return bounds;

        bool firstBoundsFound = false;
        for (int i = 0; i < renderers.Count; ++i)
        {
            var mr = renderers[i];
            if (mr == null)
                continue;
            
            var mf = mr.GetComponent<MeshFilter> ();
            var mesh = mf.sharedMesh;
            if (mesh == null)
                continue;

            if (firstBoundsFound)
                bounds.Encapsulate (mesh.bounds);
            else
            {
                bounds.extents = mesh.bounds.extents;
                bounds.center = mesh.bounds.center;
                firstBoundsFound = true;
            }
        }

        return bounds;
    }

    [Button ("Center using renderer bounds")]
    public void CenterUsingBounds ()
    {
        var bounds = GetRendererBounds ();
        
        var t = transform;
        Debug.Log ($"Centering {gameObject.name} | Bounds: {bounds}");
        t.localPosition = t.localRotation * -bounds.center;
    }

    [Button ("Scale using renderer bounds")]
    public void ScaleUsingBounds ()
    {
        var t = transform;
        t.localScale = Vector3.one;
        
        var bounds = GetRendererBounds ();
        
        var extentMax = Mathf.Max (Mathf.Max (bounds.extents.x, bounds.extents.y), bounds.extents.z);
        if (extentMax <= 0f)
            return;
        
        var scale = 1f / extentMax * 2f;
        Debug.Log ($"Scaling & centering {gameObject.name} | Bounds: {bounds} | Scale: {scale}");
        // DebugExtensions.DrawCube (transform.TransformPoint (bounds.center), bounds.center, Color.white, 1f);
        t.localScale = Vector3.one * scale;
        t.localPosition = t.localRotation * -bounds.center * scale;
    }
    
    public void OnDrawGizmosSelected ()
    {
        if (legGroups != null && legGroups.Count > 0)
        {
            for (int i = 0, iLimit = legGroups.Count; i < iLimit; ++i)
            {
                var group = legGroups[i];
                if (group.step == null || group.legs == null || group.legs.Count == 0)
                    continue;
                
                for (int l = 0, lLimit = group.legs.Count; l < lLimit; ++l)
                {
                    var leg = group.legs[l];
                    leg.OnDrawGizmos (group.step);
                }
            }
        }
    }
}
