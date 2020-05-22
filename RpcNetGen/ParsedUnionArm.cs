namespace RpcNetGen
{
    internal sealed class ParsedUnionArm
    {
        public ParsedUnionArm(string value, ParsedDeclaration element)
        {
            Value = value;
            Element = element;
        }

        public string Value { get; }
        public ParsedDeclaration Element { get; }
    }
}
