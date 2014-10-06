// Copyright (c) CodeSmith Tools, LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data;
using System.Diagnostics;
using CodeSmith.SchemaHelper.Util;
using SchemaExplorer;

namespace CodeSmith.SchemaHelper {
    /// <summary>
    /// TableMember represents a column from a Table
    /// </summary>
    [DebuggerDisplay("TableProperty = {Name}, Type = {SystemType}, Key = {KeyName}, Entity = {Entity.Name}")]
    public sealed class TableProperty : PropertyBase<IColumnSchema>, ISchemaProperty {
        #region Constuctor(s)

        /// <summary>
        /// Creates a IColumnSchema from a TableSchema.
        /// </summary>
        /// <param name="column"></param>
        /// <param name="entity"></param>
        public TableProperty(IColumnSchema column, IEntity entity) : base(column, entity) {}

        #endregion

        #region Column Specific Properties

        /// <summary>
        /// Returns the string representing the Database type of the property.
        /// </summary>
        public DbType DataType { get; internal set; }

        /// <summary>
        /// Returns the string representing the actual data type of the property in the Source Database.
        /// </summary>
        public string NativeType { get; internal set; }

        /// <summary>
        /// Is this IProperty a art of a Key
        /// </summary>
        public bool IsPrimaryKey { get; internal set; }

        /// <summary>
        /// Is this IProperty part of a Foreign Key.
        /// </summary>
        public bool IsForeignKey { get; internal set; }

        /// <summary>
        /// Is this IProperty Unique for the Entity.
        /// </summary>
        public bool IsUnique { get; internal set; }

        /// <summary>
        /// Is this IProperty an Identity column.
        /// </summary>
        public bool IsIdentity { get; internal set; }

        /// <summary>
        /// Is this IProperty computed.
        /// </summary>
        public bool IsComputed { get; internal set; }

        /// <summary>
        /// Is this IProperty a RowVersion column for the Entity.
        /// </summary>
        public bool IsRowVersion { get; internal set; }

        /// <summary>
        /// Is this IProperty ReadOnly.  Computed by looking at IsIdentity, IsRowVersion, and IsComputed.
        /// </summary>
        public bool IsReadOnly { get { return (IsIdentity || IsRowVersion || IsComputed); } }

        #endregion

        #region Method Overrides

        /// <summary>
        /// Loads the Property Settings.
        /// This method is called from the base classes constructor.
        /// </summary>
        public override void Initialize() {
            #region Base Properties

            KeyName = PropertySource.Name;
            Description = PropertySource.Description;

            #region Data Type Related

            SystemType = TypeHelper.ResolveSystemType(PropertySource.SystemType(), PropertySource.AllowDBNull, true);

            Size = PropertySource.Size;
            Scale = PropertySource.Scale;
            Precision = PropertySource.Precision;
            FixedLength = PropertySource.IsFixedLength();
            Unicode = PropertySource.IsUnicode();
            IsNullable = PropertySource.AllowDBNull;

            #endregion

            #endregion

            #region Column Specific Properties

            DataType = PropertySource.DataType;
            NativeType = PropertySource.NativeType;

            IsUnique = PropertySource.IsUnique;
            IsPrimaryKey = PropertySource.IsPrimaryKeyMember;
            IsForeignKey = PropertySource.IsForeignKeyMember;
            IsRowVersion = PropertySource.IsColumnRowVersion();
            IsIdentity = PropertySource.IsColumnIdentity();
            IsComputed = PropertySource.IsColumnComputed();

            PropertyType = ResolvePropertyType();

            #endregion
        }

        protected override string LoadDefaultValue() {
            object value;
            if (!ExtendedProperties.TryGetValue("CS_Default", out value) || String.IsNullOrEmpty(value.ToString()))
                return null;

            string defaultValue = value.ToString();
            if (String.Equals(BaseSystemType, "System.Boolean", StringComparison.OrdinalIgnoreCase))
                defaultValue = defaultValue.Contains("0") || defaultValue.ToLowerInvariant().Contains("false") ? Boolean.FalseString : Boolean.TrueString;
            else if (String.Equals(BaseSystemType, "System.Single", StringComparison.OrdinalIgnoreCase) || String.Equals(BaseSystemType, "System.Int16", StringComparison.OrdinalIgnoreCase) || String.Equals(BaseSystemType, "System.Int32", StringComparison.OrdinalIgnoreCase) || String.Equals(BaseSystemType, "System.Int64", StringComparison.OrdinalIgnoreCase) || String.Equals(BaseSystemType, "System.Byte", StringComparison.OrdinalIgnoreCase) || String.Equals(BaseSystemType, "System.Decimal", StringComparison.OrdinalIgnoreCase) || String.Equals(BaseSystemType, "System.Double", StringComparison.OrdinalIgnoreCase))
                defaultValue = defaultValue.Replace("(", "").Replace(")", "");
            else if (defaultValue.ToLowerInvariant().Contains("null")) {
                defaultValue = null;
                //} else if (defaultValue.ToLowerInvariant().Contains("autoincrement")) { // Required for ef, need to look into special case it in the ef templates.
                //    defaultValue = null;
            } else if (defaultValue.ToLowerInvariant().Contains("newid"))
                defaultValue = "newid()";
            else if (defaultValue.ToLowerInvariant().Contains("getdate"))
                defaultValue = "getdate()";
            else if (!String.Equals(BaseSystemType, "System.String", StringComparison.OrdinalIgnoreCase) && defaultValue.StartsWith("\"") && defaultValue.EndsWith("\"") && defaultValue.Length > 2)
                defaultValue = defaultValue.Substring(1, defaultValue.Length - 2);

            return defaultValue;
        }

        protected override void LoadExtendedProperties() {
            ExtendedProperties.AddRange(PropertySource);
        }

        private PropertyType ResolvePropertyType() {
            PropertyType? type = null;

            if (IsPrimaryKey)
                type = PropertyType.Key;

            if (IsForeignKey) {
                if (!type.HasValue)
                    type = PropertyType.Foreign;
                else
                    type |= PropertyType.Foreign;
            }
            if (IsIdentity) {
                if (!type.HasValue)
                    type = PropertyType.Identity;
                else
                    type |= PropertyType.Identity;
            }
            if (IsRowVersion) {
                if (!type.HasValue)
                    type = PropertyType.Concurrency;
                else
                    type |= PropertyType.Concurrency;
            }
            if (IsComputed) {
                if (!type.HasValue)
                    type = PropertyType.Computed;
                else
                    type |= PropertyType.Computed;
            }
            if (IsUnique) {
                if (!type.HasValue)
                    type = PropertyType.Index;
                else
                    type |= PropertyType.Index;
            }

            return type ?? PropertyType.Normal;
        }

        #endregion
    }
}
