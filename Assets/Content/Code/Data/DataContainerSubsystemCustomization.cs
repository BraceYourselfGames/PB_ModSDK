using System;
using Entitas;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PhantomBrigade.Data
{
    [Equipment, Persistent]
    public sealed class DataKeyEquipmentLivery : IComponent
    {
        public string s;
    }
    
    [Serializable]
    public class DataContainerEquipmentLivery : DataContainer
    {
        public bool hidden;
        public int priority;
        
        [HideInInspector]
        public string textName;
        
        [HideInInspector]
        public string source;
        
        [PropertyRange (1, 3)]
        public int rating;

        // [ValueDropdown ("@DataLinkerUI.GetUnitPatternKeys ()")]
        [InlineButton ("ClearPattern", "-")]
        public string pattern;

        public Color colorPrimary = new Color (0.5f, 0.5f, 0.5f, 0f);
        public Color colorSecondary = new Color (0.5f, 0.5f, 0.5f, 0f);
        public Color colorTertiary = new Color (0.5f, 0.5f, 0.5f, 0f);

        public Vector4 materialPrimary = new Vector4 (0.0f, 0.2f, 0.5f, 0f);
        public Vector4 materialSecondary = new Vector4 (0.0f, 0.2f, 0.5f, 0f);
        public Vector4 materialTertiary = new Vector4 (0.0f, 0.2f, 0.5f, 0f);
        
        public Vector4 effect = new Vector4 (0.0f, 0.0f, 0.0f, 0.0f);
        
        #if UNITY_EDITOR
        private void ClearPattern (string value) => pattern = null;
        
        #endif
    }
}

