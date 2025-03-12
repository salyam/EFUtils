namespace Salyam.EFUtils.Tags;

/// <summary>
/// Defines a service interface for managing tags associated with entities in the database.
/// </summary>
/// <typeparam name="TaggableType">The type of the entity to which the tags will be applied.</typeparam>
public interface ITagService<TaggableType>
    where TaggableType : class
{
    /// <summary>
    /// Asynchronously sets tags of a specified entity.
    /// </summary>
    /// <param name="entity">The entity to be tagged.</param>
    /// <param name="tags">The tags of the specified entity.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task SetTagsAsync(
        TaggableType entity,
        IEnumerable<string> tags,
        CancellationToken cancellationToken = default
        );

    /// <summary>
    /// Retrieves the tags associated with the specified entity.
    /// </summary>
    /// <param name="entity">The entity whose tags are to be retrieved.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A list of strings representing the tags of the entity.</returns>
    Task<List<string>> GetTagsAsync(
        TaggableType entity,
        CancellationToken cancellationToken = default
        );

    /// <summary>
    /// Retrieves entities that have been tagged with any of the specified tags.
    /// </summary>
    /// <param name="tagNames">An IEnumerable of strings representing the tag names to filter the entities by.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An IQueryable of EntityType representing entities that match the specified tags.</returns>
    IQueryable<TaggableType> GetTaggedEntitiesAsync(
        IEnumerable<string> tagNames);
}
