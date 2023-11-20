using System.Reflection;
using System.Text;
using Couchbase.Lite;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;

namespace CouchbaseLite.Browser.Data
{
	public class DatabaseService
	{
		private Dictionary<Guid, DatabaseConnection> _connections = new Dictionary<Guid, DatabaseConnection>();
		public LoadedDatabase LoadDb(DatabaseViewerProperties properties)
		{
			DatabaseConnection connection = null;

			if (!_connections.TryGetValue(properties.ViewerId, out connection))
			{
				connection = new DatabaseConnection();

				try
				{
					connection.Connect(properties.DatabasePath, "HHNext");
					_connections.Add(properties.ViewerId, connection);
				}
				catch (Exception ex)
				{
					return new LoadedDatabase() { ErrorMessage = "Error loading database:" + ex.Message };
				}
			}

			var documentIds = GetResults(connection, properties);

			var documentIdGroups = documentIds.GroupBy(x =>
			{
				var split = x.Split("::");
				return split.Length > 1 ? split[0] : "Ungrouped";

			});
			var documentGroups = new List<DatabaseDocumentGroup>();
			var documentList = new List<DatabaseDocument>();

			foreach (var idGroup in documentIdGroups)
			{
				var documentGroup = new DatabaseDocumentGroup
				{
					GroupName = idGroup.Key
				};

				foreach (var documentId in idGroup)
				{
					var doc = new DatabaseDocument() { DisplayName = documentId };

					documentGroup.Documents.Add(doc);
					documentList.Add(doc);
				}

				documentGroups.Add(documentGroup);
			}

			return new LoadedDatabase()
			{
				DatabaseDocuments = documentList,
				DatabaseDocumentGroups = documentGroups,
				ResultCount = documentList.Count()
			};
		}

		private List<string> GetResults(DatabaseConnection connection, DatabaseViewerProperties databaseViewerProperties)
		{
			if (string.IsNullOrWhiteSpace(databaseViewerProperties.DatabaseQuery))
			{
				return connection.FindDocumentIds().OrderBy(x => x).ToList();
			}
			else
			{
				return CompileAndExecuteQuery(databaseViewerProperties.DatabaseQuery, connection.Database);
			}
		}

		private List<string> CompileAndExecuteQuery(string source, Database db)
		{
			var result = new List<string>();
			var tree = SyntaxFactory.ParseSyntaxTree(source.Trim());

			var compilation = CSharpCompilation.Create("Executor.cs")
						.WithOptions(new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary,
							optimizationLevel: OptimizationLevel.Release))
						.WithReferences(GetReferences())
						.AddSyntaxTrees(tree);

			string errorMessage = null;
			Assembly assembly = null;
			Stream codeStream = null;

			using (codeStream = new MemoryStream())
			{
				// Actually compile the code
				EmitResult compilationResult = null;
				compilationResult = compilation.Emit(codeStream);

				// Compilation Error handling
				if (!compilationResult.Success)
				{
					var sb = new StringBuilder();
					foreach (var diag in compilationResult.Diagnostics)
					{
						sb.AppendLine(diag.ToString());
					}

					errorMessage = sb.ToString();

					return result;
				}

				// Load
				assembly = Assembly.Load(((MemoryStream)codeStream).ToArray());
			}

			// Instantiate
			dynamic instance = assembly.CreateInstance("__ScriptExecution.__Executor");

			return instance.FindDocumentIds(db);
		}

		private List<MetadataReference> GetReferences()
		{
			var references = new List<MetadataReference>();

			var couchbaseAssembly = typeof(Couchbase.Lite.Collection).Assembly;

			foreach (var corePath in GetNetCoreDefaultReferences())
			{
				references.Add(MetadataReference.CreateFromFile(corePath));
			}

			references.Add(MetadataReference.CreateFromFile(couchbaseAssembly.Location));

			return references;
		}

		public List<string> GetNetCoreDefaultReferences()
		{
			var rtPath = Path.GetDirectoryName(typeof(object).Assembly.Location) +
						 Path.DirectorySeparatorChar;

			return new List<string>
			{
				rtPath + "System.Private.CoreLib.dll",
				rtPath + "System.Runtime.dll",
				rtPath + "System.Console.dll",
				rtPath + "netstandard.dll",

				rtPath + "System.Text.RegularExpressions.dll", // IMPORTANT!
				rtPath + "System.Linq.dll",
				rtPath + "System.Linq.Expressions.dll", // IMPORTANT!

				rtPath + "System.IO.dll",
				rtPath + "System.Net.Primitives.dll",
				rtPath + "System.Net.Http.dll",
				rtPath + "System.Private.Uri.dll",
				rtPath + "System.Reflection.dll",
				rtPath + "System.ComponentModel.Primitives.dll",
				rtPath + "System.Globalization.dll",
				rtPath + "System.Collections.dll",
				rtPath + "System.Collections.Concurrent.dll",
				rtPath + "System.Collections.NonGeneric.dll",
				rtPath + "Microsoft.CSharp.dll"
			};
		}
	}
}