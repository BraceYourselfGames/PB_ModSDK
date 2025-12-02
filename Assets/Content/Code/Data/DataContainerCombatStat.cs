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
        public const string DamageInflictedTotal = "damage_inflicted_total";
        public const string DamageTakenTotal = "damage_taken_total";
        public const string MoveDistance = "move_distance";
        public const string LevelPointsDestroyed = "level_points_destroyed";

        public const string FriendlyUnitsCount = "friendly_units_count";
        public const string FriendlyUnitsLevel = "friendly_units_level";
        public const string FriendlyUnitsThreat = "friendly_units_threat";
        public const string FriendlyUnitsCrashed = "friendly_units_crashed";
        public const string FriendlyUnitsDestroyed = "friendly_units_destroyed";
        public const string FriendlyUnitsDisabled = "friendly_units_disabled";
        
        public const string FriendlyProjectiles = "friendly_projectiles_fired";
        public const string FriendlyHitRate = "friendly_hit_rate";
        public const string FriendlyHitRateAccidental = "friendly_hit_rate_accidental";
        
        public const string EnemyUnitsCount = "enemy_units_count";
        public const string EnemyUnitsLevel = "enemy_units_level";
        public const string EnemyUnitsThreat = "enemy_units_threat";
        public const string EnemyUnitsCrashed = "enemy_units_crashed";
        public const string EnemyUnitsDestroyed = "enemy_units_destroyed";
        public const string EnemyUnitsDisabled = "enemy_units_disabled";
        
        public const string EnemyProjectiles = "enemy_projectiles_fired";
        public const string EnemyHitRate = "enemy_hit_rate";
        public const string EnemyHitRateAccidental = "enemy_hit_rate_accidental";
        
        public const string UnitProjectilesFired = "unit_projectiles_fired";
        public const string UnitHitsIntended = "unit_hits_intended";
        public const string UnitHitsAccidental = "unit_hits_accidental";
        public const string UnitCrashes = "unit_crashes";
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

