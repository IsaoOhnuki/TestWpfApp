using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace ObjectAreaLibrary
{
    using NodePoint = Point;
    using NodeRect = Rect;
    using VectorPos = Tuple<VectorType, Point>;
    using Viewpoint = Func<Point, double, Rect, IEnumerable<Rect>, VectorType, IEnumerable<Tuple<VectorType, Point>>>;
    using Heuristic = Func<Point, Point, double>;

    public enum VectorType
    {
        None,
        LeftToRight,
        RightToLeft,
        TopToBottom,
        BottomToTop,
    }

    public enum VectorDirection
    {
        None,
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
                        && (
                            (CheckVectorDirection(nodeVector) == VectorDirection.Horizontal && (lastNode = GetLastVectorDirection(parentNode, VectorDirection.Horizontal)) != null)
                            || (CheckVectorDirection(nodeVector) == VectorDirection.Virtical && (lastNode = GetLastVectorDirection(parentNode, VectorDirection.Virtical)) != null)
                        )
                        && lastNode.NodePoint.Item1 != nodeVector && (lastNode.Parent == null || lastNode.NodePoint.Item1 == lastNode.Parent.NodePoint.Item1))
                    {
                        ShortCut(lastNode.Parent, node, parentNodeVector, step, inertia, limitRect, obstacles, viewpoint, heuristic);
                    }
                }

                var newViewpPoints = viewpoint(nodePoint, step, limitRect, obstacles, VectorType.None);
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

        private void ShortCut(AStarNode nodeL, AStarNode nodeR, VectorType priorityVector, double step, double inertia,
            NodeRect limitRect, IEnumerable<NodeRect> obstacles, Viewpoint viewpoint, Heuristic heuristic)
        {
            var node = nodeL;
            var endPos = nodeR.NodePoint.Item2;
            while (node != null)
            {
                if (node.NodePoint.Item2.X != endPos.X && node.NodePoint.Item2.Y != endPos.Y)
                    break;

                var nodeVector = node.NodePoint.Item1;
                var nodePoint = node.NodePoint.Item2;
                var viewpoints = viewpoint(nodePoint, step, limitRect, obstacles, priorityVector);
                if (viewpoints.Count() == 0)
                    break;

                var pos = viewpoints.First();
                if (pos.Item2 == endPos)
                {
                    nodeR.Parent = node;
                    break;
                }

                var vector = pos.Item1;
                var viewPos = pos.Item2;
                var newNode = NodeAt(viewPos);
                if (newNode == null)
                {
                    var hVal = heuristic(viewPos, endPos);
                    hVal -= inertia;
                    newNode = CreatAStarNode(pos, hVal, GetBackward(viewPos, endPos), parent: node);
                }
                node = newNode;
            }
        }

        private AStarNode GetLastVectorDirection(AStarNode node, VectorDirection vectorDirection)
        {
            while (node != null)
            {
                var nodeVector = node.NodePoint.Item1;
                if ((vectorDirection == VectorDirection.Horizontal && nodeVector == VectorType.LeftToRight || nodeVector == VectorType.RightToLeft)
                    || (vectorDirection == VectorDirection.Virtical && nodeVector == VectorType.TopToBottom || nodeVector == VectorType.BottomToTop))
                {
                    break;
                }
                node = node.Parent;
            }
            return node;
        }

        private VectorDirection CheckVectorDirection(VectorType vectorTypeL)
        {
            if (vectorTypeL == VectorType.LeftToRight || vectorTypeL == VectorType.RightToLeft)
            {
                return VectorDirection.Horizontal;
            }
            if (vectorTypeL == VectorType.TopToBottom || vectorTypeL == VectorType.BottomToTop)
            {
                return VectorDirection.Virtical;
            }
            return VectorDirection.None;
        }

        private bool EqualVectorDirection(VectorType vectorTypeL, VectorType vectorTypeR)
        {
            if ((vectorTypeL == VectorType.LeftToRight || vectorTypeL == VectorType.RightToLeft)
                && (vectorTypeR == VectorType.LeftToRight || vectorTypeR == VectorType.RightToLeft))
            {
                return true;
            }
            if ((vectorTypeL == VectorType.TopToBottom || vectorTypeL == VectorType.BottomToTop)
                && (vectorTypeR == VectorType.TopToBottom || vectorTypeR == VectorType.BottomToTop))
            {
                return true;
            }
            return false;
        }

        private bool EqualVectorDirection(VectorType vectorType, VectorDirection vectorDirection)
        {
            if ((vectorType == VectorType.LeftToRight || vectorType == VectorType.RightToLeft) && (vectorDirection == VectorDirection.Horizontal))
            {
                return true;
            }
            if ((vectorType == VectorType.TopToBottom || vectorType == VectorType.BottomToTop) && (vectorDirection == VectorDirection.Virtical))
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
            else if(vector.X < 0 && vector.Y < 0)
            {
                type = Math.Abs(vector.X) > Math.Abs(vector.Y) ? VectorType.RightToLeft : VectorType.BottomToTop;
            }
            else
            {
                type = VectorType.None;
            }

            return type;
        }

        public static IEnumerable<VectorPos> Viewpoint(NodePoint point, double step, NodeRect limitRect, IEnumerable<NodeRect> rects, VectorType priorityVector = VectorType.None)
        {
            bool noLimmit = limitRect.Width == 0 || limitRect.Height == 0;
            var rectContains = rects.Count() > 0;
            List<VectorPos> ret = new List<VectorPos>();
            if ((priorityVector == VectorType.None || priorityVector == VectorType.LeftToRight)
                && (noLimmit || point.X + step <= limitRect.BottomRight.X))
            {
                var pos = new NodePoint(point.X + step, point.Y);
                if (!rectContains || !rects.Any(_ => _.Contains(pos)))
                {
                    ret.Add(new VectorPos(VectorType.LeftToRight, pos));
                }
            }
            if ((priorityVector == VectorType.None || priorityVector == VectorType.TopToBottom)
                && (noLimmit || point.Y + step <= limitRect.BottomRight.Y))
            {
                var pos = new NodePoint(point.X, point.Y + step);
                if (!rectContains || !rects.Any(_ => _.Contains(pos)))
                {
                    ret.Add(new VectorPos(VectorType.TopToBottom, pos));
                }
            }
            if ((priorityVector == VectorType.None || priorityVector == VectorType.RightToLeft)
                && (noLimmit || point.X - step >= limitRect.TopLeft.X))
            {
                var pos = new NodePoint(point.X - step, point.Y);
                if (!rectContains || !rects.Any(_ => _.Contains(pos)))
                {
                    ret.Add(new VectorPos(VectorType.RightToLeft, pos));
                }
            }
            if ((priorityVector == VectorType.None || priorityVector == VectorType.BottomToTop)
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
