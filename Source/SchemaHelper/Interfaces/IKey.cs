// Copyright (c) CodeSmith Tools, LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace CodeSmith.SchemaHelper {
    /// <summary>
    /// Interface that describes the contract for an Entity.
    /// </summary>
    public interface IKey {
        /// <summary>
        /// Is the key Composite.
        /// </summary>
        bool IsComposite { get; }

        /// <summary>
        /// Is this Key an Identity.
        /// </summary>
        bool IsIdentity { get; }

        /// <summary>
        /// Properties that make up the Key.
        /// </summary>
        List<IProperty> Properties { get; }

        /// <summary>
        /// Associations that make up the Key.
        /// </summary>
        List<IAssociation> Associations { get; }
    }
}
