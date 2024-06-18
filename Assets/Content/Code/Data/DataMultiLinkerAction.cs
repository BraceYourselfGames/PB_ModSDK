using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PhantomBrigade.Data
{
    [ExecuteInEditMode]
    public class DataMultiLinkerAction : DataMultiLinker<DataContainerAction>
    {
        public DataMultiLinkerAction ()
        {
            DataMultiLinkerUtility.RegisterStandardTextHandling (dataType, ref textSectorKeys, TextLibs.combatActions); 
        }
        
        [ShowInInspector]
        public static bool showUI = true;
        
        [ShowInInspector]
        public static bool showCore = true;

        [ShowInInspector]
        public static bool showOther = true;

        [FoldoutGroup ("Utilities", false), Button, PropertyOrder (-2)]
        public void MoveOverrideColor ()
        {
            foreach (var kvp in data)
            {
                var action = kvp.Value;
                action.dataUI.colorOverride = new HSBColor (new HSBColor (action.dataUI.color).h, 1f, 1f).ToColor ();
            }
        }
        
        [FoldoutGroup ("Utilities", false), Button, PropertyOrder (-2)]
        public void ClearUnusedData ()
        {
            foreach (var kvp in data)
            {
                var action = kvp.Value;
                if (action.dataCore != null)
                {
                    ClearListIfEmpty (ref action.dataCore.eventsOnValidation);
                    ClearListIfEmpty (ref action.dataCore.eventsOnCreation);
                    ClearListIfEmpty (ref action.dataCore.eventsOnModification);
                    ClearListIfEmpty (ref action.dataCore.eventsOnDispose);
                    ClearListIfEmpty (ref action.dataCore.eventsOnStart);
                    ClearListIfEmpty (ref action.dataCore.eventsOnEnd);

                    if (action.dataCore.check != null)
                    {
                        ClearListIfEmpty (ref action.dataCore.check.tags);
                        ClearListIfEmpty (ref action.dataCore.check.blueprints);
                        ClearListIfEmpty (ref action.dataCore.check.parts);
                        ClearListIfEmpty (ref action.dataCore.check.subsystems);
                        ClearListIfEmpty (ref action.dataCore.check.stats);
                        ClearListIfEmpty (ref action.dataCore.check.pilot);
                    }
                }
            }
        }

        private void ClearListIfEmpty<T> (ref List<T> list)
        {
            if (list != null && list.Count == 0)
                list = null;
        }

        /*
        [Button ("Move text", ButtonSizes.Large), PropertyOrder (-10)]
        public void Upgrade ()
        {
            foreach (var kvp in data)
            {
                var action = kvp.Value;
                var key = action.dataIdentifiable.name.ToLower ()
                    .Replace (" (", "_")
                    .Replace (")", "")
                    .Replace ("primary weapon", "attack_primary")
                    .Replace (" brace", "");

                if (!string.IsNullOrEmpty (action.dataIdentifiable.name))
                {
                    var nameKey = $"{key}_name";
                    DataManagerText.TryAddingText (TextLibs.uiActions, nameKey, action.dataIdentifiable.name);
                    action.dataIdentifiable.name = nameKey;
                }

                if (!string.IsNullOrEmpty (action.dataIdentifiable.description))
                {
                    var descriptionKey = $"{key}_desc";
                    DataManagerText.TryAddingText (TextLibs.uiActions, descriptionKey, action.dataIdentifiable.description);
                    action.dataIdentifiable.description = descriptionKey;
                }
            }
        }
        */
    }
}


