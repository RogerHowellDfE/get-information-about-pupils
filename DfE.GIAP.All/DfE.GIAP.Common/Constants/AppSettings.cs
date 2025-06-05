namespace DfE.GIAP.Common.Constants
{
    public static class AppSettings
    {
        public const string DsiAudience = "DsiAudience";
        public const string DsiAuthorisationUrl = "DsiAuthorisationUrl";
        public const string DsiClientId = "DsiClientId";
        public const string DsiClientSecret = "DsiClientSecret";
        public const string DsiApiClientSecret = "DsiApiClientSecret";
        public const string DsiMetadataAddress = "DsiMetadataAddress";
        public const string DsiServiceId = "DsiServiceId";
        public const string RSAPrivateKey = "RSAPrivateKey";

        public const string DsiScopeOpenId = "openid";
        public const string DsiScopeEmail = "email";
        public const string DsiScopeProfile = "profile";
        public const string DsiScopeOrganisationId = "organisationid";

        public const string DsiCallbackPath = "/auth/cb";
        public const string DsiSignedOutCallbackPath = "/signout/complete";
        public const string DsiLogoutPath = "/auth/logout";

        public const string SessionTimeout = "SessionTimeout";
    }
}