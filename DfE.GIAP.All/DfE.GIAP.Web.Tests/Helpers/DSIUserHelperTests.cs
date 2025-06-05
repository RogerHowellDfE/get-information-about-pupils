using DfE.GIAP.Common.Constants.DsiConfiguration;
using DfE.GIAP.Web.Helpers.DSIUser;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace DfE.GIAP.Web.Tests.Helpers
{
    public class DSIUserHelperTests
    {
        [Theory]
        [InlineData(true, false, false, Role.Admin)]
        [InlineData(false, true, false, Role.Approver)]
        [InlineData(false, false, true, Role.User)]
        [InlineData(true, true, true, Role.Admin)]
        [InlineData(false, true, true, Role.Approver)]
        [InlineData(false, false, false, "")]
        public void GetGIAPUserRole_correctly_returns_role_name(bool isAdmin, bool isApprover, bool isUser, string name)
        {
            Assert.Equal(name, DSIUserHelper.GetGIAPUserRole(isAdmin, isApprover, isUser));
        }

        [Theory]
        [InlineData(OrganisationCategory.Establishment, "Establishment")]
        [InlineData(OrganisationCategory.LocalAuthority, "LocalAuthority")]
        [InlineData(OrganisationCategory.MultiAcademyTrust, "MultiAcademyTrust")]
        [InlineData(OrganisationCategory.SingleAcademyTrust, "SingleAcademyTrust")]
        [InlineData(OrganisationCategory.FurtherEducation, "FurtherEducation")]
        [InlineData("", "")]
        public void GetOrganisationType_correctly_returns_organisation_name(string organisationCategoryId, string name)
        {
            Assert.Equal(name, DSIUserHelper.GetOrganisationType(organisationCategoryId));
        }
    }
}
