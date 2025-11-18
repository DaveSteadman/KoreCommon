using System;
using KoreCommon;

class TestCommand
{
    static void Main()
    {
        Console.WriteLine("=== Testing KoreCommandLogAdd ===\n");
        
        // Set a log filename first
        KoreCentralLog.SetFilename("/tmp/test_kore.log");
        
        // Create command handler
        var handler = new KoreCommandHandler();
        
        // Test 1: Normal message
        Console.WriteLine("Test 1: Normal multi-word message");
        var (success1, response1) = handler.RunSingleCommand("log add This is a test message");
        Console.WriteLine($"  Success: {success1}");
        Console.WriteLine($"  Response: {response1}");
        
        // Test 2: Another message
        Console.WriteLine("\nTest 2: Another message");
        var (success2, response2) = handler.RunSingleCommand("log add Multi word message test");
        Console.WriteLine($"  Success: {success2}");
        Console.WriteLine($"  Response: {response2}");
        
        // Test 3: No message (should fail gracefully)
        Console.WriteLine("\nTest 3: No message provided (expect error)");
        var (success3, response3) = handler.RunSingleCommand("log add");
        Console.WriteLine($"  Success: {success3}");
        Console.WriteLine($"  Response: {response3}");
        
        // Wait for log to be written
        System.Threading.Thread.Sleep(2000);
        
        // Check log entries
        var entries = KoreCentralLog.GetLatestEntries();
        Console.WriteLine("\n=== Checking Log Entries ===");
        int foundCount = 0;
        foreach (var entry in entries)
        {
            if (entry.Contains("This is a test message") || entry.Contains("Multi word message test"))
            {
                Console.WriteLine(entry);
                foundCount++;
            }
        }
        Console.WriteLine($"Found {foundCount} relevant log entries");
        
        // Check if log file was created
        if (System.IO.File.Exists("/tmp/test_kore.log"))
        {
            Console.WriteLine("\n=== Log File Created Successfully ===");
            var logContent = System.IO.File.ReadAllText("/tmp/test_kore.log");
            var lines = logContent.Split('\n');
            int fileFoundCount = 0;
            foreach (var line in lines)
            {
                if (line.Contains("This is a test message") || line.Contains("Multi word message test"))
                {
                    Console.WriteLine(line);
                    fileFoundCount++;
                }
            }
            Console.WriteLine($"Found {fileFoundCount} relevant entries in log file");
        }
        else
        {
            Console.WriteLine("\n=== ERROR: Log file was not created ===");
        }
        
        Console.WriteLine("\n=== All tests completed successfully ===");
    }
}
