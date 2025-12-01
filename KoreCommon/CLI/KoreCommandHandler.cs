// <fileheader>

using System;
using System.Text;
using System.Threading;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Linq;

namespace KoreCommon;
using KoreCommon.UnitTest;

#nullable enable

// Class to run a thread for command line input.
// This class can't do anything, it must delgate all actual processing to the KoreEventDriver class.
public class KoreCommandHandler
{
    // Thread control
    private Thread? consoleThread = null;
    private bool running;

    // List of command handlers (with a signature) we can select and exeute.
    private readonly List<KoreCommand> commandHandlers = new List<KoreCommand>();

    // Two lists to hold input and output strings for the console.
    private KoreThreadsafeStringList InputQueue  = new KoreThreadsafeStringList();
    private KoreThreadsafeStringList OutputQueue = new KoreThreadsafeStringList();

    // Event to set on new input, to unblock the console thread to process new commands.
    private AutoResetEvent InputEvent = new AutoResetEvent(false);

    public bool IsRunning => running;

    // ---------------------------------------------------------------------------------------------
    // MARK: Constructor
    // ---------------------------------------------------------------------------------------------

    public KoreCommandHandler()
    {
        running = false;
    }

    // ---------------------------------------------------------------------------------------------
    // MARK: Thread control
    // ---------------------------------------------------------------------------------------------

    public void Start()
    {
        if (running == false)
        {
            KoreCentralLog.AddEntry($"Starting console thread... ({commandHandlers.Count} Commands.)");
            running = true;
            consoleThread = new Thread(ConsoleLoop);
            consoleThread.IsBackground = true; // Allow app to exit even if this thread is still running
            consoleThread?.Start();
        }
    }

    public void Stop()
    {
        running = false;
        consoleThread = null;
    }

    public void WaitForExit()
    {
        KoreCentralLog.AddEntry("Waiting on Join()...");
        consoleThread?.Join(); // This will block until consoleThread finishes execution
        KoreCentralLog.AddEntry("Join() returned.");
    }

    // ---------------------------------------------------------------------------------------------
    // MARK: InitializeCommands
    // ---------------------------------------------------------------------------------------------

    public void AddCommandHandler(KoreCommand command)
    {
        if (commandHandlers.Any(c => c.SignatureString == command.SignatureString))
        {
            KoreCentralLog.AddEntry($"Command {command.SignatureString} already exists.");
            return;
        }
        commandHandlers.Add(command);
        KoreCentralLog.AddEntry($"Command {command.SignatureString} added.");
    }

    // ---------------------------------------------------------------------------------------------

    private void ConsoleLoop()
    {
        KoreCentralLog.AddEntry("Console thread starting...");

        // delay for a second to allow the main thread to start up
        System.Threading.Thread.Sleep(1000);

        while (running)
        {
            // Wait for input trigger
            InputEvent.WaitOne();
            ProcessCommand();
        }
        KoreCentralLog.AddEntry("Console thread exiting...");
    }

    // ---------------------------------------------------------------------------------------------

    private void ProcessCommand()
    {
        // Loop while we have commands to process
        while (!InputQueue.IsEmpty())
        {
            // Get the string a space-delimit the parts
            string inputLine = InputQueue.RetrieveString();
            //var inputParts = inputLine.Trim().Split(' ').ToList();

            // Split the input line into parts, removing any empty entries
            var inputParts = inputLine.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList();

            // Check for internal commands - true if executed.
            if (RunInternalCommand(inputParts))
                continue;

            // Go through each of the registered command handlers looking for a match
            bool commandMatchFound = false;
            foreach (var currCmd in commandHandlers)
            {
                if (currCmd.Matches(inputParts))
                {
                    // Pass remaining parts as parameters to the command
                    string responseStr = currCmd.Execute(inputParts.Skip(currCmd.SignatureCount).ToList());
                    OutputQueue.AddString(responseStr);
                    commandMatchFound = true;
                    break; // leave the current command loop - move back out to the next input line
                }
            }
            if (!commandMatchFound)
                OutputQueue.AddString($"Command Not Found.");
        }
    }

    // ---------------------------------------------------------------------------------------------

    public (bool, string) RunSingleCommand(string commandLine)
    {
        var inputParts = commandLine.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList();

        // Check for internal commands first
        var (internalHandled, internalResponse) = TryRunInternalCommand(inputParts);
        if (internalHandled)
        {
            return (true, internalResponse);
        }

        // Go through each of the registered command handlers looking for a match
        foreach (var currCmd in commandHandlers)
        {
            if (currCmd.Matches(inputParts))
            {
                // Pass remaining parts as parameters to the command
                string responseStr = currCmd.Execute(inputParts.Skip(currCmd.SignatureCount).ToList());
                return (true, responseStr);
            }
        }
        return (false, $"Command Not Found.");
    }

    // ---------------------------------------------------------------------------------------------
    // MARK: Input
    // ---------------------------------------------------------------------------------------------

    public void AddInput(string input)
    {
        InputQueue.AddString(input);
        InputEvent.Set();
    }

    // ---------------------------------------------------------------------------------------------
    // MARK: Output
    // ---------------------------------------------------------------------------------------------

    public bool HasOutput()
    {
        return !OutputQueue.IsEmpty();
    }

    public string GetOutput()
    {
        StringBuilder output = new StringBuilder();

        while (!OutputQueue.IsEmpty())
            output.AppendLine(OutputQueue.RetrieveString());

        return output.ToString();
    }

    // ---------------------------------------------------------------------------------------------
    // Command functions:
    // ---------------------------------------------------------------------------------------------
    // - private void Cmd<Name>(string[] args)

    // New method that returns response as a tuple - used by RunSingleCommand
    private (bool handled, string response) TryRunInternalCommand(List<string> inputParts)
    {
        if (inputParts.Count == 0)
            return (false, string.Empty);

        string command = inputParts[0];

        switch (command)
        {
            case "help":
                {
                    StringBuilder helpStr = new StringBuilder();
                    helpStr.AppendLine("Available commands:");
                    foreach (var cmd in commandHandlers)
                    {
                        helpStr.AppendLine($"- {cmd.HelpString}");
                    }
                    return (true, helpStr.ToString());
                }

            case "runfile":
                {
                    if (inputParts.Count < 2)
                    {
                        return (true, "Usage: runfile <filename>");
                    }

                    string filename = inputParts[1];

                    if (!System.IO.File.Exists(filename))
                    {
                        return (true, $"File does not exist: {filename}");
                    }

                    StringBuilder result = new StringBuilder();
                    result.AppendLine($"Running file: {filename}");

                    string[] lines = System.IO.File.ReadAllLines(filename);

                    foreach (string line in lines)
                    {
                        // Queue the commands to be executed
                        InputQueue.AddString(line);

                        // trim line to 100 characters for display
                        if (line.Length > 100)
                        {
                            string shortLine = line.Substring(0, 100) + "...";
                            result.AppendLine($"FILE>> {shortLine}");
                        }
                        else
                        {
                            result.AppendLine($"FILE>> {line}");
                        }
                    }
                    return (true, result.ToString());
                }

            default:
                return (false, string.Empty);
        }
    }

    // Legacy method that uses OutputQueue - used by ProcessCommand (threaded mode)
    private bool RunInternalCommand(List<string> inputParts)
    {
        var (handled, response) = TryRunInternalCommand(inputParts);

        if (handled && !string.IsNullOrEmpty(response))
        {
            OutputQueue.AddString(response);
        }

        return handled;
    }
}

