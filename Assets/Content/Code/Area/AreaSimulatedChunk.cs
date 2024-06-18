using UnityEngine;
using System.Collections.Generic;
using PhantomBrigade.Data;
using Unity.Entities;

namespace Area
{
    public class AreaSimulatedChunk : MonoBehaviour
    {
        public class DeceleratingChunk
        {
            public GameObject gameObject;
            public Vector3 velocityOnCreation;
            public float timeRemaining = 0f;
            public float timeTotal = 0f;
        }

        public Entity entityRoot;
        public List<DeceleratingChunk> deceleratingChunks;
        public Queue<GameObject> emitterCandidates;
        public GameObject emitterLast;

        public Dictionary<Collider, AreaVolumePoint> colliderToPointMap;
        public List<AreaVolumePoint> pointsAffected;
        public Rigidbody simulatedRigidbody;
        public Vector3 initialPosition;

        public int id = 0;
        public bool resetCenterOfMass = false;

        private float soundPower = 0f;
        private float timer = 0f;
        private float interval = 0.5f;
        private bool instantSoundAllowed = false;

        public bool useVerticalForce = false;
        public Vector3 positionForForce = Vector3.zero;
        public float timerForForce = 5f;
        public float durationForForce = 5f;
        public int initialPointCount = 1;

        private float limit = 0.01f;
        private int cycles = 0;
        private int cyclesToSleep = 10;

        //private List<AreaVolumePoint> pointsToAddPropsTo = null;

        public void Start ()
        {
            //AudioSystem.PlayEvent (AudioEvents.BUILDING_FALL, gameObject);
        }

        public void Update ()
        {
            timer += Time.deltaTime;
            if (timer > interval)
            {
                timer = 0f;
                if (soundPower > 0f)
                    PlaySound ();
                else
                    instantSoundAllowed = true;
            }
        }

        public void FixedUpdate ()
        {
            if (deceleratingChunks != null)
            {
                for (int i = deceleratingChunks.Count - 1; i >= 0; --i)
                {
                    DeceleratingChunk dc = deceleratingChunks[i];
                    float multiplier = dc.timeRemaining / Mathf.Max (0.01f, dc.timeTotal);
                    dc.gameObject.transform.position += dc.velocityOnCreation * multiplier * Time.fixedDeltaTime;

                    dc.timeRemaining -= Time.fixedDeltaTime;
                    if (dc.timeRemaining <= 0f)
                        deceleratingChunks.RemoveAt (i);
                }
            }

            if (useVerticalForce && timerForForce > 0f)
            {
                if (simulatedRigidbody.velocity.sqrMagnitude > limit)
                    cycles = cyclesToSleep;

                if (cycles > 0)
                {
                    cycles--;
                    Vector3 force = Mathf.Clamp01 (timerForForce / Mathf.Max (durationForForce, 0.1f)) * -Physics.gravity / 2f * Mathf.Clamp01 (colliderToPointMap.Count / Mathf.Max (1, initialPointCount));
                    simulatedRigidbody.AddForceAtPosition (force, positionForForce, ForceMode.Acceleration);
                }

                timerForForce = Mathf.Max (0, timerForForce - Time.fixedDeltaTime);
            }
        }

        public void LateUpdate ()
        {
            if (resetCenterOfMass)
            {
                resetCenterOfMass = false;
                simulatedRigidbody.ResetCenterOfMass ();
            }
        }



        private Collider r_OCE_SimulatedCollider;
        private AreaVolumePoint r_OCE_PointFromCollider;
        private AreaVolumePoint r_OCE_PointAround;
        private GameObject r_OCE_DamageObject;

        public void OnCollisionEnter (Collision collision)
        {
            ContactPoint mainContact = collision.contacts[0];
            r_OCE_SimulatedCollider = mainContact.thisCollider;

            if (colliderToPointMap.ContainsKey (r_OCE_SimulatedCollider))
            {
                r_OCE_PointFromCollider = colliderToPointMap[r_OCE_SimulatedCollider];
                
                var sceneHelper = CombatSceneHelper.ins;
                sceneHelper.areaManager.ApplyDamageToPoint (r_OCE_PointFromCollider, 1000);
                Transform parentForLeftovers = sceneHelper.areaManager.GetHolderSimulatedLeftovers ();

                r_OCE_SimulatedCollider.enabled = false;
                r_OCE_SimulatedCollider.transform.parent = parentForLeftovers;
                // r_OCE_SimulatedCollider.gameObject.SetActive (false);

                if (emitterCandidates == null)
                    emitterCandidates = new Queue<GameObject> (100);

                emitterCandidates.Enqueue (r_OCE_SimulatedCollider.gameObject);
                for (int i = 0; i < r_OCE_PointFromCollider.pointsWithSurroundingSpots.Length; ++i)
                {
                    r_OCE_PointAround = r_OCE_PointFromCollider.pointsWithSurroundingSpots[i];
                    if (r_OCE_PointAround != null && r_OCE_PointAround.spotConfigurationWithDamage == 0)
                    {
                        // This place previously enqueued demolished gameobjects for audio events
                    }
                }

                if (deceleratingChunks == null)
                    deceleratingChunks = new List<DeceleratingChunk> (colliderToPointMap.Count);

                DeceleratingChunk dc = new DeceleratingChunk ();
                dc.gameObject = r_OCE_SimulatedCollider.gameObject;
                dc.velocityOnCreation = simulatedRigidbody.velocity;
                dc.timeRemaining = 15f;
                dc.timeTotal = 15f;
                deceleratingChunks.Add (dc);

                // Debug.Log ("Adding collider " + dc.gameObject.name + " to list of decelerating objects with " + dc.gameObject.transform.childCount + " children");

                colliderToPointMap.Remove (r_OCE_SimulatedCollider);
                if (soundPower == 0f && instantSoundAllowed)
                {
                    instantSoundAllowed = false;
                    PlaySound ();
                    timer = 0f;
                }
                else
                {
                    soundPower += 10f;
                }

                // Debug.Log ("New impact, power for next audio tick now at " + power);

                if (colliderToPointMap.Count > 0)
                {
                    UpdateRigidbody ();
                    // simulatedRigidbody.AddForceAtPosition (new Vector3 (Random.Range (-0.1f, 0.1f), Random.Range (0.1f, 0.2f), Random.Range (-0.1f, 0.1f)) * colliderToPointMap.Count * Tweakables.data.areaSystem.crashBounceForceMultiplier, mainContact.point, ForceMode.Impulse);
                    positionForForce = mainContact.point;
                    timerForForce = durationForForce = 3f;
                }

                // AreaManager.instance.ApplyDamageToPosition (mainContact.point - mainContact.normal, 1000);
                // GameCalculations.DamageTilesInRange (mainContact.point - mainContact.normal, 2f, 0.25f, 0.3f, 1f);
                // GameCalculations.DamageTilesInRangeFast (mainContact.point - mainContact.normal, Tweakables.data.areaSystem.crashDamageRadius, Tweakables.data.areaSystem.crashDamageAtEpicenter, Tweakables.data.areaSystem.crashDamageAtEdge, structureCrashMask, 1f);
                /*
                GameCalculations.ExplosiveDetonation 
                (
                    mainContact.point - mainContact.normal, 
                    Tweakables.data.areaSystem.crashDamageRadius,
                    Tweakables.data.areaSystem.crashDamageAtEdge, 
                    Tweakables.data.areaSystem.crashDamageAtEpicenter, 
                    true,
                    layerMask: Constants.structureCrashMask
                );
                */

                if (colliderToPointMap.Count == 0)
                {
                    CombatSceneHelper.ins.areaManager.OnSimulatedHelperFinish (this);
                    simulatedRigidbody.isKinematic = true;
                }
            }
            else
            {
                // Debug.LogWarning ("Failed to find a collider in a map, it's hosted on object called: " + simulatedCollider.gameObject.gameObject);
            }
        }

        public void UpdateRigidbody ()
        {
            int multiplier = colliderToPointMap.Count > 0 ? colliderToPointMap.Count : 1;
            var data = DataLinkerSettingsArea.data;

            simulatedRigidbody.mass = data.blockMass * multiplier;
            simulatedRigidbody.drag = data.blockDrag * multiplier;
            simulatedRigidbody.angularDrag = data.blockAngularDrag * multiplier;
            simulatedRigidbody.ResetCenterOfMass ();
        }

        private void PlaySound ()
        {
            // AudioSystem.SetAudioParameter (AudioParameters.IMPACT_POWER, Mathf.Min (power, 100f));
            // AudioSystem.PlayEvent (AudioEvents.BUILDING_FALL_IMPACT, gameObject);

            GameObject emitter = null;
            if (emitterCandidates.Count > 0)
                emitter = emitterCandidates.Dequeue ();
            else if (emitterLast != null)
            {
                Debug.LogWarning ("AreaSimulatedChunk | PlaySound | No emitter candidates left in the queue, reusing last emitter - this may lead to jumps in power of already playing sound effects!");
                emitter = emitterLast;
            }
            else
            {
                Debug.LogWarning ("AreaSimulatedChunk | PlaySound | Forced to use simulated holder GameObject to drive sound - this may lead to jumps in power of already playing sound effects!");
                emitter = gameObject;
            }

            if (emitter != null)
            {
                emitterLast = emitter;
                //AudioSystem.PlayEventWithParamerer (AudioEvents.BUILDING_FALL_IMPACT, emitter, AudioParameters.IMPACT_POWER, Mathf.Min (soundPower, 100f));

                float soundPowerClamped = Mathf.Clamp (soundPower - 100f, 0f, 100f);
                soundPower = soundPowerClamped;
            }
            else
            {
                Debug.LogWarning ("AreaSimulatedChunk | PlaySound | No emitter found, skipping sound effects!");
            }
        }
    }
}