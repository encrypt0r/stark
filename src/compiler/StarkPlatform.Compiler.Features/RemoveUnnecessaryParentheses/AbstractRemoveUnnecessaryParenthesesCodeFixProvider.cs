﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using StarkPlatform.Compiler.CodeActions;
using StarkPlatform.Compiler.CodeFixes;
using StarkPlatform.Compiler.Diagnostics;
using StarkPlatform.Compiler.Editing;
using StarkPlatform.Compiler.LanguageServices;
using StarkPlatform.Compiler.Shared.Extensions;
using Roslyn.Utilities;

namespace StarkPlatform.Compiler.RemoveUnnecessaryParentheses
{
    internal abstract class AbstractRemoveUnnecessaryParenthesesCodeFixProvider<TParenthesizedExpressionSyntax>
        : SyntaxEditorBasedCodeFixProvider
        where TParenthesizedExpressionSyntax : SyntaxNode
    {
        public override ImmutableArray<string> FixableDiagnosticIds
           => ImmutableArray.Create(IDEDiagnosticIds.RemoveUnnecessaryParenthesesDiagnosticId);

        protected abstract bool CanRemoveParentheses(TParenthesizedExpressionSyntax current, SemanticModel semanticModel);

        public override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            context.RegisterCodeFix(
                new MyCodeAction(
                    c => FixAsync(context.Document, context.Diagnostics[0], c)),
                    context.Diagnostics);
            return Task.CompletedTask;
        }

        protected override Task FixAllAsync(
            Document document, ImmutableArray<Diagnostic> diagnostics,
            SyntaxEditor editor, CancellationToken cancellationToken)
        {
            var syntaxFacts = document.GetLanguageService<ISyntaxFactsService>();
            var originalNodes = diagnostics.SelectAsArray(
                d => (TParenthesizedExpressionSyntax)d.AdditionalLocations[0].FindNode(
                    findInsideTrivia: true, getInnermostNodeForTie: true, cancellationToken));

            return editor.ApplyExpressionLevelSemanticEditsAsync(
                document, originalNodes,
                (semanticModel, current) => current != null && CanRemoveParentheses(current, semanticModel),
                (_, currentRoot, current) => currentRoot.ReplaceNode(current, syntaxFacts.Unparenthesize(current)),
                cancellationToken);
        }

        private class MyCodeAction : CodeAction.DocumentChangeAction
        {
            public MyCodeAction(Func<CancellationToken, Task<Document>> createChangedDocument)
                : base(FeaturesResources.Remove_unnecessary_parentheses, createChangedDocument, FeaturesResources.Remove_unnecessary_parentheses)
            {
            }
        }
    }
}
