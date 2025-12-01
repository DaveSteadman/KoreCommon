
using KoreCommon;

namespace KoreSim;

public static class KoreCommonCommands
{
    // Usage: KoreCommonCommands.RegisterCommands(console)
    public static void RegisterCommands(KoreCommandHandler console)
    {

        // Register commands and their handlers here
        KoreCentralLog.AddEntry("KoreCommandHandler: Initializing common commands...");

        // General app control commands
        console.AddCommandHandler(new KoreCommandFileRename());
        console.AddCommandHandler(new KoreCommandCommonUnitTest());
    }
}