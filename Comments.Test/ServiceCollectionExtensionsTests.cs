using Salyam.EFUtils.Comments.Test.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Salyam.EFUtils.Comments;

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
        fixture.Services.AddEfCoreComments();

        // Assert - check if the TagService is added with the correct type
        var tagService = fixture.Services.BuildServiceProvider()
            .GetService<IBookCommentService>();
        Assert.NotNull(tagService);
    }

    // Test if extension method returns the original ServiceCollection
    [Fact]
    public void Given_ServiceCollection_When_AddingEfCoreTagging_Should_ReturnModifiedServiceCollection()
    {
        // Arrange - Key selector function
        using var fixture = new TestFixture();

        // Act - use the extension method and get the returned value
        var result = fixture.Services.AddEfCoreComments();

        // Assert - check if the returned object is the same as the service collection
        Assert.Same(fixture.Services, result);
    }
}
