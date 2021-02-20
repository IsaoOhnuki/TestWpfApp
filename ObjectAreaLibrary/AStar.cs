using System;
using System.Collections.Generic;
using System.Linq;

namespace ObjectAreaLibrary
{
    using Heuristic = Func<System.Drawing.Point, int>;
    using NodePoint = System.Drawing.Point;
    using ObstacleRect = System.Drawing.Rectangle;
    using Viewpoint = Func<System.Drawing.Point, System.Drawing.Point, IEnumerable<System.Drawing.Rectangle>, IEnumerable<System.Drawing.Point>>;

    public class AStarNode
    {
        public NodePoint NodePoint;
        public int Forward;
        public int Backward;
        public int Cost;
        public bool Inspected;
        public bool Adopt;
        public int Index;

        private static int _creatIndex;
        public static AStarNode Creator(bool resetndex = false)
        {
            if (resetndex)
            {
                _creatIndex = 0;
            }
            return new AStarNode()
            {
                Index = ++_creatIndex,
            };
        }
    }

    public class AStar
    {
        private SortedDictionary<NodePoint, AStarNode> NodeCollection { get; } = new SortedDictionary<NodePoint, AStarNode>();

        private void AddNode(AStarNode node)
        {
            NodeCollection.Add(node.NodePoint, node);
        }

        private AStarNode NodeAt(NodePoint point)
        {
            return NodeCollection.Where(_ => _.Key == point).Select(_ => _.Value).FirstOrDefault();
        }

        public IEnumerable<AStarNode> AdoptList()
        {
            return NodeCollection.Where(_ => _.Value.Adopt).Select(_ => _.Value).OrderBy(_ => _.Index);
        }

        public bool Exec(NodePoint startPos, NodePoint endPos, NodePoint maxPos, IEnumerable<ObstacleRect> obstacles, Viewpoint viewpoint, Heuristic heuristic)
        {
            var nodes = new List<AStarNode>();
            var viewpoints = viewpoint(startPos, maxPos, obstacles);
            foreach (var pos in viewpoints)
            {
                if (pos == endPos)
                {
                    return true;
                }
                var node = NodeAt(pos);
                if (node == null)
                {
                    node = AStarNode.Creator();
                    node.NodePoint = pos;
                    node.Forward = heuristic(startPos);
                    node.Backward = heuristic(endPos);
                    node.Cost = node.Forward + node.Backward;
                    AddNode(node);
                }
                nodes.Add(node);
            }

            bool result = false;
            foreach (var node in nodes.Where(_ => !_.Inspected).OrderBy(_ => _.Cost).ThenBy(_ => _.Forward))
            {
                node.Inspected = true;
                if (Exec(node.NodePoint, endPos, maxPos, obstacles, viewpoint, heuristic))
                {
                    node.Adopt = true;
                    result = true;
                    break;
                }
            }
            return result;
        }

        public int Heuristic(NodePoint point)
        {
            return (int)Math.Round(Math.Sqrt(point.X * point.X + point.Y * point.Y));
        }

        public IEnumerable<NodePoint> Viewpoint(NodePoint point, NodePoint maxPoint, IEnumerable<ObstacleRect> rects)
        {
            List<NodePoint> ret = new List<NodePoint>();
            if (point.X - 1 >= 0)
            {
                var pos = new NodePoint(point.X - 1, point.Y);
                if (rects.All(_ => !_.Contains(pos)))
                {
                    ret.Add(pos);
                }
            }
            if (point.Y - 1 >= 0)
            {
                var pos = new NodePoint(point.X, point.Y - 1);
                if (rects.All(_ => !_.Contains(pos)))
                {
                    ret.Add(pos);
                }
            }
            if (point.X + 1 < maxPoint.X)
            {
                var pos = new NodePoint(point.X + 1, point.Y);
                if (rects.All(_ => !_.Contains(pos)))
                {
                    ret.Add(pos);
                }
            }
            if (point.Y + 1 < maxPoint.Y)
            {
                var pos = new NodePoint(point.X, point.Y + 1);
                if (rects.All(_ => !_.Contains(pos)))
                {
                    ret.Add(pos);
                }
            }
            return ret;
        }
    }
}
