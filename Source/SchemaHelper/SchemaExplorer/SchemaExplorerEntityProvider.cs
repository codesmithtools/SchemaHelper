// Copyright (c) CodeSmith Tools, LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SchemaExplorer;

namespace CodeSmith.SchemaHelper {
    /// <summary>
    /// </summary>
    public class SchemaExplorerEntityProvider : IEntityProvider {
        protected DatabaseSchema _database;
        protected TableSchemaCollection _tables;
        protected ViewSchemaCollection _views;
        protected CommandSchemaCollection _commands;

        #region Constructors

        /// <summary>
        /// </summary>
        /// <param name="database"></param>
        public SchemaExplorerEntityProvider(DatabaseSchema database) {
            _database = database;

            if (_database != null) {
                if (!_database.DeepLoad) {
                    _database.DeepLoad = true;
                    _database.Refresh();
                }

                _tables = _database.Tables;
                _views = _database.Views;
                _commands = _database.Commands;
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="tables"></param>
        /// <param name="views"></param>
        /// <param name="commands"></param>
        public SchemaExplorerEntityProvider(TableSchemaCollection tables, ViewSchemaCollection views, CommandSchemaCollection commands) {
            _tables = tables;
            _views = views;
            _commands = commands;

            if (_tables != null && _tables.Count > 0)
                _database = _tables[0].Database;
            else if (_views != null && _views.Count > 0)
                _database = _views[0].Database;
            else if (_commands != null && _commands.Count > 0)
                _database = _commands[0].Database;
        }

        #endregion

        #region Provider Methods

        /// <summary>
        /// </summary>
        /// <returns></returns>
        public bool Validate() {
            if (_database == null && _tables == null && _views == null && _commands == null)
                return false;

            return true;
        }

        /// <summary>
        /// </summary>
        public void Load() {
            if (!Validate())
                return;

            Initialize(_tables, _views, _commands);
        }

        /// <summary>
        /// </summary>
        public void Save() {}

        #endregion

        #region Helpers

        /// <summary>
        /// Load all the Entities into the EntityStore.
        /// </summary>
        protected virtual void Initialize(TableSchemaCollection tables, ViewSchemaCollection views, CommandSchemaCollection commands) {
            LoadTables(tables);

            LoadViews(views);

            LoadCommands(commands);

            foreach (IEntity entity in EntityStore.Instance.EntityCollection.Values) {
                List<IEntity> entities = EntityStore.Instance.EntityCollection.Values.Where(e => e.Name == entity.Name).ToList();
                if (entities.Count > 1) {
                    for (int index = 1; index < entities.Count(); index++)
                        entities[index].AppendNameSuffix(index);
                }
            }

            // For all entities, Initialize them.
            foreach (IEntity entity in EntityStore.Instance.EntityCollection.Values)
                entity.Initialize();

            foreach (IEntity entity in EntityStore.Instance.EntityCollection.Values)
                entity.ValidateAllMembers();
        }

        /// <summary>
        /// Load the Tables.
        /// </summary>
        /// <param name="tables"></param>
        protected virtual void LoadTables(TableSchemaCollection tables) {
            if (tables != null && tables.Count > 0 && tables[0].Database != null) {
                // First loop through the tables and populate the EntityCollection with the Entity / Columns.
                foreach (TableSchema table in tables) {
                    if (table == null)
                        continue;

                    if (!Configuration.Instance.IncludeRegexIsMatch(table.FullName) || Configuration.Instance.ExcludeRegexIsMatch(table.FullName) || (Configuration.Instance.ExcludeNonPrimaryKeyTables && !table.HasPrimaryKey)) {
                        Trace.WriteLine(String.Format("Skipping table: '{0}', the table was excluded or no Primary Key was found!", table.FullName));
                        Debug.WriteLine(String.Format("Skipping table: '{0}', the table was excluded or no Primary Key was found!", table.FullName));

                        EntityStore.Instance.ExcludedEntityCollection.Add(table.FullName, null);
                        continue;
                    }

                    if (!Configuration.Instance.IncludeManyToManyEntity && table.IsManyToMany()) {
                        Trace.WriteLine(String.Format("Skipping ManyToMany table: '{0}', ManyToMany tables are set to be excluded.", table.FullName));
                        Debug.WriteLine(String.Format("Skipping ManyToMany table: '{0}', ManyToMany tables are set to be excluded.", table.FullName));
                        EntityStore.Instance.ExcludedEntityCollection.Add(table.FullName, new TableEntity(table));
                    } else if (Configuration.Instance.IncludeEnumEntity && table.IsEnum()) {
                        Trace.WriteLine(String.Format("Enum table: '{0}' added to the Entities Collection", table.FullName));
                        Debug.WriteLine(String.Format("Enum table: '{0}' added to the Entities Collection", table.FullName));
                        EntityStore.Instance.EntityCollection.Add(table.FullName, new TableEnumEntity(table));
                    } else
                        EntityStore.Instance.EntityCollection.Add(table.FullName, new TableEntity(table));
                }

                if (!Configuration.Instance.IncludeAssociations)
                    return;

                foreach (var entity in EntityStore.Instance.EntityCollection.Where(e => e.Value is TableEntity && !(e.Value is TableEnumEntity)).ToList()) {
                    ITableSchema table = ((TableEntity)entity.Value).EntitySource;
                    foreach (TableKeySchema tks in table.ForeignKeys) {
                        if (EntityStore.Instance.GetEntity(tks.PrimaryKeyTable.FullName) != null || !Configuration.Instance.IncludeRegexIsMatch(tks.PrimaryKeyTable.FullName) || Configuration.Instance.ExcludeRegexIsMatch(tks.PrimaryKeyTable.FullName) || (Configuration.Instance.ExcludeNonPrimaryKeyTables && !tks.PrimaryKeyTable.HasPrimaryKey))
                            continue;

                        EntityStore.Instance.EntityCollection.Add(tks.PrimaryKeyTable.FullName, new TableEntity(tks.PrimaryKeyTable));
                    }

                    foreach (TableKeySchema tks in table.PrimaryKeys) {
                        if (EntityStore.Instance.GetEntity(tks.ForeignKeyTable.FullName) != null || !Configuration.Instance.IncludeRegexIsMatch(tks.ForeignKeyTable.FullName) || Configuration.Instance.ExcludeRegexIsMatch(tks.ForeignKeyTable.FullName) || (Configuration.Instance.ExcludeNonPrimaryKeyTables && !tks.ForeignKeyTable.HasPrimaryKey))
                            continue;

                        if (!Configuration.Instance.IncludeManyToManyEntity && tks.ForeignKeyTable.IsManyToMany() && EntityStore.Instance.GetExcludedEntity(tks.ForeignKeyTable.FullName) != null) {
                            EntityStore.Instance.ExcludedEntityCollection.Add(tks.ForeignKeyTable.FullName, new TableEntity(tks.ForeignKeyTable));
                            continue;
                        }

                        EntityStore.Instance.EntityCollection.Add(tks.ForeignKeyTable.FullName, new TableEntity(tks.ForeignKeyTable));
                    }
                }
            }
        }

        protected virtual void LoadViews(ViewSchemaCollection views) {
            // Load the Views.
            if (Configuration.Instance.IncludeViews && views != null && views.Count > 0 && views[0].Database != null) {
                foreach (ViewSchema view in views) {
                    if (!Configuration.Instance.IncludeRegexIsMatch(view.FullName) || Configuration.Instance.ExcludeRegexIsMatch(view.FullName) || EntityStore.Instance.GetEntity(view.FullName) != null) {
                        Trace.WriteLine(String.Format("Skipping view: '{0}'", view.FullName));
                        Debug.WriteLine(String.Format("Skipping view: '{0}'", view.FullName));

                        EntityStore.Instance.ExcludedEntityCollection.Add(view.FullName, null);
                        continue;
                    }
                    EntityStore.Instance.EntityCollection.Add(view.FullName, new ViewEntity(view));
                }
            }
        }

        /// <summary>
        /// Loop through all the commands and create the command objects.
        /// </summary>
        /// <param name="commands"></param>
        protected virtual void LoadCommands(CommandSchemaCollection commands) {
            if (Configuration.Instance.IncludeFunctions && commands != null && commands.Count > 0) {
                foreach (CommandSchema command in commands) {
                    if (!Configuration.Instance.IncludeRegexIsMatch(command.FullName) || Configuration.Instance.ExcludeRegexIsMatch(command.FullName) || EntityStore.Instance.GetEntity(command.FullName) != null) {
                        Trace.WriteLine(String.Format("Skipping command: '{0}'", command.FullName));
                        Debug.WriteLine(String.Format("Skipping command: '{0}'", command.FullName));

                        EntityStore.Instance.ExcludedEntityCollection.Add(command.FullName, null);
                        continue;
                    }

                    var commandEntity = new CommandEntity(command);
                    EntityStore.Instance.CommandCollection.Add(command.FullName, commandEntity);
                    EntityStore.Instance.EntityCollection.Add(command.FullName, commandEntity);
                }
            }
        }

        #endregion

        public string Name { get { return "TableSchema Entity Provider"; } }

        public string Description { get { return "TableSchema Entity Provider"; } }
    }
}
