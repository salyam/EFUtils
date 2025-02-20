using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Salyam.EFUtils.Comments.Attributes;

namespace Salyam.EFUtils.Comments.SourceGenerator;

public class ClassInfo
{
    public string Name { get; private set; }
    public string Namespace { get; private set; } 
    public string Modifiers { get; private set; }

    public ClassInfo(string name, string @namespace, string modifiers)
    {
        this.Name = name;
        this.Namespace = @namespace;
        this.Modifiers = modifiers;
    }

    public ClassInfo(ClassDeclarationSyntax cls)
    {
        this.Name = cls.Identifier.ToString();
        this.Namespace = SourceGenerationHelper.GetNamespace(cls);
        this.Modifiers = cls.Modifiers.ToString();
    }
}

public class EntityClassInfo : ClassInfo
{
    public string IdPropertyType { get; set; } = "System.Int32";
    public string IdPropertyName { get; set; } = "Id";
    
    public EntityClassInfo(string name, string @namespace, string modifiers, string idPropertyType, string idPropertyName) : base(name, @namespace, modifiers)
    {
        this.IdPropertyType = idPropertyType;
        this.IdPropertyName = idPropertyName;
    }

    public EntityClassInfo(SemanticModel model, ClassDeclarationSyntax cls) : base(cls)
    {
        // Get the symbol for the class
        if (model.GetDeclaredSymbol(cls) is not INamedTypeSymbol classSymbol)
        {
            throw new NotSupportedException("Cannot find class declaration symbol in model.");
        }

        // Find the property named "Id"
        var idProperty = classSymbol.GetMembers()
                                    .OfType<IPropertySymbol>()
                                    .FirstOrDefault(p => p.Name == "Id");

        // Set ID property name and type
        if (idProperty != null)
        {
            this.IdPropertyName = idProperty.Name;
            if(!string.IsNullOrEmpty(idProperty.Type.ContainingNamespace.Name))
            {
                this.IdPropertyType = idProperty.Type.ContainingNamespace.Name + '.' + idProperty.Type.Name;
            }
            else
            {
                this.IdPropertyType = idProperty.Type.Name;
            }
        }
    }
}

public class CommentableClassInfo
{
    public EntityClassInfo TargetTypeInfo { get; private set; }

    public EntityClassInfo CommenterTypeInfo { get; private set; }

    public CommentableClassInfo(GeneratorAttributeSyntaxContext ctx)
    {
        this.TargetTypeInfo = new EntityClassInfo(ctx.SemanticModel, (ClassDeclarationSyntax)ctx.TargetNode);

        var commentableAttribute = (ctx.TargetSymbol as INamedTypeSymbol)!.GetAttributes()
            .FirstOrDefault(a => a.AttributeClass?.Name == nameof(CommentableAttribute))!;
        var commenterType = commentableAttribute.ConstructorArguments.FirstOrDefault().Value as INamedTypeSymbol;
        // Retrieve the CommenterType property from the attribute
        /*INamedTypeSymbol? commenterType = null;
        foreach (var namedArg in commentableAttribute.NamedArguments)
        {
            if (namedArg.Key == nameof(CommentableAttribute.CommenterType) && namedArg.Value.Value is INamedTypeSymbol typeSymbol)
            {
                commenterType = typeSymbol;
                break;
            }
        }*/

        // Find the "Id" property inside CommenterType
        var idProperty = commenterType!.GetMembers()
            .OfType<IPropertySymbol>()
            .FirstOrDefault(p => p.Name == "Id");
        
        var idType = idProperty.Type; // The type of the "Id" property
        this.CommenterTypeInfo = new EntityClassInfo(
            name: commenterType.Name, 
            @namespace: commenterType.ContainingNamespace.ToString(), 
            modifiers: "", 
            idPropertyType: idType.ContainingNamespace + "." + idType.Name, 
            idPropertyName: "Id");
    }
}

public class DbContextClassInfo(ClassDeclarationSyntax cls) : ClassInfo(cls)
{
}