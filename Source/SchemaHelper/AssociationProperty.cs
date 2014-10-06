// Copyright (c) CodeSmith Tools, LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace CodeSmith.SchemaHelper {
    /// <summary>
    /// The Association property.
    /// </summary>
    public class AssociationProperty {
        #region Constructor(s)

        /// <summary>
        /// Creates an AssociationProperty from the supplied data.
        /// </summary>
        /// <param name="association"></param>
        /// <param name="property"></param>
        /// <param name="foreignProperty"></param>
        public AssociationProperty(IAssociation association, IProperty property, IProperty foreignProperty) {
            Property = property;
            ForeignProperty = foreignProperty;
            Cascade = (association.AssociationType == AssociationType.OneToMany && property != null && !property.IsNullable);
            Association = association;
        }

        #endregion

        #region Public Read-Only Methods

        /// <summary>
        /// The parent association for the property.
        /// </summary>
        public IAssociation Association { get; private set; }

        /// <summary>
        /// Local property to the entity.
        /// </summary>
        public IProperty Property { get; private set; }

        /// <summary>
        /// Foreign property to the entity.
        /// </summary>
        public IProperty ForeignProperty { get; private set; }

        /// <summary>
        /// I think this is used for Cascading of deletes??
        /// </summary>
        public bool Cascade { get; private set; }

        #endregion
    }
}
