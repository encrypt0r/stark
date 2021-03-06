﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Diagnostics;
using StarkPlatform.Compiler.Stark.Syntax;
using Roslyn.Utilities;

namespace StarkPlatform.Compiler.Stark
{
    internal static class SyntaxNodeExtensions
    {
        public static TNode WithAnnotations<TNode>(this TNode node, params SyntaxAnnotation[] annotations) where TNode : CSharpSyntaxNode
        {
            return (TNode)node.Green.SetAnnotations(annotations).CreateRed();
        }

        public static bool IsAnonymousFunction(this SyntaxNode syntax)
        {
            Debug.Assert(syntax != null);
            switch (syntax.Kind())
            {
                case SyntaxKind.ParenthesizedLambdaExpression:
                case SyntaxKind.SimpleLambdaExpression:
                case SyntaxKind.AnonymousMethodExpression:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsQuery(this SyntaxNode syntax)
        {
            Debug.Assert(syntax != null);
            switch (syntax.Kind())
            {
                case SyntaxKind.FromClause:
                case SyntaxKind.GroupClause:
                case SyntaxKind.JoinClause:
                case SyntaxKind.JoinIntoClause:
                case SyntaxKind.LetClause:
                case SyntaxKind.OrderByClause:
                case SyntaxKind.QueryContinuation:
                case SyntaxKind.QueryExpression:
                case SyntaxKind.SelectClause:
                case SyntaxKind.WhereClause:
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// This method is used to keep the code that generates binders in sync
        /// with the code that searches for binders.  We don't want the searcher
        /// to skip over any nodes that could have associated binders, especially
        /// if changes are made later.
        /// 
        /// "Local binder" is a term that refers to binders that are
        /// created by LocalBinderFactory.
        /// </summary>
        internal static bool CanHaveAssociatedLocalBinder(this SyntaxNode syntax)
        {
            SyntaxKind kind = syntax.Kind();
            switch (kind)
            {
                case SyntaxKind.CatchClause:
                case SyntaxKind.ParenthesizedLambdaExpression:
                case SyntaxKind.SimpleLambdaExpression:
                case SyntaxKind.AnonymousMethodExpression:
                case SyntaxKind.CatchFilterClause:
                case SyntaxKind.SwitchSection:
                case SyntaxKind.EqualsValueClause:
                case SyntaxKind.Attribute:
                case SyntaxKind.ArgumentList:
                case SyntaxKind.ArrowExpressionClause:
                case SyntaxKind.SwitchExpression:
                case SyntaxKind.SwitchExpressionArm:
                case SyntaxKind.BaseConstructorInitializer:
                case SyntaxKind.ThisConstructorInitializer:
                case SyntaxKind.ConstructorDeclaration:
                    return true;
                default:
                    return syntax is StatementSyntax || IsValidScopeDesignator(syntax as ExpressionSyntax);

            }
        }

        internal static bool IsValidScopeDesignator(this ExpressionSyntax expression)
        {
            // All these nodes are valid scope designators due to the pattern matching and out vars features.
            CSharpSyntaxNode parent = expression?.Parent;
            switch (parent?.Kind())
            {
                case SyntaxKind.SimpleLambdaExpression:
                case SyntaxKind.ParenthesizedLambdaExpression:
                    return ((LambdaExpressionSyntax)parent).Body == expression;

                case SyntaxKind.SwitchStatement:
                    return ((SwitchStatementSyntax)parent).Expression == expression;

                case SyntaxKind.ForStatement:
                    return ((ForStatementSyntax)parent).Expression == expression;

                default:
                    return false;
            }
        }

        internal static bool IsVariableDeclarationInitialization(this SyntaxNode node)
        {
            Debug.Assert(node != null);

            SyntaxNode equalsValueClause = node.Parent;

            if (!equalsValueClause.IsKind(SyntaxKind.EqualsValueClause))
            {
                return false;
            }

            SyntaxNode variableDeclarator = equalsValueClause.Parent;

            if (!variableDeclarator.IsKind(SyntaxKind.VariableDeclaration))
            {
                return false;
            }

            return variableDeclarator.Parent.IsKind(SyntaxKind.VariableDeclaration);
        }

        /// <summary>
        /// Because the instruction cannot have any values on the stack before CLR execution.
        /// Limit it to assignments and conditional expressions for now.
        /// https://github.com/dotnet/roslyn/issues/22046
        /// </summary>
        internal static bool IsLegalSpanStackAllocPosition(this SyntaxNode node)
        {
            Debug.Assert(node != null);

            if (node.Parent.IsKind(SyntaxKind.CastExpression))
            {
                node = node.Parent;
            }

            while (node.Parent.IsKind(SyntaxKind.IfExpression))
            {
                node = node.Parent;
            }

            SyntaxNode parentNode = node.Parent;

            if (parentNode is null)
            {
                return false;
            }

            switch (parentNode.Kind())
            {
                // In case of a declaration of a Span<T> variable
                case SyntaxKind.EqualsValueClause:
                    {
                        SyntaxNode variableDeclarator = parentNode.Parent;

                        return variableDeclarator.IsKind(SyntaxKind.VariableDeclaration) &&
                            variableDeclarator.Parent.IsKind(SyntaxKind.VariableDeclaration);
                    }
                // In case of reassignment to a Span<T> variable
                case SyntaxKind.SimpleAssignmentExpression:
                    {
                        return parentNode.Parent.IsKind(SyntaxKind.ExpressionStatement);
                    }
            }

            return false;
        }

        internal static CSharpSyntaxNode AnonymousFunctionBody(this SyntaxNode lambda)
        {
            switch (lambda.Kind())
            {
                case SyntaxKind.SimpleLambdaExpression:
                    return ((SimpleLambdaExpressionSyntax)lambda).Body;
                case SyntaxKind.ParenthesizedLambdaExpression:
                    return ((ParenthesizedLambdaExpressionSyntax)lambda).Body;
                case SyntaxKind.AnonymousMethodExpression:
                    return ((AnonymousMethodExpressionSyntax)lambda).Block;

                default:
                    throw ExceptionUtilities.UnexpectedValue(lambda.Kind());
            }
        }

        /// <summary>
        /// Given an initializer expression infer the name of anonymous property or tuple element.
        /// Returns default if unsuccessful
        /// </summary>
        internal static SyntaxToken ExtractAnonymousTypeMemberName(this ExpressionSyntax input)
        {
            while (true)
            {
                switch (input.Kind())
                {
                    case SyntaxKind.IdentifierName:
                        return ((IdentifierNameSyntax)input).Identifier;

                    case SyntaxKind.SimpleMemberAccessExpression:
                        input = ((MemberAccessExpressionSyntax)input).Name;
                        continue;

                    case SyntaxKind.ConditionalAccessExpression:
                        input = ((ConditionalAccessExpressionSyntax)input).WhenNotNull;
                        if (input.Kind() == SyntaxKind.MemberBindingExpression)
                        {
                            return ((MemberBindingExpressionSyntax)input).Name.Identifier;
                        }

                        continue;

                    default:
                        return default(SyntaxToken);
                }
            }
        }

        internal static RefKind GetRefKind(this TypeSyntax syntax)
        {
            var refKind = RefKind.None;
            if (syntax != null && syntax.Kind() == SyntaxKind.RefType)
            {
                var refType = (RefTypeSyntax)syntax;
                switch (refType.RefKindKeyword.Kind())
                {
                    case SyntaxKind.RefKeyword:
                        refKind = RefKind.Ref;
                        break;
                    case SyntaxKind.InKeyword:
                        refKind = RefKind.In;
                        break;
                    case SyntaxKind.OutKeyword:
                        refKind = RefKind.Out;
                        break;
                    default:
                        throw ExceptionUtilities.UnexpectedValue(refType.RefKindKeyword);
                }

                if (refType.LetKeyword.Kind() == SyntaxKind.LetKeyword)
                {
                    if (refKind == RefKind.Ref)
                    {
                        refKind = RefKind.RefReadOnly; // LetRef
                    }
                    else
                    {
                        // error
                    }
                }
            }

            return refKind;
        }

        internal static ExpressionSyntax CheckAndUnwrapRefExpression(
            this ExpressionSyntax syntax,
            DiagnosticBag diagnostics,
            out RefKind refKind)
        {
            refKind = RefKind.None;
            if (syntax?.Kind() == SyntaxKind.RefExpression)
            {
                refKind = ((RefExpressionSyntax)syntax).RefKeyword.Kind() == SyntaxKind.RefKeyword ? RefKind.Ref : RefKind.In;
                syntax = ((RefExpressionSyntax)syntax).Expression;

                syntax.CheckDeconstructionCompatibleArgument(diagnostics);
            }

            return syntax;
        }

        internal static void CheckDeconstructionCompatibleArgument(this ExpressionSyntax expression, DiagnosticBag diagnostics)
        {
            if (IsDeconstructionCompatibleArgument(expression))
            {
                diagnostics.Add(ErrorCode.ERR_VarInvocationLvalueReserved, expression.GetLocation());
            }
        }

        /// <summary>
        /// See if the expression is an invocation of a method named 'var',
        /// I.e. something like "var(x, y)" or "var(x, (y, z))" or "var(1)".
        /// We report an error when such an invocation is used in a certain syntactic contexts that
        /// will require an lvalue because we may elect to support deconstruction
        /// in the future. We need to ensure that we do not successfully interpret this as an invocation of a
        /// ref-returning method named var.
        /// </summary>
        private static bool IsDeconstructionCompatibleArgument(ExpressionSyntax expression)
        {
            if (expression.Kind() == SyntaxKind.InvocationExpression)
            {
                var invocation = (InvocationExpressionSyntax)expression;
                var invocationTarget = invocation.Expression;

                return invocationTarget.Kind() == SyntaxKind.IdentifierName &&
                    ((IdentifierNameSyntax)invocationTarget).IsNullWithNoType();
            }

            return false;
        }
    }
}
