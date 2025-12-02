using System;
using Sirenix.OdinInspector;
using UnityEngine;
using YamlDotNet.Serialization;

namespace PhantomBrigade.Data
{
    /// <summary>
    /// Base class for all unique data containers like simulation settings, UI data containers etc.
    /// </summary>
    
    [Serializable]
    public class DataContainerUnique
    {
        public virtual void OnBeforeSerialization ()
        {

        }
        
        public virtual void OnAfterDeserialization ()
        {

        }
    }

    /// <summary>
    /// Base class for all data containers that 
    /// </summary>
    [Serializable]// [ShowIf ("IsVisibleInInspector")]
    public class DataContainer //  : ISearchFilterable
    {
        [HideInInspector, YamlIgnore] 
        public string key;
        
        // [ShowInInspector, PropertyOrder (-200), HideLabel, YamlIgnore, DisplayAsString] 
        [HideInInspector, YamlIgnore] 
        public string path;
        
        // [ShowInInspector, PropertyOrder (-200), HideLabel, YamlIgnore, DisplayAsString] 
        [HideInInspector, YamlIgnore] 
        public int index;
        
        public virtual void OnAfterDeserialization (string key)
        {
            this.key = key;
        }
        
        public virtual void OnBeforeSerialization ()
        {

        }
        
        public virtual void OnAfterSerialization ()
        {

        }

        /*
        public bool IsMatch (string searchString)
        {
            if (key != null && key.Contains (searchString))
                return true;
            return false;
        }
        */
        
        private const string keyOldIgnored = "new_00";
        
        public virtual void OnKeyReplacement (string keyOld, string keyNew)
        {
            if (Application.isPlaying)
            {
                Debug.LogError ($"Attempt to replace key of a data container {key} in play mode. This is not supported!");
                return;
            }
            
            if (keyOld == keyOldIgnored)
                return;
            
            DataLinkerHistory.RegisterKeyChange (GetType ().Name, keyOld, keyNew); 
            key = keyNew;
        }

        public override string ToString ()
        {
            return key;
        }
    }

    public class DataContainerWithText : DataContainer
    {
        public override void OnAfterDeserialization (string key)
        {
            base.OnAfterDeserialization (key);
            ResolveText ();
        }

        // [Button, ButtonGroup, PropertyOrder (-1), ShowIf (DataEditor.textAttrArg)]
        public virtual void ResolveText ()
        {
            
        }
        
        #if UNITY_EDITOR
        
        // [Button, ButtonGroup, PropertyOrder (-1), ShowIf (DataEditor.textAttrArg)]
        public virtual void SaveText ()
        {
            
        }
        
        private bool IsTextShown () =>
            DataEditor.showLibraryText;

        protected bool IsTextSavingPossible ()
        {
            if (string.IsNullOrEmpty (key))
            {
                Debug.LogWarning ("Key of this container was not set, can't generate text library key without it");
                return false;
            }

            return true;
        }

        #endif
    }
}

