using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AOFL.KrakenIoc.Core.V1.Exceptions
{
    public class TypeCategoryAlreadyBoundException : TypeAlreadyBoundException
    {
        private const string _exceptionFormat = "Type {0} was already bound with category {1} by the container!";
        
        public object Category { get; set; }

        public TypeCategoryAlreadyBoundException(Type bindingType, object category) : base(string.Format(_exceptionFormat, bindingType, category))
        {
            BindingType = bindingType;
            Category = category;
        }

        public TypeCategoryAlreadyBoundException(string message) : base(message)
        {
        }

        public TypeCategoryAlreadyBoundException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
