// Copyright (c) CodeSmith Tools, LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data;
using System.Diagnostics;
using CodeSmith.SchemaHelper.Util;
using SchemaExplorer;

namespace CodeSmith.SchemaHelper {
    /// <summary>
    /// ViewMember represents a column from a Table
    /// </summary>
    [DebuggerDisplay("ViewProperty = {Name}, Type = {SystemType}, Key = {KeyName}, Entity = {Entity.Name}")]
    public class ViewProperty : PropertyBase<IViewColumnSchema>, ISchemaProperty {
        #region Constructor(s)

        /// <summary>
        /// Creates a ViewProperty from a ViewColumnSchema.
        /// </summary>
        /// <param name="column"></param>
        /// <param name="entity"></param>
        public ViewProperty(IViewColumnSchema column, IEntity entity) : base(column, entity) {}

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

        #endregion

        #region Method Overrides

        /// <summary>
        /// Loads the Property Settings.
        /// This method is called from the base classes constructor.
        /// </summary>
        public override void Initialize() {
            #region Base Properties

            KeyName = PropertySource.Name;
            //Description = PropertySource.Description;

            #region Data Type Related

            SystemType = TypeHelper.ResolveSystemType(PropertySource.SystemType(), PropertySource.AllowDBNull, true);
            //DefaultValue = PropertySource.DefaultValue;
            Size = PropertySource.Size;
            Scale = PropertySource.Scale;
            Precision = PropertySource.Precision;
            //FixedLength =
            //Unicode =

            IsNullable = PropertySource.AllowDBNull;

            #endregion

            #endregion

            #region Column Specific Properties

            DataType = PropertySource.DataType;
            NativeType = PropertySource.NativeType;

            //IsUnique = PropertySource.IsUnique;
            //IsForeignKey = PropertySource.IsForeignKeyMember;
            IsRowVersion = PropertySource.IsColumnRowVersion();
            IsIdentity = PropertySource.IsColumnIdentity();
            IsComputed = PropertySource.IsColumnComputed();
            Unicode = PropertySource.IsUnicode();
            FixedLength = PropertySource.IsFixedLength();

            //  Views do not define any keys, but in cases where frameworks need to define a key like entity framework a key is defined as any non-nullable column.
            if (Configuration.Instance.GenerateViewKeys)
                IsPrimaryKey = !IsNullable && !FixedLength;

            PropertyType = ResolvePropertyType();

            #endregion
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

            return type ?? PropertyType.Normal;
        }

        protected override void LoadExtendedProperties() {
            ExtendedProperties.AddRange(PropertySource);
        }

        #endregion
    }
}
