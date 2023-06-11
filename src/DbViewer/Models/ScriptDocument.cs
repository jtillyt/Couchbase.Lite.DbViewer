using System;
using System.Collections.Generic;
using System.Text;
using Couchbase.Lite;
using MoonSharp.Interpreter;

namespace DbViewer.Models
{
    [MoonSharpUserData]
    public class ScriptDocument
    {

        private Document _document;
        private Dictionary<string, object> _dictionary;
        public ScriptDocument(Document document)
        {
            _document = document;
            _dictionary = document.ToDictionary();
        }

        public object this[string field]
        {
            get { return _dictionary[field]; }
            set { _dictionary[field] = value; }
        }
    }
}
