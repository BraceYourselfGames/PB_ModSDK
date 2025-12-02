using System;
using System.Collections.Generic;
using PhantomBrigade.Functions;
using PhantomBrigade.Data.UI;
using PhantomBrigade.Game;
using PhantomBrigade.Overworld;
using Sirenix.OdinInspector;
using UnityEngine;
using YamlDotNet.Serialization;

#if UNITY_EDITOR
#endif

namespace PhantomBrigade.Data
{
    public static class TextVariantHelper
    {
        public static Dictionary<string, int> pronounsToMemoryValues = new Dictionary<string, int>
        {
            { pronoun1, 1 },
            { pronoun2, 2 },
            { pronoun3, 3 }
        };
        
        public static Dictionary<int, string> memoryValuesToPronouns = new Dictionary<int, string>
        {
            { 1, pronoun1 },
            { 2, pronoun2 },
            { 3, pronoun3 }
        };
        
        public static List<string> pronouns = new List<string>
        {
            pronoun1,
            pronoun2,
            pronoun3
        };

        public const string memoryKey = "pronoun";
        
        public const string pronoun1 = "she";
        public const string pronoun2 = "he";
        public const string pronoun3 = "they";
    }

    public class DataBlockResourceChange
    {
        [ValueDropdown ("GetResourceKeys")]
        public string key;

        [Tooltip ("If true, a change subtracting resources will be checked before being allowed to proceed")]
        public bool check = true;

        [ShowIf ("check")]
        [Tooltip ("If true, and used in overworld option, option will be hidden instead of grayed out if a change isn't possible")]
        public bool checkStrict = false;
        
        [Tooltip ("If true, the value below will be added to current amount of a given resource, if false, then value below will replace current amount")]
        public bool offset = true;
        
        public int value = 0;

        #region EDITOR
        #if UNITY_EDITOR
        
        private IEnumerable<string> GetResourceKeys =>
            DataMultiLinkerResource.data?.Keys;
        
        #endif
        #endregion
        
        public override string ToString ()
        {
            string keyText = !string.IsNullOrEmpty (key) ? key : "[no key]";
            string checkText = check ? checkStrict ? "(checked strictly)" : "(checked)" : "(unchecked)"; 
            
            if (offset)
            {
                return $"{keyText}: Offset by {value} {checkText}";
            }
            else
            {
                return $"{keyText}: Set to {value} {checkText}";
            }
        }
    }
}