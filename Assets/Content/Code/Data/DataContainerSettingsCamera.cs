using System;
using System.Collections.Generic;
using System.Text;
using Sirenix.OdinInspector;
using UnityEngine;
using YamlDotNet.Serialization;

namespace PhantomBrigade.Data
{
    [Serializable]
    public class DataBlockUnitCameraState
    {
        [LabelText ("Loc. primary"), PropertyOrder (-2)]
        public string presetNamePrimary = "";
        
        [LabelText ("Loc. secondary"), PropertyOrder (-2)]
        public string presetNameSecondary = "";
        
        [LabelText ("Loc. tertiary"), PropertyOrder (-2)]
        public string presetNameTertiary = "";

        [LabelText ("Loc. preview"), PropertyOrder (-1), ReadOnly, ShowInInspector, YamlIgnore]
        public string presetLocText => GetLocalizedText (false);
        
        private static StringBuilder sb = new StringBuilder ();

        public string GetLocalizedText (bool multiline)
        {
            var primaryText = !string.IsNullOrEmpty (presetNamePrimary) ? Txt.Get (TextLibs.uiCombat, "cam_preset_" + presetNamePrimary) : null;
            bool primaryUsed = !string.IsNullOrEmpty (primaryText);
                
            var secondaryText = !string.IsNullOrEmpty (presetNameSecondary) ? Txt.Get (TextLibs.uiCombat, "cam_preset_" + presetNameSecondary) : null;
            bool secondaryUsed = !string.IsNullOrEmpty (secondaryText);
                
            var tertiaryText = !string.IsNullOrEmpty (presetNameTertiary) ? Txt.Get (TextLibs.uiCombat, "cam_preset_" + presetNameTertiary) : null;
            bool tertiaryUsed = !string.IsNullOrEmpty (tertiaryText);

            sb.Clear ();
                
            if (primaryUsed)
                sb.Append (primaryText);
                
            if (secondaryUsed)
            {
                if (sb.Length > 0)
                    sb.Append (" / ");
                sb.Append (secondaryText);
            }
                
            if (tertiaryUsed)
            {
                if (multiline)
                {
                    if (sb.Length > 0)
                        sb.Append ("\n");
                    sb.Append ("[aa]");
                    sb.Append (tertiaryText);
                    sb.Append ("[ff]");
                }
                else
                {
                    if (sb.Length > 0)
                        sb.Append (" / ");
                    sb.Append (tertiaryText);
                }
            }
                
            return sb.ToString ();
        }
        
        public enum FollowMode
        {
            Locked,
            Orbital
        }

        public enum TargetMode
        {
            Body,
            WeaponLeft,
            WeaponRight,
            ShoulderLeft,
            ShoulderRight
        }

        public FollowMode followMode = FollowMode.Locked;
        public TargetMode targetMode = TargetMode.Body;
        public Vector3 offset = new Vector3 (0.0f, 1.5f, -7.0f);
        public float fov = 45f;
        public float dampingPosition = 0.0f;
        public float dampingRotation = 0.5f;
        
        
    }

    [Serializable]
    public class DataContainerSettingsCamera : DataContainerUnique
    {
        public float cameraTelemetrySmoothing = 7.5f;
        public float heightMultiplierForListenerTakeoff = 0.02f;

        public bool disableCombatCameraHeightSensor = false;

        [Header ("Actual Shadow Distances")]
        public int shadowDistanceOverworld = 250;
        public int shadowDistanceCombat = 250;
        public int shadowDistanceBase = 50;
        public SortedDictionary<string, int> shadowDistanceBasePerCamera = new SortedDictionary<string, int> ();

        [Header ("Frustum Shadow Distance Offsets (from actual shadows)")]
        public int shadowOffsetOverworld = -60;
        public int shadowOffsetCombat = -60;
        public int shadowOffsetBase = -10;

        [Header ("Unit-Mounted Camera Presets")]
        public List<DataBlockUnitCameraState> unitCameraPresets = new List<DataBlockUnitCameraState> ();
    }
}
