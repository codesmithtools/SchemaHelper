// Copyright (c) CodeSmith Tools, LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace CodeSmith.SchemaHelper {
    /// <summary>
    /// Holds the Key members for an Entity.
    /// </summary>
    public class EntityKey : IKey {
        #region Constructor(s)

        /// <summary>
        /// </summary>
        public EntityKey() {
            Properties = new List<IProperty>();
            Associations = new List<IAssociation>();
        }

        #endregion

        #region Public Read-Only Properties

        /// <summary>
        /// Is the key Composite.
        /// </summary>
        public bool IsComposite { get { return (Properties.Count + Associations.Count > 1); } }

        /// <summary>
        /// Properties that make up the Key.
        /// </summary>
        public List<IProperty> Properties { get; set; }

        /// <summary>
        /// Properties that make up the Key.
        /// </summary>
        public List<IAssociation> Associations { get; set; }

        /// <summary>
        /// Is this Key an Identity.
        /// </summary>
        public bool IsIdentity { get { return Properties.Count(x => x.IsType(PropertyType.Identity)) > 0; } }

        #endregion
    }
}
