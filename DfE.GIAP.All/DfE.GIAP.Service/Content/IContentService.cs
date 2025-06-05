using DfE.GIAP.Common.Enums;
using DfE.GIAP.Core.Models.Common;
using DfE.GIAP.Domain.Models.Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DfE.GIAP.Service.Content
{
    public interface IContentService
    {
        Task<CommonResponseBody> GetContent(DocumentType documentType);

        Task<CommonResponseBody> SetDocumentToPublished(CommonRequestBody requestBody, AzureFunctionHeaderDetails headerDetails);

        Task<CommonResponseBody> AddOrUpdateDocument(CommonRequestBody requestBody, AzureFunctionHeaderDetails azureFunctionHeaderDetails);
    }
}
