// Copyright (c) CodeSmith Tools, LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.Linq;
using CodeSmith.SchemaHelper.Util;
using SchemaExplorer;

namespace CodeSmith.SchemaHelper {
    /// <summary>
    /// Class that wraps a Stored Procedure into an Entity
    /// </summary>
    [DebuggerDisplay("CommandEntity = {Name}, Key = {EntityKeyName}")]
    public sealed class CommandEntity : EntityBase<ICommandSchema> {
        #region Private Member(s)

        private bool? _isFunction;

        private readonly string[] _functionExtendedPropertyNames = new[] { "CS_IsScalarFunction", "CS_IsTableValuedFunction", "CS_IsInlineTableValuedFunction", "CS_IsMultiStatementTableValuedFunction" };

        private CommandParameter _returnValueParameter;

        #endregion

        #region Constructor(s)

        /// <summary>
        /// Construct the CommandEntity from the ICommandSchema.
        /// </summary>
        /// <param name="command"></param>
        public CommandEntity(ICommandSchema command) : base(command) {
            EntityKeyName = EntitySource.Name;
            SchemaName = EntitySource.Owner;
            Namespace = NamingConventions.PropertyName(EntitySource.Database.Name);
        }

        #endregion

        #region Properties

        /// <summary>
        /// The associated Entity for this command. This is usually a Table or a View.
        /// This comes from the name of the Custom command.
        /// </summary>
        public IEntity AssociatedEntity { get; set; }

        /// <summary>
        /// Is this Command associated to an Entity
        /// </summary>
        public bool IsAssociated { get { return AssociatedEntity != null; } }

        /// <summary>
        /// Returns true if the Commands ResultSet (Properties) is a 1 - 1 match with the AssociatedEntity (properties).
        /// </summary>
        public bool IsStronglyTypedAssociatedEntity {
            get {
                if (!IsAssociated)
                    return false;

                return IsStronglyTypedAssociation(AssociatedEntity);
            }
        }

        /// <summary>
        /// Returns true if the Command is a Function.
        /// </summary>
        public bool IsFunction {
            get {
                if (_isFunction.HasValue)
                    return _isFunction.Value;

                _isFunction = false;
                foreach (string key in _functionExtendedPropertyNames) {
                    if (!ExtendedProperties.ContainsKey(key))
                        continue;

                    bool isFunction;
                    string temp = ExtendedProperties[key].ToString();
                    if (bool.TryParse(temp, out isFunction) && isFunction) {
                        _isFunction = true;
                        break;
                    }
                }

                return _isFunction.Value;
            }
        }

        /// <summary>
        /// Returns the Commands Return Value.
        /// </summary>
        public CommandParameter ReturnValueParameter { get { return _returnValueParameter; } }

        #endregion

        #region Method Overrides

        /// <summary>
        /// Initialize the Command
        /// </summary>
        public override void Initialize() {
            try {
                if (EntitySource != null && EntitySource.ReturnValueParameter != null)
                    _returnValueParameter = new CommandParameter(EntitySource.ReturnValueParameter, this);
            } catch (NotSupportedException) {
                string message = String.Format("This provider does not support Commands. Please disable the generation of commands by setting IncludeFunctions to false.");
                Trace.WriteLine(message);
                Debug.WriteLine(message);
            } catch (Exception ex) {
                string message = String.Format("Unable to load return value parameter for Command Entity '{0}'. Exception: {1}", EntityKeyName, ex.Message);
                Trace.WriteLine(message);
                Debug.WriteLine(message);
            }

            LoadSearchCriteria();
        }

        protected override void LoadKeys() {}

        /// <summary>
        /// Sets the PropertyMap for all columns.
        /// </summary>
        protected override void LoadProperties() {
            try {
                if (EntitySource != null && EntitySource.CommandResults.Count > 0) {
                    foreach (CommandResultColumnSchema column in EntitySource.CommandResults[0].Columns) {
                        var property = new CommandProperty(column, this);
                        if (!Configuration.Instance.ExcludeRegexIsMatch(column.FullName) && !PropertyMap.ContainsKey(column.Name.ToLower()))
                            PropertyMap.Add(column.Name.ToLower(), property);
                    }
                }
            } catch (NotSupportedException) {
                string message = String.Format("This provider does not support Commands. Please disable the generation of commands by setting IncludeFunctions to false.");
                Trace.WriteLine(message);
                Debug.WriteLine(message);
            } catch (Exception ex) {
                string message = String.Format("Unable to load properties for Command Entity '{0}'. Exception: {1}", EntityKeyName, ex.Message);
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
        /// Get all the parameters from the ICommandSchema and store them in the parameter collection.
        /// </summary>
        protected override void LoadSearchCriteria() {
            if (EntitySource == null)
                return;

            var criteria = new CommandSearchCritieria(this);
            if (!String.IsNullOrEmpty(criteria.Key))
                SearchCriteria.Add(criteria);
        }

        #endregion

        /// <summary>
        /// Does the name of the IEntity that is passed in match the name of the IEntity from the Command.
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public bool MatchesEntity(IEntity entity) {
            ////Append the owner to the Prefix if there is an owner
            ////TODO: This is not going to work long term....will this work in Oracle.
            //customPrefix = String.IsNullOrEmpty(entity.SourceOwner) ? customPrefix : entity.Owner + "." + customPrefix;

            // Match by name Pattern.
            string namePattern = String.Format(Configuration.Instance.CustomProcedureNameFormat, entity.EntityKeyName);
            if (EntityKeyName.Contains(namePattern)) {
                AssociatedEntity = entity;
                return true;
            }

            // Match by name and result set.
            if (EntityKeyName.Contains(entity.EntityKeyName) && IsStronglyTypedAssociation(entity)) {
                AssociatedEntity = entity;
                return true;
            }

            return false;
        }

        private bool IsStronglyTypedAssociation(IEntity entity) {
            if (entity == null || entity.Properties.Count != Properties.Count)
                return false;

            foreach (IProperty ep in entity.Properties) {
                if (!Properties.Any(p => p.Name == ep.Name && p.SystemType == ep.SystemType))
                    return false;
            }

            return true;
        }
    }
}
