using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace ObjectAreaLibrary
{
    using NodePoint = Point;
    using NodeRect = Rect;
    using VectorPos = Tuple<VectorType, Point>;
    using Viewpoint = Func<Point, double, Rect, IEnumerable<Rect>, IEnumerable<Tuple<VectorType, Point>>>;
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

        public bool Exec(NodePoint startPos, NodePoint endPos, double Step, double inertia, NodeRect limitRect, IEnumerable<NodeRect> obstacles, Viewpoint viewpoint, Heuristic heuristic)
        {
            ClearNodes();
            SetGoal(endPos, step);

            VectorType vector = GetFirstVector(startPos, endPos);
            var astarBounds = NodeRect.Inflate(new NodeRect(startPos, endPos), Step, Step);

            var firstNode = CreatAStarNode(new VectorPos(vector, startPos), heuristic(startPos, endPos), GetBackward(startPos, endPos), clear: true);
            AddNodes(firstNode);

            return ExecAStar(firstNode, endPos, Step, inertia, limitRect, obstacles
                .Where(_ =>
                {
                    var diff = NodeRect.Intersect(_, astarBounds);
                    return diff.Width > 0 || diff.Height > 0;
                }),
                viewpoint, heuristic);
        }

        private bool ExecAStar(AStarNode node, NodePoint endPos, double step, double inertia, NodeRect limitRect, IEnumerable<NodeRect> obstacles, Viewpoint viewpoint, Heuristic heuristic)
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

                var viewpoints = viewpoint(node.NodePoint.Item2, step, limitRect, obstacles);
                foreach (var pos in viewpoints)
                {
                    var vector = pos.Item1;
                    var viewPos = pos.Item2;
                    var newNode = NodeAt(viewPos);
                    if (newNode == null)
                    {
                        var hVal = heuristic(viewPos, endPos);
                        hVal -= node.NodePoint.Item1 == vector ? inertia : 0;
                        AddNodes(CreatAStarNode(pos, hVal, GetBackward(viewPos, endPos), parent: node));
                    }
                }

                node = NodeCollection
                    .Select(_ => _.Value)
                    .Where(_ => !_.Inspected)
                    .OrderBy(_ => _.Cost)
                    .ThenByDescending(_ => _.Backward)
                    .FirstOrDefault();
            }

            return result;
        }

        private NodeRect _goal;
        private void SetGoal(NodePoint endPos, double step)
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
            return (startPos.X < endPos.X ? endPos.X - startPos.X : startPos.X - endPos.X)
                + (startPos.Y < endPos.Y ? endPos.Y - startPos.Y : startPos.Y - endPos.Y);
        }

        public static double Heuristic(NodePoint startPos, NodePoint endPos)
        {
            var point = endPos - startPos;
            return Math.Sqrt(point.X * point.X + point.Y * point.Y);
        }

        private VectorType GetFirstVector(NodePoint startPos, NodePoint endPos)
        {
            VectorType type;
            Vector vector = endPos - startPos;
            if (vector.X >= 0 && vector.Y >= 0)
            {
                type = Math.Abs(vector.X) > Math.Abs(vector.Y) ? VectorType.Left : VectorType.Top;
            }
            else if (vector.X < 0 && vector.Y >= 0)
            {
                type = Math.Abs(vector.X) > Math.Abs(vector.Y) ? VectorType.Right : VectorType.Top;
            }
            else if (vector.X >= 0 && vector.Y < 0)
            {
                type = Math.Abs(vector.X) > Math.Abs(vector.Y) ? VectorType.Left : VectorType.Bottom;
            }
            else //if(vector.X < 0 && vector.Y < 0)
            {
                type = Math.Abs(vector.X) > Math.Abs(vector.Y) ? VectorType.Right : VectorType.Bottom;
            }

            return type;
        }

        public static IEnumerable<VectorPos> Viewpoint(NodePoint vPos, double step, NodeRect limitRect, IEnumerable<NodeRect> rects)
        {
            bool noLimmit = limitRect.Width == 0 || limitRect.Height == 0;
            var rectContains = rects.Count() > 0;
            List<VectorPos> ret = new List<VectorPos>();
            if (noLimmit || vPos.X + step <= limitRect.BottomRight.X)
            {
                var pos = new NodePoint(point.X + step, point.Y);
                if (!rectContains || !rects.Any(_ => _.Contains(pos)))
                {
                    ret.Add(new VectorPos(VectorType.Left, pos));
                }
            }
            if (noLimmit || point.Y + step <= limitRect.BottomRight.Y)
            {
                var pos = new NodePoint(point.X, point.Y + step);
                if (!rectContains || !rects.Any(_ => _.Contains(pos)))
                {
                    ret.Add(new VectorPos(VectorType.Top, pos));
                }
            }
            if (noLimmit || point.X - step >= limitRect.TopLeft.X)
            {
                var pos = new NodePoint(point.X - step, point.Y);
                if (!rectContains || !rects.Any(_ => _.Contains(pos)))
                {
                    ret.Add(new VectorPos(VectorType.Right, pos));
                }
            }
            if (noLimmit || point.Y - step >= limitRect.TopLeft.Y)
            {
                var pos = new NodePoint(point.X, point.Y - step);
                if (!rectContains || !rects.Any(_ => _.Contains(pos)))
                {
                    ret.Add(new VectorPos(VectorType.Bottom, pos));
                }
            }

            return ret;
        }
    }
}
