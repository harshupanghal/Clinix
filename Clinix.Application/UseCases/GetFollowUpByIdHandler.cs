using AutoMapper;
using Clinix.Application.Dtos.FollowUps;
using Clinix.Application.Interfaces.Functionalities;
using Microsoft.Extensions.Logging;

namespace Clinix.Application.UseCases;

public sealed class GetFollowUpByIdHandler
    {
    private readonly IFollowUpRepository _repo;
    private readonly IMapper _mapper;
    private readonly ILogger<GetFollowUpByIdHandler> _logger;

    public GetFollowUpByIdHandler(IFollowUpRepository repo, IMapper mapper, ILogger<GetFollowUpByIdHandler> logger)
        {
        _repo = repo;
        _mapper = mapper;
        _logger = logger;
        }

    public async Task<FollowUpDto> HandleAsync(long followUpId)
        {
        _logger.LogDebug("Admin requested follow-up {FollowUpId}", followUpId);
        var followUp = await _repo.GetByIdAsync(followUpId);
        if (followUp == null) throw new InvalidOperationException("Follow-up not found.");
        return _mapper.Map<FollowUpDto>(followUp);
        }
    }

