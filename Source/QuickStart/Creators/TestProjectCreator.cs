// Copyright (c) CodeSmith Tools, LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Generator.QuickStart {
    public class TestProjectCreator : ProjectCreator {
        private readonly string _projectTemplateFile = "TestProject.zip";

        public TestProjectCreator(ProjectBuilderSettings projectBuilder) : base(projectBuilder) {}

        public TestProjectCreator(ProjectBuilderSettings projectBuilder, string projectTemplateFile) : base(projectBuilder) {
            _projectTemplateFile = projectTemplateFile;
        }

        public override string ProjectTemplateFile { get { return _projectTemplateFile; } }

        protected override void AddFiles() {}
    }
}
