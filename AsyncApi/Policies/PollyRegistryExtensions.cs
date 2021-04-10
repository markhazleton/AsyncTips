﻿using Microsoft.Extensions.Logging;
using Polly;
using Polly.Registry;
using System;
using System.Net.Http;

namespace AsyncApi.Policies
{
    /// <summary>
    /// 
    /// </summary>
    public static class PollyRegistryExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="policyRegistry"></param>
        /// <returns></returns>
        public static IPolicyRegistry<string> AddBasicRetryPolicy(this IPolicyRegistry<string> policyRegistry)
        {
            var retryPolicy = Policy
                .Handle<Exception>()
                .OrResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
                .WaitAndRetryAsync(3, retryCount => TimeSpan.FromSeconds(10), (result, timeSpan, retryCount, context) =>
                {
                    if (!context.TryGetLogger(out var logger)) return;

                    if (result.Exception != null)
                    {
                        logger.LogError(result.Exception, "An exception occurred on retry {RetryAttempt} for {PolicyKey}", retryCount, context.PolicyKey);
                    }
                    else
                    {
                        logger.LogError("A non success code {StatusCode} was received on retry {RetryAttempt} for {PolicyKey}",
                            (int)result.Result.StatusCode, retryCount, context.PolicyKey);
                    }
                })
                .WithPolicyKey(PolicyNames.BasicRetry);

            policyRegistry.Add(PolicyNames.BasicRetry, retryPolicy);

            return policyRegistry;
        }
    }
}
