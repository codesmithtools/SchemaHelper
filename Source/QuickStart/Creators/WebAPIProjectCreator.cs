// Copyright (c) CodeSmith Tools, LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using Ionic.Zip;

namespace Generator.QuickStart {
    public class WebAPIProjectCreator : ProjectCreator {
        public WebAPIProjectCreator(ProjectBuilderSettings projectBuilder) : base(projectBuilder) {}

        public override string ProjectTemplateFile { get { return "WebAPIProject.zip"; } }

        protected override void AddFiles() {
            string path = Path.Combine(Path.Combine(ProjectBuilder.WorkingDirectory, ProjectBuilder.ZipFileRoot), ".nuget.zip");

            using (var zip = new ZipFile(path))
                zip.ExtractAll(Path.Combine(ProjectBuilder.Location, ".nuget"), ExtractExistingFileAction.DoNotOverwrite);
        }
    }
}
