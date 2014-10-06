// Copyright (c) CodeSmith Tools, LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data;
using System.Diagnostics;
using CodeSmith.SchemaHelper.Util;
using SchemaExplorer;

namespace CodeSmith.SchemaHelper {
    /// <summary>
    /// A View Entity.
    /// </summary>
    [DebuggerDisplay("ViewEntity = {Name}, Key = {EntityKeyName}")]
    public sealed class ViewEntity : EntityBase<IViewSchema>, ISchemaEntity {
        #region Private Members

        private DataTable _data;

        private string _sourceText;

        #endregion

        #region Constructor(s)

        /// <summary>
        /// Constructor that passes in the Table that this class will represent.
        /// </summary>
        public ViewEntity(IViewSchema view) : base(view) {
            EntityKeyName = EntitySource.Name;
            SchemaName = EntitySource.Owner;
            Namespace = NamingConventions.PropertyName(EntitySource.Database.Name);
            ConnectionString = EntitySource.Database.ConnectionString;

            LoadProperties();
            LoadKeys(); // Called second because it is populated from already loaded properties...

            // Cannot update, insert, or delete a view.
            CanUpdate = false; //TODO: Updatable Views?
            CanDelete = false;
            CanInsert = false;
        }

        #endregion

        #region Public Overrides

        /// <summary>
        /// Initialize the view
        /// </summary>
        public override void Initialize() {
            LoadSearchCriteria();
        }

        #endregion

        #region Protected Overrides

        /// <summary>
        /// Views do not define any keys, but in cases where frameworks need to define a key like entity framework a key is defined
        /// as any non-nullable column.
        /// </summary>
        protected override void LoadKeys() {
            foreach (var pair in PropertyMap) {
                if (Key.Properties.Count >= Configuration.Instance.MaxNumberOfKeyProperties) {
                    Trace.WriteLine(String.Format("Skipping {0} keys due to the maximum number of keys has been reached. Please contact support for more info.", PropertyMap.Count - Configuration.Instance.MaxNumberOfKeyProperties));
                    return;
                }
                var property = pair.Value as ISchemaProperty;
                if (property == null)
                    continue;

                if (property.IsPrimaryKey)
                    Key.Properties.Add(property);
            }
        }

        /// <summary>
        /// Sets the PropertyMap for all columns
        /// </summary>
        protected override void LoadProperties() {
            try {
                foreach (ViewColumnSchema column in EntitySource.Columns) {
                    if (!Configuration.Instance.ExcludeRegexIsMatch(column.FullName) && !PropertyMap.ContainsKey(column.Name))
                        PropertyMap.Add(column.Name, new ViewProperty(column, this));
                }
            } catch (NotSupportedException) {
                string message = String.Format("This provider does not support Views. Please disable the generation of views by setting IncludeViews to false.");
                Trace.WriteLine(message);
                Debug.WriteLine(message);
            }
        }

        protected override void LoadAssociations() {}

        /// <summary>
        /// Load the Extended Properties
        /// </summary>
        protected override void LoadExtendedProperties() {
            ExtendedProperties.AddRange(EntitySource);
        }

        /// <summary>
        /// Add the Search Criteria
        /// </summary>
        protected override void LoadSearchCriteria() {
            // NOTE: This does nothing for views.  There are no indexes, foreign keys, etc.
            var criteria = new ViewSearchCriteria(this);
            if (!String.IsNullOrEmpty(criteria.Key))
                SearchCriteria.Add(criteria);
        }

        #endregion

        #region ISchemaEntity Implementation

        public DataTable GetData() {
            if (_data == null)
                _data = EntitySource.GetViewData();

            return _data;
        }

        public string ConnectionString { get; private set; }

        #endregion

        #region Properties

        /// <summary>
        /// The text that created the view.
        /// </summary>
        public string SourceText {
            get {
                if (String.IsNullOrEmpty(_sourceText))
                    _sourceText = EntitySource.ViewText;

                return _sourceText;
            }
        }

        #endregion
    }
}
