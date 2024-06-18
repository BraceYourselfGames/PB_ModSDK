using System;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;


[Serializable]
public class UnitRendererSocketMapped
{
    public Renderer renderer;
    public List<UnitRendererSocketMapping> socketMappings;
}

[Serializable]
public class UnitRendererSocketMapping
{
    [HorizontalGroup]
    [ValueDropdown("@DataHelperUnitEquipment.GetSockets ()")]
    public string socketName;
    
    [NonSerialized, ShowInInspector]
    [HorizontalGroup (0.2f)]
    [ReadOnly, HideLabel, HideInEditorMode]
    public int socketHashCode;
    
    [PropertyTooltip ("Vertex color channel corresponding to a given socket. Used only on objects with vehicle shader.")]
    [PropertyRange (0, 3)]
    public int targetChannel;
}

[Serializable]
public class UnitRendererOutline
{
    public Renderer renderer;
    public Material material;
}

[Serializable]
public class UnitRendererConfig
{
    public Renderer renderer;
    public List<UnitRendererSocketMapping> socketMappings;
}
