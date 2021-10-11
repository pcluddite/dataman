//
//    DataMan - Supplemental library for managing data types and handling serialization
//    Copyright (C) 2021 Timothy Baxendale
//
//    This library is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Lesser General Public
//    License as published by the Free Software Foundation; either
//    version 2.1 of the License, or (at your option) any later version.
//
//    This library is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
//    Lesser General Public License for more details.
//
//    You should have received a copy of the GNU Lesser General Public
//    License along with this library; if not, write to the Free Software
//    Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301
//    USA
//
using System.Collections;
using System;
using System.Collections.Generic;

namespace Baxendale.Data.Collections
{
    public struct TreeNode<T> : ITreeNode<T>, ICollection<TreeNode<T>>, IEquatable<TreeNode<T>>, IEnumerable<TreeNode<T>>
    {
        private Dictionary<T, TreeNode<T>> _nodes;
        private IEqualityComparer<T> _comparer;

        public ITreeNode<T> Parent { get; }

        public int Count => Nodes.Count;

        public T Value { get; }

        public TreeNode<T> this[T value] => NodeDictionary[value];

        public ICollection<TreeNode<T>> Nodes => NodeDictionary.Values;

        private Dictionary<T, TreeNode<T>> NodeDictionary
        {
            get
            {
                if (_nodes == null)
                    _nodes = new Dictionary<T, TreeNode<T>>();
                return _nodes;
            }
        }

        public IEqualityComparer<T> EqualityComparer
        {
            get
            {
                if (_comparer == null)
                    _comparer = EqualityComparer<T>.Default;
                return _comparer;
            }
        }

        public TreeNode(T value)
            : this(null, value)
        {
        }

        public TreeNode(T value, IEqualityComparer<T> comparer)
           : this(null, value, comparer)
        {
        }

        public TreeNode(T value, int capacity)
          : this(null, value, capacity)
        {
        }

        public TreeNode(T value, int capacity, IEqualityComparer<T> comparer)
           : this(null, value, capacity, comparer)
        {
        }

        private TreeNode(ITreeNode<T> parent, T value)
            : this(parent, value, EqualityComparer<T>.Default)
        {
        }

        private TreeNode(ITreeNode<T> parent, T value, IEqualityComparer<T> comparer)
        {
            Parent = parent;
            Value = value;
            _comparer = comparer;
            _nodes = new Dictionary<T, TreeNode<T>>();
        }

        private TreeNode(ITreeNode<T> parent, T value, int capacity)
            : this(parent, value, capacity, EqualityComparer<T>.Default)
        {
        }

        private TreeNode(ITreeNode<T> parent, T value, int capacity, IEqualityComparer<T> comparer)
        {
            Parent = parent;
            Value = value;
            _comparer = comparer;
            _nodes = new Dictionary<T, TreeNode<T>>(capacity);
        }

        public TreeNode<T> Add(T item)
        {
            TreeNode<T> node = new TreeNode<T>(this, item, _comparer);
            NodeDictionary.Add(node.Value, node);
            return node;
        }

        public void Clear()
        {
            foreach (TreeNode<T> node in Nodes)
                node.Clear();
            NodeDictionary.Clear();
        }

        public bool Contains(T item)
        {
            return NodeDictionary.ContainsKey(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            NodeDictionary.Keys.CopyTo(array, arrayIndex);
        }

        public void CopyTo(TreeNode<T>[] array, int arrayIndex)
        {
            NodeDictionary.Values.CopyTo(array, arrayIndex);
        }

        public bool Remove(T item)
        {
            return NodeDictionary.Remove(item);
        }

        #region ICollection<TreeNode<T>>

        bool ICollection<TreeNode<T>>.IsReadOnly => false;

        void ICollection<TreeNode<T>>.Add(TreeNode<T> item)
        {
            throw new NotSupportedException();
        }

        bool ICollection<TreeNode<T>>.Contains(TreeNode<T> item)
        {
            return Contains(item.Value);
        }

        bool ICollection<TreeNode<T>>.Remove(TreeNode<T> item)
        {
            throw new NotSupportedException();
        }

        #endregion

        #region ICollection<ITreeNode<T>>

        ITreeNode<T> ITreeNode<T>.this[T value] => this[value];

        bool ICollection<ITreeNode<T>>.IsReadOnly => true;

        void ICollection<ITreeNode<T>>.Add(ITreeNode<T> item)
        {
            throw new NotSupportedException();
        }

        bool ICollection<ITreeNode<T>>.Contains(ITreeNode<T> item)
        {
            if (item == null)
                return Contains(default(T));
            return Contains(item.Value);
        }

        void ICollection<ITreeNode<T>>.CopyTo(ITreeNode<T>[] array, int arrayIndex)
        {
            if (array == null) throw new NullReferenceException(nameof(array));
            if (arrayIndex >= (uint)array.Length) throw new ArgumentOutOfRangeException(nameof(arrayIndex));
            foreach (TreeNode<T> node in Nodes)
            {
                array[arrayIndex++] = node;
            }
        }

        bool ICollection<ITreeNode<T>>.Remove(ITreeNode<T> item)
        {
            throw new NotSupportedException();
        }

        #endregion

        #region IEnumerable

        IEnumerator<ITreeNode<T>> IEnumerable<ITreeNode<T>>.GetEnumerator()
        {
            foreach (TreeNode<T> node in Nodes)
                yield return node;
        }

        IEnumerator<TreeNode<T>> IEnumerable<TreeNode<T>>.GetEnumerator()
        {
            foreach (TreeNode<T> node in Nodes)
                yield return node;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<TreeNode<T>>)this).GetEnumerator();
        }

        #endregion

        public override int GetHashCode()
        {
            return EqualityComparer.GetHashCode(Value);
        }

        public override bool Equals(object obj)
        {
            TreeNode<T>? node = obj as TreeNode<T>?;
            if (node == null)
                return false;
            return Equals(node);
        }

        public bool Equals(ITreeNode<T> obj)
        {
            TreeNode<T>? node = obj as TreeNode<T>?;
            if (node == null)
                return false;
            return Equals(node);
        }

        public bool Equals(TreeNode<T> obj)
        {
            return EqualityComparer.Equals(this, obj);
        }

        public static implicit operator T(TreeNode<T> node)
        {
            return node.Value;
        }
    }
}
