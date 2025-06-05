using DfE.GIAP.Common.AppSettings;
using DfE.GIAP.Common.Constants;
using DfE.GIAP.Common.Constants.Messages.Downloads;
using DfE.GIAP.Common.Constants.Routes;
using DfE.GIAP.Common.Enums;
using DfE.GIAP.Common.Helpers;
using DfE.GIAP.Common.Helpers.Rbac;
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
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;

namespace DfE.GIAP.Web.Controllers.TextBasedSearch
{
    [Route(ApplicationRoute.Search)]
    public class PPLearnerTextSearchController : BaseLearnerTextSearchController
    {
        private readonly ILogger<PPLearnerTextSearchController> _logger;
        private readonly IDownloadService _downloadService;
        private readonly AzureAppSettings _appSettings;

        #region abstract property implementation

        public override string PageHeading => ApplicationLabel.SearchPupilPremiumWithOutUpnPageHeading;
        public override string SearchSessionKey => Global.PupilPremiumNonUpnSearchSessionKey;
        public override string SearchFiltersSessionKey => Global.PupilPremiumNonUpnSearchFiltersSessionKey;

        public override string SortDirectionKey => Global.PupilPremiumNonUpnSortDirectionSessionKey;
        public override string SortFieldKey => Global.PupilPremiumNonUpnSortFieldSessionKey;

        public override string DownloadLinksPartial => Global.PupilPremiumNonUpnDownloadLinksView;
        public override string SearchLearnerNumberAction => Route.PupilPremium.PupilPremiumDatabase;
        public override string RedirectUrlFormAction => Global.PupilPremiumNonUpnAction;
        public override string LearnerTextDatabaseAction => Global.PupilPremiumNonUpnAction;
        public override string LearnerTextDatabaseName => Global.PPLearnerTextSearchDatabaseName;
        public override string RedirectFrom => Route.PupilPremium.NonUPN;

        #region Search Filter Properties

        public override string SurnameFilterUrl => Route.PupilPremium.NonUPNSurnameFilter;
        public override string DobFilterUrl => Route.PupilPremium.NonUpnDobFilter;
        public override string ForenameFilterUrl => Route.PupilPremium.NonUpnForenameFilter;
        public override string MiddlenameFilterUrl => Route.PupilPremium.NonUpnMiddlenameFilter;
        public override string GenderFilterUrl => Route.PupilPremium.NonUpnGenderFilter;

        public override string SexFilterUrl => Route.PupilPremium.NonUpnSexFilter;
        public override string FormAction => Route.PupilPremium.NonUPN;
        public override string RemoveActionUrl => $"/{ApplicationRoute.Search}/{Route.PupilPremium.NonUPN}";
        public override AzureSearchIndexType IndexType => AzureSearchIndexType.PupilPremium;
        public override string SearchView => Global.NonUpnSearchView;

        #endregion Search Filter Properties

        public override string SearchLearnerNumberController => ApplicationRoute.Search;
        public override string SearchAction => Global.PupilPremiumNonUpnAction;
        public override string SearchController => Global.PupilPremiumNonUpnController;
        public override int MyPupilListLimit => _appSettings.NonUpnPPMyPupilListLimit;
        public override ReturnRoute ReturnRoute => Common.Enums.ReturnRoute.NonPupilPremium;
        public override string LearnerTextSearchController => Global.PupilPremiumNonUpnController;
        public override string LearnerTextSearchAction => SearchAction;
        public override string LearnerNumberAction => Global.PupilPremiumAction;
        public override bool ShowGender => _appSettings.PpUseGender;
        public override bool ShowLocalAuthority => _appSettings.UseLAColumn;
        public override string InvalidUPNsConfirmationAction => Global.PupilPremiumNonUpnInvalidUPNsConfirmation;
        public override string LearnerNumberLabel => Global.LearnerNumberLabel;
        public override bool ShowMiddleNames => true;

        public override string DownloadSelectedLink => ApplicationLabel.DownloadSelectedPupilPremiumDataLink;

        #endregion abstract property implementation

        public PPLearnerTextSearchController(ILogger<PPLearnerTextSearchController> logger,
            IPaginatedSearchService paginatedSearch,
            IMyPupilListService mplService,
            ITextSearchSelectionManager selectionManager,
            IContentService contentService,
            IDownloadService downloadService,
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

        [Route(Route.PupilPremium.NonUPN)]
        [HttpGet]
        public async Task<IActionResult> NonUpnPupilPremiumDatabase(bool? returnToSearch)
        {
            _logger.LogInformation("Pupil Premium NonUpn GET method called");
            return await Search(returnToSearch);
        }

        [Route(Route.PupilPremium.NonUPN)]
        [HttpPost]
        public async Task<IActionResult> NonUpnPupilPremiumDatabase(
            LearnerTextSearchViewModel model,
            string surnameFilter,
            string middleNameFilter,
            string forenameFilter,
            string searchByRemove,
            [FromQuery] string sortField,
            [FromQuery] string sortDirection,
            bool returned = false,
            bool fail = false,
            bool calledByController = false)
        {
            _logger.LogInformation("Pupil Premium NonUpn POST method called");
            model.ShowHiddenUPNWarningMessage = true;
            return await Search(model, surnameFilter, middleNameFilter, forenameFilter,
                                     searchByRemove, model.PageNumber,
                                     ControllerContext.HttpContext.Request.Query.ContainsKey("pageNumber"),
                                     calledByController,
                                     sortField, sortDirection,
                                     ControllerContext.HttpContext.Request.Query.ContainsKey("reset"));
        }

        #endregion Search

        #region Search Filters

        [Route(Route.PupilPremium.NonUpnDobFilter)]
        [HttpPost]
        public async Task<IActionResult> DobFilter(LearnerTextSearchViewModel model)
        {
            return await DobSearchFilter(model);
        }

        [Route(Route.PupilPremium.NonUPNSurnameFilter)]
        [HttpPost]
        public async Task<IActionResult> SurnameFilter(LearnerTextSearchViewModel model, string surnameFilter)
        {
            return await SurnameSearchFilter(model, surnameFilter);
        }

        [Route(Route.PupilPremium.NonUpnMiddlenameFilter)]
        [HttpPost]
        public async Task<IActionResult> MiddlenameFilter(LearnerTextSearchViewModel model, string middlenameFilter)
        {
            return await MiddlenameSearchFilter(model, middlenameFilter);
        }

        [Route(Route.PupilPremium.NonUpnForenameFilter)]
        [HttpPost]
        public async Task<IActionResult> ForenameFilter(LearnerTextSearchViewModel model, string forenameFilter)
        {
            return await ForenameSearchFilter(model, forenameFilter);
        }

        [Route(Route.PupilPremium.NonUpnGenderFilter)]
        [HttpPost]
        public async Task<IActionResult> GenderFilter(LearnerTextSearchViewModel model)
        {
            return await GenderSearchFilter(model);
        }

        [Route(Route.PupilPremium.NonUpnSexFilter)]
        [HttpPost]
        public async Task<IActionResult> SexFilter(LearnerTextSearchViewModel model)
        {
            return await SexSearchFilter(model);
        }
        #endregion Search Filters

        #region My Pupil List

        [HttpPost]
        [Route("add-pp-nonupn-to-my-pupil-list")]
        public async Task<IActionResult> PPAddToMyPupilList(LearnerTextSearchViewModel model)
        {
            return await AddToMyPupilList(model);
        }

        #endregion My Pupil List

        #region Download

        [Route(Route.PupilPremium.DownloadPupilPremiumFile)]
        [HttpPost]
        public async Task<IActionResult> DownloadPupilPremiumFile(LearnerDownloadViewModel model)
        {
            var userOrganisation = new UserOrganisation
            {
                IsAdmin = User.IsAdmin(),
                IsEstablishment = User.IsOrganisationEstablishment(),
                IsLa = User.IsOrganisationLocalAuthority(),
                IsMAT = User.IsOrganisationMultiAcademyTrust(),
                IsSAT = User.IsOrganisationSingleAcademyTrust()
            };

            var selectedPupil = PupilHelper.CheckIfStarredPupil(model.SelectedPupils) ? RbacHelper.DecryptUpn(model.SelectedPupils) : model.SelectedPupils;
            var sortOrder = new string[] { ValidationHelper.IsValidUpn(selectedPupil) ? selectedPupil : "0" };

            var downloadFile = await _downloadService.GetPupilPremiumCSVFile(new string[] { selectedPupil }, sortOrder, model.TextSearchViewModel.StarredPupilConfirmationViewModel.ConfirmationGiven,
                                                                            AzureFunctionHeaderDetails.Create(User.GetUserId(), User.GetSessionId()), ReturnRoute.NonPupilPremium, userOrganisation).ConfigureAwait(false);

            if (downloadFile == null)
            {
                return RedirectToAction(ApplicationRoute.Error, ApplicationRoute.Home);
            }

            if (downloadFile.Bytes != null)
            {
                model.ErrorDetails = null;
                return SearchDownloadHelper.DownloadFile(downloadFile);
            }
            else
            {
                model.ErrorDetails = DownloadErrorMessages.NoDataForSelectedPupils;
            }
            return RedirectToAction(Global.PupilPremiumNonUpnAction, Global.PupilPremiumNonUpnController);
        }

        [Route(Route.PupilPremium.LearnerTextDownloadRequest)]
        public async Task<IActionResult> ToDownloadSelectedPupilPremiumDataUPN(LearnerTextSearchViewModel model)
        {
            SetSelections(
            model.PageLearnerNumbers.Split(','),
            model.SelectedPupil);

            var selectedPupil = GetSelected();

            if (string.IsNullOrEmpty(selectedPupil))
            {
                model.ErrorDetails = DownloadErrorMessages.NoPupilSelected;
                model.NoPupil = true;
                model.NoPupilSelected = true;
                return await ReturnToSearch(model);
            }

            if (PupilHelper.CheckIfStarredPupil(selectedPupil) && !model.StarredPupilConfirmationViewModel.ConfirmationGiven)
            {
                PopulateConfirmationNavigation(model.StarredPupilConfirmationViewModel);
                model.StarredPupilConfirmationViewModel.SelectedPupil = selectedPupil;
                return ConfirmationForStarredPupil(model.StarredPupilConfirmationViewModel);
            }

            var searchDownloadViewModel = new LearnerDownloadViewModel
            {
                SelectedPupils = selectedPupil,
                LearnerNumber = selectedPupil,
                ErrorDetails = model.ErrorDetails,
                SelectedPupilsCount = 1,
                DownloadFileType = DownloadFileType.CSV,
                ShowTABDownloadType = false
            };

            return await DownloadPupilPremiumFile(searchDownloadViewModel);
        }

        [Route(Route.PupilPremium.DownloadNonUPNConfirmationReturn)]
        [HttpPost]
        public async Task<IActionResult> DownloadFileConfirmationReturn(StarredPupilConfirmationViewModel model)
        {
            model.ConfirmationError = !model.ConfirmationGiven;
            PopulateConfirmationNavigation(model);

            if (model.ConfirmationGiven)
            {
                var searchDownloadViewModel = new LearnerDownloadViewModel
                {
                    SelectedPupils = model.SelectedPupil,
                    LearnerNumber = model.SelectedPupil,
                    ErrorDetails = "No Confirmation Given for Starred Pupil",
                    SelectedPupilsCount = 1,
                    DownloadFileType = DownloadFileType.CSV,
                    ShowTABDownloadType = false
                };

                return await DownloadPupilPremiumFile(searchDownloadViewModel);
            }

            return ConfirmationForStarredPupil(model);
        }

        [Route(Route.PupilPremium.DownloadCancellationReturn)]
        [HttpPost]
        public async Task<IActionResult> DownloadCancellationReturn(StarredPupilConfirmationViewModel model)
        {
            return await Search(true);
        }

        private void PopulateConfirmationNavigation(StarredPupilConfirmationViewModel model)
        {
            model.DownloadType = DownloadType.PupilPremium;
            model.ConfirmationReturnController = SearchController;
            model.ConfirmationReturnAction = Global.PupilPremiumDownloadConfirmationReturnAction;
            model.CancelReturnController = SearchController;
            model.CancelReturnAction = Global.PupilPremiumDownloadCancellationReturnAction;
        }

        #endregion Download

        #region Invalid UPNs

        [HttpPost]
        [Route(Route.PPNonUpnInvalidUPNs)]
        public async Task<IActionResult> PPNonUpnInvalidUPNs(InvalidLearnerNumberSearchViewModel model)
        {
            return await InvalidUPNs(model);
        }

        [HttpPost]
        [Route(Route.PPNonUpnInvalidUPNsConfirmation)]
        public async Task<IActionResult> PPNonUpnInvalidUPNsConfirmation(InvalidLearnerNumberSearchViewModel model)
        {
            return await InvalidUPNsConfirmation(model);
        }

        #endregion Invalid UPNs
    }
}