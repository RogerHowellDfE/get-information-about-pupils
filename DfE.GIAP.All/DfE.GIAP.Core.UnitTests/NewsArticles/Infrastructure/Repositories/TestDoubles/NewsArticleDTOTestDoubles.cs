using Bogus;
using DfE.GIAP.Core.NewsArticles.Infrastructure.Repositories;
using DfE.GIAP.Core.UnitTests.Extensions;

namespace DfE.GIAP.Core.UnitTests.NewsArticles.Infrastructure.Repositories.TestDoubles;

internal static class NewsArticleDTOTestDoubles
{
    private const int NEWS_ARTICLES_DOCUMENT_TYPE = 7;
    internal static List<NewsArticleDTO> Generate(int count = 10)
    {
        int randomSeed = new Random().Next();
        List<NewsArticleDTO> articles = [];

        for (int index = 0; index < count; index++)
        {
            int seed = randomSeed + index;
            Faker<NewsArticleDTO> faker = CreateGenerator(seed);
            articles.Add(
                faker.Generate());
        }

        return articles;
    }

    private static Faker<NewsArticleDTO> CreateGenerator(int seed)
    {
        return new Faker<NewsArticleDTO>()
            .StrictMode(true)
            .UseSeed(seed)
            .RuleFor(t => t.Pinned, (f) => f.Random.Bool())
            .RuleFor(t => t.Archived, (f) => f.Random.Bool())
            .RuleFor(t => t.Published, (f) => f.Random.Bool())
            .RuleFor(t => t.ID, (f) => f.Lorem.Word())
            .RuleFor(t => t.DraftBody, (f) => f.Lorem.Words().Merge())
            .RuleFor(t => t.DraftTitle, (f) => f.Lorem.Words().Merge())
            .RuleFor(t => t.ModifiedDate, (f) => f.Date.Recent())
            .RuleFor(t => t.CreatedDate, (f) => f.Date.Recent())
            .RuleFor(t => t.Title, (f) => f.Lorem.Words().Merge())
            .RuleFor(t => t.Body, (f) => f.Lorem.Words().Merge())
            .RuleFor(t => t.DocumentType, (f) => NEWS_ARTICLES_DOCUMENT_TYPE);
    }
}
