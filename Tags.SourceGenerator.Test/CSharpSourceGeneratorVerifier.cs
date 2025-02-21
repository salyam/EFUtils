using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;

namespace Salyam.EFUtils.Tags.SourceGenerator.Test;

public static class CSharpSourceGeneratorVerifier<TSourceGenerator>
where TSourceGenerator : IIncrementalGenerator, new()
{
    public class Test : CSharpSourceGeneratorTest<TSourceGenerator, DefaultVerifier>
    {
        public Test()
        {
            ReferenceAssemblies = ReferenceAssemblies.Net.Net90
                .AddPackages(ImmutableArray.Create(
                    new PackageIdentity("Microsoft.EntityFrameworkCore", "9.0"), 
                    new PackageIdentity("Microsoft.AspNetCore.Identity.EntityFrameworkCore", "9.0")
            ));

            // Add a reference to a local project
            var coreProjectDll = Path.Combine("..", "..", "..", "..", "Tags.Attributes", "bin", "Debug", "netstandard2.0", "Tags.Attributes.dll");
            if (File.Exists(coreProjectDll))
            {
                TestState.AdditionalReferences.Add(MetadataReference.CreateFromFile(coreProjectDll));
            }
        }

        protected override CompilationOptions CreateCompilationOptions()
        {
        var compilationOptions = base.CreateCompilationOptions();
        return compilationOptions.WithSpecificDiagnosticOptions(
                compilationOptions.SpecificDiagnosticOptions.SetItems(GetNullableWarningsFromCompiler()));
        }

        public LanguageVersion LanguageVersion { get; set; } = LanguageVersion.Default;

        private static ImmutableDictionary<string, ReportDiagnostic> GetNullableWarningsFromCompiler()
        {
            string[] args = { "/warnaserror:nullable" };
            var commandLineArguments = CSharpCommandLineParser.Default.Parse(args, baseDirectory: Environment.CurrentDirectory, sdkDirectory: Environment.CurrentDirectory);
            var nullableWarnings = commandLineArguments.CompilationOptions.SpecificDiagnosticOptions;

            return nullableWarnings;
        }

        protected override ParseOptions CreateParseOptions()
        {
            return ((CSharpParseOptions)base.CreateParseOptions()).WithLanguageVersion(LanguageVersion);
        }
    }
}
