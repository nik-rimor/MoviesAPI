using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MoviesAPI.Helpers
{
    public static class HttpContextExtensions
    {
        public static void InsertPaginationParametersInResponse(this HttpContext httpContext, int totalAmountPages)
        {
            if (httpContext == null) { throw new ArgumentNullException(nameof(httpContext)); }
            httpContext.Response.Headers.Add("totalAmountPages", totalAmountPages.ToString());
        }
    }
}
