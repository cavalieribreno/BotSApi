namespace BotSaaS.Api.Shared.AI;

// A tool the model can be offered. Core defines which tools exist; each client formats them for its provider.
public record ToolDefinition(string Name, string Description, IReadOnlyList<ToolParameter> Parameters);

// One parameter of a tool - flat (no nested schema yet). Type is a JSON Schema type name ("string", "integer"...).
public record ToolParameter(string Name, string Type, string Description, bool Required);
