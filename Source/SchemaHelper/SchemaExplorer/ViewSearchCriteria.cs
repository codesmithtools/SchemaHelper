// Copyright (c) CodeSmith Tools, LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace CodeSmith.SchemaHelper {
    internal class ViewSearchCriteria : SearchCriteria {
        public ViewSearchCriteria(ViewEntity entity) : base(SearchCriteriaType.View) {}

        protected override string GetMethodName(bool isRemote) {
            return "GetResult";
        }

        public override string Key { get { return MethodName; } }
    }
}
