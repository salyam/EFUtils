using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Salyam.EFUtils.Tags;

/// <summary>
/// Defines the contract for a DbContext that manages tags and their associations with entities.
/// </summary>
/// <typeparam name="TaggableType">The type of the entity to which tags can be applied.</typeparam>
public interface ITagDbContext<TaggableType>
    where TaggableType : class
{
    /// <summary>
    /// Represents a tag with its name and normalized name.
    /// </summary>
    public class TagModel
    {
        /// <summary>
        /// Gets or sets the unique identifier of the tag.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the name of the tag.
        /// </summary>
        [MaxLength(256)]
        public required string Name { get; set; }

        /// <summary>
        /// Gets or sets the normalized (e.g., uppercase) name of the tag for case-insensitive operations.
        /// </summary>
        [MaxLength(256)]
        public required string NormalizedName { get; set; }
    }

    /// <summary>
    /// Represents an association between a tag and a tagged entity.
    /// </summary>
    public class TaggedEntityModel
    {
        /// <summary>
        /// Gets or sets the unique identifier of the tagged entity association.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the tag associated with the entity.
        /// </summary>
        public required TagModel Tag { get; set; }

        /// <summary>
        /// Gets or sets the entity that is associated with the tag.
        /// </summary>
        public required TaggableType Entity { get; set; }
    }

    /// <summary>
    /// Gets or sets the DbSet for TagModel.
    /// </summary>
    DbSet<TagModel> Tags { get; }

    /// <summary>
    /// Gets or sets the DbSet for TaggedEntityModel.
    /// </summary>
    DbSet<TaggedEntityModel> TaggedEntities { get; }
}
