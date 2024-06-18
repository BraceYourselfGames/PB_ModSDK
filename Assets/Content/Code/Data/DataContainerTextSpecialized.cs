using System.Text.RegularExpressions;
using UnityEngine;
using Random = UnityEngine.Random;

namespace PhantomBrigade.Data
{
    public static class PseudoLoc
    {
        /*
        File: pLocalize.cs
        Version:  1.0
        
        Author:
          Anne Gunn (ompeag at wyoming dot com)
          
        Copyright:
          Copyright (c) 2004, Anne Gunn.
          
        License:
          This module is distributed under the MIT License.
          http://www.opensource.org/licenses/mit-license.php
          
        Notes:
          This module is a pseudolocalization translation engine.  It can transform
          English strings into not-English-but-still-readable strings for testing
          the translation-worthiness of code, databases, XML, etc.  It's distributed
          with and intended to be used from a commandline wrapper pLoc.exe and a 
          Windows wrapper called pLocWin.exe.
        */
    
        class RegexTuple
        {
            Regex thisRegex;
            string replaceWith;
            int numTimes;

            public RegexTuple (string expression, string replacement, int iTimes)
            {
                thisRegex = new Regex (expression);
                replaceWith = replacement;
                numTimes = iTimes;
            }

            public int GetNumTimes ()
            {
                return numTimes;
            }

            public string GetReplaceWith ()
            {
                return replaceWith;
            }

            public Regex GetRegex ()
            {
                return thisRegex;
            }
        }

        // set up the transformations we want to execute
        private static RegexTuple[] arrRegexTuples =
        {
            new RegexTuple ("a", "ââ", 1), // find the first lowercase a and replace with double â
            new RegexTuple ("A", "ÀÀ", 1), // find the first uppercase A and replace with double ÀÀ
            new RegexTuple ("e", "éé", 1), // etc
            new RegexTuple ("E", "ÉÉ", 1),
            new RegexTuple ("i", "íí", 1),
            new RegexTuple ("I", "ÍÍ", 1),
            new RegexTuple ("o", "óó", 1),
            new RegexTuple ("O", "ÓÓ", 1),
            new RegexTuple ("u", "üü", 1),
            new RegexTuple ("U", "ÜÜ", 1),
            new RegexTuple ("n", "ñ", -1), // all n's and c's get transformed but not doubled
            new RegexTuple ("N", "Ñ", -1),
            new RegexTuple ("c", "ç", -1),
            new RegexTuple ("C", "Ç", -1),
            
            // transformations that helpexpose punctuation assumptions
            new RegexTuple ("(\\S):", "$1 :", -1), // change every colon not already preceded by whitespace to have
            new RegexTuple ("(\\S)\\?", "$1 ?", -1), // ditto with question marks and exclamations 
            new RegexTuple ("(\\S)!", "$1 !", -1),
            
            // a transformation to help find truncated strings
            new RegexTuple ("(.*)", "($1)", 1) // wrap the whole text in round brackets
        };

        public static string Localize (string oldString)
        {
            string pLocalString = oldString;
            foreach (RegexTuple tuple in arrRegexTuples)
            {
                pLocalString = tuple.GetRegex ().Replace (pLocalString, tuple.GetReplaceWith (), tuple.GetNumTimes ());
            }

            return pLocalString;
        }
        
        private const string pseudolocColorTag = "[93ff8f]";

        public static string ApplyPseudoloc (string text)
        {
            var textModified = PseudoLoc.Localize (text);
            textModified = $"{pseudolocColorTag}{textModified}[-]";
            return textModified;
        }
    }
    
    public static class TextUtility
    {
        public static void GetRandomPilotIdentificationIndexes (out int nameIndexPrimary, out int nameIndexSecondary, out int callsignIndex)
        {
            nameIndexPrimary = -1;
            nameIndexSecondary = -1;
            callsignIndex = -1;

            var collectionGivenNames = DataManagerText.GetTextCollectionByTag ("pilot_names_given");
            int givenNameCount = collectionGivenNames != null ? collectionGivenNames.Count : 0;

            var collectionFamilyNames = DataManagerText.GetTextCollectionByTag ("pilot_names_family");
            int familyNameCount = collectionFamilyNames != null ? collectionFamilyNames.Count : 0;
            
            var collectionCallsigns = DataManagerText.GetTextCollectionByTag ("pilot_names_callsigns");
            int callsignCount = collectionCallsigns != null ? collectionCallsigns.Count : 0;

            if (givenNameCount > 0)
                nameIndexPrimary = UnityEngine.Random.Range (0, givenNameCount);
            
            if (familyNameCount > 0)
                nameIndexSecondary = UnityEngine.Random.Range (0, familyNameCount);
            
            if (callsignCount > 0)
                callsignIndex = UnityEngine.Random.Range (0, callsignCount);
        }

        public static string GetPilotCallsignTextFromIndex (int callsignIndex)
        {
            var collectionCallsigns = DataManagerText.GetTextCollectionByTag ("pilot_names_callsigns");
            bool callsignsPresent = collectionCallsigns != null && collectionCallsigns.Count > 0;
                
            if (callsignsPresent && callsignIndex.IsValidIndex (collectionCallsigns))
                return collectionCallsigns[callsignIndex];
                
            if (callsignIndex != -1)
                Debug.LogWarning ($"Failed to find callsign of a pilot using index {callsignIndex}");
            return string.Empty;
        }
        
        public static string GetPilotNameTextFromIndexes (int nameIndexPrimary, int nameIndexSecondary)
        {
            var collectionGivenNames = DataManagerText.GetTextCollectionByTag ("pilot_names_given");
            bool givenNamesPresent = collectionGivenNames != null && collectionGivenNames.Count > 0;
            string givenName = string.Empty;

            if (givenNamesPresent && nameIndexPrimary.IsValidIndex (collectionGivenNames))
                givenName = collectionGivenNames[nameIndexPrimary];
            else if (nameIndexPrimary != -1)
                Debug.LogWarning ($"Failed to find primary name of a pilot using index {nameIndexPrimary} | Primary collection: {(collectionGivenNames != null ? collectionGivenNames.Count.ToString() : "null")}");
            
            var collectionFamilyNames = DataManagerText.GetTextCollectionByTag ("pilot_names_family");
            bool familyNamesPresent = collectionFamilyNames != null && collectionFamilyNames.Count > 0;
            string familyName = string.Empty;
            
            if (familyNamesPresent && nameIndexSecondary.IsValidIndex (collectionFamilyNames))
                familyName = collectionFamilyNames[nameIndexSecondary];
            else if (nameIndexSecondary != -1)
                Debug.LogWarning ($"Failed to find secondary name of a pilot using index {nameIndexSecondary} | Secondary collection: {(collectionFamilyNames != null ? collectionFamilyNames.Count.ToString() : "null")}");

            bool givenNameFound = !string.IsNullOrEmpty (givenName);
            bool familyNameFound = !string.IsNullOrEmpty (familyName);

            if (givenNameFound)
            {
                if (familyNameFound)
                    return $"{givenName} {familyName}"; // TODO: Add support for a localization config field that swaps this order
                
                return givenName;
            }
            
            if (familyNameFound)
                return familyName;
                
            return string.Empty;
        }
        
        public static string GetPilotBuiltinBioFromIndex (int bioIndex)
        {
            var text = DataManagerText.GetText (TextLibs.uiBase, $"pilot_bio_builtin_{bioIndex:00}");
            return text;
        }
        
        
        
        public static int GetRandomOverworldEntityIdentificationFromGroup (string nameGroupKey)
        {
            var nameGroup = DataMultiLinkerOverworldNameGroup.GetEntry (nameGroupKey, false);
            if (nameGroup == null)
            {
                // Debug.LogWarning ($"Failed to get random name index of a site using name group key {nameGroupKey}");
                return -1;
            }

            return GetRandomOverworldEntityIdentification (nameGroup.collectionTag);
        }
        
        public static int GetRandomOverworldEntityIdentification (string collectionTag)
        {
            var collection = DataManagerText.GetTextCollectionByTag (collectionTag);
            int collectionCount = collection != null ? collection.Count : 0;
            if (collectionCount <= 0)
            {
                Debug.LogWarning ($"Failed to get random name index of a site using text collection tag {collectionTag}");
                return -1;
            }

            return UnityEngine.Random.Range (0, collectionCount);
        }

        public static void GetRandomUnitIdentification (out int nameSerial, out int nameIndex)
        {
            nameSerial = Random.Range (1, 10);
            nameIndex = -1;
            
            var collectionNames = DataManagerText.GetTextCollectionByTag ("group_unit");
            int nameCount = collectionNames != null ? collectionNames.Count : 0;
            if (nameCount < 0)
                return;
            
            nameIndex = Random.Range (0, nameCount);
        }

        public static string GetUnitIdentificationText (int nameSerial, int nameIndex)
        {
            bool nameSerialUsed = nameSerial > 0;
            
            var collectionNames = DataManagerText.GetTextCollectionByTag ("group_unit");
            int nameCount = collectionNames?.Count ?? 0;
            if (collectionNames == null || nameCount <= 0 || nameIndex < 0 || nameIndex >= nameCount)
                return string.Empty;

            var nameBase = collectionNames[nameIndex];
            return nameSerialUsed ? $"{nameBase}-{nameSerial}" : nameBase;
        }
    }
}