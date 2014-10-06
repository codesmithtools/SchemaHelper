// Copyright (c) CodeSmith Tools, LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel;
using CodeSmith.Engine;

namespace CodeSmith.SchemaHelper {
    [Serializable]
    [PropertySerializer(typeof(XmlPropertySerializer))]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class SearchCriteriaProperty {
        #region Constructor(s)

        public SearchCriteriaProperty() {
            SearchCriteria = SearchCriteriaType.All;
            Prefix = "GetBy";
            Delimeter = String.Empty;
            MethodKeySuffix = "Key";
            Suffix = String.Empty;
        }

        #endregion

        #region Public Overridden Method(s)

        public override string ToString() {
            return "(Expand to edit...)";
        }

        #endregion

        #region Public Properties

        [NotifyParentProperty(true)]
        [Description("Which SearchCriteria to generate.")]
        public SearchCriteriaType SearchCriteria { get; set; }

        [NotifyParentProperty(true)]
        [Description("Prefix for a search method.")]
        public string Prefix { get; set; }

        [NotifyParentProperty(true)]
        [Description("Delimeter between member names for a search method.")]
        public string Delimeter { get; set; }

        [NotifyParentProperty(true)]
        [Description("Suffix for a Key.")]
        public string MethodKeySuffix { get; set; }

        [NotifyParentProperty(true)]
        [Description("Suffix for a search method.")]
        public string Suffix { get; set; }

        #endregion
    }
}
