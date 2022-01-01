using System;

namespace PettingZoo.Core.Generator
{
    public interface IExampleGenerator
    {
        void Select(object? ownerWindow, Action<IExample> onExampleSelected);
    }


    public interface IExample
    {
        string Generate();
    }


    public interface IClassTypeExample : IExample
    {
        public string AssemblyName { get; }
        public string? Namespace { get; }
        public string ClassName { get; }

        public string FullClassName => !string.IsNullOrEmpty(Namespace) ? Namespace + "." : "" + ClassName;
    }


    /*
    public interface IValidatingExample : IExample
    {
        bool Validate(string payload, out string validationMessage);
    }
    */
}
