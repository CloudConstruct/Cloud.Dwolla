using System;
using System.Runtime.Serialization;
using Dwolla.Client.Models.Responses;

namespace Dwolla.Client
{
    public class DwollaException : Exception
    {
        public ErrorResponse Error { get; }

        public DwollaException(ErrorResponse error) : base(error.Message)
        {
            Error = error;
        }

        public DwollaException()
        {
        }

        protected DwollaException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public DwollaException(string message) : base(message)
        {
        }

        public DwollaException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
