using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text.Json;

namespace Doyen.API.Logging
{
    [ExcludeFromCodeCoverage]
    public class ApplicationInsightsLogger : ITraceLogger
    {
        private readonly TelemetryClient telemetryClient;

        public ApplicationInsightsLogger(TelemetryConfiguration telemetryConfiguration)
        {
            // Initialize the telemetry client with the provided telemetry configuration
            this.telemetryClient = new TelemetryClient() { InstrumentationKey = telemetryConfiguration.InstrumentationKey };
        }

        /// <summary>
        /// Traces an error message.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="reflection">The method reflection (optional).</param>
        public void TraceErrorMessage(string message, MethodBase? reflection = null)
        {
            string messageWithClassAndMethodNames = GetTraceMessageModelJson(message, reflection);
            telemetryClient.TrackTrace(messageWithClassAndMethodNames, SeverityLevel.Error);
        }

        /// <summary>
        /// Traces an exception.
        /// </summary>
        /// <param name="exception">The exception to trace.</param>
        public void TraceException(Exception exception)
        {
            telemetryClient.TrackException(exception);
        }

        /// <summary>
        /// Traces an information message.
        /// </summary>
        /// <param name="message">The information message.</param>
        /// <param name="reflection">The method reflection (optional).</param>
        public void TraceInformationMessage(string message, MethodBase? reflection = null)
        {
            string messageWithClassAndMethodNames = GetTraceMessageModelJson(message, reflection);
            telemetryClient.TrackTrace(messageWithClassAndMethodNames, SeverityLevel.Information);
        }

        /// <summary>
        /// Traces a warning message.
        /// </summary>
        /// <param name="message">The warning message.</param>
        /// <param name="reflection">The method reflection (optional).</param>
        public void TraceWarningMessage(string message, MethodBase? reflection = null)
        {
            string messageWithClassAndMethodNames = GetTraceMessageModelJson(message, reflection);
            telemetryClient.TrackTrace(messageWithClassAndMethodNames, SeverityLevel.Warning);
        }

        /// <summary>
        /// Retrieves the JSON representation of the trace message model.
        /// </summary>
        /// <param name="message">The message to include in the model.</param>
        /// <param name="reflection">The method reflection (optional).</param>
        /// <returns>The JSON representation of the trace message model.</returns>
        protected string GetTraceMessageModelJson(string message, MethodBase? reflection)
        {
            // Create a trace message model with the provided message and reflection
            TraceMessageModel messageModel = new TraceMessageModel(message, reflection);
            return JsonSerializer.Serialize(messageModel);
        }
    }
}
