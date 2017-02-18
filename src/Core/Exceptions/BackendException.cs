using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Core.Exceptions
{
    public class BackendException : Exception
    {
        public ErrorCode Code { get; private set; }
        public string Text { get; private set; }

        public BackendException(string text, ErrorCode code)
            : base(text)
        {
            Code = code;
            Text = text;
        }
    }

    public enum ErrorCode
    {
        Exception,
        ContractPoolEmpty,
        MissingRequiredParams
    }
}
