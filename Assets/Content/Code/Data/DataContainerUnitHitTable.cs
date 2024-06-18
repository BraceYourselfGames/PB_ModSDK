using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using YamlDotNet.Serialization;

namespace PhantomBrigade.Data
{
    [Serializable][HideReferenceObjectPicker]
    public class DirectionToSocket
    {
        [LabelText ("Dir. scaling")]
        [DictionaryKeyDropdown ("@DataHelperUnitEquipment.GetSockets ()")]
        [DictionaryDrawerSettings (KeyLabel = "Socket")]
        public SortedDictionary<string, int> socketScaling = new SortedDictionary<string, int> ();
        
        [ReadOnly, YamlIgnore]
        [LabelText ("Dir. scaling (normalized)")]
        [DictionaryKeyDropdown("@DataHelperUnitEquipment.GetSockets ()")]
        [DictionaryDrawerSettings (KeyLabel = "Socket")]
        public Dictionary<string, float> socketProportions; 
    }

    [Serializable]
    public class DataContainerUnitHitTable : DataContainer
    {
        [LabelText ("Avg. scaling")]
        [OnValueChanged ("RefreshCoreLookups", true)]
        [DictionaryKeyDropdown ("@DataHelperUnitEquipment.GetSockets ()")]
        [DictionaryDrawerSettings (KeyLabel = "Socket")]
        public SortedDictionary<string, int> socketScaling = new SortedDictionary<string, int> ();
        
        [ReadOnly, YamlIgnore]
        [LabelText ("Avg. scaling (normalized)")]
        [DictionaryKeyDropdown("@DataHelperUnitEquipment.GetSockets ()")]
        [DictionaryDrawerSettings (KeyLabel = "Socket")]
        public Dictionary<string, float> socketProportions; 
        
        [OnValueChanged ("RefreshDirectionLookups", true)]
        public Dictionary<string, DirectionToSocket> directions;

        public override void OnAfterDeserialization (string key)
        {
            base.OnAfterDeserialization (key);
            RefreshCoreLookups ();
            RefreshDirectionLookups ();
        }

        private void RefreshCoreLookups ()
        {
            if (socketScaling == null)
                return;
            
            if (socketProportions != null)
                socketProportions.Clear ();
            else
                socketProportions = new Dictionary<string, float> ();
            
            int total = 0;
            foreach (var kvp2 in socketScaling)
                total += kvp2.Value;
            
            if (total <= 0)
                return;
            
            float weightPerPart = 1f / total;
            foreach (var kvp2 in socketScaling)
            {
                var socket = kvp2.Key;
                var proportion = (float)kvp2.Value * weightPerPart;
                socketProportions.Add (socket, proportion);
            }
        }

        private void RefreshDirectionLookups ()
        {
            if (directions != null && directions.Count > 0)
            {
                foreach (var kvp in directions)
                {
                    var dir = kvp.Value;
                    if (dir == null || dir.socketScaling == null)
                        continue;
                    
                    if (dir.socketProportions != null)
                        dir.socketProportions.Clear ();
                    else
                        dir.socketProportions = new Dictionary<string, float> ();

                    int total = 0;
                    foreach (var kvp2 in dir.socketScaling)
                        total += kvp2.Value;
                   
                    if (total <= 0)
                        continue;
                    
                    float weightPerPart = 1f / total;
                    foreach (var kvp2 in dir.socketScaling)
                    {
                        var socket = kvp2.Key;
                        var proportion = (float)kvp2.Value * weightPerPart;
                        dir.socketProportions.Add (socket, proportion);
                    }
                }
            }
        }
    }
}

