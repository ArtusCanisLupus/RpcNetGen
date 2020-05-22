namespace RpcNetGen
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    internal sealed class ParsedStruct : ParsedElementBase
    {
        public ParsedStruct(string identifier, IEnumerable elements)
        {
            Identifier = identifier;
            Elements = elements?.Cast<ParsedDeclaration>().ToList() ?? new List<ParsedDeclaration>();
        }

        public List<ParsedDeclaration> Elements { get; }
    }
}
