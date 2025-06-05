using DfE.GIAP.Core.Models.Common;
using DfE.GIAP.Web.ViewModels;

namespace DfE.GIAP.Web.Tests.FakeData
{
    public class AccessibilityResultsFake
    {
        public AccessibilityViewModel GetAccessibilityDetails()
        {
            return new AccessibilityViewModel() { Response = new CommonResponseBodyViewModel() { Title = "Test Title", Body = "Sample body" } };
        }

        public CommonResponseBody GetCommonResponseBody()
        {
            return new CommonResponseBody
            {
                Id = "1",
                Title = "Test Title",
                Body = "Sample body"
            };
        }
    }
}
