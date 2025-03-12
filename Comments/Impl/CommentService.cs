using Microsoft.EntityFrameworkCore;

namespace Salyam.EFUtils.Comments.Impl;

internal class CommentService<DbContextType, CommentableType, CommenterType>
    (DbContextType dbContext)
    : ICommentService<CommentableType, CommenterType>
    where DbContextType : DbContext, ICommentDbContext<CommentableType, CommenterType>
    where CommentableType : class
    where CommenterType : class
{
    /// <inheritdoc />
    public async Task AddCommentAsync(CommentableType entity, CommenterType? commenter, string text, CancellationToken cancellationToken)
    {
        await dbContext.Comments.AddAsync(new () 
            {
                Text = text,
                Entity = entity,
                Commenter = commenter
            },
            cancellationToken
        );
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public IQueryable<ICommentService<CommentableType, CommenterType>.CommentData> GetCommentsAsync(CommentableType entity, CancellationToken cancellationToken)
    {
        return dbContext.Comments.Where(c => c.Entity == entity)
            .Select(c => new ICommentService<CommentableType, CommenterType>.CommentData(c.Id, c.Entity, c.Commenter, c.Text))
            .AsQueryable();
    }

    public async Task RemoveCommentAsync(int id, CancellationToken cancellationToken)
    {
        var entity = await dbContext.Comments.FindAsync(id, cancellationToken)
            ?? throw new KeyNotFoundException($"No comment with id {id} was found.");
        dbContext.Comments.Remove(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
