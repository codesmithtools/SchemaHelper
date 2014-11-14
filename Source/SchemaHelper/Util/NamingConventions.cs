// Copyright (c) CodeSmith Tools, LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Text.RegularExpressions;
using CodeSmith.Engine;

namespace CodeSmith.SchemaHelper.Util {
    /// <summary>
    /// </summary>
    public static class NamingConventions {
        private static readonly Regex _cleanNumberPrefix = new Regex(@"^\d+", RegexOptions.Compiled);
        private static readonly Regex _cleanIdRegex = new Regex(@"(_ID|_id|_Id|\.ID|\.id|\.Id|ID|Id)$", RegexOptions.Compiled);
        private static MapCollection _keywordRenameAlias;
        private static MapCollection _systemTypeEscape;

        /// <summary>
        /// </summary>
        /// <param name="value"></param>
        /// <param name="useKeywordRenameAlias"></param>
        /// <param name="preserveNaming"></param>
        /// <returns></returns>
        public static string PrivateMemberVariableName(string value, bool useKeywordRenameAlias = false, bool preserveNaming = false) {
            if (String.IsNullOrEmpty(value))
                return String.Empty;

            value = CleanName(value.Trim(), useKeywordRenameAlias, preserveNaming);

            return EscapeSystemType(String.Concat(Configuration.Instance.PrivateMemberPrefix, preserveNaming ? value : StringUtil.ToCamelCase(value)));
        }

        /// <summary>
        /// </summary>
        /// <param name="value"></param>
        /// <param name="useKeywordRenameAlias"></param>
        /// <param name="preserveNaming"></param>
        /// <returns></returns>
        public static string VariableName(string value, bool useKeywordRenameAlias = true, bool preserveNaming = false) {
            if (String.IsNullOrEmpty(value))
                return String.Empty;

            value = CleanName(value.Trim(), useKeywordRenameAlias, preserveNaming);

            // Replace system keywords..
            return EscapeSystemType(preserveNaming ? value : StringUtil.ToCamelCase(value));
        }

        /// <summary>
        /// </summary>
        /// <param name="value"></param>
        /// <param name="useKeywordRenameAlias"></param>
        /// <param name="preserveNaming"></param>
        /// <returns></returns>
        public static string PropertyName(string value, bool useKeywordRenameAlias = true, bool preserveNaming = false) {
            if (String.IsNullOrEmpty(value))
                return String.Empty;

            value = CleanName(value.Trim(), useKeywordRenameAlias, preserveNaming);

            // TODO: this is called twice because if you have a column named a_b_c_myName it will be returned as abcMyName. This will be fixed in the next Release of CodeSmith Generator.
            return EscapeSystemType(preserveNaming ? value : StringUtil.ToPascalCase(StringUtil.ToPascalCase(value)));
        }

        /// <summary>
        /// Validates the entity name is not the same name as the Property.
        /// </summary>
        /// <returns></returns>
        public static string ValidateName(IProperty property) {
            object value;
            string name = (property.ExtendedProperties.TryGetValue(Configuration.Instance.AliasExtendedProperty, out value)) ? value.ToString() : property.KeyName; // E.G. Column Name

            string result = PropertyName(name, preserveNaming: Configuration.Instance.NamingProperty.PropertyNaming == PropertyNaming.Preserve);

            // Check to see if the returned PropertyName is null or empty. If it is, then the column only contained punctuation or non standard characters like .,~.
            // These characters are stripped by the StringUtil.
            name = !String.IsNullOrEmpty(result) ? result : Configuration.Instance.SingularMemberSuffix;

            string className = property.Entity.Name;
            // Check to see if the column name is prefixed with the table name.
            if (Configuration.Instance.NamingProperty.PropertyNaming == PropertyNaming.NormalizeRemovePrefix) {
                // Also make sure that the stripped name is at least two characters (E.G CategoryID --> ID).
                if (name.StartsWith(className, StringComparison.CurrentCultureIgnoreCase) && name.Length > className.Length + 1)
                    name = PropertyName(name.Remove(0, className.Length), preserveNaming: Configuration.Instance.NamingProperty.PropertyNaming == PropertyNaming.Preserve);
            }

            if (String.Compare(className, name, true) == 0)
                name = String.Concat(name, Configuration.Instance.SingularMemberSuffix);

            return name;
        }

        /// <summary>
        /// Validates the entity name.
        /// </summary>
        /// <returns>Returns properly formatted class name based off the name of the table.</returns>
        public static string ValidateName(IEntity entity) {
            bool useKeywordRenameAlias = false;
            string className = entity.EntityKeyName;

            object value;
            if (entity.ExtendedProperties.TryGetValue(Configuration.Instance.AliasExtendedProperty, out value))
                className = value.ToString();
            else if (KeywordRenameAlias.ContainsKey(className))
                className = KeywordRenameAlias[className];
            else {
                useKeywordRenameAlias = true;

                if (!String.IsNullOrEmpty(Configuration.Instance.TablePrefix) && className.StartsWith(Configuration.Instance.TablePrefix))
                    className = className.Remove(0, Configuration.Instance.TablePrefix.Length);

                if (Configuration.Instance.NamingProperty.EntityNaming == EntityNaming.Plural)
                    className = StringUtil.ToPascalCase(StringUtil.ToPlural(className));
                else if (Configuration.Instance.NamingProperty.EntityNaming == EntityNaming.Singular)
                    className = StringUtil.ToSingular(className);
            }

            return PropertyName(className, useKeywordRenameAlias, Configuration.Instance.NamingProperty.EntityNaming == EntityNaming.Preserve);
        }

        /// <summary>
        /// Removes any instance of ID from a string.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string RemoveId(string value) {
            string result = _cleanIdRegex.Replace(value, String.Empty);
            return !String.IsNullOrEmpty(result) ? result : value;
        }

        /// <summary>
        /// </summary>
        /// <param name="schema"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string GetSafeName(string schema, string name) {
            return String.IsNullOrEmpty(schema) ? GetSafeName(name) : String.Concat(GetSafeName(schema), ".", GetSafeName(name));
        }

        /// <summary>
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string GetSafeName(string name) {
            return String.Concat(Configuration.Instance.SafeNamePrefix, name, Configuration.Instance.SafeNameSuffix);
        }

        public static string CleanName(string value, bool useKeywordRenameAlias, bool preserveNaming = false) {
            if (String.IsNullOrEmpty(value))
                return String.Empty;

            if (preserveNaming)
                value = CleanEscapeSystemType(value);

            // Lookup in mapping file for overrides.
            if (useKeywordRenameAlias && KeywordRenameAlias.ContainsKey(value)) {
                // TODO: Add ToSpacedWords Support and check each value.
                return KeywordRenameAlias[value, value];
            }

            if (Configuration.Instance.CleanExpressions.Count > 0) {
                foreach (Regex regex in Configuration.Instance.CleanExpressions) {
                    if (regex.IsMatch(value)) {
                        value = regex.Replace(value, String.Empty);
                        break;
                    }
                }
            }

            string result = _cleanNumberPrefix.Replace(value, String.Empty, 1).Trim();
            // If a column's name is 123, then this will ensure that the name is valid by making the name Member123.
            if (String.IsNullOrEmpty(result))
                result = String.Concat(Configuration.Instance.SingularMemberSuffix, value);
            else if (result.Equals("."))
                result = "Period";
            else if (result.Equals("'"))
                result = "Apostrophe";
            else if (result.Equals("_"))
                result = "Underscore";

            // If the result is empty then the name only contained numbers. If this is true then prefix the column.
            return !String.IsNullOrEmpty(result) ? result : String.Concat(Configuration.Instance.SingularMemberSuffix, value);
        }

        /// <summary>
        /// Removes the prefix and suffix of any escaped System Type.
        /// </summary>
        public static string CleanEscapeSystemType(string value) {
            // A preserved name might be escaped.
            if ((value.StartsWith("@") || value.StartsWith("[")))
                value = value.Substring(1);

            // A preserved name might be escaped.
            if (value.EndsWith("]"))
                value = value.Substring(0, value.Length - 1);

            return value;
        }

        private static string EscapeSystemType(string name) {
            string value;
            return SystemTypeEscape.TryGetValue(name, out value) ? value : name;
        }

        /// <summary>
        /// Returns the DBTypeToSystemTypeEscape MapCollection.
        /// </summary>
        /// <returns>Returns the correct SystemTypeEscape MapCollection.</returns>
        private static MapCollection SystemTypeEscape {
            get {
                string mapFileName = Configuration.Instance.TargetLanguage == Language.CSharp ? "CSharpKeywordEscape" : "VBKeywordEscape";

                if (_systemTypeEscape == null) {
                    string path;
                    Map.TryResolvePath(mapFileName, String.Empty, out path);
                    _systemTypeEscape = File.Exists(path) ? Map.Load(path) : new MapCollection();
                }

                return _systemTypeEscape;
            }
        }

        /// <summary>
        /// Returns the DBTypeToSystemTypeEscape MapCollection.
        /// </summary>
        /// <returns>Returns the correct SystemTypeEscape MapCollection.</returns>
        private static MapCollection KeywordRenameAlias {
            get {
                if (_keywordRenameAlias == null) {
                    string path;
                    if (!Map.TryResolvePath("KeywordRenameAlias", String.Empty, out path) && TemplateContext.Current != null) {
                        // If the mapping file wasn't found in the maps folder than look it up in the common folder.
                        string baseDirectory = Path.GetFullPath(Path.Combine(TemplateContext.Current.RootCodeTemplate.CodeTemplateInfo.DirectoryName, @"..\..\Common"));
                        if (!Map.TryResolvePath("KeywordRenameAlias", baseDirectory, out path)) {
                            baseDirectory = Path.GetFullPath(Path.Combine(TemplateContext.Current.RootCodeTemplate.CodeTemplateInfo.DirectoryName, @"..\Common"));
                            if (!Map.TryResolvePath("KeywordRenameAlias", baseDirectory, out path)) {
                                baseDirectory = Path.Combine(TemplateContext.Current.RootCodeTemplate.CodeTemplateInfo.DirectoryName, "Common");
                                Map.TryResolvePath("KeywordRenameAlias", baseDirectory, out path);
                            }
                        }
                    }

                    // Prevents a NullReferenceException from occurring.
                    _keywordRenameAlias = File.Exists(path) ? Map.Load(path) : new MapCollection();
                }

                return _keywordRenameAlias;
            }
        }
    }
}
