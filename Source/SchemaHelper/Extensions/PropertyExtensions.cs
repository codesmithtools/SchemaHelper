// Copyright (c) CodeSmith Tools, LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using CodeSmith.Core.Extensions;

namespace CodeSmith.SchemaHelper {
    /// <summary>
    /// Extension methods for IProperty.
    /// </summary>
    public static class PropertyExtensions {
        /// <summary>
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        public static string FriendlyName(this IProperty property) {
            const string key = "CS_FriendlyName";

            string name = null;
            if (property.ExtendedProperties.ContainsKey(key))
                name = property.ExtendedProperties[key].ToString().Trim();

            return !String.IsNullOrEmpty(name) ? name : property.Name.ToSpacedWords();
        }
    }
}
