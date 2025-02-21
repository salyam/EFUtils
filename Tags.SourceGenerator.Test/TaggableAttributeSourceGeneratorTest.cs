using CodeVerifier = Salyam.EFUtils.Tags.SourceGenerator.Test.CSharpSourceGeneratorVerifier<Salyam.EFUtils.Tags.SourceGenerator.TaggableAttributeSourceGenerator>;

namespace Salyam.EFUtils.Tags.SourceGenerator.Test;

public class TaggableAttributeSourceGeneratorTest
{
    private const string BookCode = 
        """
        namespace Utest.Example;

        [Salyam.EFUtils.Tags.Attributes.Taggable]
        public partial class Book
        {
            public string Id { get; set; }

            public string Title { get; set; }

            public string Author { get; set; }
        }
        """;

    private const string DbContextCode = 
        """
        using Microsoft.EntityFrameworkCore;
        using System;
        using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

        namespace Utest.Example;

        public partial class TestDbContext : IdentityDbContext
        {
            public DbSet<Book> Books { get; set; }
        }
        """;

    [Fact]
    public async Task Given_ACompilationUnit_When_Generated_Should_GenerateAttribute()
    {
        // Arrange: set the expected generated code

        // Act & Assert: Run the code generator, check the generated source file's name, the generated source code an the number of error messages
        await new CodeVerifier.Test
        {
            TestState = 
            {
                Sources = { BookCode, DbContextCode },
                GeneratedSources =
                {
                    (
                        sourceGeneratorType: typeof(TaggableAttributeSourceGenerator), 
                        filename: "Tag.Generated.cs", 
                        content: SourceGenerationHelper.TagModelClassCode
                    ),
                    (
                        sourceGeneratorType: typeof(TaggableAttributeSourceGenerator),
                        filename: "ITaggable.Generated.cs",
                        content: SourceGenerationHelper.TaggableInterfaceCode
                    ),
                    (
                        sourceGeneratorType: typeof(TaggableAttributeSourceGenerator), 
                        filename: "TaggedEntity_Book.Generated.cs", 
                        content: 
                        """
                        namespace Salyam.EFUtils.Tags._Models
                        {
                            [System.ComponentModel.DataAnnotations.Schema.Table("salyam.efutils.tags.tagged_entities.book")]
                            public class TaggedEntity_Book
                            {
                                public int Id { get; set; }

                                public Tag Tag { get; set; }
                                public int TagId { get; set; }

                                public Utest.Example.Book Entity { get; set; }
                                public System.String EntityId { get; set; }
                            }
                        }
                        """
                    ),
                    (
                        sourceGeneratorType: typeof(TaggableAttributeSourceGenerator),
                        filename: "ServiceCollection.TaggableExtension.Generated.cs",
                        content: 
                        """
                        using Microsoft.Extensions.DependencyInjection;
                        
                        namespace Salyam.EFUtils.Tags
                        {
                            public static class ServiceCollectionExtension
                            {
                                public static IServiceCollection AddEfCoreTagging(this IServiceCollection services)
                                {
                                    services.AddTransient<ITagService<Utest.Example.Book>, Salyam.EFUtils.Tags._Services.Book_Service>();
                                    return services;
                                }
                            }
                        }
                        
                        """
                    ),
                    (
                        sourceGeneratorType: typeof(TaggableAttributeSourceGenerator), 
                        filename: "Book_Service.Generated.cs", 
                        content: 
                        """
                        using Microsoft.EntityFrameworkCore;
                        using System.Collections.Generic;
                        using System.Linq;
                        using System.Threading;
                        using System.Threading.Tasks;

                        namespace Salyam.EFUtils.Tags._Services
                        {
                            public class Book_Service(Utest.Example.TestDbContext db) : Salyam.EFUtils.Tags.ITagService<Utest.Example.Book>
                            {
                                /// <inheritdoc/>
                                public async Task TagEntityAsync(Utest.Example.Book entity, string tagName, CancellationToken cancellationToken)
                                {
                                    var normalizedTagName = tagName.ToUpperInvariant();
                                    var tag = await db.Tags
                                        .SingleOrDefaultAsync(x => x.NormalizedName == normalizedTagName, cancellationToken);
                                    if (tag == null)
                                    {
                                        var tagEntity = await db.Tags
                                            .AddAsync(new Salyam.EFUtils.Tags._Models.Tag(){ Name = tagName, NormalizedName = normalizedTagName}, cancellationToken);
                                        await db.SaveChangesAsync(cancellationToken);
                                        tag = tagEntity.Entity;
                                    }

                                    var taggedEntity = await db.TaggedEntities_Book
                                        .SingleOrDefaultAsync(x => x.TagId == tag.Id && x.EntityId == entity.Id, cancellationToken);
                                    if (taggedEntity == null)
                                    {
                                        await db.TaggedEntities_Book
                                            .AddAsync(new () { TagId = tag.Id, EntityId = entity.Id }, cancellationToken);
                                        await db.SaveChangesAsync(cancellationToken);
                                    }
                                }
                        
                                public async Task UnTagEntityAsync(Utest.Example.Book entity, string tagName, CancellationToken cancellationToken)
                                {
                                    var normalizedTagName = tagName.ToUpperInvariant();
                                    var tag = await db.Tags
                                        .SingleOrDefaultAsync(x => x.NormalizedName == normalizedTagName, cancellationToken);
                                    if (tag == null)
                                        return;

                                    var taggedEntity = await db.TaggedEntities_Book
                                        .SingleOrDefaultAsync(x => x.TagId == tag.Id && x.EntityId == entity.Id, cancellationToken);
                                    if (taggedEntity != null)
                                    {
                                        db.TaggedEntities_Book
                                            .Remove(taggedEntity);
                                        await db.SaveChangesAsync(cancellationToken);
                                    }
                                }
                        
                                public async Task<List<string>> GetTagsOfEntityAsync(Utest.Example.Book entity, CancellationToken cancellationToken)
                                {
                                    var query = from te in db.TaggedEntities_Book
                                                join tag in db.Tags on te.TagId equals tag.Id
                                                where te.EntityId == entity.Id
                                                select tag.Name;
                                    return await query.ToListAsync(cancellationToken);
                                }
                        
                                public IQueryable<Utest.Example.Book> GetTaggedEntitiesByTag(System.Collections.Generic.IEnumerable<string> tagNames)
                                {
                                    var normalizedTagNames = tagNames.Select(x => x.ToUpperInvariant()).ToList();
                                    var query = from te in db.TaggedEntities_Book
                                                join tag in db.Tags on te.TagId equals tag.Id
                                                join e in db.Set<Utest.Example.Book>() on te.EntityId equals e.Id
                                                where normalizedTagNames.Contains(tag.NormalizedName)
                                                select e;
                                    return query;
                                }
                            }
                        }

                        """
                    ),
                    (
                        sourceGeneratorType: typeof(TaggableAttributeSourceGenerator), 
                        filename: "TestDbContext.Generated.cs", 
                        content: 
                        """
                        namespace Utest.Example
                        {
                            public partial class TestDbContext
                            {
                                public Microsoft.EntityFrameworkCore.DbSet<Salyam.EFUtils.Tags._Models.Tag> Tags { get; set; }
                                public Microsoft.EntityFrameworkCore.DbSet<Salyam.EFUtils.Tags._Models.TaggedEntity_Book> TaggedEntities_Book { get; set; }
                            }
                        }
                        """
                    )
                },
            },
        }.RunAsync();
    }
}
