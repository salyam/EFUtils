using Microsoft.EntityFrameworkCore;

namespace Salyam.EFUtils.Comments;

public interface ICommentDbContext<CommentableType, CommenterType>
    where CommentableType : class
    where CommenterType : class
{
    public class CommentModel
    {
        public int Id { get; set; }

        [System.ComponentModel.DataAnnotations.MaxLength(65535)]
        public required string Text { get; set; }

        public required CommentableType Entity { get; set; }

        public CommenterType? Commenter { get; set; }
    }

    DbSet<CommentModel> Comments { get; }
}
