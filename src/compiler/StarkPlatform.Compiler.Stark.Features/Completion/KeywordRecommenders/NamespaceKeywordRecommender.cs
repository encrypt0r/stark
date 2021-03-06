﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Threading;
using StarkPlatform.Compiler.Stark.Extensions;
using StarkPlatform.Compiler.Stark.Extensions.ContextQuery;
using StarkPlatform.Compiler.Stark.Syntax;
using StarkPlatform.Compiler.Shared.Extensions;

namespace StarkPlatform.Compiler.Stark.Completion.KeywordRecommenders
{
    internal class NamespaceKeywordRecommender : AbstractSyntacticSingleKeywordRecommender
    {
        public NamespaceKeywordRecommender()
            : base(SyntaxKind.NamespaceKeyword)
        {
        }

        protected override bool IsValidContext(int position, CSharpSyntaxContext context, CancellationToken cancellationToken)
        {
            var syntaxTree = context.SyntaxTree;

            // namespaces are illegal in interactive code:
            if (syntaxTree.IsScript())
            {
                return false;
            }

            // cases:
            // root: |

            // root: n|

            // extern alias a;
            // |

            // extern alias a;
            // n|

            // using Goo;
            // |

            // using Goo;
            // n|

            // using Goo = Bar;
            // |

            // using Goo = Bar;
            // n|

            // namespace N {}
            // |

            // namespace N {}
            // n|

            // class C {}
            // |

            // class C {}
            // n|

            var leftToken = context.LeftToken;
            var token = context.TargetToken;

            // root: n|

            // ns Goo { n|

            // extern alias a;
            // n|

            // using Goo;
            // n|

            // using Goo = Bar;
            // n|

            // a namespace can't come before usings/externs
            // a child namespace can't come before usings/externs
            if (leftToken.GetNextToken(includeSkipped: true).IsUsingOrExternKeyword())
            {
                return false;
            }

            // root: |
            if (token.Kind() == SyntaxKind.None)
            {
                // root namespace
                var root = syntaxTree.GetRoot(cancellationToken) as CompilationUnitSyntax;
                if (root.Externs.Count > 0 ||
                    root.Usings.Count > 0)
                {
                    return false;
                }

                return true;
            }

            if (token.Kind() == SyntaxKind.OpenBraceToken &&
                token.Parent.IsKind(SyntaxKind.NamespaceDeclaration))
            {
                return true;
            }

            // extern alias a;
            // |

            // using Goo;
            // |
            if (token.Kind() == SyntaxKind.SemicolonToken)
            {
                if (token.Parent.IsKind(SyntaxKind.ExternAliasDirective, SyntaxKind.ImportDirective))
                {
                    return true;
                }
            }

            // class C {}
            // |
            if (token.Kind() == SyntaxKind.CloseBraceToken)
            {
                if (token.Parent is TypeDeclarationSyntax &&
                    !(token.Parent.GetParent() is TypeDeclarationSyntax))
                {
                    return true;
                }
                else if (token.Parent.IsKind(SyntaxKind.NamespaceDeclaration))
                {
                    return true;
                }
            }

            // delegate void D();
            // |

            if (token.Kind() == SyntaxKind.SemicolonToken)
            {
                if (token.Parent.IsKind(SyntaxKind.DelegateDeclaration) &&
                    !(token.Parent.GetParent() is TypeDeclarationSyntax))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
