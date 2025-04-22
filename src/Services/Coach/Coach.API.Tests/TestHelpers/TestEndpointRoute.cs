using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Coach.API.Tests.TestHelpers
{
    public class TestEndpointRoute
    {
        private readonly Delegate _delegate;

        public TestEndpointRoute(Delegate @delegate)
        {
            _delegate = @delegate;
        }

        public async Task<IResult> InvokeAsync(HttpContext httpContext, ISender sender, params object[] args)
        {
            // Remove any try-catch blocks that might be swallowing exceptions
            // Just directly invoke the delegate
            var combinedArgs = new List<object> { httpContext, sender };
            if (args != null)
            {
                combinedArgs.AddRange(args);
            }

            return await (Task<IResult>)_delegate.DynamicInvoke(combinedArgs.ToArray());
        }
    }
}