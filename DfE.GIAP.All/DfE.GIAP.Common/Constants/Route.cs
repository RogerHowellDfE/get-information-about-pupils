namespace DfE.GIAP.Common.Constants
{
    public static class Route
    {
        //Search Route
        public const string DownloadSelectedNationalPupilDatabaseData = "download";

        public const string NPDNonUpnAddToMyPupilList = "add-npd-nonupn-to-my-pupil-list";

        public const string NPDInvalidUPNs = "npd-invalid-upns";
        public const string NPDInvalidUPNsConfirmation = "npd-invalid-upns-confirmation";
        public const string NPDNonUpnInvalidUPNs = "npd-nonupn-invalid-upns";
        public const string NPDNonUpnInvalidUPNsConfirmation = "npd-nonupn-invalid-upns-confirmation";
        public const string PPInvalidUPNs = "pp-invalid-upns";
        public const string PPInvalidUPNsConfirmation = "pp-invalid-upns-confirmation";
        public const string PPNonUpnInvalidUPNs = "pp-nonupn-invalid-upns";
        public const string PPNonUpnInvalidUPNsConfirmation = "pp-nonupn-invalid-upns-confirmation";

        public const string InvalidUPNsReturn = "InvalidUPNsReturn";

        public static class SearchMyPupilList
        {
            public const string AddToMyPupilList = "AddToMyPupilList";
            public const string RemoveSelected = "RemoveSelected";
            public const string DownloadNonUPNConfirmationReturn = "mpl-nonupn-starred-pupil-confirmation";
            public const string DownloadCancellationReturn = "mpl-starred-pupil-cancellation";
            public const string MyPupilListView = "~/Views/Search/MyPupilList/MyPupilList.cshtml";
            public const string MyPupilListViewConfirmation = "~/Views/Search/MyPupilList/PupilListConfirmation.cshtml";
        }

        public static class NationalPupilDatabase
        {
            public const string NationalPupilDatabaseLearnerNumber = "national-pupil-database";
            public const string NationalPupilDatabaseNonUPN = "npd-nonupn";
            public const string NationalPupilDatabaseLearnerNumberWithPage = "national-pupil-database/{pageNumber:int?}";
            public const string LearnerNumberDownloadFile = "download-file";
            public const string LearnerNumberDownloadRequest = "download-upn";
            public const string LearnerTextDataDownloadRequest = "download-non-upn";
            public const string LearnerTextDownloadOptions = "non-upn-download";
            public const string LearnerTextDownloadFile = "download-file-non-upn";
            public const string DownloadCTFData = "npd-non-upn-download-ctf";
            public const string DownloadNonUPNConfirmationReturn = "mpl-nonupn-starred-pupil-confirmation";
            public const string DownloadCancellationReturn = "mpl-starred-pupil-cancellation";
            public const string NonUpnDobFilter = "/search/npd-nonupn/dob-filter";
            public const string NonUpnGenderFilter = "/search/npd-nonupn/gender-filter";
            public const string NonUpnSexFilter = "/search/npd-nonupn/sex-filter";
            public const string NonUpnForenameFilter = "/search/npd-nonupn/forename-filter";
            public const string NonUpnMiddlenameFilter = "/search/npd-nonupn/middlename-filter";
            public const string NonUpnSurnameFilter = "/search/npd-nonupn/surname-filter";
            public const string NonUpnNationalPupilDatabaseReturn = "NonUpnNationalPupilDatabaseReturn";
        }

        public static class PupilPremium
        {
            public const string PupilPremiumDatabase = "pupil-premium";
            public const string NonUPN = "pp-nonupn";
            public const string NonUPNSurnameFilter = "/search/pp-nonupn/surname-filter";
            public const string NonUpnDobFilter = "/search/pp-nonupn/dob-filter";
            public const string NonUpnGenderFilter = "/search/pp-nonupn/gender-filter";
            public const string NonUpnSexFilter = "/search/pp-nonupn/sex-filter";
            public const string NonUpnForenameFilter = "/search/pp-nonupn/forename-filter";
            public const string NonUpnMiddlenameFilter = "/search/pp-nonupn/middlename-filter";
            public const string DownloadSelectedPupilPremiumDatabaseData = "download";
            public const string DownloadSelectedPupilPremiumNonUpnDatabaseData = "download-pupilpremium-nonpun";
            public const string LearnerNumberDownloadRequest = "download-pp-upn";
            public const string LearnerTextDownloadRequest = "download-pp-nonupn";
            public const string DownloadPupilPremiumFile = "pp-download-file";
            public const string NonUpnPupilPremiumDatabaseReturn = "NonUpnPupilPremiumDatabaseReturn";
            public const string DownloadNonUPNConfirmationReturn = "pp-nonupn-starred-pupil-confirmation";
            public const string DownloadCancellationReturn = "pp-nonupn-starred-pupil-cancellation";
        }

        public static class FurtherEducation
        {
            public const string LearnerNumberSearch = "pupil-uln";
            public const string LearnerTextSearch = "pupil-non-uln";
            public const string NonULNSurnameFilter = "/search/fe-non-uln/surname-filter";
            public const string NonULNForenameFilter = "/search/fe-non-uln/forename-filter";
            public const string NonULNDobFilter = "/search/fe-non-uln/dob-filter";
            public const string NonULNGenderFilter = "/search/fe-non-uln/gender-filter";
            public const string NonULNSexFilter = "/search/fe-non-uln/sex-filter";
            public const string DownloadNonUlnRequest = "download-nonuln-fe";
            public const string DownloadNonUlnFile = "download-nonuln-fe-file";
        }

        public static class DownloadLinks
        {
            public const string NonUPNSearchDownloadLink = "npd-nonupn/download-link";
        }
    }
}