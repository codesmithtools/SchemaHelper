// Copyright (c) CodeSmith Tools, LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using CodeSmith.Engine;
using CodeSmith.SchemaHelper;
using Configuration = CodeSmith.SchemaHelper.Configuration;

namespace Generator.QuickStart.Base {
    public class BaseTemplate : CodeTemplate {
        #region Constructor(s)

        public BaseTemplate() {
            ResolveTargetLanguage();
            TemplateContext = new Dictionary<string, string>();
        }

        #endregion

        #region Public Properties

        [Browsable(false)]
        public Dictionary<string, string> TemplateContext { get; set; }

        //[Editor("System.Windows.Forms.Design.FileNameEditor, System.Design, Version=1.0.5000.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(System.Windows.Forms.Design.FileNameEditor))]
        //[Category("1. DataSource")]
        //[Description("The full path to the edmx file.")]
        //public string EdmxFile { get; set; }

        #endregion

        #region Private Method(s)

        private void ResolveTargetLanguage() {
            if (CodeTemplateInfo != null)
                Configuration.Instance.TargetLanguage = CodeTemplateInfo.TargetLanguage.ToUpper() == "VB" ? Language.VB : Language.CSharp;
        }

        #endregion

        #region Public Method(s)

        public virtual void RegisterReferences() {
            RegisterReference("System.Configuration");
            //RegisterReference(Path.Combine(CodeTemplateInfo.DirectoryName, @"..\..\..\Framework\NetTiers.Build\netTiers.Core.dll"));
        }

        /// <summary>
        /// </summary>
        public virtual void Generate() {
            throw new NotImplementedException();
        }

        ///// <summary>
        ///// Returns the prefix for the DataRepository class based on the Owner
        ///// </summary>
        ///// <param name="owner"></param>
        ///// <returns></returns>
        //public virtual string GetRepositoryPrefix(string owner)
        //{
        //    return String.IsNullOrEmpty(owner) ? "Data" : owner;
        //}

        #endregion
    }
}
