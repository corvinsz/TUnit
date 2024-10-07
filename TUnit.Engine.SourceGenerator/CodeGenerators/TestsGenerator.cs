﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TUnit.Engine.SourceGenerator.CodeGenerators.Helpers;
using TUnit.Engine.SourceGenerator.CodeGenerators.Writers;
using TUnit.Engine.SourceGenerator.Models;
using TUnit.Engine.SourceGenerator.Models.Arguments;

namespace TUnit.Engine.SourceGenerator.CodeGenerators;

[Generator]
internal class TestsGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var basicTests = context.SyntaxProvider
            .ForAttributeWithMetadataName(
                "TUnit.Core.TestAttribute",
                predicate: static (s, _) => IsSyntaxTargetForGeneration(s),
                transform: static (ctx, _) =>
                    new TestCollectionDataModel(GetSemanticTargetForGeneration(ctx)))
            .Where(static m => m is not null);
        
        context.RegisterSourceOutput(basicTests, Execute);
    }

    static bool IsSyntaxTargetForGeneration(SyntaxNode node)
    {
        return node is MethodDeclarationSyntax;
    }

    static IEnumerable<TestSourceDataModel> GetSemanticTargetForGeneration(GeneratorAttributeSyntaxContext context)
    {
        if (context.TargetSymbol is not IMethodSymbol methodSymbol)
        {
            yield break;
        }

        if (methodSymbol.ContainingType.IsAbstract)
        {
            yield break;
        }

        if (methodSymbol.IsStatic)
        {
            yield break;
        }

        if (methodSymbol.DeclaredAccessibility != Accessibility.Public)
        {
            yield break;
        }

        foreach (var testSourceDataModel in methodSymbol.ParseTestDatas(context, methodSymbol.ContainingType))
        {
            yield return testSourceDataModel;
        }
    }

    private void Execute(SourceProductionContext context, TestCollectionDataModel testCollection)
    {
        foreach (var model in testCollection.TestSourceDataModels)
        {
            var className = $"{model.MinimalTypeName}__{model.MethodName}";
            var fileName = $"{className}__{Guid.NewGuid():N}";

            using var sourceBuilder = new SourceCodeWriter();

            sourceBuilder.WriteLine("// <auto-generated/>");
            sourceBuilder.WriteLine("#pragma warning disable");
            sourceBuilder.WriteLine("using global::TUnit.Core;");
            sourceBuilder.WriteLine();
            sourceBuilder.WriteLine("namespace TUnit.SourceGenerated;");
            sourceBuilder.WriteLine();
            sourceBuilder.WriteLine("[global::System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]");
            sourceBuilder.WriteLine($"file partial class {className}");
            sourceBuilder.WriteLine("{");
            sourceBuilder.WriteLine("[global::System.Runtime.CompilerServices.ModuleInitializer]");
            sourceBuilder.WriteLine("public static void Initialise()");
            sourceBuilder.WriteLine("{");

            sourceBuilder.WriteLine($"var {VariableNames.ClassDataIndex} = 0;");
            sourceBuilder.WriteLine($"var {VariableNames.TestMethodDataIndex} = 0;");
            
            sourceBuilder.WriteLine("try");
            sourceBuilder.WriteLine("{");
            GenericTestInvocationWriter.GenerateTestInvocationCode(sourceBuilder, model);
            sourceBuilder.WriteLine("}");
            sourceBuilder.WriteLine("catch (global::System.Exception exception)");
            sourceBuilder.WriteLine("{");
            FailedTestInitializationWriter.GenerateFailedTestCode(sourceBuilder, model);
            sourceBuilder.WriteLine("}");
            
            sourceBuilder.WriteLine("}");
            sourceBuilder.WriteLine("}");

            context.AddSource($"{fileName}.Generated.cs", sourceBuilder.ToString());
        }
    }
}