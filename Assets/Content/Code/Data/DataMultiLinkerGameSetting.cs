using System.Collections.Generic;
using UnityEngine;

namespace PhantomBrigade.Data
{
    [ExecuteInEditMode]
    public class DataMultiLinkerGameSetting : DataMultiLinker<DataContainerGameSetting>
    {
        public DataMultiLinkerGameSetting ()
        {
            textSectorKeys = new List<string> { TextLibs.uiSettings };
            DataMultiLinkerUtility.RegisterOnTextExport 
            (
                dataType, 
                () => TextLibraryHelper.OnBeforeTextExport (dataType, TextLibs.uiSettings),
                () => TextLibraryHelper.OnAfterTextExport (dataType, TextLibs.uiSettings)
            );
        }
        

        /*
        [FoldoutGroup ("Utilities", false)]
        [Button, PropertyOrder (-2)]
        public void UpgradeFormat ()
        {
            foreach (var kvp in data)
            {
                var definition = kvp.Value;
                if (definition == null)
                    continue;
                
                if (definition.type == OptionType.Bool)
                {
                    definition.levels = null;
                    definition.levelsUsed = false;
                }
                else
                {
                    var levelsOld = definition.levels;
                    var levelsNew = new SortedDictionary<int, OptionLevel> ();
                    definition.levels = levelsNew;

                    foreach (var kvp2 in levelsOld)
                    {
                        var keyOld = kvp2.Key;
                        var levelInfoOld = kvp2.Value;
                        Debug.Log ($"{definition.key} | {keyOld} | {levelInfoOld.GetValueText ()}");

                        var levelInfoNew = new OptionLevel ();
                        levelsNew.Add (keyOld, levelInfoNew);
                        
                        levelInfoNew.upgraded = true;
                        levelInfoNew.upgradedKey = keyOld;
                        levelInfoNew.textName = levelInfoOld.textName;
                        levelInfoNew.textCustom = levelInfoOld.textCustom;

                        if (definition.type == OptionType.String)
                            levelInfoNew.valueRaw = (levelInfoOld as OptionLevelString)?.value;
                        else if (definition.type == OptionType.Float)
                            levelInfoNew.valueRaw = (levelInfoOld as OptionLevelFloat)?.value.ToString();
                        else if (definition.type == OptionType.Integer)
                            levelInfoNew.valueRaw = (levelInfoOld as OptionLevelInteger)?.value.ToString();
                        else if (definition.type == OptionType.Vector2Int)
                        {
                            var s = (levelInfoOld as OptionLevelVector2Int);
                            if (s != null)
                                levelInfoNew.valueRaw = $"({s.width}, {s.height})";
                        }
                    }
                }
            }
        }
        */
    }
}


