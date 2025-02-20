using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Salyam.EFUtils.Tags.Attributes;

namespace Salyam.EFUtils.Tags.SourceGenerator;

[Generator]
public class TaggableAttributeSourceGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Generate the [Taggable] attribute
        context.RegisterPostInitializationOutput(ctx => {
            ctx.AddSource(hintName: "Tag.Generated.cs", source: SourceGenerationHelper.TagModelClassCode);
            ctx.AddSource(hintName: "ITaggable.Generated.cs", source: SourceGenerationHelper.TaggableInterfaceCode);
            });

        // Collect all DbContext-derived classes
        var dbContextClasses = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (node, _) => 
                    node is ClassDeclarationSyntax cls
                    && cls.BaseList?.Types.Any(x => x.Type.ToString() == "DbContext") == true,
                transform: static (ctx, _) => new DbContextClassInfo((ClassDeclarationSyntax)ctx.Node)
            )
            .Collect();

        // Do a simple filter for enums
        var taggableClasses = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                fullyQualifiedMetadataName: typeof(TaggableAttribute).FullName,
                predicate: static (_, _) => true,
                transform: static (ctx, _) => new TaggableClassInfo(ctx.SemanticModel, (ClassDeclarationSyntax)ctx.TargetNode))
            .Collect();

        // Merge the collections and generate the final code
        var combinedData  = dbContextClasses.Combine(taggableClasses);
        
        context.RegisterSourceOutput(combinedData, GenerateCode);
    }

    public static void GenerateCode(SourceProductionContext context, (ImmutableArray<DbContextClassInfo> dbContexts, ImmutableArray<TaggableClassInfo> taggables) data)
    {
        GenerateTagModelClasses(context, data.taggables);

        if(data.dbContexts.Length > 0)
        {
        GenerateServiceCollectionExtension(context, data.dbContexts[0], data.taggables);
            GenerateTaggableServices(context, data.dbContexts[0], data.taggables);
            GenerateDbContextFields(context, data.dbContexts[0], data.taggables);
        }
    }

    private static void GenerateTagModelClasses(SourceProductionContext context, ImmutableArray<TaggableClassInfo> taggables)
    {
        foreach(var taggable in taggables)
        {
            var tagModelCode = 
                $$"""
                namespace Salyam.EFUtils.Tags._Models
                {
                    [System.ComponentModel.DataAnnotations.Schema.Table("salyam.efutils.tags.tagged_entities.{{taggable.Name.ToLowerInvariant()}}")]
                    public class TaggedEntity_{{taggable.Name}}
                    {
                        public int Id { get; set; }

                        public Tag Tag { get; set; }
                        public int TagId { get; set; }

                        public {{taggable.NameSpace}}.{{taggable.Name}} Entity { get; set; }
                        public {{taggable.IdPropertyType}} EntityId { get; set; }
                    }
                }
                """;
                context.AddSource(hintName: $"TaggedEntity_{taggable.Name}.Generated.cs", source: tagModelCode);
        }
    }

    private static void GenerateDbContextFields(SourceProductionContext context, DbContextClassInfo dbContext, ImmutableArray<TaggableClassInfo> taggables)
    {
        var sb = new StringBuilder();
        sb.AppendLine(
            $$"""
            namespace {{dbContext.NameSpace}}
            {
                {{dbContext.Modifiers}} class {{dbContext.Name}}
                {
                    public Microsoft.EntityFrameworkCore.DbSet<Salyam.EFUtils.Tags._Models.Tag> Tags { get; set; }
            """);
        foreach(var taggable in taggables)
        {
            sb.AppendLine($"        public Microsoft.EntityFrameworkCore.DbSet<Salyam.EFUtils.Tags._Models.TaggedEntity_{taggable.Name}> TaggedEntities_{taggable.Name} {{ get; set; }}");
        }

        sb.Append(
            $$"""
                }
            }
            """);
        context.AddSource(hintName: $"{dbContext.Name}.Generated.cs", source: sb.ToString());
    }

    private static void GenerateTaggableServices(SourceProductionContext context, DbContextClassInfo dbContext, ImmutableArray<TaggableClassInfo> taggables)
    {
        foreach(var taggable in taggables)
        {
            var sb = new StringBuilder();
            sb.AppendLine(
            $$"""
            using Microsoft.EntityFrameworkCore;
            using System.Collections.Generic;
            using System.Linq;
            using System.Threading;
            using System.Threading.Tasks;
            
            namespace Salyam.EFUtils.Tags._Services
            {
                public class {{taggable.Name}}_Service({{dbContext.NameSpace}}.{{dbContext.Name}} db) : Salyam.EFUtils.Tags.ITagService<{{taggable.NameSpace}}.{{taggable.Name}}>
                {
                    /// <inheritdoc/>
                    public async Task TagEntityAsync({{taggable.NameSpace}}.{{taggable.Name}} entity, string tagName, CancellationToken cancellationToken)
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

                        var taggedEntity = await db.TaggedEntities_{{taggable.Name}}
                            .SingleOrDefaultAsync(x => x.TagId == tag.Id && x.EntityId == entity.{{taggable.IdPropertyName}}, cancellationToken);
                        if (taggedEntity == null)
                        {
                            await db.TaggedEntities_{{taggable.Name}}
                                .AddAsync(new () { TagId = tag.Id, EntityId = entity.{{taggable.IdPropertyName}} }, cancellationToken);
                            await db.SaveChangesAsync(cancellationToken);
                        }
                    }

                    public async Task UnTagEntityAsync({{taggable.NameSpace}}.{{taggable.Name}} entity, string tagName, CancellationToken cancellationToken)
                    {
                        var normalizedTagName = tagName.ToUpperInvariant();
                        var tag = await db.Tags
                            .SingleOrDefaultAsync(x => x.NormalizedName == normalizedTagName, cancellationToken);
                        if (tag == null)
                            return;

                        var taggedEntity = await db.TaggedEntities_{{taggable.Name}}
                            .SingleOrDefaultAsync(x => x.TagId == tag.Id && x.EntityId == entity.{{taggable.IdPropertyName}}, cancellationToken);
                        if (taggedEntity != null)
                        {
                            db.TaggedEntities_{{taggable.Name}}
                                .Remove(taggedEntity);
                            await db.SaveChangesAsync(cancellationToken);
                        }
                    }

                    public async Task<List<string>> GetTagsOfEntityAsync({{taggable.NameSpace}}.{{taggable.Name}} entity, CancellationToken cancellationToken)
                    {
                        var query = from te in db.TaggedEntities_{{taggable.Name}}
                                    join tag in db.Tags on te.TagId equals tag.Id
                                    where te.EntityId == entity.Id
                                    select tag.Name;
                        return await query.ToListAsync(cancellationToken);
                    }

                    public IQueryable<{{taggable.NameSpace}}.{{taggable.Name}}> GetTaggedEntitiesByTag(System.Collections.Generic.IEnumerable<string> tagNames)
                    {
                        var normalizedTagNames = tagNames.Select(x => x.ToUpperInvariant()).ToList();
                        var query = from te in db.TaggedEntities_{{taggable.Name}}
                                    join tag in db.Tags on te.TagId equals tag.Id
                                    join e in db.Set<{{taggable.NameSpace}}.{{taggable.Name}}>() on te.EntityId equals e.Id
                                    where normalizedTagNames.Contains(tag.NormalizedName)
                                    select e;
                        return query;
                    }
                }
            }
            """);
            context.AddSource(hintName: $"{taggable.Name}_Service.Generated.cs", source: sb.ToString());
        }
    }

    private static void GenerateServiceCollectionExtension(SourceProductionContext context, DbContextClassInfo dbContext, ImmutableArray<TaggableClassInfo> taggables)
    {
        var sb = new StringBuilder();
        sb.AppendLine(
            """
            using Microsoft.Extensions.DependencyInjection;
            
            namespace Salyam.EFUtils.Tags
            {
                public static class ServiceCollectionExtension
                {
                    public static IServiceCollection AddEfCoreTagging(this IServiceCollection services)
                    {
            """
        );
        foreach(var taggable in taggables)
        {
            sb.AppendLine($"            services.AddTransient<ITagService<{taggable.NameSpace}.{taggable.Name}>, Salyam.EFUtils.Tags._Services.{taggable.Name}_Service>();");
        }

        sb.AppendLine(
            """
                        return services;
                    }
                }
            }
            """
        );
        context.AddSource(hintName: "ServiceCollection.TaggableExtension.Generated.cs", source:sb.ToString());
    }
}