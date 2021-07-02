using System.Collections.Generic;
using System.Threading.Tasks;
using Lib;
using Raven.Client.Documents;
using Raven.Client.Documents.Linq;
using Raven.Client.Documents.Session;
using Raven.TestDriver;
using Xunit;

namespace StreamProjectionTest
{
    public class StreamProjectionPassingTest: RavenTestDriver
    {
        /// <summary>
        /// Make DbContext class part of the same assembly and the same test will pass...
        /// </summary>
        private class DbContextInSameAssembly
        {
            public DbContextInSameAssembly(IDocumentStore store)
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
        
        [Fact]
        public async Task ShouldAllowWrappingPreviousQueryInClausesInOnBeforeQueryForStreamsWithProjection()
        {
            using IDocumentStore store = GetDocumentStore();

            // PREPARE DATA
            using (IAsyncDocumentSession session = store.OpenAsyncSession())
            {
                await session.StoreAsync(
                    new Article("Article 1", "foo", false),
                    "articles/1"
                );

                await session.StoreAsync(
                    new Article("Article 2", "bar", true),
                    "articles/2"
                );
                await session.SaveChangesAsync();
            }

            // QUERY 
            var context = new DbContextInSameAssembly(store);
            var query = context
                .Session
                .Query<Article>()
                .Select(article => new { ArticleDocument = article });

            var streamResults = await context
                .Session
                .Advanced
                .StreamAsync(query);

            var allArticles = new List<Article>();
            while (await streamResults.MoveNextAsync())
            {
                allArticles.Add(streamResults.Current.Document.ArticleDocument);
            }

            Assert.NotEmpty(allArticles);
            Assert.Single(allArticles);
        }
    }
}