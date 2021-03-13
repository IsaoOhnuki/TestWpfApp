using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace ObjectAreaLibrary
{
    using NodePoint = Point;
    using NodeRect = Rect;
    using VectorPos = Tuple<VectorType, Point>;
    using ViewpointFunc = Func<Point, double, Rect, IEnumerable<Rect>, VectorType?, IEnumerable<Tuple<VectorType, Point>>>;
    using HeuristicFunc = Func<Point, Point, double>;

    public enum AStarStates
    {
        Ready,
        Run,
        Cancel,
        Goal,
    }

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

    public class AStar
    {
        private static AStar _instance;
        public static AStar Instance { get => _instance ??= new AStar(); }

        public volatile AStarStates States;

        public bool IsCanceled { get => States == AStarStates.Cancel; }
        public bool IsRunning { get => States == AStarStates.Run; }

        private readonly object _syncObject = new object();
        public void Cancel()
        {
            lock (_syncObject)
            {
                if (States == AStarStates.Run)
                {
                    States = AStarStates.Cancel;
                }
            }
        }

        private bool Run(Func<bool> func)
        {
            lock (_syncObject)
            {
                if (States == AStarStates.Ready)
                {
                    States = AStarStates.Run;
                }
            }
            var result = false;
            if (States == AStarStates.Run)
            {
                result = func();
            }
            lock (_syncObject)
            {
                if (States == AStarStates.Run)
                {
                    States = AStarStates.Goal;
                }
            }
            return result;
        }

        public IEnumerable<NodePoint> Exec(VectorPos start, VectorPos end, double step, double inertia,
            NodeRect limitRect, IEnumerable<NodeRect> obstacles, ViewpointFunc viewpointFunc, HeuristicFunc heuristicFunc)
        {
            VectorType startVector = start.Item1;
            var startPos = start.Item2;
            var endPos = end.Item2;
            var astarBounds = new NodeRect(startPos, endPos);
            if (limitRect.IsEmptyRect())
            {
                limitRect = GetUnionRectOrDefaultRect(obstacles, astarBounds);
                limitRect.Union(astarBounds);
                limitRect.Inflate(step, step);
            }

            AStarNodeCollection aStarCollection = new AStarNodeCollection();
            aStarCollection.Clear();
            SetGoal(endPos, step);

            var firstNode = AStarNodeCollection.Create(new VectorPos(startVector, startPos), heuristicFunc(startPos, endPos), GetBackward(startPos, endPos));
            aStarCollection.Add(firstNode);

            if (Run(() => ExecAStar(aStarCollection, firstNode, endPos, step, inertia, limitRect, obstacles, viewpointFunc, heuristicFunc)))
            {
                return aStarCollection.AdoptList();
            }

            return Enumerable.Empty<NodePoint>();
        }

        public async Task<IEnumerable<NodePoint>> ExecAsynk(VectorPos start, VectorPos end, double step, double inertia,
            NodeRect limitRect, IEnumerable<NodeRect> obstacles, ViewpointFunc viewpointFunc, HeuristicFunc heuristicFunc)
        {
            VectorType startVector = start.Item1;
            var startPos = start.Item2;
            var endPos = end.Item2;
            var astarBounds = new NodeRect(startPos, endPos);
            if (limitRect.IsEmptyRect())
            {
                limitRect = GetUnionRectOrDefaultRect(obstacles, astarBounds);
                limitRect.Union(astarBounds);
                limitRect.Inflate(step, step);
            }

            AStarNodeCollection astarCollection = new AStarNodeCollection();
            astarCollection.Clear();
            SetGoal(endPos, step);

            var firstNode = AStarNodeCollection.Create(new VectorPos(startVector, startPos), heuristicFunc(startPos, endPos), GetBackward(startPos, endPos));
            astarCollection.Add(firstNode);

            if (await Task.Run(() => Run(() => ExecAStar(astarCollection, firstNode, endPos, step, inertia, limitRect, obstacles, viewpointFunc, heuristicFunc))))
            {
                return astarCollection.AdoptList();
            }

            return Enumerable.Empty<NodePoint>();
        }

        private bool ExecAStar(AStarNodeCollection astarCollection, AStarNode node, NodePoint endPos, double step, double inertia,
            NodeRect limitRect, IEnumerable<NodeRect> obstacles, ViewpointFunc viewpointFunc, HeuristicFunc heuristicFunc)
        {
            if (obstacles != null && obstacles.Any(_ => _.Contains(node.NodePoint.Item2) || _.Contains(endPos)))
            {
                return false;
            }

            var sortedNodes = new SortedList<AStarKey, AStarNode>
            {
                { new AStarKey() { Cost = node.Cost, Backward = node.Backward, NodePoint = node.NodePoint.Item2 }, node }
            };
            var nodes = astarCollection.Sort();
            var result = false;
            while (node != null && !result)
            {
                node.Inspected = true;
                if (CheckGoal(astarCollection, node, endPos) || IsCanceled)
                {
                    while (node != null)
                    {
                        node.Adopt = true;
                        node = node.Parent;
                    }
                    result = true;
                    break;
                }

                var nodeVector = node.Vector;
                var nodePoint = node.Point;

                var parentNode = node.Parent;
                if (parentNode != null)
                {
                    var parentNodeVector = parentNode.NodePoint.Item1;
                    if (!nodeVector.EqualVectorDirection(parentNodeVector)
                        && GetLastVectorDirection(parentNode, nodeVector.VectorDirection(), out AStarNode lastNode)
                        && lastNode.NodePoint.Item1 != nodeVector)
                    {
                        var nd = node.Parent;
                        if (ShortCut(astarCollection, lastNode.Parent, node, endPos, parentNodeVector, step, inertia, limitRect, obstacles, viewpointFunc, heuristicFunc))
                        {
                            while (nd != lastNode)
                            {
                                nd.Forward += inertia;
                                nd = nd.Parent;
                            }
                            nodeVector = node.Vector;
                            nodePoint = node.Point;
                        }
                    }
                }

                var newViewPoints = viewpointFunc(nodePoint, step, limitRect, obstacles, null);
                foreach (var newViewPoint in newViewPoints)
                {
                    var newNodeVector = newViewPoint.Item1;
                    var newNodePos = newViewPoint.Item2;
                    var newNode = astarCollection.NodeAt(newNodePos);
                    if (newNode == null)
                    {
                        var hVal = heuristicFunc(newNodePos, endPos);
                        hVal -= nodeVector == newNodeVector ? inertia : 0;
                        newNode = AStarNodeCollection.Create(newViewPoint, hVal, GetBackward(newNodePos, endPos), parent: node);
                        astarCollection.Add(newNode);
                    }
                }

                if (IsCanceled)
                {
                    break;
                }

                nodes = astarCollection.Sort();
                node = nodes.FirstOrDefault();
            }

            return result;
        }

        private bool ShortCut(AStarNodeCollection aStarCollection, AStarNode nodeL, AStarNode nodeR, NodePoint goalPos, VectorType priorityVector, double step, double inertia,
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
                var newNode = aStarCollection.NodeAt(viewPos);
                if (newNode == null)
                {
                    newNode = AStarNodeCollection.Create(pos, heuristic(viewPos, goalPos) - inertia, GetBackward(viewPos, goalPos), parent: node);
                    aStarCollection.Add(newNode);
                }
                newNode.NodePoint = pos;
                newNode.Inspected = true;
                newNode.Parent = node;
                node = newNode;
            }

            return result;
        }

        private NodeRect _goalPoint;
        private void SetGoal(NodePoint endPos, double inflate)
        {
            _goalPoint = new NodeRect(endPos, new Size(1, 1));
            _goalPoint.Inflate(inflate, inflate);
        }

        private bool CheckGoal(AStarNodeCollection aStarCollection, AStarNode node, NodePoint endPos)
        {
            var nodePos = node.NodePoint.Item2;
            var result = _goalPoint.Contains(nodePos);
            if (result)
            {
                var vctType = node.NodePoint.Item1;
                switch (vctType)
                {
                    case VectorType.LeftToRight:
                        aStarCollection.Add(AStarNodeCollection.Create(new VectorPos(vctType, new NodePoint(endPos.X, nodePos.Y)), 0, 0, adopt: true));
                        break;
                    case VectorType.TopToBottom:
                        aStarCollection.Add(AStarNodeCollection.Create(new VectorPos(vctType, new NodePoint(nodePos.X, endPos.Y)), 0, 0, adopt: true));
                        break;
                    case VectorType.RightToLeft:
                        aStarCollection.Add(AStarNodeCollection.Create(new VectorPos(vctType, new NodePoint(endPos.X, nodePos.Y)), 0, 0, adopt: true));
                        break;
                    case VectorType.BottomToTop:
                        aStarCollection.Add(AStarNodeCollection.Create(new VectorPos(vctType, new NodePoint(nodePos.X, endPos.Y)), 0, 0, adopt: true));
                        break;
                }
                var goalNode = AStarNodeCollection.Create(new VectorPos(vctType, endPos), 0, 0, adopt: true);
                goalNode.Goal = true;
                aStarCollection.Add(goalNode);
            }
            return result;
        }

        private bool GetLastVectorDirection(AStarNode node, VectorDirection vectorDirection, out AStarNode result)
        {
            result = node;
            for (int idx = 0; result != null; ++idx)
            {
                var nodeVector = result.NodePoint.Item1;
                if (nodeVector.EqualVectorDirection(vectorDirection))
                {
                    break;
                }
                result = result.Parent;
            }
            return result != null;
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
            bool noLimmit = limitRect.IsEmptyRect();
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
