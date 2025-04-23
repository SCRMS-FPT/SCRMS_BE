using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using MediatR;

namespace Coach.API.Tests.TestHelpers
{
    public class TestEndpointRoute
    {
        private readonly Delegate _delegate;

        public TestEndpointRoute(Delegate @delegate)
        {
            _delegate = @delegate;
        }

        public async Task<IResult> InvokeAsync(HttpContext httpContext, params object[] args)
        {
            try
            {
                // Get the parameter information from the delegate
                var parameters = _delegate.Method.GetParameters();
                var paramList = new List<object>();

                // First handle the standard parameters that almost all endpoints need
                foreach (var param in parameters)
                {
                    if (param.ParameterType == typeof(HttpContext))
                    {
                        paramList.Add(httpContext);
                        continue;
                    }

                    // Handle ISender parameter (MediatR)
                    if (typeof(ISender).IsAssignableFrom(param.ParameterType))
                    {
                        var sender = args.FirstOrDefault(a => a != null && a is ISender);
                        if (sender != null)
                        {
                            paramList.Add(sender);
                            continue;
                        }
                    }

                    // Try to find a matching parameter in the args
                    bool parameterFound = false;
                    foreach (var arg in args)
                    {
                        if (arg != null && param.ParameterType.IsInstanceOfType(arg))
                        {
                            paramList.Add(arg);
                            parameterFound = true;
                            break;
                        }
                    }

                    if (parameterFound) continue;

                    // Try to get parameter from Query if it's a primitive type
                    if (httpContext.Request?.Query != null &&
                        param.Name != null &&
                        httpContext.Request.Query.ContainsKey(param.Name))
                    {
                        var queryValue = httpContext.Request.Query[param.Name].ToString();

                        // Attempt to convert the query value to the parameter type
                        try
                        {
                            var convertedValue = Convert.ChangeType(queryValue, param.ParameterType);
                            paramList.Add(convertedValue);
                            continue;
                        }
                        catch
                        {
                            // Conversion failed, will fall back to default
                        }
                    }

                    // Use default value if provided
                    if (param.HasDefaultValue)
                    {
                        paramList.Add(param.DefaultValue!);
                    }
                    // Create default instance for value types
                    else if (param.ParameterType.IsValueType)
                    {
                        paramList.Add(Activator.CreateInstance(param.ParameterType)!);
                    }
                    // Add null for reference types
                    else
                    {
                        paramList.Add(null!);
                    }
                }

                // Invoke the delegate with the parameter list we've built
                var result = _delegate.DynamicInvoke(paramList.ToArray());

                // Handle Task<IResult> return type
                if (result is Task<IResult> taskResult)
                {
                    return await taskResult;
                }
                // Handle IResult return type
                else if (result is IResult directResult)
                {
                    return directResult;
                }
                // Handle Task return type (with no result)
                else if (result is Task task)
                {
                    await task;
                    return Results.Ok();
                }

                // Default response if no other return type matched
                return Results.Ok();
            }
            catch (TargetInvocationException ex)
            {
                // Unwrap the TargetInvocationException to get the real exception
                if (ex.InnerException != null)
                {
                    throw ex.InnerException;
                }
                throw;
            }
        }
    }
}