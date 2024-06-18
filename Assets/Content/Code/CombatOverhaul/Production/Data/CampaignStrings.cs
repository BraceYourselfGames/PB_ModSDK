using System.Collections.Generic;

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

    public static class PartGroups
    {
        public const string structure = "structure";
        public const string defensive = "equipment_defensive";
        public const string weaponMelee = "equipment_wpn_melee";
        public const string weaponRanged = "equipment_wpn_ranged";
    }

    public static class HitDirections
    {
        public static List<string> directions = new List<string> {front,back,left,right};

        public const string front = "front";
        public const string back = "back";
        public const string left = "left";
        public const string right = "right";        
    }
    
    [System.Serializable]
    public class PilotData
    {
        public string name;
        public string givenName;
        public string familyName;
    }
}



