// Copyright (c) CodeSmith Tools, LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel;
using CodeSmith.Engine;
using CodeSmith.SchemaHelper;
using Generator.QuickStart.Base;
using SchemaExplorer;

namespace Generator.QuickStart {
    public class EntityCodeTemplate : BaseTemplate {
        #region Private property(s)

        private IEntity _entity;

        #endregion

        #region Constructor(s)

        public EntityCodeTemplate() {
            //CleanExpressions = new StringCollection();
        }

        #endregion

        #region Public Properties

        //#region 1. DataSource

        //[Category("1. DataSource")]
        //[Description("List of regular expressions to clean table, view and column names.")]
        //[Optional]
        //[DefaultValue("^\\w+_")]
        //public StringCollection CleanExpressions { get; set; }

        //#endregion

        #region 3. Entity Project

        [Category("2. Class")]
        [Description("The namespace for the entity project.")]
        public string EntityNamespace { get; set; }

        [Optional]
        [Browsable(false)]
        public TableSchema SourceTable { set { Entity = new TableEntity(value); } }

        [Optional]
        [Browsable(false)]
        public ViewSchema SourceView { set { Entity = new ViewEntity(value); } }

        [Optional]
        [Browsable(false)]
        public CommandSchema SourceCommand { set { Entity = new CommandEntity(value); } }

        [Browsable(false)]
        public IEntity Entity {
            get { return _entity; }
            set {
                if (value != null && _entity != value) {
                    _entity = value;
                    OnEntityChanged();
                }
            }
        }

        //[Browsable(false)]
        //public string ChildBusinessClassName
        //{
        //    get
        //    {
        //        if (Entity.Name.EndsWith("List"))
        //            return Entity.Name.Replace("List", "");

        //        return Entity.Name;
        //    }
        //}

        ///// <summary>
        ///// 
        ///// </summary>
        //[Browsable(false)]
        //public bool IsHeavyEntity
        //{
        //    get
        //    {
        //        return true;
        //    }
        //}

        #endregion

        #endregion

        #region Public Virtual Methods

        /// <summary>
        /// </summary>
        public virtual void OnEntityChanged() {
            ////TODO: What is this?  Not sure what this is doing on this Property.  Doesn't look like the Entity effects this code at all.
            //if (CleanExpressions.Count == 0)
            //    CleanExpressions.Add("^\\w+_");

            //Configuration.Instance.CleanExpressions = new List<Regex>();
            //foreach (string clean in CleanExpressions)
            //{
            //    if (!String.IsNullOrEmpty(clean))
            //    {
            //        Configuration.Instance.CleanExpressions.Add(new Regex(clean, RegexOptions.IgnoreCase));
            //    }
            //}

            //TODO: Fix This
            //if (String.IsNullOrEmpty(EntityProjectName))
            //    EntityProjectName = String.Format("{0}.Entity", Entity.Namespace());
        }

        #endregion

        #region Public Method(s)

        public override void RegisterReferences() {
            RegisterReference("System.Configuration");
            //RegisterReference(Path.Combine(CodeTemplateInfo.DirectoryName, @"..\..\..\Framework\NetTiers.Build\netTiers.Core.dll"));
        }

        //[Browsable(false)]
        //[Optional]
        //public virtual string ClientName
        //{
        //    get
        //    {
        //        if (Entity == null) return String.Empty;

        //        //switch (Entity.Database.Provider.Name)
        //        //{
        //        //    case "OracleSchemaProvider":
        //        //        return "OracleClient";
        //        //}

        //        return "SqlClient";
        //    }
        //}

        #endregion
    }
}
