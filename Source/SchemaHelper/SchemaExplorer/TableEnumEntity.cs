// Copyright (c) CodeSmith Tools, LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using SchemaExplorer;

namespace CodeSmith.SchemaHelper {
    [DebuggerDisplay("TableEnumEntity = {Name}, Key = {EntityKeyName}")]
    public class TableEnumEntity : TableEntity {
        /// <summary>
        /// Constructor that passes in the Table that this class will represent.
        /// </summary>
        public TableEnumEntity(ITableSchema table) : base(table) {}
    }
}
