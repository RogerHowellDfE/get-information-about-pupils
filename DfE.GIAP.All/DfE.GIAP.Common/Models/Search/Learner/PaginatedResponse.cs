using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace DfE.GIAP.Domain.Search.Learner
{
    [ExcludeFromCodeCoverage]
    public class PaginatedResponse
    {
        public List<Learner> Learners { get; set; } = new List<Learner>();
        public List<FilterData> Filters { get; set; } = new List<FilterData>();
        public long? Count { get; set; }

        public HashSet<string> GetLearnerNumbers()
        {
            var learnerNumbers = new HashSet<string>();

            foreach (var learner in Learners)
            {
                learnerNumbers.Add(learner.LearnerNumber);
            }

            return learnerNumbers;
        }

        public HashSet<string> GetLearnerNumberIds()
        {
            var learnerNumberIds = new HashSet<string>();

            foreach (var learner in Learners)
            {
                learnerNumberIds.Add(learner.LearnerNumberId);
            }

            return learnerNumberIds;
        }

        public string GetLearnerIdsAsString()
        {
            return string.Join(",", this.Learners.Select(l => l.Id));
        }
    }
}