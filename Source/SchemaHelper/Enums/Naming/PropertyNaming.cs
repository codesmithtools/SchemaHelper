// Copyright (c) CodeSmith Tools, LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace CodeSmith.SchemaHelper {
    /// <summary>
    /// Controls the naming of properties.
    /// </summary>
    public enum PropertyNaming : byte {
        /// <summary>
        /// Preserve the property name.
        /// </summary>
        Preserve = 0,

        /// <summary>
        /// Normalizes the property name.
        /// </summary>
        Normalize = 1,

        /// <summary>
        /// Normalizes the property name and removes the entity name prefix from the property name if present.
        /// </summary>
        NormalizeRemovePrefix = 2
    }
}
