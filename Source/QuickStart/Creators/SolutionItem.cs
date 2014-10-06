// Copyright (c) CodeSmith Tools, LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using CodeSmith.SchemaHelper;

namespace Generator.QuickStart {
    public class SolutionItem {
        public SolutionItem(string name, string path, Language language) : this(name, path, language, false, null) {}

        public SolutionItem(string name, string path, Language language, bool website, IEnumerable<SolutionItem> projectReferences) {
            Guid = Guid.NewGuid();
            Name = name;
            Path = path;
            Language = language;
            Website = website;
            ProjectReferences = projectReferences != null ? new List<SolutionItem>(projectReferences) : new List<SolutionItem>();
        }

        public List<SolutionItem> ProjectReferences { get; set; }
        public bool Website { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }

        public Guid Guid { get; set; }

        public string GuidString { get { return Guid.ToString().ToUpper(); } }

        public Language Language { get; set; }
        public string LanguageGuidString { get { return (Language == Language.CSharp) ? "FAE04EC0-301F-11D3-BF4B-00C04F79EFBC" : "F184B08F-C81C-45F6-A57F-5ABD9991F28F"; } }
    }
}
