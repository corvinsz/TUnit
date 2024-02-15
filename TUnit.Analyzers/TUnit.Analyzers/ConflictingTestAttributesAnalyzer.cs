﻿using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace TUnit.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class ConflictingTestAttributesAnalyzer : ConcurrentDiagnosticAnalyzer
{
    private static readonly string[] TestAttributes = [ "global::TUnit.Core.TestAttribute", "global::TUnit.Core.DataDrivenTestAttribute", "global::TUnit.Core.DataSourceDrivenTestAttribute", "global::TUnit.Core.CombinativeTestAttribute"];

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
        ImmutableArray.Create(Rules.ConflictingTestAttributes);

    public override void InitializeInternal(AnalysisContext context)
    { 
        context.RegisterSyntaxNodeAction(AnalyzeSyntax, SyntaxKind.MethodDeclaration);
    }
    
    private void AnalyzeSyntax(SyntaxNodeAnalysisContext context)
    { 
        if (context.Node is not MethodDeclarationSyntax methodDeclarationSyntax)
        {
            return;
        }

        if (context.SemanticModel.GetDeclaredSymbol(methodDeclarationSyntax)
            is not { } methodSymbol)
        {
            return;
        }

        var attributes = methodSymbol.GetAttributes();

        if (attributes
                .Where(x => TestAttributes.Contains(x.AttributeClass?.ToDisplayString(DisplayFormats.FullyQualifiedNonGenericWithGlobalPrefix)))
                .GroupBy(x =>
                    x.AttributeClass?.ToDisplayString(DisplayFormats.FullyQualifiedNonGenericWithGlobalPrefix)
                    )
                .Count() > 1)
        {
            context.ReportDiagnostic(
                Diagnostic.Create(Rules.ConflictingTestAttributes,
                    methodDeclarationSyntax.GetLocation())
            );
        }
    }
}