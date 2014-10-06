// Copyright (c) CodeSmith Tools, LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data;
using System.Diagnostics;
using CodeSmith.SchemaHelper.Util;
using SchemaExplorer;

namespace CodeSmith.SchemaHelper {
    /// <summary>
    /// This represents a Parameter to a command object from the Database.  Using a property to inherit the naming
    /// Functionality from the Schema Helper.
    /// </summary>
    [DebuggerDisplay("CommandParameter = {Name}, Type = {SystemType}, Key = {KeyName}, Entity = {Entity.Name}")]
    public class CommandParameter : PropertyBase<IParameterSchema>, ISchemaProperty {
        #region Constructor(s)

        /// <summary>
        /// Creates a CommandParameter from a ParameterSchema.
        /// </summary>
        /// <param name="parameter"></param>
        /// <param name="entity"></param>
        public CommandParameter(IParameterSchema parameter, IEntity entity) : base(parameter, entity) {}

        #endregion

        #region Public Read-Only Properties

        /// <summary>
        /// The direction of the Parameter of the command
        /// </summary>
        public ParameterDirection ParameterDirection { get; private set; }

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

        #endregion

        #region Method Overrides

        /// <summary>
        /// Loads the Property Settings.
        /// This method is called from the base classes constructor.
        /// </summary>
        public override void Initialize() {
            #region Base Properties

            KeyName = PropertySource.Name.Replace("@", String.Empty);
            // NOTE: This can take forever in some cases on long stored procedures.
            //Description = PropertySource.Description;

            #region Data Type Related

            SystemType = TypeHelper.ResolveSystemType(PropertySource.SystemType(), PropertySource.AllowDBNull, true);
            //DefaultValue = PropertySource.DefaultValue;
            Size = PropertySource.Size;
            Scale = PropertySource.Scale;
            Precision = PropertySource.Precision;

            IsNullable = PropertySource.AllowDBNull;

            #endregion

            #endregion

            #region Column Specific Properties

            DataType = PropertySource.DataType;
            NativeType = PropertySource.NativeType;

            //IsUnique = PropertySource.IsUnique;
            //IsPrimaryKey = PropertySource.IsPrimaryKeyMember;
            //IsForeignKey = PropertySource.IsForeignKeyMember;
            IsRowVersion = PropertySource.IsColumnRowVersion();
            //IsIdentity = PropertySource.IsColumnIdentity(); // NOTE: This is not needed for a command paramater...
            IsComputed = IsRowVersion; // PropertySource.IsColumnComputed();
            Unicode = PropertySource.IsUnicode();
            FixedLength = PropertySource.IsFixedLength();

            #endregion

            ParameterDirection = PropertySource.Direction;

            PropertyType = ResolvePropertyType();
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
            if (Configuration.Instance.IncludeFunctionExtendedProperties)
                ExtendedProperties.AddRange(PropertySource);
        }

        #endregion
    }
}
