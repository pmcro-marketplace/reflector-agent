using A2A;
using Microsoft.Agents.AI;

const string Correlation =
    "PMCRO_CORRELATION {\"workspace_id\":\"workspace-proof-001\",\"trail_id\":\"trail-proof-001\",\"cycle_id\":\"cycle-proof-001\",\"a2a_context_id\":\"context-proof-001\"}";

var expectCorrelation = args.Any(arg => arg == "--correlation");
var baseUrl = args.FirstOrDefault(arg => !arg.StartsWith("--", StringComparison.Ordinal))
    ?? "http://127.0.0.1:5095";

var resolver = new A2ACardResolver(new Uri(baseUrl));
AIAgent remoteAgent = await resolver.GetAIAgentAsync();

var request = expectCorrelation
    ? Correlation
    : "PMCR-O Reflector A2A transport proof. Do not execute reflector.";

AgentResponse response = await remoteAgent.RunAsync(request);

if (!response.Text.StartsWith("NOT_INVOKED:", StringComparison.Ordinal))
{
    Console.Error.WriteLine("FAIL unexpected A2A response: " + response.Text);
    return 1;
}

if (expectCorrelation)
{
    if (!response.Text.Contains(Correlation, StringComparison.Ordinal))
    {
        Console.Error.WriteLine("FAIL PMCR-O correlation envelope changed or was dropped.");
        Console.Error.WriteLine(response.Text);
        return 2;
    }
    Console.WriteLine("PASS PMCR-O correlation preservation");
}
else
{
    Console.WriteLine("PASS A2A discovery");
    Console.WriteLine("PASS A2A message transport");
}

Console.WriteLine(response.Text);
return 0;
