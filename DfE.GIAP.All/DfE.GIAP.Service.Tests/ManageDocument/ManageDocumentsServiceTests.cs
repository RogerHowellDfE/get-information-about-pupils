using DfE.GIAP.Core.Models.Editor;
using DfE.GIAP.Service.ManageDocument;
using Moq;
using System.Collections.Generic;
using Xunit;

namespace DfE.GIAP.Service.Tests.Services.ManageDocument
{
    [Trait("Category", "Manage Documents Service Unit Tests")]
    public class ManageDocumentsServiceTests
    {
        [Fact]
        public void ManageDocumentsServiceReturnsListOfDocuments()
        {
            //arrange
            List<Document> expectedDocumentsList = new List<Document>();
            expectedDocumentsList.Add(new Document() { Id = 1, DocumentId = "TestNewsArticle", DocumentName = "Test News Articles", SortId=1,IsEnabled=true });
            expectedDocumentsList.Add(new Document() { Id = 2, DocumentId = "PublicationSchedule", DocumentName = "Publication Schedule", SortId = 2, IsEnabled = true });
            expectedDocumentsList.Add(new Document() { Id = 3, DocumentId = "PlannedMaintenance", DocumentName = "Planned Maintenance", SortId = 3, IsEnabled = true });
                        
            var manageDocumentsService = new Mock<IManageDocumentsService>();
            manageDocumentsService.Setup(x => x.GetDocumentsList()).Returns(expectedDocumentsList);

            // act
            var actual = manageDocumentsService.Object.GetDocumentsList();

            // assert
            Assert.NotNull(actual);
            Assert.IsType<List<Document>>(actual);
            Assert.Equal(expectedDocumentsList[0].DocumentId, actual[0].DocumentId);
        }
    }

}
