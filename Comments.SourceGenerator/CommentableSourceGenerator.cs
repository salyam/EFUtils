using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using Salyam.EFUtils.Comments.Attributes;

namespace Salyam.EFUtils.Comments.SourceGenerator;

[Generator]
public class CommentableSourceGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
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
                fullyQualifiedMetadataName: typeof(CommentableAttribute).FullName,
                predicate: static (_, _) => true,
                transform: static (ctx, _) => new CommentableClassInfo(ctx))
            .Collect();

        // Merge the collections and generate the final code
        var combinedData  = dbContextClasses.Combine(taggableClasses);
        
        context.RegisterSourceOutput(combinedData, GenerateCode);
    }

    public static void GenerateCode(SourceProductionContext context, (ImmutableArray<DbContextClassInfo> dbContexts, ImmutableArray<CommentableClassInfo> commentables) data)
    {
        if (data.dbContexts.Length == 0)
            return;
        
        foreach (var commentable in data.commentables)
        {
            context.AddSource($"{commentable.TargetTypeInfo.Name}.CommentModel.Generated.cs", 
                source: SourceGenerationHelper.GetCommentModelCode(commentable));
            context.AddSource($"{commentable.TargetTypeInfo.Name}.CommentServiceInterface.Generated.cs", 
                source: SourceGenerationHelper.GetCommentInterfaceCode(commentable));
            context.AddSource($"{commentable.TargetTypeInfo.Name}.CommentServiceImplementation.Generated.cs", 
                source: SourceGenerationHelper.GetCommentInterfaceImplementationCode(data.dbContexts[0], commentable));
        }

        GenerateDbContextFields(context, data.dbContexts[0], data.commentables);
        GenerateServiceCollectionExtension(context, data.commentables);
    }

    private static void GenerateDbContextFields(SourceProductionContext context, DbContextClassInfo dbContext, ImmutableArray<CommentableClassInfo> commentables)
    {
        var sb = new StringBuilder();
        sb.AppendLine(
            $$"""
            using Microsoft.EntityFrameworkCore;
            using Salyam.EFUtils.Comments._Models;

            namespace {{dbContext.Namespace}}
            {
                {{dbContext.Modifiers}} class {{dbContext.Name}}
                {
            """);
        foreach(var commentable in commentables)
        {
            sb.AppendLine($"        public DbSet<Comment_{commentable.TargetTypeInfo.Name}> Comments_{commentable.TargetTypeInfo.Name} {{ get; set; }}");
        }

        sb.Append(
            $$"""
                }
            }
            """);
        context.AddSource(hintName: $"{dbContext.Name}.CommentFieldExtensions.Generated.cs", source: sb.ToString());
    }

    private static void GenerateServiceCollectionExtension(SourceProductionContext context, ImmutableArray<CommentableClassInfo> commentables)
    {
        var sb = new StringBuilder();
        sb.AppendLine(
            """
            using Microsoft.Extensions.DependencyInjection;
            
            namespace Salyam.EFUtils.Comments
            {
                public static class ServiceCollectionExtension
                {
                    public static IServiceCollection AddEfCoreComments(this IServiceCollection services)
                    {
            """
        );
        foreach(var commentable in commentables)
        {
            sb.AppendLine($"            services.AddTransient<I{commentable.TargetTypeInfo.Name}CommentService, {commentable.TargetTypeInfo.Name}CommentService>();");
        }

        sb.AppendLine(
            """
                        return services;
                    }
                }
            }
            """
        );
        context.AddSource(hintName: "ServiceCollection.CommentableExtension.Generated.cs", source:sb.ToString());
    }
}