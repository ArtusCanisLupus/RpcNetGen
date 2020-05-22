namespace RpcNetGen
{
    using System;

    [Serializable]
    internal sealed class ParserException : Exception
    {
        public ParserException()
        {
        }

        public ParserException(string msg)
            : base(msg)
        {
        }
    }
}
