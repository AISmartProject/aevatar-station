using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Aevatar.Agents.Creator;
using Aevatar.Application.Grains.Agents.Creator;
using Aevatar.Application.Grains.Subscription;
using Aevatar.Common;
using Aevatar.Core.Abstractions;
using Aevatar.Domain.Grains.Subscription;
using Aevatar.Subscription;
using Microsoft.Extensions.Logging;
using Orleans;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Auditing;
using Volo.Abp.ObjectMapping;

namespace Aevatar.Service;

public interface ISubscriptionAppService
{
    Task<List<EventDescriptionDto>> GetAvailableEventsAsync(string agentId);
    Task<SubscriptionDto> SubscribeAsync(CreateSubscriptionDto createSubscriptionDto);
    Task CancelSubscriptionAsync(Guid subscriptionId);
    Task<SubscriptionDto> GetSubscriptionAsync(Guid subscriptionId);
    Task PublishEventAsync(PublishEventDto dto);
}

[RemoteService(IsEnabled = false)]
[DisableAuditing]
public class SubscriptionAppService : ApplicationService, ISubscriptionAppService
{
    private readonly IClusterClient _clusterClient;
    private readonly ILogger<SubscriptionAppService> _logger;
    private readonly IObjectMapper _objectMapper;
    private readonly IUserAppService _userAppService;
    
    public SubscriptionAppService(
        IClusterClient clusterClient,
        IObjectMapper objectMapper,
        IUserAppService userAppService,
        ILogger<SubscriptionAppService> logger)
    {
        _clusterClient = clusterClient;
        _objectMapper = objectMapper;
        _logger = logger;
        _userAppService = userAppService;
    }
    
    public async Task<List<EventDescriptionDto>> GetAvailableEventsAsync(string agentId)
    {
        var agent = _clusterClient.GetGrain<ICreatorGAgent>(ParseGuid(agentId));
        var agentState = await agent.GetAgentAsync();
        var dto = _objectMapper.Map<List<EventDescription>, List<EventDescriptionDto>>(agentState.EventInfoList);
        return dto;
    }
    

    public async Task<SubscriptionDto> SubscribeAsync(CreateSubscriptionDto createSubscriptionDto)
    {

      var  input = _objectMapper.Map<CreateSubscriptionDto, SubscribeEventInputDto>(createSubscriptionDto);
      var subscriptionStateAgent =
          _clusterClient.GetGrain<ISubscriptionGAgent>(GuidUtil.StringToGuid(createSubscriptionDto.AgentId));
      
      input.UserId = _userAppService.GetCurrentUserId();
      var subscriptionState = await subscriptionStateAgent.SubscribeAsync(input);
      
      var agent = _clusterClient.GetGrain<ICreatorGAgent>(ParseGuid(input.AgentId));
      await agent.RegisterAsync(subscriptionStateAgent);
      return _objectMapper.Map<EventSubscriptionState, SubscriptionDto>(subscriptionState);
    }

    public async Task CancelSubscriptionAsync(Guid subscriptionId)
    {
        var subscriptionStateAgent =
            _clusterClient.GetGrain<ISubscriptionGAgent>(subscriptionId);
        var subscriptionState = await subscriptionStateAgent.GetStateAsync();
        var currentUserId = _userAppService.GetCurrentUserId();
        if (subscriptionState.UserId != currentUserId)
        {
            _logger.LogInformation("User {userId} is not allowed to cancel subscription {subscriptionId}.", currentUserId, subscriptionId);
            throw new UserFriendlyException("User is not allowed to cancel subscription");
        }
        
        var agent = _clusterClient.GetGrain<ICreatorGAgent>(ParseGuid(subscriptionState.AgentId));
        await agent.UnregisterAsync(subscriptionStateAgent);
        await subscriptionStateAgent.UnsubscribeAsync();
    }

    public async Task<SubscriptionDto> GetSubscriptionAsync(Guid subscriptionId)
    {
        var subscriptionState = await _clusterClient.GetGrain<ISubscriptionGAgent>(subscriptionId)
            .GetStateAsync();
        return _objectMapper.Map<EventSubscriptionState, SubscriptionDto>(subscriptionState);
    }

    public async Task PublishEventAsync(PublishEventDto dto)
    {
        var agent = _clusterClient.GetGrain<ICreatorGAgent>(dto.AgentId);
        var agentState = await agent.GetAgentAsync();
        var currentUserId = _userAppService.GetCurrentUserId();
        if (agentState.UserId != currentUserId)
        {
            _logger.LogInformation("User {userId} is not allowed to publish event {eventType}.", currentUserId, dto.EventType);
            throw new UserFriendlyException("User is not allowed to publish event");
        }
        
        var eventList = agentState.EventInfoList;

        var eventDescription = eventList.Find(i => i.EventType.Name == dto.EventType);
        if (eventDescription == null)
        {
            _logger.LogInformation("Type {type} could not be found.", dto.EventType);
            throw new UserFriendlyException("event could not be found");
        }
        
        var eventType = eventDescription.EventType;
        object? eventInstance = Activator.CreateInstance(eventType);
        if (eventInstance == null)
        {
            _logger.LogInformation("Type {type} could not be instantiated.", dto.EventType);
            throw new UserFriendlyException("event could not be instantiated");
        }
        
        foreach (var property in dto.EventProperties)
        {
            string propertyName = property.Key;
            object propertyValue = property.Value;
            
            PropertyInfo? propInfo = eventType.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
            if (propInfo == null || !propInfo.CanWrite)
            {
                _logger.LogInformation("Property {propertyName} not found or cannot be written.", propertyName);
                throw new UserFriendlyException("property could not be found or cannot be written");
            }
  
            object? convertedValue = ReflectionUtil.ConvertValue(propInfo.PropertyType, propertyValue);
            propInfo.SetValue(eventInstance, convertedValue);
        }
        
        await agent.PublishEventAsync((EventBase)eventInstance);
    }
    
    private Guid ParseGuid(string id)
    {
        if (!Guid.TryParse(id, out Guid validGuid))
        {
            _logger.LogInformation("Invalid id: {id}", id);
            throw new UserFriendlyException("Invalid id");
        }
        return validGuid;
    }
}