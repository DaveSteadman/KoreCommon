
using KoreCommon;


namespace KoreGIS;

public static class KoreGISCommands
{

    // Usage: KoreGISCommands.RegisterCommands(console)
    public static void RegisterCommands(KoreCommandHandler console)
    {

        // Register commands and their handlers here
        KoreCentralLog.AddEntry("KoreGISCommands: Initializing commands...");

        // MapServer
        console.AddCommandHandler(new KoreCommandElePrep());
        console.AddCommandHandler(new KoreCommandEleLoadArc());
        console.AddCommandHandler(new KoreCommandEleSaveTile());
        console.AddCommandHandler(new KoreCommandEleSaveTileSet());
        console.AddCommandHandler(new KoreCommandEleLoadTile());
        console.AddCommandHandler(new KoreCommandEleForPos());
        console.AddCommandHandler(new KoreCommandEleReport());
        console.AddCommandHandler(new KoreCommandElePatchLoad());
        console.AddCommandHandler(new KoreCommandElePatchSave());

        // Tile Images
        console.AddCommandHandler(new KoreCommandSatCollate());
        console.AddCommandHandler(new KoreCommandSatDivide());
        console.AddCommandHandler(new KoreCommandSatDivideTo());


    }
}