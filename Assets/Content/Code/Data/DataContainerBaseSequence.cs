using System;
using System.Collections.Generic;
using System.Text;
using PhantomBrigade.Functions;
using Sirenix.OdinInspector;
using UnityEngine;
using YamlDotNet.Serialization;

namespace PhantomBrigade.Data
{
    public class DataBlockBaseSequenceStep : DataBlockFilterLinked<DataContainerBaseEffect>
    {
        public override IEnumerable<string> GetTags () => DataMultiLinkerBaseEffect.GetTags ();
        public override SortedDictionary<string, DataContainerBaseEffect> GetData () => DataMultiLinkerBaseEffect.data;
        protected override string GetTooltip () => "Base effect filter.";
        
        [ToggleLeft]
        public bool stopSequenceIfEmpty = false;
        
        [PropertyOrder (-10)]
        [DropdownReference]
        public List<IOverworldGlobalValidationFunction> checksGlobal;
        
        [PropertyOrder (-10)]
        [DropdownReference]
        public List<IOverworldEntityValidationFunction> checksBase;
        
        #if UNITY_EDITOR
        
        [ShowInInspector, PropertyOrder (100)]
        private DataEditor.DropdownReferenceHelper helper;
        
        public DataBlockBaseSequenceStep () => 
            helper = new DataEditor.DropdownReferenceHelper (this);

        #endif
    }

    
    public class DataContainerBaseSequence : DataContainer, IDataContainerTagged
    {
        [DropdownReference, HideLabel]
        public DataBlockComment comment;
        
        [ToggleLeft]
        public bool hidden = false;
        
        [ToggleLeft]
        public bool stopOnEmptyStep = false;
        
        [DropdownReference]
        [ValueDropdown("@DataMultiLinkerBaseSequence.tags")]
        public HashSet<string> tags = new HashSet<string> ();

        public List<DataBlockBaseSequenceStep> steps = new List<DataBlockBaseSequenceStep> ();

        public HashSet<string> GetTags (bool processed) =>
            tags;
        
        public bool IsHidden () => hidden;

        public override void OnAfterDeserialization (string key)
        {
            base.OnAfterDeserialization (key);

            if (steps != null)
            {
                foreach (var step in steps)
                {
                    if (step != null)
                        step.Refresh ();
                }
            }
        }

        #if UNITY_EDITOR
        
        [ShowInInspector, PropertyOrder (100)]
        private DataEditor.DropdownReferenceHelper helper;
        
        public DataContainerBaseSequence () => 
            helper = new DataEditor.DropdownReferenceHelper (this);

        #endif
    }
}

