using DfE.GIAP.Common.AppSettings;
using DfE.GIAP.Common.Constants;
using DfE.GIAP.Common.Constants.Messages.Downloads;
using DfE.GIAP.Common.Constants.Routes;
using DfE.GIAP.Common.Enums;
using DfE.GIAP.Common.Helpers;
using DfE.GIAP.Domain.Models.Common;
using DfE.GIAP.Service.Content;
using DfE.GIAP.Service.Download;
using DfE.GIAP.Service.MPL;
using DfE.GIAP.Service.Search;
using DfE.GIAP.Web.Extensions;
using DfE.GIAP.Web.Helpers.Search;
using DfE.GIAP.Web.Helpers.SearchDownload;
using DfE.GIAP.Web.Helpers.SelectionManager;
using DfE.GIAP.Web.ViewModels.Search;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace DfE.GIAP.Web.Controllers.LearnerNumber
{
    [Route(ApplicationRoute.Search)]
    public class PupilPremiumLearnerNumberController : BaseLearnerNumberController
    {
        private readonly ILogger<PupilPremiumLearnerNumberController> _logger;
        private readonly IDownloadService _downloadService;
        private readonly AzureAppSettings _appSettings;

        #region abstract property implementation

        public override string PageHeading => ApplicationLabel.SearchPupilPremiumWithUpnPageHeading;

        public override string SearchAction => "PupilPremium";
        public override string FullTextLearnerSearchController => Global.PupilPremiumNonUpnController;
        public override string FullTextLearnerSearchAction => "NonUpnPupilPremiumDatabase";
        public override string InvalidUPNsConfirmationAction => "PPInvalidUPNsConfirmation";
        public override string DownloadLinksPartial => "~/Views/Shared/LearnerNumber/_SearchPupilPremiumDownloadLinks.cshtml";
        public override AzureSearchIndexType IndexType => AzureSearchIndexType.PupilPremium;
        public override string SearchSessionKey => "SearchPPUPN_SearchText";
        public override string SearchSessionSortField => "SearchPPUPN_SearchTextSortField";
        public override string SearchSessionSortDirection => "SearchPPUPN_SearchTextSortDirection";
        public override int MyPupilListLimit => _appSettings.UpnPPMyPupilListLimit;
        public override bool ShowLocalAuthority => _appSettings.UseLAColumn;
        public override bool ShowMiddleNames => true;
        public override string DownloadSelectedLink => ApplicationLabel.DownloadSelectedPupilPremiumDataLink;

        public override string LearnerNumberLabel => Global.LearnerNumberLabel;

        #endregion abstract property implementation

        public PupilPremiumLearnerNumberController(ILogger<PupilPremiumLearnerNumberController> logger,
            IDownloadService downloadService,
            IPaginatedSearchService paginatedSearch,
            IMyPupilListService mplService,
            ISelectionManager selectionManager,
            IContentService contentService,
            IOptions<AzureAppSettings> azureAppSettings)
            : base(logger, paginatedSearch, mplService, selectionManager, contentService, azureAppSettings)
        {
            _logger = logger ??
                throw new ArgumentNullException(nameof(logger));
            _downloadService = downloadService ??
                throw new ArgumentNullException(nameof(downloadService));
            _appSettings = azureAppSettings.Value;
        }

        #region Search

        [Route(ApplicationRoute.PupilPremium)]
        [HttpGet]
        public async Task<IActionResult> PupilPremium(bool? returnToSearch)
        {
            _logger.LogInformation("Pupil Premium Upn GET method called");
            return await Search(returnToSearch);
        }

        [Route(ApplicationRoute.PupilPremium)]
        [HttpPost]
        public async Task<IActionResult> PupilPremium(
            [FromForm] LearnerNumberSearchViewModel model,
            [FromQuery] int pageNumber,
            [FromQuery] string sortField,
            [FromQuery] string sortDirection,
            bool calledByController = false)
        {
            _logger.LogInformation("Pupil Premium Upn POST method called");
            return await Search(
                model,
                pageNumber,
                sortField,
                sortDirection,
                !ControllerContext.HttpContext.Request.Query.ContainsKey("pageNumber"),
                calledByController,
                ControllerContext.HttpContext.Request.Query.ContainsKey("reset"));
        }

        #endregion Search

        #region Invalid UPNs

        [HttpPost]
        [Route(Route.PPInvalidUPNs)]
        public async Task<IActionResult> PPInvalidUPNs(InvalidLearnerNumberSearchViewModel model)
        {
            return await InvalidUPNs(model);
        }

        [HttpPost]
        [Route(Route.PPInvalidUPNsConfirmation)]
        public async Task<IActionResult> PPInvalidUPNsConfirmation(InvalidLearnerNumberSearchViewModel model)
        {
            return await InvalidUPNsConfirmation(model);
        }

        #endregion Invalid UPNs

        #region MPL

        [HttpPost]
        [Route("add-pp-to-my-pupil-list")]
        public async Task<IActionResult> PPAddToMyPupilList(LearnerNumberSearchViewModel model)
        {
            return await AddToMyPupilList(model);
        }

        #endregion MPL

        #region Download

        [Route(Route.PupilPremium.LearnerNumberDownloadRequest)]
        [HttpPost]
        public async Task<IActionResult> ToDownloadSelectedPupilPremiumDataUPN(LearnerNumberSearchViewModel searchViewModel)
        {
            SetSelections(
                searchViewModel.PageLearnerNumbers.Split(','),
                searchViewModel.SelectedPupil);

            var selectedPupils = GetSelected(searchViewModel.LearnerNumberIds.FormatLearnerNumbers());

            if (selectedPupils.Count == 0)
            {
                searchViewModel.NoPupil = true;
                searchViewModel.NoPupilSelected = true;
                return await PupilPremium(searchViewModel, searchViewModel.PageNumber, this.HttpContext.Session.GetString(SearchSessionSortField), this.HttpContext.Session.GetString(SearchSessionSortDirection), true);
            }

            var userOrganisation = new UserOrganisation
            {
                IsAdmin = User.IsAdmin(),
                IsEstablishment = User.IsOrganisationEstablishment(),
                IsLa = User.IsOrganisationLocalAuthority(),
                IsMAT = User.IsOrganisationMultiAcademyTrust(),
                IsSAT = User.IsOrganisationSingleAcademyTrust()
            };

            var downloadFile = await _downloadService.GetPupilPremiumCSVFile(selectedPupils.ToArray(), searchViewModel.LearnerNumber.FormatLearnerNumbers(),
                true, AzureFunctionHeaderDetails.Create(User.GetUserId(), User.GetSessionId()), ReturnRoute.PupilPremium, userOrganisation).ConfigureAwait(false);

            if (downloadFile == null)
            {
                return RedirectToAction(ApplicationRoute.Error, ApplicationRoute.Home);
            }

            if (downloadFile.Bytes != null)
            {
                return SearchDownloadHelper.DownloadFile(downloadFile);
            }
            else
            {
                searchViewModel.ErrorDetails = DownloadErrorMessages.NoDataForSelectedPupils;
            }

            return await PupilPremium(searchViewModel, searchViewModel.PageNumber, this.HttpContext.Session.GetString(SearchSessionSortField), this.HttpContext.Session.GetString(SearchSessionSortDirection), true);
        }

        #endregion Download

        #region Abstract implementation

        protected override async Task<IActionResult> ReturnToPage(LearnerNumberSearchViewModel model)
        {
            return await PupilPremium(model, model.PageNumber, model.SortField, model.SortDirection, true);
        }

        protected override bool ValidateLearnerNumber(string learnerNumber)
        {
            return ValidationHelper.IsValidUpn(learnerNumber);
        }

        protected override string GenerateValidationMessage()
        {
            return PupilHelper.GenerateValidationMessageUpnSearch(ModelState);
        }

        #endregion Abstract implementation
    }
}