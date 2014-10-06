// Copyright (c) CodeSmith Tools, LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace CodeSmith.SchemaHelper {
    /// <summary>
    /// Controls how entities are named.
    /// </summary>
    public enum EntityNaming : byte {
        /// <summary>
        /// Preserve the entity name.
        /// </summary>
        Preserve = 0,

        /// <summary>
        /// Singularize the entity name.
        /// </summary>
        Singular = 1,

        /// <summary>
        /// Pluralize the entity name.
        /// </summary>
        Plural = 2
    }
}
