using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Salyam.EFUtils.Tags.SourceGenerator;

public class SourceGenerationHelper
{
    public const string TagModelClassCode = 
    """
    namespace Salyam.EFUtils.Tags._Models
    {
        [System.ComponentModel.DataAnnotations.Schema.Table("salyam.efutils.tags.tags")]
        public class Tag
        {
            public int Id { get; set; }

            [System.ComponentModel.DataAnnotations.MaxLength(256)]
            public string Name { get; set; }

            [System.ComponentModel.DataAnnotations.MaxLength(256)]
            public string NormalizedName { get; set; }
        }
    }
    """;

    public const string TaggableInterfaceCode = 
    """
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    namespace Salyam.EFUtils.Tags
    {
        /// <summary>
        /// Defines a service interface for managing tags associated with entities in the database.
        /// </summary>
        /// <typeparam name="EntityType">The type of the entity to which the tags will be applied.</typeparam>
        public interface ITagService<EntityType>
            where EntityType : class
        {
            /// <summary>
            /// Asynchronously adds a tag to a specified entity.
            /// </summary>
            /// <param name="entity">The entity to be tagged.</param>
            /// <param name="tagName">The name of the tag to be added.</param>
            /// <param name="cancellationToken">A token to cancel the operation.</param>
            /// <returns>A task representing the asynchronous operation.</returns>
            Task TagEntityAsync(
                EntityType entity,
                string tagName,
                CancellationToken cancellationToken = default
                );

            /// <summary>
            /// Asynchronously removes a tag from a specified entity.
            /// </summary>
            /// <param name="entity">The entity from which the tag will be removed.</param>
            /// <param name="tagName">The name of the tag to be removed.</param>
            /// <param name="cancellationToken">A token to cancel the operation.</param>
            /// <returns>A task representing the asynchronous operation.</returns>
            Task UnTagEntityAsync(
                EntityType entity,
                string tagName,
                CancellationToken cancellationToken = default
                );

            /// <summary>
            /// Retrieves the tags associated with the specified entity.
            /// </summary>
            /// <param name="entity">The entity whose tags are to be retrieved.</param>
            /// <param name="cancellationToken">A token to cancel the operation.</param>
            /// <returns>A list of strings representing the tags of the entity.</returns>
            Task<List<string>> GetTagsOfEntityAsync(
                EntityType entity,
                CancellationToken cancellationToken = default
                );

            /// <summary>
            /// Retrieves entities that have been tagged with any of the specified tags.
            /// </summary>
            /// <param name="tagNames">An IEnumerable of strings representing the tag names to filter the entities by.</param>
            /// <param name="cancellationToken">A token to cancel the operation.</param>
            /// <returns>An IQueryable of EntityType representing entities that match the specified tags.</returns>
            IQueryable<EntityType> GetTaggedEntitiesByTag(
                IEnumerable<string> tagNames);
        }
    }
    """;


    public static string GetNamespace(ClassDeclarationSyntax cls)
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
