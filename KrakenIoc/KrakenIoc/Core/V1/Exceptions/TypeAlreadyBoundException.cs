using System;
using System.Runtime.Serialization;

namespace AOFL.KrakenIoc.Core.V1.Exceptions
{
    public class TypeAlreadyBoundException : Exception
    {
        private const string _exceptionFormat = "Type {0} was already bound by the container!";

        public Type BindingType { get; set; }

        public TypeAlreadyBoundException(Type bindingType) : base(string.Format(_exceptionFormat, bindingType))
        {
            BindingType = bindingType;
        }

        public TypeAlreadyBoundException(string message) : base(message)
        {
        }

        public TypeAlreadyBoundException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected TypeAlreadyBoundException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
