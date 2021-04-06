using Microsoft.AspNetCore.Http;

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
        ///  AppBaseUrl
        /// </summary>
        public static string AppBaseUrl => $"{Current.Request.Scheme}://{Current.Request.Host}{Current.Request.PathBase}";

        internal static void Configure(IHttpContextAccessor contextAccessor)
            {
            m_httpContextAccessor = contextAccessor;
            }
        }
    }