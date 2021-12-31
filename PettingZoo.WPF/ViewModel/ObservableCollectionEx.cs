using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

// ReSharper disable UnusedMember.Global - public API

namespace PettingZoo.WPF.ViewModel
{
    public class ObservableCollectionEx<T> : ObservableCollection<T>
    {
        private int updateLockCount;
        private bool updatedInLock;


        public void AddRange(IEnumerable<T> items)
        {
            BeginUpdate();
            try
            {
                foreach (var item in items)
                    Add(item);
            }
            finally
            {
                EndUpdate();
            }
        }


        public void ReplaceAll(IEnumerable<T> newItems)
        {
            BeginUpdate();
            try
            {
                Clear();
                foreach (var item in newItems)
                    Add(item);
            }
            finally
            {
                EndUpdate();
            }
        }


        /// <summary>
        /// Disables the OnCollectionChanged notification. Should always be matched with an EndUpdate to resume notifications.
        /// </summary>
        public void BeginUpdate()
        {
            updateLockCount++;
        }


        public void EndUpdate()
        {
            if (updateLockCount == 0)
                throw new InvalidOperationException("EndUpdate does not have a matching BeginUpdate");

            updateLockCount--;
            if (updateLockCount == 0 && updatedInLock)
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }



        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (updateLockCount > 0)
            {
                updatedInLock = true;
                return;
            }

            base.OnCollectionChanged(e);
        }
    }
}
