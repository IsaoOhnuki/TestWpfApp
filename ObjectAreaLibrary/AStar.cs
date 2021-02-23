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

        private int _astarNodeIndex;
        public AStarNode CreatAStarNode()
        {
            return new AStarNode()
            {
                Index = ++_astarNodeIndex,
            };
        }

        public AStarNode CreatAStarNode(VectorPos vectorPos, double forward, double backward, bool adopt = false)
        {
            return new AStarNode()
            {
                Index = ++_astarNodeIndex,
                NodePoint = vectorPos,
                Forward = forward,
                Backward = backward,
                Cost = forward + backward,
                Adopt = adopt,
            };
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

        public IEnumerable<NodePoint> AdoptList()
        {
            return NodeCollection
                .Where(_ => _.Value.Adopt)
                .OrderBy(_ => _.Value.Index)
                .Select(_ => _.Value.NodePoint.Item2);
        }

        private NodeRect _gool;
        private static readonly int _step = 10;

        public bool Exec(NodePoint startPos, NodePoint endPos, NodeRect limitRect, IEnumerable<NodeRect> obstacles, Viewpoint viewpoint, Heuristic heuristic)
        {
            NodeCollection.Clear();
            _astarNodeIndex = 0;
            _gool = new NodeRect(endPos, new Size(1, 1));
            _gool.Inflate(_step, _step);

            var astarBounds = NodeRect.Inflate(new NodeRect(startPos, endPos), _step, _step);

            var firstNode = CreatAStarNode(new VectorPos(VectorType.Left, startPos), heuristic(endPos - startPos), Math.Abs(endPos.X - startPos.X + endPos.Y - startPos.Y));
            AddNode(firstNode);

            return ExecAStar(firstNode, endPos, _step, limitRect, obstacles
                .Where(_ =>
                {
                    var diff = NodeRect.Intersect(_, astarBounds);
                    return diff.Width > 0 || diff.Height > 0;
                }),
                viewpoint, heuristic);
        }

        private bool ExecAStar(AStarNode node, NodePoint endPos, int step, NodeRect limitRect, IEnumerable<NodeRect> obstacles, Viewpoint viewpoint, Heuristic heuristic)
        {
            node.Inspected = true;
            if (_gool.Contains(node.NodePoint.Item2))
            {
                var residue = endPos - node.NodePoint.Item2;
                if (residue.X < residue.Y)
                {
                    AddNode(CreatAStarNode(new VectorPos(node.NodePoint.Item1, new NodePoint(endPos.X, node.NodePoint.Item2.Y)), 0, 0, true));
                }
                else
                {
                    AddNode(CreatAStarNode(new VectorPos(node.NodePoint.Item1, new NodePoint(node.NodePoint.Item2.X, endPos.Y)), 0, 0, true));
                }
                AddNode(CreatAStarNode(new VectorPos(node.NodePoint.Item1, endPos), 0, 0, true));
                node.Adopt = true;
                return true;
            }

            var viewpoints = viewpoint(node.NodePoint, step, limitRect, obstacles);
            var newNodes = new List<(AStarNode, bool, bool)>();
            foreach (var pos in viewpoints)
            {
                var newNode = NodeAt(pos.Item2);
                var responsibility = false;
                if (newNode == null)
                {
                    newNode = CreatAStarNode(pos, heuristic(endPos - pos.Item2), Math.Abs(endPos.X - pos.Item2.X + endPos.Y - pos.Item2.Y));
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
                result = ExecAStar(newNode.Item1, endPos, step, limitRect, obstacles, viewpoint, heuristic);
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
