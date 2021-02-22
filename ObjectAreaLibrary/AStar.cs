using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace ObjectAreaLibrary
{
    using NodePoint = Point;
    using NodeRect = Rect;
    using VectorPos = Tuple<VectorType, Point>;
    using Viewpoint = Func<Tuple<VectorType, Point>, Point, Point, int, IEnumerable<Rect>, IEnumerable<Tuple<VectorType, Point>>>;
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
        private static AStar _instance;
        public static AStar Instance { get => _instance ??= new AStar(); }

        private Dictionary<NodePoint, AStarNode> NodeCollection { get; } = new Dictionary<NodePoint, AStarNode>();

        public AStarNode CreatAStarNode()
        {
            return new AStarNode();
        }

        private void AddNode(AStarNode node)
        {
            if (!NodeCollection.ContainsKey(node.NodePoint.Item2))
            {
                NodeCollection.Add(node.NodePoint.Item2, node);
            }
        }

        private AStarNode NodeAt(NodePoint point)
        {
            return NodeCollection.Where(_ => _.Key == point).Select(_ => _.Value).FirstOrDefault();
        }

        public IEnumerable<NodePoint> AdoptList(Vector diff)
        {
            return NodeCollection
                .Where(_ => _.Value.Adopt)
                .OrderBy(_ => _.Value.Cost)
                .Select(_ => _.Value.NodePoint.Item2 - diff);
        }

        private NodeRect _gool;
        private static readonly int _step = 10;

        public bool Exec(NodePoint startPos, NodePoint endPos, NodePoint minPos, NodePoint maxPos, IEnumerable<NodeRect> obstacles, Viewpoint viewpoint, Heuristic heuristic)
        {
            NodeCollection.Clear();
            _gool = new NodeRect(endPos, new Size(1, 1));
            _gool.Inflate(12, 12);

            var astarBounds = NodeRect.Inflate(new NodeRect(startPos, endPos), _step, _step);
            var bounds = new NodeRect(minPos, maxPos);

            var firstNode = CreatAStarNode();
            firstNode.NodePoint = new VectorPos(VectorType.Left, startPos);
            firstNode.Forward = heuristic(endPos - startPos);
            firstNode.Backward = Math.Abs(endPos.X - startPos.X + endPos.Y - startPos.Y);
            firstNode.Cost = firstNode.Forward + firstNode.Backward;
            AddNode(firstNode);

            return ExecAStar(firstNode, endPos, bounds.TopLeft, bounds.BottomRight, _step, obstacles
                .Where(_ =>
                {
                    var diff = NodeRect.Intersect(_, astarBounds);
                    return diff.Width > 0 || diff.Height > 0;
                }),
                viewpoint, heuristic);
        }

        private bool ExecAStar(AStarNode node, NodePoint endPos, NodePoint minPos, NodePoint maxPos, int step, IEnumerable<NodeRect> obstacles, Viewpoint viewpoint, Heuristic heuristic)
        {
            node.Inspected = true;
            if (_gool.Contains(node.NodePoint.Item2))
            {
                node.Adopt = true;
                return true;
            }

            var viewpoints = viewpoint(node.NodePoint, minPos, maxPos, step, obstacles);
            var newNodes = new List<(AStarNode, bool, bool)>();
            foreach (var pos in viewpoints)
            {
                var newNode = NodeAt(pos.Item2);
                var responsibility = false;
                if (newNode == null)
                {
                    newNode = CreatAStarNode();
                    newNode.NodePoint = pos;
                    newNode.Forward = heuristic(endPos - pos.Item2);
                    newNode.Backward = Math.Abs(endPos.X - pos.Item2.X + endPos.Y - pos.Item2.Y);
                    newNode.Cost = newNode.Forward + newNode.Backward;
                    responsibility = true;
                }

                var possibility = NodeCollection
                    .Where(_ => !_.Value.Inspected)
                    .All(_ => newNode.Cost < _.Value.Cost || (newNode.Cost == _.Value.Cost && newNode.Backward > _.Value.Backward));

                newNodes.Add((newNode, possibility, responsibility));
            }

            foreach (var newNode in newNodes)
            {
                AddNode(newNode.Item1);
            }

            var result = false;
            foreach (var newNode in newNodes.Where(_ => !_.Item1.Inspected && _.Item2).OrderBy(_ => _.Item1.Cost).ThenByDescending(_ => _.Item1.Backward))
            {
                result = ExecAStar(newNode.Item1, endPos, minPos, maxPos, step, obstacles, viewpoint, heuristic);
                if (result)
                {
                    node.Adopt = true;
                    break;
                }
            }

            return result;
        }

        public static double Heuristic(Vector point)
        {
            return Math.Sqrt(point.X * point.X + point.Y * point.Y);
        }

        public static IEnumerable<VectorPos> Viewpoint(VectorPos vPos, NodePoint minPoint, NodePoint maxPoint, int step, IEnumerable<NodeRect> rects)
        {
            var rectContains = rects.Count() > 0;
            NodePoint? leftPos = null;
            if (vPos.Item2.X + step <= maxPoint.X)
            {
                var pos = new NodePoint(vPos.Item2.X + step, vPos.Item2.Y);
                if (!rectContains || !rects.Any(_ => _.Contains(pos)))
                {
                    leftPos = pos;
                }
            }
            NodePoint? topPos = null;
            if (vPos.Item2.Y + step <= maxPoint.Y)
            {
                var pos = new NodePoint(vPos.Item2.X, vPos.Item2.Y + step);
                if (!rectContains || !rects.Any(_ => _.Contains(pos)))
                {
                    topPos = pos;
                }
            }
            NodePoint? rightPos = null;
            if (vPos.Item2.X - step >= minPoint.X)
            {
                var pos = new NodePoint(vPos.Item2.X - step, vPos.Item2.Y);
                if (!rectContains || !rects.Any(_ => _.Contains(pos)))
                {
                    rightPos = pos;
                }
            }
            NodePoint? bottomPos = null;
            if (vPos.Item2.Y - step >= minPoint.Y)
            {
                var pos = new NodePoint(vPos.Item2.X, vPos.Item2.Y - step);
                if (!rectContains || !rects.Any(_ => _.Contains(pos)))
                {
                    bottomPos = pos;
                }
            }

            List<VectorPos> ret = new List<VectorPos>();
            //if (leftPos.HasValue)
            //    ret.Add(new VectorPos(VectorType.Left, leftPos.Value));
            //if (topPos.HasValue)
            //    ret.Add(new VectorPos(VectorType.Top, topPos.Value));
            //if (rightPos.HasValue)
            //    ret.Add(new VectorPos(VectorType.Right, rightPos.Value));
            //if (bottomPos.HasValue)
            //    ret.Add(new VectorPos(VectorType.Bottom, bottomPos.Value));
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
