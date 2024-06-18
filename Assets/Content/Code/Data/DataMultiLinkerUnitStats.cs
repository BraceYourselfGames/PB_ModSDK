using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace PhantomBrigade.Data
{
    [ExecuteInEditMode]
    public class DataMultiLinkerUnitStats : DataMultiLinker<DataContainerUnitStat> 
    {
        public DataMultiLinkerUnitStats ()
        {
            DataMultiLinkerUtility.RegisterOnAfterDeserialization (dataType, OnAfterDeserialization);
            DataMultiLinkerUtility.RegisterStandardTextHandling (dataType, ref textSectorKeys, TextLibs.unitStats); 
        }
        
        [HideReferenceObjectPicker]
        public class Presentation
        {
            [ShowInInspector]
            public static bool showUI = true;
        
            [ShowInInspector]
            public static bool showExposure = true;
        
            [ShowInInspector]
            public static bool showValues = true;
            
            [ShowInInspector]
            public static bool showSorting = false;
        }

        [ShowInInspector, HideLabel, FoldoutGroup ("View options")]
        public Presentation presentation = new Presentation ();

        [ShowIf ("@DataMultiLinkerUnitStats.Presentation.showSorting")]
        [ShowInInspector]
        public static List<string> keysSorted = new List<string> ();
        
        [ShowIf ("@DataMultiLinkerUnitStats.Presentation.showSorting")]
        [ShowInInspector]
        public static HashSet<string> keysMarkedAsOutputs = new HashSet<string> ();
        
        // [ShowInInspector]
        public static List<DataContainerUnitStat> dataSorted = new List<DataContainerUnitStat> ();
        
        public static void OnAfterDeserialization ()
        {
            dataSorted.Clear ();
            keysMarkedAsOutputs.Clear ();
            
            foreach (var kvp in data)
            {
                var container = kvp.Value;
                dataSorted.Add (container);
                if (container.showAsOutput)
                    keysMarkedAsOutputs.Add (kvp.Key);
            }
            
            dataSorted.Sort ((x, y) => 
                x.uiPriority.CompareTo (y.uiPriority));
            
            keysSorted.Clear ();
            foreach (var container in dataSorted)
                keysSorted.Add (container.key);
            
            
        }

        /*
        [FoldoutGroup ("Utilities", false)]
        [Button (ButtonSizes.Large)]
        public void Upgrade ()
        {
            foreach (var kvp in data)
            {
                var bp = kvp.Value;
                
                if (bp.minPerPartLimit)
                    bp.minInPart = new DataBlockFloat { f = bp.minPerPart };
                
                if (bp.minPerUnitLimit)
                    bp.minInUnit = new DataBlockFloat { f = bp.minPerUnit };
                
                if (bp.maxPerPartLimit)
                    bp.maxInPart = new DataBlockFloat { f = bp.maxPerPart };
                
                if (bp.maxPerUnitLimit)
                    bp.maxInUnit = new DataBlockFloat { f = bp.maxPerUnit };
                
                if (bp.levelUsed)
                    bp.increasePerLevel = new DataBlockFloat { f = bp.levelIncrease };
                
                if (bp.multiplierUsed && !bp.multiplier.RoughlyEqual (1f))
                    bp.scale = new DataBlockFloat { f = bp.multiplier };
            }
        }
        */
    }
}


