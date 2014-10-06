// Copyright (c) CodeSmith Tools, LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace CodeSmith.SchemaHelper {
    public class EntityManager {
        public EntityManager(IEntityProvider provider) {
            EntityStore.Instance.EntityCollection.Clear();
            EntityStore.Instance.ExcludedEntityCollection.Clear();
            EntityStore.Instance.CommandCollection.Clear();

            if (provider.Validate())
                provider.Load();

            Entities = EntityStore.Instance.EntityCollection.Values.ToList();
            ExcludedEntities = EntityStore.Instance.ExcludedEntityCollection.Values.ToList();
        }

        public List<IEntity> Entities { get; private set; }

        public IEnumerable<IEntity> ExcludedEntities { get; private set; }
    }
}
