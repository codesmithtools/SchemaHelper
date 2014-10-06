// Copyright (c) CodeSmith Tools, LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace CodeSmith.SchemaHelper {
    [Flags]
    public enum AssociationType : byte {
        /// <summary>
        /// Returns a list of all Associated Many to One keys.
        /// </summary>
        ManyToOne = 0,

        /// <summary>
        /// Returns a list of all One to Many keys.
        /// </summary>
        OneToMany = 1,

        /// <summary>
        /// Returns a list of all One to Many keys.
        /// </summary>
        ZeroOrOneToMany = 2,

        /// <summary>
        /// Returns a list of all Many to Many keys
        /// </summary>
        ManyToMany = 4,

        /// <summary>
        /// Returns a list of all One to Zero or one keys.
        /// </summary>
        ManyToZeroOrOne = 8,

        /// <summary>
        /// Returns a list of all One to Zero or one keys.
        /// </summary>
        OneToZeroOrOne = 16,

        /// <summary>
        /// Returns a list of all One to Zero or one keys.
        /// </summary>
        OneToOne = 32
    }
}
