using System.Reflection;

namespace Doyen.API.Logging
{
    internal class TraceMessageModel
    {
        public string Class { get; private set; }

        public string Method { get; private set; }

        public string Message { get; private set; }

        public TraceMessageModel(string message, MethodBase reflection)
        {
            if (reflection != null)
            {
                if (reflection.ReflectedType != null)
                {
                    this.Class = reflection.ReflectedType.Name;
                    this.Method = reflection.Name;
                }
            }

            this.Message = message;
        }
    }
}
