namespace TUVienna.CS_CUP.Runtime
{
    using System;
    using System.Collections;
    using System.Globalization;
    using System.Text;

    public class mStack : Stack
    {
        public mStack(Stack origin) : base(origin)
        {
        }

        public object elementAt(int index) =>
            base.ToArray()[index];
    }

    public abstract class lr_parser
    {
        private const int _error_sync_size = 3;
        protected bool _done_parsing;
        private Scanner _scanner;
        protected short[][] action_tab;
        protected Symbol cur_token;
        protected Symbol[] lookahead;
        protected int lookahead_pos;
        protected short[][] production_tab;
        protected short[][] reduce_tab;
        protected Stack stack = new Stack();
        protected int tos;

        public lr_parser()
        {
        }

        public lr_parser(Scanner s) : this()
        {
            setScanner(s);
        }

        protected int error_sync_size() => _error_sync_size;

        public abstract short[][] production_table();
        public abstract short[][] action_table();
        public abstract short[][] reduce_table();
        public abstract int start_state();
        public abstract int start_production();
        public abstract int EOF_sym();
        public abstract int error_sym();
        public void done_parsing() => _done_parsing = true;
        public void setScanner(Scanner s) => _scanner = s;
        public Scanner getScanner() => _scanner;
        public abstract Symbol do_action(int act_num, lr_parser parser, Stack stack, int top);

        public virtual void user_init()
        {
        }

        protected virtual void init_actions()
        {
        }

        public virtual Symbol scan()
        {
            var sym = getScanner().next_token();
            return sym ?? new Symbol(EOF_sym());
        }

        public void report_fatal_error(string message, object info)
        {
            done_parsing();
            report_error(message, info);
            throw new Exception("Can't recover from previous error(s)");
        }

        public void report_error(string message, object info)
        {
            Console.Error.Write(message);
            if (info.GetType() == typeof(Symbol))
            {
                if (((Symbol)info).left != -1)
                {
                    Console.Error.WriteLine(" at character " +
                                            ((Symbol)info).left +
                                            " of input");
                }
                else
                {
                    Console.Error.WriteLine("");
                }
            }
            else
            {
                Console.Error.WriteLine("");
            }
        }

        public void syntax_error(Symbol cur_token) => report_error("Syntax error", cur_token);

        public void unrecovered_syntax_error(Symbol cur_token) =>
            report_fatal_error("Couldn't repair and continue parse", cur_token);

        protected short get_action(int state, int sym)
        {
            short tag;
            int first, last, probe;
            var row = action_tab[state];

            if (row.Length < 20)
            {
                for (probe = 0; probe < row.Length; probe++)
                {
                    tag = row[probe++];
                    if (tag == sym || tag == -1)
                    {
                        return row[probe];
                    }
                }
            }
            else
            {
                first = 0;
                last = ((row.Length - 1) / 2) - 1;
                while (first <= last)
                {
                    probe = (first + last) / 2;
                    if (sym == row[probe * 2])
                    {
                        return row[(probe * 2) + 1];
                    }

                    if (sym > row[probe * 2])
                    {
                        first = probe + 1;
                    }
                    else
                    {
                        last = probe - 1;
                    }
                }

                return row[row.Length - 1];
            }

            return 0;
        }

        protected short get_reduce(int state, int sym)
        {
            short tag;
            var row = reduce_tab[state];

            if (row == null)
            {
                return -1;
            }

            for (int probe = 0; probe < row.Length; probe++)
            {
                tag = row[probe++];
                if (tag == sym || tag == -1)
                {
                    return row[probe];
                }
            }

            return -1;
        }

        public Symbol parse()
        {
            int act;
            Symbol lhs_sym = null;
            short handle_size, lhs_sym_num;

            production_tab = production_table();
            action_tab = action_table();
            reduce_tab = reduce_table();

            init_actions();

            user_init();

            cur_token = scan();

            stack.Clear();
            stack.Push(new Symbol(0, start_state()));
            tos = 0;

            for (_done_parsing = false; !_done_parsing;)
            {
                if (cur_token.used_by_parser)
                {
                    throw new Exception("Symbol recycling detected (fix your scanner).");
                }

                act = get_action(((Symbol)stack.Peek()).parse_state, cur_token.sym);

                if (act > 0)
                {
                    cur_token.parse_state = act - 1;
                    cur_token.used_by_parser = true;
                    stack.Push(cur_token);
                    tos++;

                    cur_token = scan();
                }
                else if (act < 0)
                {
                    lhs_sym = do_action(-act - 1, this, stack, tos);

                    lhs_sym_num = production_tab[-act - 1][0];
                    handle_size = production_tab[-act - 1][1];

                    for (int i = 0; i < handle_size; i++)
                    {
                        stack.Pop();
                        tos--;
                    }

                    act = get_reduce(((Symbol)stack.Peek()).parse_state, lhs_sym_num);

                    lhs_sym.parse_state = act;
                    lhs_sym.used_by_parser = true;
                    stack.Push(lhs_sym);
                    tos++;
                }
                else if (act == 0)
                {
                    syntax_error(cur_token);

                    if (!error_recovery(false))
                    {
                        unrecovered_syntax_error(cur_token);
                        done_parsing();
                    }
                    else
                    {
                        lhs_sym = (Symbol)stack.Peek();
                    }
                }
            }

            return lhs_sym;
        }

        public void debug_message(string mess) => Console.Error.WriteLine(mess);

        public void dump_stack()
        {
            if (stack == null)
            {
                debug_message("# Stack dump requested, but stack is null");
                return;
            }

            debug_message("============ Parse Stack Dump ============");

            var myEnum = stack.GetEnumerator();
            while (myEnum.MoveNext())
            {
                debug_message("Symbol: " +
                              ((Symbol)myEnum.Current).sym +
                              " State: " +
                              ((Symbol)myEnum.Current).parse_state);
            }

            debug_message("==========================================");
        }

        public void debug_reduce(int prod_num, int nt_num, int rhs_size) => debug_message("# Reduce with prod #" +
                                                                                          prod_num +
                                                                                          " [NT=" +
                                                                                          nt_num +
                                                                                          ", " +
                                                                                          "SZ=" +
                                                                                          rhs_size +
                                                                                          "]");

        public void debug_shift(Symbol shift_tkn) => debug_message("# Shift under term #" +
                                                                   shift_tkn.sym +
                                                                   " to state #" +
                                                                   shift_tkn.parse_state);

        public void debug_stack()
        {
            var sb = new StringBuilder("## STACK:");
            var e = stack.GetEnumerator();
            int i = 0;
            while (e.MoveNext())
            {
                i++;
                var s = (Symbol)e.Current;
                sb.Append(" <state " + s.parse_state + ", sym " + s.sym + ">");
                if (i % 3 == 2 || i == stack.Count - 1)
                {
                    debug_message(sb.ToString());
                    sb = new StringBuilder("         ");
                }
            }
        }

        public Symbol debug_parse()
        {
            int act;
            Symbol lhs_sym = null;
            short handle_size, lhs_sym_num;

            production_tab = production_table();
            action_tab = action_table();
            reduce_tab = reduce_table();

            debug_message("# Initializing parser");

            init_actions();

            user_init();

            cur_token = scan();

            debug_message("# Current Symbol is #" + cur_token.sym);

            stack.Clear();
            stack.Push(new Symbol(0, start_state()));
            tos = 0;

            for (_done_parsing = false; !_done_parsing;)
            {
                if (cur_token.used_by_parser)
                {
                    throw new Exception("Symbol recycling detected (fix your scanner).");
                }

                act = get_action(((Symbol)stack.Peek()).parse_state, cur_token.sym);

                if (act > 0)
                {
                    cur_token.parse_state = act - 1;
                    cur_token.used_by_parser = true;
                    debug_shift(cur_token);
                    stack.Push(cur_token);
                    tos++;

                    cur_token = scan();
                    debug_message("# Current token is " + cur_token);
                }
                else if (act < 0)
                {
                    lhs_sym = do_action(-act - 1, this, stack, tos);
                    lhs_sym_num = production_tab[-act - 1][0];
                    handle_size = production_tab[-act - 1][1];

                    debug_reduce(-act - 1, lhs_sym_num, handle_size);

                    for (int i = 0; i < handle_size; i++)
                    {
                        stack.Pop();
                        tos--;
                    }

                    act = get_reduce(((Symbol)stack.Peek()).parse_state, lhs_sym_num);
                    debug_message("# Reduce rule: top state " +
                                  ((Symbol)stack.Peek()).parse_state +
                                  ", lhs sym " +
                                  lhs_sym_num +
                                  " -> state " +
                                  act);

                    lhs_sym.parse_state = act;
                    lhs_sym.used_by_parser = true;
                    stack.Push(lhs_sym);
                    tos++;

                    debug_message("# Goto state #" + act);
                }
                else if (act == 0)
                {
                    syntax_error(cur_token);

                    if (!error_recovery(true))
                    {
                        unrecovered_syntax_error(cur_token);
                        done_parsing();
                    }
                    else
                    {
                        lhs_sym = (Symbol)stack.Peek();
                    }
                }
            }

            return lhs_sym;
        }

        protected bool error_recovery(bool debug)
        {
            if (debug)
            {
                debug_message("# Attempting error recovery");
            }

            if (!find_recovery_config(debug))
            {
                if (debug)
                {
                    debug_message("# Error recovery fails");
                }

                return false;
            }

            read_lookahead();

            for (;;)
            {
                if (debug)
                {
                    debug_message("# Trying to parse ahead");
                }

                if (try_parse_ahead(debug))
                {
                    break;
                }

                if (lookahead[0].sym == EOF_sym())
                {
                    if (debug)
                    {
                        debug_message("# Error recovery fails at EOF");
                    }

                    return false;
                }

                if (debug)
                {
                    debug_message("# Consuming Symbol #" + lookahead[0].sym);
                }

                restart_lookahead();
            }

            if (debug)
            {
                debug_message("# Parse-ahead ok, going back to normal parse");
            }

            parse_lookahead(debug);
            return true;
        }

        protected bool shift_under_error() =>
            get_action(((Symbol)stack.Peek()).parse_state, error_sym()) > 0;

        protected bool find_recovery_config(bool debug)
        {
            Symbol error_token;
            int act;

            if (debug)
            {
                debug_message("# Finding recovery state on stack");
            }

            int right_pos = ((Symbol)stack.Peek()).right;
            int left_pos = ((Symbol)stack.Peek()).left;

            while (!shift_under_error())
            {
                if (debug)
                {
                    debug_message("# Pop stack by one, state was # " +
                                  ((Symbol)stack.Peek()).parse_state);
                }

                left_pos = ((Symbol)stack.Pop()).left;
                tos--;

                if (stack.Count == 0)
                {
                    if (debug)
                    {
                        debug_message("# No recovery state found on stack");
                    }

                    return false;
                }
            }

            act = get_action(((Symbol)stack.Peek()).parse_state, error_sym());
            if (debug)
            {
                debug_message("# Recover state found (#" +
                              ((Symbol)stack.Peek()).parse_state +
                              ")");
                debug_message("# Shifting on error to state #" + (act - 1));
            }

            error_token = new Symbol(error_sym(), left_pos, right_pos)
            {
                parse_state = act - 1,
                used_by_parser = true
            };
            stack.Push(error_token);
            tos++;

            return true;
        }

        protected void read_lookahead()
        {
            lookahead = new Symbol[error_sync_size()];

            for (int i = 0; i < error_sync_size(); i++)
            {
                lookahead[i] = cur_token;
                cur_token = scan();
            }

            lookahead_pos = 0;
        }

        protected Symbol cur_err_token() => lookahead[lookahead_pos];

        protected bool advance_lookahead()
        {
            lookahead_pos++;
            return lookahead_pos < error_sync_size();
        }

        protected void restart_lookahead()
        {
            for (int i = 1; i < error_sync_size(); i++)
            {
                lookahead[i - 1] = lookahead[i];
            }

            lookahead[error_sync_size() - 1] = cur_token;
            cur_token = scan();

            lookahead_pos = 0;
        }

        protected bool try_parse_ahead(bool debug)
        {
            int act;
            short lhs, rhs_size;

            var vstack = new virtual_parse_stack(stack);

            for (;;)
            {
                act = get_action(vstack.top(), cur_err_token().sym);

                if (act == 0)
                {
                    return false;
                }

                if (act > 0)
                {
                    vstack.push(act - 1);

                    if (debug)
                    {
                        debug_message("# Parse-ahead shifts Symbol #" +
                                      cur_err_token().sym +
                                      " into state #" +
                                      (act - 1));
                    }

                    if (!advance_lookahead())
                    {
                        return true;
                    }
                }
                else
                {
                    if (-act - 1 == start_production())
                    {
                        if (debug)
                        {
                            debug_message("# Parse-ahead accepts");
                        }

                        return true;
                    }

                    lhs = production_tab[-act - 1][0];
                    rhs_size = production_tab[-act - 1][1];

                    for (int i = 0; i < rhs_size; i++)
                    {
                        vstack.pop();
                    }

                    if (debug)
                    {
                        debug_message("# Parse-ahead reduces: handle size = " +
                                      rhs_size +
                                      " lhs = #" +
                                      lhs +
                                      " from state #" +
                                      vstack.top());
                    }

                    vstack.push(get_reduce(vstack.top(), lhs));
                    if (debug)
                    {
                        debug_message("# Goto state #" + vstack.top());
                    }
                }
            }
        }

        protected void parse_lookahead(bool debug)
        {
            int act;
            Symbol lhs_sym = null;
            short handle_size, lhs_sym_num;
            lookahead_pos = 0;
            if (debug)
            {
                debug_message("# Reparsing saved input with actions");
                debug_message("# Current Symbol is #" + cur_err_token().sym);
                debug_message("# Current state is #" + ((Symbol)stack.Peek()).parse_state);
            }

            while (!_done_parsing)
            {
                act = get_action(((Symbol)stack.Peek()).parse_state, cur_err_token().sym);

                if (act > 0)
                {
                    cur_err_token().parse_state = act - 1;
                    cur_err_token().used_by_parser = true;
                    if (debug)
                    {
                        debug_shift(cur_err_token());
                    }

                    stack.Push(cur_err_token());
                    tos++;

                    if (!advance_lookahead())
                    {
                        if (debug)
                        {
                            debug_message("# Completed reparse");
                        }

                        return;
                    }

                    if (debug)
                    {
                        debug_message("# Current Symbol is #" + cur_err_token().sym);
                    }
                }
                else if (act < 0)
                {
                    lhs_sym = do_action(-act - 1, this, stack, tos);

                    lhs_sym_num = production_tab[-act - 1][0];
                    handle_size = production_tab[-act - 1][1];

                    if (debug)
                    {
                        debug_reduce(-act - 1, lhs_sym_num, handle_size);
                    }

                    for (int i = 0; i < handle_size; i++)
                    {
                        stack.Pop();
                        tos--;
                    }

                    act = get_reduce(((Symbol)stack.Peek()).parse_state, lhs_sym_num);

                    lhs_sym.parse_state = act;
                    lhs_sym.used_by_parser = true;
                    stack.Push(lhs_sym);
                    tos++;

                    if (debug)
                    {
                        debug_message("# Goto state #" + act);
                    }
                }
                else if (act == 0)
                {
                    report_fatal_error("Syntax error", lhs_sym);
                    return;
                }
            }
        }

        protected static int GetChar(string s, ref int n)
        {
            int res;
            if (s[n + 1] == 'u')
            {
                res = int.Parse(s.Substring(n + 2, 4), NumberStyles.AllowHexSpecifier);
                n += 6;
            }
            else
            {
                res = (int.Parse(s.Substring(n + 1, 1)) * 64) +
                      (int.Parse(s.Substring(n + 2, 1)) * 8) +
                      int.Parse(s.Substring(n + 3, 1));
                n += 4;
            }

            return res;
        }

        protected static short[][] unpackFromStrings(string[] sa)
        {
            string sb = sa[0];
            for (int i = 1; i < sa.Length; i++)
            {
                sb += sa[i];
            }

            int n = 0;
            int size1 = (GetChar(sb, ref n) << 16) | GetChar(sb, ref n);
            var result = new short[size1][];
            for (int i = 0; i < size1; i++)
            {
                int size2 = (GetChar(sb, ref n) << 16) | GetChar(sb, ref n);

                result[i] = new short[size2];

                for (int j = 0; j < size2; j++)
                {
                    result[i][j] = (short)(GetChar(sb, ref n) - 2);
                }
            }

            return result;
        }
    }
}
