using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Football.Bot.Extensions;

public static class DiExtensions
{
    public static IServiceCollection AddConfigurationModel<TConfiguration>(
        this IServiceCollection services,
        IConfiguration configuration,
        string key) where TConfiguration : class =>
        services.AddTransient(_ => configuration.GetSection(key).Get<TConfiguration>());
}
