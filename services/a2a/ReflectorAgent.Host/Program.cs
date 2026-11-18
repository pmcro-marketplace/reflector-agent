using A2A;
using A2A.AspNetCore;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

const string AgentName = "reflector-agent";
const string A2APath = "/a2a/reflector";
const string CorrelationPrefix = "PMCRO_CORRELATION ";

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddKeyedSingleton<AIAgent>(AgentName, (services, _) =>
    new ChatClientAgent(
        new CorrelationProofChatClient(),
        instructions:
            "You are the PMCR-O Reflector transport/correlation proof. " +
            "Do not claim role execution. Preserve any PMCR-O correlation envelope exactly.",
        name: AgentName,
        description: "Records evidence-earned lessons and may propose the next bounded intent; it never dispatches the next PMCR-O cycle.",
        services: services));

builder.AddA2AServer(AgentName);

var app = builder.Build();
var publicBaseUrl = builder.Configuration["A2A_PUBLIC_BASE_URL"]?.TrimEnd('/') ?? "http://127.0.0.1:5095";

app.MapGet("/healthz", () => Results.Ok(new
{
    status = "PASS",
    agent = AgentName,
    transport = "A2A",
    provider = "transport-proof",
    execution = "NOT_INVOKED", next_cycle_dispatch=false
}));

app.MapA2AHttpJson(AgentName, A2APath);
app.MapA2AJsonRpc(AgentName, A2APath);

app.MapWellKnownAgentCard(new AgentCard
{
    Name = "PMCR-O Reflector Agent",
    Description = "Records evidence-earned lessons and may propose the next bounded intent; it never dispatches the next PMCR-O cycle.",
    Version = "1.1.0",
    DefaultInputModes = ["text"],
    DefaultOutputModes = ["text"],
    SupportedInterfaces =
    [
        new AgentInterface
        {
            Url = publicBaseUrl + A2APath,
            ProtocolBinding = ProtocolBindingNames.HttpJson,
            ProtocolVersion = "1.0",
        }
    ]
});

app.Run();

internal sealed class CorrelationProofChatClient : IChatClient
{
    private const string Disposition =
        "NOT_INVOKED: A2A transport reached reflector-agent; real Reflector execution is not connected and next-cycle dispatch remains forbidden.";

    public Task<ChatResponse> GetResponseAsync(
        IEnumerable<ChatMessage> messages,
        ChatOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var text = BuildDisposition(messages);
        return Task.FromResult(new ChatResponse(new ChatMessage(ChatRole.Assistant, text)));
    }

    public async IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
        IEnumerable<ChatMessage> messages,
        ChatOptions? options = null,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await Task.Yield();
        yield return new ChatResponseUpdate(ChatRole.Assistant, BuildDisposition(messages));
    }

    private static string BuildDisposition(IEnumerable<ChatMessage> messages)
    {
        var correlation = messages
            .Select(message => message.Text)
            .LastOrDefault(text => text.StartsWith(CorrelationPrefix, StringComparison.Ordinal));

        return correlation is null
            ? Disposition
            : Disposition + " " + correlation;
    }

    public object? GetService(Type serviceType, object? serviceKey = null) =>
        serviceKey is null && serviceType.IsInstanceOfType(this) ? this : null;

    public void Dispose() { }
}
