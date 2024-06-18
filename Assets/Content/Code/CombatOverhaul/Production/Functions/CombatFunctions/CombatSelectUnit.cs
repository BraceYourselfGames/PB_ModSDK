using System;
using Sirenix.OdinInspector;

namespace PhantomBrigade.Functions
{
    [Serializable]
    [PropertyTooltip ("Allows selecting and/or focusing a specific unit with a known internal name. Primarily used for unique scenarios with directly defined units.")]
    public class CombatSelectUnit : CombatFunctionWithDelay, ICombatFunction
    {
        [PropertyTooltip ("Internal name of the targeted persistent unit entity")]
        public string nameInternal;
        
        [PropertyTooltip ("Whether a unit should be selected on the timeline")]
        [HideIf ("@string.IsNullOrEmpty (nameInternal)")]
        public bool select = true;
        
        [PropertyTooltip ("Whether a unit should be focused by the camera")]
        [HideIf ("@string.IsNullOrEmpty (nameInternal)")]
        public bool focus = true;
        
        public override void Run ()
        {
            base.Run ();
        }
        
        [Button ("Apply"), ButtonGroup, PropertyOrder (-1), ShowIf ("IsInCombat")]
        protected override void RunDelayed ()
        {
            #if !PB_MODSDK
            
            if (!IsRunningPossible ())
                return;
            
            var unitPersistent = IDUtility.GetPersistentEntity (nameInternal);
            var unitCombat = IDUtility.GetLinkedCombatEntity (unitPersistent);
            if (unitCombat == null)
            {
                if (Contexts.sharedInstance.combat.hasUnitSelected)
                {
                    Contexts.sharedInstance.combat.RemoveUnitSelected ();
                    GameCameraSystem.ClearTarget ();
                }
                
                return;
            }

            if (select)
                Contexts.sharedInstance.combat.ReplaceUnitSelected (unitCombat.id.id);
            
            if (focus)
                GameCameraSystem.MoveToUnit (unitCombat);
            
            #endif
        }
    }
}