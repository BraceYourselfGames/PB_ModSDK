using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PhantomBrigade.Functions
{
    // Useful class for building combat functions that:
    // - Wait for completion of processes such as cutscenes
    // - Provide a timer that delays execution further
    public class CombatFunctionWithDelay : ICombatFunctionDelayed
    {
        public float delay = 0f;
        
        public bool IsDelayed ()
        {
            return delay > 0f;
        }

        public virtual void Run ()
        {
            #if !PB_MODSDK
            
            var delaySafe = Mathf.Clamp (delay, 0f, 60f);
            if (delaySafe <= 0f)
                RunDelayed ();
            else
            {
                // Debug.Log ($"Starting countdown to function: {delaySafe:0.##}");
                Co.Delay (delaySafe, RunDelayed);
            }
            
            #endif
        }

        protected virtual bool IsRunningPossible ()
        {
            #if !PB_MODSDK
            
            if (!IDUtility.IsGameLoaded () || !IDUtility.IsGameState (GameStates.combat))
            {
                Debug.LogWarning ($"Can't run a delayed function due to wrong context");
                return false;
            }
            
            var game = Contexts.sharedInstance.game;
            var combat = Contexts.sharedInstance.combat;
            if (game.isCutsceneInProgress || combat.isScenarioIntroInProgress)
            {
                Debug.LogWarning ($"Can't run a delayed function due to ongoing cutscene");
                return false;
            }
            
            #endif
            
            return true;
        }

        protected virtual void RunDelayed ()
        {
            // Since invocation of this method might have been delayed, we need to run some checks and bail if they fail
            if (!IsRunningPossible ())
                return;
        }

        private static bool IsInCombat ()
        {
            #if !PB_MODSDK
            
            return IDUtility.IsGameLoaded () && IDUtility.IsGameState (GameStates.combat);
            
            #endif
            
            return false;
        }
    }

    [Serializable]
    public class CombatCameraChange : CombatFunctionWithDelay, ICombatFunction
    {
        public bool smoothed = true;
        public bool positionOverride = false;
        
        [ShowIf ("@positionOverride && string.IsNullOrEmpty (positionFromEntityName)")]
        public Vector3 position = Vector3.zero;
        
        [ShowIf ("positionOverride")]
        [LabelText ("Position From Entity")]
        public string positionFromEntityName;
        
        public bool rotationXOverride = false;
        
        [ShowIf ("rotationXOverride")]
        public float rotationX = 45f;
        
        public bool rotationYOverride = false;
        
        [ShowIf ("rotationYOverride")]
        public float rotationY = 180f;
        
        public bool zoomOverride = false;
        
        [ShowIf ("zoomOverride")]
        public float zoom = 0.5f;



        public override void Run ()
        {
            #if !PB_MODSDK
            
            base.Run ();
            
            #endif
        }
        
        [Button ("Apply"), ButtonGroup, PropertyOrder (-1), ShowIf ("IsInCombat")]
        protected override void RunDelayed ()
        {
            #if !PB_MODSDK
            
            if (!IsRunningPossible ())
                return;
            
            var cameraPos = GameCameraSystem.GetPositionTarget ();
            var cameraRotationX = GameCameraSystem.GetRotationX ();
            var cameraRotationY = GameCameraSystem.GetRotationY ();
            var cameraZoom = GameCameraSystem.GetZoomTarget ();

            if (positionOverride)
            {
                if (!string.IsNullOrEmpty (positionFromEntityName))
                {
                    var unitPersistent = Contexts.sharedInstance.persistent.GetEntityWithNameInternal (positionFromEntityName);
                    var unitCombat = IDUtility.GetLinkedCombatEntity (unitPersistent);
                    var combatView = unitCombat != null && unitCombat.hasCombatView ? unitCombat.combatView.view : null;
                    if (combatView != null)
                        cameraPos = combatView.transform.position;
                }
                else
                    cameraPos = position;
            }

            if (rotationXOverride)
                cameraRotationX = rotationX;
                
            if (rotationYOverride)
                cameraRotationY = rotationY;
                
            if (zoomOverride)
                cameraZoom = zoom;
                
            GameCameraSystem.ClearTarget ();
            GameCameraSystem.OverrideInputTargets (cameraPos, cameraRotationY, cameraRotationX, cameraZoom, !smoothed);
            Debug.Log ($"Overriding combat camera | Position: {cameraPos} | Yaw: {rotationY} | Pitch: {rotationX} | Zoom: {cameraZoom}");
            
            #endif
        }

        [Button, ButtonGroup, PropertyOrder (-1), ShowIf ("IsInCombat")]
        private void Grab ()
        {
            #if !PB_MODSDK
            
            var cameraPos = GameCameraSystem.GetPositionTarget ();
            var cameraRotationX = GameCameraSystem.GetRotationX ();
            var cameraRotationY = GameCameraSystem.GetRotationY ();
            var cameraZoom = GameCameraSystem.GetZoomTarget ();

            positionOverride = true;
            position = cameraPos;
            
            rotationXOverride = true;
            rotationX = cameraRotationX;
            
            rotationYOverride = true;
            rotationY = cameraRotationY;
            
            zoomOverride = true;
            zoom = cameraZoom;
            
            #endif
        }
    }
}