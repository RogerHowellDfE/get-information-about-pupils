using Microsoft.Extensions.DependencyInjection;

namespace DfE.GIAP.Core.UnitTests.TestDoubles;
internal static class ServiceCollectionTestDoubles
{
    internal static IServiceCollection Default() => new ServiceCollection();
}
