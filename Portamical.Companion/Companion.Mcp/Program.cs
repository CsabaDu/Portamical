// SPDX-License-Identifier: MIT
// Copyright (c) 2026. Csaba Dudas (CsabaDu)

using Portamical.Companion.Mcp;

var server = new McpServer("portamical-companion", "5.0.0");
CompanionTools.RegisterAll(server);
server.Run(Console.In, Console.Out);
