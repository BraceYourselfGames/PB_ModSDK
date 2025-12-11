using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PhantomBrigade.Overworld.View
{
    public class OverworldViewHelperGround : MonoBehaviour
    {
        public bool groundRoot = false;
        public float groundOffset = 0f;
        
        [InlineButton ("FillGroundedObjects", "Fill")]
        public Transform groundedHolder;
        public List<Transform> groundedObjects = new List<Transform> ();

        private void FillGroundedObjects ()
        {
            if (groundedHolder == null)
                return;
        
            groundedObjects = new List<Transform> ();
            for (int i = 0; i < groundedHolder.childCount; ++i)
            {
                var child = groundedHolder.GetChild (i);
                groundedObjects.Add (child);
            }
        }

        [Button ("Ground all", ButtonSizes.Large), PropertyOrder (-1)]
        public void GroundAll ()
        {
            if (groundedObjects == null)
            {
                Debug.LogWarning ($"Skipping grounding on {gameObject.name}");
                return;
            }

            if (groundRoot)
            {
                var positionOriginal = transform.position;
                var positionRay = new Vector3 (positionOriginal.x, positionOriginal.y + 100f, positionOriginal.z);
                if (UtilityGameObjects.RaycastInContext (gameObject, positionRay, Vector3.down, out var hit, 200f, LayerMasks.environmentMask))
                {
                    var distanceVertical = Mathf.Abs (positionOriginal.y - hit.point.y);
                    if (distanceVertical > 0.01f)
                        transform.position = new Vector3 (positionOriginal.x, hit.point.y, positionOriginal.z);
                }
            }
            
            foreach (var target in groundedObjects)
            {
                if (target == null)
                    continue;

                var positionOriginal = target.position;
                var positionRay = new Vector3 (positionOriginal.x, positionOriginal.y + 100f, positionOriginal.z);
                if (UtilityGameObjects.RaycastInContext (gameObject, positionRay, Vector3.down, out var hit, 200f, LayerMasks.environmentMask))
                {
                    var hitHeight = hit.point.y + groundOffset;
                    var distanceVertical = Mathf.Abs (positionOriginal.y - hitHeight);
                    if (distanceVertical > 0.01f)
                        target.position = new Vector3 (positionOriginal.x, hitHeight, positionOriginal.z);
                }
            }
        }
    }
}
