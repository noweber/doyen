using System.Reflection;

namespace Doyen.API.Logging
{
    /// <summary>
    /// Defines a logger for tracing and logging purposes.
    /// </summary>
    public interface ITraceLogger
    {
        /// <summary>
        /// Traces an information message.
        /// </summary>
        /// <param name="message">The information message.</param>
        /// <param name="reflection">The method reflection (optional).</param>
        void TraceInformationMessage(string message, MethodBase? reflection = null);

        /// <summary>
        /// Traces a warning message.
        /// </summary>
        /// <param name="message">The warning message.</param>
        /// <param name="reflection">The method reflection (optional).</param>
        void TraceWarningMessage(string message, MethodBase? reflection = null);

        /// <summary>
        /// Traces an error message.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="reflection">The method reflection (optional).</param>
        void TraceErrorMessage(string message, MethodBase? reflection = null);

        /// <summary>
        /// Traces an exception.
        /// </summary>
        /// <param name="exception">The exception to trace.</param>
        void TraceException(Exception exception);
    }
}
