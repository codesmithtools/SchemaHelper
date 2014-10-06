// Copyright (c) CodeSmith Tools, LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Generator.QuickStart {
    public class DataProjectCreator : ProjectCreator {
        public DataProjectCreator(ProjectBuilderSettings projectBuilder) : base(projectBuilder) {}

        public override string ProjectTemplateFile { get { return "DataProject.zip"; } }

        protected override void AddFiles() {}
    }
}
