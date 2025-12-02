using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PhantomBrigade.Data
{
    [ExecuteInEditMode]
    public class DataMultiLinkerPilotEvent : DataMultiLinker<DataContainerPilotEvent>
    {
        public DataMultiLinkerPilotEvent ()
        {
            DataMultiLinkerUtility.RegisterStandardTextHandling (dataType, ref textSectorKeys, TextLibs.pilotEvents); 
            DataMultiLinkerUtility.RegisterOnAfterDeserialization (dataType, OnAfterDeserialization);
        }

        private static Dictionary<PilotEventType, DataContainerPilotEvent> dataByEventType = new Dictionary<PilotEventType, DataContainerPilotEvent> ();
        private static Array eventEnumTypes = Enum.GetValues (typeof (PilotEventType));
        
        public static void OnAfterDeserialization ()
        {
            dataByEventType.Clear ();
            if (data != null)
            {
                foreach (var kvp in data)
                {
                    var config = kvp.Value;
                    if (config != null)
                        dataByEventType[config.type] = config;
                }
            }

            foreach (var typeObj in eventEnumTypes)
            {
                var type = (PilotEventType)typeObj;
                if (!dataByEventType.ContainsKey (type) && type != PilotEventType.Unknown)
                {
                    Debug.LogWarning ($"Pilot event enum {type} has no config defined!");
                }
            }
        }

        public static DataContainerPilotEvent GetEventDataByType (PilotEventType eventType)
        {
            LoadDataChecked ();
            bool found = dataByEventType.TryGetValue (eventType, out var config);
            if (found)
                return config;
            return null;
        }
    }
}


