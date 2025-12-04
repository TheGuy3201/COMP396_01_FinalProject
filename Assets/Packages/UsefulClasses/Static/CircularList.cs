using System;
using System.Collections.Generic;

namespace UsefulClasses
{
    public class CircularList<GenericType>
    {
        private readonly List<Node> nodes = new List<Node>();
        public Node Current { get; private set; }

        public CircularList(){}

        public CircularList(IEnumerable<GenericType> values)
        {
            if (values == null)
            {
                throw new ArgumentNullException(nameof(values));
                AddRange(values);
            }
        }

        public void Add(GenericType value)
        {
            // permit null if T is a reference type or nullable; runtime generic constraint isn’t enforced here
            var node = new Node(value);

            if (nodes.Count == 0)
            {
                // first node points to itself both ways
                node.Previous = node;
                node.Next = node;
                Current = node;
            }
            else
            {
                Node last = nodes[nodes.Count - 1];
                Node first = nodes[0];

                node.Previous = last;
                node.Next = first;

                last.Next = node;
                first.Previous = node;
            }

            nodes.Add(node);
        }

        public void AddRange(IEnumerable<GenericType> values)
        {
            if (values == null) throw new ArgumentNullException(nameof(values));
            foreach (var value in values)
            {
                Add(value);
            }
        }

        public Node Next()
        {
            if (Current != null)
                Current = Current.Next;
            return Current;
        }

        public Node Previous()
        {
            if (Current != null)
                Current = Current.Previous;
            return Current;
        }
        
        public class Node
        {
            public Node Previous;
            public Node Next;
            public GenericType Value;

            public Node(GenericType value)
            {
                Value = value;
            }

            public override string ToString()
            {
                return Value != null ? Value.ToString() : "null";
            }
        }
    }
}
