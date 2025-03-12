using Salyam.EFUtils.Comments.Test.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Identity;

namespace Salyam.EFUtils.Comments.Test;

public class ServiceCollectionExtensionsTests
{
    // Test if TagService is added to ServiceCollection
    [Fact]
    public void Given_ServiceCollection_When_AddingEfCoreTagging_Should_AddCommentService()
    {
        // Arrange - Key selector function
        using var fixture = new TestFixture();

        
        // Act - use the extension method to add tagging services
        fixture.Services
            .AddEfCoreComments<TestDbContext, Book, IdentityUser>()
            .AddEfCoreComments<TestDbContext, Article, IdentityUser>();

        // Assert - check if the TagService is added with the correct type
        var bookCommentService = fixture.Services.BuildServiceProvider()
            .GetService<ICommentService<Book, IdentityUser>>();
        var articleCommentService = fixture.Services.BuildServiceProvider()
            .GetService<ICommentService<Book, IdentityUser>>();
        Assert.NotNull(bookCommentService);
        Assert.NotNull(articleCommentService);
    }

    // Test if extension method returns the original ServiceCollection
    [Fact]
    public void Given_ServiceCollection_When_AddingEfCoreTagging_Should_ReturnModifiedServiceCollection()
    {
        // Arrange - Key selector function
        using var fixture = new TestFixture();

        // Act - use the extension method and get the returned value
        var result = fixture.Services
            .AddEfCoreComments<TestDbContext, Book, IdentityUser>()
            .AddEfCoreComments<TestDbContext, Article, IdentityUser>();

        // Assert - check if the returned object is the same as the service collection
        Assert.Same(fixture.Services, result);
    }
}
