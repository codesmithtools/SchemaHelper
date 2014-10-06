// Copyright (c) CodeSmith Tools, LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using CodeSmith.SchemaHelper.Util;
using SchemaExplorer;

namespace CodeSmith.SchemaHelper {
    /// <summary>
    /// </summary>
    public class TableAssociation : AssociationBase<ITableKeySchema> {
        private readonly bool _isChildManyToMany;
        private readonly IEntity _sourceManyToManyTable;

        /// <summary>
        /// </summary>
        /// <param name="source"></param>
        /// <param name="associationType"></param>
        /// <param name="entity"></param>
        /// <param name="foreignEntity"></param>
        /// <param name="isParentEntity"></param>
        /// <param name="namespace"></param>
        /// <param name="intermediaryAssociation"></param>
        /// <param name="isChildManyToMany"> </param>
        /// <param name="sourceManyToManyTable"> </param>
        public TableAssociation(ITableKeySchema source, AssociationType associationType, IEntity entity, IEntity foreignEntity, bool isParentEntity, string @namespace = null, IAssociation intermediaryAssociation = null, bool isChildManyToMany = false, IEntity sourceManyToManyTable = null) : base(source, associationType, entity, foreignEntity, isParentEntity, @namespace, intermediaryAssociation) {
            _isChildManyToMany = isChildManyToMany;
            _sourceManyToManyTable = sourceManyToManyTable;
        }

        /// <summary>
        /// Do any Post constructor initialization here.
        /// By default, this does nothing.
        /// </summary>
        public override void Initialize() {
            AssociationKeyName = AssociationSource.Name;

            if (String.IsNullOrEmpty(Namespace))
                Namespace = NamingConventions.PropertyName(AssociationSource.Database.Name);
        }

        /// <summary>
        /// Override to populate the properties from the implemented association.
        /// </summary>
        protected override void LoadProperties() {
            if (AssociationType == AssociationType.ManyToMany) {
                // From Parent Many To Many
                if (!_isChildManyToMany) {
                    for (int index = 0; index < AssociationSource.PrimaryKeyMemberColumns.Count; index++) {
                        IProperty foreignProperty = ForeignEntity.Properties.FirstOrDefault(x => x.KeyName == AssociationSource.PrimaryKeyMemberColumns[index].Name);
                        IProperty property = Entity.Properties.FirstOrDefault(x => x.KeyName == AssociationSource.ForeignKeyMemberColumns[index].Name);

                        //This checks to see if one side of the association is ignored (ignored column name etc...).
                        if (property != null && foreignProperty != null)
                            AddAssociationProperty(property, foreignProperty);
                    }

                    return;
                }

                // From Child Many To Many
                if (IntermediaryAssociation != null) {
                    //association = new TableAssociation(AssociationSource, AssociationType.ManyToMany, source, intermediaryEntity, true, intermediaryAssociation: intermediaryAssocation);
                    for (int index = 0; index < AssociationSource.PrimaryKeyMemberColumns.Count; index++) {
                        List<IProperty> properties = Entity.EntityKeyName.Equals(AssociationSource.PrimaryKeyMemberColumns[index].Table.Name, StringComparison.OrdinalIgnoreCase) ? Entity.Properties : ForeignEntity.Properties;
                        IProperty foreignProperty = ForeignEntity.Properties.FirstOrDefault(x => x.KeyName == AssociationSource.ForeignKeyMemberColumns[index].Name);
                        IProperty property = properties.FirstOrDefault(x => x.KeyName == AssociationSource.PrimaryKeyMemberColumns[index].Name);

                        //This checks to see if one side of the association is ignored (ignored column name etc...).
                        if (property != null && foreignProperty != null)
                            AddAssociationProperty(property, foreignProperty);
                    }
                } else if (_sourceManyToManyTable != null) {
                    //var intermediaryAssocation = new TableAssociation(AssociationSource, AssociationType.ManyToMany, Entity, ForeignEntity, false);
                    for (int index = 0; index < AssociationSource.PrimaryKeyMemberColumns.Count; index++) {
                        List<IProperty> properties = _sourceManyToManyTable.EntityKeyName.Equals(AssociationSource.PrimaryKeyMemberColumns[index].Table.Name, StringComparison.OrdinalIgnoreCase) ? _sourceManyToManyTable.Properties : Entity.Properties;
                        IProperty foreignProperty = ForeignEntity.Properties.FirstOrDefault(x => x.KeyName == AssociationSource.ForeignKeyMemberColumns[index].Name);
                        IProperty property = properties.FirstOrDefault(x => x.KeyName == AssociationSource.PrimaryKeyMemberColumns[index].Name);

                        //This checks to see if one side of the association is ignored (ignored column name etc...).
                        if (property != null && foreignProperty != null)
                            AddAssociationProperty(property, foreignProperty);
                    }
                }

                return;
            }

            if (!IsParentEntity) {
                // Populate Parent Association Properties
                for (int index = 0; index < AssociationSource.ForeignKeyMemberColumns.Count; index++) {
                    IMemberColumnSchema column = AssociationSource.ForeignKeyMemberColumns[index]; //Local Column.
                    IMemberColumnSchema foreignColumn = AssociationSource.PrimaryKeyMemberColumns[index];

                    //Check to see if the IProperty is a primary key column, if it is a primary key, it also needs to be a Foreign
                    if (!column.IsPrimaryKeyMember || (column.IsPrimaryKeyMember && column.IsForeignKeyMember)) {
                        //Find the Properties for the columns
                        IProperty associatedMember = ForeignEntity.Properties.FirstOrDefault(x => x.KeyName == foreignColumn.Name);
                        IProperty property = Entity.Properties.FirstOrDefault(x => x.KeyName == column.Name);

                        //This checks to see if one side of the association is ignored (ignored column name etc...).
                        if (property != null && associatedMember != null)
                            AddAssociationProperty(property, associatedMember);
                    }
                }
            } else {
                // Populate Child Association Properties
                for (int index = 0; index < AssociationSource.ForeignKeyMemberColumns.Count; index++) {
                    IMemberColumnSchema column = AssociationSource.PrimaryKeyMemberColumns[index]; //Local Column.
                    IMemberColumnSchema foreignColumn = AssociationSource.ForeignKeyMemberColumns[index];

                    //Find the Properties for the columns
                    IProperty associatedMember = ForeignEntity.Properties.FirstOrDefault(x => x.KeyName == foreignColumn.Name);
                    IProperty property = Entity.Properties.FirstOrDefault(x => x.KeyName == column.Name);

                    //This checks to see if one side of the association is ignored (ignored column name etc...).
                    if (property != null && associatedMember != null)
                        AddAssociationProperty(property, associatedMember);
                }
            }
        }

        /// <summary>
        /// Override to populate the extended properties from the implemented association.
        /// </summary>
        protected override void LoadExtendedProperties() {
            ExtendedProperties.AddRange(AssociationSource);
        }

        #region Parent Associations

        /// <summary>
        /// Gets a parent association from a foreign key.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="tableKeySchema"></param>
        /// <returns></returns>
        public static TableAssociation FromParentForeignKey(TableEntity source, ITableKeySchema tableKeySchema) {
            //In Parent Associations, the Primary table should be the Associated IEntity
            IEntity foreignEntity = EntityStore.Instance.GetEntity(tableKeySchema.PrimaryKeyTable.FullName);
            if (foreignEntity == null) {
                Trace.WriteLine("Foreign Key not generated since related Table not included in generation");
                return null;
            }

            bool isParentMany = false;
            bool isParentOne = false;
            bool isParentOneOrZero = false;
            bool isParentUnique = tableKeySchema.PrimaryKeyMemberColumns.Count(m => m.IsUnique || m.IsPrimaryKeyMember) == tableKeySchema.PrimaryKeyMemberColumns.Count;
            if (isParentUnique && tableKeySchema.PrimaryKeyMemberColumns.Count(m => m.AllowDBNull) != tableKeySchema.PrimaryKeyMemberColumns.Count)
                isParentOne = true;
            else if (isParentUnique && tableKeySchema.PrimaryKeyMemberColumns.Count(m => m.AllowDBNull) == tableKeySchema.PrimaryKeyMemberColumns.Count)
                isParentOneOrZero = true;
            else
                isParentMany = true;

            bool isForeignKeyAlsoComposite = tableKeySchema.ForeignKeyTable.HasPrimaryKey && tableKeySchema.ForeignKeyTable.PrimaryKey.MemberColumns.Count > 1;

            bool isChildMany = false;
            bool isChildOne = false;
            bool isChildOneOrZero = false;
            bool isChildUnique = !isForeignKeyAlsoComposite && tableKeySchema.ForeignKeyMemberColumns.Count(m => m.IsUnique || m.IsPrimaryKeyMember) == tableKeySchema.ForeignKeyMemberColumns.Count;
            if (isChildUnique && tableKeySchema.ForeignKeyMemberColumns.Count(m => m.AllowDBNull) != tableKeySchema.ForeignKeyMemberColumns.Count)
                isChildOne = true;
            else if (isChildUnique && tableKeySchema.ForeignKeyMemberColumns.Count(m => m.AllowDBNull) == tableKeySchema.ForeignKeyMemberColumns.Count)
                isChildOneOrZero = true;
            else
                isChildMany = true;

            var type = AssociationType.ManyToOne;
            if (isChildMany && isParentMany)
                type = AssociationType.ManyToMany;
            else if (isChildMany && isParentOne)
                type = AssociationType.ManyToOne;
            else if (isChildMany && isParentOneOrZero)
                type = AssociationType.ManyToZeroOrOne;
            else if (isChildOne && isParentMany)
                type = AssociationType.OneToMany;
            else if (isChildOne && isParentOne)
                type = AssociationType.OneToOne;
            else if (isChildOne && isParentOneOrZero)
                type = AssociationType.OneToZeroOrOne;
            else if (isChildOneOrZero && isParentMany)
                type = AssociationType.ZeroOrOneToMany;
            else
                Debug.WriteLine("Contact support cause you have a crazy up schema.");

            var association = new TableAssociation(tableKeySchema, type, source, foreignEntity, false);
            if (association.Properties.Count <= 0 || String.IsNullOrEmpty(association.AssociationKey))
                return null;

            return association;
        }

        /// <summary>
        /// Gets a parent many to many association from a foreign key.
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static TableAssociation FromParentManyToMany(TableEntity source) {
            if (!source.EntitySource.IsManyToMany())
                return null;

            //ASpNetUser -> AspNetUserInRole -> AspnetRole
            ITableSchema rightTable = GetToManyTable(source.EntitySource, source.EntitySource);
            ITableSchema leftTable = GetToManyTable(source.EntitySource, rightTable) ?? GetToManyTable(source.EntitySource, source.EntitySource); // This could be a Many-To-Many relation ship to the same table User (User) -> UserMappings <- User (Usee).

            if (leftTable == null || rightTable == null)
                return null;

            IEntity rightEntity = EntityStore.Instance.GetEntity(rightTable.FullName);
            IEntity leftEntity = EntityStore.Instance.GetEntity(leftTable.FullName);
            if (leftEntity == null || rightEntity == null)
                return null;

            int leftIndex = leftEntity.EntityKeyName.Equals(source.EntitySource.ForeignKeys[0].PrimaryKeyMemberColumns[0].Table.Name, StringComparison.OrdinalIgnoreCase) ? 0 : 1;
            int rightIndex = leftIndex == 1 ? 0 : 1;

            var intermediaryAssocation = new TableAssociation(source.EntitySource.ForeignKeys[rightIndex], AssociationType.ManyToMany, source, rightEntity, true);
            var association = new TableAssociation(source.EntitySource.ForeignKeys[leftIndex], AssociationType.ManyToMany, source, leftEntity, true, intermediaryAssociation: intermediaryAssocation);

            //End Association between AspNetUser -> ASpNetRole with Intermediary table as a property
            if (association.Properties.Count <= 0 || String.IsNullOrEmpty(association.AssociationKey))
                return null;

            return association;
        }

        #endregion

        #region Child Associations

        /// <summary>
        /// </summary>
        /// <param name="source"></param>
        /// <param name="tableKeySchema"></param>
        /// <returns></returns>
        public static TableAssociation FromChildPrimaryKey(TableEntity source, ITableKeySchema tableKeySchema) {
            //In Child Associations, the ForeignKey should be the associated entity
            IEntity associationEntity = EntityStore.Instance.GetEntity(tableKeySchema.ForeignKeyTable.FullName);

            if (associationEntity == null && !Configuration.Instance.IncludeManyToManyEntity && tableKeySchema.ForeignKeyTable.IsManyToMany())
                associationEntity = EntityStore.Instance.GetExcludedEntity(tableKeySchema.ForeignKeyTable.FullName);

            if (associationEntity == null)
                return null;

            TableAssociation association = null;
            foreach (IMemberColumnSchema foreignColumn in tableKeySchema.ForeignKeyMemberColumns) {
                //Added a check to see if the FK is also a Foreign composite key (http://community.codesmithtools.com/forums/t/10266.aspx).
                bool isForeignKeyAlsoComposite = foreignColumn.Table.HasPrimaryKey && foreignColumn.Table.PrimaryKey.MemberColumns.Count > 1 && foreignColumn.IsPrimaryKeyMember && foreignColumn.IsForeignKeyMember;

                if (!foreignColumn.IsPrimaryKeyMember || isForeignKeyAlsoComposite) {
                    if (foreignColumn.Table.IsManyToMany()) {
                        //&& foreignTable != null) // NOTE: This can because a ManyToMany can be to itself.
                        ITableSchema foreignTable = GetToManyTable(foreignColumn.Table, source.EntitySource) ?? source.EntitySource;
                        association = FromChildManyToMany(source, foreignColumn, foreignTable.FullName);
                    } else {
                        var type = AssociationType.OneToMany;
                        if (tableKeySchema.ForeignKeyMemberColumns.Count(m => m.AllowDBNull) == tableKeySchema.ForeignKeyMemberColumns.Count)
                            type = AssociationType.ZeroOrOneToMany;
                        else if (!isForeignKeyAlsoComposite && tableKeySchema.ForeignKeyMemberColumns.Count(m => m.IsUnique || m.IsPrimaryKeyMember) == tableKeySchema.ForeignKeyMemberColumns.Count)
                            type = AssociationType.OneToZeroOrOne;

                        association = new TableAssociation(tableKeySchema, type, source, associationEntity, true);
                    }
                } else if (GetToManyTable(foreignColumn.Table, source.EntitySource) == null)
                    association = new TableAssociation(tableKeySchema, AssociationType.OneToZeroOrOne, source, associationEntity, true);
            }

            //End Association between AspNetUser -> ASpNetRole with Intermediary table as a property
            if (association == null || association.Properties.Count <= 0 || String.IsNullOrEmpty(association.AssociationKey))
                return null;

            return association;
        }

        /// <summary>
        /// </summary>
        /// <param name="source"></param>
        /// <param name="foreignColumn"></param>
        /// <param name="foreignTableName"></param>
        /// <returns></returns>
        public static TableAssociation FromChildManyToMany(TableEntity source, IMemberColumnSchema foreignColumn, string foreignTableName) {
            TableAssociation association = null;

            //ASpNetUser -> AspNetUserInRole -> AspnetRole
            IEntity intermediaryEntity = EntityStore.Instance.GetEntity(foreignColumn.Table.FullName) ?? EntityStore.Instance.GetExcludedEntity(foreignColumn.Table.FullName);
            IEntity rightEntity = EntityStore.Instance.GetEntity(foreignTableName);

            if (intermediaryEntity != null && rightEntity != null) {
                int leftIndex = source.EntityKeyName.Equals(foreignColumn.Table.ForeignKeys[0].PrimaryKeyMemberColumns[0].Table.Name, StringComparison.OrdinalIgnoreCase) ? 0 : 1;
                int rightIndex = leftIndex == 1 ? 0 : 1;

                var intermediaryAssocation = new TableAssociation(foreignColumn.Table.ForeignKeys[rightIndex], AssociationType.ManyToMany, rightEntity, intermediaryEntity, false, isChildManyToMany: true, sourceManyToManyTable: source);
                association = new TableAssociation(foreignColumn.Table.ForeignKeys[leftIndex], AssociationType.ManyToMany, source, intermediaryEntity, true, intermediaryAssociation: intermediaryAssocation, isChildManyToMany: true);
                //End Association between AspNetUser -> ASpNetRole with Intermediary table as a property
            }

            if (association == null || association.Properties.Count <= 0 || String.IsNullOrEmpty(association.AssociationKey))
                return null;

            return association;
        }

        #endregion

        private static ITableSchema GetToManyTable(ITableSchema manyToTable, ITableSchema sourceTable) {
            if (manyToTable == null || sourceTable == null)
                return null;

            // This will return null if the one or two of the foreign keys point to the manyToMany Table.
            if (!manyToTable.IsManyToMany() && manyToTable.ForeignKeys.Count(m => m.PrimaryKeyTable.FullName.Equals(sourceTable.FullName, StringComparison.InvariantCulture)) > 0)
                return null;

            ITableSchema result = null;
            foreach (TableKeySchema key in manyToTable.ForeignKeys) {
                if (!key.PrimaryKeyTable.FullName.Equals(sourceTable.FullName, StringComparison.InvariantCulture)) {
                    result = key.PrimaryKeyTable;
                    break;
                }
            }

            return result;
        }
    }
}
