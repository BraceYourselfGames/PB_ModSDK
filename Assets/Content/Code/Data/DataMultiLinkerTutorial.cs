using UnityEngine;

namespace PhantomBrigade.Data
{
    [ExecuteInEditMode]
    public class DataMultiLinkerTutorial : DataMultiLinker<DataContainerTutorial>
    {
        public DataMultiLinkerTutorial ()
        {
            DataMultiLinkerUtility.RegisterStandardTextHandling (dataType, ref textSectorKeys, TextLibs.uiTutorials); 
        }

        /*
        [FoldoutGroup ("Utilities", false)]
        [Button ("Upgrade centers", ButtonSizes.Large), PropertyOrder (-10)]
        public void UpgradeCenters ()
        {
            foreach (var kvp in data)
            {
                var tutorial = kvp.Value;
                if (tutorial.pages == null)
                    continue;

                for (int i = 0; i < tutorial.pages.Count; ++i)
                {
                    var page = tutorial.pages[i];
                    if (page == null)
                        continue;

                    if (page.hint == null)
                    {
                        page.center = new DataBlockTutorialCenter ();
                        page.center.textImage = page.textImage;
                        page.center.textHeader = page.textHeader;
                        page.center.textContent = page.textContent;
                        
                        page.textImage = null;
                        page.textHeader = null;
                        page.textContent = null;
                    }
                }
            }
        }
        */
    }
}


