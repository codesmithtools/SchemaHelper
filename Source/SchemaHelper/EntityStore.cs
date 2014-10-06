// Copyright (c) CodeSmith Tools, LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace CodeSmith.SchemaHelper {
    /// <summary>
    /// Stores all the Entities in a collection. EntityStore is made static so it can be accessed anywhere.
    /// </summary>
    public sealed class EntityStore {
        private EntityStore() {
            EntityCollection = new SortedDictionary<string, IEntity>();
            ExcludedEntityCollection = new SortedDictionary<string, IEntity>();
            CommandCollection = new SortedDictionary<string, CommandEntity>();
        }

        #region Public Properties

        #region Instance

        /// <summary>
        /// </summary>
        public static EntityStore Instance { get { return Nested.Current; } }

        /// <summary>
        /// </summary>
        private class Nested {
            /// <summary>
            /// Current singleton instance.
            /// </summary>
            internal static readonly EntityStore Current;

            /// <summary>
            /// </summary>
            static Nested() {
                Current = new EntityStore();
            }
        }

        #endregion

        /// <summary>
        /// Collection of all the valid entities
        /// </summary>
        public SortedDictionary<string, IEntity> EntityCollection { get; internal set; }

        /// <summary>
        /// Collection of all the Entities that are excluded
        /// </summary>
        public SortedDictionary<string, IEntity> ExcludedEntityCollection { get; internal set; }

        /// <summary>
        /// Collection of Commands (Stored Procedures)
        /// </summary>
        public SortedDictionary<string, CommandEntity> CommandCollection { get; internal set; }

        /// <summary>
        /// Returns the entity based on the Key.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public IEntity GetEntity(string key) {
            if (EntityCollection.ContainsKey(key))
                return EntityCollection[key];

            return null;
        }

        /// <summary>
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public IEntity GetExcludedEntity(string key) {
            if (ExcludedEntityCollection.ContainsKey(key))
                return ExcludedEntityCollection[key];

            return null;
        }

        /// <summary>
        /// Returns a list of Commands that match the naming for this Entity
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public IEnumerable<CommandEntity> GetCommandsForEntity(IEntity entity) {
            return CommandCollection.Values.Where(cmd => cmd.MatchesEntity(entity)).ToList();
        }

        #endregion
    }
}
