using SkiaSharp;

namespace KorePlotter.NatoSymbol;

// --------------------------------------------------------------------------------------------
// MARK: Basic NATO Symbol Enums and Types
// --------------------------------------------------------------------------------------------

// Usage: var x = NatoSymbolAffiliation.Friendly;
public enum NatoSymbolAffiliation
{
    Pending,
    Unknown,
    Suspect,
    Hostile,
    Neutral,
    AssumedFriend,
    Friend
}

public enum NatoSymbolDomain
{
    Air,
    Space,
    Land,
    SeaSurface,
    SeaSubsurface,
    Equipment,
    Installation,
    Activity
}

public enum NatoPlatformFunction
{
    Military,
    Civilian,
    MilitaryFixedWing,
    CivilianFixedWing,
    MilitaryRotaryWing,
    CivilianRotaryWing,
    MilitaryBalloon,
    CivilianBalloon,
    MilitaryAirship,
    CivilianAirship,
    UnmannedAerialVehicle,
    AirDecoy,
    MedicalEvacuation,
    Attackstrike,
    Bomber,
    Cargo,
    Fighter,
    JammerEcm,
    Tanker,
    Patrol,
    Reconnaissance,
    Trainer,
    Utility,
    VSTOL,
    AirborneCommandPost,
    AirborneEarlyWarning,
    AntisurfaceWarfare,
    AntisubmarineWarfare,
    Communications,
    CombatSearchAndRescue,
    ElectronicSupportMeasures,
    Government,
    MineCountermeasures,
    PersonnelRecovery,
    Passenger,
    SearchAndRescue,
    SupressionOfEnemyAirDefence,
    SpecialOperationsForces,
    UltraLight,
    Vip
}

public enum DrawMode
{
    Fill,
    Stroke,
    FillAndStroke,
}


public static class NatoSymbolUtils
{
    // Usage: string domainName = NatoSymbolUtils.NatoSymbolDomainToString(domain);
    public static string NatoSymbolDomainToString(NatoSymbolDomain domain)
    {
        return domain switch
        {
            NatoSymbolDomain.Air           => "Air",
            NatoSymbolDomain.Space         => "Space",
            NatoSymbolDomain.Land          => "Land",
            NatoSymbolDomain.SeaSurface    => "Sea Surface",
            NatoSymbolDomain.SeaSubsurface => "Sea Subsurface",
            NatoSymbolDomain.Equipment     => "Equipment",
            NatoSymbolDomain.Installation  => "Installation",
            NatoSymbolDomain.Activity      => "Activity",
            _                              => "Invalid"
        };
    }
}
