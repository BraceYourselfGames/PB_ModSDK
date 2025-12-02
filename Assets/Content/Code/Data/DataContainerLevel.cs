using System;
using System.Collections.Generic;
using System.Text;
using Area;
using Sirenix.OdinInspector;
using UnityEngine;
using YamlDotNet.Serialization;

namespace PhantomBrigade.Data
{
    public class DataContainerLevel
    {
        [YamlIgnore, HideInInspector]
        public AreaDataContainerSerialized dataCollections;
        
        [YamlIgnore, HideInInspector]
        public AreaDataContainer dataUnpacked;

        [YamlIgnore, MultiLineProperty (8), ShowInInspector, ReadOnly, HideLabel]
        private string previewInfo => GetInfo ();

        private StringBuilder sb = new StringBuilder ();
        
        private string GetInfo ()
        {
            sb.Clear ();
            
            sb.Append ("Raw collections: ");
            if (dataCollections == null)
                sb.Append ("\nnull");
            else
            {
                sb.Append ("\n- Points: ");
                if (dataCollections.points == null)
                    sb.Append ("null");
                else
                    sb.Append (dataCollections.points.Length);
                
                sb.Append ("\n- Spots indexes: ");
                if (dataCollections.spotIndexes == null)
                    sb.Append ("null");
                else
                    sb.Append (dataCollections.spotIndexes.Length);
            }
            
            sb.Append ("\n\nUnpacked data: ");
            if (dataUnpacked == null)
                sb.Append ("\nnull");
            else
            {
                sb.Append ("\n- Points: ");
                if (dataUnpacked.points == null)
                    sb.Append ("null");
                else
                    sb.Append (dataUnpacked.points.Length);
                
                sb.Append ("\n- Spots: ");
                if (dataUnpacked.spots == null)
                    sb.Append ("null");
                else
                    sb.Append (dataUnpacked.spots.Count);
            }

            return sb.ToString ();
        }
    }
}