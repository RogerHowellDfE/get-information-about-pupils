using DfE.GIAP.Common.Constants;
using DfE.GIAP.Domain.Models.Search;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace DfE.GIAP.Common.Helpers.Rbac
{
    public static class RbacHelper
    {
        public static List<T> CheckRbacRulesGeneric<T>(List<T> results, int statutoryLowAge, int statutoryHighAge, DateTime? from = null)
           where T : IRbac
        {
            // Rbac rules don't apply
            if (statutoryLowAge == 0 && statutoryHighAge == 0)
            {
                return results;
            }

            foreach (var item in results)
            {
                if (item.DOB != null)
                {
                    var age = CalculateAge(item.DOB.Value, from);

                    if ((age < statutoryLowAge || age > statutoryHighAge) && item.LearnerNumber != null)
                    {
                        item.LearnerNumberId = EncryptUpn(item.LearnerNumberId);
                        item.LearnerNumber = Global.UpnMask;
                    }
                }
                else
                {
                    item.LearnerNumberId = EncryptUpn(item.LearnerNumberId);
                    item.LearnerNumber = Global.UpnMask;
                }
            }

            return results;
        }

        public static string EncryptUpn(string learnerNumber)
        {
            var bytes = Encoding.UTF8.GetBytes(learnerNumber);

            var encodedString = Convert.ToBase64String(bytes);

            return encodedString + Global.EncryptedMarker;
        }

        public static string DecryptUpn(string learnerNumber)
        {
            if (string.IsNullOrEmpty(learnerNumber))
                return string.Empty;

            learnerNumber = learnerNumber.Replace(Global.EncryptedMarker, "");
            var bytes = Convert.FromBase64String(learnerNumber);

            var decodedString = Encoding.UTF8.GetString(bytes);

            return decodedString;
        }

        public static IEnumerable<string> DecryptUpnCollection(IEnumerable<string> learnerNumbers)
        {
            var unencryptedLearnerNumbers = learnerNumbers.Where(l => !l.Contains(Global.EncryptedMarker));
            var decryptedLearnerNumbers = from learner in learnerNumbers
                                          where learner.Contains(Global.EncryptedMarker)
                                          select RbacHelper.DecryptUpn(learner);

            var unionPageLearnerNumbers = unencryptedLearnerNumbers.Union(decryptedLearnerNumbers);

            return unionPageLearnerNumbers;
        }

        public static int CalculateAge(DateTime dob, DateTime? from = null)
        {
            var dateCalc = DateTime.Today;
            if (from != null) dateCalc = from.Value;

            var age = dateCalc.Year - dob.Year;
            if (dob.Date > dateCalc.AddYears(-age)) age--;

            return age;
        }
    }
}