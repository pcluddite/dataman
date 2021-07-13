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
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace Baxendale.DataManagement.Collections.Concurrent
{
    public class LockingEnumerator<T> : IEnumerator<T>
    {
        private IEnumerator<T> _inner;

        public object SyncRoot { get; }

        public T Current
        {
            get
            {
                throw new NotImplementedException();
            }
        }
        
        public LockingEnumerator(IEnumerable<T> collection, object syncRoot)
            : this(collection.GetEnumerator(), syncRoot)
        {
        }

        public LockingEnumerator(IEnumerator<T> inner, object syncRoot)
        {
            if (inner == null) throw new ArgumentNullException(nameof(inner));
            if (syncRoot == null) throw new ArgumentNullException(nameof(syncRoot));
            _inner = inner;
            SyncRoot = syncRoot;
            Monitor.Enter(syncRoot);
        }

        object IEnumerator.Current
        {
            get
            {
                return Current;
            }
        }

        public void Dispose()
        {
            _inner.Dispose();
            Monitor.Exit(SyncRoot);
        }

        public bool MoveNext()
        {
            return _inner.MoveNext();
        }

        public void Reset()
        {
            _inner.Reset();
        }
    }
}
