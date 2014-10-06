// Copyright (c) CodeSmith Tools, LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using Ionic.Zip;
using Microsoft.Build.BuildEngine;

namespace Generator.QuickStart {
    public class WebApplicationCreator : ProjectCreator {
        public WebApplicationCreator(ProjectBuilderSettings projectBuilder) : base(projectBuilder) {}

        public override string ProjectTemplateFile { get { return "WebApplication.zip"; } }

        protected override void AddFiles() {
            if (!ProjectBuilder.IncludeDataServices)
                return;

            string directoryName = ProjectDirectory;
            string path = Path.Combine(ProjectBuilder.ZipFileFolder, "DataServiceApplication.zip");

            using (var zip = new ZipFile(path))
                zip.ExtractAll(directoryName, ExtractExistingFileAction.DoNotOverwrite);

            string dataService = ProjectBuilder.DatabaseName + "DataService.svc";
            string dataServiceClass = ProjectBuilder.DatabaseName + "DataService.svc." + ProjectBuilder.LanguageAppendage;

            string dataServicePath = Path.Combine(directoryName, dataService);
            string dataServiceClassPath = Path.Combine(directoryName, dataServiceClass);

            File.Move(Path.Combine(directoryName, "DataService.svc"), dataServicePath);

            File.Move(Path.Combine(directoryName, "DataService.svc." + ProjectBuilder.LanguageAppendage), dataServiceClassPath);

            Project project = GetProject();
            if (project == null)
                return;

            BuildItem serviceItem = project.AddNewItem("Content", dataService);
            BuildItem serviceClass = project.AddNewItem("Compile", dataServiceClass);
            serviceClass.SetMetadata("DependentUpon", dataService);

            project.Save(ProjectFile.FullName);

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
