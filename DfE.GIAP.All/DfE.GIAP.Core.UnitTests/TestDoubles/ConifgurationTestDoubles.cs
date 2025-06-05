using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Memory;

namespace DfE.GIAP.Core.UnitTests.TestDoubles;
internal static class ConfigurationTestDoubles
{
    internal static IConfiguration Default() => DefaultBuilder().Build();

    internal static IConfiguration WithRepositoryOptions()
    {
        ConfigurationBuilder builder = DefaultBuilder();

        Dictionary<string, string?> config = new()
        {
            ["RepositoryOptions:ConnectionMode"] = "1",
            ["RepositoryOptions:EndpointUri"] = "https://endpoint-uri.test",
            ["RepositoryOptions:PrimaryKey"] = "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==", // must be base64 encoded
        };

        builder.Add(
            new MemoryConfigurationSource()
            {
                InitialData = config
            });

        return builder.Build();
    }

    private static ConfigurationBuilder DefaultBuilder() => new();
}
