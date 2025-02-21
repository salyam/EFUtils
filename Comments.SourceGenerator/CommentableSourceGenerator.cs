using System;
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
    private static bool InheritsFromDbContext(ClassDeclarationSyntax classDeclaration, SemanticModel semanticModel)
    {
        if (classDeclaration.BaseList == null)
            return false;

        foreach (var baseType in classDeclaration.BaseList.Types)
        {
            var baseTypeSymbol = semanticModel.GetTypeInfo(baseType.Type).Type;

            // Traverse the inheritance hierarchy
            while (baseTypeSymbol != null)
            {
                if (baseTypeSymbol.ToString() == "Microsoft.EntityFrameworkCore.DbContext")
                    return true;

                baseTypeSymbol = baseTypeSymbol.BaseType;
            }
        }

        return false;
    }

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Collect all classes that are derived from something
        var classDeclarations = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (node, _) => 
                    node is ClassDeclarationSyntax cls 
                    && cls.BaseList != null 
                    && cls.BaseList.Types.Count > 0,
                transform: static (ctx, _) => (ClassDeclarationSyntax)ctx.Node
            )
            .Collect();
        
        // Do a simple filter for enums
        var commentableClasses = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                fullyQualifiedMetadataName: typeof(CommentableAttribute).FullName,
                predicate: static (_, _) => true,
                transform: static (ctx, _) => new CommentableClassInfo(ctx))
            .Collect();

        // Merge the collections and generate the final code
        var combinedData = context.CompilationProvider.Combine(classDeclarations.Combine(commentableClasses));
        
        context.RegisterSourceOutput(combinedData, (ctx, source )=> 
        {
            var (compilation, (classDeclarations, commentableClassInfos)) = source;
            var dbContextClasses = classDeclarations
                .Where(x => InheritsFromDbContext(x, compilation.GetSemanticModel(x.SyntaxTree)))
                .Select(x => new DbContextClassInfo(x))
                .ToList();
            if (dbContextClasses.Count == 0)
            {
                ctx.ReportDiagnostic(Diagnostic.Create(new DiagnosticDescriptor(
                    id: "EFUTILS001",
                    title: "No DbContext descendant found",
                    messageFormat: $"No DbContext descendant was found.",
                    category: "Usage",
                    defaultSeverity: DiagnosticSeverity.Error,
                    isEnabledByDefault: true
                    ),
                    classDeclarations[0].GetLocation()
                    ));
            }
            else
            {
                GenerateCode(ctx, dbContextClasses[0], commentableClassInfos);
            }
        });
    }

    public static void GenerateCode(SourceProductionContext context, DbContextClassInfo dbContextClassInfo, ImmutableArray<CommentableClassInfo> commentables)
    {        
        foreach (var commentable in commentables)
        {
            context.AddSource($"{commentable.TargetTypeInfo.Name}.CommentModel.Generated.cs", 
                source: SourceGenerationHelper.GetCommentModelCode(commentable));
            context.AddSource($"{commentable.TargetTypeInfo.Name}.CommentServiceInterface.Generated.cs", 
                source: SourceGenerationHelper.GetCommentInterfaceCode(commentable));
            context.AddSource($"{commentable.TargetTypeInfo.Name}.CommentServiceImplementation.Generated.cs", 
                source: SourceGenerationHelper.GetCommentInterfaceImplementationCode(dbContextClassInfo, commentable));
        }

        GenerateDbContextFields(context, dbContextClassInfo, commentables);
        GenerateServiceCollectionExtension(context, commentables);
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