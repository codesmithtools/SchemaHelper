// Copyright (c) CodeSmith Tools, LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Generator.QuickStart {
    public class MVCProjectCreator : ProjectCreator {
        public MVCProjectCreator(ProjectBuilderSettings projectBuilder) : base(projectBuilder) {}

        public override string ProjectTemplateFile { get { return "MvcProject.zip"; } }

        protected override void AddFiles() {}
    }
}
