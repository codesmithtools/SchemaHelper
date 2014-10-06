// Copyright (c) CodeSmith Tools, LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace CodeSmith.SchemaHelper {
    /// <summary>
    /// Helps determine the Access Modifier of Interfaces, Enumerations, Classes, Structs, Properties and Members.
    /// Documentation: http://msdn.microsoft.com/en-us/library/ms173121.aspx
    /// TODO: Add support for checking member types.
    /// TODO: Add better support for checking dissallowed types and unit tests.
    /// TODO: Add checks to see if the new Accessibility keyword is needed.
    /// TODO: Add Support for VB.NET
    /// </summary>
    public static class AccessibilityHelper {
        #region Members

        private static readonly Dictionary<string, int> AccessModifierRank = new Dictionary<string, int> {
            { AccessibilityConstants.Public, 1 },
            { AccessibilityConstants.Protected, 2 },
            { AccessibilityConstants.ProtectedInternal, 3 },
            { AccessibilityConstants.Internal, 4 },
            { AccessibilityConstants.Private, 5 }
        };

        #endregion

        /// <summary>
        /// Resolves the Accessibility for a Enumeration.
        /// Valid modifiers: public, protected, internal, private, new
        /// Documentation: http://msdn.microsoft.com/en-us/library/wxh6fsc7(v=VS.100).aspx,
        /// http://msdn.microsoft.com/en-us/library/sbbt4032(VS.71).aspx
        /// Note: Enumeration members are always public, and no access modifiers can be applied.
        /// </summary>
        /// <param name="modifier"></param>
        /// <param name="defaultAccessModifier"></param>
        /// <param name="useDefaultAccessModifier"></param>
        /// <param name="isNested"></param>
        public static string EnumerationAccessibility(string modifier, string defaultAccessModifier = AccessibilityConstants.Internal, bool useDefaultAccessModifier = false, bool isNested = false) {
            defaultAccessModifier = AccessModifierRank.ContainsKey(defaultAccessModifier) ? defaultAccessModifier : AccessibilityConstants.Internal;
            return BuildAccessModifier(modifier, defaultAccessModifier, useDefaultAccessModifier, isNested);
        }

        /// <summary>
        /// Resolves the Accessibility for a Interface.
        /// Valid modifiers: public, protected, internal, private, new
        /// Documentation: http://msdn.microsoft.com/en-us/library/wxh6fsc7(v=VS.100).aspx,
        /// http://msdn.microsoft.com/en-us/library/sbbt4032(VS.71).aspx
        /// Note: Interfaces declared directly within a namespace can be declared as public or internal and, just like classes and
        /// structs,
        /// interfaces default to internal access. Interface members are always public because the purpose of an interface is to
        /// enable other types to access a class or struct. No access modifiers can be applied to interface members.
        /// Enumeration members are always public, and no access modifiers can be applied.
        /// </summary>
        /// <param name="modifier"></param>
        /// <param name="defaultAccessModifier"></param>
        /// <param name="useDefaultAccessModifier"></param>
        /// <param name="isNested"></param>
        public static string InterfaceAccessibility(string modifier, string defaultAccessModifier = AccessibilityConstants.Internal, bool useDefaultAccessModifier = false, bool isNested = false) {
            defaultAccessModifier = AccessModifierRank.ContainsKey(defaultAccessModifier) ? defaultAccessModifier : AccessibilityConstants.Internal;
            return BuildAccessModifier(modifier, defaultAccessModifier, useDefaultAccessModifier, isNested);
        }

        /// <summary>
        /// Resolves the Accessibility for a Struct.
        /// Valid Access Modifiers: public, protected, internal, private, new
        /// Documentation: http://msdn.microsoft.com/en-us/library/wxh6fsc7(v=VS.100).aspx,
        /// http://msdn.microsoft.com/en-us/library/sbbt4032(VS.71).aspx
        /// Notes: Struct that are declared directly within a namespace (in other words, that are not nested within other classes
        /// or structs) can be either public or internal.
        /// Internal is the default if no access modifier is specified.
        /// Struct members, including nested classes and structs, can be declared as public, internal, or private.
        /// Class members, including nested classes and structs, can be public, protected internal, protected, internal, or
        /// private.
        /// The access level for class members and struct members, including nested classes and structs, is private by default.
        /// Private nested types are not accessible from outside the containing type.
        /// </summary>
        /// <param name="modifier"></param>
        /// <param name="defaultAccessModifier"></param>
        /// <param name="useDefaultAccessModifier"></param>
        /// <param name="isNested"></param>
        /// <returns></returns>
        public static string StructAccessibility(string modifier, string defaultAccessModifier = AccessibilityConstants.Internal, bool useDefaultAccessModifier = false, bool isNested = false) {
            defaultAccessModifier = AccessModifierRank.ContainsKey(defaultAccessModifier) ? defaultAccessModifier : AccessibilityConstants.Internal;
            return BuildAccessModifier(modifier, defaultAccessModifier, useDefaultAccessModifier, isNested);
        }

        /// <summary>
        /// Resolves the Accessibility for a Class.
        /// Valid Access Modifiers: public, protected, internal, private, new
        /// Documentation: http://msdn.microsoft.com/en-us/library/wxh6fsc7(v=VS.100).aspx,
        /// http://msdn.microsoft.com/en-us/library/sbbt4032(VS.71).aspx
        /// Notes: Classes that are declared directly within a namespace (in other words, that are not nested within other classes
        /// or structs) can be either public or internal.
        /// Internal is the default if no access modifier is specified.
        /// Derived classes cannot have greater accessibility than their base types.
        /// In other words, you cannot have a public class B that derives from an internal class A. If this were allowed,
        /// it would have the effect of making A public, because all protected or internal members of A are accessible from the
        /// derived class.
        /// Class members, including nested classes and structs, can be public, protected internal, protected, internal, or
        /// private.
        /// The access level for class members and struct members, including nested classes and structs, is private by default.
        /// Private nested types are not accessible from outside the containing type.
        /// </summary>
        /// <param name="modifier"></param>
        /// <param name="baseClassModifier"></param>
        /// <param name="defaultAccessModifier"></param>
        /// <param name="useDefaultAccessModifier"></param>
        /// <param name="isNested"></param>
        /// <returns></returns>
        public static string ClassAccessibility(string modifier, string defaultAccessModifier = AccessibilityConstants.Internal, bool useDefaultAccessModifier = false, string baseClassModifier = null, bool isNested = false) {
            defaultAccessModifier = AccessModifierRank.ContainsKey(defaultAccessModifier) ? defaultAccessModifier : AccessibilityConstants.Internal;
            return BuildAccessModifier(modifier, defaultAccessModifier, useDefaultAccessModifier, isNested, baseClassModifier);
        }

        /// <summary>
        /// Resolves the Accessibility for a Property.
        /// Valid Access Modifiers: public, protected, protected internal, internal, private, new
        /// Documentation: http://msdn.microsoft.com/en-us/library/wxh6fsc7(v=VS.100).aspx,
        /// http://msdn.microsoft.com/en-us/library/sbbt4032(VS.71).aspx
        /// Note: Class members (including nested classes and structs) can be declared with any of the five types of access. Struct
        /// members cannot be
        /// declared as protected because structs do not support inheritance.
        /// Normally, the accessibility of a member is not greater than the accessibility of the type that contains it. However, a
        /// public member of an
        /// internal class might be accessible from outside the assembly if the member implements interface methods or overrides
        /// virtual methods
        /// that are defined in a public base class.
        /// The type of any member that is a field, property, or event must be at least as accessible as the member itself.
        /// Similarly,
        /// the return type and the parameter types of any member that is a method, indexer, or delegate must be at least as
        /// accessible as the member itself.
        /// For example, you cannot have a public method M that returns a class C unless C is also public. Likewise, you cannot
        /// have a protected property of
        /// type A if A is declared as private.
        /// User-defined operators must always be declared as public. For more information, see operator (C# Reference).
        /// Destructors cannot have accessibility modifiers.
        /// </summary>
        /// <param name="getterModifier"></param>
        /// <param name="setterModifier"></param>
        /// <param name="useDefaultAccessModifier"></param>
        /// <returns></returns>
        public static string PropertyAccessibility(ref string getterModifier, ref string setterModifier, bool useDefaultAccessModifier = false) {
            // todo: ADD support for detecting basetype accessibility.
            getterModifier = GetAccessibilityModifier(getterModifier, AccessibilityConstants.Private, useDefaultAccessModifier);
            int getterModifierRank = AccessModifierRank.ContainsKey(getterModifier) ? AccessModifierRank[getterModifier] : AccessModifierRank[AccessibilityConstants.Private];

            setterModifier = GetAccessibilityModifier(setterModifier, AccessibilityConstants.Private, useDefaultAccessModifier);
            int setterModifierRank = AccessModifierRank.ContainsKey(setterModifier) ? AccessModifierRank[setterModifier] : AccessModifierRank[AccessibilityConstants.Private];

            int propertyModifierRank = Math.Min(getterModifierRank, setterModifierRank);
            if (getterModifierRank == propertyModifierRank)
                getterModifier = String.Empty;
            if (setterModifierRank == propertyModifierRank)
                setterModifier = String.Empty;

            return AccessModifierRank.Where(m => m.Value == propertyModifierRank).Select(m => m.Key).First();
        }

        /// <summary>
        /// </summary>
        /// <param name="property"></param>
        /// <typeparam name="T"></typeparam>
        public static void UpdatePropertyAccessibility<T>(PropertyBase<T> property) where T : class {
            // Validate the properties Type Access.
            property.TypeAccess = GetAccessibilityModifier(property.TypeAccess, AccessibilityConstants.Public, true);

            property.GetterAccess = GetAccessibilityModifier(property.GetterAccess, AccessibilityConstants.Public, true);
            int getterModifierRank = AccessModifierRank.ContainsKey(property.GetterAccess) ? AccessModifierRank[property.GetterAccess] : AccessModifierRank[AccessibilityConstants.Private];

            property.SetterAccess = GetAccessibilityModifier(property.SetterAccess, AccessibilityConstants.Public, true);
            int setterModifierRank = AccessModifierRank.ContainsKey(property.SetterAccess) ? AccessModifierRank[property.SetterAccess] : AccessModifierRank[AccessibilityConstants.Private];

            int propertyModifierRank = Math.Min(getterModifierRank, setterModifierRank);
            if (getterModifierRank == propertyModifierRank)
                property.GetterAccess = String.Empty;

            if (setterModifierRank == propertyModifierRank)
                property.SetterAccess = String.Empty;

            // Set the type's rank based on the getter and setters modifier ranking.
            property.TypeAccess = AccessModifierRank.Where(m => m.Value == propertyModifierRank).Select(m => m.Key).First();
        }

        /// <summary>
        /// </summary>
        /// <param name="association"></param>
        /// <typeparam name="T"></typeparam>
        public static void UpdateAssociationAccessibility<T>(AssociationBase<T> association) where T : class {
            // Validate the properties Type Access.
            association.TypeAccess = GetAccessibilityModifier(association.TypeAccess, AccessibilityConstants.Public, true);

            association.GetterAccess = GetAccessibilityModifier(association.GetterAccess, AccessibilityConstants.Public, true);
            int getterModifierRank = AccessModifierRank.ContainsKey(association.GetterAccess) ? AccessModifierRank[association.GetterAccess] : AccessModifierRank[AccessibilityConstants.Private];

            association.SetterAccess = GetAccessibilityModifier(association.SetterAccess, AccessibilityConstants.Public, true);
            int setterModifierRank = AccessModifierRank.ContainsKey(association.SetterAccess) ? AccessModifierRank[association.SetterAccess] : AccessModifierRank[AccessibilityConstants.Private];

            int propertyModifierRank = Math.Min(getterModifierRank, setterModifierRank);
            if (getterModifierRank == propertyModifierRank)
                association.GetterAccess = String.Empty;

            if (setterModifierRank == propertyModifierRank)
                association.SetterAccess = String.Empty;

            // Set the type's rank based on the getter and setters modifier ranking.
            association.TypeAccess = AccessModifierRank.Where(m => m.Value == propertyModifierRank).Select(m => m.Key).First();
        }

        #region Helpers

        /// <summary>
        /// Builds the Access Modifier.
        /// </summary>
        /// <param name="modifier"></param>
        /// <param name="hasNewModifier"></param>
        /// <returns></returns>
        private static string BuildAccessModifier(string modifier, bool hasNewModifier) {
            if (String.IsNullOrEmpty(modifier))
                return hasNewModifier ? AccessibilityConstants.New : String.Empty;

            return hasNewModifier ? String.Format("{0} {1}", modifier, AccessibilityConstants.New) : modifier;
        }

        /// <summary>
        /// Resolves the access modifier for a class.
        /// Valid Access Modifiers: public, protected, internal, private, new
        /// Documentation: http://msdn.microsoft.com/en-us/library/wxh6fsc7(v=VS.100).aspx,
        /// http://msdn.microsoft.com/en-us/library/sbbt4032(VS.71).aspx
        /// Notes: Classes that are declared directly within a namespace (in other words, that are not nested within other classes
        /// or structs) can be either public or internal.
        /// Internal is the default if no access modifier is specified.
        /// Derived classes cannot have greater accessibility than their base types.
        /// In other words, you cannot have a public class B that derives from an internal class A. If this were allowed,
        /// it would have the effect of making A public, because all protected or internal members of A are accessible from the
        /// derived class.
        /// </summary>
        /// <param name="modifier"></param>
        /// <param name="baseTypeModifier"></param>
        /// <param name="useDefaultAccessModifier"></param>
        /// <param name="isNested"></param>
        /// <param name="defaultAccessModifier"></param>
        /// <returns></returns>
        private static string BuildAccessModifier(string modifier, string defaultAccessModifier, bool useDefaultAccessModifier, bool isNested, string baseTypeModifier = null) {
            bool hasNewModifier;
            modifier = GetAccessibilityModifier(modifier, out hasNewModifier, defaultAccessModifier, useDefaultAccessModifier);
            int classAccessModifierRank = AccessModifierRank.ContainsKey(modifier) ? AccessModifierRank[modifier] : AccessModifierRank[defaultAccessModifier];

            if (String.IsNullOrEmpty(baseTypeModifier)) {
                if (!isNested) {
                    if (classAccessModifierRank == AccessModifierRank[AccessibilityConstants.Public] || classAccessModifierRank == AccessModifierRank[AccessibilityConstants.Internal])
                        return modifier;

                    return useDefaultAccessModifier ? defaultAccessModifier : String.Empty;
                }

                return modifier;
            }

            baseTypeModifier = GetAccessibilityModifier(baseTypeModifier, defaultAccessModifier);
            int baseClassAccessModifierRank = AccessModifierRank.ContainsKey(baseTypeModifier) ? AccessModifierRank[baseTypeModifier] : AccessModifierRank[defaultAccessModifier];

            modifier = AccessModifierRank.Where(m => m.Value == Math.Max(classAccessModifierRank, baseClassAccessModifierRank)).Select(m => m.Key).First();
            return BuildAccessModifier(modifier, hasNewModifier);
        }

        /// <summary>
        /// Parses the default Access Modifier from the passed in String.
        /// </summary>
        /// <param name="modifier"></param>
        /// <param name="defaultAccessModifier"></param>
        /// <param name="useDefaultAccessModifier"></param>
        /// <returns></returns>
        private static string GetAccessibilityModifier(string modifier, string defaultAccessModifier, bool useDefaultAccessModifier = false) {
            bool temp;
            return GetAccessibilityModifier(modifier, out temp, defaultAccessModifier, useDefaultAccessModifier);
        }

        /// <summary>
        /// Parses the default Access Modifier from the passed in String.
        /// </summary>
        /// <param name="modifier"></param>
        /// <param name="hasNewAccessModifier"></param>
        /// <param name="defaultAccessModifier"></param>
        /// <param name="useDefaultAccessModifier"></param>
        /// <returns></returns>
        private static string GetAccessibilityModifier(string modifier, out bool hasNewAccessModifier, string defaultAccessModifier, bool useDefaultAccessModifier = false) {
            hasNewAccessModifier = false;

            // TODO: Add validation logic to see if the modifier type is allowed.
            if (!String.IsNullOrEmpty(modifier)) {
                modifier = modifier.ToLower().Trim();

                // Check to see if the modifier has the new keyword.
                hasNewAccessModifier = modifier.Contains(AccessibilityConstants.New);
                if (hasNewAccessModifier)
                    modifier = modifier.Replace(AccessibilityConstants.New, String.Empty).Trim();

                switch (modifier) {
                    case AccessibilityConstants.Protected:
                        return AccessibilityConstants.Protected;
                    case AccessibilityConstants.ProtectedInternal:
                        return AccessibilityConstants.ProtectedInternal;
                    case AccessibilityConstants.Internal:
                        return AccessibilityConstants.Internal;
                    case AccessibilityConstants.Private:
                        return AccessibilityConstants.Private;
                    case AccessibilityConstants.Public:
                        return AccessibilityConstants.Public;
                }
            }

            return useDefaultAccessModifier ? defaultAccessModifier : String.Empty;
        }

        #endregion
    }
}
