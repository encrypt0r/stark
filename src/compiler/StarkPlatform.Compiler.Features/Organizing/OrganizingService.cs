﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using StarkPlatform.Compiler.Organizing.Organizers;
using StarkPlatform.Compiler.Shared.Extensions;

namespace StarkPlatform.Compiler.Organizing
{
    internal static partial class OrganizingService
    {
        /// <summary>
        /// Organize the whole document.
        /// 
        /// Optionally you can provide your own organizers. otherwise, default will be used.
        /// </summary>
        public static Task<Document> OrganizeAsync(Document document, IEnumerable<ISyntaxOrganizer> organizers = null, CancellationToken cancellationToken = default)
        {
            var service = document.GetLanguageService<IOrganizingService>();
            return service.OrganizeAsync(document, organizers, cancellationToken);
        }
    }
}
