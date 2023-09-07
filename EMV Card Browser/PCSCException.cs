using System;

namespace EMV_Card_Browser
{
    /// <summary>
    ///  PC/SC exceptions
    /// </summary>
    public class PCSCException : Exception
    {
        public PCSCException()
            : base("PC/SC exception")
        {
        }

        public PCSCException(int result)
            : base(WinSCard.SCardErrorMessage(result))
        {
            Result = result;
        }

        public int Result { get; private set; }
    }
}
