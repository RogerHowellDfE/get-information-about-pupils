namespace DfE.GIAP.Common.Constants.Routes
{
    public static class SecurityReports
    {
        public const string SecurityReportsByUpnUln = "security-reports-by-upn-uln";
        public const string SecurityReportsBySchool = "admin/security-reports-by-school";
        public const string SecurityReportsBySchoolDownload = "admin/security-reports-by-school-download";
        public const string SecurityReportsBySchoolConfirmation = "admin/security-reports-by-school-confirmation";
        public const string SecurityReportsBySchoolGetDownload = "admin/security-reports-by-school-get-download/{reportType}/{estabCode}";
        public const string AdminOptions = "admin/admin-options";
        public const string SecurityReportsBySchoolSchoolCollegeDownload = "admin/school-college-download";
        public const string SecurityReportsBySchoolEstablishmentSelection = "admin/establishment-selection";
        public const string SecurityReportsByOrganisation = "admin/security-reports-for-your-organisation-school";
        public const string SecurityReportsByOrganisationDownload = "admin/security-reports-for-your-organisation-download";
        public const string SecurityReportsByUpnNoDataReturn = "SecurityReportsByUpnNoDataReturn";

        public static class Names
        {
            public const string GetSecurityReportDownload = "GetSRDownload";
            public const string SRConfirmation = "SRConfirmation";
        }
    }
}
