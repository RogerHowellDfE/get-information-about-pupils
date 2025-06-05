namespace DfE.GIAP.Common.Constants
{
    public static class Global
    {
        public const string BaseViewModel = "BaseViewModel";

        public const string GiapComms = "giapcomms";
        public const string GiapWebsiteUse = "giapwebsiteuse";
        public const int YearMinLimit = 1972;

        public const string NewsBannerStatus = "NewsBannerStatus";

        public const string StatusTrue = "true";
        public const string StatusFalse = "false";

        public const string UserAccount = "someuser";
        public const string UserName = "someUsername";

        public const string ContactLinkURL = "https://form.education.gov.uk/en/AchieveForms/?form_uri=sandbox-publish://AF-Process-2b61dfcd-9296-4f6a-8a26-4671265cae67/AF-Stage-f3f5200e-e605-4a1b-ae6b-3536bc77305c/definition.json&redirectlink=%2Fen&cancelRedirectLink=%2Fen";

        public const string UpnMask = "*************";
        public const string EncryptedMarker = "-GIAP";


        #region NPD

        public const string LearnerNumberLabel = "UPN";
        public const string SearchControllerName = "Search";
        public const string NPDLearnerNumberSearchController = "NPDLearnerNumberSearch";
        public const string NPDLearnerNumberSearchAction = "NationalPupilDatabase";
        public const string NPDLearnerTextSearchDatabaseName = "NonUpnDatabaseName";
        public const string NonUpnSearchView = "~/Views/Shared/LearnerText/Search.cshtml";

        public const string NationalPupilDatabaseTextSearchController = "NPDLearnerTextSearch";
        public const string NationalPupilDatabaseAction = "NationalPupilDatabase";
        public const string NationalPupilDatabaseNonUpnAction = "NonUpnNationalPupilDatabase";
        public const string NationalPupilDatabaseNonUpnSearchSessionKey = "SearchNonUPN_SearchText";
        public const string NationalPupilDatabaseNonUpnSearchFiltersSessionKey = "SearchNonUPN_SearchFilters";

        //TODO: shorten names
        public const string NationalPupilDatabaseNonUpnSortDirectionSessionKey = "SearchNonUPN_SortDirection";

        public const string NationalPupilDatabaseNonUpnSortFieldSessionKey = "SearchNonUPN_SortField";

        public const string NPDDownloadConfirmationReturnAction = "DownloadFileConfirmationReturn";
        public const string NPDDownloadCancellationReturnAction = "DownloadCancellationReturn";
        public const string NPDCTFDownloadConfirmationReturnAction = "DownloadNpdCommonTransferFileDataReturn";
        public const string NationalPupilDatabaseNonUpnInvalidUPNsConfirmation = "NonUpnInvalidUPNsConfirmation";
        public const string NationalPupilDatabaseView = "~/Views/Search/Upn/NationalPupilDatabase.cshtml";
        public const string StarredPupilConfirmationView = "~/Views/Search/Common/_StarredPupilConfirmation.cshtml";
        public const string NationalPupilDatabaseNonUpnDownloadLinksView = "~/Views/Shared/LearnerText/_SearchPageDownloadLinks.cshtml";

        #endregion NPD

        #region Pupil Premium

        //TODO: shorten names.
        public const string PupilPremiumNonUpnSearchSessionKey = "SearchPPNonUPN_SearchText";

        public const string PupilPremiumNonUpnSearchFiltersSessionKey = "SearchPPNonUPN_SearchFilters";
        public const string PupilPremiumNonUpnSortDirectionSessionKey = "SearchPPNonUPN_SortDirection";
        public const string PupilPremiumNonUpnSortFieldSessionKey = "SearchPPNonUPN_SortField";
        public const string PupilPremiumAction = "PupilPremium";
        public const string PupilPremiumNonUpnController = "PPLearnerTextSearch";
        public const string PupilPremiumNonUpnAction = "NonUpnPupilPremiumDatabase";
        public const string PPLearnerTextSearchDatabaseName = "NonUpnDatabaseName";
        public const string PPLearnerNumberSearchController = "PupilPremiumLearnerNumber";
        public const string PupilPremiumDownloadConfirmationReturnAction = "DownloadFileConfirmationReturn";
        public const string PupilPremiumDownloadCancellationReturnAction = "DownloadCancellationReturn";
        public const string PupilPremiumNonUpnInvalidUPNsConfirmation = "PPNonUpnInvalidUPNsConfirmation";
        public const string PupilPremiumView = "~/Views/Search/Upn/PupilPremium.cshtml";
        public const string PupilPremiumNonUpnDownloadLinksView = "~/Views/Shared/LearnerText/_SearchPupilPremiumDownloadLinks.cshtml";

        #endregion Pupil Premium

        #region Further Eduction

        //TODO: shorten names
        public const string FurtherEducationLearnerNumberLabel = "ULN";

        public const string FurtherEducationNonUlnSearchSessionKey = "SearchNonULN_SearchText";
        public const string FurtherEducationNonUlnSearchFiltersSessionKey = "SearchNonULN_SearchFilters";
        public const string FurtherEducationNonUlnSortDirectionSessionKey = "SearchNonULN_SortDirection";
        public const string FurtherEducationNonUlnSortFieldSessionKey = "SearchNonULN_SortField";
        public const string FurtherEducationLearnerNumberSearchController = "FELearnerNumber";
        public const string FurtherEducationLearnerNumberSearchAction = "PupilUlnSearch";
        public const string FurtherEducationLearnerTextSearchController = "FELearnerTextSearch";
        public const string FurtherEducationLearnerTextSearchAction = "FurtherEducationNonUlnSearch";
        public const string FurtherEducationNonUlnDownloadLinksView = "~/Views/Shared/LearnerText/_SearchFurtherEducationDownloadLinks.cshtml";

        #endregion Further Eduction

        #region My Pupil List

        public const string MyPupilListControllerName = "SearchMyPupilList";
        public const string MyPupilListAction = "MyPupilList";
        public const string AddToPupilList = "AddToPupilList";
        public const string MyPupilListDownloadConfirmationReturnAction = "DownloadFileConfirmationReturn";
        public const string MyPupilListDownloadCancellationReturnAction = "DownloadCancellationReturn";

        #endregion My Pupil List

        #region Invalid UPNs

        public const string InvalidUPNConfirmation_ReturnToSearch = "ReturnToSearch";
        public const string InvalidUPNConfirmation_MyPupilList = "GoToMyPupilList";

        #endregion Invalid UPNs

        #region Number Search

        public const string SearchView = "~/Views/Shared/LearnerNumber/Search.cshtml";
        public const string InvalidUPNsView = "~/Views/Shared/LearnerNumber/InvalidUPNs.cshtml";

        public const string LearnerNumberSearchBoxView = "../Shared/LearnerNumber/_SearchBox";
        public const string LearnerNumberSearchBoxViewDownloads = "../Shared/LearnerNumber/_SearchBoxDownloads";
        public const string DownloadNPDOptionsView = "../Shared/LearnerNumber/DownloadOptions";

        #endregion Number Search

        #region Text Search

        public const string MPLDownloadNPDOptionsView = "../Search/MyPupilList/DownloadOptions";
        public const string NonLearnerNumberDownloadOptionsView = "../Shared/LearnerText/DownloadOptions";
        public const string LearnerTextControllerName = "BaseLearnerTextSearch";

        #endregion Text Search
    }
}