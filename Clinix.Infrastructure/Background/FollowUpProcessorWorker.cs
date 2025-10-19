using Clinix.Application.UseCases;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Clinix.Infrastructure.Background;

public class FollowUpProcessorWorker : BackgroundService
    {
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<FollowUpProcessorWorker> _logger;
    private readonly TimeSpan _interval = TimeSpan.FromMinutes(15);

    public FollowUpProcessorWorker(IServiceProvider serviceProvider, ILogger<FollowUpProcessorWorker> logger)
        {
        _serviceProvider = serviceProvider;
        _logger = logger;
        }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
        while (!stoppingToken.IsCancellationRequested)
            {
            using var scope = _serviceProvider.CreateScope();
            var processor = scope.ServiceProvider.GetRequiredService<IProcessDueFollowUpItemsUseCase>();

            try
                {
                _logger.LogInformation("Running Follow-up due item processor...");
                await processor.HandleAsync();
                }
            catch (Exception ex)
                {
                _logger.LogError(ex, "Error while processing follow-up items.");
                }

            await Task.Delay(_interval, stoppingToken);
            }
        }
    }
