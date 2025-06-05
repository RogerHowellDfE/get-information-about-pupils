using DfE.GIAP.Common.AppSettings;
using DfE.GIAP.Common.Constants;
using DfE.GIAP.Common.Constants.Messages.Downloads;
using DfE.GIAP.Common.Constants.Messages.Search;
using DfE.GIAP.Common.Constants.Routes;
using DfE.GIAP.Common.Enums;
using DfE.GIAP.Domain.Models.Common;
using DfE.GIAP.Service.Content;
using DfE.GIAP.Service.Download;
using DfE.GIAP.Service.MPL;
using DfE.GIAP.Service.Search;
using DfE.GIAP.Web.Extensions;
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

namespace DfE.GIAP.Web.Controllers.TextBasedSearch
{
    [Route(ApplicationRoute.Search)]
    public class FELearnerTextSearchController : BaseLearnerTextSearchController
    {
        #region abstract property implementation

        public override string PageHeading => ApplicationLabel.SearchFEWithoutUlnPageHeading;
        public override string SearchSessionKey => Global.FurtherEducationNonUlnSearchSessionKey;
        public override string SearchFiltersSessionKey => Global.FurtherEducationNonUlnSearchFiltersSessionKey;
        public override string SortDirectionKey => Global.FurtherEducationNonUlnSortDirectionSessionKey;
        public override string SortFieldKey => Global.FurtherEducationNonUlnSortFieldSessionKey;
        public override string DownloadLinksPartial => Global.FurtherEducationNonUlnDownloadLinksView;
        public override string SearchLearnerNumberAction => Route.FurtherEducation.LearnerNumberSearch;
        public override string RedirectUrlFormAction => Global.FurtherEducationLearnerTextSearchAction;
        public override string LearnerTextDatabaseAction => Global.FurtherEducationLearnerTextSearchAction;
        public override string LearnerTextDatabaseName => Global.FurtherEducationLearnerTextSearchAction;
        public override string RedirectFrom => Route.FurtherEducation.LearnerTextSearch;

        #region Search Filter Properties

        public override string SurnameFilterUrl => Route.FurtherEducation.NonULNSurnameFilter;
        public override string DobFilterUrl => Route.FurtherEducation.NonULNDobFilter;
        public override string ForenameFilterUrl => Route.FurtherEducation.NonULNForenameFilter;
        public override string MiddlenameFilterUrl => "";
        public override string GenderFilterUrl => Route.FurtherEducation.NonULNGenderFilter;
        public override string SexFilterUrl => Route.FurtherEducation.NonULNSexFilter;

        public override string FormAction => Route.FurtherEducation.LearnerTextSearch;
        public override string RemoveActionUrl => $"/{ApplicationRoute.Search}/{Route.FurtherEducation.LearnerTextSearch}";
        public override AzureSearchIndexType IndexType => AzureSearchIndexType.FurtherEducation;
        public override string SearchView => Global.NonUpnSearchView;

        #endregion Search Filter Properties

        public override string SearchLearnerNumberController => ApplicationRoute.Search;
        public override int MyPupilListLimit => _appSettings.NonUpnNPDMyPupilListLimit; //Not valid for FE so arbitrarily set to default non UPN limit
        public override string SearchAction => Global.FurtherEducationLearnerTextSearchAction;
        public override string SearchController => Global.FurtherEducationLearnerTextSearchController;
        public override ReturnRoute ReturnRoute => ReturnRoute.NonUniqueLearnerNumber;
        public override string LearnerTextSearchController => Global.FurtherEducationLearnerTextSearchController;
        public override string LearnerTextSearchAction => Global.FurtherEducationLearnerTextSearchAction;
        public override string LearnerNumberAction => Route.NationalPupilDatabase.NationalPupilDatabaseLearnerNumber;

        public override bool ShowGender => _appSettings.FeUseGender;
        public override bool ShowLocalAuthority => false;
        public override string InvalidUPNsConfirmationAction => "";
        public override string LearnerNumberLabel => Global.FurtherEducationLearnerNumberLabel;
        public override bool ShowMiddleNames => false;

        public override string DownloadSelectedLink => ApplicationLabel.DownloadSelectedFurtherEducationLink;

        #endregion abstract property implementation

        private readonly IDownloadService _downloadService;
        private readonly AzureAppSettings _appSettings;
        private readonly ILogger<FELearnerTextSearchController> _logger;

        public FELearnerTextSearchController(ILogger<FELearnerTextSearchController> logger,
           IPaginatedSearchService paginatedSearch,
           IMyPupilListService mplService,
           ITextSearchSelectionManager selectionManager,
           IContentService contentService,
           IDownloadService downloadService,
           IOptions<AzureAppSettings> azureAppSettings)
           : base(logger,
                 paginatedSearch,
                 mplService,
                 selectionManager,
                 contentService,
                 azureAppSettings)
        {
            _logger = logger ??
                throw new ArgumentNullException(nameof(logger));
            _downloadService = downloadService ??
                throw new ArgumentNullException(nameof(downloadService));
            _appSettings = azureAppSettings.Value;
        }

        #region Search

        [Route(Route.FurtherEducation.LearnerTextSearch)]
        [HttpGet]
        public async Task<IActionResult> FurtherEducationNonUlnSearch(bool? returnToSearch)
        {
            _logger.LogInformation("Further education non ULN search GET method called");
            return await Search(returnToSearch);
        }

        [Route(Route.FurtherEducation.LearnerTextSearch)]
        [HttpPost]
        public async Task<IActionResult> FurtherEducationNonUlnSearch(
            LearnerTextSearchViewModel model,
            string surnameFilter,
            string middlenameFilter,
            string forenameFilter,
            string searchByRemove,
            [FromQuery] string sortField,
            [FromQuery] string sortDirection,
            bool calledByController = false)
        {
            _logger.LogInformation("Further education non ULN search POST method called");
            model.ShowHiddenUPNWarningMessage = false;
            return await Search(model, surnameFilter, middlenameFilter, forenameFilter,
                                     searchByRemove, model.PageNumber,
                                     ControllerContext.HttpContext.Request.Query.ContainsKey("pageNumber"),
                                     calledByController, sortField, sortDirection,
                                     ControllerContext.HttpContext.Request.Query.ContainsKey("reset"));
        }

        #endregion Search

        #region Search Filters

        [Route(Route.FurtherEducation.NonULNDobFilter)]
        [HttpPost]
        public async Task<IActionResult> DobFilter(LearnerTextSearchViewModel model)
        {
            return await DobSearchFilter(model);
        }

        [Route(Route.FurtherEducation.NonULNSurnameFilter)]
        [HttpPost]
        public async Task<IActionResult> SurnameFilter(LearnerTextSearchViewModel model, string surnameFilter)
        {
            return await SurnameSearchFilter(model, surnameFilter);
        }

        [Route(Route.FurtherEducation.NonULNForenameFilter)]
        [HttpPost]
        public async Task<IActionResult> ForenameFilter(LearnerTextSearchViewModel model, string forenameFilter)
        {
            return await ForenameSearchFilter(model, forenameFilter);
        }

        [Route(Route.FurtherEducation.NonULNGenderFilter)]
        [HttpPost]
        public async Task<IActionResult> GenderFilter(LearnerTextSearchViewModel model)
        {
            return await GenderSearchFilter(model);
        }

        [Route(Route.FurtherEducation.NonULNSexFilter)]
        [HttpPost]
        public async Task<IActionResult> SexFilter(LearnerTextSearchViewModel model)
        {
            return await SexSearchFilter(model);
        }
        #endregion Search Filters

        #region Download

        [Route(Route.DownloadSelectedNationalPupilDatabaseData)]
        [HttpPost]
        public async Task<IActionResult> DownloadSelectedFurtherEducationData(
            string selectedPupil,
            string searchText)
        {
            var searchDownloadViewModel = new LearnerDownloadViewModel
            {
                SelectedPupils = selectedPupil,
                LearnerNumber = selectedPupil,
                ErrorDetails = (string)TempData["ErrorDetails"],
                SelectedPupilsCount = 1,
                DownloadFileType = DownloadFileType.CSV,
                ShowTABDownloadType = false
            };

            if (IndexType == AzureSearchIndexType.FurtherEducation)
            {
                SearchDownloadHelper.AddUlnDownloadDataTypes(searchDownloadViewModel, User, User.GetOrganisationHighAge(), User.IsDfeUser());
            }
            else
            {
                SearchDownloadHelper.AddDownloadDataTypes(searchDownloadViewModel, User, User.GetOrganisationLowAge(), User.GetOrganisationHighAge(), User.IsOrganisationLocalAuthority(), User.IsOrganisationAllAges());
            }

            ModelState.Clear();

            searchDownloadViewModel.TextSearchViewModel.PageLearnerNumbers = selectedPupil;
            searchDownloadViewModel.SearchAction = Global.FurtherEducationLearnerTextSearchAction;
            searchDownloadViewModel.DownloadRoute = Route.FurtherEducation.DownloadNonUlnFile;
            searchDownloadViewModel.RedirectRoute = Route.FurtherEducation.LearnerTextSearch;
            searchDownloadViewModel.TextSearchViewModel = new LearnerTextSearchViewModel() { LearnerNumberLabel = LearnerNumberLabel, SearchText = searchText };
            PopulateNavigation(searchDownloadViewModel.TextSearchViewModel);

            var downloadTypeArray = searchDownloadViewModel.SearchDownloadDatatypes.Select(d => d.Value).ToArray();
            var disabledTypes = await _downloadService.CheckForFENoDataAvailable(new string[] { selectedPupil }, downloadTypeArray, AzureFunctionHeaderDetails.Create(User.GetUserId(), User.GetSessionId())).ConfigureAwait(false);
            SearchDownloadHelper.DisableDownloadDataTypes(searchDownloadViewModel, disabledTypes);

            searchDownloadViewModel.SearchResultPageHeading = PageHeading;
            return View(Global.NonLearnerNumberDownloadOptionsView, searchDownloadViewModel);
        }

        [Route(Route.FurtherEducation.DownloadNonUlnFile)]
        [HttpPost]
        public async Task<IActionResult> DownloadFurtherEducationFile(LearnerDownloadViewModel model)
        {
            if (!string.IsNullOrEmpty(model.LearnerNumber))
            {
                var selectedPupils = model.LearnerNumber.Split(',');

                if (model.SelectedDownloadOptions == null)
                {
                    model.ErrorDetails = SearchErrorMessages.SelectOneOrMoreDataTypes;
                }
                else if (model.DownloadFileType != DownloadFileType.None)
                {
                    var downloadFile = await _downloadService.GetFECSVFile(selectedPupils, model.SelectedDownloadOptions, true, AzureFunctionHeaderDetails.Create(User.GetUserId(), User.GetSessionId()), ReturnRoute.NonUniqueLearnerNumber).ConfigureAwait(false);

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
                }
                else
                {
                    model.ErrorDetails = SearchErrorMessages.SelectFileType;
                }

                TempData["ErrorDetails"] = model.ErrorDetails;

                if (this.HttpContext.Session.Keys.Contains(SearchSessionKey))
                    model.TextSearchViewModel.SearchText = this.HttpContext.Session.GetString(SearchSessionKey);

                return await DownloadSelectedFurtherEducationData(model.SelectedPupils, model.TextSearchViewModel?.SearchText);
            }

            return RedirectToAction(Global.FurtherEducationLearnerTextSearchAction, Global.FurtherEducationLearnerTextSearchController);
        }

        [Route(Route.FurtherEducation.DownloadNonUlnRequest)]
        [HttpPost]
        public async Task<IActionResult> ToDownloadSelectedFEDataULN(LearnerTextSearchViewModel model)
        {
            SetSelections(
                model.PageLearnerNumbers.Split(','),
                model.SelectedPupil);

            var selectedPupil = GetSelected();

            if (string.IsNullOrEmpty(selectedPupil))
            {
                model.NoPupil = true;
                model.NoPupilSelected = true;
                model.ErrorDetails = DownloadErrorMessages.NoLearnerSelected;
                return await FurtherEducationNonUlnSearch(model, null, null, null, null, null, null);
            }

            return await DownloadSelectedFurtherEducationData(selectedPupil, model.SearchText);
        }

        #endregion Download
    }
}