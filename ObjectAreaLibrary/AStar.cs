using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace ObjectAreaLibrary
{
    using NodePoint = Point;
    using NodeRect = Rect;
    using VectorPos = Tuple<VectorType, Point>;
    using Viewpoint = Func<Tuple<VectorType, Point>, int, Rect, IEnumerable<Rect>, IEnumerable<Tuple<VectorType, Point>>>;
    using Heuristic = Func<Point, Point, double>;

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
        public AStarNode Parent;
    }

    public class AStar
    {
        private static AStar _instance;
        public static AStar Instance { get => _instance ??= new AStar(); }

        private Dictionary<NodePoint, AStarNode> NodeCollection { get; } = new Dictionary<NodePoint, AStarNode>();

        public static int Step { get; set; } = 10;

        private int _astarNodeIndex;
        private AStarNode CreatAStarNode(VectorPos vectorPos, double forward, double backward, AStarNode parent = null, bool adopt = false, bool clear = false)
        {
            if (clear)
            {
                _astarNodeIndex = 0;
            }
            return new AStarNode()
            {
                Index = ++_astarNodeIndex,
                NodePoint = vectorPos,
                Forward = forward,
                Backward = backward,
                Cost = forward + backward,
                Adopt = adopt,
                Parent = parent,
            };
        }

        private void ClearNodes()
        {
            NodeCollection.Clear();
        }

        private void AddNodes(AStarNode node)
        {
            if (!NodeCollection.ContainsKey(node.NodePoint.Item2))
            {
                NodeCollection.Add(node.NodePoint.Item2, node);
            }
        }

        private AStarNode NodeAt(NodePoint point)
        {
            return NodeCollection.ContainsKey(point) ? NodeCollection[point] : null;
        }

        public IEnumerable<NodePoint> AdoptList()
        {
            return NodeCollection
                .Where(_ => _.Value.Adopt)
                .OrderBy(_ => _.Value.Index)
                .Select(_ => _.Value.NodePoint.Item2);
        }

        public bool Exec(NodePoint startPos, NodePoint endPos, NodeRect limitRect, IEnumerable<NodeRect> obstacles, Viewpoint viewpoint, Heuristic heuristic)
        {
            ClearNodes();
            SetGoal(endPos, Step);

            var astarBounds = NodeRect.Inflate(new NodeRect(startPos, endPos), Step, Step);

            var firstNode = CreatAStarNode(new VectorPos(VectorType.Left, startPos), heuristic(startPos, endPos), Math.Abs(GetBackward(startPos, endPos)), clear: false);
            AddNodes(firstNode);

            return ExecAStar(firstNode, endPos, Step, limitRect, obstacles
                .Where(_ =>
                {
                    var diff = NodeRect.Intersect(_, astarBounds);
                    return diff.Width > 0 || diff.Height > 0;
                }),
                viewpoint, heuristic);
        }

        private bool ExecAStar(AStarNode node, NodePoint endPos, int step, NodeRect limitRect, IEnumerable<NodeRect> obstacles, Viewpoint viewpoint, Heuristic heuristic)
        {
            if (obstacles.Any(_ => _.Contains(node.NodePoint.Item2) || _.Contains(endPos)))
            {
                return false;
            }

            var result = false;
            while (node != null && !result)
            {
                node.Inspected = true;
                if (CheckGoal(node, endPos))
                {
                    node.Adopt = true;
                    while (node.Parent != null)
                    {
                        node = node.Parent;
                        node.Adopt = true;
                    }
                    result = true;
                    break;
                }

                var viewpoints = viewpoint(node.NodePoint, step, limitRect, obstacles);
                foreach (var pos in viewpoints)
                {
                    var viewPpos = pos.Item2;
                    var newNode = NodeAt(viewPpos);
                    if (newNode == null)
                    {
                        AddNodes(CreatAStarNode(pos, heuristic(viewPpos, endPos), Math.Abs(GetBackward(viewPpos, endPos)), parent: node));
                    }
                }

                node = NodeCollection.Select(_ => _.Value).Where(_ => !_.Inspected).OrderBy(_ => _.Cost).ThenByDescending(_ => _.Backward).FirstOrDefault();
            }

            return result;
        }

        private NodeRect _goal;
        private void SetGoal(NodePoint endPos, int step)
        {
            _goal = new NodeRect(endPos, new Size(1, 1));
            _goal.Inflate(step, step);
        }

        private bool CheckGoal(AStarNode node, NodePoint endPos)
        {
            var nodePos = node.NodePoint.Item2;
            var result = _goal.Contains(nodePos);
            if (result)
            {
                var vctType = node.NodePoint.Item1;
                var residue = endPos - nodePos;
                if (Math.Abs(residue.X) < Math.Abs(residue.Y))
                {
                    AddNodes(CreatAStarNode(new VectorPos(vctType, new NodePoint(nodePos.X, endPos.Y)), 0, 0, adopt: true));
                }
                else
                {
                    AddNodes(CreatAStarNode(new VectorPos(vctType, new NodePoint(endPos.X, nodePos.Y)), 0, 0, adopt: true));
                }
                AddNodes(CreatAStarNode(new VectorPos(vctType, endPos), 0, 0, adopt: true));
            }
            return result;
        }

        private double GetBackward(NodePoint startPos, NodePoint endPos)
        {
            return (endPos.X - startPos.X) + (endPos.Y - startPos.Y);
        }

        public static double Heuristic(NodePoint startPos, NodePoint endPos)
        {
            var point = endPos - startPos;
            return Math.Sqrt(point.X * point.X + point.Y * point.Y);
        }

        public static IEnumerable<VectorPos> Viewpoint(VectorPos vPos, int step, NodeRect limitRect, IEnumerable<NodeRect> rects)
        {
            bool noLimmit = limitRect.Width == 0 || limitRect.Height == 0;
            var rectContains = rects.Count() > 0;
            NodePoint? leftPos = null;
            if (noLimmit || vPos.Item2.X + step <= limitRect.BottomRight.X)
            {
                var pos = new NodePoint(vPos.Item2.X + step, vPos.Item2.Y);
                if (!rectContains || !rects.Any(_ => _.Contains(pos)))
                {
                    leftPos = pos;
                }
            }
            NodePoint? topPos = null;
            if (noLimmit || vPos.Item2.Y + step <= limitRect.BottomRight.Y)
            {
                var pos = new NodePoint(vPos.Item2.X, vPos.Item2.Y + step);
                if (!rectContains || !rects.Any(_ => _.Contains(pos)))
                {
                    topPos = pos;
                }
            }
            NodePoint? rightPos = null;
            if (noLimmit || vPos.Item2.X - step >= limitRect.TopLeft.X)
            {
                var pos = new NodePoint(vPos.Item2.X - step, vPos.Item2.Y);
                if (!rectContains || !rects.Any(_ => _.Contains(pos)))
                {
                    rightPos = pos;
                }
            }
            NodePoint? bottomPos = null;
            if (noLimmit || vPos.Item2.Y - step >= limitRect.TopLeft.Y)
            {
                var pos = new NodePoint(vPos.Item2.X, vPos.Item2.Y - step);
                if (!rectContains || !rects.Any(_ => _.Contains(pos)))
                {
                    bottomPos = pos;
                }
            }

            List<VectorPos> ret = new List<VectorPos>();
            if (leftPos.HasValue)
                ret.Add(new VectorPos(VectorType.Left, leftPos.Value));
            if (topPos.HasValue)
                ret.Add(new VectorPos(VectorType.Top, topPos.Value));
            if (rightPos.HasValue)
                ret.Add(new VectorPos(VectorType.Right, rightPos.Value));
            if (bottomPos.HasValue)
                ret.Add(new VectorPos(VectorType.Bottom, bottomPos.Value));
            //switch (vPos.Item1)
            //{
            //    case VectorType.Left:
            //        if (leftPos.HasValue)
            //            ret.Add(new VectorPos(VectorType.Left, leftPos.Value));
            //        if (topPos.HasValue)
            //            ret.Add(new VectorPos(VectorType.Top, topPos.Value));
            //        if (bottomPos.HasValue)
            //            ret.Add(new VectorPos(VectorType.Bottom, bottomPos.Value));
            //        if (rightPos.HasValue)
            //            ret.Add(new VectorPos(VectorType.Right, rightPos.Value));
            //        break;
            //    case VectorType.Right:
            //        if (rightPos.HasValue)
            //            ret.Add(new VectorPos(VectorType.Right, rightPos.Value));
            //        if (topPos.HasValue)
            //            ret.Add(new VectorPos(VectorType.Top, topPos.Value));
            //        if (bottomPos.HasValue)
            //            ret.Add(new VectorPos(VectorType.Bottom, bottomPos.Value));
            //        if (leftPos.HasValue)
            //            ret.Add(new VectorPos(VectorType.Left, leftPos.Value));
            //        break;
            //    case VectorType.Top:
            //        if (topPos.HasValue)
            //            ret.Add(new VectorPos(VectorType.Top, topPos.Value));
            //        if (leftPos.HasValue)
            //            ret.Add(new VectorPos(VectorType.Left, leftPos.Value));
            //        if (rightPos.HasValue)
            //            ret.Add(new VectorPos(VectorType.Right, rightPos.Value));
            //        if (bottomPos.HasValue)
            //            ret.Add(new VectorPos(VectorType.Bottom, bottomPos.Value));
            //        break;
            //    case VectorType.Bottom:
            //        if (bottomPos.HasValue)
            //            ret.Add(new VectorPos(VectorType.Bottom, bottomPos.Value));
            //        if (leftPos.HasValue)
            //            ret.Add(new VectorPos(VectorType.Left, leftPos.Value));
            //        if (rightPos.HasValue)
            //            ret.Add(new VectorPos(VectorType.Right, rightPos.Value));
            //        if (topPos.HasValue)
            //            ret.Add(new VectorPos(VectorType.Top, topPos.Value));
            //        break;
            //}

            return ret;
        }
    }
}
