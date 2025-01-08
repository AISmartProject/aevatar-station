using System.Threading.Tasks;
using Aevatar.AtomicAgent.Dtos;
using Aevatar.CombinationAgent.Dtos;

namespace Aevatar.Service;

public interface IAgentService
{
    Task<AtomicAgentDto> GetAtomicAgentAsync(string id);
    Task<AtomicAgentDto> CreateAtomicAgentAsync(CreateAtomicAgentDto createDto);
    Task<AtomicAgentDto> UpdateAtomicAgentAsync(string id, UpdateAtomicAgentDto updateDto);
    Task DeleteAtomicAgentAsync(string id);
    Task<CombinationAgentDto> CombineAgentAsync(CombineAgentDto combineAgentDto);
}