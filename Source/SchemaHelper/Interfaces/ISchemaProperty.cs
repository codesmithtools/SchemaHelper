// Copyright (c) CodeSmith Tools, LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data;

namespace CodeSmith.SchemaHelper {
    public interface ISchemaProperty : IProperty {
        #region Data Type Specific

        /// <summary>
        /// Returns the string representing the Database type of the member.
        /// </summary>
        DbType DataType { get; }

        /// <summary>
        /// Returns the string representing the actual data type of the member in the Source Database.
        /// </summary>
        string NativeType { get; }

        #endregion

        /// <summary>
        /// Is this IProperty a art of a Key
        /// </summary>
        bool IsPrimaryKey { get; }

        /// <summary>
        /// Is this IProperty part of a Foreign Key.
        /// </summary>
        bool IsForeignKey { get; }

        /// <summary>
        /// Is this IProperty Unique for the Entity.
        /// </summary>
        bool IsUnique { get; }

        /// <summary>
        /// Is this IProperty an Identity column.
        /// </summary>
        bool IsIdentity { get; }

        /// <summary>
        /// Is this IProperty computed.
        /// </summary>
        bool IsComputed { get; }

        /// <summary>
        /// Is this IProperty a RowVersion column for the Entity.
        /// </summary>
        bool IsRowVersion { get; }

        /// <summary>
        /// Is this IProperty ReadOnly.
        /// Computed by looking at IsIdentity, IsRowVersion, and IsComputed.
        /// </summary>
        bool IsReadOnly { get; }
    }
}
