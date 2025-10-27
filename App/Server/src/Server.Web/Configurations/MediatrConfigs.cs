using System.Reflection;
using Server.Core.UserAggregate;
using Server.UseCases.ErrorTest;
using Server.UseCases.Users.Register;

namespace Server.Web.Configurations;

public static class MediatrConfigs
{
  public static IServiceCollection AddMediatrConfigs(this IServiceCollection services)
  {
    var mediatRAssemblies = new[]
      {
        Assembly.GetAssembly(typeof(User)), // Core
        Assembly.GetAssembly(typeof(RegisterUserCommand)), // UseCases
        Assembly.GetAssembly(typeof(ThrowInUseCaseQuery)) // UseCases.ErrorTest
      };

    services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(mediatRAssemblies!))
            .AddScoped<IDomainEventDispatcher, MediatRDomainEventDispatcher>();

    // Automatically register pipeline behaviors for all ICommand<T> and IQuery<T> types
    // Uses reflection to discover types, but the behaviors themselves use constrained generics (no reflection at runtime)
    RegisterPipelineBehaviorsForResultRequests(services, mediatRAssemblies.Where(a => a != null).ToArray()!);

    return services;
  }

  /// <summary>
  /// Template method that orchestrates the registration of pipeline behaviors in the correct order.
  /// Uses reflection at startup to discover all ICommand and IQuery types, but the behaviors
  /// themselves use constrained generics with zero reflection at runtime.
  /// </summary>
  private static void RegisterPipelineBehaviorsForResultRequests(IServiceCollection services, Assembly[] assemblies)
  {
    var resultRequestTypes = assemblies
      .SelectMany(assembly => assembly.GetTypes())
      .Where(type => !type.IsAbstract && !type.IsInterface)
      .Select(type => new
      {
        Type = type,
        ResultRequestInterface = type.GetInterfaces()
          .FirstOrDefault(i => i.IsGenericType &&
                              (i.GetGenericTypeDefinition() == typeof(Server.SharedKernel.ICommand<>) ||
                               i.GetGenericTypeDefinition() == typeof(IQuery<>)))
      })
      .Where(x => x.ResultRequestInterface != null)
      .ToList();

    foreach (var typeInfo in resultRequestTypes)
    {
      var requestType = typeInfo.Type;
      var innerType = typeInfo.ResultRequestInterface!.GetGenericArguments()[0];
      var resultType = typeof(Result<>).MakeGenericType(innerType);

      // Register behaviors in the correct order:
      // 1. LoggingBehavior (logs request handling)
      // 2. TransactionBehavior (wraps handler execution in transaction)
      // 3. ExceptionHandlingBehavior (catches exceptions and converts to Result)
      RegisterBehavior(services, typeof(LoggingBehavior<,>), requestType, innerType, resultType);
      RegisterBehavior(services, typeof(TransactionBehavior<,>), requestType, innerType, resultType);
      RegisterBehavior(services, typeof(ExceptionHandlingBehavior<,>), requestType, innerType, resultType);
    }
  }

  private static void RegisterBehavior(
    IServiceCollection services,
    Type behaviorGenericType,
    Type requestType,
    Type innerType,
    Type resultType)
  {
    var behaviorType = behaviorGenericType.MakeGenericType(requestType, innerType);
    var interfaceType = typeof(IPipelineBehavior<,>).MakeGenericType(requestType, resultType);

    services.AddScoped(interfaceType, behaviorType);
  }
}
