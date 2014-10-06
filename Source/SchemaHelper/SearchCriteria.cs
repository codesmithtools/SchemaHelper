// Copyright (c) CodeSmith Tools, LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Text;
using CodeSmith.Core.Extensions;
using CodeSmith.SchemaHelper.Util;

namespace CodeSmith.SchemaHelper {
    /// <summary>
    /// </summary>
    public class SearchCriteria {
        private string _methodName;
        private string _associatedMethodName;

        /// <summary>
        /// </summary>
        /// <param name="type"></param>
        public SearchCriteria(SearchCriteriaType type) {
            ForeignProperties = new List<AssociationProperty>();
            Properties = new List<IProperty>();
            SearchCriteriaType = type;
        }

        #region Public Read-Only Properties

        /// <summary>
        /// The Association that the SearchCriteria is created for
        /// </summary>
        public IAssociation Association { get; set; }

        /// <summary>
        /// This is the FK associated members.
        /// </summary>
        public List<AssociationProperty> ForeignProperties { get; set; }

        /// <summary>
        /// This is the list of local columns that are part of the Search Criteria.
        /// </summary>
        public List<IProperty> Properties { get; private set; }

        /// <summary>
        /// </summary>
        public bool IsUniqueResult { get; set; }

        /// <summary>
        /// What is the MethodName for this SearchCriteria
        /// </summary>
        public string MethodName {
            get {
                if (String.IsNullOrEmpty(_methodName))
                    _methodName = GetMethodName(false);

                return _methodName;
            }
        }

        /// <summary>
        /// The associated method name for any ForeignKey Search Criteria
        /// </summary>
        public string AssociatedMethodName {
            get {
                if (String.IsNullOrEmpty(_associatedMethodName))
                    _associatedMethodName = IsType(SearchCriteriaType.ForeignKey) ? GetMethodName(true) : MethodName;

                return _associatedMethodName;
            }
        }

        /// <summary>
        /// SearchCriteriaType for this SearchCriteria
        /// </summary>
        public SearchCriteriaType SearchCriteriaType { get; internal set; }

        /// <summary>
        /// Returns true if the passed in property has a specific flag set.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public bool IsType(SearchCriteriaType type) {
            switch (type) {
                case SearchCriteriaType.PrimaryKey:
                    return SearchCriteriaType.IsFlagOn(SearchCriteriaType.PrimaryKey);
                case SearchCriteriaType.ForeignKey:
                    return SearchCriteriaType.IsFlagOn(SearchCriteriaType.ForeignKey);
                case SearchCriteriaType.NoForeignKeys:
                    return !SearchCriteriaType.IsFlagOn(SearchCriteriaType.ForeignKey);
                case SearchCriteriaType.Index:
                    return SearchCriteriaType.IsFlagOn(SearchCriteriaType.Index);
                case SearchCriteriaType.Command:
                    return SearchCriteriaType.IsFlagOn(SearchCriteriaType.Command);
                case SearchCriteriaType.View:
                    return SearchCriteriaType.IsFlagOn(SearchCriteriaType.View);
            }

            return true;
        }

        #endregion

        #region Internal Read-Only Properties

        /// <summary>
        /// They unique Key for this Search Criteria
        /// </summary>
        public virtual string Key {
            get {
                var sb = new StringBuilder();

                foreach (IProperty member in Properties)
                    sb.Append(member.Name);

                return sb.ToString();
            }
        }

        #endregion

        protected virtual string GetMethodName(bool isRemote) {
            var sb = new StringBuilder();
            bool isFirst = true;

            if (IsType(SearchCriteriaType.ForeignKey)) {
                foreach (AssociationProperty associationProperty in ForeignProperties) {
                    if (isFirst) {
                        isFirst = false;
                        sb.AppendFormat(Configuration.Instance.SearchCriteriaProperty.Prefix, NamingConventions.CleanEscapeSystemType(Association.ForeignEntity.Name));
                    } else
                        sb.Append(Configuration.Instance.SearchCriteriaProperty.Delimeter);

                    if (isRemote)
                        sb.Append(NamingConventions.CleanEscapeSystemType(associationProperty.ForeignProperty.Name));
                    else
                        sb.Append(NamingConventions.CleanEscapeSystemType(associationProperty.Property.Name));
                }

                sb.Append(Configuration.Instance.SearchCriteriaProperty.Suffix);
            } else if (IsType(SearchCriteriaType.PrimaryKey) && !String.IsNullOrEmpty(Configuration.Instance.SearchCriteriaProperty.MethodKeySuffix)) {
                sb.Append(Configuration.Instance.SearchCriteriaProperty.Prefix);
                sb.Append(Configuration.Instance.SearchCriteriaProperty.MethodKeySuffix);
            } else // Handle anything not a ForeignKey.
            {
                foreach (IProperty member in Properties) {
                    if (isFirst) {
                        isFirst = false;
                        sb.AppendFormat(Configuration.Instance.SearchCriteriaProperty.Prefix, NamingConventions.CleanEscapeSystemType(member.Entity.Name));
                    } else
                        sb.Append(Configuration.Instance.SearchCriteriaProperty.Delimeter);

                    sb.Append(member.Name);
                }

                sb.Append(Configuration.Instance.SearchCriteriaProperty.Suffix);
            }

            return sb.ToString();
        }

        public override string ToString() {
            var descriptions = new List<string>();
            if (IsType(SearchCriteriaType.PrimaryKey))
                descriptions.Add("Primary Key");

            if (IsType(SearchCriteriaType.ForeignKey))
                descriptions.Add("Foreign Key");

            if (IsType(SearchCriteriaType.Index))
                descriptions.Add("Index");

            if (IsType(SearchCriteriaType.Command))
                descriptions.Add("Command");

            if (IsType(SearchCriteriaType.View))
                descriptions.Add("View");

            return descriptions.ToDelimitedString("|");
        }
    }
}
