using DfE.GIAP.Common.Constants.DsiConfiguration;
using Microsoft.AspNetCore.Http;

namespace DfE.GIAP.Web.Helpers.DSIUser
{
    public static class DSIUserHelper
    {
        public static string GetGIAPUserRole(bool isAdmin, bool isApprover, bool isUser)
        {
            if (isAdmin)
                return Role.Admin;
            if (isApprover)
                return Role.Approver;
            if (isUser)
                return Role.User;
            return string.Empty;

        }

        public static string GetOrganisationType(string organisationCategoryId)
        {
            return organisationCategoryId switch
            {
                OrganisationCategory.Establishment => nameof(OrganisationCategory.Establishment),
                OrganisationCategory.LocalAuthority => nameof(OrganisationCategory.LocalAuthority),
                OrganisationCategory.MultiAcademyTrust => nameof(OrganisationCategory.MultiAcademyTrust),
                OrganisationCategory.SingleAcademyTrust => nameof(OrganisationCategory.SingleAcademyTrust),
                OrganisationCategory.FurtherEducation => nameof(OrganisationCategory.FurtherEducation),
                _ => string.Empty,
            };
        }


    }
}
