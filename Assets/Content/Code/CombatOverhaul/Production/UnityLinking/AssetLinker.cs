using System;
using System.Collections.Generic;
using PhantomBrigade;
using PhantomBrigade.Data;
using Sirenix.OdinInspector;
using UnityEngine;

public class AssetLinker : MonoBehaviour
{
    [Serializable]
    public struct GraphicsSetting
    {
        public GameObject[] forceOff;
        public ParticleSystem[] disableParticleNoise;
        [HideInInspector] 
        public bool[] particleSystemNoiseDefault;
    }

    [Header ("Graphics Settings")]
    public bool multipleGraphicsLevels = false;

    [ShowInInspector] [ShowIf ("multipleGraphicsLevels")]
    public GraphicsSetting lowGraphicsOverrides = new GraphicsSetting ();

    [ShowInInspector] [ShowIf ("multipleGraphicsLevels")]
    public GraphicsSetting medGraphicsOverrides = new GraphicsSetting ();

    [Header ("Systems")]
    public FXTween fxTween;
    public FXHelperProjectile fxHelperProjectile;
    public FXHelperBeam fxHelperBeam;

    public ParticleSystem particleSystem;

    [InfoBox("These systems will have their collision forced on for regular gameplay, off for replays")]
    public ParticleSystem[] collisionSystems;

    public ParticleSystem[] noiseSystems;

    [Header ("Activation")]
    public ProjectileVisualsController projectileVisualsController;
    
    public bool activatedAtRoot = false;

    public List<GameObject> activatedChildren;
}
