// Copyright (c) CodeSmith Tools, LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace CodeSmith.SchemaHelper {
    [Flags]
    public enum PropertyType : int {
        Normal = 1,
        Key = 2,
        Foreign = 4,
        Identity = 8,
        Concurrency = 16,
        Computed = 32,
        Index = 64,

        All = Normal | Key | Foreign | Identity | Concurrency | Computed | Index,
        Keys = Key | Foreign,
        NoConcurrency = All ^ Concurrency,
        NoKey = All ^ Key,
        NoForeign = All ^ Foreign,
        NoKeys = All ^ Keys,
        NoKeysOrConcurrency = All & ~Key & ~Foreign & ~Concurrency,
        NonIdentity = All ^ Identity,
        UpdateInsert = (Normal | Foreign | Key) & ~Identity & ~Concurrency & ~Computed
    }
}
