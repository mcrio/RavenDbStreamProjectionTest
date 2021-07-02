using Raven.Client.Documents;
using Raven.Client.Documents.Session;

namespace Lib
{
    public class DbContextFromAnotherAssembly
    {
        public DbContextFromAnotherAssembly(IDocumentStore store)
        {
            Session = store.OpenAsyncSession();
            Session.Advanced.OnBeforeQuery += OnBeforeQueryHandler;
        }

        public IAsyncDocumentSession Session { get; }

        private static void OnBeforeQueryHandler(object sender, BeforeQueryEventArgs args)
        {
            dynamic queryToBeExecuted = args.QueryCustomization;
            queryToBeExecuted.AndAlso(wrapPreviousQueryClauses: true);
            queryToBeExecuted.WhereEquals(nameof(Article.IsDeleted), true);
        }
    }
}