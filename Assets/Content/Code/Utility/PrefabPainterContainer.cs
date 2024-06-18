using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[System.Serializable]
public class PrefabPainterEntry
{
    public GameObject prefab = null;
    public Vector2 scaleMinMax = Vector2.one;
    public bool randomRotationX = false;
    public bool randomRotationY = true;
    public bool randomRotationZ = false;
    public bool orientVertically = true;
    public bool orientWithZUp = false;

    public PrefabPainterEntry ()
    {
        scaleMinMax = new Vector2 (0.8f, 1.2f);
        randomRotationX = false;
        randomRotationY = true;
        randomRotationZ = false;
        orientVertically = true;
    }
}

[System.Serializable]
public class PrefabPainterMask
{
    public string name = "Main";
    public List<bool> toggles;

    public PrefabPainterMask ()
    {
        toggles = new List<bool> ();
    }

    public PrefabPainterMask (int size)
    {
        toggles = new List<bool> (new bool[size]);
        for (int i = 0; i < toggles.Count; ++i)
            toggles[i] = true;
    }
}


[System.Serializable]
public class PrefabPainterSettings
{
    public float brushSpacing = 0.6f;
    public float brushRadius = 0.6f;
    public int brushDensity = 1;
    public LayerMask paintMask = 1;
    public float maxYPosition = 400;
    public List<PrefabPainterEntry> entries;

    [HideInInspector]
    public List<PrefabPainterMask> masks;

    [HideInInspector]
    public int maskSelected = 0;
}

public class PrefabPainterContainer : MonoBehaviour
{
    public PrefabPainterSettings settings = new PrefabPainterSettings ();
}
