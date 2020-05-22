namespace RpcNetGen
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    internal sealed class ParsedEnum : ParsedElementBase
    {
        public ParsedEnum(string identifier, IEnumerable enumElements)
        {
            Identifier = identifier;
            EnumElements = enumElements?.Cast<ParsedConst>().ToList() ?? new List<ParsedConst>();
        }

        public List<ParsedConst> EnumElements { get; }
    }
}
