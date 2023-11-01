﻿using System.Collections.Generic;
using Couchbase.Lite;
using Couchbase.Lite.Query;

namespace DbViewer.Models
{
	public interface IDatabaseConnection
	{
		bool IsConnected { get; }
		IDataSourceAs ActiveSource { get; }

		bool Connect(string dbDirectory, string dbName);
		List<string> ListAllDocumentIds(bool sort = false);
		Document GetDocumentById(string id);
		void DeleteDocumentById(string id);
		bool Disconnect();
		void SaveDocument(MutableDocument document);
		void Compact();
	}
}
