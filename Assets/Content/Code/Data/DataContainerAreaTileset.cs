using System;
using System.Collections.Generic;
using Area;
using YamlDotNet.Serialization;

namespace PhantomBrigade.Data
{
    [Serializable]
    public class DataContainerAreaTileset : DataContainer
    {
        public int id;
        public int idOfInterior;
        
        [YamlIgnore]
        public string name;
        
        public string sfxNameImpact = "impact_bullet_rock";
        public string fxNameHit = "fx_impact_concrete";
        public string fxNameStep = "fx_mech_footstep_concrete";
        public string fxNameExplosion = "fx_environment_explosion";
        public int propIDDebrisPile = 100;
        public List<int> propIDDebrisClumps = new List<int> (new int[] { 103 });
        
        public Dictionary<int, string> groupIdentifiers;
        public Dictionary<Vector3Int, AreaDataNavOverride> navOverrides;
        
        public override void OnBeforeSerialization ()
        {

        }
        
        public override void OnAfterDeserialization (string key)
        {
            base.OnAfterDeserialization (key);
            name = key;
        }
    }
}

