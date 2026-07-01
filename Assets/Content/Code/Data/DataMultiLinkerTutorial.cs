using Sirenix.OdinInspector;
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
        
        [FoldoutGroup ("Utilities", false)]
        [Button ("Log controller text", ButtonSizes.Large), PropertyOrder (-10)]
        public void LogControllerText ()
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

                    if (page.hint != null)
                    {
                        if (page.hint.textLinkController != null)
                        {
                            var textController = page.hint.textLinkController.GetText ();
                            var textMain = page.hint.textLink.GetText ();
                            Debug.Log ($"Tutorial {kvp.Key} page {i} hint has controller text:\n- Main: {textMain}\n- Ctrl: {textController}");
                        }
                    }
                    
                    if (page.center != null)
                    {
                        if (page.center.textContentLinkController != null)
                        {
                            var textController = page.center.textContentLinkController.GetText ();
                            var textMain = page.center.textContentLink.GetText ();
                            Debug.Log ($"Tutorial {kvp.Key} page {i} center has controller text:\n- Main: {textMain}\n- Ctrl: {textController}");
                        }
                    }
                }
            }
        }
    }
}


