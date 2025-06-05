using Microsoft.Extensions.Options;

namespace DfE.GIAP.Core.UnitTests.TestDoubles;
internal static class OptionsTestDoubles
{
    internal static IOptions<T> Default<T>() where T : class, new()
    {
        return Wrap(new T());
    }

    internal static IOptions<T> WithNullValue<T>() where T : class
    {
        return Wrap<T>(null);
    }

    private static IOptions<T> Wrap<T>(T? value) where T : class
    {
        Mock<IOptions<T>> mock = new();
        mock.Setup(t => t.Value).Returns(value).Verifiable();
        return mock.Object;
    }
}
