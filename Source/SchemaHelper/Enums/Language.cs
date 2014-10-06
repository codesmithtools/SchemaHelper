// Copyright (c) CodeSmith Tools, LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel;

namespace CodeSmith.SchemaHelper {
    public enum Language : byte {
        /// <summary>
        /// CSharp
        /// </summary>
        [Description("C#")]
        CSharp = 0,

        /// <summary>
        /// Visual Basic
        /// </summary>
        [Description("Visual Basic")]
        VB = 1
    }
}
