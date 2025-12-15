using System;
using Sirenix.OdinInspector;
using UnityEngine;
using YamlDotNet.Serialization;

namespace PhantomBrigade.Data
{
    [Serializable]
    public class DataBlockTextTrimodal
    {
        public enum Mode
        {
            Fixed = 0,
            LocUnique = 1,
            LocReused = 2
        }
        
        [PropertyOrder (-1)]
        [EnumToggleButtons, GUIColor ("GetColor"), LabelText (" ")]
        public Mode mode = Mode.Fixed;

        [ShowIf ("IsFixed"), GUIColor ("GetColor"), HideLabel, TextArea (1, 10)]
        public string text;
        
        [YamlIgnore]
        [ShowIf ("IsLocUnique"), GUIColor ("GetColor"), HideLabel, TextArea (1, 10)]
        public string textLocUnique;
        
        [ShowIf ("IsLocReused"), InfoBox ("@GetText ()", InfoMessageType.None)]
        [ValueDropdown ("@DataManagerText.GetLibrarySectorKeys ()"), GUIColor ("GetColor"), LabelText ("Sector")]
        public string textSector;
        
        [ShowIf ("IsLocReused")]
        [ValueDropdown ("@DataManagerText.GetLibraryTextKeys (textSector)"), GUIColor ("GetColor"), LabelText ("Key")]
        public string textKey;

        public string GetText ()
        {
            if (mode == Mode.LocReused)
                return DataManagerText.GetText (textSector, textKey, true);

            if (mode == Mode.LocUnique)
                return textLocUnique;
            
            return text;
        }

        public virtual void ResolveText (string sectorKey, string textKeyUnique)
        {
            if (mode == Mode.LocUnique)
                textLocUnique = DataManagerText.GetText (sectorKey, textKeyUnique, true);
        }

        #if UNITY_EDITOR
        
        public virtual void SaveText (string sectorKey, string textKeyUnique)
        {
            if (mode == Mode.LocUnique && !string.IsNullOrEmpty (textLocUnique))
                DataManagerText.TryAddingTextToLibrary (sectorKey, textKeyUnique, textLocUnique);
        }

        private string GetTextLabel => IsFixed ? "Text" : "Loc. Text";
        private Color GetColor => Color.HSVToRGB (IsLocUnique ? 0.35f : 0.56f, IsFixed ? 0f : 0.25f, 1f);
        private bool IsFixed => mode == Mode.Fixed;
        private bool IsLocUnique => mode == Mode.LocUnique;
        private bool IsLocReused => mode == Mode.LocReused;

        #endif
    }
}