﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using StarkPlatform.Compiler.CodeActions;
using StarkPlatform.Compiler.Host;

namespace StarkPlatform.Compiler.GenerateMember.GenerateConstructor
{
    internal interface IGenerateConstructorService : ILanguageService
    {
        Task<ImmutableArray<CodeAction>> GenerateConstructorAsync(Document document, SyntaxNode node, CancellationToken cancellationToken);
    }
}
