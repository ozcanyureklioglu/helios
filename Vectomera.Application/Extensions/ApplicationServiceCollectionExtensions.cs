using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Vectomera.Application.Extensions;

public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // FluentValidation: tÃ¼m validator'larÄ± otomatik tarar ve kaydeder
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        return services;
    }
}

