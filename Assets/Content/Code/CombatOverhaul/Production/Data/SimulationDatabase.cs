using UnityEngine;

namespace PhantomBrigade
{
    public static class LayerMasks
    {
        private const string environmentLayer = "Environment";
        private const string unitLayer = "Units";
        private const string propLayer = "Props";
        private const string puppetRagdollLayer = "PuppetRagdoll";
        private const string simulationProxyLayer = "SimulationProxy";
        private const string unitPathEndLayer = "UnitNodes";
        private const string impactTriggersLayer = "ImpactTriggers";
        private const string overlapTriggersLayer = "OverlapTrigger";
        private const string groundLayer = "GroundLevel";
        private const string debrisLayer = "Debris";


        public static int unitSelectionMask => LayerMask.GetMask (unitLayer, puppetRagdollLayer, impactTriggersLayer, simulationProxyLayer);
        public static int projectileMask => LayerMask.GetMask (unitLayer, puppetRagdollLayer, environmentLayer, propLayer);
        public static int flightMask => LayerMask.GetMask (environmentLayer, propLayer);
        public static int beamMask => LayerMask.GetMask (unitLayer, puppetRagdollLayer);
        
        public static int puppetRagdollMask => LayerMask.GetMask (puppetRagdollLayer);
        public static int puppetRagdollLayerID => LayerMask.NameToLayer (puppetRagdollLayer);
        
        public static int unitPathEndMask => LayerMask.GetMask (unitPathEndLayer);
        
        public static int cameraTopDownMask => LayerMask.GetMask (environmentLayer, groundLayer);
        
        public static int environmentMask => LayerMask.GetMask (environmentLayer);
        public static int environmentLayerID => LayerMask.NameToLayer (environmentLayer);
        
        
        public static int environmentAndDebrisMask => LayerMask.GetMask (environmentLayer, debrisLayer);
        

        public static int overlapTriggerMask => LayerMask.GetMask (overlapTriggersLayer);
        public static int overlapTriggersID => LayerMask.NameToLayer (overlapTriggersLayer);
        
        
        public static int unitMask => LayerMask.GetMask (unitLayer);
        public static int unitLayerID => LayerMask.NameToLayer (unitLayer);
        

        public static int impactTriggersMask = LayerMask.GetMask (impactTriggersLayer);
        public static int impactTriggersLayerID => LayerMask.NameToLayer (impactTriggersLayer);
        
        public static int simulationProxyAndEnvironmentMask => LayerMask.GetMask (environmentLayer, simulationProxyLayer);
        public static int simulationProxyMask => LayerMask.GetMask (simulationProxyLayer);
        public static int simulationProxyLayerID => LayerMask.NameToLayer (simulationProxyLayer);
        

        public static int propMask => LayerMask.GetMask (propLayer);
        public static int propLayerID => LayerMask.NameToLayer (propLayer);

        
        public static int groundLayerMask => LayerMask.GetMask (groundLayer);
        public static int groundLayerID => LayerMask.NameToLayer (groundLayer);
    }
}