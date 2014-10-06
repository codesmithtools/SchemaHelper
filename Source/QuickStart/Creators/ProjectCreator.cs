// Copyright (c) CodeSmith Tools, LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using CodeSmith.SchemaHelper;
using Ionic.Zip;
using Microsoft.Build.BuildEngine;

namespace Generator.QuickStart {
    public abstract class ProjectCreator {
        protected ProjectBuilderSettings ProjectBuilder { get; private set; }

        public abstract string ProjectTemplateFile { get; }

        public string CspFileName { get; set; }
        public string ProjectName { get; private set; }
        public string ProjectDirectory { get; protected set; }
        public FileInfo ProjectFile { get; protected set; }
        public SolutionItem SolutionItem { get; protected set; }
        public string ConnectionStringName { get; protected set; }
        public string ConnectionStringFormat { get; protected set; }
        public string ConnectionStringProvider { get; protected set; }

        public ProjectCreator(ProjectBuilderSettings projectBuilder) {
            ProjectBuilder = projectBuilder;
            TemplateDirectory = @"..\Templates\";
        }

        public virtual void CreateProject(string projectName) {
            CreateProject(projectName, new SolutionItem[] { });
        }

        public virtual void CreateProject(string projectName, params SolutionItem[] dependancies) {
            CreateProject(projectName, null, null, dependancies);
        }

        public virtual void CreateProject(string projectName, string connectionStringName, string connectionStringFormat, params SolutionItem[] dependancies) {
            ProjectName = projectName;
            ConnectionStringName = connectionStringName;
            ConnectionStringFormat = connectionStringFormat;

            Initialize(projectName, dependancies);

            //1) extract project template
            ExtractTemplate();

            //2) rename project file
            RenameProject();

            //3) add quick start files
            AddFiles();

            //4) add a csp file to the project
            AddCspFile();

            //5) replace project variables
            ReplaceVariables();

            //6) add dependencies
            if (dependancies != null && dependancies.Length > 0)
                AddProjectReferences(dependancies);

            AddReferences();
        }

        protected virtual void Initialize(string projectName, IEnumerable<SolutionItem> projectReferences) {
            string fileName = String.Format("{0}.{1}proj", projectName, ProjectBuilder.LanguageAppendage);
            ProjectDirectory = Path.Combine(ProjectBuilder.Location, projectName);
            ProjectFile = new FileInfo(Path.Combine(ProjectDirectory, fileName));
            SolutionItem = new SolutionItem(projectName, ProjectFile.FullName, ProjectBuilder.Language, false, projectReferences);
        }

        protected virtual void ExtractTemplate() {
            string zipPath = Path.Combine(ProjectBuilder.ZipFileFolder, ProjectTemplateFile);
            if (!File.Exists(zipPath))
                throw new ApplicationException("Invalid Path: " + zipPath);

            using (var zipFile = new ZipFile(zipPath))
                zipFile.ExtractAll(ProjectDirectory, ExtractExistingFileAction.DoNotOverwrite);
        }

        protected virtual void RenameProject() {
            if (ProjectFile == null)
                return;

            string extenstion = String.Format(".{0}proj", ProjectBuilder.LanguageAppendage);

            string original = Path.ChangeExtension(Path.GetFileName(ProjectTemplateFile), extenstion);

            var originalFile = new FileInfo(Path.Combine(ProjectDirectory, original));
            originalFile.MoveTo(ProjectFile.FullName);
        }

        protected abstract void AddFiles();

        protected virtual void AddCspFile() {
            if (String.IsNullOrWhiteSpace(CspFileName))
                return;

            string templateCspFile = Path.Combine(ProjectBuilder.ZipFileRoot, CspFileName);
            string content = File.ReadAllText(Path.Combine(ProjectBuilder.WorkingDirectory, templateCspFile));

            content = ReplaceFileVariables(content, true);

            if (ProjectBuilder.Language == Language.VB)
                content = content.Replace("\\CSharp\\", "\\VisualBasic\\");

            if (String.Equals(CspFileName, "WebAPI.csp", StringComparison.OrdinalIgnoreCase))
                CspFileName = "Web.csp";

            File.WriteAllText(Path.Combine(ProjectDirectory, CspFileName), content);

            AddNewItem("Generate", CspFileName);
        }

        public string TemplateDirectory { get; set; }

        private string TemplatePath {
            get {
                string templatePath = (ProjectBuilder.CopyTemplatesToFolder) ? TemplateDirectory : ProjectBuilder.WorkingDirectory;

                templatePath = CodeSmith.Core.IO.PathHelper.RelativePathTo(ProjectDirectory, templatePath);

                if (!templatePath.EndsWith(@"\"))
                    templatePath += @"\";

                return templatePath;
            }
        }

        protected void AddNewItem(string itemName, string fileName) {
            Project project = GetProject();
            if (project == null)
                return;

            BuildItem buildItem = project.AddNewItem(itemName, fileName);
            project.Save(ProjectFile.FullName);
        }

        protected void AddProjectReference(SolutionItem solutionItem) {
            AddProjectReferences(new[] { solutionItem });
        }

        protected void AddProjectReferences(IEnumerable<SolutionItem> solutionItems) {
            Project project = GetProject();
            if (project == null)
                return;

            foreach (SolutionItem solutionItem in solutionItems) {
                string path = CodeSmith.Core.IO.PathHelper.RelativePathTo(ProjectDirectory, solutionItem.Path);
                BuildItem buildItem = project.AddNewItem("ProjectReference", path);
                buildItem.SetMetadata("Project", solutionItem.Guid.ToString("B"));
                buildItem.SetMetadata("Name", solutionItem.Name);
            }

            project.Save(ProjectFile.FullName);
        }

        protected virtual void ReplaceVariables() {
            IEnumerable<string> files = GetVariableFiles();

            foreach (string f in files) {
                string content = File.ReadAllText(f);
                content = ReplaceFileVariables(content, false);
                File.WriteAllText(f, content);
            }
        }

        protected virtual IEnumerable<string> GetVariableFiles() {
            IEnumerable<string> files = PathHelper.GetFiles(ProjectDirectory, "*.*proj", "*.config", "*." + ProjectBuilder.LanguageAppendage, "*.as*x", "*.svc", "*.master");

            return files;
        }

        protected virtual string ReplaceFileVariables(string content, bool isCSP) {
            string connectionString = ProjectBuilder.SourceDatabase != null ? ProjectBuilder.SourceDatabase.ConnectionString : String.Empty;
            if (ProjectBuilder.EnsureMultipleResultSets)
                connectionString = QuickStartUtils.EnsureMultipleResultSets(connectionString);

            if (String.IsNullOrEmpty(ConnectionStringName))
                ConnectionStringName = "$databaseName$ConnectionString";
            if (String.IsNullOrEmpty(ConnectionStringFormat))
                ConnectionStringFormat = "{0}";
            if (String.IsNullOrEmpty(ConnectionStringProvider))
                ConnectionStringProvider = ProjectBuilder.ConnectionStringProvider;

            string connectionStringProviderNameAttribute = !String.IsNullOrEmpty(ProjectBuilder.ConnectionStringProviderNameAttribute) ? String.Format(" providerName=\"{0}\"", ProjectBuilder.ConnectionStringProviderNameAttribute) : String.Empty;

            string projectReferencePath = String.Empty;
            if (SolutionItem.ProjectReferences.Count(p => !p.Website) > 0)
                projectReferencePath = CodeSmith.Core.IO.PathHelper.RelativePathTo(ProjectDirectory, new FileInfo(SolutionItem.ProjectReferences.First(p => !p.Website).Path).DirectoryName);
            else if (SolutionItem.ProjectReferences.Count > 0)
                projectReferencePath = CodeSmith.Core.IO.PathHelper.RelativePathTo(ProjectDirectory, new FileInfo(SolutionItem.ProjectReferences.First().Path).DirectoryName);

            return content.Replace("$connectionStringName$", ConnectionStringName).Replace("$connectionString$", isCSP ? connectionString : String.Format(ConnectionStringFormat, connectionString)).Replace("$connectionStringProvider$", ConnectionStringProvider).Replace("$connectionStringProviderNameAttribute$", connectionStringProviderNameAttribute).Replace("$databaseProvider$", ProjectBuilder.DatabaseProvider).Replace("$projectname$", ProjectName).Replace("$safeprojectname$", ProjectName).Replace("$DataProjectDirectory$", projectReferencePath).Replace("$rootnamespace$", ProjectName).Replace("$guid1$", SolutionItem.Guid.ToString("B")).Replace("$assemblyGuid$", Guid.NewGuid().ToString()).Replace("$registeredorganization$", "CodeSmith Tools, LLC").Replace("$year$", DateTime.Now.Year.ToString(CultureInfo.InvariantCulture)).Replace("$targetframeworkversion$", ProjectBuilder.FrameworkString).Replace("$frameworkEnum$", GetFrameworkVersionString()).Replace("$datacontext$", ProjectBuilder.DataContextName).Replace("$entityNamespace$", ProjectBuilder.DataProjectName).Replace("$databaseName$", ProjectBuilder.DatabaseName).Replace("$baseNamespace$", ProjectBuilder.BaseNamespace).Replace("$language$", ProjectBuilder.Language == Language.CSharp ? "CSharp" : "VisualBasic").Replace("$template.path$", TemplatePath).Replace("$includeFunctions$", ProjectBuilder.IncludeFunctions.ToString()).Replace("$includeViews$", ProjectBuilder.IncludeViews.ToString());
        }

        private string GetFrameworkVersionString() {
            switch (ProjectBuilder.FrameworkVersion) {
                case FrameworkVersion.v45:
                    return "v45";

                case FrameworkVersion.v40:
                    return "v40";

                default:
                    return "v35_SP1";
            }
        }

        protected virtual void AddReferences() {
            Project project = GetProject();
            if (project == null)
                return;

            //var item = project.AddNewItem("Reference", "CodeSmith.Data");

            //string fullPath = Path.Combine(ProjectBuilder.WorkingDirectory, "Common");
            //fullPath = Path.Combine(fullPath, ProjectBuilder.FrameworkFolder);
            //fullPath = Path.Combine(fullPath, "CodeSmith.Data.dll");

            //string relativePath = CodeSmith.Engine.Utility.PathUtil.RelativePathTo(ProjectDirectory, fullPath);

            //item.SetMetadata("HintPath", relativePath);

            project.Save(ProjectFile.FullName);
        }

        protected Project GetProject() {
            if (ProjectFile == null)
                return null;

            var project = new Project();
            project.Load(ProjectFile.FullName, ProjectLoadSettings.IgnoreMissingImports);
            return project;
        }
    }
}
