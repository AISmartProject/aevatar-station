using Orleans;

namespace Aevatar.Agents.Atomic.GEvents;

[GenerateSerializer]
public class UpdateAgentGEvent : AtomicAgentGEvent
{
    [Id(0)] public string Name { get; set; }
    [Id(1)] public string Properties { get; set; }
}