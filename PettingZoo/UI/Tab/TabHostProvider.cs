using System;

namespace PettingZoo.UI.Tab
{
    public class TabHostProvider : ITabHostProvider
    {
        private ITabHost? instance;

        public ITabHost Instance => instance ?? throw new InvalidOperationException("ITabHost instance must be initialized before acquiring");


        // ReSharper disable once ParameterHidesMember
        public void SetInstance(ITabHost instance)
        {
            this.instance = instance;
        }
    }
}
