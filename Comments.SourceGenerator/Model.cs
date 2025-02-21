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

        if (ctx.TargetSymbol is not INamedTypeSymbol nts)
            throw new NotSupportedException($"Context arget symbol is not 'INamedTypeSymbol' (context target symbol name: {ctx.TargetSymbol.Name}).");

        var commentableAttribute = nts.GetAttributes()
            .FirstOrDefault(a => a.AttributeClass?.Name == nameof(CommentableAttribute))!;

        // Retrieve the CommenterType property from the attribute
        if (commentableAttribute.ConstructorArguments.FirstOrDefault().Value is not INamedTypeSymbol commenterType)
            throw new NotSupportedException($"Cannot determines 'Commenter' (context target symbol name: {ctx.TargetSymbol.Name}).");

        // Find the "Id" property inside CommenterType and its base classes
        IPropertySymbol? idProperty = null;
        for(var currentType = commenterType; currentType != null && idProperty == null; currentType = currentType.BaseType)
        {
            idProperty = currentType.GetMembers()
                .OfType<IPropertySymbol>()
                .FirstOrDefault(p => p.Name == "Id");
        }

        if (idProperty == null)
            throw new NotSupportedException($"Cannot find id property of commenter type {commenterType.Name}.");
        
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