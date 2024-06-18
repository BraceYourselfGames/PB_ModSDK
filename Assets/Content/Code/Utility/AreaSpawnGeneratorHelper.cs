using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class AreaSpawnGeneratorHelper : MonoBehaviour
{
    [Serializable]
    public class SpawnLink
    {
        public string name;
        public string preset;
        public string suffix;
        
        public Transform transform;
        public Vector3 linkPos;
        public Vector2 linkPosFlat;

        [ListDrawerSettings (DefaultExpandedState = false)]
        [NonSerialized, ShowInInspector]
        public List<SpawnPointCandidate> pointCandidates;
        
        [ListDrawerSettings (DefaultExpandedState = false)]
        [NonSerialized, ShowInInspector]
        public List<SpawnPointCandidate> pointsFinal;
    }
    
    [Serializable]
    public class SpawnPointCandidate
    {
        public Vector3 position;
        public float distance;
    }
    
    [Serializable]
    public class SpawnGroupPreset
    {
        public string key;
        
        [ValueDropdown ("@DataShortcuts.sim.combatSpawnTags")]
        public List<string> tags;
    }
    
    [Serializable]
    public class DirectionalTag
    {
        [HideLabel]
        [ValueDropdown ("@DataShortcuts.sim.combatSpawnTags")]
        public string key;
        
        [InlineButton ("Shift45Neg", "R -45")]
        [InlineButton ("Shift45Pos", "R +45")]
        public int angle = 0;
        
        [InlineButton ("Resize10Neg", "W -10")]
        [InlineButton ("Resize10Pos", "W +10")]
        public int arc = 40;
        
        private void Shift45Neg () => Shift (-45);
        private void Shift45Pos () => Shift (45);
        
        private void Shift (int value)
        {
            angle = angle.OffsetAndWrap (value, -360, 360);
        }
        
        private void Resize10Neg () => Resize (-10);
        private void Resize10Pos () => Resize (10);
        
        private void Resize (int value)
        {
            arc = arc + value;
        }
    }
    
    public float distanceThreshold = 9f;
    public int pointsPerGroup = 5;

    public Transform transformNavigationValidation;
    
    [ListDrawerSettings (DefaultExpandedState = false)]
    public List<SpawnGroupPreset> presets;
    
    [ListDrawerSettings (DefaultExpandedState = false)]
    public List<SpawnLink> links;

    public List<DirectionalTag> tagsDirectional;

    [Button, PropertyOrder (-1)]
    private void FillLinks ()
    {
        var children = transform.FindChildrenDeep ("Cube", false);
        links.Clear ();
        foreach (var child in children)
        {
            links.Add (new SpawnLink
            {
                name = child.parent.name,
                transform = child,
                pointCandidates = new List<SpawnPointCandidate> (),
                pointsFinal = new List<SpawnPointCandidate> (),
                linkPos = child.position,
                linkPosFlat = child.position.Flatten2D (),
            });
        }

        SortLinks ();
        FillPresets ();
    }
    
    [Button, PropertyOrder (-1)]
    private void SortLinks ()
    {
        links.Sort ((x, y) => x.name.CompareTo (y.name));
    }
    
    [Button, PropertyOrder (-1)]
    private void FillPresets ()
    {
        if (links == null || presets == null)
            return;
        
        foreach (var link in links)
        {
            foreach (var preset in presets)
            {
                if (link.name.Contains (preset.key))
                {
                    link.preset = preset.key;
                    break;
                }
            }

            int iop = link.name.IndexOf (link.preset);
            int lop = link.preset.Length;
            int offset = iop + lop;
            link.suffix = link.name.Substring (offset, link.name.Length - offset);
        }
    }
}
