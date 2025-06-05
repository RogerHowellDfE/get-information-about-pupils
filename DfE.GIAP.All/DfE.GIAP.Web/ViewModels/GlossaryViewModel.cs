using DfE.GIAP.Core.Models.Common;
using DfE.GIAP.Core.Models.Glossary;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace DfE.GIAP.Web.ViewModels
{
    [ExcludeFromCodeCoverage]
    public class GlossaryViewModel
    {
        public List<MetaDataDownload> MetaDataDownloadList = new List<MetaDataDownload>();

        public CommonResponseBodyViewModel Response { get; set; }
    }
}
