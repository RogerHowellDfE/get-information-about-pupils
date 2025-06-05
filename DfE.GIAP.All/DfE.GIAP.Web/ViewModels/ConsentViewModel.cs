using DfE.GIAP.Core.Models.Common;
using System.Diagnostics.CodeAnalysis;

namespace DfE.GIAP.Web.ViewModels
{
    [ExcludeFromCodeCoverage]
    public class ConsentViewModel
    {
        public CommonResponseBody Response { get; set; }

        public bool ConsentGiven { get; set; }

        public bool ConsentError { get; set; }
    }
}
