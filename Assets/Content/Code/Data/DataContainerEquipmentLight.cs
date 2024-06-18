using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PhantomBrigade.Data
{
    [Serializable][HideReferenceObjectPicker][LabelWidth (120f)]
    public class DataContainerEquipmentLight : DataContainer
    {
        public bool colorByFaction = false;
        
        [ColorUsage (showAlpha: false)]
        public Color color = new Color (1f, 1f, 1f, 1f);
        
        [ColorUsage (showAlpha: false)]
        public Color colorEnemy = new Color (1f, 1f, 1f, 1f);
        
        [PropertyRange (0f, 8f)]
        public float intensity = 4f;
        
        [PropertyRange (0f, 1f)]
        public float durationBuildup = 0.01f;

        public float durationStable = 0.05f;

        public float durationFade = 0.25f;
        
        public float offset = 0f;
    }
}

