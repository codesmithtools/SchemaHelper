// Copyright (c) CodeSmith Tools, LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace CodeSmith.SchemaHelper {
    /// <summary>
    /// Interface that describes the contract for an Entity.
    /// </summary>
    public interface IAssociation : ICommonEntityProperty {
        /// <summary>
        /// This is the name of the Association from the DataSource.
        /// </summary>
        string AssociationKeyName { get; }

        /// <summary>
        /// This is the unique key of the Association.
        /// </summary>
        string AssociationKey { get; }

        /// <summary>
        /// The namespace that the Association belongs in.
        /// </summary>
        string Namespace { get; }

        #region Properties

        /// <summary>
        /// Returns the association type of the first associated property.
        /// </summary>
        AssociationType AssociationType { get; }

        /// <summary>
        /// Returns the Entity.
        /// </summary>
        IEntity Entity { get; }

        /// <summary>
        /// Returns the Associated Entity.
        /// </summary>
        IEntity ForeignEntity { get; }

        /// <summary>
        /// Returns the Intermediary Entity for a Many to Many Association
        /// </summary>
        IAssociation IntermediaryAssociation { get; }

        /// <summary>
        /// Returns true if the current association was created by the parent table and not the child.
        /// </summary>
        bool IsParentEntity { get; }

        /// <summary>
        /// Returns a list of all the Properties of this Association.
        /// </summary>
        List<AssociationProperty> Properties { get; }

        /// <summary>
        /// The resolved Type Name for this Association.
        /// </summary>
        string TypeName { get; }

        /// <summary>
        /// Resolved GenericProperty for this Association.
        /// </summary>
        string GenericProperty { get; }

        /// <summary>
        /// Set name without formatting it.
        /// </summary>
        /// <param name="name"></param>
        void SetName(string name);

        /// <summary>
        /// Expands an associations name into the full name with column properties.
        /// </summary>
        void MakeUnique();

        /// <summary>
        /// The SearchCriteria for this Association.
        /// </summary>
        SearchCriteria SearchCriteria { get; set; }

        #endregion

        #region Accessibility Modifiers

        /// <summary>
        /// Controls the Access Modifier the class is defined with.
        /// </summary>
        string TypeAccess { get; }

        /// <summary>
        /// The Getter Access of the Association.
        /// </summary>
        string GetterAccess { get; }

        /// <summary>
        /// The Setter Access of the Association.
        /// </summary>
        string SetterAccess { get; }

        #endregion
    }
}
