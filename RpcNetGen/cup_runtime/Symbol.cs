namespace TUVienna.CS_CUP.Runtime
{
    public class Symbol
    {
        public int left, right;
        public int parse_state;
        public int sym;
        public bool used_by_parser;
        public object value;

        public Symbol(int id, int l, int r, object o) : this(id)
        {
            left = l;
            right = r;
            value = o;
        }

        public Symbol(int id, object o) : this(id, -1, -1, o)
        {
        }

        public Symbol(int id, int l, int r) : this(id, l, r, null)
        {
        }

        public Symbol(int symNum) : this(symNum, -1)
        {
            left = -1;
            right = -1;
            value = null;
        }

        public Symbol(int symNum, int state)
        {
            sym = symNum;
            parse_state = state;
        }

        public override string ToString() => "#" + sym;
    }
}
