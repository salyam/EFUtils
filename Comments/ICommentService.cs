namespace Salyam.EFUtils.Comments;

/// <summary>
/// Defines a service interface for managing comments associated with entities in the database.
/// </summary>
/// <typeparam name="CommentableType">The type of the entity to which the comments will be applied.</typeparam>
/// <typeparam name="CommenterType">The type of the entities that can create comments.</typeparam>
public interface ICommentService<CommentableType, CommenterType>
    where CommentableType : class
    where CommenterType : class
{
    /// <summary>
    /// Asynchronously adds a tag to a specified entity.
    /// </summary>
    /// <param name="entity">The entity to add the comment to.</param>
    /// <param name="commenter">The commenter of the newly added comment.</param>
    /// <param name="text">The text of the comment to be added.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task AddCommentAsync(
        CommentableType entity,
        CommenterType? commenter,
        string text,
        CancellationToken cancellationToken = default
        );

    /// <summary>
    /// Asynchronously removes a comment from a specified entity.
    /// </summary>
    /// <param name="id">The id of the entity to be removed.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task RemoveCommentAsync(
        int id,
        CancellationToken cancellationToken = default
        );

    public record CommentData(int Id, CommentableType Entity, CommenterType? Commenter, string Text);

    /// <summary>
    /// Retrieves the tags associated with the specified entity.
    /// </summary>
    /// <param name="entity">The entity whose comments are to be retrieved.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A query object representing the list of comments.</returns>
    IQueryable<CommentData> GetCommentsAsync(
        CommentableType entity,
        CancellationToken cancellationToken = default
        );
}
