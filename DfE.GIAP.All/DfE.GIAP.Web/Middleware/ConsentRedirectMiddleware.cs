using DfE.GIAP.Common.Helpers.CookieManager;
using DfE.GIAP.Web.Helpers.Consent;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Http.Features;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace DfE.GIAP.Web.Middleware
{
    public class ConsentRedirectMiddleware
    {
        private readonly RequestDelegate _next;

        public ConsentRedirectMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var request = context.Request;
            var response = context.Response;

            if (context.User.Identity.IsAuthenticated)
            {

                var endpoint = context.Features.Get<IEndpointFeature>()?.Endpoint;
                var attribute = endpoint?.Metadata.GetMetadata<AllowWithoutConsentAttribute>();
                if (attribute == null)
                {
                    if (!ConsentHelper.HasGivenConsent(context))
                    {
                        response.Redirect($"/Home");
                        return;
                    }
                }
            }

            await _next(context);
        }
    }

    [ExcludeFromCodeCoverage]
    public static class ConsentRequestExtensions
    {
        public static IApplicationBuilder UseConsentCheck(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ConsentRedirectMiddleware>();
        }
    }
}
