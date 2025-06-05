using DfE.GIAP.Core.Models.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
namespace DfE.GIAP.Web.ViewModels
{
    [ExcludeFromCodeCoverage]
    public class LandingViewModel
    {
        public CommonResponseBody LandingResponse { get; set; }
        public CommonResponseBody PlannedMaintenanceResponse { get; set; }
        public CommonResponseBody PublicationScheduleResponse { get; set; }
        public CommonResponseBody FAQResponse { get; set; }
    }
}
