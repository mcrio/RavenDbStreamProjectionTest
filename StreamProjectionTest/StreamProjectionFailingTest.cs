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
    public class StreamProjectionFailingTest : RavenTestDriver
    {
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
            var context = new DbContextFromAnotherAssembly(store);
            var query = context
                .Session
                .Query<Article>()
                
                // NOTE: If you remove this projection the test will pass ! =======================================
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