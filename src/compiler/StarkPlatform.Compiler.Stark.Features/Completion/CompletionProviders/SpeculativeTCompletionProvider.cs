﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using StarkPlatform.Compiler.Completion;
using StarkPlatform.Compiler.Stark.Extensions;
using StarkPlatform.Compiler.Stark.Extensions.ContextQuery;
using StarkPlatform.Compiler.Stark.Syntax;
using StarkPlatform.Compiler.Stark.Utilities;
using StarkPlatform.Compiler.ErrorReporting;
using StarkPlatform.Compiler.Options;
using StarkPlatform.Compiler.Shared.Extensions;
using StarkPlatform.Compiler.Text;

namespace StarkPlatform.Compiler.Stark.Completion.Providers
{
    internal class SpeculativeTCompletionProvider : CommonCompletionProvider
    {
        internal override bool IsInsertionTrigger(SourceText text, int characterPosition, OptionSet options)
        {
            return CompletionUtilities.IsTriggerCharacter(text, characterPosition, options);
        }

        public override async Task ProvideCompletionsAsync(CompletionContext context)
        {
            try
            {
                var document = context.Document;
                var position = context.Position;
                var cancellationToken = context.CancellationToken;

                var showSpeculativeT = await document.IsValidContextForDocumentOrLinkedDocumentsAsync(
                    (doc, ct) => ShouldShowSpeculativeTCompletionItemAsync(doc, position, ct),
                    cancellationToken).ConfigureAwait(false);

                if (showSpeculativeT)
                {
                    var text = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);

                    const string T = nameof(T);
                    context.AddItem(CommonCompletionItem.Create(
                        T, displayTextSuffix: "", CompletionItemRules.Default, glyph: Glyph.TypeParameter));
                }
            }
            catch (Exception e) when (FatalError.ReportWithoutCrashUnlessCanceled(e))
            {
                // nop
            }
        }

        private async Task<bool> ShouldShowSpeculativeTCompletionItemAsync(Document document, int position, CancellationToken cancellationToken)
        {
            var syntaxTree = await document.GetSyntaxTreeAsync(cancellationToken).ConfigureAwait(false);
            if (syntaxTree.IsInNonUserCode(position, cancellationToken) ||
                syntaxTree.IsPreProcessorDirectiveContext(position, cancellationToken))
            {
                return false;
            }

            // If we're in a generic type argument context, use the start of the generic type name
            // as the position for the rest of the context checks.
            int testPosition = position;
            var leftToken = syntaxTree.FindTokenOnLeftOfPosition(position, cancellationToken);

            var semanticModel = await document.GetSemanticModelForNodeAsync(leftToken.Parent, cancellationToken).ConfigureAwait(false);
            if (syntaxTree.IsGenericTypeArgumentContext(position, leftToken, cancellationToken, semanticModel))
            {
                // Walk out until we find the start of the partial written generic
                while (syntaxTree.IsInPartiallyWrittenGeneric(testPosition, cancellationToken, out var nameToken))
                {
                    testPosition = nameToken.SpanStart;
                }

                // If the user types Goo<T, automatic brace completion will insert the close brace
                // and the generic won't be "partially written".
                if (testPosition == position)
                {
                    var typeArgumentList = leftToken.GetAncestor<TypeArgumentListSyntax>();
                    if (typeArgumentList != null)
                    {
                        if (typeArgumentList.LessThanToken != default && typeArgumentList.GreaterThanToken != default)
                        {
                            testPosition = typeArgumentList.LessThanToken.SpanStart;
                        }
                    }
                }
            }

            if ((!leftToken.GetPreviousTokenIfTouchingWord(position).IsKindOrHasMatchingText(SyntaxKind.AsyncKeyword) &&
                syntaxTree.IsMemberDeclarationContext(testPosition, contextOpt: null, validModifiers: SyntaxKindSet.AllMemberModifiers, validTypeDeclarations: SyntaxKindSet.ClassInterfaceStructTypeDeclarations, canBePartial: false, cancellationToken: cancellationToken)) ||
                syntaxTree.IsStatementContext(testPosition, leftToken, cancellationToken) ||
                syntaxTree.IsGlobalMemberDeclarationContext(testPosition, SyntaxKindSet.AllGlobalMemberModifiers, cancellationToken) ||
                syntaxTree.IsGlobalStatementContext(testPosition, cancellationToken) ||
                syntaxTree.IsDelegateReturnTypeContext(testPosition, syntaxTree.FindTokenOnLeftOfPosition(testPosition, cancellationToken), cancellationToken))
            {
                return true;
            }

            return false;
        }
    }
}
