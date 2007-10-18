namespace SolrNet.DSL {
	public interface IDSLQuery<T> where T: ISolrDocument, new() {
		IDSLQuery<T> ByRange<RT>(string fieldName, RT from, RT to);
		ISolrQueryResults<T> Run();
		IDSLQueryBy<T> By(string fieldName);
	}
}