﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using StarkPlatform.Compiler.Stark.Syntax;

namespace StarkPlatform.Compiler.Stark.CodeGeneration
{
    internal static class ArgumentGenerator
    {
        public static ArgumentSyntax GenerateArgument(SyntaxNode argument)
        {
            if (argument is ExpressionSyntax expression)
            {
                return SyntaxFactory.Argument(expression);
            }

            return (ArgumentSyntax)argument;
        }

        public static ArgumentListSyntax GenerateArgumentList(IList<SyntaxNode> arguments)
        {
            return SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList(arguments.Select(GenerateArgument)));
        }

        public static BracketedArgumentListSyntax GenerateBracketedArgumentList(IList<SyntaxNode> arguments)
        {
            return SyntaxFactory.BracketedArgumentList(SyntaxFactory.SeparatedList(arguments.Select(GenerateArgument)));
        }
    }
}
