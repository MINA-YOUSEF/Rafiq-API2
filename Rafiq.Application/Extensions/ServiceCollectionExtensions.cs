using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Rafiq.Application.Mapping;

namespace Rafiq.Application.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddAutoMapper(typeof(MappingProfile).Assembly);
        services.AddValidatorsFromAssembly(typeof(MappingProfile).Assembly);
        return services;
    }
}
