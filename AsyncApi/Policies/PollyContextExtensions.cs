using Microsoft.Extensions.Logging;
using Polly;

namespace AsyncApi.Policies
    {
    /// <summary>
    /// 
    /// </summary>
    public static class PollyContextExtensions
        {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="logger"></param>
        /// <returns></returns>
        public static bool TryGetLogger(this Context context, out ILogger logger)
            {
            if (context.TryGetValue(PolicyContextItems.Logger, out var loggerObject) && loggerObject is ILogger theLogger)
                {
                logger = theLogger;
                return true;
                }

            logger = null;
            return false;
            }
        }
    }
