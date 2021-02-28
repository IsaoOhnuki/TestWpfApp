using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace ObjectAreaLibrary
{
    using NodePoint = Point;
    using NodeRect = Rect;
    using VectorPos = Tuple<VectorType, Point>;
    using Viewpoint = Func<Point, double, Rect, IEnumerable<Rect>, VectorType?, IEnumerable<Tuple<VectorType, Point>>>;
    using Heuristic = Func<Point, Point, double>;

    public enum VectorType
    {
        LeftToRight,
        RightToLeft,
        TopToBottom,
        BottomToTop,
    }

    public enum VectorDirection
    {
        Horizontal,
        Virtical,
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

        public enum ValueType
        {
            None,
            Adopt,
            Forward,
            Cost,
            Vector,
        }

        #region ToString
        public string AdoptString()
        {
            return Adopt ? "*" : "-";
        }
        public string IndexString()
        {
            return Index.ToString("0000");
        }
        public string ForwardString()
        {
            return Forward.ToString("0.00000000");
        }
        public string BackwardString()
        {
            return Backward.ToString("0.00000000");
        }
        public string CostString()
        {
            return Cost.ToString("0.00000000");
        }
        public string PointString()
        {
            return "(" + ((int)NodePoint.Item2.X).ToString("0000") + ":" + ((int)NodePoint.Item2.Y).ToString("0000") + ")";
        }
        public string VectorString()
        {
            switch (NodePoint.Item1)
            {
                case VectorType.LeftToRight:
                    return "→";
                case VectorType.TopToBottom:
                    return "↓";
                case VectorType.RightToLeft:
                    return "←";
                case VectorType.BottomToTop:
                    return "↑";
                default:
                    throw new ArgumentException();
            }
        }
        public string ToString(ValueType type)
        {
            switch (type)
            {
                case ValueType.Adopt:
                    return AdoptString();
                case ValueType.Forward:
                    return ForwardString();
                case ValueType.Vector:
                    return VectorString();
                case ValueType.Cost:
                    return CostString();
                default:
                    return ToString();
            }
        }
        public override string ToString()
        {
            var ret = new StringBuilder();
            ret.Append(AdoptString() + IndexString());
            ret.Append(";");
            ret.Append("F " + ForwardString());
            ret.Append(";");
            ret.Append("B " + BackwardString());
            ret.Append(";");
            ret.Append(VectorString());
            ret.Append(PointString());
            return ret.ToString();
        }
        #endregion
    }

    public class AStar
    {
        private static AStar _instance;
        public static AStar Instance { get => _instance ??= new AStar(); }

        private Dictionary<NodePoint, AStarNode> NodeCollection { get; } = new Dictionary<NodePoint, AStarNode>();

        private int _astarNodeIndex;
        private AStarNode CreatAStarNode(VectorPos vectorPos, double forward, double backward, AStarNode parent = null, bool adopt = false)
        {
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
            _astarNodeIndex = 0;
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

        public string GetCsv(int step, AStarNode.ValueType csvType)
        {
            var list = NodeCollection
                .OrderBy(_ => _.Value.NodePoint.Item2.Y)
                .ThenBy(_ => _.Value.NodePoint.Item2.X)
                .Select(_ => new Tuple<System.Drawing.Point, AStarNode>(new System.Drawing.Point((int)_.Value.NodePoint.Item2.X, (int)_.Value.NodePoint.Item2.Y), _.Value));
            var minX = NodeCollection.Min(_ => (int)_.Key.X);
            var minY = NodeCollection.Min(_ => (int)_.Key.Y);
            var maxX = NodeCollection.Max(_ => (int)_.Key.X);
            var maxY = NodeCollection.Max(_ => (int)_.Key.Y);

            var strY = new List<string>();
            for (var y = minY; y <= maxY; y += step)
            {
                var strX = new List<string>();
                for (var x = minX; x <= maxX; x += step)
                {
                    var node = list.Where(_ => _.Item1.X == x && _.Item1.Y == y).FirstOrDefault();
                    strX.Add((node != null) ? node.Item2.ToString(csvType) : "");
                }
                strY.Add(string.Join(',', strX));
            }
            return string.Join("\r\n", strY);
        }

        public bool Exec(NodePoint startPos, NodePoint endPos, double step, double inertia,
            NodeRect limitRect, IEnumerable<NodeRect> obstacles, Viewpoint viewpoint, Heuristic heuristic)
        {
            ClearNodes();
            SetGoal(endPos, step);

            VectorType vector = GetFirstVector(startPos, endPos);
            var astarBounds = NodeRect.Inflate(new NodeRect(startPos, endPos), step, step);

            var firstNode = CreatAStarNode(new VectorPos(vector, startPos), heuristic(startPos, endPos), GetBackward(startPos, endPos));
            AddNodes(firstNode);

            return ExecAStar(firstNode, endPos, step, inertia,
                limitRect,
                obstacles.Where(_ =>
                {
                    var diff = NodeRect.Intersect(_, astarBounds);
                    return diff.Width > 0 || diff.Height > 0;
                }),
                viewpoint, heuristic);
        }

        private bool ExecAStar(AStarNode node, NodePoint endPos, double step, double inertia,
            NodeRect limitRect, IEnumerable<NodeRect> obstacles, Viewpoint viewpoint, Heuristic heuristic)
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
                    while (node.Parent != null)
                    {
                        node.Adopt = true;
                        node = node.Parent;
                    }
                    result = true;
                    break;
                }

                var nodeVector = node.NodePoint.Item1;
                var nodePoint = node.NodePoint.Item2;

                var parentNode = node.Parent;
                if (parentNode != null)
                {
                    var parentNodeVector = parentNode.NodePoint.Item1;
                    AStarNode lastNode;
                    if (!EqualVectorDirection(nodeVector, parentNodeVector)
                        && (lastNode = GetLastVectorDirection(parentNode, GetVectorDirection(nodeVector))) != null
                        && lastNode.NodePoint.Item1 != nodeVector)
                    {
                        var nd = node.Parent;
                        if (ShortCut(lastNode.Parent, node, endPos, parentNodeVector, step, inertia, limitRect, obstacles, viewpoint, heuristic))
                        {
                            while (nd != lastNode)
                            {
                                nd.Forward += inertia;
                                nd = nd.Parent;
                            }
                            nodeVector = node.NodePoint.Item1;
                            nodePoint = node.NodePoint.Item2;
                        }
                    }
                }

                var newViewpPoints = viewpoint(nodePoint, step, limitRect, obstacles, null);
                foreach (var newViewPoint in newViewpPoints)
                {
                    var newNodeVector = newViewPoint.Item1;
                    var newNodePos = newViewPoint.Item2;
                    var newNode = NodeAt(newNodePos);
                    if (newNode == null)
                    {
                        var hVal = heuristic(newNodePos, endPos);
                        hVal -= nodeVector == newNodeVector ? inertia : 0;
                        newNode = CreatAStarNode(newViewPoint, hVal, GetBackward(newNodePos, endPos), parent: node);
                        AddNodes(newNode);
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

        private bool ShortCut(AStarNode nodeL, AStarNode nodeR, NodePoint goalPos, VectorType priorityVector, double step, double inertia,
            NodeRect limitRect, IEnumerable<NodeRect> obstacles, Viewpoint viewpoint, Heuristic heuristic)
        {
            bool result = true;
            var node = nodeL;
            var endPos = nodeR.NodePoint.Item2;
            while (node != null)
            {
                var nodeVector = node.NodePoint.Item1;
                var nodePoint = node.NodePoint.Item2;
                var viewpoints = viewpoint(nodePoint, step, limitRect, obstacles, priorityVector);
                if (viewpoints.Count() == 0)
                    break;

                var pos = viewpoints.First();
                if (pos.Item2 == endPos)
                {
                    nodeR.NodePoint = pos;
                    nodeR.Parent = node;
                    result = true;
                    break;
                }

                var vector = pos.Item1;
                var viewPos = pos.Item2;
                var newNode = NodeAt(viewPos);
                if (newNode == null)
                {
                    newNode = CreatAStarNode(pos, heuristic(viewPos, goalPos) - inertia, GetBackward(viewPos, goalPos), parent: node);
                    AddNodes(newNode);
                }
                newNode.NodePoint = pos;
                newNode.Inspected = true;
                newNode.Parent = node;
                node = newNode;
            }

            return result;
        }

        private AStarNode GetLastVectorDirection(AStarNode node, VectorDirection vectorDirection)
        {
            for (int idx = 0; node != null; ++idx)
            {
                var nodeVector = node.NodePoint.Item1;
                if (EqualVectorDirection(nodeVector, vectorDirection))
                {
                    break;
                }
                node = node.Parent;
            }
            return node;
        }

        private VectorDirection GetVectorDirection(VectorType vectorType)
        {
            switch (vectorType)
            {
                case VectorType.LeftToRight:
                    return VectorDirection.Horizontal;
                case VectorType.TopToBottom:
                    return VectorDirection.Virtical;
                case VectorType.RightToLeft:
                    return VectorDirection.Horizontal;
                case VectorType.BottomToTop:
                    return VectorDirection.Virtical;
                default:
                    throw new ArgumentException();
            }
        }

        private VectorDirection OppositeVectorDirection(VectorType vectorType)
        {
            return OppositeVectorDirection(GetVectorDirection(vectorType));
        }

        private VectorDirection OppositeVectorDirection(VectorDirection direction)
        {
            if (direction == VectorDirection.Virtical)
            {
                return VectorDirection.Horizontal;
            }
            else if (direction == VectorDirection.Horizontal)
            {
                return VectorDirection.Virtical;
            }
            throw new ArgumentException();
        }

        private bool EqualVectorDirection(VectorType vectorTypeL, VectorType vectorTypeR)
        {
            if (GetVectorDirection(vectorTypeL) == GetVectorDirection(vectorTypeR))
            {
                return true;
            }
            return false;
        }

        private bool EqualVectorDirection(VectorType vectorType, VectorDirection vectorDirection)
        {
            if (GetVectorDirection(vectorType) == vectorDirection)
            {
                return true;
            }
            return false;
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
                type = Math.Abs(vector.X) > Math.Abs(vector.Y) ? VectorType.LeftToRight : VectorType.TopToBottom;
            }
            else if (vector.X < 0 && vector.Y >= 0)
            {
                type = Math.Abs(vector.X) > Math.Abs(vector.Y) ? VectorType.RightToLeft : VectorType.TopToBottom;
            }
            else if (vector.X >= 0 && vector.Y < 0)
            {
                type = Math.Abs(vector.X) > Math.Abs(vector.Y) ? VectorType.LeftToRight : VectorType.BottomToTop;
            }
            else // if(vector.X < 0 && vector.Y < 0)
            {
                type = Math.Abs(vector.X) > Math.Abs(vector.Y) ? VectorType.RightToLeft : VectorType.BottomToTop;
            }

            return type;
        }

        public static IEnumerable<VectorPos> Viewpoint(NodePoint point, double step, NodeRect limitRect, IEnumerable<NodeRect> rects, VectorType? priorityVector = null)
        {
            bool noLimmit = limitRect.Width == 0 || limitRect.Height == 0;
            var rectContains = rects.Count() > 0;
            List<VectorPos> ret = new List<VectorPos>();
            if ((!priorityVector.HasValue || priorityVector == VectorType.LeftToRight)
                && (noLimmit || point.X + step <= limitRect.BottomRight.X))
            {
                var pos = new NodePoint(point.X + step, point.Y);
                if (!rectContains || !rects.Any(_ => _.Contains(pos)))
                {
                    ret.Add(new VectorPos(VectorType.LeftToRight, pos));
                }
            }
            if ((!priorityVector.HasValue || priorityVector == VectorType.TopToBottom)
                && (noLimmit || point.Y + step <= limitRect.BottomRight.Y))
            {
                var pos = new NodePoint(point.X, point.Y + step);
                if (!rectContains || !rects.Any(_ => _.Contains(pos)))
                {
                    ret.Add(new VectorPos(VectorType.TopToBottom, pos));
                }
            }
            if ((!priorityVector.HasValue || priorityVector == VectorType.RightToLeft)
                && (noLimmit || point.X - step >= limitRect.TopLeft.X))
            {
                var pos = new NodePoint(point.X - step, point.Y);
                if (!rectContains || !rects.Any(_ => _.Contains(pos)))
                {
                    ret.Add(new VectorPos(VectorType.RightToLeft, pos));
                }
            }
            if ((!priorityVector.HasValue || priorityVector == VectorType.BottomToTop)
                && (noLimmit || point.Y - step >= limitRect.TopLeft.Y))
            {
                var pos = new NodePoint(point.X, point.Y - step);
                if (!rectContains || !rects.Any(_ => _.Contains(pos)))
                {
                    ret.Add(new VectorPos(VectorType.BottomToTop, pos));
                }
            }

            return ret;
        }
    }
}
