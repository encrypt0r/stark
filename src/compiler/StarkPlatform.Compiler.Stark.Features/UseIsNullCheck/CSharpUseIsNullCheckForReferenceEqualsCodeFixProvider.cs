﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Composition;
using StarkPlatform.Compiler.CodeFixes;
using StarkPlatform.Compiler.Stark.Extensions;
using StarkPlatform.Compiler.Stark.Syntax;
using StarkPlatform.Compiler.UseIsNullCheck;

namespace StarkPlatform.Compiler.Stark.UseIsNullCheck
{
    [ExportCodeFixProvider(LanguageNames.Stark), Shared]
    internal class CSharpUseIsNullCheckForReferenceEqualsCodeFixProvider : AbstractUseIsNullCheckForReferenceEqualsCodeFixProvider
    {
        protected override string GetIsNullTitle()
            => CSharpFeaturesResources.Use_is_null_check;

        protected override string GetIsNotNullTitle()
            => GetIsNullTitle();

        private static SyntaxNode CreateEqualsNullCheck(SyntaxNode argument, SyntaxKind comparisonOperator)
            => SyntaxFactory.BinaryExpression(
                comparisonOperator,
                (ExpressionSyntax)argument,
                SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression)).Parenthesize();

        private static SyntaxNode CreateIsNullCheck(SyntaxNode argument)
            => SyntaxFactory.IsPatternExpression(
                (ExpressionSyntax)argument,
                SyntaxFactory.ConstantPattern(SyntaxFactory.LiteralExpression(SyntaxKind.NullLiteralExpression))).Parenthesize();

        private static SyntaxNode CreateIsNotNullCheck(SyntaxNode notExpression, SyntaxNode argument)
            => ((PrefixUnaryExpressionSyntax)notExpression).WithOperand((ExpressionSyntax)CreateIsNullCheck(argument));

        protected override SyntaxNode CreateNullCheck(SyntaxNode argument, bool isUnconstrainedGeneric)
            => isUnconstrainedGeneric
                ? CreateEqualsNullCheck(argument, SyntaxKind.EqualsExpression)
                : CreateIsNullCheck(argument);

        protected override SyntaxNode CreateNotNullCheck(SyntaxNode notExpression, SyntaxNode argument, bool isUnconstrainedGeneric)
            => isUnconstrainedGeneric
                ? CreateEqualsNullCheck(argument, SyntaxKind.NotEqualsExpression)
                : CreateIsNotNullCheck(notExpression, argument);
    }
}
