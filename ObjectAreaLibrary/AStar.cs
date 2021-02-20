using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace ObjectAreaLibrary
{
    using Heuristic = Func<Vector, double>;
    using NodePoint = Point;
    using NodeRect = Rect;
    using Viewpoint = Func<Point, Point, Point, IEnumerable<Rect>, IEnumerable<Point>>;

    public class AStarNode
    {
        public NodePoint NodePoint;
        public double Forward;
        public double Backward;
        public double Cost;
        public bool Inspected;
        public bool Adopt;
        public int Index;
    }

    public class AStar
    {
        private Dictionary<NodePoint, AStarNode> NodeCollection { get; } = new Dictionary<NodePoint, AStarNode>();

        private int _astarNodeIndex;
        public AStarNode CreatAStarNode()
        {
            return new AStarNode()
            {
                Index = ++_astarNodeIndex,
            };
        }

        private void AddNode(AStarNode node)
        {
            NodeCollection.Add(node.NodePoint, node);
        }

        private AStarNode NodeAt(NodePoint point)
        {
            return NodeCollection.Where(_ => _.Key == point).Select(_ => _.Value).FirstOrDefault();
        }

        private IEnumerable<NodePoint> AdoptList(NodePoint startPos)
        {
            return NodeCollection
                .Where(_ => _.Value.Adopt)
                .OrderBy(_ => _.Value.Cost)
                .Select(_ => new NodePoint(_.Value.NodePoint.X - startPos.X, _.Value.NodePoint.Y - startPos.Y));
        }

        private NodeRect _gool;

        public IEnumerable<NodePoint> Exec(NodePoint startPos, NodePoint endPos, NodePoint minPos, NodePoint maxPos, IEnumerable<NodeRect> obstacles, Viewpoint viewpoint, Heuristic heuristic)
        {
            _astarNodeIndex = 0;
            NodeCollection.Clear();
            _gool = new NodeRect(endPos, new Size(1, 1));
            _gool.Inflate(11, 11);

            var rect = new NodeRect(startPos, endPos);
            rect.Inflate(2, 2);
            var bounds = new Rect(minPos, maxPos);
            ExecAStar(startPos, endPos, startPos, bounds.TopLeft, bounds.BottomRight, obstacles
                .Where(_ =>
                {
                    var diff = NodeRect.Intersect(_, rect);
                    return diff.Width > 0 || diff.Height > 0;
                }),
                viewpoint, heuristic);
            return AdoptList(startPos);
        }

        private bool ExecAStar(NodePoint startPos, NodePoint endPos, NodePoint nowPos, NodePoint minPos, NodePoint maxPos, IEnumerable<NodeRect> obstacles, Viewpoint viewpoint, Heuristic heuristic)
        {
            var viewpoints = viewpoint(nowPos, minPos, maxPos, obstacles);
            foreach (var pos in viewpoints)
            {
                var node = NodeAt(pos);
                if (node == null)
                {
                    node = CreatAStarNode();
                    node.NodePoint = pos;
                    node.Forward = heuristic(endPos - pos);
                    node.Backward = pos.X - startPos.X + pos.Y - startPos.Y;
                    node.Cost = node.Forward + node.Backward;
                    AddNode(node);
                }
                if (_gool.Contains(pos))
                {
                    node.Adopt = true;
                    return true;
                }
            }

            foreach (var node in NodeCollection.Where(_ => !_.Value.Inspected).OrderBy(_ => _.Value.Cost).ThenBy(_ => _.Value.Forward))
            {
                node.Value.Inspected = true;
                if (ExecAStar(startPos, endPos, node.Value.NodePoint, minPos, maxPos, obstacles, viewpoint, heuristic))
                {
                    node.Value.Adopt = true;
                    return true;
                }
            }
            return false;
        }

        public static double Heuristic(Vector point)
        {
            return Math.Sqrt(point.X * point.X + point.Y * point.Y);
        }

        public static IEnumerable<NodePoint> Viewpoint(NodePoint point, NodePoint minPoint, NodePoint maxPoint, IEnumerable<NodeRect> rects)
        {
            int diff = 10;
            List<NodePoint> ret = new List<NodePoint>();
            if (point.X - diff >= minPoint.X)
            {
                var pos = new NodePoint(point.X - diff, point.Y);
                if (rects.All(_ => !_.Contains(pos)))
                {
                    ret.Add(pos);
                }
            }
            if (point.Y - diff >= minPoint.Y)
            {
                var pos = new NodePoint(point.X, point.Y - diff);
                if (rects.All(_ => !_.Contains(pos)))
                {
                    ret.Add(pos);
                }
            }
            if (point.X + diff <= maxPoint.X)
            {
                var pos = new NodePoint(point.X + diff, point.Y);
                if (rects.All(_ => !_.Contains(pos)))
                {
                    ret.Add(pos);
                }
            }
            if (point.Y + diff <= maxPoint.Y)
            {
                var pos = new NodePoint(point.X, point.Y + diff);
                if (rects.All(_ => !_.Contains(pos)))
                {
                    ret.Add(pos);
                }
            }
            return ret;
        }
    }
}
