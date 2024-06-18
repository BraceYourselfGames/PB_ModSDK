using UnityEditor;
using UnityEngine;

namespace PhantomBrigade.ModTools
{
    // Relocated here for use in data editors
    public static class ModToolsColors
    {
        #if UNITY_EDITOR
        private static bool darkMode => EditorGUIUtility.isProSkin;
        #else
        private static bool darkMode => true;
        #endif
        
        public static readonly Color BoxBackground = darkMode ? new Color(1f, 1f, 1f, 0.06f) : new Color(1f, 1f, 1f, 0.26f);
        public static readonly Color BoxBackgroundGreen = darkMode ? new Color (0.7f, 1f, 0.85f, 0.06f) : new Color (0.7f, 1f, 0.85f, 0.26f);

        public static readonly Color BoxText = darkMode ? new Color(1f, 1f, 1f, 0.6f) : new Color(0.0f, 0.0f, 0.0f, 0.6f);
        public static readonly Color BoxTextGreen = darkMode ? new Color (0.69f, 1f, 0.85f, 0.6f) : new Color (0.07f, 0.15f, 0.11f, 0.6f);

        public static readonly Color HighlightSteam = Color.Lerp (new Color (0.48f, 0.72f, 1f), Color.white, 0.6f);
        public static readonly Color HighlightSelectedMod = new Color (0.8f, 1f, 0.9f);
        public static readonly Color HighlightGreen = new Color (0.78f, 1f, 0.48f);
        public static readonly Color HighlightBlue = new Color (0.3f, 0.76f, 1f);
        
        public static readonly Color HighlightNeonGreen = new Color (0.78f, 1f, 0.48f) * 1.5f;
        public static readonly Color HighlightNeonBlue = new Color (0.3f, 0.76f, 1f) * 1.5f;
        public static readonly Color HighlightNeonSepia = new Color (1f, 0.8f, 0.6f) * 1.5f;
        
        public static readonly Color HighlightValid = new Color (0.3f, 0.76f, 1f);
        public static readonly Color HighlightWarning = darkMode ? new Color(1f, 0.75686276f, 0.02745098f) : new Color(0.7882353f, 0.5921569f, 0.0f, 1f);
        public static readonly Color HighlightError = darkMode ? new Color (1f, 0.3f, 0.3f) : new Color (0.78f, 0.2f, 0.2f);
    }
}
