// <fileheader>

using System.Collections.Generic;
using System.Text;

namespace KoreCommon;

#nullable enable

public class KoreCommandLogAdd : KoreCommand
{
    public KoreCommandLogAdd()
    {
        Signature.Add("log");
        Signature.Add("add");
    }

    public override string HelpString => $"{SignatureString} <message>";

    public override string Execute(List<string> parameters)
    {
        StringBuilder sb = new();

        if (parameters.Count == 0)
        {
            return "KoreCommandLogAdd: No message provided. Usage: " + HelpString;
        }

        // Join all parameters into a single message
        string message = string.Join(" ", parameters);

        // Add the message to the central log
        KoreCentralLog.AddEntry(message);

        sb.AppendLine($"KoreCommandLogAdd: Message added to log: {message}");

        return sb.ToString();
    }
}
