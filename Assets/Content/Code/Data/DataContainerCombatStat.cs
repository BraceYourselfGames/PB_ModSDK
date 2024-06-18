using System;
using Sirenix.OdinInspector;
using UnityEngine;
using YamlDotNet.Serialization;

namespace PhantomBrigade.Data
{
    public static class CombatStatKeys
    {
        public const string Duration = "duration";
        public const string ActionsPlanned = "actions_planned";
        public const string DamageInflictedPerSecond = "damage_inflicted_per_second";
        public const string DamageInflictedTotal = "damage_inflicted_total";
        public const string DamageTakenTotal = "damage_taken_total";
        public const string SalvageableParts = "loot_parts";
        public const string SalvageableSupplies = "loot_supplies";
        public const string SalvageableSubsystems = "loot_subsystems";
        public const string MoveDistance = "move_distance";
        
        public const string FriendlyUnitsLost = "friendly_units_lost";
        public const string FriendlyUnitsCount = "friendly_units_count";
        public const string FriendlyUnitsLevel = "friendly_units_level";
        public const string FriendlyUnitsThreat = "friendly_units_threat";
        
        public const string EnemyUnitsLost = "enemy_units_lost";
        public const string EnemyUnitsCount = "enemy_units_count";
        public const string EnemyUnitsLevel = "enemy_units_level";
        public const string EnemyUnitsThreat = "enemy_units_threat";
        
        public const string LevelPointsDestroyed = "level_points_destroyed";
        public const string EnemyUnitsAvoided = "enemy_units_avoided";
        
        //Hit rate keys
        public const string HitRateFriendlyIntended = "friendly_intended_hit_rate";
        public const string HitRateFriendlyUnintended = "friendly_unintended_hit_rate";
        public const string HitRateEnemyIntended = "enemy_intended_hit_rate";
        public const string HitRateEnemyUnintended = "enemy_unintended_hit_rate";
        
        //Projectile keys
        public const string ProjectilesFired = "projectiles_fired";
        public const string ProjectilesIntended = "projectile_intended";
        public const string ProjectilesUnintended = "projectile_unintended";
    }
    
    public struct DataBlockCombatStat
    {
        public string qualifier;
        public float value;
    }
    
    [Serializable]
    public class DataContainerCombatStat : DataContainerWithText
    {
        [YamlIgnore, LabelText ("Header")]
        public string textName;
        
        [YamlIgnore, LabelText ("Subtitle")]
        public string textSubtitle;
        
        [YamlIgnore, LabelText ("Tooltip"), TextArea (1, 10)]
        public string textTooltip;
        
        public string textSuffix;
        public string format = "0.##";
        public string icon;
        
        [Range (1f, 100f)]
        public float valueMultiplier = 1f;
        public bool positive;
        
        public override void ResolveText ()
        {
            textName = DataManagerText.GetText (TextLibs.combatStats, $"{key}__header");
            textSubtitle = DataManagerText.GetText (TextLibs.combatStats, $"{key}__subtitle");
            textTooltip = DataManagerText.GetText (TextLibs.combatStats, $"{key}__tooltip");
        }

        #if UNITY_EDITOR 

        public override void SaveText ()
        {
            if (!IsTextSavingPossible ())
                return;

            DataManagerText.TryAddingTextToLibrary (TextLibs.combatStats, $"{key}__header", textName);
            DataManagerText.TryAddingTextToLibrary (TextLibs.combatStats, $"{key}__subtitle", textSubtitle);
            DataManagerText.TryAddingTextToLibrary (TextLibs.combatStats, $"{key}__tooltip", textTooltip);
        }

        #endif
    }
}

