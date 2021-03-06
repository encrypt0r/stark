﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using StarkPlatform.Compiler.CodeActions;
using StarkPlatform.Compiler.Shared.Extensions;
using StarkPlatform.Compiler.Text;

namespace StarkPlatform.Compiler.CodeFixes.Async
{
#pragma warning disable RS1016 // Code fix providers should provide FixAll support. https://github.com/dotnet/roslyn/issues/23528
    internal abstract partial class AbstractAsyncCodeFix : CodeFixProvider
#pragma warning restore RS1016 // Code fix providers should provide FixAll support.
    {
        protected abstract Task<CodeAction> GetCodeActionAsync(
            SyntaxNode root, SyntaxNode node, Document document, Diagnostic diagnostic, CancellationToken cancellationToken);

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            if (!TryGetNode(root, context.Span, out var node))
            {
                return;
            }

            var diagnostic = context.Diagnostics.FirstOrDefault();

            var codeAction = await GetCodeActionAsync(
                root, node, context.Document, diagnostic, context.CancellationToken).ConfigureAwait(false);
            if (codeAction != null)
            {
                context.RegisterCodeFix(codeAction, context.Diagnostics);
            }
        }

        private bool TryGetNode(SyntaxNode root, TextSpan span, out SyntaxNode node)
        {
            node = null;
            var ancestors = root.FindToken(span.Start).GetAncestors<SyntaxNode>();
            if (!ancestors.Any())
            {
                return false;
            }

            node = ancestors.FirstOrDefault(n => n.Span.Contains(span) && n != root);
            return node != null;
        }
    }
}
