using UnityEngine;
using Unity.Entities;
using System.Reflection;

public static class UtilityECS
{
    public static bool ExistsNonNull (this EntityManager entityManager, Entity entity)
    {
        if (entity == Entity.Null)
            return false;
        else
            return entityManager.Exists (entity);
    }

    #if UNITY_EDITOR
    private static bool updateScheduled = false;
    private static bool updateInitialized = false;
    
    private static void CheckForScheduledUpdates ()
    {
        if (updateScheduled)
        {
            updateScheduled = false;
            UnityEditor.EditorApplication.QueuePlayerLoopUpdate ();
            // UnityEditor.SceneView.RepaintAll ();
            // UnityEditor.EditorWindow view = UnityEditor.EditorWindow.GetWindow<UnityEditor.SceneView>();
            // view.Repaint();
        }
    }
    #endif

    public static void ScheduleUpdate ()
    {
        #if UNITY_EDITOR
        
        if (updateScheduled)
            return;
        
        if (!updateInitialized)
        {
            updateInitialized = true;
            UnityEditor.EditorApplication.update -= CheckForScheduledUpdates;
            UnityEditor.EditorApplication.update += CheckForScheduledUpdates;
        }
        updateScheduled = true;
        #endif
    }

    private static string worldInitializationMethodName = "Unity.Entities.DefaultWorldInitialization";
    private static MethodInfo worldInitializationMethodInfo = null;
    private static bool log = false;

    public static bool IsSafeToUseWorld ()
    {
        #if UNITY_EDITOR
        if (World.DefaultGameObjectInjectionWorld == null)
        {
            if (UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode)
            {
                if (log)
                    Debug.LogWarning ("IsSafeToUseWorld | No world detected, but it's not safe to use this due to EditorApplication.isPlayingOrWillChangePlaymode");
                return false;
            }
            else
            {
                if (worldInitializationMethodInfo == null)
                {
                    var assembly = typeof (GameObjectEntity).Assembly;
                    if (log)
                        Debug.LogWarning ("IsSafeToUseWorld | No world detected and no init method found, searching assembly " + assembly.FullName);

                    var type = assembly.GetType (worldInitializationMethodName);
                    if (type != null)
                    {
                        if (log)
                            Debug.LogWarning ("IsSafeToUseWorld | Type found, trying to get the init method");
                        worldInitializationMethodInfo = type.GetMethod ("Initialize", BindingFlags.Static | BindingFlags.Public);
                    }
                    else if (log)
                        Debug.LogWarning ("IsSafeToUseWorld | Type not found: " + worldInitializationMethodName);
                }

                if (worldInitializationMethodInfo != null)
                {
                    if (log)
                        Debug.LogWarning ("IsSafeToUseWorld | No world was previously found, creating one");
                    worldInitializationMethodInfo.Invoke (obj: null, parameters: new object[] { "Editor World", true });
                    return true;
                }
                else
                {
                    if (log)
                        Debug.LogWarning ("IsSafeToUseWorld | Init method is unavailable, not safe to use world");
                    return false;
                }

                // DefaultWorldInitialization.Initialize ("Editor World", true);
            }
        }
        else
            return true;
        #else
        return true;
        #endif
    }
    
    public static bool IsApplicationNotPlaying ()
    {
        #if UNITY_EDITOR
        return !UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode && !Application.isPlaying;
        #else
        return false;
        #endif
    }

    public static World GetOrCreateWorld ()
    {
        #if UNITY_EDITOR
        if (World.DefaultGameObjectInjectionWorld == null)
        {
            if (UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode)
            {
                if (log)
                    Debug.LogWarning ("GetOrCreateWorld | No world detected, but it's not safe to use this due to EditorApplication.isPlayingOrWillChangePlaymode");
                return null;
            }
            else
            {
                if (worldInitializationMethodInfo == null)
                {
                    var assembly = typeof (GameObjectEntity).Assembly;
                    if (log)
                        Debug.LogWarning ("GetOrCreateWorld | No world detected and no init method found, searching assembly " + assembly.FullName);

                    var type = assembly.GetType (worldInitializationMethodName);
                    if (type != null)
                    {
                        if (log)
                            Debug.LogWarning ("GetOrCreateWorld | Type found, trying to get the init method");
                        worldInitializationMethodInfo = type.GetMethod ("Initialize", BindingFlags.Static | BindingFlags.Public);
                    }
                    else if (log)
                        Debug.LogWarning ("GetOrCreateWorld | Type not found: " + worldInitializationMethodName);
                }

                if (worldInitializationMethodInfo != null)
                {
                    if (log)
                        Debug.LogWarning ("GetOrCreateWorld | No world was previously found, creating one");
                    worldInitializationMethodInfo.Invoke (obj: null, parameters: new object[] { "Editor World", true });
                    return World.DefaultGameObjectInjectionWorld;
                }
                else
                {
                    if (log)
                        Debug.LogWarning ("GetOrCreateWorld | Init method is unavailable, not safe to use world");
                    return null;
                }

                // DefaultWorldInitialization.Initialize ("Editor World", true);
            }
        }
        else
            return World.DefaultGameObjectInjectionWorld;
        #else
        return World.DefaultGameObjectInjectionWorld;
        #endif
    }


}
