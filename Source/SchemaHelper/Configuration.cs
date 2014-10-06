// Copyright (c) CodeSmith Tools, LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace CodeSmith.SchemaHelper {
    /// <summary>
    /// The Main Configuration Class.
    /// </summary>
    public class Configuration {
        private string _rowVersionColumn;
        internal Regex RowVersionColumnRegex { get; private set; }

        /// <summary>
        /// </summary>
        public Configuration() {
            AliasExtendedProperty = "CS_Alias";
            ManyToManyExtendedProperty = "CS_ManyToMany";
            DescriptionExtendedProperty = "CS_Description";
            GenericPropertyExtendedProperty = "CS_IsGeneric";
            IsIdentityColumnExtendedProperty = "CS_IsIdentity";
            IsComputedColumnExtendedProperty = "CS_IsComputed";

            VisualStudioVersion = VisualStudioVersion.VS_2013;
            FrameworkVersion = FrameworkVersion.v45;

            TablePrefix = String.Empty;
            PrivateMemberPrefix = "_";
            ParameterPrefix = "@p_";
            NamingProperty = new NamingProperty {
                EntityNaming = EntityNaming.Singular,
                PropertyNaming = PropertyNaming.Normalize,
                AssociationNaming = AssociationNaming.Plural,
            };

            SearchCriteriaProperty = new SearchCriteriaProperty();

            CustomProcedureNameFormat = "_{0}_";
            UseRowVersionRegex = false;
            RowVersionColumn = "^((R|r)ow)?(V|v)ersion$";
            SingularMemberSuffix = "Member";
            IncludeManyToManyEntity = true;
            IncludeManyToManyAssociations = true;
            ExcludeNonPrimaryKeyTables = false;
            ListSuffix = "List";

            EnumExpressions = new List<Regex>();
            IncludeExpressions = new List<Regex>();
            IgnoreExpressions = new List<Regex>(); //new[] { "sysdiagrams$", "^dbo.aspnet", "^dbo.vw_aspnet" });
            CleanExpressions = new List<Regex>(); //new[] { "^(sp|tbl|udf|vw)_" });

            IncludeViews = true;
            IncludeFunctions = true;
            IncludeAssociations = true;
            IncludeFunctionExtendedProperties = false;
            MaxNumberOfKeyProperties = Int32.MaxValue;
        }

        #region Instance

        /// <summary>
        /// </summary>
        public static Configuration Instance { get { return Nested.Current; } }

        /// <summary>
        /// </summary>
        private class Nested {
            /// <summary>
            /// Current singleton instance.
            /// </summary>
            internal static readonly Configuration Current;

            /// <summary>
            /// </summary>
            static Nested() {
                Current = new Configuration();
            }
        }

        #endregion

        #region Internal Method(s)

        /// <summary>
        /// Checks to see if a string is in the EnumExpressions.
        /// </summary>
        /// <param name="name">Table FullName, Column Name, Command Name, View Name.</param>
        /// <param name="isDescription"></param>
        /// <returns></returns>
        public bool EnumRegexIsMatch(string name, bool isDescription) {
            return isDescription ? RegexIsMatch(name, EnumDescriptionExpressions) : RegexIsMatch(name, EnumExpressions);
        }

        /// <summary>
        /// Checks to see if a string is in the IncludeExpressions.
        /// </summary>
        /// <param name="name">Table FullName, Column Name, Command Name, View Name.</param>
        /// <returns></returns>
        public bool IncludeRegexIsMatch(string name) {
            return RegexIsMatch(name, IncludeExpressions.Count == 0 ? new List<Regex>() {
                new Regex(".*")
            } : IncludeExpressions);
        }

        /// <summary>
        /// Checks to see if a string is in the IgnoreExpressions.
        /// </summary>
        /// <param name="name">Table FullName, Column Name, Command Name, View Name.</param>
        /// <returns></returns>
        public bool ExcludeRegexIsMatch(string name) {
            return RegexIsMatch(name, IgnoreExpressions);
        }

        /// <summary>
        /// Checks to see if a string is in the IgnoreExpressions.
        /// </summary>
        /// <param name="name">Table FullName, Column Name, Command Name, View Name.</param>
        /// <param name="expressions">A List of Regex's to check.</param>
        /// <returns></returns>
        private static bool RegexIsMatch(string name, IEnumerable<Regex> expressions) {
            bool result = false;
            foreach (Regex regex in expressions) {
                if (!regex.IsMatch(name))
                    continue;

                result = true;
                break;
            }

            return result;
        }

        #endregion

        #region Public Properties

        public Language TargetLanguage { get; set; }

        public string AliasExtendedProperty { get; set; }

        public string DescriptionExtendedProperty { get; set; }

        public string GenericPropertyExtendedProperty { get; set; }

        public string ManyToManyExtendedProperty { get; set; }

        public string IsIdentityColumnExtendedProperty { get; set; }

        public string IsComputedColumnExtendedProperty { get; set; }

        /// <summary>
        /// Set the maximum number of view properties that can be added to the primary keys collection.
        /// Entity Framework has a limit of 15 keys that can be on an entity.
        /// </summary>
        public int MaxNumberOfKeyProperties { get; set; }

        public string TablePrefix { get; set; }

        public string PrivateMemberPrefix { get; set; }

        public string ParameterPrefix { get; set; }

        public NamingProperty NamingProperty { get; set; }

        public SearchCriteriaProperty SearchCriteriaProperty { get; set; }

        public bool UseRowVersionRegex { get; set; }

        public string RowVersionColumn {
            get { return _rowVersionColumn; }
            set {
                _rowVersionColumn = value;
                RowVersionColumnRegex = new Regex(value, RegexOptions.Compiled);
            }
        }

        public VisualStudioVersion VisualStudioVersion { get; set; }

        public string SingularMemberSuffix { get; set; }

        public string ListSuffix { get; set; }

        public List<Regex> CleanExpressions { get; set; }

        public List<Regex> EnumExpressions { get; set; }
        public List<Regex> EnumDescriptionExpressions { get; set; }

        public List<Regex> IgnoreExpressions { get; set; }

        public List<Regex> IncludeExpressions { get; set; }

        /// <summary>
        /// When true, the intermediary table will added to the EntityManager's Entities List.
        /// </summary>
        public bool IncludeManyToManyEntity { get; set; }

        /// <summary>
        /// Controls whether Many to Many associations will be created.
        /// When set to false, ManyToMany associations will be treated as 1-0/* and */1-0.
        /// </summary>
        public bool IncludeManyToManyAssociations { get; set; }

        public bool IncludeEnumEntity { get; set; }

        public bool ExcludeNonPrimaryKeyTables { get; set; }

        public bool ExcludeForiegnKeyIdProperties { get; set; }

        /// <summary>
        /// The format needed for the custom procedures.
        /// Sample: usp_cust_{0}_
        /// </summary>
        public string CustomProcedureNameFormat { get; set; }

        public bool IncludeSilverlightSupport { get; set; }

        public bool IncludeWinRTSupport { get; set; }

        public bool IncludeViews { get; set; }

        public bool IncludeFunctions { get; set; }

        /// <summary>
        /// If set to true, Expensive operations will be executed. This includes getting ExtendedProperty information for Commands
        /// and other expensive operations.
        /// </summary>
        public bool IncludeFunctionExtendedProperties { get; set; }

        public bool IncludeAssociations { get; set; }

        public FrameworkVersion FrameworkVersion { get; set; }

        public string SafeNamePrefix { get; set; }

        public string SafeNameSuffix { get; set; }

        private string _schemaProviderName;

        // TODO: Remove this.
        public string SchemaProviderName {
            get { return _schemaProviderName; }
            set {
                _schemaProviderName = value;
                switch (value) {
                    case "SqlSchemaProvider":
                        SafeNamePrefix = "[";
                        SafeNameSuffix = "]";
                        break;

                        // This has a negitive side effect on nHibernate support
                        //case "OracleSchemaProvider":
                        //    SafeNamePrefix = "\"";
                        //    SafeNameSuffix = "\"";
                        //    break;

                        //case "MySQLSchemaProvider":
                        //    SafeNamePrefix = String.Empty;
                        //    SafeNameSuffix = String.Empty;
                        //    break;

                    default:
                        SafeNamePrefix = String.Empty;
                        SafeNameSuffix = String.Empty;
                        break;
                }
            }
        }

        /// <summary>
        /// Views do not define any keys, but in cases where frameworks need to define a key like entity framework a key is defined
        /// as any non-nullable column.
        /// </summary>
        public bool GenerateViewKeys { get; set; }

        #endregion
    }
}
