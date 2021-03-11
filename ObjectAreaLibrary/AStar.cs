﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace ObjectAreaLibrary
{
    using NodePoint = Point;
    using NodeRect = Rect;
    using VectorPos = Tuple<VectorType, Point>;
    using ViewpointFunc = Func<Point, double, Rect, IEnumerable<Rect>, VectorType?, IEnumerable<Tuple<VectorType, Point>>>;
    using HeuristicFunc = Func<Point, Point, double>;

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
        public bool Goal;
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
            return NodePoint.Item1 switch
            {
                VectorType.LeftToRight => "→",
                VectorType.TopToBottom => "↓",
                VectorType.RightToLeft => "←",
                VectorType.BottomToTop => "↑",
                _ => throw new ArgumentException(),
            };
        }
        public string ToString(ValueType type)
        {
            return type switch
            {
                ValueType.Adopt => AdoptString(),
                ValueType.Forward => ForwardString(),
                ValueType.Vector => VectorString(),
                ValueType.Cost => CostString(),
                _ => ToString(),
            };
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

        private IEnumerable<NodePoint> AdoptList()
        {
            return NodeCollection
                .Where(_ => _.Value.Adopt && (_.Value.Goal || _.Value.Parent == null || _.Value.Parent.NodePoint.Item1 != _.Value.NodePoint.Item1))
                .OrderBy(_ => _.Value.Index)
                .Select(_ => _.Value.Goal || _.Value.Parent == null ? _.Value.NodePoint.Item2 : _.Value.Parent.NodePoint.Item2);
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

        public bool IsCanceled { get; private set; }
        public bool IsRunning { get; private set; }

        private readonly string _cancelMutex = "CancelLock";
        public void Cancel()
        {
            using (var mutex = new Mutex(false, _cancelMutex + nameof(AStar) + GetHashCode().ToString()))
            {
                mutex.WaitOne();
                try
                {
                    IsCanceled = true;
                }
                finally
                {
                    mutex.ReleaseMutex();
                }
            }
        }

        private readonly string _runningMutex = "RunningLock";
        private bool Run(Func<bool> func)
        {
            using (var mutexRun = new Mutex(false, _runningMutex + nameof(AStar) + GetHashCode().ToString()))
            {
                mutexRun.WaitOne();
                try
                {
                    using (var mutexCancel = new Mutex(false, _cancelMutex + nameof(AStar) + GetHashCode().ToString()))
                    {
                        mutexCancel.WaitOne();
                        try
                        {
                            IsCanceled = false;
                            IsRunning = true;
                        }
                        finally
                        {
                            mutexCancel.ReleaseMutex();
                        }
                    }
                    var result = func();
                    IsRunning = false;
                    return result;
                }
                finally
                {
                    mutexRun.ReleaseMutex();
                }
            }
        }

        public IEnumerable<NodePoint> Exec(VectorPos start, VectorPos end, double step, double inertia,
            NodeRect limitRect, IEnumerable<NodeRect> obstacles, ViewpointFunc viewpointFunc, HeuristicFunc heuristicFunc)
        {
            VectorType startVector = start.Item1;
            var startPos = start.Item2;
            var endPos = end.Item2;
            var astarBounds = new NodeRect(startPos, endPos);
            if (limitRect == null)
            {
                limitRect = GetUnionRectOrDefaultRect(obstacles, astarBounds);
                limitRect.Inflate(step, step);
            }

            ClearNodes();
            SetGoal(endPos, step);

            var firstNode = CreatAStarNode(new VectorPos(startVector, startPos), heuristicFunc(startPos, endPos), GetBackward(startPos, endPos));
            AddNodes(firstNode);

            if (!Run(() => ExecAStar(firstNode, endPos, step, inertia, limitRect, obstacles, viewpointFunc, heuristicFunc)))
            {
                return Enumerable.Empty<NodePoint>();
            }

            return AdoptList();
        }

        public async Task<IEnumerable<NodePoint>> ExecAsynk(VectorPos start, VectorPos end, double step, double inertia,
            NodeRect limitRect, IEnumerable<NodeRect> obstacles, ViewpointFunc viewpointFunc, HeuristicFunc heuristicFunc)
        {
            VectorType startVector = start.Item1;
            var startPos = start.Item2;
            var endPos = end.Item2;
            var astarBounds = new NodeRect(startPos, endPos);
            if (limitRect == null)
            {
                limitRect = GetUnionRectOrDefaultRect(obstacles, astarBounds);
                limitRect.Inflate(step, step);
            }

            ClearNodes();
            SetGoal(endPos, step);

            var firstNode = CreatAStarNode(new VectorPos(startVector, startPos), heuristicFunc(startPos, endPos), GetBackward(startPos, endPos));
            AddNodes(firstNode);

            var result = await Task.Run(() => Run(() => ExecAStar(firstNode, endPos, step, inertia, limitRect, obstacles, viewpointFunc, heuristicFunc)));
            if (!result)
            {
                return Enumerable.Empty<NodePoint>();
            }

            return AdoptList();
        }

        private NodeRect GetUnionRectOrDefaultRect(IEnumerable<NodeRect> rects, NodeRect defaultRect)
        {
            var rect = defaultRect;
            if (rects != null)
            {
                if (rects.Count() > 0)
                {
                    rect = rects.First();
                    foreach (var r in rects)
                    {
                        rect.Union(r);
                    }
                }
            }
            return rect;
        }

        private bool ExecAStar(AStarNode node, NodePoint endPos, double step, double inertia,
            NodeRect limitRect, IEnumerable<NodeRect> obstacles, ViewpointFunc viewpointFunc, HeuristicFunc heuristicFunc)
        {
            if (obstacles != null && obstacles.Any(_ => _.Contains(node.NodePoint.Item2) || _.Contains(endPos)))
            {
                return false;
            }

            var nodes = NodeCollection
                .Where(_ => !_.Value.Inspected)
                .Select(_ => _.Value)
                .OrderBy(_ => _.Cost)
                .ThenByDescending(_ => _.Backward);
            var result = false;
            while (node != null && !result)
            {
                node.Inspected = true;
                if (CheckGoal(node, endPos) || IsCanceled)
                {
                    while (node != null)
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
                    if (!EqualVectorDirection(nodeVector, parentNodeVector)
                        && GetLastVectorDirection(parentNode, GetVectorDirection(nodeVector), out AStarNode lastNode)
                        && lastNode.NodePoint.Item1 != nodeVector)
                    {
                        var nd = node.Parent;
                        if (ShortCut(lastNode.Parent, node, endPos, parentNodeVector, step, inertia, limitRect, obstacles, viewpointFunc, heuristicFunc))
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

                var newViewPoints = viewpointFunc(nodePoint, step, limitRect, obstacles, null);
                var bestNode = node;
                foreach (var newViewPoint in newViewPoints)
                {
                    var newNodeVector = newViewPoint.Item1;
                    var newNodePos = newViewPoint.Item2;
                    var newNode = NodeAt(newNodePos);
                    if (newNode == null)
                    {
                        var hVal = heuristicFunc(newNodePos, endPos);
                        hVal -= nodeVector == newNodeVector ? inertia : 0;
                        newNode = CreatAStarNode(newViewPoint, hVal, GetBackward(newNodePos, endPos), parent: node);
                        AddNodes(newNode);
                    }
                    if (!newNode.Inspected && newNode.Cost < bestNode.Cost && newNode.Backward > bestNode.Backward)
                    {
                        bestNode = newNode;
                    }
                }

                if (IsCanceled)
                {
                    break;
                }

                if (!bestNode.Inspected)
                {
                    node = bestNode;
                }
                else
                {
                    nodes = NodeCollection
                        .Where(_ => !_.Value.Inspected)
                        .Select(_ => _.Value)
                        .OrderBy(_ => _.Cost)
                        .ThenByDescending(_ => _.Backward);
                    node = nodes.FirstOrDefault();
                }
            }

            return result;
        }

        private bool ShortCut(AStarNode nodeL, AStarNode nodeR, NodePoint goalPos, VectorType priorityVector, double step, double inertia,
            NodeRect limitRect, IEnumerable<NodeRect> obstacles, ViewpointFunc viewpoint, HeuristicFunc heuristic)
        {
            bool result = true;
            var node = nodeL;
            var endPos = nodeR.NodePoint.Item2;
            while (node != null)
            {
                if (IsCanceled)
                {
                    result = false;
                    break;
                }

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

        private bool GetLastVectorDirection(AStarNode node, VectorDirection vectorDirection, out AStarNode result)
        {
            result = node;
            for (int idx = 0; result != null; ++idx)
            {
                var nodeVector = result.NodePoint.Item1;
                if (EqualVectorDirection(nodeVector, vectorDirection))
                {
                    break;
                }
                result = result.Parent;
            }
            return result != null;
        }

        private VectorDirection GetVectorDirection(VectorType vectorType)
        {
            return vectorType switch
            {
                VectorType.LeftToRight => VectorDirection.Horizontal,
                VectorType.TopToBottom => VectorDirection.Virtical,
                VectorType.RightToLeft => VectorDirection.Horizontal,
                VectorType.BottomToTop => VectorDirection.Virtical,
                _ => throw new ArgumentException(),
            };
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

        private NodeRect GoalPoint;
        private void SetGoal(NodePoint endPos, double inflate)
        {
            GoalPoint = new NodeRect(endPos, new Size(1, 1));
            GoalPoint.Inflate(inflate, inflate);
        }

        private bool CheckGoal(AStarNode node, NodePoint endPos)
        {
            var nodePos = node.NodePoint.Item2;
            var result = GoalPoint.Contains(nodePos);
            if (result)
            {
                var vctType = node.NodePoint.Item1;
                switch (vctType)
                {
                    case VectorType.LeftToRight:
                        AddNodes(CreatAStarNode(new VectorPos(vctType, new NodePoint(endPos.X, nodePos.Y)), 0, 0, adopt: true));
                        break;
                    case VectorType.TopToBottom:
                        AddNodes(CreatAStarNode(new VectorPos(vctType, new NodePoint(nodePos.X, endPos.Y)), 0, 0, adopt: true));
                        break;
                    case VectorType.RightToLeft:
                        AddNodes(CreatAStarNode(new VectorPos(vctType, new NodePoint(endPos.X, nodePos.Y)), 0, 0, adopt: true));
                        break;
                    case VectorType.BottomToTop:
                        AddNodes(CreatAStarNode(new VectorPos(vctType, new NodePoint(nodePos.X, endPos.Y)), 0, 0, adopt: true));
                        break;
                }
                var goalNode = CreatAStarNode(new VectorPos(vctType, endPos), 0, 0, adopt: true);
                goalNode.Goal = true;
                AddNodes(goalNode);
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

        public static IEnumerable<VectorPos> Viewpoint(NodePoint point, double step, NodeRect limitRect, IEnumerable<NodeRect> obstacles, VectorType? priorityVector = null)
        {
            bool noLimmit = limitRect.Width == 0 || limitRect.Height == 0;
            var rectContains = obstacles != null && obstacles.Count() > 0;
            List<VectorPos> ret = new List<VectorPos>();
            if ((!priorityVector.HasValue || priorityVector == VectorType.LeftToRight)
                && (noLimmit || point.X + step <= limitRect.BottomRight.X))
            {
                var pos = new NodePoint(point.X + step, point.Y);
                if (!rectContains || !obstacles.Any(_ => _.Contains(pos)))
                {
                    ret.Add(new VectorPos(VectorType.LeftToRight, pos));
                }
            }
            if ((!priorityVector.HasValue || priorityVector == VectorType.TopToBottom)
                && (noLimmit || point.Y + step <= limitRect.BottomRight.Y))
            {
                var pos = new NodePoint(point.X, point.Y + step);
                if (!rectContains || !obstacles.Any(_ => _.Contains(pos)))
                {
                    ret.Add(new VectorPos(VectorType.TopToBottom, pos));
                }
            }
            if ((!priorityVector.HasValue || priorityVector == VectorType.RightToLeft)
                && (noLimmit || point.X - step >= limitRect.TopLeft.X))
            {
                var pos = new NodePoint(point.X - step, point.Y);
                if (!rectContains || !obstacles.Any(_ => _.Contains(pos)))
                {
                    ret.Add(new VectorPos(VectorType.RightToLeft, pos));
                }
            }
            if ((!priorityVector.HasValue || priorityVector == VectorType.BottomToTop)
                && (noLimmit || point.Y - step >= limitRect.TopLeft.Y))
            {
                var pos = new NodePoint(point.X, point.Y - step);
                if (!rectContains || !obstacles.Any(_ => _.Contains(pos)))
                {
                    ret.Add(new VectorPos(VectorType.BottomToTop, pos));
                }
            }

            return ret;
        }
    }
}
