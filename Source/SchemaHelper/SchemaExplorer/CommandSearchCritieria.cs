// Copyright (c) CodeSmith Tools, LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using SchemaExplorer;

namespace CodeSmith.SchemaHelper {
    internal class CommandSearchCritieria : SearchCriteria {
        private readonly bool _isValid = true;

        public CommandSearchCritieria(CommandEntity entity) : base(SearchCriteriaType.Command) {
            try {
                if (entity.EntitySource.Parameters.Count <= 0)
                    return;

                foreach (ParameterSchema parameter in entity.EntitySource.Parameters) {
                    if (parameter == null)
                        continue;

                    if (parameter.Name.Contains("RETURN_VALUE"))
                        continue;

                    Properties.Add(new CommandParameter(parameter, entity));
                }
            } catch (NotSupportedException) {
                string message = String.Format("This provider does not support Commands. Please disable the generation of commands by setting IncludeFunctions to false.");
                Trace.WriteLine(message);
                Debug.WriteLine(message);
            } catch (Exception ex) {
                string message = String.Format("Unable to load Command Search Criteria for '{0}'. Exception: {1}", entity.EntityKeyName, ex.Message);
                Trace.WriteLine(message);
                Debug.WriteLine(message);
                _isValid = false;
            }
        }

        protected override string GetMethodName(bool isRemote) {
            if (Properties.Count > 0)
                return base.GetMethodName(isRemote);

            return "GetResult";
        }

        public override string Key {
            get {
                if (!_isValid)
                    return null;

                if (Properties.Count > 0)
                    return base.Key;

                return MethodName;
            }
        }
    }
}
