namespace CouchbaseLite.Browser.Data
{
	public class DatabaseViewerProperties
	{
		public Guid ViewerId { get; set; }

		public string DatabasePath { get; set; } = "Z:\\Projects\\Pepsi\\Pepsi.Private\\DBWithPlanogramAndDocumentExport\\DataStore";

		public string DatabaseQuery { get; set; } =
@"using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Couchbase.Lite.Query;
using Couchbase.Lite;

namespace __ScriptExecution {
	public class __Executor 
	{ 
		public List<string> FindDocumentIds(Database db)
		{
			if (db == null)
			{
				return Enumerable.Empty<string>().ToList();
			}

			var docIds = QueryBuilder
				.Select(SelectResult.Expression(Meta.ID))
								.From(DataSource.Database(db))
								.Where(Expression.Property(""$Type"")
										 .EqualTo(Expression.String(""CustomerPlanogramList""))
										 .And(Expression.Property(""GtmuCustId"")
											.EqualTo(Expression.String(""42179616"")))
											.And(ArrayExpression.Any(ArrayExpression.Variable(""Fxtrs""))
												.In(Expression.Property(""Fxtrs""))
												.Satisfies(ArrayExpression.Variable(""Fxtrs.FxtrId"")
												.EqualTo(Expression.String(""5672C8F1-D8E8-471B-A1B7-8401983BDCE6"")))))

				.Execute()
				.Select(i => i.GetString(""id""))
				.Where(docId => docId != null).ToList();

			return docIds;
		}
	}
}";

		public string DatabaseName { get; set; } = string.Empty;
	}
}
