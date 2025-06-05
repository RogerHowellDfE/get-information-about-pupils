using DfE.GIAP.Common.Enums;
using DfE.GIAP.Core.Models.Editor;
using DfE.GIAP.Service.ApiProcessor;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;

namespace DfE.GIAP.Service.ManageDocument
{
    public class ManageDocumentsService : IManageDocumentsService
    {
        public ManageDocumentsService() { }

        public IList<Document> GetDocumentsList()
        {
            IList<Document> enumValList = new List<Document>();
            int counter = 1;
            foreach (var e in Enum.GetValues(typeof(DocumentType)))
            {
                var fieldInfo = e.GetType().GetField(e.ToString());
                var attributes = (DescriptionAttribute[])fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), false);
                var document = new Document
                {
                    Id = counter,
                    DocumentId = e.ToString(),
                    DocumentName = attributes[0].Description,
                    SortId = counter,
                    IsEnabled = true
                };
                enumValList.Add(document);
                counter++;
            }
            return enumValList.OrderBy(x => x.DocumentName).ToList();
        }
    }
}
