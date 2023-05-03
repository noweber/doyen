using System.Reflection;

namespace Doyen.API.Logging
{
    public interface ITraceLogger
    {
        void TraceInformationMessage(string message, MethodBase? reflection = null);

        void TraceWarningMessage(string message, MethodBase? reflection = null);

        void TraceErrorMessage(string message, MethodBase? reflection = null);

        void TraceException(Exception exception);
    }
}
