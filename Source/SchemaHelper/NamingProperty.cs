// Copyright (c) CodeSmith Tools, LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel;
using CodeSmith.Engine;

namespace CodeSmith.SchemaHelper {
    [Serializable]
    [PropertySerializer(typeof(XmlPropertySerializer))]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class NamingProperty {
        private EntityNaming _entityNaming = EntityNaming.Singular;
        private PropertyNaming _propertyNaming = PropertyNaming.NormalizeRemovePrefix;
        private AssociationNaming _associationNaming = AssociationNaming.Plural;

        public override string ToString() {
            return "(Expand to edit...)";
        }

        [NotifyParentProperty(true)]
        [Description("Desired naming convention to be used by generator.")]
        public EntityNaming EntityNaming { get { return _entityNaming; } set { _entityNaming = value; } }

        [NotifyParentProperty(true)]
        [Description("Desired column naming convention to be used by generator.")]
        public PropertyNaming PropertyNaming { get { return _propertyNaming; } set { _propertyNaming = value; } }

        [NotifyParentProperty(true)]
        [Description("Desired association naming convention to be used by generator.")]
        public AssociationNaming AssociationNaming { get { return _associationNaming; } set { _associationNaming = value; } }
    }
}
