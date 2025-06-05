using DfE.GIAP.Core.Models.Editor;
using System.Diagnostics.CodeAnalysis;

namespace DfE.GIAP.Web.ViewModels.Admin
{
    [ExcludeFromCodeCoverage]
    public class ManageDocumentsViewModel
    {
        public Document DocumentList { get; set; }

        public CommonResponseBodyViewModel DocumentData { get; set; }

        public string SelectedNewsId { get; set; }

        public string ArchivedNewsId { get; set; }

        public bool HasInvalidDocumentList { get; set; }

        public bool HasInvalidNewsList { get; set; }

        public bool HasInvalidArchiveList { get; set; }

        public Confirmation Confirmation { get; set; }

        public string ErrorDetails { get; set; }

        public BackButtonViewModel BackButton { get; set; }
    }
}
