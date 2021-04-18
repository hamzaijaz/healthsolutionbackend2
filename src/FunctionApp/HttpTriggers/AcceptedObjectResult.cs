using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace MyHealthSolution.Service.FunctionApp
{
    /// <summary>
    /// AcceptedObjectResult
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class AcceptedObjectResult : ObjectResult
    {
        private readonly string _location;
        /// <summary>
        /// Initializes a new instance of the &lt;see cref="AcceptedObjectResult"/&gt; class.
        /// </summary>
        /// <param name="location">Route location.</param>
        /// <param name="value">Object to return.</param>
        public AcceptedObjectResult(string location, object value) : base(value)
        {
            _location = location;
        }
        /// <summary>
        /// ExecuteResult
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public override Task ExecuteResultAsync(ActionContext context)
        {
            context.HttpContext.Response.StatusCode = 202;
            var uri = new UriBuilder(context.HttpContext.Request.Scheme, context.HttpContext.Request.Host.Host)
            {
                Path = $@"api/{_location}",
            };
            if (context.HttpContext.Request.Host.Port.HasValue)
            {
                uri.Port = context.HttpContext.Request.Host.Port.Value;
            }

            context.HttpContext.Response.Headers.Add(@"Location", uri.ToString());

            return base.ExecuteResultAsync(context);
        }

    }
}
