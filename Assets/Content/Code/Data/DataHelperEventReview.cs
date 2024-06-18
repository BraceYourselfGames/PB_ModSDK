using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace PhantomBrigade.Data
{
    public class DataBlockStepLink
    {
        [HideInInspector]
        public string eventKey;
        
        [HideInInspector]
        public string stepKey;
        
        [HideLabel, Header ("@eventKey"), DisplayAsString]
        public string stepText;

        [BoxGroup, HideLabel, HideReferenceObjectPicker, HideDuplicateReferenceBox]
        public DataBlockOverworldEventStep step;
    }
    
    [ExecuteInEditMode]
    public class DataHelperEventReview : MonoBehaviour
    {
        [FoldoutGroup ("View options", true), ShowInInspector, HideLabel]
        public DataMultiLinkerOverworldEvent.Presentation presentation = new DataMultiLinkerOverworldEvent.Presentation ();
        
        [HideLabel, OnValueChanged ("UpdateSelectionFromIndex")]
        public int index = 0;
        
        [ShowIf ("IsLoaded")]
        [ShowInInspector, PropertyOrder (21)]
        [BoxGroup, HideLabel, HideReferenceObjectPicker, HideDuplicateReferenceBox]
        public static DataBlockStepLink stepLinkSelected;
        
        private static List<DataBlockStepLink> stepLinks = new List<DataBlockStepLink> ();
        
        private static List<DataBlockStepLink> stepLinksFiltered = new List<DataBlockStepLink> ();

        [PropertyOrder (-10)]
        [ButtonGroup ("Linker"), Button]
        private void Load ()
        {
            stepLinks.Clear ();
            
            var data = DataMultiLinkerOverworldEvent.data;
            foreach (var kvp in data)
            {
                var eventKey = kvp.Key;
                var eventData = kvp.Value;

                if (eventData.hidden || eventData.steps == null || eventData.steps.Count == 0)
                    continue;

                int stepCount = eventData.steps.Count;
                int stepIndex = 1;
                foreach (var kvp2 in eventData.steps)
                {
                    var stepKey = kvp2.Key;
                    var step = kvp2.Value;
                    
                    stepLinks.Add (new DataBlockStepLink
                    {
                        eventKey = eventKey,
                        stepKey = stepKey,
                        stepText = $"{stepIndex}/{stepCount}: {stepKey}",
                        step = step
                    });

                    stepIndex += 1;
                }
            }

            UpdateSelectionFromIndex ();
        }
        
        [PropertyOrder (-10)]
        [ButtonGroup ("Linker"), Button]
        private void Save ()
        {
            DataMultiLinkerOverworldEvent.SaveData ();
        }

        private void UpdateSelectionFromIndex ()
        {
            if (stepLinks.Count == 0)
            {
                stepLinkSelected = null;
                return;
            }
            
            if (index < 0)
                index = stepLinks.Count - 1;

            if (index >= stepLinks.Count)
                index = 0;

            stepLinkSelected = stepLinks[index];
        }
        
        private bool IsLoaded => stepLinks != null && stepLinks.Count > 0;

        [BoxGroup ("bgHeader", false)]
        [ShowIf ("IsLoaded")]
        [PropertyOrder (10)]
        [HorizontalGroup ("bgHeader/hg", 48f)]
        [Button (SdfIconType.ArrowLeftSquare, IconAlignment.LeftEdge, ButtonHeight = 48)]
        private void SelectPrev ()
        {
            index -= 1;
            UpdateSelectionFromIndex ();
        }

        [BoxGroup ("bgHeader")]
        [ShowIf ("IsLoaded")]
        [PropertyOrder (12)]
        [HorizontalGroup ("bgHeader/hg")]
        [Button ("@GetHeaderText ()", ButtonSizes.Large, ButtonHeight = 48)]
        [GUIColor ("GetCurrentStepColor")]
        private void HeaderButton ()
        {
            if (stepLinkSelected != null && stepLinkSelected.step != null)
                stepLinkSelected.step.reviewed = !stepLinkSelected.step.reviewed;
        }
        
        [BoxGroup ("bgHeader")]
        [ShowIf ("IsLoaded")]
        [PropertyOrder (12)]
        [HorizontalGroup ("bgHeader/hg", 48f)]
        [Button (SdfIconType.ArrowRightSquare, IconAlignment.RightEdge, ButtonHeight = 48)]
        private void SelectNext ()
        {
            index += 1;
            UpdateSelectionFromIndex ();
        }

        private Color GetCurrentStepColor ()
        {
            if (stepLinkSelected == null || stepLinkSelected.step == null)
                return new Color (1f, 1f, 1f, 1f);
            else
                return Color.HSVToRGB (stepLinkSelected.step.reviewed ? 0.3f : 0f, 0.5f, 1f).WithAlpha (1f);
        }

        private string GetHeaderText ()
        {
            bool reviewed = stepLinkSelected != null && stepLinkSelected.step != null && stepLinkSelected.step.reviewed;
            string result = $"{index} / {stepLinks.Count}\n{(reviewed ? "Reviewed" : "Not reviewed")}";
            return result;
        }
    }
}


