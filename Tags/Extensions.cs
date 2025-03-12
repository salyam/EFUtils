using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Salyam.EFUtils.Tags.Impl;

namespace Salyam.EFUtils.Tags;

/// <summary>
/// Provides extension methods for configuring and registering tag services within an IServiceCollection.
/// </summary>
public static class Extensions
{
    /// <summary>
    /// Adds the necessary services for managing tags to the dependency injection container.
    /// </summary>
    /// <typeparam name="DbContextType">The type of the DbContext, which must implement ITagDbContext.</typeparam>
    /// <typeparam name="TaggableType">The type of the entity that can be tagged.</typeparam>
    /// <param name="services">The IServiceCollection to add the services to.</param>
    /// <param name="lifetime">The lifetime of the tag service (default is Scoped).</param>
    /// <returns>The updated IServiceCollection.</returns>
    public static IServiceCollection AddEfCoreTags<DbContextType, TaggableType>(this IServiceCollection services, ServiceLifetime lifetime = ServiceLifetime.Scoped)
        where DbContextType : DbContext, ITagDbContext<TaggableType>
        where TaggableType : class
    {
        // Register the TagService implementation with the specified lifetime.
        // This allows resolving ITagService<TaggableType> from the service provider.
        var descriptor = new ServiceDescriptor(
            serviceType: typeof(ITagService<TaggableType>),
            implementationType: typeof(TagService<DbContextType, TaggableType>),
            lifetime: lifetime);
        services.Add(descriptor);

        return services;
    }
}
