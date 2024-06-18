using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PhantomBrigade.Data
{
    [Serializable, HideReferenceObjectPicker]
    public class DataBlockOverworldSpawningSubcheckTag
    {
        [ValueDropdown ("@DataMultiLinkerOverworldEntityBlueprint.tags")] [InlineButton ("Invert", "@GetInversionLabel ()"), GUIColor ("GetInversionColor")]
        public string tag;

        [HideInInspector] public bool not;

        #if UNITY_EDITOR
                private void Invert () => not = !not;
                private string GetInversionLabel () => not ? "Prohibited" : "Required";
                private Color GetInversionColor () => Color.HSVToRGB (not ? 0f : 0.55f, 0.5f, 1f);
        #endif
    }

    [Serializable, HideReferenceObjectPicker]
    public class DataBlockInvestigationSquad : DataContainer
    {
        [ListDrawerSettings (AlwaysAddDefaultValue = true, DefaultExpandedState = true, ShowPaging = false)]
        public List<DataBlockOverworldSpawningSubcheckTag> originLocationTags = new List<DataBlockOverworldSpawningSubcheckTag> ();

        public float maximumDistance = 300f;

        [DictionaryDrawerSettings (KeyColumnWidth = DataEditor.dictionaryKeyWidth)]
        public Dictionary<string, bool> investigationSquadTags = new Dictionary<string, bool> ();
    }

    [Serializable, HideReferenceObjectPicker]
    public class DataBlockAttackSquad : DataContainer
    {
        [ListDrawerSettings (AlwaysAddDefaultValue = true, DefaultExpandedState = true, ShowPaging = false)]
        public List<DataBlockOverworldSpawningSubcheckTag> originLocationTags = new List<DataBlockOverworldSpawningSubcheckTag> ();

        [ListDrawerSettings (AlwaysAddDefaultValue = true, DefaultExpandedState = true, ShowPaging = false)]
        public List<DataBlockOverworldSpawningSubcheckTag> destinationLocationTags = new List<DataBlockOverworldSpawningSubcheckTag> ();

        [DictionaryDrawerSettings (KeyColumnWidth = DataEditor.dictionaryKeyWidth)]
        public Dictionary<string, bool> attackSquadTags = new Dictionary<string, bool> ();
    }

    [Serializable, HideReferenceObjectPicker]
    public class DataBlockOverworldConvoy : DataContainer
    {
        [ListDrawerSettings (AlwaysAddDefaultValue = true, DefaultExpandedState = true, ShowPaging = false)]
        public List<DataBlockOverworldSpawningSubcheckTag> startLocationTags = new List<DataBlockOverworldSpawningSubcheckTag> ();

        [ListDrawerSettings (AlwaysAddDefaultValue = true, DefaultExpandedState = true, ShowPaging = false)]
        public List<DataBlockOverworldSpawningSubcheckTag> destinationLocationTags = new List<DataBlockOverworldSpawningSubcheckTag> ();

        [DictionaryDrawerSettings (KeyColumnWidth = DataEditor.dictionaryKeyWidth)]
        public Dictionary<string, bool> convoyTags = new Dictionary<string, bool> ();

        public int minEscalationLevel = 0;
        public int maxEscalationLevel = 3;
        public float selectionWeight = 1.0f;
        
        [DictionaryDrawerSettings (KeyColumnWidth = DataEditor.dictionaryKeyWidth)]
        public Dictionary<int, float> selectionWeightPerLevel = new Dictionary<int, float>();
    }

    [Serializable, HideReferenceObjectPicker]
    public class DataBlockOverworldPatrol : DataContainer
    {
        [ListDrawerSettings (AlwaysAddDefaultValue = true, DefaultExpandedState = true, ShowPaging = false)]
        public List<DataBlockOverworldSpawningSubcheckTag> spawnLocationTags = new List<DataBlockOverworldSpawningSubcheckTag> ();

        [DictionaryDrawerSettings (KeyColumnWidth = DataEditor.dictionaryKeyWidth)]
        public Dictionary<string, bool> patrolTags;

        public int minEscalationLevel = 0;
        public int maxEscalationLevel = 3;
        public float selectionWeight = 1.0f;
        
        [DictionaryDrawerSettings (KeyColumnWidth = DataEditor.dictionaryKeyWidth)]
        public Dictionary<int, float> selectionWeightPerLevel = new Dictionary<int, float>();
    }
}