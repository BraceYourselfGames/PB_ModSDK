using System;
using System.Collections.Generic;
using UnityEngine;

using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

using CustomRendering;

namespace Area
{
    public class AreaPropPrototypeData
    {
        public int id;
        public string name;

        public AreaProp prefab;
        public Collider prefabCollider;
        public List<AreaPropSubObject> subObjects;
    }

    public struct AreaPropSubObject
    {
        public string name;
        public int contextIndex;
        public long modelID;
        public float3 position;
        public quaternion rotation;
        public float3 scale;
        public int customTransformIndex;
    }

    public class AreaPropState
    {
        public float4 cachedMaterial_Scale = new float4 (1f, 1f, 1f, 1f); // Used for mirroring in the shader
        public float4 cachedMaterial_HSBPrimaryAndEmission = new float4 (0f, 0.5f, 0.5f, 0f);
        public float4 cachedMaterial_HSBSecondary = new float4 (0f, 0.5f, 0.5f, 0f);

        public AreaPropTransform cachedTransformOriginal;

        public float3 cachedRootPosition = new float3 (1f, 1f, 1f);
        public quaternion cachedRootRotation = quaternion.identity;

        public Quaternion destructionRotation;
        public Vector3 destructionOffset;

        public List<AreaPropTransformChild> cachedChildTransformsOriginal;
    }

    public struct AreaPropTransform
    {
        public float3 position;
        public quaternion rotation;
        public float3 scale;

        public AreaPropTransform (Vector3 position, Quaternion rotation, Vector3 scale)
        {
            this.position = position;
            this.rotation = rotation;
            this.scale = scale;
        }
    }

    public struct AreaPropTransformChild
    {
        public int index;
        public float3 position;
        public quaternion rotation;
        public float3 scale;

        public AreaPropTransformChild (int index, Vector3 position, Quaternion rotation, Vector3 scale)
        {
            this.index = index;
            this.position = position;
            this.rotation = rotation;
            this.scale = scale;
        }
    }

    [Serializable]
    public class AreaPlacementProp
    {
        public int id;
        public int pivotIndex;
		public Vector3Int clipboardPosition; //only used for copy/paste
        public byte rotation;
        public bool flipped;
        public float offsetX;
        public float offsetZ;

        public Vector4 hsbPrimary = new Vector4 (0.5f, 0.5f, 0.5f, 0f);
        public Vector4 hsbSecondary = new Vector4 (0.5f, 0.5f, 0.5f, 0f);

        public AreaPropPrototypeData prototype;
        public AreaPropState state;

        public bool destroyed = false;
        public float destructionTime = -100f;

        public bool removed = false;
        public float removalTime = -100f;

        public bool revealed = false;
        public float revealTime = -100f;

        public float animatedTimeLast = -100f;

        public Collider instanceCollision;
        public Entity entityRoot;
        public List<Entity> entitiesChildren;










        public AreaPlacementProp ()
        {
            state = new AreaPropState ();
        }

		public AreaPlacementProp SimpleClone()
		{
			var clone = (AreaPlacementProp)MemberwiseClone();

			clone.state = null;
			clone.instanceCollision = null;
			clone.entityRoot = Entity.Null;
			clone.entitiesChildren = null;

			return clone;
		}

        public void Setup (AreaManager am, AreaVolumePoint point)
        {
            if (point == null || entitiesChildren == null)
                return;

            var rootRotation = Quaternion.Euler (new Vector3 (0f, -90f * rotation, 0f));
            if (prototype.prefab.rotateRandomly)
            {
                if (prototype.prefab.useAllAxesForRandomRotation)
                    rootRotation = Quaternion.Euler
                        (
                        UnityEngine.Random.Range (prototype.prefab.randomRotationMin, prototype.prefab.randomRotationMax),
                        -90f * rotation + UnityEngine.Random.Range (prototype.prefab.randomRotationMin, prototype.prefab.randomRotationMax),
                        UnityEngine.Random.Range (prototype.prefab.randomRotationMin, prototype.prefab.randomRotationMax)
                        );
                else
                    rootRotation = Quaternion.Euler (0f, -90f * rotation + UnityEngine.Random.Range (prototype.prefab.randomRotationMin, prototype.prefab.randomRotationMax), 0f);
            }

            var rootPosition = point.instancePosition;
            if (prototype.prefab.allowPositionOffset)
                rootPosition += AreaUtility.GetPropOffsetAsVector (this, rootRotation);

            var rootScale = Vector3.one;
            if (prototype.prefab.scaleRandomly)
                rootScale = Vector3.Lerp (prototype.prefab.scaleMin, prototype.prefab.scaleMax, UnityEngine.Random.Range (0f, 1f));

            // Cache for editor use without slow ECS polling
            state.cachedTransformOriginal.position = rootPosition;
            state.cachedTransformOriginal.rotation = rootRotation;
            state.cachedTransformOriginal.scale = rootScale;

            state.cachedRootPosition = rootPosition;
            state.cachedRootRotation = rootRotation;

            var scale =
            (
                prototype.prefab.mirrorOnZAxis ?
                new float4 (1f, 1f, flipped ? -1f : 1f, 1f) :
                new float4 (flipped ? -1f : 1f, 1f, 1f, 1f)
            );

            state.cachedMaterial_Scale = scale;

            bool night = false;
            var sky = SkyManagerSimple.ins;
            if (sky != null && sky.IsNight ())
                night = true;

            var hsb1 = prototype.prefab.allowTinting ? hsbPrimary : Constants.defaultHSBOffset;
            hsb1.w = !night && prototype.prefab.activeLightsOnlyAtNight ? 0f : 1f;
            var hsb2 = prototype.prefab.allowTinting ? hsbSecondary : Constants.defaultHSBOffset;

            var entityManager = AreaManager.world.EntityManager;
            for (int i = 0; i < prototype.subObjects.Count; ++i)
            {
                var entityChild = entitiesChildren[i];
                var subObject = prototype.subObjects[i];
                var rendererInfo = prototype.prefab.renderers[subObject.contextIndex];

                // This is 0 or higher if original prefab says a child is randomly offset, rotated or scaled
                if (subObject.customTransformIndex >= 0)
                {
                    var childTransform = new AreaPropTransformChild (i, subObject.position, subObject.rotation, subObject.scale);

                    if (rendererInfo.offsetRandomly)
                    {
                        childTransform.position += new float3
                        (
                            UnityEngine.Random.Range (-rendererInfo.offsetScale.x, rendererInfo.offsetScale.x),
                            UnityEngine.Random.Range (-rendererInfo.offsetScale.y, rendererInfo.offsetScale.y),
                            UnityEngine.Random.Range (-rendererInfo.offsetScale.z, rendererInfo.offsetScale.z)
                        );
                    }

                    if (rendererInfo.rotateRandomly)
                    {
                        childTransform.rotation = Quaternion.Euler
                        (
                            0f,
                            -90f * rotation + UnityEngine.Random.Range (rendererInfo.randomRotationMin, rendererInfo.randomRotationMax),
                            0f
                        );
                    }

                    if (rendererInfo.scaleRandomly)
                    {
                        childTransform.scale = Vector3.Lerp
                        (
                            rendererInfo.scaleMin,
                            rendererInfo.scaleMax,
                            UnityEngine.Random.Range (0f, 1f)
                        );
                    }

                    if (state.cachedChildTransformsOriginal == null)
                        state.cachedChildTransformsOriginal = new List<AreaPropTransformChild> { childTransform };
                    else
                        state.cachedChildTransformsOriginal.Add (childTransform);
                }

                var rendererMode = rendererInfo.mode;
                Vector4 packedValues;

                if (rendererMode == AreaProp.RendererMode.ActiveWhenDestroyed)
                    packedValues = new Vector4 (0f, 0f, 1f, 1f);
                else
                    packedValues = new Vector4 (1f, 0f, 1f, 1f);

                entityManager.SetComponentData (entityChild, new ScaleShaderProperty { property = new HalfVector4(scale) });
                entityManager.SetComponentData (entityChild, new HSBOffsetProperty{ property = new HalfVector8(new HalfVector4(hsb1), new HalfVector4(hsb2))});
                entityManager.SetComponentData (entityChild, new PackedPropShaderProperty { property = new HalfVector4(packedValues) });

                AreaManager.MarkEntityDirty (entityChild);
            }

            ApplyCachedTransformations ();

            UtilityECS.ScheduleUpdate ();
        }



        public void UpdateOffsets (AreaManager am)
        {
            if (!AreaManager.IsECSSafe ())
                return;

            if (entitiesChildren == null || !pivotIndex.IsValidIndex (am.points))
                return;

            Vector3 rootPosition = am.points[pivotIndex].instancePosition;
            if (prototype.prefab.allowPositionOffset)
                rootPosition += AreaUtility.GetPropOffsetAsVector (this, state.cachedRootRotation);

            var entityManager = AreaManager.world.EntityManager;
            entityManager.SetComponentData (entityRoot, new Translation { Value = rootPosition });
            state.cachedRootPosition = rootPosition;
        }

        public void ApplyCachedTransformations ()
        {
            ApplyCachedRootTransformations ();

            var entityManager = AreaManager.world.EntityManager;
            for (int i = 0; i < prototype.subObjects.Count; ++i)
            {
                var subObject = prototype.subObjects[i];
                Entity entityChild = entitiesChildren[i];

                var position = subObject.position;
                var rotation = subObject.rotation;
                var scale = subObject.scale;

                if (subObject.customTransformIndex >= 0)
                {
                    var transformData = state.cachedChildTransformsOriginal[subObject.customTransformIndex];
                    position = transformData.position;
                    rotation = transformData.rotation;
                    scale = transformData.scale;
                }

                if (entityManager.HasComponent (entityChild, typeof(Translation)))
                    entityManager.SetComponentData (entityChild, new Translation { Value = position });
                else
                    entityManager.AddComponentData (entityChild, new Translation { Value = position });

                if (entityManager.HasComponent (entityChild, typeof(Rotation)))
                    entityManager.SetComponentData (entityChild, new Rotation { Value = rotation });
                else
                    entityManager.AddComponentData (entityChild, new Rotation { Value = rotation });

                if (entityManager.HasComponent (entityChild, typeof(NonUniformScale)))
                    entityManager.SetComponentData (entityChild, new NonUniformScale { Value = scale });
                else
                    entityManager.AddComponentData (entityChild, new NonUniformScale { Value = scale });
            }
        }

        private void ApplyCachedRootTransformations ()
        {
            ApplyRootTransformations (state.cachedRootPosition, state.cachedRootRotation, state.cachedTransformOriginal.scale);
        }

        private void ApplyRootTransformations (Vector3 rootPosition, Quaternion rootRotation, Vector3 rootScale)
        {
            var entityManager = AreaManager.world.EntityManager;
            if (!entityManager.Exists (entityRoot))
                return;

            if (instanceCollision != null && instanceCollision.enabled)
            {
                var t = instanceCollision.transform;
                t.localPosition = rootPosition;
                t.localRotation = rootRotation;
                t.localScale = rootScale;
            }

            if (entityManager.HasComponent (entityRoot, typeof(Translation)))
                entityManager.SetComponentData (entityRoot, new Translation {Value = rootPosition});
            else
                entityManager.AddComponentData (entityRoot, new Translation {Value = rootPosition});

            if (entityManager.HasComponent (entityRoot, typeof(Rotation)))
                entityManager.SetComponentData (entityRoot, new Rotation {Value = rootRotation});
            else
                entityManager.AddComponentData (entityRoot, new Rotation {Value = rootRotation});

            if (entityManager.HasComponent (entityRoot, typeof(NonUniformScale)))
                entityManager.SetComponentData (entityRoot, new NonUniformScale {Value = rootScale});
            else
                entityManager.AddComponentData (entityRoot, new NonUniformScale {Value = rootScale});

            // state.cachedMaterial_PackedPropData.w = rootPosition.y + 0.05f;
        }

        public void OverrideAndApplyTransformations (Vector3 rootPosition, Quaternion rootRotation, Vector3 rootScale)
        {
            if (!AreaManager.IsECSSafe ())
                return;

            state.cachedTransformOriginal.position = rootPosition;
            state.cachedTransformOriginal.rotation = rootRotation;
            state.cachedTransformOriginal.scale = rootScale;

            state.cachedRootPosition = rootPosition;
            state.cachedRootRotation = rootRotation;

            ApplyRootTransformations (rootPosition, rootRotation, rootScale);
        }




        public void UpdateMaterial_HSBOffsets (Vector4 primary, Vector4 secondary)
        {
            if (entitiesChildren == null)
                return;

            hsbPrimary = primary;
            hsbSecondary = secondary;

            state.cachedMaterial_HSBPrimaryAndEmission = new float4 (primary.x, primary.y, primary.z, hsbPrimary.w);
            state.cachedMaterial_HSBSecondary = secondary;
            ApplyPropertiesToRenderers (AreaProp.PropertyTarget.All);
        }

        public void UpdateMaterial_Emission (float emissionIntensity)
        {
            if (entitiesChildren == null)
                return;

            state.cachedMaterial_HSBPrimaryAndEmission = new float4
            (
                state.cachedMaterial_HSBPrimaryAndEmission.x,
                state.cachedMaterial_HSBPrimaryAndEmission.y,
                state.cachedMaterial_HSBPrimaryAndEmission.z,
                emissionIntensity
            );
            ApplyPropertiesToRenderers (AreaProp.PropertyTarget.All);
        }




        /*
        public void UpdateInputs (float destruction, float removal, float reveal)
        {
            destruction = Mathf.Clamp01 (destruction);
            removal = Mathf.Clamp01 (removal);
            reveal = Mathf.Clamp01 (reveal);
            float visibility = reveal * (1f - removal);

            var destructionType = prototype.prefab.destructionType;
            if (destructionType == AreaProp.DestructionType.FallAnimated)
            {
                float rotationInterpolant =
                    prototype.prefab.destructionFallCurveCustom ?
                    prototype.prefab.destructionFallCurve.Evaluate (destruction) :
                    AreaAnimationSystem.propFallCurve.Evaluate (destruction);

                rotationInterpolant = Mathf.Clamp01 (rotationInterpolant);
                if (float.IsNaN (rotationInterpolant))
                    rotationInterpolant = 0f;

                state.cachedRootRotation = Quaternion.Lerp (state.destructionRotationFrom, state.destructionRotationTo, rotationInterpolant);
                state.cachedRootPosition = Vector3.Lerp (state.destructionPositionFrom, state.destructionPositionTo, destruction);
                ApplyCachedTransformations ();

                // Fade out all renderers for intact state
                state.cachedMaterial_PackedPropData = new Vector4 (visibility * (1f - destruction), removal, state.cachedMaterial_PackedPropData.z, reveal);
                ApplyPropertiesToRenderers (AreaProp.PropertyTarget.OnlyIntact);

                // Fade in all renderers only visible when destroyed
                state.cachedMaterial_PackedPropData = new Vector4 (visibility * destruction, removal, state.cachedMaterial_PackedPropData.z, reveal);
                ApplyPropertiesToRenderers (AreaProp.PropertyTarget.OnlyDestroyed);
            }
            else
            {
                // Fade out and explode all renderers for intact state
                state.cachedMaterial_PackedPropData = new Vector4 (visibility, destruction, state.cachedMaterial_PackedPropData.z, reveal);
                ApplyPropertiesToRenderers (AreaProp.PropertyTarget.OnlyIntact);

                // Fade in all renderers only visible when destroyed
                state.cachedMaterial_PackedPropData = new Vector4 (visibility * destruction, removal, state.cachedMaterial_PackedPropData.z, reveal);
                ApplyPropertiesToRenderers (AreaProp.PropertyTarget.OnlyDestroyed);
            }
        }
        */

        public void UpdatePackedPropertiesForTime (float timeRequested, bool conservativeSampling)
        {
            if (entitiesChildren == null)
                return;

            // No point animating if this prop has never been modified from default state
            if (!destroyed && !removed && !revealed)
                return;

            // No point animating if we already animated to this time
            // This makes a safe assumption state didn't change without time changing, but that's a fairly safe assumption
            if (conservativeSampling && animatedTimeLast.RoughlyEqual (timeRequested, 0.0005f))
                return;

            animatedTimeLast = timeRequested;

            bool updateTransformations = false;
            var positionOffset = Vector3.zero;
            var rotationModified = state.cachedTransformOriginal.rotation;

            float reveal = 1f;
            float revealInv = 0f;

            if (revealed)
            {
                float timeSinceReveal = timeRequested - revealTime;
                float duration = 2f; // Make this data driven later, if necessary
                reveal = Mathf.Clamp01 (timeSinceReveal / duration);
                revealInv = 1f - reveal;

                positionOffset += prototype.prefab.offsetReveal * Mathf.Pow (revealInv, 4); // for smooth stop
                updateTransformations = true;
            }

            float destruction = 0f;
            float destructionInv = 1f;

            if (destroyed && destructionTime < timeRequested)
            {
                float timeSinceDestruction = timeRequested - destructionTime;
                float duration = Mathf.Max (0.01f, prototype.prefab.destructionDuration);
                destruction = Mathf.Clamp01 (timeSinceDestruction / duration);
                destructionInv = 1f - destruction;

                var destructionType = prototype.prefab.destructionType;
                if (destructionType == AreaProp.DestructionType.FallAnimated || destructionType == AreaProp.DestructionType.FallSimulated)
                {
                    float fallProgress = Mathf.Clamp01 (timeSinceDestruction / Mathf.Max (0.01f, prototype.prefab.animatedFallDuration));
                    float rotationInterpolant =
                        prototype.prefab.destructionFallCurveCustom ?
                        prototype.prefab.destructionFallCurve.Evaluate (fallProgress) :
                        AreaAnimationSystem.propFallCurve.Evaluate (fallProgress);

                    rotationInterpolant = Mathf.Clamp01 (rotationInterpolant);
                    if (float.IsNaN (rotationInterpolant))
                        rotationInterpolant = 0f;

                    rotationModified = Quaternion.Lerp (state.cachedTransformOriginal.rotation, state.destructionRotation, rotationInterpolant);
                    positionOffset += state.destructionOffset * (1f - Mathf.Pow (destructionInv, 4)); // for smooth stop
                    updateTransformations = true;
                }
            }

            float removal = 0f;
            float removalInv = 1f;

            if (removed && removalTime < timeRequested)
            {
                float timeSinceRemoval = timeRequested - removalTime;
                float duration = Mathf.Max (0.01f, prototype.prefab.destructionDuration);
                removal = Mathf.Clamp01 (timeSinceRemoval / duration);
                removalInv = 1f - removal;

                positionOffset += prototype.prefab.offsetRemoval * Mathf.Pow (removal, 4); // for smooth start
                updateTransformations = true;
            }

            if (removed || destroyed || revealed)
                updateTransformations = true;

            if (updateTransformations)
            {
                state.cachedRootPosition = state.cachedTransformOriginal.position + new float3 (positionOffset);
                state.cachedRootRotation = rotationModified;

                ApplyCachedTransformations ();
            }

            var entityManager = AreaManager.world.EntityManager;
            for (int i = 0; i < prototype.subObjects.Count; ++i)
            {
                // No point working with any child entities that don't have this component (there shouldn't be any, but worth verifying just in case)
                var entityChild = entitiesChildren[i];
                if (!entityManager.HasComponent<PackedPropShaderProperty> (entityChild))
                    continue;

                var subObject = prototype.subObjects[i];
                var rendererInfo = prototype.prefab.renderers[subObject.contextIndex];
                var rendererMode = rendererInfo.mode;
                HalfVector4 halfValues;

                // Next, set up channels of packed property
                // If a given effect shows up when input reaches 0, multiple factors can be combined via multiplication
                // If a given effect shows up when input reaches 1, multiple factors can be combined via addition

                if (rendererMode == AreaProp.RendererMode.ActiveWhenIntact)
                {
                    halfValues = new HalfVector4
                    (
                        reveal * destructionInv * removalInv,          // approach 0 on: reveal 0, destruction 1 or removal 1
                        Mathf.Min (1f, destruction + removal),   // approach 1 on: destruction 1 or removal 1
                        1f,
                        reveal * removalInv                            // approach 0 on: reveal 0 or removal 1
                    );
                }
                else if (rendererMode == AreaProp.RendererMode.ActiveWhenDestroyed)
                {
                    halfValues = new HalfVector4
                    (
                        reveal * destruction * removalInv,             // approach 0 on: reveal 0, destruction 0 or removal 1
                        removal,                                       // approach 1 on: removal 1
                        1f,
                        reveal * removalInv                            // approach 0 on: reveal 0 or removal 1
                    );
                }
                else
                {
                    halfValues = new HalfVector4
                    (
                        reveal * removalInv,                           // approach 0 on: reveal 0 or removal 1
                        removal,                                       // approach 1 on: removal 1
                        1f,
                        reveal * removalInv                            // approach 0 on: reveal 0 or removal 1
                    );
                }

                var packedValuesCurrent = entityManager.GetComponentData<PackedPropShaderProperty> (entityChild).property;

                // Vector4 == operator returns true if the magnitude of the difference is less than 1e-5, which is perfect for our needs here
                if (packedValuesCurrent == halfValues)
                    continue;

                entityManager.SetComponentData (entityChild, new PackedPropShaderProperty {property = halfValues});
                AreaManager.MarkEntityDirty (entityChild);
            }

            UtilityECS.ScheduleUpdate ();
        }

        private void ApplyPropertiesToRenderers (AreaProp.PropertyTarget target)
        {
            if (!AreaManager.IsECSSafe ())
                return;

            if (entitiesChildren == null)
                return;

            var entityManager = AreaManager.world.EntityManager;
            for (int i = 0; i < prototype.subObjects.Count; ++i)
            {
                var subObject = prototype.subObjects[i];
                var propRenderer = prototype.prefab.renderers[subObject.contextIndex];

                bool apply = target == AreaProp.PropertyTarget.All ||
                    (
                        target == AreaProp.PropertyTarget.OnlyIntact &&
                        propRenderer.mode != AreaProp.RendererMode.ActiveWhenDestroyed &&
                        propRenderer.mode != AreaProp.RendererMode.ActiveConstantly
                    ) ||
                    (
                        target == AreaProp.PropertyTarget.OnlyDestroyed &&
                        propRenderer.mode != AreaProp.RendererMode.ActiveWhenIntact &&
                        propRenderer.mode != AreaProp.RendererMode.ActiveConstantly
                    );

                if (apply)
                {
                    Entity entityChild = entitiesChildren[i];

                    if (entityManager.HasComponent<ScaleShaderProperty> (entityChild))
                        entityManager.SetComponentData (entityChild, new ScaleShaderProperty {property = new HalfVector4(state.cachedMaterial_Scale)});

                    if (entityManager.HasComponent<HSBOffsetProperty> (entityChild))
                        entityManager.SetComponentData (entityChild, new HSBOffsetProperty {property = new HalfVector8(new HalfVector4(state.cachedMaterial_HSBPrimaryAndEmission), new HalfVector4(state.cachedMaterial_HSBSecondary))});

                    if (entityManager.HasComponent<PackedPropShaderProperty> (entityChild))
                    {
                        Vector4 packedValues;
                        if (propRenderer.mode == AreaProp.RendererMode.ActiveWhenDestroyed)
                            packedValues = new Vector4 (0f, 0f, 1f, 1f);
                        else
                            packedValues = new Vector4 (1f, 0f, 1f, 1f);

                        entityManager.SetComponentData (entityChild, new PackedPropShaderProperty { property = new HalfVector4 (packedValues) });
                    }

                    AreaManager.MarkEntityDirty (entityChild);
                }
            }

            UtilityECS.ScheduleUpdate ();
        }

        /// <summary>
        /// Used for end-of-life cleanup of entities associated with a given placement.
        /// The placement itself is expected to be discarded and collected past this point.
        /// </summary>
        public void Cleanup ()
        {
            if (!AreaManager.IsECSSafe ())
                return;

            var entityManager = AreaManager.world.EntityManager;

            if (entityManager.ExistsNonNull (entityRoot))
                entityManager.DestroyEntity (entityRoot);

            if (entitiesChildren != null)
            {
                for (int c = 0; c < entitiesChildren.Count; ++c)
                {
                    if (entityManager.ExistsNonNull (entitiesChildren[c]))
                        entityManager.DestroyEntity (entitiesChildren[c]);
                }
            }
        }

        #if UNITY_EDITOR
        public void UpdateVisibility (HalfVector4 halfValues)
        {
            if (entitiesChildren == null || entitiesChildren.Count == 0)
            {
                return;
            }

            var entityManager = AreaManager.world.EntityManager;
            var count = Mathf.Min (entitiesChildren.Count, prototype.subObjects.Count);
            for (var i = 0; i < count; i += 1)
            {
                var entityChild = entitiesChildren[i];
                if (!entityManager.HasComponent<PackedPropShaderProperty> (entityChild))
                {
                    continue;
                }

                var subObject = prototype.subObjects[i];
                var rendererInfo = prototype.prefab.renderers[subObject.contextIndex];
                var rendererMode = rendererInfo.mode;
                if (rendererMode != AreaProp.RendererMode.ActiveConstantly
                    && rendererMode != AreaProp.RendererMode.ActiveWhenIntact)
                {
                    continue;
                }

                var packedValuesCurrent = entityManager.GetComponentData<PackedPropShaderProperty> (entityChild).property;
                if (packedValuesCurrent == halfValues)
                {
                    continue;
                }

                entityManager.SetComponentData (entityChild, new PackedPropShaderProperty {property = halfValues});
                AreaManager.MarkEntityDirty (entityChild);
            }
            UtilityECS.ScheduleUpdate ();
        }

        public void UpdateVisibilityWithECS (bool visible, ComponentType componentTypeModel)
        {
            if (entitiesChildren == null || entitiesChildren.Count == 0)
            {
                return;
            }

            var entityManager = AreaManager.world.EntityManager;
            var count = Mathf.Min (entitiesChildren.Count, prototype.subObjects.Count);
            for (var i = 0; i < count; i += 1)
            {
                var entityChild = entitiesChildren[i];
                var hasModel = entityManager.HasComponent (entityChild, componentTypeModel);
                if (visible && hasModel)
                {
                    continue;
                }
                if (!visible && !hasModel)
                {
                    continue;
                }
                if (visible)
                {
                    var subObject = prototype.subObjects[i];
                    entityManager.AddSharedComponentData (entityChild, AreaAssetHelper.GetInstancedModel (subObject.modelID));
                }
                else
                {
                    entityManager.RemoveComponent (entityChild, componentTypeModel);
                }
                AreaManager.MarkEntityDirty (entityChild);
            }
            UtilityECS.ScheduleUpdate ();
        }
        #endif
    }
}
