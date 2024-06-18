using UnityEngine;

namespace PhantomBrigade.Data.UI
{
    [ExecuteInEditMode]
    public class DataLinkerUI : DataLinker<DataContainerUI>
    {
        public static Texture2D GetTexture (string key)
        {
            return 
                data != null && 
                data.textures != null && 
                !string.IsNullOrEmpty (key) && 
                data.textures.ContainsKey (key) ? data.textures[key].texture : null;
        }

        /*

        private void MoveUnitInfo (UnitWidgetConfig config, string socket, int index)
        {
            var bar = config.bars[index];
            var labelPos = index.IsValidIndex (config.blueprintLabelPositions) ? config.blueprintLabelPositions[index] : Vector2.zero;
            var spriteName = index.IsValidIndex (config.blueprintSpriteNames) ? config.blueprintSpriteNames[index] : string.Empty;
            
            var block = new UnitWidgetSocket ();
            block.barPositionStandard = bar.positionStandard;
            block.barPositionTarget = bar.positionTarget;
            block.barTooltipKey = bar.tooltipKey;
            block.barTooltipHeader = bar.tooltipHeader;
            block.barTooltipContent = bar.tooltipContent;
            block.blueprintLabelPosition = labelPos;
            block.blueprintSpriteName = spriteName;
            
            config.sockets.Add (socket, block);
        }
        
        */
    }
}


