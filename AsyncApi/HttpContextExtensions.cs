using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace AsyncApi
    {
    /// <summary>
    /// HttpContextExtensions
    /// </summary>
    public static class HttpContextExtensions
        {
        /// <summary>
        /// AddHttpContextAccessor
        /// </summary>
        /// <param name="services"></param>
        public static void AddHttpContextAccessor(this IServiceCollection services)
            {
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            }

        /// <summary>
        /// UseHttpContext
        /// </summary>
        /// <param name="app"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseHttpContext(this IApplicationBuilder app)
            {
            MyHttpContext.Configure(app.ApplicationServices.GetRequiredService<IHttpContextAccessor>());
            return app;
            }
        }
    }