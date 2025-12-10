using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using Polly.Timeout;

namespace CreditoAPI.Infrastructure.Resilience;

public static class PollyPolicies
{
    public static AsyncRetryPolicy CreateRetryPolicy(ILogger logger, int retryCount = 3)
    {
        return Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(
                retryCount,
                retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                onRetry: (exception, timeSpan, retry, context) =>
                {
                    logger.LogWarning(exception,
                        "Retry {Retry} after {Delay}s due to {ExceptionType}: {Message}",
                        retry, timeSpan.TotalSeconds, exception.GetType().Name, exception.Message);
                });
    }

    public static AsyncCircuitBreakerPolicy CreateCircuitBreakerPolicy(ILogger logger, 
        int exceptionsBeforeBreaking = 5, 
        int durationOfBreakInSeconds = 30)
    {
        return Policy
            .Handle<Exception>()
            .CircuitBreakerAsync(
                exceptionsAllowedBeforeBreaking: exceptionsBeforeBreaking,
                durationOfBreak: TimeSpan.FromSeconds(durationOfBreakInSeconds),
                onBreak: (exception, duration) =>
                {
                    logger.LogError(exception,
                        "Circuit breaker opened for {Duration}s due to: {Message}",
                        duration.TotalSeconds, exception.Message);
                },
                onReset: () =>
                {
                    logger.LogInformation("Circuit breaker reset");
                },
                onHalfOpen: () =>
                {
                    logger.LogInformation("Circuit breaker half-open, testing...");
                });
    }

    public static AsyncTimeoutPolicy CreateTimeoutPolicy(int timeoutInSeconds = 30)
    {
        return Policy.TimeoutAsync(TimeSpan.FromSeconds(timeoutInSeconds), TimeoutStrategy.Pessimistic);
    }

    public static IAsyncPolicy CreateCombinedPolicy(ILogger logger)
    {
        var retryPolicy = CreateRetryPolicy(logger);
        var circuitBreakerPolicy = CreateCircuitBreakerPolicy(logger);
        var timeoutPolicy = CreateTimeoutPolicy();

        // Wrap policies: Timeout -> Retry -> Circuit Breaker
        return Policy.WrapAsync(timeoutPolicy, retryPolicy, circuitBreakerPolicy);
    }
}
