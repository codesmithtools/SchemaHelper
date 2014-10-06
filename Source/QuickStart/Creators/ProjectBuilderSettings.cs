// Copyright (c) CodeSmith Tools, LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.IO;
using CodeSmith.Engine;
using CodeSmith.SchemaHelper;
using SchemaExplorer;

namespace Generator.QuickStart {
    public class ProjectBuilderSettings {
        public ProjectBuilderSettings(IDatabaseSchema database) {
            if (database == null)
                throw new ArgumentNullException("database");

            QueryPattern = QueryPatternEnum.ManagerClasses;
            EnsureMultipleResultSets = false;
            ZipFileRoot = "Common";
            SourceDatabase = database;

            Type type = database.Provider.GetType();
            DatabaseProvider = String.Format("{0}, {1}", type.FullName, type.Assembly.FullName.Split(',')[0]);

            // System.Data.OleDb,
            // System.Data.Odbc,
            // http://www.mono-project.com/Provider_Factory
            // http://msdn.microsoft.com/en-us/library/system.web.ui.webcontrols.sqldatasource.providername.aspx
            switch (DatabaseProvider) {
                case "SchemaExplorer.ADOXSchemaProvider, SchemaExplorer.ADOXSchemaProvider":
                    ConnectionStringProvider = "System.Data.OleDb";
                    break;
                case "SchemaExplorer.DB2zOSSchemaProvider, SchemaExplorer.DB2zOSSchemaProvider":
                case "SchemaExplorer.ISeriesSchemaProvider, SchemaExplorer.ISeriesSchemaProvider":
                    ConnectionStringProvider = "IBM.Data.DB2";
                    break;
                case "SchemaExplorer.MySQLSchemaProvider, SchemaExplorer.MySQLSchemaProvider":
                    ConnectionStringProvider = "MySql.Data.MySqlClient";
                    break;
                case "SchemaExplorer.OracleSchemaProvider, SchemaExplorer.OracleSchemaProvider":
                    ConnectionStringProvider = "System.Data.OracleClient";
                    break;
                case "SchemaExplorer.PostgreSQLSchemaProvider, SchemaExplorer.PostgreSQLSchemaProvider":
                    ConnectionStringProvider = "Npgsql";
                    break;
                case "SchemaExplorer.SQLAnywhereSchemaProvider, SchemaExplorer.SQLAnywhereSchemaProvider":
                    ConnectionStringProvider = "IAnywhere.Data.SQLAnywhere";
                    break;
                case "SchemaExplorer.SqlCompactSchemaProvider, SchemaExplorer.SqlCompactSchemaProvider":
                    ConnectionStringProvider = "System.Data.SqlServerCe.3.5";
                    break;
                case "SchemaExplorer.SqlCompact4SchemaProvider, SchemaExplorer.SqlCompact4SchemaProvider":
                    ConnectionStringProvider = "System.Data.SqlServerCe.4.0";
                    break;
                case "SchemaExplorer.SQLiteSchemaProvider, SchemaExplorer.SQLiteSchemaProvider":
                    ConnectionStringProvider = "System.Data.SQLite";
                    break;
                case "SchemaExplorer.VistaDBSchemaProvider, SchemaExplorer.VistaDBSchemaProvider":
                case "SchemaExplorer.VistaDB4SchemaProvider, SchemaExplorer.VistaDB4SchemaProvider":
                    ConnectionStringProvider = "System.Data.VistaDB";
                    break;
                case "SchemaExplorer.SqlSchemaProvider, SchemaExplorer.SqlSchemaProvider":
                    EnsureMultipleResultSets = true;
                    ConnectionStringProvider = "System.Data.SqlClient";
                    break;
                default:
                    Trace.WriteLine(String.Format("ConnectionStringProvider could not be resolved for provider: {0}", DatabaseProvider));
                    break;
            }
        }

        #region Database Related

        public IDatabaseSchema SourceDatabase { get; protected set; }

        public string ConnectionStringProvider { get; protected set; }

        private string _connectionStringProviderNameAttribute = String.Empty;

        public string ConnectionStringProviderNameAttribute { get { return String.IsNullOrEmpty(_connectionStringProviderNameAttribute) ? String.Empty : _connectionStringProviderNameAttribute; } set { _connectionStringProviderNameAttribute = value; } }

        private string _databaseProvider = String.Empty;

        public string DatabaseProvider { get { return String.IsNullOrEmpty(_databaseProvider) ? String.Empty : _databaseProvider; } protected set { _databaseProvider = value; } }

        public bool IncludeFunctions { get; set; }
        public bool IncludeViews { get; set; }

        public bool EnsureMultipleResultSets { get; set; }

        private string _databaseName = String.Empty;

        public string DatabaseName {
            get {
                if (String.IsNullOrEmpty(_databaseName) && SourceDatabase != null)
                    _databaseName = StringUtil.ToPascalCase(SourceDatabase.Database.Name);
                else if (String.IsNullOrEmpty(_databaseName))
                    return DataProjectName;

                return _databaseName;
            }
        }

        #endregion

        public string Location { get; set; }
        public string WorkingDirectory { get; set; }
        public string ZipFileRoot { get; set; }

        public string SolutionName { get; set; }
        public string BusinessProjectName { get; set; }
        public string DataProjectName { get; set; }
        public string InterfaceProjectName { get; set; }
        public string TestProjectName { get; set; }

        private string _entityNamespace = null;

        public string EntityNamespace { get { return String.IsNullOrEmpty(_entityNamespace) ? DataProjectName : _entityNamespace; } set { _entityNamespace = value; } }

        private string _baseNamespace = null;

        public string BaseNamespace { get { return String.IsNullOrEmpty(_baseNamespace) ? DataProjectName : _baseNamespace; } set { _baseNamespace = value; } }

        public Language Language { get; set; }
        public ProjectTypeEnum ProjectType { get; set; }

        public bool IncludeDataProject { get; set; }
        public bool IncludeDataServices { get; set; }
        public bool IncludeTestProject { get; set; }
        public bool CopyTemplatesToFolder { get; set; }

        public QueryPatternEnum QueryPattern { get; set; }
        public FrameworkVersion FrameworkVersion { get; set; }

        /// <summary>
        /// Returns the Framework Folder based off the <see cref="FrameworkVersion" />.
        public string FrameworkFolder {
            get {
                switch (FrameworkVersion) {
                    case FrameworkVersion.v35:
                        return "v3.5";
                    case FrameworkVersion.v40:
                        return "v4.0";
                    case FrameworkVersion.v45:
                        return "v4.5";
                    default:
                        throw new InvalidOperationException("Invalid FrameworkVersion.");
                }
            }
        }

        public string FrameworkString {
            get {
                switch (FrameworkVersion) {
                    case FrameworkVersion.v35:
                        return "3.5";
                    case FrameworkVersion.v40:
                        return "4.0";
                    case FrameworkVersion.v45:
                        return "4.5";
                    default:
                        throw new InvalidOperationException("Invalid FrameworkVersion.");
                }
            }
        }

        public string LanguageFolder { get { return (Language == Language.CSharp) ? "CSharp" : "VisualBasic"; } }

        public string LanguageAppendage { get { return (Language == Language.CSharp) ? "cs" : "vb"; } }

        private string _zipFileFolder;

        public string ZipFileFolder {
            get {
                if (String.IsNullOrEmpty(_zipFileFolder)) {
                    _zipFileFolder = Path.Combine(WorkingDirectory, ZipFileRoot);
                    _zipFileFolder = Path.Combine(_zipFileFolder, FrameworkFolder);
                    _zipFileFolder = Path.Combine(_zipFileFolder, LanguageFolder);
                }

                return _zipFileFolder;
            }
        }

        private string _dataContextName;

        public string DataContextName {
            get {
                if (String.IsNullOrEmpty(_dataContextName))
                    _dataContextName = DatabaseName + "DataContext";

                return _dataContextName;
            }
            set { _dataContextName = value; }
        }
    }
}
