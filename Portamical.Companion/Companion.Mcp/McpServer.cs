// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

using System.Text.Json;
using System.Text.Json.Nodes;

namespace Portamical.Companion.Mcp;

/// <summary>
/// Minimal Model Context Protocol server over stdio (newline-delimited JSON-RPC 2.0).
/// Handles <c>initialize</c>, <c>tools/list</c> and <c>tools/call</c>; tool handlers are
/// registered via <see cref="RegisterTool"/>.
/// </summary>
public sealed class McpServer(string serverName, string serverVersion)
{
    private sealed record ToolRegistration(
        string Name,
        string Description,
        JsonObject InputSchema,
        Func<JsonObject?, string> Handler);

    private readonly Dictionary<string, ToolRegistration> _tools = [];

    /// <summary>
    /// Registers a tool with its JSON schema and a handler mapping arguments to text output.
    /// </summary>
    public void RegisterTool(
        string name,
        string description,
        JsonObject inputSchema,
        Func<JsonObject?, string> handler)
    => _tools[name] = new ToolRegistration(name, description, inputSchema, handler);

    /// <summary>
    /// Runs the read-dispatch-respond loop until stdin closes.
    /// </summary>
    public void Run(TextReader input, TextWriter output)
    {
        string? line;

        while ((line = input.ReadLine()) is not null)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            JsonNode? message;

            try
            {
                message = JsonNode.Parse(line);
            }
            catch (JsonException)
            {
                continue;
            }

            JsonNode? id = message?["id"];
            string? method = message?["method"]?.GetValue<string>();

            if (method is null)
            {
                continue;
            }

            if (id is null)
            {
                continue; // notifications need no response
            }

            JsonNode response = Dispatch(method, message?["params"] as JsonObject, id.DeepClone());
            output.WriteLine(response.ToJsonString(JsonSerializerOptions.Default));
            output.Flush();
        }
    }

    private JsonNode Dispatch(string method, JsonObject? parameters, JsonNode id)
    => method switch
    {
        "initialize" => Result(id, new JsonObject
        {
            ["protocolVersion"] = parameters?["protocolVersion"]?.DeepClone() ?? "2025-06-18",
            ["capabilities"] = new JsonObject { ["tools"] = new JsonObject() },
            ["serverInfo"] = new JsonObject
            {
                ["name"] = serverName,
                ["version"] = serverVersion,
            },
        }),
        "tools/list" => Result(id, new JsonObject
        {
            ["tools"] = new JsonArray([.. _tools.Values.Select(t => (JsonNode)new JsonObject
            {
                ["name"] = t.Name,
                ["description"] = t.Description,
                ["inputSchema"] = t.InputSchema.DeepClone(),
            })]),
        }),
        "tools/call" => CallTool(parameters, id),
        "ping" => Result(id, new JsonObject()),
        _ => Error(id, -32601, $"Method not found: {method}"),
    };

    private JsonNode CallTool(JsonObject? parameters, JsonNode id)
    {
        string? name = parameters?["name"]?.GetValue<string>();

        if (name is null || !_tools.TryGetValue(name, out var tool))
        {
            return Error(id, -32602, $"Unknown tool: {name}");
        }

        try
        {
            string text = tool.Handler(parameters?["arguments"] as JsonObject);

            return Result(id, new JsonObject
            {
                ["content"] = new JsonArray(new JsonObject
                {
                    ["type"] = "text",
                    ["text"] = text,
                }),
            });
        }
        catch (Exception ex)
        {
            return Result(id, new JsonObject
            {
                ["content"] = new JsonArray(new JsonObject
                {
                    ["type"] = "text",
                    ["text"] = $"{ex.GetType().Name}: {ex.Message}",
                }),
                ["isError"] = true,
            });
        }
    }

    private static JsonNode Result(JsonNode id, JsonNode result)
    => new JsonObject
    {
        ["jsonrpc"] = "2.0",
        ["id"] = id,
        ["result"] = result,
    };

    private static JsonNode Error(JsonNode id, int code, string message)
    => new JsonObject
    {
        ["jsonrpc"] = "2.0",
        ["id"] = id,
        ["error"] = new JsonObject
        {
            ["code"] = code,
            ["message"] = message,
        },
    };
}
