using EFTagTest.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Salyam.EFUtils.Tags;

namespace EFTagTest;

public class TagServiceTests
{
    private readonly TestFixture _fixture;

    public TagServiceTests()
    {
        this._fixture = new TestFixture();
        this._fixture.Services
            .AddEfCoreTags<TestDbContext, Book>()
            .AddEfCoreTags<TestDbContext, Article>();
    }

    [Fact]
    public async Task Given_EntityAndTagName_When_EntityIsTagged_Should_CreateTaggedEntity()
    {
        // Arrange: create database and query test book instance
        var provider = this._fixture.Services.BuildServiceProvider();
        var db = provider.GetRequiredService<TestDbContext>();
        var tagService = provider.GetRequiredService<ITagService<Book>>();

        var book = await db.Books.SingleAsync(x => x.Id == this._fixture.SeededBooks[0].Id);

        // Act: tag the test book instance
        await tagService.SetTagsAsync(book, ["Tag"]);

        // Assert: query the tagged entites and check whether entity is tagged
        var taggedEntity = await (db as ITagDbContext<Book>).TaggedEntities
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
        var provider = this._fixture.Services.BuildServiceProvider();
        var db = provider.GetRequiredService<TestDbContext>();
        var tagService = provider.GetRequiredService<ITagService<Book>>();

        var book = await db.Books.SingleAsync(x => x.Id == this._fixture.SeededBooks[0].Id);

        // Act: tag and untag the test book instance
        await tagService.SetTagsAsync(book, ["Tag"]);
        await tagService.SetTagsAsync(book, []);

        // Assert: query the tagged entites and check whether entity is tagged
        var taggedEntity = await (db as ITagDbContext<Book>).TaggedEntities
            .Include(x => x.Entity)
            .Include(x => x.Tag)
            .SingleOrDefaultAsync(x => x.Tag.Name == "Tag");

        Assert.Null(taggedEntity);
    }

    [Fact]
    public async Task Given_TaggedEntity_When_TagsAreQueried_Should_ReturnTags()
    {
        // Arrange: create database and query test book instance
        var provider = this._fixture.Services.BuildServiceProvider();
        var db = provider.GetRequiredService<TestDbContext>();
        var tagService = provider.GetRequiredService<ITagService<Book>>();

        var book = await db.Books.SingleAsync(x => x.Id == this._fixture.SeededBooks[0].Id);

        // Act: tag and untag the test book instance
        await tagService.SetTagsAsync(book, ["Tag1", "Tag2"]);
        var tags = await tagService.GetTagsAsync(book);

        // Assert: query the tagged entites and check whether entity is tagged
        Assert.Equal(2, tags.Count);
        Assert.Equal("Tag1", tags[0]);
        Assert.Equal("Tag2", tags[1]);
    }

    [Fact]
    public async Task Given_TaggedEntity_When_TaggedEntitiesAreQueried_Should_ReturnTags()
    {
        // Arrange: create database and query test book instance
        var provider = this._fixture.Services.BuildServiceProvider();
        var db = provider.GetRequiredService<TestDbContext>();
        var tagService = provider.GetRequiredService<ITagService<Book>>();

        var book = await db.Books.SingleAsync(x => x.Id == this._fixture.SeededBooks[0].Id);

        // Act: tag and untag the test book instance
        await tagService.SetTagsAsync(book, ["Tag"]);
        var taggedEntities = await tagService.GetTaggedEntitiesAsync([ "Tag" ]).ToListAsync();

        // Assert: query the tagged entites and check whether entity is tagged
        Assert.Single(taggedEntities);
        Assert.Equal(book.Title, taggedEntities[0].Title);
    }
}