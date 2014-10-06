// Copyright (c) CodeSmith Tools, LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace CodeSmith.SchemaHelper {
    /// <summary>
    /// Interface that describes the contract for an Entity.
    /// </summary>
    public interface IEntity : ICommonEntityProperty {
        /// <summary>
        /// This is the name of the IEntity from the DataSource.
        /// </summary>
        string EntityKeyName { get; }

        /// <summary>
        /// The namespace that the entity belongs in.
        /// </summary>
        string Namespace { get; set; }

        /// <summary>
        /// Name of the table schema/owner.
        /// </summary>
        string SchemaName { get; }

        #region Keys

        /// <summary>
        /// Returns whether or not the IEntity has a Key.
        /// </summary>
        bool HasKey { get; }

        /// <summary>
        /// Returns the Key for the Entity.
        /// </summary>
        IKey Key { get; }

        #endregion

        #region Properties

        /// <summary>
        /// Returns a list of all the Properties of this Entity.
        /// </summary>
        List<IProperty> Properties { get; }

        /// <summary>
        /// Returns a list of Properties filtered by the PropertyType
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        List<IProperty> GetProperties(PropertyType type);

        /// <summary>
        /// Returns whether or not this IEntity has a ConcurrencyProperty.
        /// </summary>
        bool HasConcurrencyProperty { get; }

        /// <summary>
        /// Returns the ConcurrencyProperty of the Entity.
        /// </summary>
        IProperty ConcurrencyProperty { get; }

        /// <summary>
        /// Does the Entity have an Identity Property.
        /// </summary>
        bool HasIdentityProperty { get; }

        /// <summary>
        /// The Identity Property.
        /// </summary>
        IProperty IdentityProperty { get; }

        #endregion

        #region Associations

        /// <summary>
        /// Collection of Associations for this Entity.
        /// </summary>
        List<IAssociation> Associations { get; }

        /// <summary>
        /// Returns a list of Search Criteria filtered by the SearchCriteriaType
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        List<IAssociation> GetAssociations(AssociationType type);

        #endregion

        #region SearchCriteria

        /// <summary>
        /// The list of SearchCriteria for this IEntity
        /// </summary>
        List<SearchCriteria> SearchCriteria { get; }

        /// <summary>
        /// Returns a list of Search Criteria filtered by the SearchCriteriaType
        /// </summary>
        /// <param name="criteria"></param>
        /// <returns></returns>
        List<SearchCriteria> GetSearchCriteria(SearchCriteriaType criteria);

        #endregion

        #region Inheritance

        /// <summary>
        /// The Entity that the current entity inherits from.
        /// </summary>
        IEntity BaseEntity { get; }

        /// <summary>
        /// A List of all children entities that inherit from this entity.
        /// </summary>
        List<IEntity> DerivedEntities { get; }

        /// <summary>
        /// Returns true if the IEntity is marked as abstract.
        /// </summary>
        bool IsAbstract { get; }

        #endregion

        #region Accessibility Modifiers

        /// <summary>
        /// Controls the Access Modifer the class is defined with.
        /// </summary>
        string TypeAccess { get; }

        #endregion

        #region Supported Operations

        /// <summary>
        /// Can this entity be updated.
        /// </summary>
        bool CanDelete { get; }

        /// <summary>
        /// Can this entity be updated.
        /// </summary>
        bool CanInsert { get; }

        /// <summary>
        /// Can this entity be updated.
        /// </summary>
        bool CanUpdate { get; }

        #endregion

        /// <summary>
        /// Returns a generic parameter if an extended property named CS_IsGeneric exists and the value of CS_IsGeneric is true.
        /// </summary>
        string GenericProperty { get; }

        /// <summary>
        /// Set name without formatting it.
        /// </summary>
        /// <param name="name"></param>
        void SetName(string name);

        /// <summary>
        /// Get Database safe full name.
        /// </summary>
        /// <returns></returns>
        string GetSafeName();

        void ValidateAllMembers();
    }
}
