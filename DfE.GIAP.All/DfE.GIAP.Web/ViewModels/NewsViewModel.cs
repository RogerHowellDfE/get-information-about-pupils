using DfE.GIAP.Core.Models.Common;
using DfE.GIAP.Core.Models.News;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace DfE.GIAP.Web.ViewModels
{
    [ExcludeFromCodeCoverage]
    public class NewsViewModel
    {
        public List<Article> Articles { get; set; }

        public CommonResponseBody NewsPublication { get; set; }

        public CommonResponseBody NewsMaintenance { get; set; }

        public static explicit operator NewsViewModel(List<Article> articles)
        {
            return new NewsViewModel { Articles = articles };
        }
    }
}
