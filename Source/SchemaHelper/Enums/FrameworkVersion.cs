// Copyright (c) CodeSmith Tools, LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace CodeSmith.SchemaHelper {
    /// <summary>
    /// The .NET Framework Version.
    /// </summary>
    public enum FrameworkVersion : byte {
        /// <summary>
        /// .NET 3.5
        /// </summary>
        v35 = 0,

        /// <summary>
        /// .NET 4.0
        /// </summary>
        v40 = 1,

        /// <summary>
        /// .NET 4.5
        /// </summary>
        v45 = 2
    }
}
