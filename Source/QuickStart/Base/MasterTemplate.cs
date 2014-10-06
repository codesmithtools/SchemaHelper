// Copyright (c) CodeSmith Tools, LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Windows.Forms.Design;
using CodeSmith.SchemaHelper;

namespace Generator.QuickStart.Base {
    public class MasterTemplate : BaseTemplate {
        #region Private property(s)

        //private TableSchemaCollection _tables;
        //private TableSchemaCollection _lightTables;
        //private ViewSchemaCollection _viewSchemaCollection;
        //private StringCollection _ignoreExpressions;
        //private StringCollection _cleanExpressions;
        //private string _customProcedureNameFormat;

        private const string PropertyGroup1 = "1. DataSource";
        private const string PropertyGroup2 = "2. Solution";
        private const string PropertyGroup3 = "3. Misc. Options";

        #endregion

        #region Constructor(s)

        public MasterTemplate() {
            //CleanExpressions = new StringCollection();
            //IgnoreExpressions = new StringCollection();
        }

        #endregion

        #region 1. DataSource

        //[Category(PropertyGroup1)]
        //[Description("Source Tables")]
        //[Optional]
        //public TableSchemaCollection SourceTables
        //{
        //    get
        //    {
        //        return _tables;
        //    }
        //    set
        //    {
        //        if (value != null && _tables != value)
        //        {
        //            _tables = value;
        //        }
        //    }
        //}

        //[Category(PropertyGroup1)]
        //[Description("Source Views")]
        //[Optional]
        //public ViewSchemaCollection SourceViews
        //{
        //    get
        //    {
        //        return _viewSchemaCollection;
        //    }
        //    set
        //    {
        //        if (value != null && _viewSchemaCollection != value)
        //        {
        //            _viewSchemaCollection = value;
        //        }
        //    }
        //}

        //[Category(PropertyGroup1)]
        //[Description("List of regular expressions to clean table, view and column names.")]
        //[Optional]
        //[DefaultValue("^\\w+_")]
        //public StringCollection CleanExpressions
        //{
        //    get
        //    {
        //        return _cleanExpressions;
        //    }
        //    set
        //    {
        //        _cleanExpressions = value;

        //        Configuration.Instance.CleanExpressions = new List<Regex>();
        //        foreach (string clean in _cleanExpressions)
        //        {
        //            if (!String.IsNullOrEmpty(clean))
        //            {
        //                Configuration.Instance.CleanExpressions.Add(new Regex(clean, RegexOptions.IgnoreCase));
        //            }
        //        }
        //    }
        //}

        //[Category(PropertyGroup1)]
        //[Description("List of regular expressions to ignore tables when generating.")]
        //[Optional]
        //[DefaultValue("sysdiagrams$")]
        //public StringCollection IgnoreExpressions
        //{
        //    get
        //    {
        //        return _ignoreExpressions;
        //    }
        //    set
        //    {
        //        _ignoreExpressions = value;

        //        Configuration.Instance.IgnoreExpressions = new List<Regex>();
        //        foreach (string ignore in _ignoreExpressions)
        //        {
        //            if (!String.IsNullOrEmpty(ignore))
        //            {
        //                Configuration.Instance.IgnoreExpressions.Add(new Regex(ignore, RegexOptions.IgnoreCase));
        //            }
        //        }
        //    }
        //}

        #endregion

        #region 2. Solution

        [Editor(typeof(FolderNameEditor), typeof(UITypeEditor))]
        [Category(PropertyGroup2)]
        [Description("The path to the Solution location.")]
        [DefaultValue("")]
        public string BaseDirectory { get; set; }

        #endregion

        #region 3.  Generation Options

        //[Category(PropertyGroup3)]
        //[Description("The Prefix for matching custom stored procedures to a table.  Example would be: _{0}_")]
        //[Optional]
        //[DefaultValue("_{0}_")]
        //public string CustomProcedureNameFormat
        //{
        //    get
        //    {
        //        return _customProcedureNameFormat;
        //    }
        //    set
        //    {
        //        _customProcedureNameFormat = value;

        //        Configuration.Instance.CustomProcedureNameFormat = _customProcedureNameFormat;
        //    }
        //}

        #endregion

        #region Misc. Properties

        [Browsable(false)]
        public IEnumerable<IEntity> Entities { get; internal set; }

        [Browsable(false)]
        public IEnumerable<string> Owners { get; internal set; }

        #endregion

        #region Public Virtual Method(s)

        /// <summary>
        /// </summary>
        public virtual void Initialize() {
            //    if (CleanExpressions.Count == 0)
            //        CleanExpressions.Add("^\\w+_");

            //    if (IgnoreExpressions.Count == 0)
            //    {
            //        IgnoreExpressions.Add("sysdiagrams$");
            //        IgnoreExpressions.Add("^dbo.aspnet");
            //    }

            //    if (String.IsNullOrEmpty(Location) && SourceTables.Count > 0)
            //        Location = Path.Combine(
            //            CodeSmith.Engine.Configuration.Instance.CodeSmithTemplatesDirectory,
            //            Path.Combine("netTiers", SourceTables[0].Database.Name));

            //var em = new EntityManager(EdmxFile);
            //Entities = em.Entities;
            //Owners = em.OwnerList;
        }

        #endregion
    }
}
