using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace PhantomBrigade.Data
{
    [ExecuteInEditMode]
    public class DataMultiLinkerUnitRole : DataMultiLinker<DataContainerUnitRole>
    {
        public DataMultiLinkerUnitRole ()
        {
            DataMultiLinkerUtility.RegisterOnAfterDeserialization (dataType, OnAfterDeserialization);
            DataMultiLinkerUtility.RegisterStandardTextHandling (dataType, ref textSectorKeys, TextLibs.unitRoles); 
        }

        public static List<string> dataKeysSelectable = new List<string> ();

        [Button]
        public void GenerateCustomRoles ()
        {
            int limit = 27;
            for (int i = 0; i < limit; ++i)
            {
                var key = $"custom_{i:00}";
                var role = new DataContainerUnitRole
                {
                    icon = $"s_icon_l32_role_custom{i}",
                    selectable = true,
                    textUsed = false
                };
                
                data[key] = role;
            }
        }
        
        public static void OnAfterDeserialization ()
        {
            dataKeysSelectable.Clear ();
            foreach (var kvp in data)
            {
                if (kvp.Value.selectable)
                    dataKeysSelectable.Add (kvp.Key);
            }
        }
    }
}


