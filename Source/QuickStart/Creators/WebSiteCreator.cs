// Copyright (c) CodeSmith Tools, LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using Ionic.Zip;

namespace Generator.QuickStart {
    public class WebSiteCreator : ProjectCreator {
        private readonly bool _isEntityFramework;

        public WebSiteCreator(ProjectBuilderSettings projectBuilder) : this(projectBuilder, false) {}

        public WebSiteCreator(ProjectBuilderSettings projectBuilder, bool isEntityFramework) : base(projectBuilder) {
            _isEntityFramework = isEntityFramework;
        }

        public override string ProjectTemplateFile { get { return _isEntityFramework ? "EFWebSite.zip" : "WebSite.zip"; } }

        protected override void Initialize(string projectName, IEnumerable<SolutionItem> projectReferences) {
            string fileName = String.Format("{0}.{1}.webproj", projectName, ProjectBuilder.LanguageAppendage);
            ProjectDirectory = Path.Combine(ProjectBuilder.Location, projectName);
            ProjectFile = new FileInfo(Path.Combine(ProjectDirectory, fileName));
            SolutionItem = new SolutionItem(projectName, ProjectFile.FullName, ProjectBuilder.Language, true, projectReferences);
            ProjectFile = null;
        }

        protected override void AddFiles() {
            if (!ProjectBuilder.IncludeDataServices)
                return;

            string directoryName = ProjectDirectory;
            string path = Path.Combine(ProjectBuilder.ZipFileFolder, "DataServiceWebSite.zip");

            using (var zip = new ZipFile(path))
                zip.ExtractAll(directoryName, ExtractExistingFileAction.DoNotOverwrite);

            string dataService = ProjectBuilder.DatabaseName + "DataService.svc";
            string dataServiceClass = ProjectBuilder.DatabaseName + "DataService." + ProjectBuilder.LanguageAppendage;

            string dataServicePath = Path.Combine(directoryName, dataService);
            string dataServiceClassPath = Path.Combine(directoryName, "App_Code");
            if (!Directory.Exists(dataServiceClassPath))
                Directory.CreateDirectory(dataServiceClassPath);

            dataServiceClassPath = Path.Combine(dataServiceClassPath, dataServiceClass);

            File.Move(Path.Combine(directoryName, "DataService.svc"), dataServicePath);

            File.Move(Path.Combine(directoryName, "DataService." + ProjectBuilder.LanguageAppendage), dataServiceClassPath);

            // update vars
            string content = File.ReadAllText(dataServicePath);
            content = content.Replace("$safeitemname$", Path.GetFileNameWithoutExtension(dataService));
            File.WriteAllText(dataServicePath, content);

            content = File.ReadAllText(dataServiceClassPath);
            content = content.Replace("$safeitemname$", Path.GetFileNameWithoutExtension(dataService));
            File.WriteAllText(dataServiceClassPath, content);
        }

        protected override string ReplaceFileVariables(string content, bool isCSP) {
            return base.ReplaceFileVariables(content, isCSP).Replace("$entityNamespace$", ProjectBuilder.DataProjectName).Replace("$datacontext$", ProjectBuilder.DataContextName);
        }
    }
}
