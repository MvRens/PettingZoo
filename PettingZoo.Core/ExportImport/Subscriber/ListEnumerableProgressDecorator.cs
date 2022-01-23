using System;
using System.Collections;
using System.Collections.Generic;

namespace PettingZoo.Core.ExportImport.Subscriber
{
    public class ListEnumerableProgressDecorator<T> : BaseProgressDecorator, IEnumerable<T>
    {
        private readonly IReadOnlyList<T> decoratedList;
        private int position;


        /// <summary>
        /// Wraps an IReadOnlyList and provides reports to the IProgress when it is enumerated.
        /// </summary>
        /// <param name="decoratedList">The IReadOnlyList to decorate.</param>
        /// <param name="progress">Receives progress reports. The value will be a percentage (0 - 100).</param>
        /// <param name="reportInterval">The minimum time between reports.</param>
        public ListEnumerableProgressDecorator(IReadOnlyList<T> decoratedList, IProgress<int> progress, TimeSpan? reportInterval = null) 
            : base(progress, reportInterval)
        {
            this.decoratedList = decoratedList;
        }


        protected override int GetProgress()
        {
            return decoratedList.Count > 0
                ? (int)Math.Truncate((double)position / decoratedList.Count * 100)
                : 0;
        }


        protected void AfterNext()
        {
            position++;
            UpdateProgress();
        }


        protected void Reset()
        {
            position = 0;
            UpdateProgress();
        }


        public IEnumerator<T> GetEnumerator()
        {
            return new EnumerableWrapper(this, decoratedList.GetEnumerator());
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }


        private class EnumerableWrapper : IEnumerator<T>
        {
            private readonly ListEnumerableProgressDecorator<T> owner;
            private readonly IEnumerator<T> decoratedEnumerator;


            public EnumerableWrapper(ListEnumerableProgressDecorator<T> owner, IEnumerator<T> decoratedEnumerator)
            {
                this.owner = owner;
                this.decoratedEnumerator = decoratedEnumerator;
            }


            public bool MoveNext()
            {
                var result = decoratedEnumerator.MoveNext();
                if (result)
                    owner.AfterNext();
                
                return result;
            }


            public void Reset()
            {
                decoratedEnumerator.Reset();
                owner.Reset();
            }


            public T Current => decoratedEnumerator.Current;
            object IEnumerator.Current => decoratedEnumerator.Current!;


            public void Dispose()
            {
                GC.SuppressFinalize(this);
                decoratedEnumerator.Dispose();
            }
        }
    }
}
