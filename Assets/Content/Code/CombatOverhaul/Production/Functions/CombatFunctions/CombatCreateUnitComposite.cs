using System;
using System.Collections.Generic;
using PhantomBrigade.Data;
using PhantomBrigade.DebugConsole;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PhantomBrigade.Functions
{
    [Serializable]
    public class CombatCreateUnitComposite : ICombatFunction
    {
        public bool tagsUsed;
        
        [ShowIf ("tagsUsed")]
        [DictionaryKeyDropdown ("@DataMultiLinkerUnitComposite.tags")]
        [DictionaryDrawerSettings (KeyColumnWidth = DataEditor.dictionaryKeyWidth)]
        public SortedDictionary<string, bool> tags;
        
        [HideIf ("tagsUsed")]
        [ValueDropdown ("@DataMultiLinkerUnitComposite.data.Keys")]
        [ListDrawerSettings (DefaultExpandedState = true, AlwaysAddDefaultValue = true)]
        public List<string> blueprintKeys = new List<string> ();
        
        public string instanceNameOverride;
        
        public TargetFromSource target;
        public int levelOffset;
        
        public bool friendly;
        public bool controllable;
        public bool navigationSampled;

        public void Run ()
        {
            #if !PB_MODSDK
            
            if (blueprintKeys == null || blueprintKeys.Count == 0)
            {
                Debug.Log ($"Failed to spawn unit composite: no keys provided");
                return;
            }

            string blueprintKey = null;

            if (tagsUsed)
            {
                var blueprintKeysFromTags = DataTagUtility.GetKeysWithTags (DataMultiLinkerUnitComposite.data, tags);
                blueprintKey = blueprintKeysFromTags.GetRandomEntry ();
            }
            else if (blueprintKeys != null && blueprintKeys.Count > 0)
                blueprintKey = blueprintKeys.GetRandomEntry ();

            if (string.IsNullOrEmpty (blueprintKey))
            {
                Debug.LogWarning ($"Failed to spawn unit composite: no blueprint key selected");
                return;
            }

            var instanceName = instanceNameOverride;
            if (string.IsNullOrEmpty (instanceName))
                instanceName = blueprintKey;

            var pos = new Vector3 (150f, 0f, 150f);
            var rot = Quaternion.identity;
            
            bool targetFound = ScenarioUtility.GetTarget
            (
                null, 
                target, 
                out var targetPosition, 
                out var targetDirection, 
                out var targetUnitCombat
            );

            if (targetFound)
            {
                pos = targetPosition;
                rot = Quaternion.LookRotation (targetDirection.FlattenAndNormalize ());
            }

            int level = 1;
            var sitePersistent = ScenarioUtility.GetCombatSite ();
            if (sitePersistent != null && sitePersistent.hasCombatUnitLevel)
                level = Mathf.Max (1, sitePersistent.combatUnitLevel.i + levelOffset);

            Debug.Log ($"Spawning unit composite {blueprintKey} with instance name {instanceName} at {pos}");
            UnitUtilities.CreateCompositeUnit (blueprintKey, instanceName, pos, rot, level, controllable, friendly, navigationSampled);

            #endif
        }

        [Button, PropertyOrder (-1), HideInEditorMode]
        private void Test ()
        {
            #if !PB_MODSDK
            
            if (!IDUtility.IsGameState (GameStates.combat))
                return;
            
            Run ();
            
            #endif
        }
    }
}