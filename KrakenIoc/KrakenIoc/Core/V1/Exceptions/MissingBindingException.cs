using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace AOFL.KrakenIoc.Core.V1.Exceptions
{
    public class MissingBindingException : Exception
    {
        private const string _exceptionFormat = "No binding found for type {0} and category {1}. Did you forget to bind the type?";

        public Type BindingType { get; set; }

        public MissingBindingException(Type bindingType, string category) : base(string.Format(_exceptionFormat, bindingType, category))
        {
            BindingType = bindingType;
        }

        public MissingBindingException(Type bindingType, object category) : base(string.Format(_exceptionFormat, bindingType, category))
        {
            BindingType = bindingType;
        }

        public MissingBindingException(string message) : base(message)
        {
        }

        public MissingBindingException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected MissingBindingException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
