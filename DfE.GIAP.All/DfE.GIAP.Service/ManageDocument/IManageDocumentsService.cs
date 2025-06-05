using DfE.GIAP.Core.Models.Editor;
using System.Collections.Generic;

namespace DfE.GIAP.Service.ManageDocument
{
    public interface IManageDocumentsService
    {
        IList<Document> GetDocumentsList();
    }
}
