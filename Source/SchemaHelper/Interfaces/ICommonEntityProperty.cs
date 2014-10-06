// Copyright (c) CodeSmith Tools, LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace CodeSmith.SchemaHelper {
    public interface ICommonEntityProperty {
        #region Properties

        /// <summary>
        /// The resolved Name for this Entity.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// The Private IProperty variable name.
        /// </summary>
        string PrivateMemberVariableName { get; }

        /// <summary>
        /// Variable name used within scope of a Method or Method parameter.
        /// </summary>
        string VariableName { get; }

        #region Description

        /// <summary>
        /// Returns True of the Description has a Value
        /// </summary>
        bool HasDescription { get; }

        /// <summary>
        /// The Description of the IProperty
        /// </summary>
        string Description { get; }

        #endregion

        /// <summary>
        /// Collection of Extended properties.
        /// </summary>
        Dictionary<string, object> ExtendedProperties { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Loads the Property Settings.
        /// This method is called from the base classes constructor.
        /// </summary>
        void Initialize();

        /// <summary>
        /// Appends a suffix to the name.
        /// </summary>
        /// <param name="suffix">The suffix.</param>
        void AppendNameSuffix(int suffix);

        /// <summary>
        /// Returns the Name of the member
        /// </summary>
        /// <returns></returns>
        string ToString();

        #endregion
    }
}
