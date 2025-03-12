using Salyam.EFUtils.Comments.Test.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace Salyam.EFUtils.Comments.Test;

/// <summary>
/// Contains unit tests for the ICommentService interface using XUnit.
/// </summary>
public class CommentServiceTests
{
    private readonly TestFixture _fixture;

    public CommentServiceTests()
    {
        this._fixture = new TestFixture();
        _fixture.Services
            .AddEfCoreComments<TestDbContext, Book, IdentityUser>()
            .AddEfCoreComments<TestDbContext, Article, IdentityUser>();
    }

    /// <summary>
    /// Test to verify that adding a comment successfully persists the comment for a valid Commentable and Commenter.
    /// </summary>
    [Fact]
    public async Task GivenValidCommentableAndCommenter_WhenAddCommentAsync_ThenCommentIsPersisted()
    {
        // Arrange
        const string commentText = "This is a great book!";

        var provider = this._fixture.Services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        var commentService = scope.ServiceProvider.GetRequiredService<ICommentService<Book, IdentityUser>>();
        var book = scope.ServiceProvider.GetRequiredService<TestDbContext>().Books.Find(1) ?? throw new NullReferenceException("Book not found.");
        var user = scope.ServiceProvider.GetRequiredService<TestDbContext>().Users.Find(this._fixture.SeededUsers[0].Id) ?? throw new NullReferenceException("User not found.");

        // Act
        await commentService.AddCommentAsync(book, user, commentText);

        // Assert
        var comments = await commentService.GetCommentsAsync(book).ToListAsync();
        Assert.Single(comments);
        Assert.Equal(1, comments[0].Id);
        Assert.Equal(user, comments[0].Commenter);
        Assert.Equal(book, comments[0].Entity);
        Assert.Equal(commentText, comments[0].Text);
    }

    /// <summary>
    /// Test to verify that removing a comment by its ID successfully deletes it from the corresponding Commentable.
    /// </summary>
    [Fact]
    public async Task GivenExistingCommentId_WhenRemoveCommentAsync_ThenCommentIsDeleted()
    {
        // Arrange
        const string commentText = "This is a great book!";

        var provider = this._fixture.Services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        var book = scope.ServiceProvider.GetRequiredService<TestDbContext>().Books.Find(1) ?? throw new NullReferenceException("Book not found.");
        var user = scope.ServiceProvider.GetRequiredService<TestDbContext>().Users.Find(this._fixture.SeededUsers[0].Id) ?? throw new NullReferenceException("User not found.");
        var commentService = scope.ServiceProvider.GetRequiredService<ICommentService<Book, IdentityUser>>();

        // Act
        await commentService.AddCommentAsync(book, user, commentText);
        var existingComments = commentService.GetCommentsAsync(book).ToList();
        var commentId = existingComments.First().Id;

        // Act
        await commentService.RemoveCommentAsync(commentId);

        // Assert
        var updatedComments = commentService.GetCommentsAsync(book);
        Assert.DoesNotContain(updatedComments, comment => comment.Id == commentId);
    }

    /// <summary>
    /// Test to verify that querying comments for a Commentable returns all associated comments.
    /// </summary>
    [Fact]
    public async Task GivenCommentableWithComments_WhenGetCommentsAsync_ThenAllAssociatedCommentsAreReturned()
    {

        // Arrange
        var provider = this._fixture.Services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        var book = scope.ServiceProvider.GetRequiredService<TestDbContext>().Books.Find(1) ?? throw new NullReferenceException("Book not found.");
        var user1 = scope.ServiceProvider.GetRequiredService<TestDbContext>().Users.Find(this._fixture.SeededUsers[0].Id) ?? throw new NullReferenceException("User not found.");
        var user2 = scope.ServiceProvider.GetRequiredService<TestDbContext>().Users.Find(this._fixture.SeededUsers[1].Id) ?? throw new NullReferenceException("User not found.");
        var commentService = scope.ServiceProvider.GetRequiredService<ICommentService<Book, IdentityUser>>();

        await commentService.AddCommentAsync(book, user1, "Great book!");
        await commentService.AddCommentAsync(book, user2, "Absolutely loved it!");

        // Act
        var comments = commentService.GetCommentsAsync(book);

        // Assert
        Assert.NotNull(comments);
        Assert.Equal(2, comments.Count());
        Assert.Contains(comments, comment => comment.Text == "Great book!" && comment.Commenter == user1);
        Assert.Contains(comments, comment => comment.Text == "Absolutely loved it!" && comment.Commenter == user2);
    }

    /// <summary>
    /// Test to verify that adding a comment with a null Commenter still persists the comment against the Commentable.
    /// </summary>
    [Fact]
    public async Task GivenValidCommentableAndNullCommenter_WhenAddCommentAsync_ThenCommentIsPersisted()
    {
        // Arrange
        const string commentText = "Interesting read!";
        var provider = this._fixture.Services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        var commentService = scope.ServiceProvider.GetRequiredService<ICommentService<Article, IdentityUser>>();
        var article = scope.ServiceProvider.GetRequiredService<TestDbContext>().Articles.Find(1) ?? throw new NullReferenceException("Article not found.");

        // Act
        await commentService.AddCommentAsync(article, null, commentText);

        // Assert
        var comments = commentService.GetCommentsAsync(article);
        Assert.Contains(comments, comment => comment.Entity == article && comment.Commenter == null);
    }

    /// <summary>
    /// Test to verify that querying comments for a Commentable with no comments returns an empty list.
    /// </summary>
    [Fact]
    public async Task GivenCommentableWithNoComments_WhenGetCommentsAsync_ThenEmptyListIsReturned()
    {
        // Arrange
        var provider = this._fixture.Services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        var article = scope.ServiceProvider.GetRequiredService<TestDbContext>().Articles.Find(1) ?? throw new NullReferenceException("Article not found.");
        var commentService = scope.ServiceProvider.GetRequiredService<ICommentService<Article, IdentityUser>>();

        // Act
        var comments = await commentService.GetCommentsAsync(article).ToListAsync();

        // Assert
        Assert.NotNull(comments);
        Assert.Empty(comments);
    }
}