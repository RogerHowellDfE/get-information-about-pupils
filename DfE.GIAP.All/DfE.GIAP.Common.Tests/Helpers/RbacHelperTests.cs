using DfE.GIAP.Common.Helpers.Rbac;
using DfE.GIAP.Domain.Models.Search;
using System;
using System.Collections.Generic;
using System.Globalization;
using Xunit;

namespace DfE.GIAP.Common.Tests.Helpers
{
    public class RbacHelperTests
    {
        [Fact]
        public void Crypto_round_trip_works_correctly()
        {
            var value = "This should be equal by the time its gone around the encrypt/decrypt..";

            var encrypted = RbacHelper.EncryptUpn(value);
            var decrypted = RbacHelper.DecryptUpn(encrypted);

            Assert.Equal(value, decrypted);
        }

        [Theory]
        [InlineData("")]
        public void DecryptUpn_ReturnsEmptyWhenPassedEmpty(string value)
        {
            Assert.Equal(RbacHelper.DecryptUpn(value), string.Empty);
        }

        [Theory]
        [InlineData("01/10/2010", "01/10/2022", 12)]
        [InlineData("01/09/2010", "01/10/2022", 12)]
        [InlineData("01/11/2010", "01/10/2022", 11)]
        public void Calculate_age_works_correctly(string strDob, string strToday, int age)
        {
            var dob = DateTime.Parse(strDob, new CultureInfo("en-gb"));
            var today = DateTime.Parse(strToday, new CultureInfo("en-gb"));

            Assert.Equal(age, RbacHelper.CalculateAge(dob, today));
        }

        [Fact]
        public void CheckRbacRulesGeneric_returns_the_list_unchanged_if_the_rules_dont_apply()
        {
            // Arrange
            var testData = GetTestList();

            // Act
            var results = RbacHelper.CheckRbacRulesGeneric<TestRbac>(testData, 0, 0);

            // Assert
            foreach (var result in results)
            {
                Assert.True(!"*************".Equals(result.LearnerNumber));
            }
        }

        [Fact]
        public void CheckRbacRulesGeneric_returns_the_list_with_correct_pupils_starred_out()
        {
            // Arrange
            var testData = GetTestList();

            // Act
            var results = RbacHelper.CheckRbacRulesGeneric<TestRbac>(testData, 3, 11, DateTime.Parse("01/10/2022", new CultureInfo("en-gb")));

            // Assert
            Assert.True(results[0].LearnerNumber.Equals("*************"));
            Assert.True(results[1].LearnerNumber.Equals("*************"));
            Assert.True(results[3].LearnerNumber.Equals("*************"));
        }

        private List<TestRbac> GetTestList()
        {
            var cultureInfo = new CultureInfo("en-gb");
            return new List<TestRbac>()
            {
                new TestRbac()
                {
                    DOB = DateTime.Parse("01/10/2010", cultureInfo),
                    LearnerNumber = "123456789",
                    LearnerNumberId = "123456789"

                },
                new TestRbac()
                {
                    DOB = DateTime.Parse("01/09/2010", cultureInfo),
                    LearnerNumber = "912345678",
                    LearnerNumberId = "912345678"

                },
                new TestRbac()
                {
                    DOB = DateTime.Parse("01/11/2010", cultureInfo),
                    LearnerNumber = "891234567",
                    LearnerNumberId = "891234567"

                },
                new TestRbac()
                {
                    LearnerNumber = "789123456",
                    LearnerNumberId = "789123456"

                }
            };
        }

        private class TestRbac : IRbac
        {
            public DateTime? DOB { get; set; }
            public string LearnerNumber { get; set; }
            public string LearnerNumberId { get; set; }
        }
    }
}
