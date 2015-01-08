// Copyright (c) CodeSmith Tools, LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using CodeSmith.Engine;
using CodeSmith.SchemaHelper.Util;

namespace CodeSmith.SchemaHelper {
    /// <summary>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [DebuggerDisplay("Association = {Name}, Key = {AssociationKeyName}, Type = {AssociationType}, IsParentEntity = {IsParentEntity}, Entity = {Entity}, ForeignEntity = {ForeignEntity}")]
    public abstract class AssociationBase<T> : IAssociation where T : class {
        private readonly List<AssociationProperty> _properties = new List<AssociationProperty>();
        private Dictionary<string, object> _extendedProperties;
        private string _description = String.Empty;
        private string _name = String.Empty;
        private string _typeName = String.Empty;
        private string _privateMemberVariableName = String.Empty;
        private string _variableName = String.Empty;
        private string _genericProperty;
        private bool _makeUnique;

        /// <summary>
        /// This is the name of the IAssociation from the DataSource.
        /// </summary>
        public string AssociationKeyName { get; protected set; }

        /// <summary>
        /// This is the source where the Association is populated from (E.G. EDMX Node, ITableKeySchema...);
        /// </summary>
        public T AssociationSource { get; set; }

        /// <summary>
        /// This is the unique key of the IAssociation.
        /// </summary>
        public string AssociationKey { get { return IsParentEntity + AssociationKeyName; } }

        /// <summary>
        /// The namespace that the Association belongs in.
        /// </summary>
        public string Namespace { get; protected set; }

        /// <summary>
        /// Returns the association type of the first associated property.
        /// TODO: Handle different association types between members
        /// </summary>
        public AssociationType AssociationType { get; protected set; }

        /// <summary>
        /// Returns the Entity.
        /// </summary>
        public IEntity Entity { get; protected set; }

        /// <summary>
        /// Returns the Associated Entity.
        /// </summary>
        public IEntity ForeignEntity { get; protected set; }

        /// <summary>
        /// Returns the Intermediary Entity for a Many to Many association
        /// </summary>
        public IAssociation IntermediaryAssociation { get; protected set; }

        /// <summary>
        /// Returns true if the current association was created by the parent table and not the child.
        /// </summary>
        public bool IsParentEntity { get; protected set; }

        /// <summary>
        /// Returns a list of all the Properties of this Association.
        /// </summary>
        public List<AssociationProperty> Properties {
            get {
                if (_properties.Count == 0)
                    LoadProperties();

                return _properties;
            }
        }

        /// <summary>
        /// The resolved Type Name for this Entity.
        /// </summary>
        public string TypeName {
            get {
                if (String.IsNullOrEmpty(_typeName))
                    _typeName = GetTypeName();

                return _typeName;
            }
        }

        /// <summary>
        /// Resolved GenericProperty for this Association.
        /// </summary>
        public string GenericProperty {
            get {
                if (String.IsNullOrEmpty(_genericProperty))
                    _genericProperty = GetGenericProperty();

                return _genericProperty;
            }
        }

        /// <summary>
        /// The associated SearchCriteria for this Association
        /// </summary>
        public SearchCriteria SearchCriteria { get; set; }

        /// <summary>
        /// The Type Access of the Association
        /// </summary>
        public string TypeAccess { get; protected internal set; }

        /// <summary>
        /// The Getter Access of the Association
        /// </summary>
        public string GetterAccess { get; protected internal set; }

        /// <summary>
        /// The Setter Access of the Association
        /// </summary>
        public string SetterAccess { get; protected internal set; }

        /// <summary>
        /// The resolved Name for this Entity.
        /// </summary>
        public string Name {
            get {
                if (String.IsNullOrEmpty(_name))
                    _name = GetName();

                return _name;
            }
            protected set {
                _name = value;
                _privateMemberVariableName = null;
                _variableName = null;
            }
        }

        /// <summary>
        /// Returns the concatenated PrivateMemberVariableName of all properties.
        /// </summary>
        public virtual string PrivateMemberVariableName {
            get {
                if (String.IsNullOrEmpty(_privateMemberVariableName))
                    _privateMemberVariableName = NamingConventions.PrivateMemberVariableName(Name);

                return _privateMemberVariableName;
            }
        }

        /// <summary>
        /// Returns the concatenated VariableName of all properties.
        /// </summary>
        public virtual string VariableName {
            get {
                if (String.IsNullOrEmpty(_variableName))
                    _variableName = NamingConventions.VariableName(Name);

                return _variableName;
            }
        }

        /// <summary>
        /// Returns whether or not Description contains a value
        /// </summary>
        public bool HasDescription { get { return !String.IsNullOrEmpty(Description.Trim()); } }

        /// <summary>
        /// Resolved Description for this Entity.
        /// </summary>
        public string Description {
            get {
                if (String.IsNullOrEmpty(_description))
                    _description = GetDescription();

                return _description;
            }
        }

        /// <summary>
        /// Returns a list of Extended properties.
        /// </summary>
        public virtual Dictionary<string, object> ExtendedProperties {
            get {
                if (_extendedProperties == null) {
                    _extendedProperties = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
                    LoadExtendedProperties();
                }

                return _extendedProperties;
            }
        }

        protected bool IsToMany { get { return AssociationType == AssociationType.ZeroOrOneToMany || AssociationType == AssociationType.OneToMany || AssociationType == AssociationType.ManyToMany; } }

        protected AssociationBase(T source, AssociationType associationType, IEntity entity, IEntity foreignEntity, bool isParentEntity, string @namespace, IAssociation intermediaryAssociation = null) {
            if (source == null)
                throw new ArgumentException("Association source cannot be null.", "source");

            AssociationSource = source;
            AssociationType = associationType;
            Entity = entity;
            ForeignEntity = foreignEntity;
            IntermediaryAssociation = intermediaryAssociation;
            IsParentEntity = isParentEntity;
            Namespace = @namespace;

            // TODO: The code below could invoke code that will run before the inheriting constructor sets values etc..
            Initialize();
            AccessibilityHelper.UpdateAssociationAccessibility(this);
        }

        #region IAssociation Implementation

        #endregion

        #region ICommonEntityProperty Implementation

        /// <summary>
        /// Override to populate the properties from the implemented association.
        /// </summary>
        protected abstract void LoadProperties();

        protected virtual string GetTypeName() {
            return ResolveAssociationName(true);
        }

        protected virtual string ResolveAssociationName(bool preserveSuffix = false) {
            string name;
            List<string> properties;

            // Resolve the association name from the association's relationship.
            if (AssociationType == AssociationType.ManyToMany && IntermediaryAssociation != null) {
                bool result = !IntermediaryAssociation.ForeignEntity.Name.Equals(ForeignEntity.Name, StringComparison.OrdinalIgnoreCase);
                name = result ? IntermediaryAssociation.ForeignEntity.Name : IntermediaryAssociation.Entity.Name;
                properties = result ? IntermediaryAssociation.Properties.Select(p => NamingConventions.RemoveId(p.ForeignProperty.Name)).ToList() : IntermediaryAssociation.Properties.Select(p => NamingConventions.RemoveId(p.Property.Name)).ToList();
            } else {
                name = ForeignEntity.Name;
                properties = IsToMany ? Properties.Select(p => NamingConventions.RemoveId(p.ForeignProperty.Name)).ToList() : Properties.Select(p => NamingConventions.RemoveId(p.Property.Name)).ToList();
            }

            if (_makeUnique)
                name = String.Concat(String.Join(String.Empty, properties), name);

            if (preserveSuffix)
                return name;

            // update the suffixes.
            if (IsToMany) {
                switch (Configuration.Instance.NamingProperty.AssociationNaming) {
                    case AssociationNaming.List:
                        name += Configuration.Instance.ListSuffix;
                        break;
                    case AssociationNaming.SingularList:
                        name = String.Format("{0}{1}", StringUtil.ToPascalCase(StringUtil.ToSingular(name)), Configuration.Instance.ListSuffix);
                        break;
                    case AssociationNaming.Plural:
                        name = StringUtil.ToPascalCase(StringUtil.ToPlural(name));
                        break;
                    case AssociationNaming.Singular:
                        name = StringUtil.ToPascalCase(StringUtil.ToSingular(name));
                        break;
                }
            } else
                name = StringUtil.ToPascalCase(StringUtil.ToSingular(name));

            return name;
        }

        /// <summary>
        /// Returns the GenericProperty for this association.  This must be implemented by the inheriting class.
        /// </summary>
        /// <returns></returns>
        protected virtual string GetGenericProperty() {
            string genericProperty = String.Empty;

            object value;
            if (ExtendedProperties.TryGetValue(Configuration.Instance.GenericPropertyExtendedProperty, out value) && ((bool)value))
                genericProperty = "<T>";

            return genericProperty;
        }

        /// <summary>
        /// Override to populate the extended properties from the implemented association.
        /// </summary>
        protected abstract void LoadExtendedProperties();

        protected virtual string GetName() {
            string name = ResolveAssociationName();

            string suffix = !IsToMany ? Configuration.Instance.SingularMemberSuffix : StringUtil.ToPascalCase(StringUtil.ToPlural(Configuration.Instance.SingularMemberSuffix));

            if (Entity.Properties.Count(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase)) > 0)
                name += suffix;

            // If the Property is the same name as the Entity it will not compile, so add a the member prefix to resolve this.
            if (name.Equals(Entity.Name, StringComparison.OrdinalIgnoreCase))
                name += suffix;

            return name;
        }

        /// <summary>
        /// Returns the description for this association.  This must be implemented by the inheriting class.
        /// </summary>
        /// <returns></returns>
        protected virtual string GetDescription() {
            string description = String.Empty;

            if (ExtendedProperties.ContainsKey(Configuration.Instance.DescriptionExtendedProperty))
                description = ExtendedProperties[Configuration.Instance.DescriptionExtendedProperty].ToString().Replace("\r\n", " ").Trim();

            return description;
        }

        /// <summary>
        /// Do any Post constructor initialization here.
        /// By default, this does nothing.
        /// </summary>
        public abstract void Initialize();

        public void SetName(string name) {
            Name = name;
        }

        /// <summary>
        /// Expands an associations name into the full name with column properties.
        /// </summary>
        void IAssociation.MakeUnique() {
            _makeUnique = true;
            Name = GetName();
            _makeUnique = false;
        }

        /// <summary>
        /// Appends a suffix to the name.
        /// </summary>
        /// <param name="suffix">The suffix.</param>
        public void AppendNameSuffix(int suffix) {
            Name = String.Concat(Name, suffix);
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Ensures that all the names are unique.
        /// </summary>
        /// <param name="entity"></param>
        public static void ValidateAssocationNames(IEntity entity) {
            List<string> duplicates = entity.Associations.GroupBy(a => a.Name).Where(assocs => assocs.Count() > 1).Select(assocs => assocs.Key).ToList();
            foreach (string name in duplicates) {
                foreach (IAssociation association in entity.Associations) {
                    if (String.Equals(association.Name, name, StringComparison.OrdinalIgnoreCase))
                        association.MakeUnique();
                }
            }
        }

        protected virtual void AddAssociationProperty(IProperty property, IProperty foreignProperty) {
            if (property == null || String.IsNullOrEmpty(property.KeyName) || _properties.Count(p => p.Property != null && !String.IsNullOrEmpty(p.Property.KeyName) && p.Property.KeyName.Equals(property.KeyName, StringComparison.OrdinalIgnoreCase)) == 0)
                _properties.Add(new AssociationProperty(this, property, foreignProperty));
        }

        #endregion
    }
}
