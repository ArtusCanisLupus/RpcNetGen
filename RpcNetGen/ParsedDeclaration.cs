namespace RpcNetGen
{
    internal sealed class ParsedDeclaration : ParsedElementBase
    {
        public ParsedDeclaration(
            string identifier,
            string dataType,
            DeclarationType kind = DeclarationType.Scalar,
            string size = null)
        {
            Identifier = identifier;
            DataType = dataType;
            Kind = kind;
            Size = size;
        }

        public string DataType { get; }
        public DeclarationType Kind { get; }
        public string Size { get; }
    }
}
