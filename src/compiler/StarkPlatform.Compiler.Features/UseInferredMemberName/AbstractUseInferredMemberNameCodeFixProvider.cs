﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using StarkPlatform.Compiler.CodeActions;
using StarkPlatform.Compiler.CodeFixes;
using StarkPlatform.Compiler.Diagnostics;
using StarkPlatform.Compiler.Editing;
using Roslyn.Utilities;

namespace StarkPlatform.Compiler.UseInferredMemberName
{
    internal abstract class AbstractUseInferredMemberNameCodeFixProvider : SyntaxEditorBasedCodeFixProvider
    {
        abstract protected void LanguageSpecificRemoveSuggestedNode(SyntaxEditor editor, SyntaxNode node);

        public override ImmutableArray<string> FixableDiagnosticIds { get; }
            = ImmutableArray.Create(IDEDiagnosticIds.UseInferredMemberNameDiagnosticId);

        public override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            context.RegisterCodeFix(new MyCodeAction(
                c => FixAsync(context.Document, context.Diagnostics.First(), c)),
                context.Diagnostics);

            return Task.CompletedTask;
        }

        protected override Task FixAllAsync(
            Document document, ImmutableArray<Diagnostic> diagnostics,
            SyntaxEditor editor, CancellationToken cancellationToken)
        {
            var root = editor.OriginalRoot;

            foreach (var diagnostic in diagnostics)
            {
                var node = root.FindNode(diagnostic.Location.SourceSpan);
                LanguageSpecificRemoveSuggestedNode(editor, node);
            }

            return Task.CompletedTask;
        }


        private class MyCodeAction : CodeAction.DocumentChangeAction
        {
            public MyCodeAction(Func<CancellationToken, Task<Document>> createChangedDocument)
                : base(FeaturesResources.Use_inferred_member_name, createChangedDocument, FeaturesResources.Use_inferred_member_name)
            {
            }
        }
    }
}
