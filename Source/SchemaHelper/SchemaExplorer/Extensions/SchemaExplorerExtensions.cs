// Copyright (c) CodeSmith Tools, LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using SchemaExplorer;

namespace CodeSmith.SchemaHelper {
    /// <summary>
    /// Extension Methods for ITableSchema
    /// </summary>
    public static class SchemaExplorerExtensions {
        /// <summary>
        /// Checks to see if a Table is part of a ManyToMany relationship. It checks for the following:
        /// 1) Table must have Two ForeignKeys.
        /// 2) All columns must be either...
        /// a) IProperty of the Primary Key.
        /// b) IProperty of a Foreign Key.
        /// c) A DateTime stamp (CreateDate, EditDate, etc).
        /// d) Name matches Version Regex.
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        public static bool IsManyToMany(this ITableSchema table) {
            if (!Configuration.Instance.IncludeManyToManyAssociations)
                return false;

            // Bypass logic if table contains Extended Property for ManyToMany
            if (table.ExtendedPropertiesContainsKey(Configuration.Instance.ManyToManyExtendedProperty)) {
                bool manyToMany;
                if (Boolean.TryParse(table.ExtendedProperties[Configuration.Instance.ManyToManyExtendedProperty].Value.ToString(), out manyToMany)) {
                    if (!manyToMany)
                        return false;

                    // Table must have atleast two fk's...
                    if (table.ForeignKeys.Count < 2)
                        return false;

                    return true;
                }
            }

            // 1) Table must have Two ForeignKeys.
            // 2) All columns must be either...
            //    a) IProperty of the Primary Key.
            //    b) IProperty of a Foreign Key.
            //    c) A DateTime stamp (CreateDate, EditDate, etc).
            //    d) Name matches Version Regex.

            // has to be at least 2 columns
            if (table.Columns.Count < 2)
                return false;

            if (table.ForeignKeys.Count != 2)
                return false;

            foreach (IColumnSchema column in table.Columns) {
                //bool isManyToMany = (column.IsForeignKeyMember || column.IsPrimaryKeyMember || column.SystemType.Equals(typeof(DateTime)) || (Configuration.Instance.UseRowVersionRegex && Configuration.Instance.RowVersionColumnRegex.IsMatch(column.Name)));
                // Had to apply this stupid workaround because Microsoft IEntity Framework doesn't support IdentityColumns as Part of a Many-To-Many.
                bool isManyToMany = (column.IsForeignKeyMember || (column.IsColumnComputed() && !column.IsColumnIdentity()) || (Configuration.Instance.UseRowVersionRegex && Configuration.Instance.RowVersionColumnRegex.IsMatch(column.Name))) && !column.AllowDBNull;
                if (!isManyToMany)
                    return false;
            }

            return true;
        }

        #region IsEnum()

        /// <summary>
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        public static bool IsEnum(this ITableSchema table) {
            return Configuration.Instance.EnumRegexIsMatch(table.FullName, false) // 1) Matches the Enum Regex.
                   && table.HasPrimaryKey // 2) Has a Primary Key...
                   && table.PrimaryKey.MemberColumns.Count == 1 // 3) ...that is a single column...
                   && IsEnumSystemType(table.PrimaryKey.MemberColumns[0]) // 4) ...of a number type.
                   && !String.IsNullOrEmpty(GetEnumNameColumnName(table)); // 5) Contains a column for name.
        }

        private static bool IsEnumSystemType(IMemberColumnSchema column) {
            return column.NativeType.Equals("int", StringComparison.OrdinalIgnoreCase) || column.NativeType.Equals("bigint", StringComparison.OrdinalIgnoreCase) || column.NativeType.Equals("tinyint", StringComparison.OrdinalIgnoreCase) || column.NativeType.Equals("byte", StringComparison.OrdinalIgnoreCase) || column.NativeType.Equals("smallint", StringComparison.OrdinalIgnoreCase);
        }

        public static string GetEnumNameColumnName(ITableSchema table) {
            string result = String.Empty;
            foreach (ColumnSchema column in table.Columns) {
                if (Configuration.Instance.EnumRegexIsMatch(column.Name, true)) {
                    result = column.Name;
                    break;
                }
            }

            // If no Regex match found, use first column of type String.
            if (String.IsNullOrEmpty(result)) {
                foreach (ColumnSchema column in table.Columns) {
                    if (column.SystemType == typeof(string)) {
                        result = column.Name;
                        break;
                    }
                }
            }

            return result;
        }

        #endregion

        /// <summary>
        /// Is the column an Identity column.
        /// </summary>
        /// <param name="column"></param>
        /// <returns></returns>
        public static bool IsColumnIdentity(this IDataObject column) {
            return (column.ExtendedPropertiesContainsKey(Configuration.Instance.IsIdentityColumnExtendedProperty) && ((bool)column.ExtendedProperties[Configuration.Instance.IsIdentityColumnExtendedProperty].Value));
        }

        /// <summary>
        /// Is the column a computed column.
        /// </summary>
        /// <param name="column"></param>
        /// <returns></returns>
        public static bool IsColumnComputed(this IDataObject column) {
            return (column.IsColumnRowVersion() || column.IsColumnIdentity() || column.ExtendedPropertiesContainsKey(Configuration.Instance.IsComputedColumnExtendedProperty) && ((bool)column.ExtendedProperties[Configuration.Instance.IsComputedColumnExtendedProperty].Value));
        }

        /// <summary>
        /// Tries to detect the correct SystemType
        /// </summary>
        /// <param name="column"></param>
        /// <returns></returns>
        public static Type SystemType(this IDataObject column) {
            // This check determines if a type is a char or not.
            //if (column.DataType == DbType.AnsiStringFixedLength 
            //    || column.DataType == DbType.StringFixedLength 
            //    && column.Size == 1)
            //    return typeof (char);:

            return column.SystemType;
        }

        /// <summary>
        /// Is the column a RowVersion column.
        /// </summary>
        /// <param name="column"></param>
        /// <returns></returns>
        public static bool IsColumnRowVersion(this IDataObject column) {
          if (Configuration.Instance.UseRowVersionRegex)
          {
            if (Configuration.Instance.RowVersionColumnRegex.IsMatch(column.Name))
              return true;
            else
              return false;
          }

          if (String.Equals(column.NativeType, "rowversion", StringComparison.OrdinalIgnoreCase) //|| String.Equals(column.NativeType, "timestmp", StringComparison.OrdinalIgnoreCase) // IBM
                || String.Equals(column.NativeType, "timestamp", StringComparison.OrdinalIgnoreCase))
                return true;

            return false;
        }

        /// <summary>
        /// Is the column a Unicode column.
        /// </summary>
        /// <param name="column"></param>
        /// <returns></returns>
        public static bool IsUnicode(this IDataObject column) {
            if (column.NativeType.ToLower() == "nchar" || column.NativeType.ToLower() == "nvarchar" || column.NativeType.ToLower() == "ntext" || column.NativeType.ToLower() == "xml")
                return true;

            return false;
        }

        /// <summary>
        /// Is the column a FixedLength column.
        /// http://msdn.microsoft.com/en-us/library/aa258271(v=sql.80).aspx
        /// </summary>
        /// <param name="column"></param>
        /// <returns></returns>
        public static bool IsFixedLength(this IDataObject column) {
            if (column.NativeType.ToLower() == "char" || column.NativeType.ToLower() == "nchar" || column.NativeType.ToLower() == "binary" || column.NativeType.ToLower() == "timestamp")
                return true;

            return false;
        }

        /// <summary>
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public static string EntityKey(this IEntity entity) {
            return String.Format("{0}.{1}", entity.SchemaName, entity.EntityKeyName);
        }

        /// <summary>
        /// </summary>
        /// <param name="entities"></param>
        /// <returns></returns>
        public static TableSchemaCollection ToCollection(this IEnumerable<IEntity> entities) {
            if (entities == null)
                return null;

            var collection = new TableSchemaCollection();
            foreach (IEntity entity in entities) {
                var tableEntity = entity as TableEntity;
                if (tableEntity != null)
                    collection.Add((tableEntity).EntitySource as TableSchema);
            }

            return collection;
        }

        /// <summary>
        /// Adds all of the extended properties from an ISchemaObject's ExtendedProperties collection.
        /// </summary>
        /// <param name="dictionary"></param>
        /// <param name="schemaObject"></param>
        public static void AddRange(this Dictionary<string, object> dictionary, ISchemaObject schemaObject) {
            if (dictionary == null)
                return;
            try {
                foreach (IExtendedProperty extendedProperty in schemaObject.ExtendedProperties)
                    dictionary[extendedProperty.Name] = extendedProperty.Value;
            } catch (NotSupportedException) {} catch (Exception ex) {
                string message = String.Format("Unable to Add Extended Properties for '{0}'. Exception: {1}", schemaObject.Name, ex.Message);
                Trace.WriteLine(message);
                Debug.WriteLine(message);
            }

            var dataObject = schemaObject as IDataObject;
            if (dataObject != null) {
                dictionary["DataType"] = (dataObject).DataType;
                dictionary["NativeType"] = (dataObject).NativeType;
            }
        }

        private static bool ExtendedPropertiesContainsKey(this ISchemaObject schemaObject, string key) {
            try {
                return schemaObject.ExtendedProperties.Contains(key);
            } catch {}

            return false;
        }
    }
}
