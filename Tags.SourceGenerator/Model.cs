using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Salyam.EFUtils.Tags.SourceGenerator;

public class ClassInfo(ClassDeclarationSyntax cls)
{
    public string Name { get; private set; } = cls.Identifier.ToString();
    public string NameSpace { get; private set; } = SourceGenerationHelper.GetNamespace(cls);
    public string Modifiers { get; private set; } = cls.Modifiers.ToString();
}

public class TaggableClassInfo : ClassInfo
{
    public string IdPropertyType { get; set; } = "System.Int32";
    public string IdPropertyName { get; set; } = "Id";

    public TaggableClassInfo(SemanticModel model, ClassDeclarationSyntax cls) : base(cls)
    {
        // Get the symbol for the class
        if (model.GetDeclaredSymbol(cls) is not INamedTypeSymbol classSymbol)
        {
            throw new NotSupportedException("Cannot find class declaration symbol in model.");
        }

        // Find the property named "Id"
        var idProperty = SourceGenerationHelper.GetIdProperty(classSymbol);

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

public class DbContextClassInfo(ClassDeclarationSyntax cls) : ClassInfo(cls)
{
}