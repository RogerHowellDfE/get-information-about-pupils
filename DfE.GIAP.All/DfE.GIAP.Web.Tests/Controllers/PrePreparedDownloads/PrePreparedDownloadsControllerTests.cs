using DfE.GIAP.Common.AppSettings;
using DfE.GIAP.Common.Helpers.CookieManager;
using DfE.GIAP.Domain.Models.Common;
using DfE.GIAP.Service.Common;
using DfE.GIAP.Service.PreparedDownloads;
using DfE.GIAP.Web.Controllers.PreparedDownload;
using DfE.GIAP.Web.Tests.FakeData;
using DfE.GIAP.Web.ViewModels.PrePreparedDownload;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Moq;
using NSubstitute;
using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace DfE.GIAP.Web.Tests.Controllers.PrePreparedDownloads
{
    [Trait("PreparedDownloads", "PreparedDownloads Controller Unit Tests")]
    public class PrePreparedDownloadsControllerTests : IClassFixture<PreparedDownloadsResultsFake>,
                                                    IClassFixture<UserClaimsPrincipalFake>
    {
        private readonly PreparedDownloadsResultsFake _preparedDownloadsResultsFake;
        private readonly UserClaimsPrincipalFake _userClaimsPrincipalFake;
        private readonly IPrePreparedDownloadsService _mockprePreparedDownloadsService = Substitute.For<IPrePreparedDownloadsService>();
        private readonly ISession _mockSession = Substitute.For<ISession>();
        private readonly IOptions<AzureAppSettings> _mockAzureAppSettings = Substitute.For<IOptions<AzureAppSettings>>();

        public PrePreparedDownloadsControllerTests(PreparedDownloadsResultsFake preparedDownloadsResultsFake,
                                                UserClaimsPrincipalFake userClaimsPrincipalFake)
        {
            _preparedDownloadsResultsFake = preparedDownloadsResultsFake;
            _userClaimsPrincipalFake = userClaimsPrincipalFake;
        }
        [Fact]
        public async Task PreparedDownloadsController_Should_Returns_Correct_View()
        {
            // Arrange
            var results = _preparedDownloadsResultsFake.GetPrePreparedDownloadsList();
            var mockPrePreparedDownloads = new Mock<IPrePreparedDownloadsService>();
            mockPrePreparedDownloads.Setup(repo => repo.GetPrePreparedDownloadsList(It.IsAny<AzureFunctionHeaderDetails>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(_preparedDownloadsResultsFake.GetPrePreparedDownloadsList());
            _mockAzureAppSettings.Value.Returns(new AzureAppSettings
            {
                IsSessionIdStoredInCookie = true
            });
            var model = _preparedDownloadsResultsFake.GetGetPrePreparedDownloadsDetails();
            var controller = GetPreparedDownloadsController(mockPrePreparedDownloads);

            // Act
            var result = await controller.GetPreparedDownloadsAsync().ConfigureAwait(false);

            // Assert
            var viewResult = Assert.IsAssignableFrom<ViewResult>(result);
            var viewModel = viewResult.Model as PrePreparedDownloadsViewModel;

            viewResult.ViewName.Should().Be("~/Views/PrePreparedDownloads/PrePreparedDownload.cshtml");


        }
        [Fact]
        public async Task PreparedDownloadsController_Should_Returns_List_With_PrePrepareddownloads()
        {
            // Arrange
            var results = _preparedDownloadsResultsFake.GetPrePreparedDownloadsList();
            var mockPrePreparedDownloads = new Mock<IPrePreparedDownloadsService>();
            mockPrePreparedDownloads.Setup(repo => repo.GetPrePreparedDownloadsList(It.IsAny<AzureFunctionHeaderDetails>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(_preparedDownloadsResultsFake.GetPrePreparedDownloadsList());
            _mockAzureAppSettings.Value.Returns(new AzureAppSettings
            {
                IsSessionIdStoredInCookie = true
            });
            var model = _preparedDownloadsResultsFake.GetGetPrePreparedDownloadsDetails();
            var controller = GetPreparedDownloadsController(mockPrePreparedDownloads);

            // Act
            var result = await controller.GetPreparedDownloadsAsync().ConfigureAwait(false);

            // Assert
            var viewResult = Assert.IsAssignableFrom<ViewResult>(result);
            var viewModel = viewResult.Model as PrePreparedDownloadsViewModel;

            viewModel.PrePreparedDownloadList.Count.Should().Be(results.Count);

        }
        [Fact]
        public async Task GetPreparedDownloadsController_DownloadPrePreparedFile_Should_Return_File()
        {
            // Arrange
            var model = _preparedDownloadsResultsFake.GetMetaDataFile();
            var mockPrePreparedDownloads = new Mock<IPrePreparedDownloadsService>();
            mockPrePreparedDownloads.Setup(repo => repo.GetPrePreparedDownloadsList(It.IsAny<AzureFunctionHeaderDetails>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(_preparedDownloadsResultsFake.GetPrePreparedDownloadsList());
            _mockAzureAppSettings.Value.Returns(new AzureAppSettings
            {
                IsSessionIdStoredInCookie = true
            });
            var azureFunctionHeaderDetails = new AzureFunctionHeaderDetails() { ClientId = "123456", SessionId = "654321" };
            var ms = new MemoryStream();
            _mockprePreparedDownloadsService.PrePreparedDownloadsFileAsync("", ms, azureFunctionHeaderDetails).Returns(Task.FromResult(model));
            _mockAzureAppSettings.Value.Returns(new AzureAppSettings { IsSessionIdStoredInCookie = false });
            var controller = GetPreparedDownloadsController(mockPrePreparedDownloads);

            // Act
            var result = await controller.DownloadPrePreparedFile("PrePreparedDownloadTestFile.csv", DateTime.Now).ConfigureAwait(false);

            // Assert
            Assert.Equal(result.FileDownloadName, model.FileDownloadName);
            Assert.Equal(result.ContentType, model.ContentType);
        }
        private PreparedDownloadsController GetPreparedDownloadsController(Mock<IPrePreparedDownloadsService> mockPrePreparedDownloadsService)
        {
            var user = _userClaimsPrincipalFake.GetUserClaimsPrincipal();

            var _commonService = new Mock<ICommonService>();

            var _mockCookieManager = new Mock<ICookieManager>();


            return new PreparedDownloadsController(_commonService.Object, mockPrePreparedDownloadsService.Object)
            {
                ControllerContext = new ControllerContext()
                {
                    HttpContext = new DefaultHttpContext { User = user, Session = _mockSession },
                }
            };
        }

    }
}
