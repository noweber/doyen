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
            this.telemetryClient = new TelemetryClient() { InstrumentationKey = telemetryConfiguration.InstrumentationKey };
        }

        public void TraceErrorMessage(string message, MethodBase? reflection = null)
        {
            string messageWithClassAndMethodNames = GetTraceMessageModelJson(message, reflection);
            telemetryClient.TrackTrace(messageWithClassAndMethodNames, SeverityLevel.Error);
        }

        public void TraceException(Exception exception)
        {
            telemetryClient.TrackException(exception);
        }

        public void TraceInformationMessage(string message, MethodBase? reflection = null)
        {
            string messageWithClassAndMethodNames = GetTraceMessageModelJson(message, reflection);
            telemetryClient.TrackTrace(messageWithClassAndMethodNames, SeverityLevel.Information);
        }

        public void TraceWarningMessage(string message, MethodBase? reflection = null)
        {
            string messageWithClassAndMethodNames = GetTraceMessageModelJson(message, reflection);
            telemetryClient.TrackTrace(messageWithClassAndMethodNames, SeverityLevel.Warning);
        }


        protected string GetTraceMessageModelJson(string message, MethodBase? reflection)
        {
            TraceMessageModel messageModel = new TraceMessageModel(message, reflection);
            return JsonSerializer.Serialize(messageModel);
        }
    }
}
