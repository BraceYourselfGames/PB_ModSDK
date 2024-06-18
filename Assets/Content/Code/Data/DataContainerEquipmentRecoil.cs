using System;
using Sirenix.OdinInspector;
using UnityEngine;
using YamlDotNet.Serialization;

namespace PhantomBrigade.Data
{
    public class RecoilOffsetDataBlock
    {
        [HorizontalGroup ("recoil")]
        [BoxGroup ("recoil/Position offset")] [LabelText ("X")] [LabelWidth (20)] [YamlIgnore] [ShowInInspector] [HideReferenceObjectPicker]
        public AnimationCurve recoilOffsetPosX;

        [YamlMember (Alias = "recoilOffsetPosX")] [HideInInspector]
        public AnimationCurveSerialized recoilOffsetPosXSerialized;

        [BoxGroup ("recoil/Position offset")] [LabelText ("Y")] [LabelWidth (20)] [YamlIgnore] [ShowInInspector] [HideReferenceObjectPicker]
        public AnimationCurve recoilOffsetPosY;

        [YamlMember (Alias = "recoilOffsetPosY")] [HideInInspector]
        public AnimationCurveSerialized recoilOffsetPosYSerialized;

        [BoxGroup ("recoil/Position offset")] [LabelText ("Z")] [LabelWidth (20)] [YamlIgnore] [ShowInInspector] [HideReferenceObjectPicker]
        public AnimationCurve recoilOffsetPosZ;

        [YamlMember (Alias = "recoilOffsetPosZ")] [HideInInspector]
        public AnimationCurveSerialized recoilOffsetPosZSerialized;

        [BoxGroup ("recoil/Rotation offset")] [LabelText ("X")] [LabelWidth (20)] [YamlIgnore] [ShowInInspector] [HideReferenceObjectPicker]
        public AnimationCurve recoilOffsetRotX;

        [YamlMember (Alias = "recoilOffsetRotX")] [HideInInspector]
        public AnimationCurveSerialized recoilOffsetRotXSerialized;

        [BoxGroup ("recoil/Rotation offset")] [LabelText ("Y")] [LabelWidth (20)] [YamlIgnore] [ShowInInspector] [HideReferenceObjectPicker]
        public AnimationCurve recoilOffsetRotY;

        [YamlMember (Alias = "recoilOffsetRotY")] [HideInInspector]
        public AnimationCurveSerialized recoilOffsetRotYSerialized;

        [BoxGroup ("recoil/Rotation offset")] [LabelText ("Z")] [LabelWidth (20)] [YamlIgnore] [ShowInInspector] [HideReferenceObjectPicker]
        public AnimationCurve recoilOffsetRotZ;

        [YamlMember (Alias = "recoilOffsetRotZ")] [HideInInspector]
        public AnimationCurveSerialized recoilOffsetRotZSerialized;
    }

    [Serializable]
    public class DataContainerEquipmentRecoil : DataContainer
    {
        public RecoilOffsetDataBlock perShotRecoilOffset = new RecoilOffsetDataBlock ();
        public RecoilOffsetDataBlock perActionRecoilOffset = new RecoilOffsetDataBlock ();

        [Header ("Recoil Data")]
        public Vector3 rotationRandom = new Vector3(0.0f, 0.0f, -4.0f);
        public Vector3 handRotationOffset = new Vector3 (0.0f, 0.0f, -3.0f);
        public float blendTime = 0.1f;
        public float shotDuration = 0.1f;
        public Vector3 recoilOffset = new Vector3 (0.0f, 0.02f, -0.1f);
        public float additivity = 1.0f;
        public float recoilMag = 1.0f;
        public bool isBeamWeapon = false;
        public float peakValue = 0.1f;
        public float additiveOffsetGoal = 5.0f;

        public override void OnBeforeSerialization ()
        {
            
            base.OnBeforeSerialization ();
            perShotRecoilOffset.recoilOffsetPosXSerialized = (AnimationCurveSerialized) perShotRecoilOffset.recoilOffsetPosX;
            perShotRecoilOffset.recoilOffsetPosYSerialized = (AnimationCurveSerialized) perShotRecoilOffset.recoilOffsetPosY;
            perShotRecoilOffset.recoilOffsetPosZSerialized = (AnimationCurveSerialized) perShotRecoilOffset.recoilOffsetPosZ;
            perShotRecoilOffset.recoilOffsetRotXSerialized = (AnimationCurveSerialized) perShotRecoilOffset.recoilOffsetRotX;
            perShotRecoilOffset.recoilOffsetRotYSerialized = (AnimationCurveSerialized) perShotRecoilOffset.recoilOffsetRotY;
            perShotRecoilOffset.recoilOffsetRotZSerialized = (AnimationCurveSerialized) perShotRecoilOffset.recoilOffsetRotZ;

            perActionRecoilOffset.recoilOffsetPosXSerialized = (AnimationCurveSerialized) perActionRecoilOffset.recoilOffsetPosX;
            perActionRecoilOffset.recoilOffsetPosYSerialized = (AnimationCurveSerialized) perActionRecoilOffset.recoilOffsetPosY;
            perActionRecoilOffset.recoilOffsetPosZSerialized = (AnimationCurveSerialized) perActionRecoilOffset.recoilOffsetPosZ;
            perActionRecoilOffset.recoilOffsetRotXSerialized = (AnimationCurveSerialized) perActionRecoilOffset.recoilOffsetRotX;
            perActionRecoilOffset.recoilOffsetRotYSerialized = (AnimationCurveSerialized) perActionRecoilOffset.recoilOffsetRotY;
            perActionRecoilOffset.recoilOffsetRotZSerialized = (AnimationCurveSerialized) perActionRecoilOffset.recoilOffsetRotZ;
        }

        public override void OnAfterDeserialization (string key)
        {
            base.OnAfterDeserialization (key);
            perShotRecoilOffset.recoilOffsetPosX = (AnimationCurve) perShotRecoilOffset.recoilOffsetPosXSerialized;
            perShotRecoilOffset.recoilOffsetPosY = (AnimationCurve) perShotRecoilOffset.recoilOffsetPosYSerialized;
            perShotRecoilOffset.recoilOffsetPosZ = (AnimationCurve) perShotRecoilOffset.recoilOffsetPosZSerialized;
            perShotRecoilOffset.recoilOffsetRotX = (AnimationCurve) perShotRecoilOffset.recoilOffsetRotXSerialized;
            perShotRecoilOffset.recoilOffsetRotY = (AnimationCurve) perShotRecoilOffset.recoilOffsetRotYSerialized;
            perShotRecoilOffset.recoilOffsetRotZ = (AnimationCurve) perShotRecoilOffset.recoilOffsetRotZSerialized;

            perActionRecoilOffset.recoilOffsetPosX = (AnimationCurve) perActionRecoilOffset.recoilOffsetPosXSerialized;
            perActionRecoilOffset.recoilOffsetPosY = (AnimationCurve) perActionRecoilOffset.recoilOffsetPosYSerialized;
            perActionRecoilOffset.recoilOffsetPosZ = (AnimationCurve) perActionRecoilOffset.recoilOffsetPosZSerialized;
            perActionRecoilOffset.recoilOffsetRotX = (AnimationCurve) perActionRecoilOffset.recoilOffsetRotXSerialized;
            perActionRecoilOffset.recoilOffsetRotY = (AnimationCurve) perActionRecoilOffset.recoilOffsetRotYSerialized;
            perActionRecoilOffset.recoilOffsetRotZ = (AnimationCurve) perActionRecoilOffset.recoilOffsetRotZSerialized;
        }
    }
}