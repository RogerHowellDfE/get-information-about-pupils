using System.Diagnostics.CodeAnalysis;

namespace DfE.GIAP.Core.Models.Editor
{
    [ExcludeFromCodeCoverage]
    public class Confirmation
    {
        public string Title { get; set; }

        public string Text { get; set; }

        public string Body { get; set; }
    }
}
