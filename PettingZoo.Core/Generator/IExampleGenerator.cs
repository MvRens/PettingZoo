using System;
using PettingZoo.Core.Validation;

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
        string AssemblyName { get; }
        string? Namespace { get; }
        string ClassName { get; }

        string FullClassName => (!string.IsNullOrEmpty(Namespace) ? Namespace + "." : "") + ClassName;

        bool TryGetPublishDestination(out string exchange, out string routingKey);
    }


    public interface IValidatingExample : IExample, IPayloadValidator
    {
    }
}
