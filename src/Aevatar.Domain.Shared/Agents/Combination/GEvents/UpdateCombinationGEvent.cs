using System.Collections.Generic;
using Orleans;

namespace Aevatar.Agents.Combination.GEvents;

public class UpdateCombinationGEvent : CombinationAgentGEvent
{
    [Id(0)] public string Name { get; set; }
    [Id(1)] public List<string> AgentComponent { get; set; }
}