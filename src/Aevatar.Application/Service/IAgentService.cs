using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Aevatar.Agent;
using Aevatar.AtomicAgent;
using Aevatar.CQRS.Dto;

namespace Aevatar.Service;

public interface IAgentService
{
    Task<AtomicAgentDto> GetAtomicAgentAsync(string id);
    Task<AtomicAgentDto> CreateAtomicAgentAsync(CreateAtomicAgentDto createDto);
    Task<AtomicAgentDto> UpdateAtomicAgentAsync(string id, UpdateAtomicAgentDto updateDto);
    Task<List<AtomicAgentDto>> GetAtomicAgentsAsync(string userAddress, int pageIndex, int pageSize);

    Task DeleteAtomicAgentAsync(string id);
    Task<Tuple<long, List<AgentGEventIndex>>> GetAgentEventLogsAsync(string agentId, int pageIndex, int pageSize);
    
    Task<List<AgentTypeDto>> GetAllAgents();
    
    Task<AgentDto> CreateAgentAsync(CreateAgentInputDto dto);
    Task<AgentDto> GetAgentAsync(Guid guid);
    Task<AgentDto> UpdateAgentAsync(Guid guid, UpdateAgentInputDto dto);
    Task<SubAgentDto> AddSubAgentAsync(Guid guid, AddSubAgentDto addSubAgentDto);
    Task<SubAgentDto> RemoveSubAgentAsync(Guid guid, RemoveSubAgentDto removeSubAgentDto);
    Task RemoveAllSubAgentAsync(Guid guid);
    Task<AgentRelationshipDto> GetAgentRelationshipAsync(Guid guid);
    Task DeleteAgentAsync(Guid guid);
}