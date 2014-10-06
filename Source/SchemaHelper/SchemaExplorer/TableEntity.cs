// Copyright (c) CodeSmith Tools, LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using CodeSmith.SchemaHelper.Util;
using SchemaExplorer;

namespace CodeSmith.SchemaHelper {
    /// <summary>
    /// </summary>
    [DebuggerDisplay("TableEntity = {Name}, Key = {SchemaName}.{EntityKeyName}")]
    public class TableEntity : EntityBase<ITableSchema>, ISchemaEntity {
        #region Private Member(s)

        private List<CommandEntity> _commandEntities;

        private DataTable _data;

        #endregion

        #region Constructor(s)

        /// <summary>
        /// Constructor that passes in the Table that this class will represent.
        /// </summary>
        public TableEntity(ITableSchema table) : base(table) {
            EntityKeyName = EntitySource.Name;
            SchemaName = EntitySource.Owner;
            Namespace = NamingConventions.PropertyName(EntitySource.Database.Name);
            ConnectionString = EntitySource.Database.ConnectionString;

            LoadProperties();
            LoadKeys(); // Called second because it is populated from already loaded properties...

            //Cannot update or insert tables with no Primary Key
            CanUpdate = HasKey; //TODO: Or has a unique column. || (!excludenonprimarykey from config && isunqiue)
            CanDelete = HasKey; //TODO: Or has a unique column.
            CanInsert = true;
        }

        #endregion

        #region Public Overrides

        /// <summary>
        /// Do any Post constructor initialization here.
        /// </summary>
        public override void Initialize() {
            LoadAssociations();
            LoadCommands();
            LoadSearchCriteria();
        }

        #endregion

        #region ISchemaEntity Impelmentation

        public DataTable GetData() {
            if (_data == null)
                _data = EntitySource.GetTableData();

            return _data;
        }

        public string ConnectionString { get; private set; }

        #endregion

        #region Methods

        #region Protected Method Overrides

        /// <summary>
        /// Override to populate the keys from the implemented entity.
        /// </summary>
        protected override sealed void LoadKeys() {
            foreach (var pair in PropertyMap) {
                var property = pair.Value as ISchemaProperty;
                if (property == null)
                    continue;

                if (property.IsPrimaryKey)
                    Key.Properties.Add(property);
            }
        }

        /// <summary>
        /// Override to populate the properties from the implemented entity.
        /// </summary>
        protected override sealed void LoadProperties() {
            foreach (ColumnSchema column in EntitySource.Columns) {
                var property = new TableProperty(column, this);
                if (!Configuration.Instance.ExcludeRegexIsMatch(column.FullName) && !PropertyMap.ContainsKey(column.Name))
                    PropertyMap.Add(column.Name, property);
            }
        }

        /// <summary>
        /// Override to populate the associations from the implemented entity.
        /// </summary>
        protected override void LoadAssociations() {
            if (!Configuration.Instance.IncludeAssociations)
                return;

            // Get all associations.
            GetParentAssociations();
            GetChildAssociations();
        }

        /// <summary>
        /// Load the extended properties for the entity.
        /// </summary>
        protected override void LoadExtendedProperties() {
            ExtendedProperties.AddRange(EntitySource);
        }

        /// <summary>
        /// Load the Search Criteria for the table
        /// </summary>
        protected override void LoadSearchCriteria() {
            switch (Configuration.Instance.SearchCriteriaProperty.SearchCriteria) {
                case SearchCriteriaType.All:
                    AddPrimaryKeySearchCriteria();
                    AddForeignKeySearchCriteria();
                    AddIndexSearchCriteria();
                    break;
                case SearchCriteriaType.ForeignKey:
                    AddForeignKeySearchCriteria();
                    break;
                case SearchCriteriaType.Index:
                    AddIndexSearchCriteria();
                    break;
                case SearchCriteriaType.PrimaryKey:
                    AddPrimaryKeySearchCriteria();
                    break;
                case SearchCriteriaType.NoForeignKeys:
                    AddPrimaryKeySearchCriteria();
                    AddIndexSearchCriteria();
                    break;
            }
        }

        #endregion

        #region Association Methods

        /// <summary>
        /// Populates the Parent Associations
        /// </summary>
        private void GetParentAssociations() {
            if (EntitySource.IsManyToMany()) {
                TableAssociation association = TableAssociation.FromParentManyToMany(this);
                if (association != null && !AssociationMap.ContainsKey(association.AssociationKey)) {
                    AssociationMap.Add(association.AssociationKey, association);
                    return;
                }
            }

            foreach (TableKeySchema tableKeySchema in EntitySource.ForeignKeys) {
                TableAssociation association = TableAssociation.FromParentForeignKey(this, tableKeySchema);
                if (association != null && !AssociationMap.ContainsKey(association.AssociationKey))
                    AssociationMap.Add(association.AssociationKey, association);
            }
        }

        /// <summary>
        /// Populates the Child Associations
        /// </summary>
        private void GetChildAssociations() {
            foreach (TableKeySchema tableKeySchema in EntitySource.PrimaryKeys) {
                IAssociation association = TableAssociation.FromChildPrimaryKey(this, tableKeySchema);

                // Always overwrite the previous associations if they exist because this is the parent association.
                if (association != null && !String.IsNullOrEmpty(association.AssociationKey))
                    AssociationMap[association.AssociationKey] = association;
            }
        }

        #endregion

        #region Command Methods

        /// <summary>
        /// Load all the custom commands for this Entity.
        /// </summary>
        private void LoadCommands() {
            _commandEntities = new List<CommandEntity>();
            _commandEntities.AddRange(EntityStore.Instance.GetCommandsForEntity(this));
        }

        #endregion

        #region Search Criteria Methods

        /// <summary>
        /// Add PrimaryKeys to the SearchCriteria
        /// </summary>
        private void AddPrimaryKeySearchCriteria() {
            if (Key.Properties.Count == 0)
                return;

            var searchCriteria = new SearchCriteria(SearchCriteriaType.PrimaryKey);

            foreach (IProperty property in Key.Properties) {
                if (property != null)
                    searchCriteria.Properties.Add(property);
            }

            searchCriteria.IsUniqueResult = true;

            AddToSearchCriteria(searchCriteria);
        }

        /// <summary>
        /// Add ForeignKeys to the SearchCriteria collection
        /// </summary>
        private void AddForeignKeySearchCriteria() {
            foreach (IAssociation association in AssociationMap.Values) {
                var searchCriteria = new SearchCriteria(SearchCriteriaType.ForeignKey) {
                    Association = association
                };
                foreach (AssociationProperty property in association.Properties) {
                    searchCriteria.ForeignProperties.Add(property);
                    searchCriteria.Properties.Add(property.Property);
                }

                if (association.AssociationType == AssociationType.ManyToOne || association.AssociationType == AssociationType.ManyToZeroOrOne)
                    AddToSearchCriteria(searchCriteria);

                association.SearchCriteria = searchCriteria;
            }
        }

        /// <summary>
        /// Add all the indexes to the Search Criteria
        /// </summary>
        private void AddIndexSearchCriteria() {
            foreach (IndexSchema indexSchema in EntitySource.Indexes) {
                var searchCriteria = new SearchCriteria(SearchCriteriaType.Index);

                foreach (MemberColumnSchema column in indexSchema.MemberColumns) {
                    IProperty property = Properties.FirstOrDefault(x => x.KeyName == column.Name);
                    if (property != null)
                        searchCriteria.Properties.Add(property);
                }

                if (indexSchema.IsUnique)
                    searchCriteria.IsUniqueResult = true;

                AddToSearchCriteria(searchCriteria);
            }
        }

        /// <summary>
        /// Add a SearchCriteria to the mapping collection
        /// </summary>
        /// <param name="criteria"></param>
        /// <returns></returns>
        private void AddToSearchCriteria(SearchCriteria criteria) {
            string key = criteria.Key;
            if (String.IsNullOrEmpty(key) || criteria.Properties.Count == 0)
                return;

            SearchCriteria existing = SearchCriteria.FirstOrDefault(x => String.Equals(x.Key, key, StringComparison.OrdinalIgnoreCase));
            if (existing != null) {
                existing.SearchCriteriaType |= criteria.SearchCriteriaType;
                existing.IsUniqueResult = criteria.IsUniqueResult;
            } else
                SearchCriteria.Add(criteria);
        }

        #endregion

        #endregion

        #region Properties

        /// <summary>
        /// List of custom commands (Stored Procedures) for this IEntity
        /// </summary>
        public List<CommandEntity> Commands {
            get {
                if (_commandEntities == null)
                    LoadCommands();

                return _commandEntities;
            }
        }

        #endregion
    }
}
