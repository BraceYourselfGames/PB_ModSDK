using System.Collections.Generic;
using Pathfinding;
using UnityEngine;
using Sirenix.OdinInspector;

namespace PhantomBrigade.Data
{
    [ExecuteInEditMode]
    public class DataMultiLinkerOverworldLandscape : DataMultiLinker<DataContainerOverworldLandscape>
    {
        public DataMultiLinkerOverworldLandscape ()
        {
            DataMultiLinkerUtility.RegisterOnAfterDeserialization (dataType, OnAfterDeserialization);
        }

        [HideReferenceObjectPicker]
        public class Presentation
        {
            [ShowInInspector]
            public static bool showSpawnsGroupKeys = true;
            
            [ShowInInspector]
            public static bool showSpawnsGrouped = true;
            
            [ShowInInspector]
            public static bool showSpawnsGeneral = true;
            
            [ShowInInspector, OnValueChanged ("OnShowLayerHeight")]
            public static bool showLayerHeight = false;

            private void OnShowLayerHeight ()
            {
                OverworldLandscapeManager.shaderDimensionOpacity = showLayerHeight ? 1f : 0f;
                OverworldLandscapeManager.RefreshGlobals ();
            }
        }

        // [ShowInInspector, HideLabel, BoxGroup ("Selected"), PropertyOrder (90)]
        public static DataContainerOverworldLandscape selection;
        public static string selectionPointGroupKey;
        public static int selectionPointIndex = -1;
        public static DataBlockOverworldLandscapeLayer selectionLayer;

        [ShowInInspector, HideLabel, FoldoutGroup ("View options")]
        public Presentation presentation = new Presentation ();
        
        public static void OnAfterDeserialization ()
        {
            selection = null;
        }

        [FoldoutGroup ("Utilities")]
        [Button, PropertyOrder (-10)]
        public void FixPrecipitationValues ()
        {
            foreach (var kvp in data)
            {
                var landscape = kvp.Value;
                if (landscape == null || landscape.layers == null)
                    continue;

                int i = -1;
                foreach (var layer in landscape.layers)
                {
                    i += 1;
                    if (layer == null)
                        continue;

                    var v = layer.precipitationFactors.v;
                    if (v.x < 0.5f || v.y < 0.5f)
                    {
                        Debug.LogWarning ($"Landscape {landscape.key} layer {i} has unusual precipitation values: {v}");
                        if (v.x.RoughlyEqual (0.5f) && v.y.RoughlyEqual (0f) && v.z.RoughlyEqual (1f))
                            layer.precipitationFactors = new DataBlockVector3 { v = new Vector3 (0.5f, 0.35f, 1f) };
                    }
                }
            }
        }
    }
}
