using UnityEngine;

namespace PhantomBrigade.Data.UI
{
    public static class DataKeysEventColor
    {
        public const string Construction = "construction_primary";
        public const string FactionEnemy = "faction_enemy";
        public const string FactionFriendly = "faction_friendly";
        public const string Negative = "negative_primary";
        public const string Neutral = "neutral_primary";
        public const string Positive = "positive_primary";
        public const string Restorative = "restorative_primary";
        public const string Warning = "warning_primary";
    }
    
    public static class DataHelperUI
    {
        private static Color colorFallback = Color.white.WithAlpha (1f);
    
        private static bool IsSafe ()
        {
            return DataLinkerUI.data != null;
        }

        public static Color GetColor (string key)
        {
            if (!IsSafe ())
                return colorFallback;

            var colors = DataLinkerUI.data.colors;
            if (colors == null || string.IsNullOrEmpty (key) || !colors.ContainsKey (key))
            {
                Debug.Log ($"Failed to find UI color with key {key}");
                return colorFallback;
            }

            return colors[key];
        }


        
        public static Color GetSharedColor (string key)
        {
            var colorProfile = DataMultiLinkerUIColor.GetEntry (key);
            if (colorProfile == null)
                return colorFallback;

            return colorProfile.colorCache.colorMain;
        }

        public static string GetEscalationLevelIcon (int escalationLevel)
        {
            string spriteName = null;
            if (escalationLevel == 1)
                spriteName = "s_icon_l32_star1";
            else if (escalationLevel == 2)
                spriteName = "s_icon_l32_star2";
            else if (escalationLevel == 3)
                spriteName = "s_icon_l32_star3";
            else if (escalationLevel == 4)
                spriteName = "s_icon_l32_star4";
            else
                spriteName = "line_horizontal_4px";
            return spriteName;
        }

        public static float GetEscalationFillNormalized (float escalation, int escalationLevel)
        {
            var escalationData = DataShortcuts.escalation;
            var thresholds = escalationData.escalationThresholds;

            int minIndex = escalationLevel - 1;
            float min = minIndex.IsValidIndex (thresholds) ? thresholds[minIndex] : 0f;
        
            int maxIndex = escalationLevel;
            float max = maxIndex.IsValidIndex (thresholds) ? thresholds[maxIndex] : 1f;
        
            float escalationNormalized = Mathf.Clamp01 ((escalation - min) / (max - min));
            if (escalationLevel == thresholds.Count)
                escalationNormalized = 1f;

            return escalationNormalized;
        }
        
        public static UnitProjectionConfig GetProjectionConfig (string faction, bool selected)
        {
            if (!IsSafe ())
                return null;
            
            bool friendly = CombatUIUtility.IsFactionFriendly (faction);
            if (selected)
                return friendly ? DataLinkerUI.data.projectionFriendlySelected : DataLinkerUI.data.projectionEnemySelected;
            else
                return friendly ? DataLinkerUI.data.projectionFriendly : DataLinkerUI.data.projectionEnemy;
        }
        
        public static UnitProjectionConfig GetProjectionConfigCollision ()
        {
            if (!IsSafe ())
                return null;

            return DataLinkerUI.data.projectionCollision;
        }
        
        public static Color GetFactionColor (string key, string faction, bool selected = false)
        {
            if (!IsSafe ())
                return Color.magenta;
            
            bool friendly = CombatUIUtility.IsFactionFriendly (faction);
            return GetFactionColor (key, friendly, selected);
        }
        
        public static Color GetFactionColorNew (bool friendly, UIColorType type = UIColorType.Main)
        {
            if (!IsSafe ())
                return Color.magenta;

            var colorProfileKey = friendly ? "faction_friendly" : "faction_enemy";
            var colorProfile = DataMultiLinkerUIColor.GetEntry (colorProfileKey);
            if (colorProfile == null)
                return Color.magenta;

            var color = colorProfile.colorCache.GetColorByType (type);
            return color;
        }
        
        public static Color GetColorNew (string key, UIColorType type = UIColorType.Main)
        {
            if (!IsSafe ())
                return Color.magenta;
            
            var colorProfile = DataMultiLinkerUIColor.GetEntry (key);
            if (colorProfile == null)
                return Color.magenta;

            var color = colorProfile.colorCache.GetColorByType (type);
            return color;
        }

        public static Color GetFactionColor (string key, bool friendly, bool selected = false)
        {
            if (!IsSafe ())
                return Color.magenta;
            
            if (DataLinkerUI.data.factionColors == null || !DataLinkerUI.data.factionColors.ContainsKey (key))
            {
                Debug.Log ($"Failed to find UI faction color block with key {key}");
                return DataLinkerUI.data.fallbackColor;
            }
            else
            {
                var data = DataLinkerUI.data.factionColors[key];
                if (selected)
                    return friendly ? data.friendlySelected : data.enemySelected;
                else
                    return friendly ? data.friendly : data.enemy;
            }
        }
    }
}

