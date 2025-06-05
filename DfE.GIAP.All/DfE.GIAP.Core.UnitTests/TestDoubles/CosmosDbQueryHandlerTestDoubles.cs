using Dfe.Data.Common.Infrastructure.Persistence.CosmosDb.Handlers.Query;
using DfE.GIAP.Core.NewsArticles.Infrastructure.Repositories;

namespace DfE.GIAP.Core.UnitTests.TestDoubles;

internal static class CosmosDbQueryHandlerTestDoubles
{
    internal static Mock<ICosmosDbQueryHandler> Default() => new();

    internal static Mock<ICosmosDbQueryHandler> MockForGetNewsArticleById(Func<NewsArticleDTO> handler)
    {
        Mock<ICosmosDbQueryHandler> mockHandler = Default();

        mockHandler
            .Setup(t => t.ReadItemByIdAsync<NewsArticleDTO>(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(handler).Verifiable();

        return mockHandler;
    }

    internal static Mock<ICosmosDbQueryHandler> MockForGetNewsArticles(Func<IEnumerable<NewsArticleDTO>> handler)
    {
        Mock<ICosmosDbQueryHandler> mockHandler = Default();

        mockHandler
            .Setup(t => t.ReadItemsAsync<NewsArticleDTO>(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(handler)
            .Verifiable();

        return mockHandler;
    }
}
