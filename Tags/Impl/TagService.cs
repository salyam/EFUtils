using Microsoft.EntityFrameworkCore;

namespace Salyam.EFUtils.Tags.Impl;

/// <summary>
/// Implements the ITagService interface to manage tags for entities within a database context.
/// </summary>
/// <typeparam name="DbContextType">The type of the DbContext, which must inherit from DbContext and implement ITagDbContext.</typeparam>
/// <typeparam name="TaggableType">The type of the entity that can be tagged.</typeparam>
internal class TagService<DbContextType, TaggableType>(DbContextType dbContext)
    : ITagService<TaggableType>
    where DbContextType : DbContext, ITagDbContext<TaggableType>
    where TaggableType : class
{
    /// <summary>
    /// Sets the tags for a given entity asynchronously.
    /// </summary>
    /// <param name="entity">The entity to set tags for.</param>
    /// <param name="tagNames">The collection of tag names to set.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task SetTagsAsync(TaggableType entity, IEnumerable<string> tagNames, CancellationToken cancellationToken)
    {
        // Normalize and prepare tag names for case-insensitive comparison.
        var tags = tagNames
            .Select(x => (raw: x, normalized: x.ToUpperInvariant()))
            .ToList();
        var normalizedTagNames = tags
            .Select(x => x.normalized)
            .ToList();

        // Retrieve existing tags associated with the entity.
        var entityTags = await dbContext.TaggedEntities
            .Where(x => x.Entity == entity)
            .Include(x => x.Tag) // Eagerly load the related Tag entities.
            .ToListAsync(cancellationToken);

        // Remove tags that are no longer associated with the entity.
        dbContext.TaggedEntities.RemoveRange(entityTags.Where(t => !normalizedTagNames.Contains(t.Tag.NormalizedName)));

        // Identify tags that need to be added to the entity.
        var tagsToAdd = tags
            .Where(x => !entityTags.Any(t => t.Tag.NormalizedName == x.normalized))
            .ToList();
        var normalizedTagNamesToAdd = tagsToAdd
            .Select(x => x.normalized)
            .ToList();
        
        // Retrieve tags that already exist in the database.
        var existingTags = await dbContext.Tags
            .Where(x => normalizedTagNamesToAdd.Contains(x.NormalizedName))
            .ToListAsync(cancellationToken);

        // Identify tags that do not yet exist in the database and need to be created.
        var notExistingTags = tagsToAdd
            .Where(x => !existingTags.Any(t => t.NormalizedName == x.normalized))
            .Select(x => new ITagDbContext<TaggableType>.TagModel() { Name = x.raw, NormalizedName = x.normalized })
            .ToList();

        // Add the new tags to the database.
        await dbContext.Tags.AddRangeAsync(notExistingTags, cancellationToken);
        
        // combine new and existing tags.
        existingTags.AddRange(notExistingTags);

        // Prepare TaggedEntity associations for new tags.
        var taggedEntityEntitiesToAdd = existingTags
            .Select(x => new ITagDbContext<TaggableType>.TaggedEntityModel() { Tag = x, Entity = entity });

        // Add the new TaggedEntity associations to the database.
        await dbContext.TaggedEntities.AddRangeAsync(taggedEntityEntitiesToAdd, cancellationToken);

        // Save all changes to the database.
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Retrieves the tags associated with a given entity asynchronously.
    /// </summary>
    /// <param name="entity">The entity to retrieve tags for.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A list of strings representing the tags of the entity.</returns>
    public async Task<List<string>> GetTagsAsync(TaggableType entity, CancellationToken cancellationToken)
    {
        // Retrieve the names of tags associated with the entity.
        return await dbContext.TaggedEntities
            .Where(x => x.Entity == entity)
            .Select(x => x.Tag.Name)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Retrieves entities that are tagged with any of the specified tags.
    /// </summary>
    /// <param name="tagNames">The collection of tag names to filter entities by.</param>
    /// <returns>An IQueryable of entities that match the specified tags.</returns>
    public IQueryable<TaggableType> GetTaggedEntitiesAsync(IEnumerable<string> tagNames)
    {
        // Normalize tag names for case-insensitive comparison.
        var normalizedTagNames = tagNames.Select(x => x.ToUpperInvariant()).ToList();
        // Retrieve entities associated with the specified tags.
        return dbContext.TaggedEntities
            .Where(x => normalizedTagNames.Contains(x.Tag.NormalizedName))
            .Select(x => x.Entity);
    }
}
