using Microsoft.AspNetCore.Http;
using System.Reflection;

namespace AsyncApi
{
    /// <summary>
    /// MyHttpContext
    /// </summary>
    public class MyHttpContext
    {
        private static IHttpContextAccessor m_httpContextAccessor;

        /// <summary>
        /// Current Context
        /// </summary>
        public static HttpContext Current => m_httpContextAccessor.HttpContext;

        /// <summary>
        /// My Test
        /// </summary>
        /// <returns></returns>
        public static string Version
        {
            get
            {
                var x = Assembly.GetExecutingAssembly().GetName();
                return $"{x.Name} {x.Version.ToString(3)}";
            }
        }

        /// <summary>
        ///  AppBaseUrl
        /// </summary>
        public static string AppBaseUrl => $"{Current.Request.Scheme}://{Current.Request.Host}{Current.Request.PathBase}";

        internal static void Configure(IHttpContextAccessor contextAccessor)
        {
            m_httpContextAccessor = contextAccessor;
        }
    }
}