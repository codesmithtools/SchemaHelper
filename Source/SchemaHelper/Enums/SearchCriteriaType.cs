// Copyright (c) CodeSmith Tools, LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace CodeSmith.SchemaHelper {
    /// <summary>
    /// Defines the type of Search Critiera.
    /// </summary>
    [Flags]
    public enum SearchCriteriaType : byte {
        All = 0,
        PrimaryKey = 1,
        ForeignKey = 2,
        NoForeignKeys = 4,
        Index = 8,
        Command = 16,
        View = 32
    }
}
