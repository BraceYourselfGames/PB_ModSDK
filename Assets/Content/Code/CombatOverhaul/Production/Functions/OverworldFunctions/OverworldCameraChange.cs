using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PhantomBrigade.Functions
{
    [Serializable]
    public class OverworldCameraChange : IOverworldFunction
    {
        public float delay = 0f;
        
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

        [Button, PropertyOrder (-1), ShowIf ("IsGrabAvailable")]
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

        private bool IsGrabAvailable ()
        {
            #if !PB_MODSDK
            
            return Application.isPlaying && IDUtility.IsGameState (GameStates.overworld);
            
            #else
            return false;
            #endif
        }

        public void Run ()
        {
            #if !PB_MODSDK
            
            var delaySafe = Mathf.Clamp (delay, 0f, 60f);
            if (delaySafe <= 0f)
                RunDelayed ();
            else
                Co.Delay (delaySafe, RunDelayed);
            
            #endif
        }
        
        private void RunDelayed ()
        {
            #if !PB_MODSDK
            
            // Since this might have been delayed, we need some safety checks
            if (!IDUtility.IsGameLoaded () || !IDUtility.IsGameState (GameStates.overworld))
                return;
            
            var game = Contexts.sharedInstance.game;
            if (game.isCutsceneInProgress)
                return;

            var cameraPos = GameCameraSystem.GetPositionTarget ();
            var cameraRotationX = GameCameraSystem.GetRotationX ();
            var cameraRotationY = GameCameraSystem.GetRotationY ();
            var cameraZoom = GameCameraSystem.GetZoomTarget ();

            if (positionOverride)
            {
                if (!string.IsNullOrEmpty (positionFromEntityName))
                {
                    var sitePersistent = Contexts.sharedInstance.persistent.GetEntityWithNameInternal (positionFromEntityName);
                    var siteOverworld = IDUtility.GetLinkedOverworldEntity (sitePersistent);
                    if (siteOverworld != null && siteOverworld.hasPosition)
                        cameraPos = siteOverworld.position.v;
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
            GameCameraSystem.OverrideInputTargets (cameraPos, cameraRotationY, cameraRotationX, cameraZoom);
            
            #endif
        }
    }
}