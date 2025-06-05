using DfE.GIAP.Common.AppSettings;
using DfE.GIAP.Common.Enums;
using DfE.GIAP.Common.Helpers.CookieManager;
using DfE.GIAP.Core.Models.Common;
using DfE.GIAP.Service.Common;
using DfE.GIAP.Service.Content;
using DfE.GIAP.Web.Controllers;
using DfE.GIAP.Web.Helpers.Consent;
using DfE.GIAP.Web.Tests.FakeData;
using DfE.GIAP.Web.ViewModels;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using System;
using System.Threading.Tasks;
using Xunit;

namespace DfE.GIAP.Web.Tests.Controllers.Home
{
    public class HomeControllerTests
    {
        private readonly IContentService _contentService = Substitute.For<IContentService>();
        private readonly IExceptionHandlerPathFeature _exceptionPathFeature = Substitute.For<IExceptionHandlerPathFeature>();


        [Fact]
        public async Task Index_returns_the_consent_view()
        {
            // Arrange
            var consentResponse = new CommonResponseBody();
            _contentService.GetContent(DocumentType.Consent).Returns(consentResponse);
            var controller = GetHomeController();

            // Act
            var result = await controller.Index();

            // Assert
            Assert.IsType<ViewResult>(result);
            var viewResult = result as ViewResult;
            Assert.IsType<ConsentViewModel>(viewResult.Model);
            var viewModel = viewResult.Model as ConsentViewModel;
            Assert.Equal(consentResponse, viewModel.Response);
        }


        [Fact]
        public void Index_Post_redirects_to_start_when_consent_given()
        {
            // Arrange
            var controller = GetHomeController();
            var consentModel = new ConsentViewModel()
            {
                ConsentGiven = true
            };

            // Act
            var result = controller.Index(consentModel);


            // Assert
            Assert.IsType<RedirectToActionResult>(result);
            var redirectResult = result as RedirectToActionResult;
            Assert.Equal("Index", redirectResult.ActionName);
            Assert.Equal("Landing", redirectResult.ControllerName);
            Assert.True(ConsentHelper.HasGivenConsent(controller.HttpContext));
        }

        [Fact]
        public void Index_Post_shows_error_when_consent_not_given()
        {
            // Arrange
            var controller = GetHomeController();
            var consentModel = new ConsentViewModel()
            {
                ConsentGiven = false
            };


            // Act
            var result = controller.Index(consentModel);

            // Assert
            Assert.IsType<ViewResult>(result);
            var viewResult = result as ViewResult;
            Assert.IsType<ConsentViewModel>(viewResult.Model);
            var viewModel = viewResult.Model as ConsentViewModel;
            Assert.True(viewModel.ConsentError);
        }

        [Fact]
        public void Error404_returns_view()
        {
            // Arrange
            var controller = GetHomeController();

            // Act
            var result = controller.Error404();

            // Assert
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public void Error_returns_view()
        {
            // Arrange
            var controller = GetHomeController();

            // Act
            var result = controller.Error(500);

            // Assert
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public void UserWithNoRole_returns_view()
        {
            // Arrange
            var controller = GetHomeController();

            // Act
            var result = controller.UserWithNoRole();

            // Assert
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public void Exception_page_shows_error_in_dev()
        {
            // Arrange
            var controller = GetHomeController();
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");

            // Act
            var result = controller.Exception();

            // Assert
            Assert.IsType<ViewResult>(result);
            var viewResult = result as ViewResult;
            Assert.IsType<ErrorModel>(viewResult.Model);
            var viewModel = viewResult.Model as ErrorModel;
            Assert.True(viewModel.ShowError);
            Assert.Equal("test Page: Home.", viewModel.ExceptionMessage);
            Assert.Null(viewModel.Stacktrace);
        }

        [Fact]
        public void Exception_page_doesnt_show_error_in_production()
        {
            // Arrange
            var controller = GetHomeController();
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Production");

            // Act
            var result = controller.Exception();

            // Assert
            Assert.IsType<ViewResult>(result);
            var viewResult = result as ViewResult;
            Assert.IsType<ErrorModel>(viewResult.Model);
            var viewModel = viewResult.Model as ErrorModel;
            Assert.False(viewModel.ShowError);
        }

        private HomeController GetHomeController()
        {
            var cookieManager = Substitute.For<ICookieManager>();
            var commonService = Substitute.For<ICommonService>();
            var logger = Substitute.For<ILogger<HomeController>>();
            var options = Substitute.For<IOptions<AzureAppSettings>>();

            var azureAppSettings = new AzureAppSettings()
            {
                IsSessionIdStoredInCookie = true
            };

            options.Value.Returns(azureAppSettings);

            _exceptionPathFeature.Error.Returns(new Exception("test"));
            _exceptionPathFeature.Path.Returns("/");

            var claimsPrincipal = new UserClaimsPrincipalFake().GetAdminUserClaimsPrincipal();
            var controllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext()
                {
                    User = claimsPrincipal,
                    Session = new TestSession(),
                }
            };
            controllerContext.HttpContext.Features.Set(_exceptionPathFeature);

            return new HomeController(options, _contentService, cookieManager)
            {
                ControllerContext = controllerContext
            };
        }
    }
}
