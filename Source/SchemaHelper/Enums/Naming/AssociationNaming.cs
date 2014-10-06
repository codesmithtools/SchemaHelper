// Copyright (c) CodeSmith Tools, LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace CodeSmith.SchemaHelper {
    /// <summary>
    /// Controls the naming for associations.
    /// </summary>
    public enum AssociationNaming : byte {
        /// <summary>
        /// Singularize the association name.
        /// </summary>
        Singular = 0,

        /// <summary>
        /// Pluralize the association name.
        /// </summary>
        Plural = 1,

        /// <summary>
        /// Append "List" to the association name.
        /// </summary>
        List = 2,

        /// <summary>
        /// Singularize the association name and append "List".
        /// </summary>
        SingularList = 4
    }
}
