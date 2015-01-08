// Copyright (c) CodeSmith Tools, LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using CodeSmith.SchemaHelper.Util;

namespace CodeSmith.SchemaHelper {
    /// <summary>
    /// The base class for all Entities.
    /// </summary>
    public abstract class EntityBase<T> : IEntity where T : class {
        #region Declarations

        private readonly Dictionary<string, IAssociation> _associationMap = new Dictionary<string, IAssociation>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, IProperty> _propertyMap = new Dictionary<string, IProperty>(StringComparer.OrdinalIgnoreCase);
        private Dictionary<string, object> _extendedProperties;

        private string _name;
        private string _privateMemberVariableName;
        private string _variableName;
        private string _description;
        private string _genericProperty;
        private string _typeAccess;
        private bool _checkedTypeAccess;
        private IKey _key;

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor that passes in the entity that this class will represent.
        /// </summary>
        protected EntityBase(T source) : this(source, String.Empty) {}

        /// <summary>
        /// Constructor that passes in the entity that this class will represent.
        /// </summary>
        protected EntityBase(T source, string @namespace) {
            if (source == null)
                throw new ArgumentException("Entity source cannot be null.", "source");

            EntitySource = source;
            Namespace = @namespace;
            DerivedEntities = new List<IEntity>();
            SearchCriteria = new List<SearchCriteria>();
        }

        #endregion

        #region Abstract Methods

        /// <summary>
        /// Do any Post constructor initialization here.
        /// By default, this does nothing.
        /// </summary>
        public abstract void Initialize();

        /// <summary>
        /// Override to populate the keys from the implemented entity.
        /// </summary>
        protected abstract void LoadKeys();

        /// <summary>
        /// Override to populate the properties from the implemented entity.
        /// </summary>
        protected abstract void LoadProperties();

        /// <summary>
        /// Override to populate the associations from the implemented entity.
        /// </summary>
        protected abstract void LoadAssociations();

        /// <summary>
        /// Override to populate the extended properties from the implemented entity.
        /// </summary>
        protected abstract void LoadExtendedProperties();

        /// <summary>
        /// Override to populate the SearchCriteria from the implemented entity.
        /// </summary>
        protected abstract void LoadSearchCriteria();

        #endregion

        #region Methods

        /// <summary>
        /// </summary>
        public void ValidateAllMembers() {
            if (Configuration.Instance.ExcludeForiegnKeyIdProperties) {
                List<string> keys = PropertyMap.Where(p => p.Value.IsType(PropertyType.Foreign)).Select(p => p.Key).ToList();
                foreach (string key in keys)
                    PropertyMap.Remove(key);
            }

            AssociationBase<T>.ValidateAssocationNames(this);

            List<DuplicateMemberHelper> members = PropertyMap.Values.Select(property => new DuplicateMemberHelper {
                Member = property,
                Name = property.Name
            }).ToList();
            members.AddRange(AssociationMap.Values.Select(association => new DuplicateMemberHelper {
                Member = association,
                Name = association.Name
            }));
            foreach (DuplicateMemberHelper member in members) {
                DuplicateMemberHelper[] duplicates = members.Where(m => m.Name == member.Name).ToArray();

                if (duplicates.Length <= 1)
                    continue;

                for (int i = 0; i < duplicates.Length; i++)
                    duplicates[i].AppendNameSuffix(i + 1);
            }
        }

        public void AppendNameSuffix(int suffix) {
            Name = String.Concat(Name, suffix);
        }

        public void SetName(string name) {
            Name = name;
        }

        public virtual string GetSafeName() {
            return NamingConventions.GetSafeName(SchemaName, EntityKeyName);
        }

        public List<IProperty> GetProperties(PropertyType type) {
            return Properties.Where(p => p.IsType(type)).ToList();
        }

        /// <summary>
        /// Return the Name of the class.
        /// </summary>
        /// <returns></returns>
        public override string ToString() {
            return Name;
        }

        /// <summary>
        /// Override to populate the Inheritance properties for the implemented entity.
        /// </summary>
        protected virtual void PopulateInheritanceProperties() {}

        public List<IAssociation> GetAssociations(AssociationType type) {
            return AssociationMap.Values.Where(a => (a.AssociationType & type) == type).ToList();
        }

        /// <summary>
        /// </summary>
        /// <param name="criteria"></param>
        /// <returns></returns>
        public List<SearchCriteria> GetSearchCriteria(SearchCriteriaType criteria) {
            List<SearchCriteria> result = null;

            if (SearchCriteria != null) {
                switch (criteria) {
                    case SearchCriteriaType.PrimaryKey:
                        result = SearchCriteria.Where(sc => sc.IsType(SearchCriteriaType.PrimaryKey)).ToList();
                        break;
                    case SearchCriteriaType.ForeignKey:
                        result = SearchCriteria.Where(sc => sc.IsType(SearchCriteriaType.ForeignKey)).ToList();
                        break;
                    case SearchCriteriaType.NoForeignKeys:
                        result = SearchCriteria.Where(sc => sc.IsType(SearchCriteriaType.NoForeignKeys)).ToList();
                        break;
                    case SearchCriteriaType.Index:
                        result = SearchCriteria.Where(sc => sc.IsType(SearchCriteriaType.Index)).ToList();
                        break;
                    case SearchCriteriaType.Command:
                        result = SearchCriteria.Where(sc => sc.IsType(SearchCriteriaType.Command)).ToList();
                        break;
                    case SearchCriteriaType.View:
                        result = SearchCriteria.Where(sc => sc.IsType(SearchCriteriaType.View)).ToList();
                        break;
                    default:
                        result = SearchCriteria;
                        break;
                }
            }

            return result;
        }

        /// <summary>
        /// Returns the description for this Entity.  This must be implemented by the inheriting class.
        /// </summary>
        /// <returns></returns>
        protected virtual string GetDescription() {
            return ExtendedProperties.ContainsKey(Configuration.Instance.DescriptionExtendedProperty)
                ? ExtendedProperties[Configuration.Instance.DescriptionExtendedProperty].ToString().Replace("\r\n", " ").Replace("\n", " ").Trim() 
                : String.Empty;
        }

        /// <summary>
        /// Returns the GenericProperty for this Entity.  This must be implemented by the inheriting class.
        /// </summary>
        /// <returns></returns>
        protected virtual string GetGenericProperty() {
            string genericProperty = String.Empty;

            if (ExtendedProperties.ContainsKey(Configuration.Instance.GenericPropertyExtendedProperty) && ((bool)ExtendedProperties[Configuration.Instance.GenericPropertyExtendedProperty]))
                genericProperty = "<T>";

            return genericProperty;
        }

        #endregion

        #region Properties

        /// <summary>
        /// This is the source where the entity is populated from (E.G. EDMX Node, TableSchema, CommandSchema...);
        /// </summary>
        public T EntitySource { get; protected set; }

        /// <summary>
        /// This is the name of the Entity from the Database or Xml.
        /// </summary>
        public string EntityKeyName { get; protected set; }

        public string Namespace { get; set; }

        /// <summary>
        /// The resolved Name for this Entity.
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

        public string SchemaName { get; protected set; }

        /// <summary>
        /// The Private IEntity variable name.
        /// </summary>
        public string PrivateMemberVariableName {
            get {
                if (String.IsNullOrEmpty(_privateMemberVariableName))
                    _privateMemberVariableName = NamingConventions.PrivateMemberVariableName(Name, false, Configuration.Instance.NamingProperty.EntityNaming == EntityNaming.Preserve);

                return _privateMemberVariableName;
            }
        }

        /// <summary>
        /// Variable name used within scope of a Method or Method parameter.
        /// </summary>
        public string VariableName {
            get {
                if (String.IsNullOrEmpty(_variableName))
                    _variableName = NamingConventions.VariableName(Name, false, Configuration.Instance.NamingProperty.EntityNaming == EntityNaming.Preserve);

                return _variableName;
            }
        }

        public bool IsAbstract { get; set; }

        public string TypeAccess {
            get {
                if (!_checkedTypeAccess) {
                    string baseClassModifer = BaseEntity != null ? BaseEntity.TypeAccess : null;
                    _typeAccess = AccessibilityHelper.ClassAccessibility(_typeAccess, AccessibilityConstants.Public, true, baseClassModifer);
                    _checkedTypeAccess = true;
                }

                return _typeAccess;
            }
            protected set { _typeAccess = value; }
        }

        /// <summary>
        /// Resolved GenericProperty for this Entity.
        /// </summary>
        public string GenericProperty {
            get {
                if (String.IsNullOrEmpty(_genericProperty))
                    _genericProperty = GetGenericProperty();

                return _genericProperty;
            }
        }

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
        /// Returns whether or not Description contains a value
        /// </summary>
        public bool HasDescription { get { return !String.IsNullOrEmpty(Description.Trim()); } }

        /// <summary>
        /// Returns the Key for the Entity.
        /// </summary>
        public virtual IKey Key {
            get {
                if (_key == null)
                    _key = new EntityKey();

                return _key;
            }
        }

        /// <summary>
        /// Returns whether or not the Entity has a Key.
        /// </summary>
        public bool HasKey { get { return Key.Properties.Count > 0; } }

        /// <summary>
        /// Returns the ConcurrencyProperty of the Entity.
        /// </summary>
        public virtual IProperty ConcurrencyProperty { get { return PropertyMap.Values.FirstOrDefault(p => p.IsType(PropertyType.Concurrency)); } }

        /// <summary>
        /// Returns whether or not this IEntity has a ConcurrencyProperty.
        /// </summary>
        public virtual bool HasConcurrencyProperty { get { return (ConcurrencyProperty != null); } }

        /// <summary>
        /// The Identity column.
        /// </summary>
        public virtual IProperty IdentityProperty { get { return PropertyMap.Values.FirstOrDefault(p => p.IsType(PropertyType.Identity)); } }

        /// <summary>
        /// Does the Entity have an Identity column.
        /// </summary>
        public virtual bool HasIdentityProperty { get { return (IdentityProperty != null); } }

        /// <summary>
        /// Collection of Associations for this Entity.
        /// </summary>
        protected internal Dictionary<string, IAssociation> AssociationMap { get { return _associationMap; } }

        /// <summary>
        /// Collection of Properties for this Entity.
        /// </summary>
        protected internal Dictionary<string, IProperty> PropertyMap { get { return _propertyMap; } }

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

        /// <summary>
        /// Can this entity be updated.
        /// </summary>
        public bool CanDelete { get; protected set; }

        /// <summary>
        /// Can this entity be updated.
        /// </summary>
        public bool CanInsert { get; protected set; }

        /// <summary>
        /// Can this entity be updated.
        /// </summary>
        public bool CanUpdate { get; protected set; }

        /// <summary>
        /// Returns a list of all the Associations of this Entity.
        /// </summary>
        public List<IAssociation> Associations { get { return AssociationMap.Values.ToList(); } }

        /// <summary>
        /// The list of SearchCriteria for this IEntity
        /// </summary>
        public virtual List<SearchCriteria> SearchCriteria { get; protected internal set; }

        /// <summary>
        /// The Entity that the current entity inherits from.
        /// </summary>
        public IEntity BaseEntity { get; protected internal set; }

        /// <summary>
        /// A List of all children entities that inherit from this entity.
        /// </summary>
        public List<IEntity> DerivedEntities { get; internal set; }

        /// <summary>
        /// Returns a list of all the Properties of this Entity.
        /// </summary>
        public virtual List<IProperty> Properties {
            get {
                if (PropertyMap.Count == 0)
                    LoadProperties();

                return PropertyMap.Values.ToList();
            }
        }

        #endregion

        private class DuplicateMemberHelper {
            public object Member { get; set; }
            public string Name { get; set; }

            public void AppendNameSuffix(int i) {
                var property = Member as IProperty;
                if (property != null) {
                    property.AppendNameSuffix(i);
                    Name = property.Name;
                }

                var association = Member as IAssociation;
                if (association != null) {
                    association.AppendNameSuffix(i);
                    Name = association.Name;
                }
            }
        }
    }
}
