using System;
using System.Collections.Generic;

namespace PettingZoo.Core.Generator
{
    public interface IExampleSource : IDisposable
    {
        IExampleFolder GetRootFolder();
    }


    public interface IExampleFolder
    {
        public string Name { get; }

        public IReadOnlyList<IExampleFolder> Folders { get; }
        public IReadOnlyList<IExampleMessage> Messages { get; }
    }


    public interface IExampleMessage
    {
        string Generate();
    }
}
