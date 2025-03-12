using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Salyam.EFUtils.Comments.Test.Helpers;

public class Book
{
    public int Id { get; set; } 
    public required string Title { get; set; } 
    
    public required string Author { get; set; } 
}

public class Article
{
    public int Id { get; set; } 

    public required string Title { get; set; }

    public required string Content { get; set; } 
    
    public required string Author { get; set; } 
}


public partial class TestDbContext : IdentityDbContext, ICommentDbContext<Book, IdentityUser>, ICommentDbContext<Article, IdentityUser>
{
    public DbSet<Book> Books { get; set; }
    public DbSet<Article> Articles { get; set; }
    
    DbSet<ICommentDbContext<Book, IdentityUser>.CommentModel> ICommentDbContext<Book, IdentityUser>.Comments { get => Set<ICommentDbContext<Book, IdentityUser>.CommentModel>(); }

    DbSet<ICommentDbContext<Article, IdentityUser>.CommentModel> ICommentDbContext<Article, IdentityUser>.Comments { get => Set<ICommentDbContext<Article, IdentityUser>.CommentModel>(); }

    public TestDbContext() {}

    public TestDbContext(DbContextOptions options) : base(options) {}

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
    }
}