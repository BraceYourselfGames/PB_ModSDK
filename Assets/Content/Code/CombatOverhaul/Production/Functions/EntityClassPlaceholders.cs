public class PersistentEntity { }
public class OverworldEntity { }
public class OverworldActionEntity { }
public class CombatEntity { }
public class ActionEntity { }
public class EquipmentEntity { }

namespace PhantomBrigade.Game
{
    namespace Components { }
    namespace Systems { }
}

namespace PhantomBrigade.Persistent
{
    namespace Components { }
    namespace Systems { }
}

namespace PhantomBrigade.Overworld
{
    namespace Components { }
    namespace Systems { }
}

namespace PhantomBrigade.Input
{
    namespace Components { }
    namespace Systems { }
}

namespace PhantomBrigade.Combat
{
    namespace Components { }
    namespace Systems { }
}

namespace PhantomBrigade.AI
{
    namespace Components { }
    namespace Systems { }
    namespace BT { }
}

namespace PhantomBrigade.Equipment
{
    namespace Components { }
    namespace Systems { }
}

namespace PhantomBrigade.DebugConsole { }
namespace PhantomBrigade.Linking { }

public enum CombatUIModes
{
    Simulating = 0,
    Unit_Selection = 1,
    Path_Drawing = 2,
    Wait_Drawing = 3,
    Time_Placement = 4,
    Targeting_Units = 5,
    Targeting_Locations = 6,
    AI_Planning = 50,
    Intermission = 60,
    Replay = 90,
    End = 100
}

public class StartEndModifier
{
    public enum Exactness
    {
        SnapToNode,
        Original,
        Interpolate,
        ClosestOnNode,
        NodeConnection
    }
}

public enum UnitStatusSource
{
    Unknown,
    Function,
    Equipment,
    Action,
    Overheat,
    Hit
}

public enum UnitFactionFilter
{
    Any,
    Allies,
    Hostiles
}

public class PilotIdentification
{
    public int callsignIndex;
    public string callsignOverride;

    public int nameIndexPrimary;
    public int nameIndexSecondary;
    public string nameOverride;
}

public enum CombatOutcome
{
    Victory = 0,
    Defeat = 1
}

public sealed class MusicReactiveModifier
{
    public enum Mode
    {
        Absolute,
        Relative
    }

    public Mode mode;
    public int value;
    public int numberOfTurns;
    public bool pauseEvaluation;
}

public enum TargetSortMode
{
    None,
    Distance,
    DistanceInv,
    Dot,
    DotInv
}

public enum ScenarioStateRefreshContext
{
    None              = 0,
    OnStepEntry       = 1,
    OnExecutionStart  = 2,
    OnExecutionEnd    = 4,
    OnActionCreated   = 8,
    OnActionDestroyed = 16,
    OnUnitDisabled = 32,
    OnPartDisabled = 64,
    OnLocationContact = 128,
    OnFieldContact = 256,
    OnLevelDestruction = 512
}
