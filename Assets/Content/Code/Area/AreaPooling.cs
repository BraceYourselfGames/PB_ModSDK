using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Very basic GameObject pooling system tailored for needs of AreaManager
/// </summary>
namespace Area
{
    public static class AreaPooling
    {
        /// <summary>
        /// Container for a set of GameObjects, only one per a given transform is created
        /// </summary>

        public class AreaGameObjectPool
        {
            public bool justLoaded = true;
            public Transform holder;
            public GameObject[] gameObjects;
            public int increment;
            public int size;

            /// <summary>
            /// Constructor guarantees an attachment to some transform
            /// </summary>
            /// <param name="holder">Transform attached to the container, becomes a parent for all created GameObjects</param>

            public AreaGameObjectPool (Transform holder)
            {
                this.holder = holder;
                increment = 100;
                size = 0;
            }

            /// <summary>
            /// Flushes everything out
            /// </summary>

            public void Clear ()
            {
                UtilityGameObjects.ClearChildren (holder);
                gameObjects = new GameObject[0];
            }

            /// <summary>
            /// Used in AreaManager and AreaManager inspector to perform extension of a pool and first part of initialization of new objects
            /// After that happens, we get a returned integer which can be used to perform a second initialization, e.g. addition of components
            /// </summary>
            /// <returns></returns>

            public int ExtendAndReturnInitStartIndex ()
            {
                size += increment;
                if (gameObjects == null || gameObjects.Length == 0)
                {
                    // Debug.Log ("AP | Pool | ExtendAndReturnInitStartIndex | Array was null or 0 entries long, starting to fill it from 0 to size " + size);
                    gameObjects = new GameObject[size];
                    InitBasics (0);
                    return 0;
                }
                else
                {
                    // Debug.Log ("AP | Pool | ExtendAndReturnInitStartIndex | Array was " + gameObjects.Length + " entries long, expanding by " + increment);
                    GameObject[] temp = gameObjects;
                    gameObjects = new GameObject[size];
                    System.Array.Copy (temp, gameObjects, temp.Length);
                    InitBasics (temp.Length); // InitBasics (temp.Length - 1);
                    return temp.Length; // return temp.Length - 1;
                }
            }

            /// <summary>
            /// First part of initialization, sets up GameObjets, their parenting and so on, should never be called outside of ExtendAndReturnInitStartIndex
            /// </summary>
            /// <param name="start">Start index in pool GameObject array</param>

            private void InitBasics (int start)
            {
                // Debug.Log ("Initializing objects from " + start + " to " + (gameObjects.Length - 1));
                for (int i = start; i < gameObjects.Length; ++i)
                {
                    GameObject goToInit = new GameObject ();
                    goToInit.transform.parent = holder;
                    goToInit.SetActive (false);
                    gameObjects[i] = goToInit;
                }
            }

            /// <summary>
            /// Hides all objects belonging to the pool
            /// </summary>

            public void DeactivateAll ()
            {
                for (int i = 0; i < gameObjects.Length; ++i)
                {
                    if (gameObjects[i] == null)
                    {
                        Debug.LogWarning ("AP | Null reference in the object array for holder " + holder.name + ", nuking the pool");
                        Clear ();
                        return;
                    }
                    gameObjects[i].SetActive (false);
                }
            }
        }

        private static Dictionary<Transform, AreaGameObjectPool> pools = new Dictionary<Transform, AreaGameObjectPool> ();

        /// <summary>
        /// This methods fetches an existing pool container or creates a new one if nothing associated with a given transform exists
        /// </summary>
        /// <param name="holder">A transform the returned set of GameObjects should be parented to</param>
        /// <returns></returns>

        public static AreaGameObjectPool GetPool (Transform holder)
        {
            if (holder != null)
            {
                if (pools.ContainsKey (holder))
                {
                    AreaGameObjectPool pool = pools[holder];

                    if (pool.justLoaded)
                    {
                        //Debug.Log ("AP | GetPool | But somehow, this is the first call to this pool");
                        pool.justLoaded = false;
                        // pool.Clear ();
                        // GameObject.DestroyImmediate (pool.holder.gameObject);
                    }

                    return pool;
                }
                else
                {
                    AreaGameObjectPool pool = new AreaGameObjectPool (holder);
                    pool.gameObjects = new GameObject[0];
                    pools.Add (holder, pool);

                    UtilityGameObjects.ClearChildren (holder);
                    //Debug.Log ("AP | GetPool | Creating new pool attached to holder " + holder.name);
                    return pool;
                }
            }
            else
            {
                //Debug.Log ("AP | GetPool | Holder transform is null, aborting");
                return null;
            }
        }
    }
}