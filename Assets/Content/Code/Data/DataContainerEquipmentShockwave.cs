using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using YamlDotNet.Serialization;

namespace PhantomBrigade.Data
{
    [HideReferenceObjectPicker]
    public class DataBlockShockwavePoint
    {
        [GUIColor ("GetColor")]
        [MinMaxSlider (0f, 1f, true)]
        public Vector2 lifetime = new Vector2 (0f, 1f);

        [ShowInInspector, DisplayAsString, ReadOnly, PropertyOrder (-2)]
        private string lifetimeScaled => $"{(lifetime.x * 1.3333f):0.###} - {(lifetime.y * 1.3333f):0.###} s";

        [ShowInInspector, DisplayAsString, ReadOnly, PropertyOrder (-2)]
        private string progress
        {
            get
            {
                var trc = DataShortcuts.anim.timeRemapMeleeStandard;
                var progressFrom = trc.Evaluate (lifetime.x) * 100f;
                var progressTo = trc.Evaluate (lifetime.x) * 100f;
                return $"{progressFrom:0.#} - {progressTo:0.#} %";
            }
        }

        public Vector3 positionFrom = new Vector3 (3f, 0f, 0f);
        public float velocityRadial;
        public float velocityForward;
        
         #if UNITY_EDITOR
        
        public static DataBlockShockwavePoint selection;
        
        [ButtonGroup, Button ("-15°"), PropertyOrder (-1)]
        private void RotateNeg15 () => RotateAroundOrigin (-15f);
        
        [ButtonGroup, Button ("-5°"), PropertyOrder (-1)]
        private void RotateNeg5 () => RotateAroundOrigin (-5f);

        [ButtonGroup, Button ("-1°"), PropertyOrder (-1)]
        private void RotateNeg1 () => RotateAroundOrigin (-1f);
        
        [ButtonGroup, Button ("+1°"), PropertyOrder (-1)]
        private void RotatePos1 () => RotateAroundOrigin (1f);
        
        [ButtonGroup, Button ("+5°"), PropertyOrder (-1)]
        private void RotatePos5 () => RotateAroundOrigin (5f);
        
        [ButtonGroup, Button ("+15°"), PropertyOrder (-1)]
        private void RotatePos15 () => RotateAroundOrigin (15f);
        
        private void RotateAroundOrigin (float angle)
        {
            var rotation = Quaternion.Euler (0f, angle, 0f);
            positionFrom = rotation * positionFrom;
            // positionTo = rotation * positionTo;
        }
        
        /*
        [ButtonGroup, Button ("Align"), PropertyOrder (-1)]
        private void Align ()
        {
            var distance = positionFrom == positionTo ? 1f : (positionTo - positionFrom).magnitude;
            var direction = positionFrom.normalized;
            positionTo = positionFrom + direction * distance;
        }
        */
        
        [ButtonGroup, Button ("@selection == this ? \"Deselect\" : \"Select\""), PropertyOrder (-1)]
        private void Select ()
        {
            if (selection != this)
                selection = this;
            else
                selection = null;
        }

        private static Color colorSelected = Color.HSVToRGB (0.55f, 0.2f, 1f).WithAlpha (1f);
        private static Color colorIdle = Color.white.WithAlpha (1f);
        private Color GetColor => selection == this ? colorSelected : colorIdle;

        #endif
    }
    
    [Serializable]
    public class DataContainerEquipmentShockwave : DataContainer
    {
        [FoldoutGroup ("Hitbox", false), LabelText ("Depth")]
        public float hitboxDepth = 6f;
        
        [FoldoutGroup ("Hitbox"), LabelText ("Collision Scale")]
        [PropertyRange (0f, 1f)]
        public float hitboxCollisionScale = 1f;
        
        [FoldoutGroup ("Hitbox"), LabelText ("Rounding")]
        [PropertyRange (0f, 1f)]
        public float hitboxRounding = 0.25f;
        
        [FoldoutGroup ("Hitbox"), LabelText ("Scaling Time")]
        [PropertyRange (0f, 1f)]
        public float hitboxScaleTime = 0.01f;
        
        [FoldoutGroup ("Hitbox"), LabelText ("End Fade Shift")]
        [PropertyRange (0f, 1f)]
        public float hitboxFadeoutTime = 0.05f;
        
        [FoldoutGroup ("Hitbox"), LabelText ("Extend Right")]
        public bool extendRight = true;
        
        [FoldoutGroup ("Hitbox"), LabelText ("Visual Preset")]
        public string visualPresetKey = "dir_01";

        [HorizontalGroup ("Forward/A")]
        [FoldoutGroup ("Forward", false), LabelText ("Velocity")]
        public float velocityForward = 0f;
        
        [HorizontalGroup ("Forward/A")]
        [FoldoutGroup ("Forward"), LabelText ("Damping")]
        public float dampingForward = 0f;
        
        [HorizontalGroup ("Forward/B")]
        [FoldoutGroup ("Forward"), LabelText ("Reach"), SuffixLabel ("m"), ShowInInspector]
        private float reachForward => UtilityMath.GetDampedTravelDistance (velocityForward, dampingForward, 1f);
        
        [HorizontalGroup ("Forward/B")]
        [FoldoutGroup ("Forward"), LabelText ("Stop Time"), SuffixLabel ("%"), ShowInInspector]
        private float stopForward => UtilityMath.GetDampedStopTime (velocityForward, dampingForward) * 100;

        [HorizontalGroup ("Radial/A")]
        [FoldoutGroup ("Radial", false), LabelText ("Velocity")]
        public float velocityRadial = 0f;
        
        [HorizontalGroup ("Radial/A")]
        [FoldoutGroup ("Radial"), LabelText ("Damping")]
        public float dampingRadial = 0f;

        [HorizontalGroup ("Radial/B")]
        [FoldoutGroup ("Radial"), LabelText ("Reach"), SuffixLabel ("m"), ShowInInspector]
        private float reachRadial => UtilityMath.GetDampedTravelDistance (velocityRadial, dampingRadial, 1f);
        
        [HorizontalGroup ("Radial/B")]
        [FoldoutGroup ("Radial"), LabelText ("Stop Time"), SuffixLabel ("%"), ShowInInspector]
        private float stopRadial => UtilityMath.GetDampedStopTime (velocityRadial, dampingRadial) * 100;
        
        [PropertyOrder (1)]
        [ListDrawerSettings (DefaultExpandedState = false, CustomAddFunction = "AddPoint")]
        public List<DataBlockShockwavePoint> points;
        
        #if UNITY_EDITOR
        
        [YamlIgnore]
        [FoldoutGroup ("Time", false), PropertyOrder (-1), PropertyRange (0f, 1f)]
        [ShowIf ("IsSelectionActive")]
        public float time;

        [YamlIgnore]
        [FoldoutGroup ("Time"), PropertyOrder (-1)]
        [ShowIf ("IsSelectionActive")]
        public float unitPositionTarget = 30f;
        
        [YamlIgnore]
        [FoldoutGroup ("Time"), PropertyOrder (-1)]
        public bool unitTimeRemap = true;
        
        public static DataContainerEquipmentShockwave selection;
        
        [PropertyOrder (-2), Button ("@selection == this ? \"Deselect\" : \"Select\"")]
        private void Select ()
        {
            if (selection != this)
                selection = this;
            else
                selection = null;
        }

        private void AddPoint ()
        {
            if (points.Count == 0)
                points.Add (new DataBlockShockwavePoint ());
            else
            {
                var last = points[points.Count - 1];
                points.Add (new DataBlockShockwavePoint
                {
                    lifetime = last.lifetime,
                    positionFrom = last.positionFrom,
                    velocityForward = last.velocityForward,
                    velocityRadial = last.velocityRadial
                });
            }
        }

        private bool IsSelectionActive => selection == this;

        [Button ("Mirror X"), FoldoutGroup ("Utilities", false)]
        private void MirrorX ()
        {
            foreach (var p in points)
                p.positionFrom = new Vector3 (-p.positionFrom.x, p.positionFrom.y, p.positionFrom.z);
        }

        [Button ("Linearize start time"), FoldoutGroup ("Utilities")]
        private void LinearizeStartTime ()
        {
            Linearize (true);
        }
        
        [Button ("Linearize end time"), FoldoutGroup ("Utilities")]
        private void LinearizeEndTime ()
        {
            Linearize (false);
        }

        private void Linearize (bool start)
        {
            var from = points[0].lifetime;
            var to = points[points.Count - 1].lifetime;
            
            for (int i = 0, countMinusOne = points.Count - 1; i <= countMinusOne; ++i)
            {
                var p = points[i];
                var factor = (float)i / countMinusOne;
                if (start)
                    p.lifetime = new Vector2 (Mathf.Lerp (from.x, to.x, factor), p.lifetime.y);
                else
                    p.lifetime = new Vector2 (p.lifetime.x, Mathf.Lerp (from.y, to.y, factor));
            }
        }

        [Button ("Offset time"), FoldoutGroup ("Utilities")]
        private void OffsetTime (Vector2 offset)
        {
            for (int i = 0, countMinusOne = points.Count - 1; i <= countMinusOne; ++i)
            {
                var p = points[i];
                p.lifetime += offset;
            }
        }
        
        [Button ("Offset position"), FoldoutGroup ("Utilities")]
        private void Offset (Vector3 offset)
        {
            foreach (var p in points)
                p.positionFrom += offset;
        }
        
        [Button ("Offset position radially"), FoldoutGroup ("Utilities")]
        private void OffsetRadial (float offset)
        {
            foreach (var p in points)
            {
                var dir = p.positionFrom.normalized;
                p.positionFrom += dir * offset;
            }
        }

        #endif
        
        public override void OnBeforeSerialization ()
        {

        }
        
        public override void OnAfterDeserialization (string key)
        {
            base.OnAfterDeserialization (key);
        }
    }
}

