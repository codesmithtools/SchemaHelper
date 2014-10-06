// Copyright (c) CodeSmith Tools, LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace CodeSmith.SchemaHelper.Util {
    /// <summary>
    /// This class aids in the resolving of Types from the CodeSmith Generator into .NET related types as well.
    /// </summary>
    public static class TypeHelper {
        public static string ResolveSystemType(string systemType, bool isNullable, bool canAppendNullable) {
            Type type = null;

            try {
                type = Type.GetType(systemType);
            } catch (Exception) {}

            if (type == null) {
                systemType = GetLanguageSpecificSystemType(systemType);
                bool appendNull = isNullable && canAppendNullable;
                return (appendNull) ? String.Format("{0}?", systemType) : systemType;
            }

            return ResolveSystemType(type, isNullable, canAppendNullable);
        }

        public static string ResolveSystemType(Type systemType, bool isNullable, bool canAppendNullable) {
            string result = GetLanguageSpecificSystemType(systemType.ToString());

            //if (systemType == typeof(XmlDocument))
            //    return "System.Xml.Linq.XElement";

            //if (systemType == typeof(byte[]))
            //   return "System.Data.Linq.Binary";

            if (Configuration.Instance.TargetLanguage == Language.VB)
                result = result.Replace("[]", "()");

            bool appendNull = isNullable && systemType.IsValueType && canAppendNullable;
            return (appendNull) ? String.Format("{0}?", result) : result;
        }

        private static string GetLanguageSpecificSystemType(string type) {
            if (String.IsNullOrEmpty(type) || Configuration.Instance.TargetLanguage == Language.VB)
                return type;

            // TODO: Convert System.String to String.
            return type;
        }
    }
}
