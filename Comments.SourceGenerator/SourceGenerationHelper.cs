using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Salyam.EFUtils.Comments.SourceGenerator;

public class SourceGenerationHelper
{

    private static string GetCommenterIdField(CommentableClassInfo commentable) => 
        commentable.CommenterTypeInfo.IdPropertyIsNullable 
            ? $"public {commentable.CommenterTypeInfo.IdPropertyType} CommenterId {{ get; set; }}"
            : $"public System.Nullable<{commentable.CommenterTypeInfo.IdPropertyType}> CommenterId {{ get; set; }}" ;

    public static string GetCommentModelCode(CommentableClassInfo commentable) => 
    $$"""
    namespace Salyam.EFUtils.Comments._Models
    {
        [System.ComponentModel.DataAnnotations.Schema.Table("salyam.efutils.comments.comments")]
        public class Comment_{{commentable.TargetTypeInfo.Name}}
        {
            public int Id { get; set; }

            [System.ComponentModel.DataAnnotations.MaxLength(65535)]
            public string Text { get; set; }

            public {{commentable.TargetTypeInfo.Namespace}}.{{commentable.TargetTypeInfo.Name}} Entity { get; set; }
            public {{commentable.TargetTypeInfo.IdPropertyType}} EntityId { get; set; }

            public {{commentable.CommenterTypeInfo.Namespace}}.{{commentable.CommenterTypeInfo.Name}} Commenter { get; set; }
            {{GetCommenterIdField(commentable)}}
        }
    }
    """;

    public static string GetCommentInterfaceCode(CommentableClassInfo commentable) =>
    $$"""
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    namespace Salyam.EFUtils.Comments
    {
        /// <summary>
        /// Defines a service interface for managing comments associated with entities in the database.
        /// </summary>
        /// <typeparam name="EntityType">The type of the entity to which the comments will be applied.</typeparam>
        public interface I{{commentable.TargetTypeInfo.Name}}CommentService
        {
            /// <summary>
            /// Asynchronously adds a tag to a specified entity.
            /// </summary>
            /// <param name="entity">The entity to add the comment to.</param>
            /// <param name="commenter">The commenter of the newly added comment.</param>
            /// <param name="text">The text of the comment to be added.</param>
            /// <param name="cancellationToken">A token to cancel the operation.</param>
            /// <returns>A task representing the asynchronous operation.</returns>
            Task AddCommentAsync(
                {{commentable.TargetTypeInfo.Namespace}}.{{commentable.TargetTypeInfo.Name}} entity,
                {{commentable.CommenterTypeInfo.Namespace}}.{{commentable.CommenterTypeInfo.Name}} commenter,
                string text,
                CancellationToken cancellationToken = default
                );

            /// <summary>
            /// Asynchronously removes a comment from a specified entity.
            /// </summary>
            /// <param name="comment">The comment to be removed.</param>
            /// <param name="cancellationToken">A token to cancel the operation.</param>
            /// <returns>A task representing the asynchronous operation.</returns>
            Task RemoveCommentAsync(
                Salyam.EFUtils.Comments._Models.Comment_{{commentable.TargetTypeInfo.Name}} comment,
                CancellationToken cancellationToken = default
                );

            /// <summary>
            /// Retrieves the tags associated with the specified entity.
            /// </summary>
            /// <param name="entity">The entity whose comments are to be retrieved.</param>
            /// <param name="cancellationToken">A token to cancel the operation.</param>
            /// <returns>A query object representing the list of comments.</returns>
            IQueryable<Salyam.EFUtils.Comments._Models.Comment_{{commentable.TargetTypeInfo.Name}}> GetCommentsAsync(
                {{commentable.TargetTypeInfo.Namespace}}.{{commentable.TargetTypeInfo.Name}} entity,
                CancellationToken cancellationToken = default
                );
        }
    }
    """;

    public static string GetCommentInterfaceImplementationCode(DbContextClassInfo dbContext, CommentableClassInfo commentable) => 
    $$"""
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    namespace Salyam.EFUtils.Comments
    {
        public class {{commentable.TargetTypeInfo.Name}}CommentService({{dbContext.Namespace}}.{{dbContext.Name}} db) : I{{commentable.TargetTypeInfo.Name}}CommentService
        {
            /// <inheritdoc />
            public async Task AddCommentAsync(
                {{commentable.TargetTypeInfo.Namespace}}.{{commentable.TargetTypeInfo.Name}} entity,
                {{commentable.CommenterTypeInfo.Namespace}}.{{commentable.CommenterTypeInfo.Name}} commenter,
                string text,
                CancellationToken cancellationToken
                )
            {
                await db.Comments_{{commentable.TargetTypeInfo.Name}}.AddAsync(new () 
                    {
                        Text = text,
                        EntityId = entity.{{commentable.TargetTypeInfo.IdPropertyName}},
                        CommenterId = commenter?.{{commentable.CommenterTypeInfo.IdPropertyName}}
                    },
                    cancellationToken
                );
                await db.SaveChangesAsync(cancellationToken);
            }

            /// <inheritdoc />
            public async Task RemoveCommentAsync(
                Salyam.EFUtils.Comments._Models.Comment_{{commentable.TargetTypeInfo.Name}} comment,
                CancellationToken cancellationToken
                )
            {
                db.Comments_{{commentable.TargetTypeInfo.Name}}.Remove(comment);
                await db.SaveChangesAsync(cancellationToken);
            }

            /// <inheritdoc />
            public IQueryable<Salyam.EFUtils.Comments._Models.Comment_{{commentable.TargetTypeInfo.Name}}> GetCommentsAsync(
                {{commentable.TargetTypeInfo.Namespace}}.{{commentable.TargetTypeInfo.Name}} entity,
                CancellationToken cancellationToken
                )
            {
                return db.Comments_{{commentable.TargetTypeInfo.Name}}.Where(c => c.Entity.Id == entity.Id);
            }
        }
    }
    """;


    public static string GetNamespace(SyntaxNode cls)
    {
        // If we don't have a namespace at all we'll return an empty string
        // This accounts for the "default namespace" case
        string nameSpace = string.Empty;

        // Get the containing syntax node for the type declaration
        // (could be a nested type, for example)
        SyntaxNode? potentialNamespaceParent = cls.Parent;
        
        // Keep moving "out" of nested classes etc until we get to a namespace
        // or until we run out of parents
        while (potentialNamespaceParent != null &&
                potentialNamespaceParent is not NamespaceDeclarationSyntax
                && potentialNamespaceParent is not FileScopedNamespaceDeclarationSyntax)
        {
            potentialNamespaceParent = potentialNamespaceParent.Parent;
        }

        // Build up the final namespace by looping until we no longer have a namespace declaration
        if (potentialNamespaceParent is BaseNamespaceDeclarationSyntax namespaceParent)
        {
            // We have a namespace. Use that as the type
            nameSpace = namespaceParent.Name.ToString();
            
            // Keep moving "out" of the namespace declarations until we 
            // run out of nested namespace declarations
            while (true)
            {
                if (namespaceParent.Parent is not NamespaceDeclarationSyntax parent)
                {
                    break;
                }

                // Add the outer namespace as a prefix to the final namespace
                nameSpace = $"{namespaceParent.Name}.{nameSpace}";
                namespaceParent = parent;
            }
        }

        // return the final namespace
        return nameSpace;
    }

    public static IPropertySymbol? GetIdProperty(INamedTypeSymbol commenterType)
    {
        // Find the "Id" property inside CommenterType and its base classes
        IPropertySymbol? idProperty = null;
        for (var currentType = commenterType; currentType != null && idProperty == null; currentType = currentType.BaseType)
        {
            idProperty = currentType.GetMembers()
                .OfType<IPropertySymbol>()
                .FirstOrDefault(p => p.Name == "Id");
        }

        return idProperty;
    }
}
