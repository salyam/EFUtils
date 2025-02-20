using EFTagTest.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Salyam.EFTag;

namespace EFTagTest;

public class TagServiceTests
{
    [Fact]
    public async Task Given_EntityAndTagName_When_EntityIsTagged_Should_CreateTaggedEntity()
    {
        // Arrange: create database and query test book instance
        using var fixture = new TestFixture();
        fixture.Services.AddEfCoreTagging();
        var db = fixture.Services.BuildServiceProvider().GetRequiredService<TestDbContext>();
        var tagService = fixture.Services.BuildServiceProvider().GetRequiredService<ITagService<Book>>();

        var book = await db.Books.SingleAsync(x => x.Id == 1);

        // Act: tag the test book instance
        await tagService.TagEntityAsync(book, "Tag");

        // Assert: query the tagged entites and check whether entity is tagged
        var taggedEntity = await db.TaggedEntities_Book
            .Include(x => x.Entity)
            .Include(x => x.Tag)
            .SingleOrDefaultAsync(x => x.Tag.Name == "Tag");

        Assert.NotNull(taggedEntity);
        Assert.Equal(book.Title, taggedEntity.Entity.Title);
        Assert.Equal("Tag", taggedEntity.Tag.Name);
    }

    [Fact]
    public async Task Given_EntityAndTagName_When_EntityIsUntagged_Should_RemoveTaggedEntity()
    {
        // Arrange: create database and query test book instance
        using var fixture = new TestFixture();
        fixture.Services.AddEfCoreTagging();
        var db = fixture.Services.BuildServiceProvider().GetRequiredService<TestDbContext>();
        var tagService = fixture.Services.BuildServiceProvider().GetRequiredService<ITagService<Book>>();

        var book = await db.Books.SingleAsync(x => x.Id == 1);

        // Act: tag and untag the test book instance
        await tagService.TagEntityAsync(book, "Tag");
        await tagService.UnTagEntityAsync(book, "Tag");

        // Assert: query the tagged entites and check whether entity is tagged
        var taggedEntity = await db.TaggedEntities_Book
            .Include(x => x.Entity)
            .Include(x => x.Tag)
            .SingleOrDefaultAsync(x => x.Tag.Name == "Tag");

        Assert.Null(taggedEntity);
    }

    [Fact]
    public async Task Given_TaggedEntity_When_TagsAreQueried_Should_ReturnTags()
    {
        // Arrange: create database and query test book instance
        using var fixture = new TestFixture();
        fixture.Services.AddEfCoreTagging();
        var db = fixture.Services.BuildServiceProvider().GetRequiredService<TestDbContext>();
        var tagService = fixture.Services.BuildServiceProvider().GetRequiredService<ITagService<Book>>();

        var book = await db.Books.SingleAsync(x => x.Id == 1);

        // Act: tag and untag the test book instance
        await tagService.TagEntityAsync(book, "Tag1");
        await tagService.TagEntityAsync(book, "Tag2");
        var tags = await tagService.GetTagsOfEntityAsync(book);

        // Assert: query the tagged entites and check whether entity is tagged
        Assert.Equal(2, tags.Count);
        Assert.Equal("Tag1", tags[0]);
        Assert.Equal("Tag2", tags[1]);
    }

    [Fact]
    public async Task Given_TaggedEntity_When_TaggedEntitiesAreQueried_Should_ReturnTags()
    {
        // Arrange: create database and query test book instance
        using var fixture = new TestFixture();
        fixture.Services.AddEfCoreTagging();
        var db = fixture.Services.BuildServiceProvider().GetRequiredService<TestDbContext>();
        var tagService = fixture.Services.BuildServiceProvider().GetRequiredService<ITagService<Book>>();

        var book = await db.Books.SingleAsync(x => x.Id == 1);

        // Act: tag and untag the test book instance
        await tagService.TagEntityAsync(book, "Tag");
        var taggedEntities = await tagService.GetTaggedEntitiesByTag([ "Tag" ]).ToListAsync();

        // Assert: query the tagged entites and check whether entity is tagged
        Assert.Single(taggedEntities);
        Assert.Equal(book.Title, taggedEntities[0].Title);
    }
}