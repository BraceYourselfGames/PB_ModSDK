

/*
[CustomEditor (typeof (PoolingHelper))]
public class PoolingHelperEditor : Editor
{
    private PoolingHelper t;
    private GameObjectPool poolCurrent;

    private float lw;
    private float fw;

    public override void OnInspectorGUI ()
    {
        t = target as PoolingHelper;

        lw = EditorGUIUtility.labelWidth;

        EditorGUIUtility.labelWidth = 60f;

        UtilityCustomInspector.DrawList (t.pools, DrawPool, AddPool, true, true, allowShifting: true);

        EditorGUIUtility.labelWidth = lw;
    }

    private void DrawPool (GameObjectPool pool)
    {
        EditorGUILayout.Space ();

        EditorGUI.BeginChangeCheck ();
        string poolName = EditorGUILayout.TextField ("Name", pool.poolName);
        if (EditorGUI.EndChangeCheck ())
        {
            Undo.RecordObject (target, "Changed pool name");
            pool.poolName = poolName;
        }

        EditorGUI.BeginChangeCheck ();
        bool allowReuse = EditorGUILayout.Toggle ("Reused", pool.allowReuse);
        if (EditorGUI.EndChangeCheck ())
        {
            Undo.RecordObject (target, "Changed pool reuse setting");
            pool.allowReuse = allowReuse;
        }

        EditorGUILayout.Space ();

        poolCurrent = pool;
        UtilityCustomInspector.DrawList (pool.pools, DrawSubPool, AddSubPool, true, true);
    }

    private void DrawSubPool (GameObjectPool.SubPool subpool)
    {
        EditorGUILayout.BeginHorizontal ();

        EditorGUI.BeginChangeCheck ();
        GameObject prefab = EditorGUILayout.ObjectField ("Prefab", subpool.prefab, typeof (GameObject), false) as GameObject;
        if (EditorGUI.EndChangeCheck ())
        {
            Undo.RecordObject (target, "Changed subpool prefab");
            subpool.prefab = prefab;
        }

        EditorGUIUtility.fieldWidth = 60f;

        EditorGUI.BeginChangeCheck ();
        int allocationCount = EditorGUILayout.IntField ("Count", subpool.allocationCount, GUILayout.Width (100f));
        if (EditorGUI.EndChangeCheck ())
        {
            Undo.RecordObject (target, "Changed subpool allocation count");
            subpool.allocationCount = allocationCount;
        }

        EditorGUIUtility.fieldWidth = fw;
        EditorGUILayout.EndHorizontal ();
    }

    private void AddPool ()
    {
        if (t == null)
            return;

        GameObjectPool pool = new GameObjectPool ();
        pool.pools = new List<GameObjectPool.SubPool> ();

        Undo.RecordObject (t, "Added new pool array element");
        t.pools.Add (pool);
    }

    private void AddSubPool ()
    {
        if (poolCurrent == null)
            return;

        Undo.RecordObject (t, "Added new subpool array element");
        poolCurrent.pools.Add (new GameObjectPool.SubPool ());
    }
}
*/