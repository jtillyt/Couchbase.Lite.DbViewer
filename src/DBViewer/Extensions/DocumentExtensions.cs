using Couchbase.Lite;

namespace DbViewer.Extensions
{
    public static class DocumentExtensions
    {
        public static MutableDocument CleanAttachments(this Document document)
        {
            var mutableDoc = document.ToMutable();

            foreach (var prop in mutableDoc)
            {
                if (prop.Value is Blob)
                {
                    mutableDoc.SetBlob(prop.Key, null);
                }
            }

            return mutableDoc;
        }
    }
}
