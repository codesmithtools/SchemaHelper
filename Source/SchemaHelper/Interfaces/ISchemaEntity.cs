// Copyright (c) CodeSmith Tools, LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data;

namespace CodeSmith.SchemaHelper {
    public interface ISchemaEntity : IEntity {
        ///// <summary>
        ///// List of custom commands (Stored Procedures) for this Entity
        ///// </summary>
        //List<CommandEntity> Commands { get; }

        ///// <summary>
        ///// Load all the custom commands for this Entity.
        ///// </summary>
        //void LoadCommands();

        /// <summary>
        /// Returns the actual data from the EntitySource.
        /// </summary>
        /// <returns></returns>
        DataTable GetData();

        /// <summary>
        /// Returns the connection string information.
        /// </summary>
        string ConnectionString { get; }
    }
}
