using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace ObjectAreaLibrary
{
    using NodePoint = Point;
    using NodeRect = Rect;
    using VectorPos = Tuple<VectorType, Point>;
    using Viewpoint = Func<Tuple<VectorType, Point>, Point, Point, IEnumerable<Rect>, IEnumerable<Tuple<VectorType, Point>>>;
    using Heuristic = Func<Vector, double>;

    public enum VectorType
    {
        None,
        Left,
        Right,
        Top,
        Bottom,
    }

    public class AStarNode
    {
        public VectorPos NodePoint;
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
            NodeCollection.Add(node.NodePoint.Item2, node);
        }

        private AStarNode NodeAt(NodePoint point)
        {
            return NodeCollection.Where(_ => _.Key == point).Select(_ => _.Value).FirstOrDefault();
        }

        private IEnumerable<NodePoint> AdoptList()
        {
            return NodeCollection
                .Where(_ => _.Value.Adopt)
                .OrderBy(_ => _.Value.Cost)
                .Select(_ => _.Value.NodePoint.Item2);
        }

        private NodeRect _gool;
        private static readonly int _step = 10;

        public IEnumerable<NodePoint> Exec(NodePoint startPos, NodePoint endPos, NodePoint minPos, NodePoint maxPos, IEnumerable<NodeRect> obstacles, Viewpoint viewpoint, Heuristic heuristic)
        {
            _astarNodeIndex = 0;
            NodeCollection.Clear();
            _gool = new NodeRect(endPos, new Size(1, 1));
            _gool.Inflate(_step, _step);

            var rect = new NodeRect(startPos, endPos);
            rect.Inflate(_step, _step);
            var bounds = new NodeRect(minPos, maxPos);
            ExecAStar(startPos, endPos, new VectorPos(VectorType.Left, startPos), bounds.TopLeft, bounds.BottomRight, obstacles
                .Where(_ =>
                {
                    var diff = NodeRect.Intersect(_, rect);
                    return diff.Width > 0 || diff.Height > 0;
                }),
                viewpoint, heuristic);
            return AdoptList();
        }

        private bool ExecAStar(NodePoint startPos, NodePoint endPos, VectorPos vPos, NodePoint minPos, NodePoint maxPos, IEnumerable<NodeRect> obstacles, Viewpoint viewpoint, Heuristic heuristic)
        {
            var viewpoints = viewpoint(vPos, minPos, maxPos, obstacles);
            foreach (var pos in viewpoints)
            {
                var node = NodeAt(pos.Item2);
                if (node == null)
                {
                    node = CreatAStarNode();
                    node.NodePoint = pos;
                    node.Forward = heuristic(endPos - pos.Item2);
                    node.Backward = pos.Item2.X - startPos.X + pos.Item2.Y - startPos.Y;
                    node.Cost = node.Forward + node.Backward;
                    AddNode(node);
                }
                if (_gool.Contains(pos.Item2))
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

        public static IEnumerable<VectorPos> Viewpoint(VectorPos vPos, NodePoint minPoint, NodePoint maxPoint, IEnumerable<NodeRect> rects)
        {
            NodePoint? leftPos = null;
            if (vPos.Item2.X - _step >= minPoint.X)
            {
                var pos = new NodePoint(vPos.Item2.X - _step, vPos.Item2.Y);
                if (rects.All(_ => !_.Contains(pos)))
                {
                    leftPos = pos;
                }
            }
            NodePoint? topPos = null;
            if (vPos.Item2.Y - _step >= minPoint.Y)
            {
                var pos = new NodePoint(vPos.Item2.X, vPos.Item2.Y - _step);
                if (rects.All(_ => !_.Contains(pos)))
                {
                    topPos = pos;
                }
            }
            NodePoint? rightPos = null;
            if (vPos.Item2.X + _step <= maxPoint.X)
            {
                var pos = new NodePoint(vPos.Item2.X + _step, vPos.Item2.Y);
                if (rects.All(_ => !_.Contains(pos)))
                {
                    rightPos = pos;
                }
            }
            NodePoint? bottomPos = null;
            if (vPos.Item2.Y + _step <= maxPoint.Y)
            {
                var pos = new NodePoint(vPos.Item2.X, vPos.Item2.Y + _step);
                if (rects.All(_ => !_.Contains(pos)))
                {
                    bottomPos = pos;
                }
            }

            List<VectorPos> ret = new List<VectorPos>();
            switch (vPos.Item1)
            {
                case VectorType.Left:
                    if (leftPos.HasValue)
                        ret.Add(new VectorPos(VectorType.Left, leftPos.Value));
                    if (topPos.HasValue)
                        ret.Add(new VectorPos(VectorType.Top, topPos.Value));
                    if (bottomPos.HasValue)
                        ret.Add(new VectorPos(VectorType.Bottom, bottomPos.Value));
                    if (rightPos.HasValue)
                        ret.Add(new VectorPos(VectorType.Right, rightPos.Value));
                    break;
                case VectorType.Right:
                    if (rightPos.HasValue)
                        ret.Add(new VectorPos(VectorType.Right, rightPos.Value));
                    if (topPos.HasValue)
                        ret.Add(new VectorPos(VectorType.Top, topPos.Value));
                    if (bottomPos.HasValue)
                        ret.Add(new VectorPos(VectorType.Bottom, bottomPos.Value));
                    if (leftPos.HasValue)
                        ret.Add(new VectorPos(VectorType.Left, leftPos.Value));
                    break;
                case VectorType.Top:
                    if (topPos.HasValue)
                        ret.Add(new VectorPos(VectorType.Top, topPos.Value));
                    if (leftPos.HasValue)
                        ret.Add(new VectorPos(VectorType.Left, leftPos.Value));
                    if (rightPos.HasValue)
                        ret.Add(new VectorPos(VectorType.Right, rightPos.Value));
                    if (bottomPos.HasValue)
                        ret.Add(new VectorPos(VectorType.Bottom, bottomPos.Value));
                    break;
                case VectorType.Bottom:
                    if (bottomPos.HasValue)
                        ret.Add(new VectorPos(VectorType.Bottom, bottomPos.Value));
                    if (leftPos.HasValue)
                        ret.Add(new VectorPos(VectorType.Left, leftPos.Value));
                    if (rightPos.HasValue)
                        ret.Add(new VectorPos(VectorType.Right, rightPos.Value));
                    if (topPos.HasValue)
                        ret.Add(new VectorPos(VectorType.Top, topPos.Value));
                    break;
            }

            return ret;
        }
    }
}
