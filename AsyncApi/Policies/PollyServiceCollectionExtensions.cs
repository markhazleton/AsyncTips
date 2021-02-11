using Microsoft.Extensions.DependencyInjection;

namespace AsyncApi.Policies
    {
    /// <summary>
    /// 
    /// </summary>
    public static class PollyServiceCollectionExtensions
        {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddPollyPolicies(this IServiceCollection services)
            {
            //var registry = services.AddPolicyRegistry();
            //registry.AddBasicRetryPolicy();
            return services;
            }
        }
    }
