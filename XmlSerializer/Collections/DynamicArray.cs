using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Baxendale.DataManagement.Collections
{
    public class DynamicArray<T> : IList<T>
    {
        private Vector<int> lengths;
        private Vector<T> elements;

        private int rank;

        public int Rank
        {
            get
            {
                return rank;
            }
            set
            {
                if (value < 1)
                    throw new ArgumentOutOfRangeException();
                if (value < rank)
                {
                    ShrinkRank(value);
                }
                else if (value > rank)
                {
                    ExpandRank(value);
                }
            }
        }

        public int[] Lengths
        {
            get
            {
                return lengths.Take(rank).ToArray();
            }
        }

        public int Size
        {
            get
            {
                return Measure(lengths, rank);
            }
        }

        private static int Measure(IEnumerable<int> lengths, int rank)
        {
            return lengths.Take(rank).Aggregate((a, b) => a * b);
        }

        private static unsafe int Measure(int* lpLens, int rank)
        {
            int factor = *lpLens;
            for (int dim = 1; dim < rank; ++dim)
                factor *= lpLens[dim];
            return factor;
        }

        public int[] UpperBound
        {
            get
            {
                int[] bound = new int[rank];
                for (int i = 0; i < rank; ++i)
                    bound[i] = lengths[i] - 1;
                return bound;
            }
        }

        public int[] LowerBound
        {
            get
            {
                return new int[rank];
            }
        }

        public DynamicArray()
        {
            elements = new T[0];
            lengths = new int[1];
            lengths[0] = 1;
            rank = lengths.Length;
        }

        public DynamicArray(params int[] lengths)
        {
            this.lengths = new Vector<int>(lengths.Length);
            for (int i = 0; i < lengths.Length; ++i)
            {
                if (i < 0) throw new ArgumentOutOfRangeException();
                this.lengths.Add(lengths[i]);
            }
            rank = lengths.Length;
            elements = new T[Size];
        }

        public Array ToArray()
        {
            Array array = Array.CreateInstance(typeof(T), Lengths);
            T[] elements = this.elements.InternalArray;
            Buffer.BlockCopy(elements, 0, array, 0, Math.Min(Buffer.ByteLength(elements), Buffer.ByteLength(array)));
            return array;
        }

        private int ToIndex(int[] indices)
        {
            return ToIndex(indices, rank, lengths);
        }

        private static unsafe int ToIndex(int[] indices, int rank, Vector<int> lengths)
        {
            if (indices == null)
                throw new NullReferenceException();
            if (rank != lengths.Length || rank != indices.Length)
                throw new RankException();
            fixed (int* lpIndices = indices, lpLens = lengths.InternalArray)
            {
                return ToIndex(lpIndices, rank, lpLens);
            }
        }

        private static unsafe int ToIndex(int* lpIndices, int rank, int* lpLens)
        {
            int arrayIndex = lpIndices[rank - 1];
            for (int dim = 0; dim < rank - 1; ++dim)
            {
                int factor = lpIndices[dim];
                for (int j = dim + 1; j < rank; ++j)
                    factor *= lpLens[j];
                arrayIndex += factor;
            }
            return arrayIndex;
        }

        private int[] ToIndices(int index)
        {
            return ToIndices(index, rank, lengths);
        }

        private static unsafe int[] ToIndices(int index, int rank, Vector<int> lengths)
        {
            if (lengths == null)
                throw new NullReferenceException();
            if (rank != lengths.Length)
                throw new RankException();
            int[] indices = new int[rank];
            fixed (int* lpIndices = indices, lpLens = lengths.InternalArray)
                ToIndices(index, rank, lpLens, lpIndices);
            return indices;
        }

        private static unsafe void ToIndices(int nIdx, int nRank, int* lpLens, int* lpOutIndices)
        {
            lpOutIndices[nRank - 1] = nIdx;
            for (int dim = nRank - 1; dim > 0; --dim)
            {
                lpOutIndices[dim - 1] = lpOutIndices[dim] / lpLens[dim];
                lpOutIndices[dim] = lpOutIndices[dim] % lpLens[dim];
            }
        }

        public unsafe int[] IncrementIndex(params int[] indices)
        {
            if (indices == null)
                throw new NullReferenceException();
            if (indices.Length != rank)
                throw new RankException();
            int[] newIndices = indices.BlockCopy();
            fixed (int* lpIndices = newIndices, lpLens = lengths.InternalArray)
                IncrementIndex(lpIndices, lpLens, rank);
            return newIndices;
        }

        private static unsafe void IncrementIndex(int* lpIndices, int* lpLens, int nRank)
        {
            ++lpIndices[nRank - 1];
            for (int dim = nRank - 1; dim > 0; --dim)
            {
                if (lpIndices[dim] >= lpLens[dim])
                {
                    for (int i = dim; i < nRank; ++i)
                        lpIndices[i] = 0;
                    ++lpIndices[dim - 1];
                }
            }
        }

        public unsafe int[] DecrementIndex(params int[] indices)
        {
            if (indices == null)
                throw new NullReferenceException();
            if (indices.Length != rank)
                throw new RankException();
            int[] newIndices = indices.BlockCopy();
            fixed (int* lpIndices = newIndices, lpLens = lengths.InternalArray)
                DecrementIndex(lpIndices, lpLens, rank);
            return newIndices;
        }

        private static unsafe void DecrementIndex(int* lpIndices, int* lpLens, int nRank)
        {
            --lpIndices[nRank - 1];
            for (int dim = nRank - 1; dim > 0; --dim)
            {
                if (lpIndices[dim] < 0)
                {
                    for (int i = dim; i < nRank; ++i)
                        lpIndices[i] = lpLens[i] - 1;
                    --lpIndices[dim - 1];
                }
            }
        }

        public int[] IndexOf(T item)
        {
            int idx = ((IList<T>)this).IndexOf(item);
            if (idx < 0)
            {
                if (Size == 0)
                    return new int[] { -1 };
                int[] indices = LowerBound;
                DecrementIndex(indices);
            }
            return ToIndices(idx);
        }

        public void TrimExcess()
        {
            int len = Size;
            if (len < elements.Length)
                elements.Length = len;            
            elements.TrimExcess();
            lengths.TrimExcess();
        }

        private void CheckIndices(int[] indices)
        {
            if (indices == null)
                throw new NullReferenceException();
            if (indices.Length != rank)
                throw new RankException();
            for (int dim = 0; dim < rank; ++dim)
            {
                if (indices[dim] < 0 || indices[dim] >= lengths[dim])
                    throw new ArgumentOutOfRangeException();
            }
        }

        private unsafe void ShrinkRank(int newRank)
        {
            int oldRank = rank;
            fixed (int* lpLens = lengths.InternalArray)
            {
                int oldLen = Measure(lpLens, oldRank);
                int newLen = Measure(lpLens, newRank);
                int* lpOldIndices = stackalloc int[oldRank];
                for (int index = 0; index < newLen; ++index)
                {
                    lpOldIndices[newRank - 1] = index;
                    for (int dim = newRank - 1; dim > 0; --dim)
                    {
                        lpOldIndices[dim - 1] = lpOldIndices[dim] / lpLens[dim];
                        lpOldIndices[dim] = lpOldIndices[dim] % lpLens[dim];
                    }
                    elements[index] = elements[ToIndex(lpOldIndices, oldRank, lpLens)];
                }
            }
            lengths.Length = newRank;
            rank = newRank;
        }

        private unsafe void ExpandRank(int[] indices)
        {
            int newRank = indices.Length;
            int oldRank = rank;
            Vector<int> newLengths = new Vector<int>(newRank)
            {
                Length = newRank
            };
            fixed (int* lpIndices = indices,
                        lpOldLens = lengths.InternalArray,
                        lpNewLens = newLengths.InternalArray)
            {
                for (int dim = 0; dim < newRank; ++dim)
                {
                    int len = lpIndices[dim] + 1;
                    if (dim < oldRank)
                        len = len > lpOldLens[dim] ? len : lpOldLens[dim];
                    lpNewLens[dim] = len;
                }
                ExpandRank(newRank, lpOldLens, lpNewLens);
            }
            lengths = newLengths;
        }

        public unsafe void ExpandRank(int newRank)
        {
            lengths.Fill(1, rank, newRank - rank + 1);
            fixed (int* lpOldLens = lengths.InternalArray)
                ExpandRank(newRank, lpOldLens, lpOldLens);
        }

        private unsafe void ExpandRank(int newRank, int* lpOldLens, int* lpNewLens)
        {
            int oldRank = rank;
            int oldSize = Measure(lpOldLens, oldRank);
            int newSize = Measure(lpNewLens, newRank);
            T[] newArr = new T[newSize];
            int* lpOldIndices = stackalloc int[newRank];
            for (int index = 0; index < oldSize; ++index)
            {
                lpOldIndices[oldRank - 1] = index;
                for (int dim = oldRank - 1; dim > 0; --dim)
                {
                    lpOldIndices[dim - 1] = lpOldIndices[dim] / lpOldLens[dim];
                    lpOldIndices[dim] = lpOldIndices[dim] % lpOldLens[dim];
                }
                newArr[ToIndex(lpOldIndices, newRank, lpNewLens)] = elements[index];
            }
            elements = newArr;
            rank = newRank;
        }

        private bool LargerThanLengths(int[] indices)
        {
            for (int dim = 0; dim < rank; ++dim)
            {
                if (indices[dim] < 0 || indices[dim] >= lengths[dim])
                    return true;
            }
            return false;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("DynamicArray<");
            sb.Append(typeof(T).Name);
            sb.Append(">[");
            sb.Append(lengths[0]);
            for (int i = 1; i < rank; ++i)
                sb.Append(",").Append(lengths[i]);
            sb.Append("]");
            return sb.ToString();
        }

        public unsafe string ToElementString()
        {
            StringBuilder sb = new StringBuilder();
            int* indices = stackalloc int[rank];
            fixed (int* lpLens = lengths.InternalArray)
            {
                for (int i = 0, len = Size; i < len; ++i)
                {
                    sb.Append("[").Append(indices[0]);
                    for (int dim = 1; dim < rank; ++dim)
                        sb.Append(",").Append(indices[dim]);
                    sb.Append("] = ");
                    sb.Append(elements[i]);
                    IncrementIndex(indices, lpLens, rank);
                }
            }
            return sb.ToString();
        }

        #region ICollection<T> Members

        void ICollection<T>.Add(T item)
        {
            int[] indices = UpperBound;
            IncrementIndex(indices);
            this[indices] = item;
        }

        void ICollection<T>.Clear()
        {
            elements.Clear();
            lengths.Clear();
        }

        public bool Contains(T item)
        {
            return ((IList<T>)this).IndexOf(item) > -1;
        }

        void ICollection<T>.CopyTo(T[] array, int arrayIndex)
        {
            elements.CopyTo(array, arrayIndex);
        }

        int ICollection<T>.Count
        {
            get { return Size; }
        }

        bool ICollection<T>.IsReadOnly
        {
            get { return true; }
        }

        bool ICollection<T>.Remove(T item)
        {
            throw new InvalidOperationException();
        }

        #endregion

        #region IEnumerable<T> Members

        public IEnumerator<T> GetEnumerator()
        {
            for (int i = 0, len = Size; i < len; ++i)
                yield return elements[i];
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #region IList<T> Members

        int IList<T>.IndexOf(T item)
        {
            elements.Length = Size;
            return elements.IndexOf(item);
        }

        void IList<T>.Insert(int index, T item)
        {
            throw new InvalidOperationException();
        }

        void IList<T>.RemoveAt(int index)
        {
            throw new InvalidOperationException();
        }

        T IList<T>.this[int index]
        {
            get
            {
                if (index < 0 || index >= Size)
                    throw new ArgumentOutOfRangeException();
                if (elements.Length < index)
                    return default(T);
                return elements[index];
            }
            set
            {
                if (index < 0 || index >= Size)
                    throw new ArgumentOutOfRangeException();
                elements[index] = value;
            }
        }

        public T this[params int[] indices]
        {
            get
            {
                CheckIndices(indices);
                int index = ToIndex(indices);
                if (elements.Length < index)
                    return default(T);
                return elements[index];
            }
            set
            {
                if (indices.Length < rank)
                    throw new RankException();
                if (indices.Length > rank || LargerThanLengths(indices))
                    ExpandRank(indices);
                elements[ToIndex(indices)] = value;
            }
        }

        #endregion
    }
}
