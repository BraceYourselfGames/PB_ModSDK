using System;
using System.Collections.Generic;
using System.Text;
using Sirenix.OdinInspector;
using UnityEngine;
using YamlDotNet.Serialization;

namespace PhantomBrigade.Data
{
    [LabelWidth (160f), HideReferenceObjectPicker, Serializable]
    public class DataBlockTextTrimodal
    {
        public enum Mode
        {
            [LabelText ("Fixed", SdfIconType.Lock)] Fixed = 0,
            [LabelText ("Unique", SdfIconType.TextLeft)] LocUnique = 1,
            [LabelText ("Reused", SdfIconType.TextIndentLeft)] LocReused = 2
        }

        [PropertyOrder (2)]
        [ShowInInspector, DisplayAsString, HideLabel, HorizontalGroup ("A")]
        private static string modePrefix = " ";
        
        [PropertyOrder (3)]
        [GUIColor ("GetColor"), HideLabel, HorizontalGroup ("A", 90f), OnValueChanged ("OnModeChanged")]
        public Mode mode = Mode.LocUnique;

        [ShowIf ("IsFixed"), GUIColor ("GetColor"), HideLabel, TextArea (1, 10)]
        public string text;
        
        [YamlIgnore]
        [ShowIf ("IsLocUnique"), GUIColor ("GetColor"), HideLabel, TextArea (1, 10)]
        public string textLocUnique;
        
        [ShowIf ("IsLocReused"), InfoBox ("@GetText ()", InfoMessageType.None)]
        [ValueDropdown ("@DataManagerText.GetLibrarySectorKeys ()"), GUIColor ("GetColor"), LabelText ("Sector"), LabelWidth (80f)]
        public string textSector;
        
        [ShowIf ("IsLocReused")]
        [ValueDropdown ("@DataManagerText.GetLibraryTextKeys (textSector)"), GUIColor ("GetColor"), LabelText ("Key"), LabelWidth (80f)]
        public string textKey;

        public string GetText ()
        {
            if (mode == Mode.LocReused)
                return DataManagerText.GetText (textSector, textKey, true);

            if (mode == Mode.LocUnique)
                return textLocUnique;
            
            return text;
        }
        
        public void SetText (string value)
        {
            if (mode == Mode.Fixed)
                text = value;

            else if (mode == Mode.LocUnique)
                textLocUnique = value;
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
        private Color GetColor => Color.HSVToRGB (IsFixed ? 0.15f : 0.56f, IsLocUnique ? 0f : 0.2f, 1f);
        private bool IsFixed => mode == Mode.Fixed;
        private bool IsLocUnique => mode == Mode.LocUnique;
        private bool IsLocReused => mode == Mode.LocReused;

        private void OnModeChanged ()
        {
            if (mode == Mode.Fixed)
            {
                if (string.IsNullOrEmpty (text))
                {
                    if (!string.IsNullOrEmpty (textLocUnique))
                        text = textLocUnique;
                }

                textLocUnique = null;
                textSector = null;
                textKey = null;
            }
            else if (mode == Mode.LocUnique)
            {
                if (string.IsNullOrEmpty (textLocUnique))
                {
                    if (!string.IsNullOrEmpty (text))
                        textLocUnique = text;
                }

                text = null;
                textSector = null;
                textKey = null;
            }
            else if (mode == Mode.LocReused)
            {
                text = null;
                textLocUnique = null;
            }
        }

        #endif
    }
}