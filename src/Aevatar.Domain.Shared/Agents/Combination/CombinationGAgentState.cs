using System;
using System.Collections.Generic;
using Aevatar.Agents.Combination.GEvents;
using Aevatar.Agents.Combination.Models;
using Aevatar.Agents.Group;
using Aevatar.Core.Abstractions;
using Orleans;

namespace Aevatar.Agents.Combination;

[GenerateSerializer]
public class CombinationGAgentState : GroupAgentState
{
    [Id(0)] public Guid Id { get; set; }
    [Id(1)] public AgentStatus Status { get; set; }
    [Id(2)] public string UserAddress { get; set; }
    [Id(3)] public string Name { get; set; }
    [Id(4)] public string GroupId { get; set; }
    [Id(5)] public List<string> AgentComponent { get; set; }
    
    public void Apply(CombineAgentGEvent combineAgentGEvent)
    {
        Id = combineAgentGEvent.Id;
        Name = combineAgentGEvent.Name;
        GroupId = combineAgentGEvent.GroupId;
        UserAddress = combineAgentGEvent.UserAddress;
        Status = AgentStatus.Running;
        AgentComponent = combineAgentGEvent.AgentComponent;
    }
    
    public void Apply(UpdateCombinationGEvent combineCombinationGEvent)
    {
        Name = combineCombinationGEvent.Name;
        AgentComponent = combineCombinationGEvent.AgentComponent;
    }
    
    public void Apply(DeleteCombinationGEvent deleteCombinationGEvent)
    {
        Name = "";
        AgentComponent = new List<string>();
        GroupId = "";
        Status = AgentStatus.Deleted;
        UserAddress = "";
    }
}

