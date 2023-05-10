using System.Reflection;

namespace Doyen.API.Logging
{
    internal class TraceMessageModel
    {
        // The name of the class where the trace message originated from
        public string Class { get; private set; }

        // The name of the method where the trace message originated from
        public string Method { get; private set; }

        // The trace message
        public string Message { get; private set; }

        /// <summary>
        /// Initializes a new instance of the TraceMessageModel class with the provided message and reflection information.
        /// </summary>
        /// <param name="message">The trace message.</param>
        /// <param name="reflection">The method reflection information.</param>
        public TraceMessageModel(string message, MethodBase reflection)
        {
            if (reflection != null)
            {
                if (reflection.ReflectedType != null)
                {
                    // Set the class and method names based on the reflected type and method names
                    this.Class = reflection.ReflectedType.Name;
                    this.Method = reflection.Name;
                }
            }

            // Set the trace message
            this.Message = message;
        }
    }
}
