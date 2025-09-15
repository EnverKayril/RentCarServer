using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using TS.MediatR;

namespace RentCarServer.Application;

public static class ServiceRegistrar
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // MediatR ve FluentValidation servislerini ekliyoruz
        services.AddMediatR(cfr =>
        {
            cfr.RegisterServicesFromAssembly(typeof(ServiceRegistrar).Assembly);
            cfr.AddOpenBehavior(typeof(Behaviors.ValidationBehavior<,>));
            cfr.AddOpenBehavior(typeof(Behaviors.PermissionBehavior<,>));
        });

        services.AddValidatorsFromAssembly(typeof(ServiceRegistrar).Assembly);

        return services;
    }
}
