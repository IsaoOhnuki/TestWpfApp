using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Windows;

namespace ObjectAreaLibrary
{
    using NodePoint = Point;
    using NodeRect = Rect;
    using VectorPos = Tuple<VectorType, Point>;

    public static class NodeRectExtension
    {
        public static bool IsEmptyRect(this NodeRect rect)
        {
            return rect.Width == 0 || rect.Height == 0;
        }
    }

    public static class VectorTypeExtension
    {
        public static VectorDirection VectorDirection(this VectorType vectorType)
        {
            return vectorType switch
            {
                VectorType.LeftToRight => ObjectAreaLibrary.VectorDirection.Horizontal,
                VectorType.TopToBottom => ObjectAreaLibrary.VectorDirection.Virtical,
                VectorType.RightToLeft => ObjectAreaLibrary.VectorDirection.Horizontal,
                VectorType.BottomToTop => ObjectAreaLibrary.VectorDirection.Virtical,
                _ => throw new ArgumentException(),
            };
        }

        public static bool EqualVectorDirection(this VectorType vectorType, VectorDirection vectorDirection)
        {
            return vectorType.VectorDirection() == vectorDirection;
        }

        public static bool EqualVectorDirection(this VectorType l, VectorType r)
        {
            return l.VectorDirection() == r.VectorDirection();
        }
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
        public VectorType Vector { get => NodePoint.Item1; }
        public NodePoint Point { get => NodePoint.Item2; }

        #region ToString
        public enum ValueType
        {
            None,
            Adopt,
            Forward,
            Cost,
            Vector,
        }

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

    public struct AStarKey : IComparer<AStarKey>, IComparable<AStarKey>
    {
        public double Cost;
        public double Backward;
        public NodePoint NodePoint;

        #region Comparison
        public static bool operator <(AStarKey l, AStarKey r)
        {
            return l.Cost < r.Cost && l.Backward > r.Backward;
        }

        public static bool operator >(AStarKey l, AStarKey r)
        {
            return l.Cost > r.Cost || l.Backward < r.Backward;
        }

        public static bool operator ==(AStarKey l, AStarKey r)
        {
            return l.Cost == r.Cost && l.Backward == r.Backward;
        }

        public static bool operator !=(AStarKey l, AStarKey r)
        {
            return l.Cost != r.Cost || l.Backward != r.Backward;
        }

        public override bool Equals(object obj)
        {
            return this == (AStarKey)obj;
        }

        public override int GetHashCode()
        {
            return NodePoint.GetHashCode();
        }

        public int Compare([AllowNull] AStarKey x, [AllowNull] AStarKey y)
        {
            if (x < y)
                return -1;
            else
                return 1;
        }

        public int CompareTo([AllowNull] AStarKey other)
        {
            return Compare(this, other);
        }
        #endregion
    }

    public class AStarNodeCollection : ICollection<AStarNode>
    {
        private int _astarNodeIndex;

        private Dictionary<NodePoint, AStarNode> Collection { get; } = new Dictionary<NodePoint, AStarNode>();

        public int Count { get => Collection.Count; }

        public static AStarNode Create(VectorPos vectorPos, double forward, double backward, AStarNode parent = null, bool adopt = false)
        {
            return new AStarNode()
            {
                NodePoint = vectorPos,
                Forward = forward,
                Backward = backward,
                Cost = forward + backward,
                Adopt = adopt,
                Parent = parent,
            };
        }

        public void Clear()
        {
            _astarNodeIndex = 0;
            Collection.Clear();
        }

        public void Add(AStarNode item)
        {
            if (!Collection.ContainsKey(item.NodePoint.Item2))
            {
                item.Index = ++_astarNodeIndex;
                Collection.Add(item.NodePoint.Item2, item);
            }
        }

        public bool Remove(AStarNode item)
        {
            return Collection.Remove(item.NodePoint.Item2);
        }

        public bool Contains(AStarNode item)
        {
            return Collection.ContainsKey(item.NodePoint.Item2);
        }

        public AStarNode NodeAt(NodePoint point)
        {
            return Collection.ContainsKey(point) ? Collection[point] : null;
        }

        public IEnumerable<AStarNode> Sort()
        {
            return Collection
                .Where(_ => !_.Value.Inspected)
                .Select(_ => _.Value)
                .OrderBy(_ => _.Cost)
                .ThenByDescending(_ => _.Backward);
        }

        public IEnumerable<NodePoint> AdoptList()
        {
            return Collection
                .Where(_ => _.Value.Adopt && (_.Value.Goal || _.Value.Parent == null || _.Value.Parent.NodePoint.Item1 != _.Value.NodePoint.Item1))
                .OrderBy(_ => _.Value.Index)
                .Select(_ => _.Value.Goal || _.Value.Parent == null ? _.Value.NodePoint.Item2 : _.Value.Parent.NodePoint.Item2);
        }

        public string GetCsv(int step, AStarNode.ValueType csvType)
        {
            var list = Collection
                .OrderBy(_ => _.Value.NodePoint.Item2.Y)
                .ThenBy(_ => _.Value.NodePoint.Item2.X)
                .Select(_ => new Tuple<System.Drawing.Point, AStarNode>(new System.Drawing.Point((int)_.Value.NodePoint.Item2.X, (int)_.Value.NodePoint.Item2.Y), _.Value));
            var minX = Collection.Min(_ => (int)_.Key.X);
            var minY = Collection.Min(_ => (int)_.Key.Y);
            var maxX = Collection.Max(_ => (int)_.Key.X);
            var maxY = Collection.Max(_ => (int)_.Key.Y);

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

        private VectorDirection OppositeVectorDirection(VectorType vectorType)
        {
            return OppositeVectorDirection(vectorType.VectorDirection());
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

        public bool IsReadOnly => throw new NotImplementedException();

        public void CopyTo(AStarNode[] array, int arrayIndex) => throw new NotImplementedException();

        public IEnumerator<AStarNode> GetEnumerator() => throw new NotImplementedException();

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => throw new NotImplementedException();
    }
}
