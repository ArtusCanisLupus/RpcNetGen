namespace RpcNetGen
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    internal sealed class ParsedUnion : ParsedElementBase
    {
        public ParsedUnion(string identifier, ParsedDeclaration descriminant, IEnumerable elements)
        {
            Identifier = identifier;
            Descriminant = descriminant;
            Elements = elements?.Cast<ParsedUnionArm>().ToList() ?? new List<ParsedUnionArm>();
        }

        public ParsedDeclaration Descriminant { get; }
        public List<ParsedUnionArm> Elements { get; }
    }
}
