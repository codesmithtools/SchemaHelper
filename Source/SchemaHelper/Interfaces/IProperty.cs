// Copyright (c) CodeSmith Tools, LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace CodeSmith.SchemaHelper {
    /// <summary>
    /// </summary>
    public interface IProperty : ICommonEntityProperty {
        #region Read-Only Properties

        /// <summary>
        /// The entity that this property is associated with.
        /// </summary>
        IEntity Entity { get; }

        /// <summary>
        /// Type of the property.
        /// </summary>
        PropertyType PropertyType { get; set; }

        /// <summary>
        /// Returns the original name of the property (E.G. The original column name).
        /// </summary>
        string KeyName { get; }

        #region Accessibility Modifiers

        /// <summary>
        /// Controls the Access Modifer the Property is defined with.
        /// </summary>
        string TypeAccess { get; }

        /// <summary>
        /// The Getter Access of the IProperty
        /// </summary>
        string GetterAccess { get; }

        /// <summary>
        /// The Setter Access of the IProperty
        /// </summary>
        string SetterAccess { get; }

        #endregion

        #region Data Type Related

        /// <summary>
        /// Returns the string for the System type of the member.
        /// </summary>
        string SystemType { get; }

        /// <summary>
        /// Returns the SystemType using the Size of the IProperty.
        /// </summary>
        string SystemTypeWithSize { get; }

        /// <summary>
        /// It returns the SystemType for Nullable types.
        /// </summary>
        string BaseSystemType { get; }

        /// <summary>
        /// The Default Value of the Property.
        /// </summary>
        string DefaultValue { get; }

        /// <summary>
        /// The size of the Property.
        /// </summary>
        int Size { get; }

        /// <summary>
        /// The Scale of the Property.
        /// </summary>
        decimal Scale { get; }

        /// <summary>
        /// The Precision of the Property.
        /// </summary>
        decimal Precision { get; }

        /// <summary>
        /// The Fixed Length of the Property.
        /// </summary>
        bool FixedLength { get; }

        /// <summary>
        /// The Unicode of the Property.
        /// </summary>
        bool Unicode { get; }

        /// <summary>
        /// Is the IProperty nullable.
        /// </summary>
        bool IsNullable { get; }

        #endregion

        #endregion

        /// <summary>
        /// Checks to see if a property is a specific property type.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        bool IsType(PropertyType type);

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
    }
}
