using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PhantomBrigade.Data
{
    public class DataContainerPilotAppearancePreset : DataContainer
    {
        public bool usableByFriendly = true;
        public bool usableByHostile = true;
        
        [BoxGroup, HideReferenceObjectPicker, HideLabel]
        public DataBlockPilotAppearance appearance = new DataBlockPilotAppearance ();

        #if UNITY_EDITOR
        
        [ShowInInspector, PropertyOrder (100)]
        private DataEditor.DropdownReferenceHelper helper;

        public DataContainerPilotAppearancePreset () =>
            helper = new DataEditor.DropdownReferenceHelper (this);
        
        #if !PB_MODSDK
        private bool IsGrabAvailable => Application.isPlaying && CIViewBaseEditor.ins != null && CIViewBaseEditor.ins.IsEntered ();
        
        [Button (ButtonSizes.Large), ShowIf ("IsGrabAvailable"), PropertyOrder (-1), ButtonGroup]
        private void GrabFromEditor ()
        {
            var cachedAppearance = CIViewBaseEditor.ins.cachedAppearance;
            if (cachedAppearance != null)
            {
                if (appearance == null)
                    appearance = new DataBlockPilotAppearance (cachedAppearance);
                else
                    appearance.CopyFrom (cachedAppearance);
            }
        }
        
        [Button (ButtonSizes.Large), ShowIf ("IsGrabAvailable"), PropertyOrder (-1), ButtonGroup]
        private void ApplyToEditor ()
        {
            var cachedAppearance = CIViewBaseEditor.ins.cachedAppearance;
            if (appearance != null && cachedAppearance != null)
            {
                cachedAppearance.CopyFrom (appearance);
                CIViewBaseEditor.ins.RefreshView ();
            }
        }
        #endif

        #endif
    }

}