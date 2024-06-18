using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CoroutineID
{
    public const string stateEntry = "stateEntry";
    public const string stateExit = "stateExit";
}

public class SceneCoroutineRunner : MonoBehaviour
{
    private static SceneCoroutineRunner ins;
    private static Dictionary<int, Coroutine> coroutinesByInt;
    private static Dictionary<string, Coroutine> coroutinesByString;

    private void Awake ()
    {
        ins = this;
    }

    public static void StartCoroutine (int id, IEnumerator enumerator)
    {
        if (ins == null)
            return;
        
        StopExistingCoroutine (id);
        var coroutine = ins.StartCoroutine (enumerator);
        if (coroutinesByInt == null)
            coroutinesByInt = new Dictionary<int, Coroutine> ();
        coroutinesByInt.Add (id, coroutine);
    }

    public static void StopExistingCoroutine (int id)
    {
        if (ins == null || coroutinesByInt == null || !coroutinesByInt.ContainsKey (id))
            return;

        // Debug.Log ($"Stopping preexisting coroutine {id}");
        ins.StopCoroutine (coroutinesByInt[id]);
        coroutinesByInt.Remove (id);
    }
    
    public static void ClearExistingCoroutine (int id)
    {
        if (ins == null || coroutinesByInt == null || !coroutinesByInt.ContainsKey (id))
            return;

        // Debug.Log ($"Clearing coroutine entry {id}");
        coroutinesByInt.Remove (id);
    }
    
    public static void StartCoroutine (string id, IEnumerator enumerator)
    {
        if (ins == null)
            return;
        
        StopExistingCoroutine (id);
        var coroutine = ins.StartCoroutine (enumerator);
        if (coroutinesByString == null)
            coroutinesByString = new Dictionary<string, Coroutine> ();
        coroutinesByString.Add (id, coroutine);
    }

    public static void StopExistingCoroutine (string id)
    {
        if (ins == null || coroutinesByString == null || !coroutinesByString.ContainsKey (id))
            return;

        // Debug.Log ($"Stopping preexisting coroutine {id}");
        ins.StopCoroutine (coroutinesByString[id]);
        coroutinesByString.Remove (id);
    }
    
    public static void ClearExistingCoroutine (string id)
    {
        if (ins == null || coroutinesByString == null || !coroutinesByString.ContainsKey (id))
            return;

        // Debug.Log ($"Clearing coroutine entry {id}");
        coroutinesByString.Remove (id);
    }
}
