namespace TUVienna.CS_CUP.Runtime
{
    using System;
    using System.Collections;

    public class virtual_parse_stack
    {
        protected int real_next;
        protected Stack real_stack;
        protected Stack vstack;

        public virtual_parse_stack(Stack shadowing_stack)
        {
            real_stack = shadowing_stack ?? throw new Exception("Internal parser error: attempt to create null virtual stack");
            vstack = new Stack();
            real_next = 0;
            get_from_real();
        }

        protected void get_from_real()
        {
            if (real_next >= real_stack.Count)
            {
                return;
            }

            var stackSym = (Symbol)real_stack.ToArray()[real_stack.Count - 1 - real_next];

            real_next++;

            vstack.Push(stackSym.parse_state);
        }

        public bool empty() =>
            vstack.Count == 0;

        public int top()
        {
            if (vstack.Count == 0)
            {
                throw new Exception("Internal parser error: top() called on empty virtual stack");
            }

            return (int)vstack.Peek();
        }

        public void pop()
        {
            if (vstack.Count == 0)
            {
                throw new Exception("Internal parser error: pop from empty virtual stack");
            }

            vstack.Pop();

            if (vstack.Count == 0)
            {
                get_from_real();
            }
        }

        public void push(int state_num) => vstack.Push(state_num);
    }
}
