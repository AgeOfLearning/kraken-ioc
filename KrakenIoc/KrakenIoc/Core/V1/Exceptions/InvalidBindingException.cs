using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace AOFL.KrakenIoc.Core.V1.Exceptions
{
    public class InvalidBindingException : Exception
    {
        public InvalidBindingException()
        {
        }

        public InvalidBindingException(string message) : base(message)
        {
        }

        public InvalidBindingException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
