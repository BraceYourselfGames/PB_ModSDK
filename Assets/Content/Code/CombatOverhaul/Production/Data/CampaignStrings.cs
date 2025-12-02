using System.Collections.Generic;

namespace PhantomBrigade.Data
{
    public static class UnitStats
    {
        public const string power = "power";
        public const string hp = "hp";
        
        public const string frameHealth = "frame_health";
        public const string frameFractures = "frame_fractures";
        public const string salvageBonus = "salvage_bonus";
        
        public const string barrier = "barrier";
        public const string barrierRegeneration = "barrier_regeneration";
        
        public const string mass = "mass";
        public const string heatDissipation = "heat_dissipation";
        
        public const string scrapValueSupplies = "scrap_value";
        public const string scrapValueUncommon = "comp2_value";
        public const string scrapValueRare = "comp3_value";
        
        public const string activationDuration = "act_duration";
        public const string activationCount = "act_count";
        public const string activationHeat = "act_heat";
        public const string activationCharges = "act_charges";

        public const string weaponDamage = "wpn_damage";
        public const string weaponDamageRadius = "wpn_damage_radius";
        public const string weaponDamageFromMass = "wpn_damage_from_mass";
        public const string weaponDamageBuildup = "wpn_damage_buildup";
        
        public const string weaponImpact = "wpn_impact";
        public const string weaponImpactRadius = "wpn_impact_radius";
        public const string weaponConcussion = "wpn_concussion";

        public const string weaponHeat = "wpn_heat";
        public const string weaponStagger = "wpn_stagger";
        
        public const string weaponScatterAngle = "wpn_scatter_angle";
        public const string weaponScatterAngleMoving = "wpn_scatter_angle_moving";
        public const string weaponScatterRadius = "wpn_scatter_radius";
        public const string weaponRangeMin = "wpn_range_min";
        public const string weaponRangeMax = "wpn_range_max";
        public const string weaponRangeIndex = "wpn_range_index";
        public const string weaponTurnRateLimit = "wpn_turnrate_limit";
        
        public const string overheatLimit = "overheat_limit";
        public const string overheatBuffer = "overheat_buffer";
        public const string thrusterPower = "thruster_power";

        public const string weaponPenetrationCharges = "wpn_penetration_charges";
        public const string weaponPenetrationUnitCost = "wpn_penetration_unitcost";
        public const string weaponPenetrationGeomCost = "wpn_penetration_geomcost";
        public const string weaponPenetrationDamageK = "wpn_penetration_damagek";
        
        public const string weaponProjectileSpeed = "wpn_speed";
        public const string weaponProjectileLifetime = "wpn_proj_lifetime";
        public const string weaponProjectileRicochet = "wpn_proj_ricochet";
        
        public const string resistanceConcussion = "res_concussion";
        public const string resistanceHeat = "res_heat";
        public const string resistanceStagger = "res_stagger";

        public const string animRotationSpeedPrimary = "anim_rotation_speed_primary";
        public const string animRotationSpeedSecondary = "anim_rotation_speed_secondary";
        public const string animRotationLimitYaw = "anim_rotation_limit_yaw";
        public const string animRotationLimitPitch = "anim_rotation_limit_pitch";
        
        public const string weaponDamageOffset = "wpn_damage_offset";
    }

    
    public static class UnitStatsDerived
    {
        public const string pmr = "drv_pmr";
        public const string speed = "drv_speed";
        public const string dashDistance = "drv_dash_distance";
        public const string ehp = "drv_ehp";
        public const string weightPoints = "drv_weight_points";
        public const string weightClass = "drv_weight_class";
        public const string rangeClassPrimary = "drv_range_class_primary";
        public const string rlp = "drv_rlp";
    }
    
    public static class UnitStatsNormalized
    {
        public const string ehp = "ehp";
        public const string minHpNormalized = "min_hp_normalized";
        
        public const string speed = "speed";
        public const string mass = "mass";
        
        public const string weightClass = "weight_class";
        
        public const string heat = "heat";
        public const string concussion = "concussion";
    }
}

namespace PhantomBrigade
{    
    public static class NavigationTags
    {
        public const int standard = 0;
        public const int transitional = 1;
    }

    public static class Factions
    {
        public const string player = "Phantoms";
        public const string enemy = "Invaders";

        private static List<string> factions = new List<string> { player, enemy };
        
        public static List<string> GetList ()
        {
            return factions;
        }
    }

    public static class LocationTypes
    {
        public static string city = "city";
        public static string militaryBase = "militaryBase";
        
        public static string patrol = "patrol";
        
        public static string battleSite = "battleSite";
        public static string capsuleLanding = "capsuleLanding";

        public static List<string> values = new List<string>
        {
            city,
            militaryBase,
            patrol,
            battleSite,
            capsuleLanding
        };
    }

    public static class LoadoutSockets
    {
        public const string corePart = "core";
        public const string secondaryPart = "secondary";
        public const string leftOptionalPart = "optional_left";
        public const string rightOptionalPart = "optional_right";
        public const string leftEquipment = "equipment_left";
        public const string rightEquipment = "equipment_right";
        public const string back = "back";
    }
    
    public static class UnitBlueprintKeys
    {
        public static string mech = "unit_mech";
        public static string tankElevated = "unit_tank_elevated";
        public static string tankStandard = "unit_tank_modular";
        public static string turret = "unit_turret";
        public static string system = "unit_system";
    }
    
    public static class UnitClassKeys
    {
        public static string mech = "mech";
        public static string tank = "tank";
        public static string turret = "turret";
        public static string system = "system";
    }

    public static class HitDirections
    {
        public static List<string> directions = new List<string> {front,back,left,right};

        public const string front = "front";
        public const string back = "back";
        public const string left = "left";
        public const string right = "right";        
    }
}



