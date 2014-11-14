// Copyright (c) CodeSmith Tools, LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using CodeSmith.Core.Extensions;
using CodeSmith.SchemaHelper.Util;

namespace CodeSmith.SchemaHelper {
    /// <summary>
    /// IProperty of an Entity.
    /// Example would be a table column, view column, or stored procedure return column.
    /// </summary>
    public abstract class PropertyBase<T> : IProperty where T : class {
        #region Private Members

        private string _description = String.Empty;
        private string _name = String.Empty;
        private string _privateMemberVariableName = String.Empty;
        private string _variableName = String.Empty;

        private string _baseSystemType = String.Empty;
        private string _systemTypeWithSize = String.Empty;
        private string _defaultValue;
        protected bool? _isReadOnly;
        private Dictionary<string, object> _extendedProperties;

        #endregion

        #region Constructor(s)

        /// <summary>
        /// Constructor that passes in the property that this class will represent.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="property"></param>
        protected PropertyBase(T property, IEntity entity) {
            if (property == null)
                throw new ArgumentException("Property cannot be null.", "property");

            if (entity == null)
                throw new ArgumentException("Entity cannot be null.", "entity");

            Entity = entity;
            PropertySource = property;

            Initialize();

            AccessibilityHelper.UpdatePropertyAccessibility(this);
        }

        #endregion

        #region Properties

        /// <summary>
        /// This is the source where the property is populated from (E.G. EDMX Node, ColumnSchema, CommandSchemaMember..);
        /// </summary>
        public T PropertySource { get; protected set; }

        /// <summary>
        /// The entity that this property is associated with.
        /// </summary>
        public IEntity Entity { get; protected set; }

        /// <summary>
        /// Type of the property.
        /// </summary>
        public PropertyType PropertyType { get; set; }

        /// <summary>
        /// Returns the original name of the property (E.G. The original column name).
        /// </summary>
        public string KeyName { get; protected set; }

        #region Description

        /// <summary>
        /// Returns True of the Description has a Value
        /// </summary>
        public bool HasDescription {
            get {
                if (!String.IsNullOrEmpty(Description))
                    return Description.Trim().Length > 0;

                return false;
            }
        }

        /// <summary>
        /// The Description of the IProperty
        /// </summary>
        /// <summary>
        /// Description from the extended properties
        /// </summary>
        public string Description {
            get {
                if (String.IsNullOrEmpty(_description))
                    _description = GetDescription();

                return _description;
            }
            protected set { _description = value; }
        }

        /// <summary>
        /// Return the description of the Column.
        /// </summary>
        /// <returns></returns>
        protected virtual string GetDescription() {
            return ExtendedProperties.ContainsKey(Configuration.Instance.DescriptionExtendedProperty) ? ExtendedProperties[Configuration.Instance.DescriptionExtendedProperty].ToString().Trim() : String.Empty;
        }

        #endregion

        #region Accessibility Modifiers

        /// <summary>
        /// The Type Access of the IProperty
        /// </summary>
        public string TypeAccess { get; protected internal set; }

        /// <summary>
        /// The Getter Access of the IProperty
        /// </summary>
        public string GetterAccess { get; protected internal set; }

        /// <summary>
        /// The Setter Access of the IProperty
        /// </summary>
        public string SetterAccess { get; protected internal set; }

        /// <summary>
        /// Returns true if the property should be treated as read only (Identity, currency, computed column or contains an extended property).
        /// </summary>
        public virtual bool IsReadOnly {
            get {
                if (!_isReadOnly.HasValue) {
                    _isReadOnly = IsType(PropertyType.Identity) 
                        || IsType(PropertyType.Concurrency) 
                        || IsType(PropertyType.Computed) 
                        || ExtendedProperties.ContainsKey(Configuration.Instance.IsReadOnlyColumnExtendedProperty);
                }

                return _isReadOnly.Value;
            }
        }

        #endregion

        /// <summary>
        /// Collection of Extended properties.
        /// </summary>
        public Dictionary<string, object> ExtendedProperties {
            get {
                if (_extendedProperties == null) {
                    _extendedProperties = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
                    LoadExtendedProperties();
                }

                return _extendedProperties;
            }
        }

        #region Naming

        /// <summary>
        /// Returns the resolved name of the IProperty.
        /// </summary>
        public string Name {
            get {
                if (String.IsNullOrEmpty(_name))
                    _name = NamingConventions.ValidateName(this);

                return _name;
            }
            protected set {
                _name = value;
                _privateMemberVariableName = null;
                _variableName = null;
            }
        }

        /// <summary>
        /// The Private IProperty variable name.
        /// </summary>
        public string PrivateMemberVariableName {
            get {
                if (String.IsNullOrEmpty(_privateMemberVariableName))
                    _privateMemberVariableName = NamingConventions.PrivateMemberVariableName(Name, false, Configuration.Instance.NamingProperty.PropertyNaming == PropertyNaming.Preserve);

                return _privateMemberVariableName;
            }
        }

        /// <summary>
        /// Variable name used within scope of a Method or Method parameter.
        /// </summary>
        public string VariableName {
            get {
                if (String.IsNullOrEmpty(_variableName))
                    _variableName = NamingConventions.VariableName(Name, false, Configuration.Instance.NamingProperty.PropertyNaming == PropertyNaming.Preserve);

                return _variableName;
            }
        }

        #endregion

        #region Data Type Related

        /// <summary>
        /// Returns the string for the System type of the property.
        /// </summary>
        public string SystemType { get; protected set; }

        /// <summary>
        /// Returns the SystemType using the Size of the IProperty.
        /// </summary>
        public string SystemTypeWithSize {
            get {
                if (String.IsNullOrEmpty(_systemTypeWithSize) && Size >= 0)
                    _systemTypeWithSize = Configuration.Instance.TargetLanguage == Language.VB ? SystemType.Replace("()", String.Format("({0})", Size)) : SystemType.Replace("[]", String.Format("[{0}]", Size));

                return _systemTypeWithSize;
            }
        }

        /// <summary>
        /// It returns the SystemType for Nullable types.
        /// </summary>
        public string BaseSystemType {
            get {
                if (String.IsNullOrEmpty(_baseSystemType)) {
                    _baseSystemType = SystemType.Replace("?", String.Empty);
                    if (_baseSystemType.Contains("System.Nullable(Of ", StringComparison.OrdinalIgnoreCase))
                        _baseSystemType = _baseSystemType.Replace("System.Nullable(Of ", String.Empty).Replace(")", String.Empty);
                }

                return _baseSystemType;
            }
        }

        /// <summary>
        /// The Default Value of the Property.
        /// </summary>
        public string DefaultValue {
            get {
                if (String.IsNullOrEmpty(_defaultValue))
                    _defaultValue = LoadDefaultValue();

                return _defaultValue;
            }
        }

        /// <summary>
        /// The size of the Property.
        /// </summary>
        public int Size { get; protected set; }

        /// <summary>
        /// The Scale of the Property.
        /// </summary>
        public decimal Scale { get; protected set; }

        /// <summary>
        /// The Precision of the Property.
        /// </summary>
        public decimal Precision { get; protected set; }

        /// <summary>
        /// The Fixed Length of the Property.
        /// </summary>
        public bool FixedLength { get; protected set; }

        /// <summary>
        /// The Unicode of the Property.
        /// </summary>
        public bool Unicode { get; protected set; }

        /// <summary>
        /// Is the IProperty nullable.
        /// </summary>
        public bool IsNullable { get; protected set; }

        #endregion

        #endregion

        public void AppendNameSuffix(int suffix) {
            Name = String.Concat(Name, suffix);
        }

        public void SetName(string name) {
            Name = name;
        }

        #region Public Overridden Method(s)

        /// <summary>
        /// Loads the Property Settings
        /// </summary>
        public abstract void Initialize();

        /// <summary>
        /// Override to populate the extended properties from the implemented property source.
        /// </summary>
        protected virtual void LoadExtendedProperties() {}

        /// <summary>
        /// Override to populate the default value for a property.
        /// </summary>
        protected virtual string LoadDefaultValue() {
            return null;
        }

        /// <summary>
        /// Returns the Name of the property
        /// </summary>
        /// <returns></returns>
        public override string ToString() {
            return Name;
        }

        #endregion

        /// <summary>
        /// Returns true if the passed in property has a specific flag set.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public bool IsType(PropertyType type) {
            switch (type) {
                case PropertyType.Keys:
                    return PropertyType.IsFlagOn(PropertyType.Key) || PropertyType.IsFlagOn(PropertyType.Foreign);
                case PropertyType.NoConcurrency:
                    return !PropertyType.IsFlagOn(PropertyType.Concurrency);
                case PropertyType.NoKey:
                    return !PropertyType.IsFlagOn(PropertyType.Key);
                case PropertyType.NoForeign:
                    return !PropertyType.IsFlagOn(PropertyType.Foreign);
                case PropertyType.NoKeys:
                    return !PropertyType.IsFlagOn(PropertyType.Key) && !PropertyType.IsFlagOn(PropertyType.Foreign);
                case PropertyType.NoKeysOrConcurrency:
                    return !PropertyType.IsFlagOn(PropertyType.Key) && !PropertyType.IsFlagOn(PropertyType.Foreign) && !PropertyType.IsFlagOn(PropertyType.Concurrency);
                case PropertyType.NonIdentity:
                    return !PropertyType.IsFlagOn(PropertyType.Identity);
                case PropertyType.UpdateInsert:
                    return !PropertyType.IsFlagOn(PropertyType.Identity) && !PropertyType.IsFlagOn(PropertyType.Concurrency) && !PropertyType.IsFlagOn(PropertyType.Computed);
                default:
                    return PropertyType.IsAnyFlagOn(type);
            }
        }

        // TODO: Remove this as it is not needed...
        public virtual string GetSafeName() {
            return NamingConventions.GetSafeName(KeyName);
        }
    }
}
