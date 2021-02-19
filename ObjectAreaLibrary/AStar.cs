using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ObjectAreaLibrary
{
    class AStar
    {
        public struct Node
        {
            public int X;
            public int Y;
            public int Forward;
            public int Backward;
            public int Cost;
            public bool Adopt;
            public int Index;
            private static int _creatIndex;
            public static Node Creator(bool resetndex = false)
            {
                if (resetndex)
                {
                    _creatIndex = 0;
                }
                return new Node()
                {
                    Index = ++_creatIndex,
                };
            }
        }

        public SortedDictionary<(int X, int Y), Node> NodeCollection { get; } = new SortedDictionary<(int, int), Node>();

        public void AddNode(Node node)
        {
            NodeCollection.Add((node.X, node.Y), node);
        }

        public void RemoveNode(Node node)
        {
            NodeCollection.Remove((node.X, node.Y));
        }

        public IEnumerable<Node> AdoptList(Node node)
        {
            return NodeCollection.Where(_ => _.Value.Adopt).Select(_ => _.Value).OrderBy(_ => _.Index);
        }
    }
}
