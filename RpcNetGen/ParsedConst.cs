namespace RpcNetGen
{
    internal sealed class ParsedConst : ParsedElementBase
    {
        public ParsedConst(string identifier, string value, string enclosure = null)
        {
            Identifier = identifier;
            Value = value;
            Enclosure = enclosure;
        }

        public string Value { get; set; }
        public string Enclosure { get; set; }
        public bool DoNotTraverseAnyMore { get; set; }

        public string DependencyIdentifier
        {
            get
            {
                int len = Value.Length;
                int idx = 0;
                // Check to see if it's an identifier and search for its end. This is necessary as elements of an
                // enumeration might have "+x" appended, where x is an integer literal
                while (idx < len)
                {
                    char c = Value[idx++];
                    if (!((c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || c == '_' || (c >= '0' && c <= '9' && idx > 0)))
                    {
                        --idx;

                        // back up to the char not belonging to the identifier
                        break;
                    }
                }

                return idx > 0 ? Value.Substring(0, idx) : null;
            }
        }

        public string ResolveValue()
        {
            if (Value.Length > 0)
            {
                // If the value is an integer literal, then we just have to return it
                if (char.IsDigit(Value[0]) || Value[0] == '-')
                {
                    return Value;
                }

                // It's an identifier, which we now have to resolve. First, look it up in the list of global
                // identifiers. Then recursively resolve the value
                ParsedElementBase element = Program.GlobalIdentifiers[Identifier];
                if (element is ParsedConst @const)
                {
                    return @const.ResolveValue();
                }
            }

            return null;
        }
    }
}
