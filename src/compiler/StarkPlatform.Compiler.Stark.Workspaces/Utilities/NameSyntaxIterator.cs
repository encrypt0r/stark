﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using StarkPlatform.Compiler.Stark;
using StarkPlatform.Compiler.Stark.Symbols;
using StarkPlatform.Compiler.Stark.Syntax;
using StarkPlatform.Compiler.Text;

namespace StarkPlatform.Compiler.Stark.Utilities
{
    internal class NameSyntaxIterator : IEnumerable<NameSyntax>
    {
        private readonly NameSyntax _name;

        public NameSyntaxIterator(NameSyntax name)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            _name = name;
        }

        public IEnumerator<NameSyntax> GetEnumerator()
        {
            var nodes = new LinkedList<NameSyntax>();

            var currentNode = _name;
            while (true)
            {
                if (currentNode.Kind() == SyntaxKind.QualifiedName)
                {
                    var qualifiedName = currentNode as QualifiedNameSyntax;
                    nodes.AddFirst(qualifiedName.Right);
                    currentNode = qualifiedName.Left;
                }
                else
                {
                    nodes.AddFirst(currentNode);
                    break;
                }
            }

            return nodes.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
