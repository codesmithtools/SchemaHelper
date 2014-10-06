// Copyright (c) CodeSmith Tools, LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;

namespace Generator.QuickStart {
    public static class PathHelper {
        public static IEnumerable<string> GetFiles(string directory, params string[] searchPatterns) {
            var files = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (string pattern in searchPatterns)
                files.UnionWith(Directory.GetFiles(directory, pattern, SearchOption.AllDirectories));

            return files;
        }
    }
}
