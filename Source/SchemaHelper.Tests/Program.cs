using System;
using System.Text.RegularExpressions;
using CodeSmith.SchemaHelper;
using SchemaExplorer;
using Configuration = CodeSmith.SchemaHelper.Configuration;

namespace SchemaHelper.Tests {
    internal class Program {
        private static void Main(string[] args) {
            string databaseName = "PetShop";

            Configuration.Instance.IncludeFunctions = true;
            Configuration.Instance.IncludeViews = true;

            Configuration.Instance.CleanExpressions.Add(new Regex("^(sp|tbl|udf|vw|se)_", RegexOptions.IgnoreCase));
            Configuration.Instance.IgnoreExpressions.Add(new Regex("^dbo.sysdiagrams$", RegexOptions.IgnoreCase));
            Configuration.Instance.IgnoreExpressions.Add(new Regex("^dbo.aspnet", RegexOptions.IgnoreCase));
            Configuration.Instance.IgnoreExpressions.Add(new Regex("^dbo.vw_aspnet", RegexOptions.IgnoreCase));

            var database = new DatabaseSchema(new SqlSchemaProvider(), String.Format(@"Data Source=.;Initial Catalog={0};Integrated Security=True", databaseName)) {
                DeepLoad = true
            };
            var provider = new SchemaExplorerEntityProvider(database);
            var manager = new EntityManager(provider);

            foreach (var entity in manager.Entities) {
                if (entity.Associations.Count == 0)
                    continue;

                Console.WriteLine("{0}-{1}", entity.TypeAccess, entity.Name);
                foreach (var association in entity.Associations) {
                    Console.WriteLine(String.Format("   {0} IsParent: {1}, AssociationType: {2}, Properties: {3}, Entity Properties: {4}, Foreign Entity Properties: {5}, AssociationKeyName: {6}", association.AssociationKeyName, association.IsParentEntity, association.AssociationType, association.Properties.Count, association.Entity.Properties.Count, association.ForeignEntity.Properties.Count, association.AssociationKeyName));
                }

                Console.WriteLine();
            }

            Console.ReadLine();
        }
    }
}