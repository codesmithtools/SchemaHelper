// Copyright (c) CodeSmith Tools, LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace CodeSmith.SchemaHelper {
    public static class AccessibilityConstants {
        public const string Public = "public"; // Rank: 1
        //private const string PUBLIC_VB = "Public";          // Rank: 1
        public const string Protected = "protected"; // Rank: 2
        //private const string PROTECTED_VB = "Protected";    // Rank: 2
        public const string ProtectedInternal = "protected internal"; // Rank: 3
        //private const string PROTECTED_INTERNAL_VB = "Protected Friend";  // Rank: 3
        public const string Internal = "internal"; // Rank: 4
        //private const string INTERNAL_VB = "Friend";      // Rank: 4
        public const string Private = "private"; // Rank: 5
        //private const string PRIVATE_VB = "Private";        // Rank: 5
        public const string New = "new";
        //private const string NEW_VB = "New";
    }
}
