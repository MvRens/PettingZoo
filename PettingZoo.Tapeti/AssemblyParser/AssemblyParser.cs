using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Runtime.Loader;
using Newtonsoft.Json;
using PettingZoo.Core.Generator;
using PettingZoo.Core.Validation;
using Tapeti.Default;

namespace PettingZoo.Tapeti.AssemblyParser
{
    public class AssemblyParser : IDisposable
    {
        private readonly string[] extraAssembliesPaths;
        private AssemblyLoadContext? loadContext;


        public AssemblyParser(params string[] extraAssembliesPaths)
        {
            this.extraAssembliesPaths = extraAssembliesPaths;
        }


        public void Dispose()
        {
            loadContext?.Unload();
            GC.SuppressFinalize(this);
        }


        public IEnumerable<IClassTypeExample> GetExamples(Stream assemblyStream)
        {
            if (loadContext == null)
            {
                /*
                    Using the MetadataLoadContext introduces extra complexity since types can not be compared
                    (a string from the loaded assembly does not equal our typeof(string) for example).
                    So instead we'll use a regular AssemblyLoadContext. Not ideal, and will probably cause other side-effects
                    if we're not careful, but I don't feel like writing a full metadata parser right now.
                    If you have a better idea, it's open-source! :-)
                */
                loadContext = new AssemblyLoadContext(null, true);

                foreach (var extraAssembly in extraAssembliesPaths.SelectMany(p => Directory.Exists(p)
                             ? Directory.GetFiles(p, "*.dll")
                             : Enumerable.Empty<string>()))
                {
                    loadContext.LoadFromAssemblyPath(extraAssembly);
                }
            }

            var assembly = loadContext.LoadFromStream(assemblyStream);

            foreach (var type in assembly.GetTypes().Where(t => t.IsClass))
                yield return new TypeExample(type);
        }



        private class TypeExample : IClassTypeExample, IValidatingExample
        {
            public string AssemblyName => type.Assembly.GetName().Name ?? "";
            public string? Namespace => type.Namespace;
            public string ClassName => type.Name;

            private readonly Type type;

            private bool validationInitialized;
            private bool validationAvailable;


            public TypeExample(Type type)
            {
                this.type = type;
            }


            public string Generate()
            {
                var serialized = TypeToJObjectConverter.Convert(type);
                return serialized.ToString(Formatting.Indented);
            }


            public bool TryGetPublishDestination(out string exchange, out string routingKey)
            {
                try
                {
                    // Assume default strategies are used
                    exchange = new NamespaceMatchExchangeStrategy().GetExchange(type);
                    routingKey = new TypeNameRoutingKeyStrategy().GetRoutingKey(type);
                    return true;
                }
                catch
                {
                    exchange = "";
                    routingKey = "";
                    return false;
                }
            }


            public bool CanValidate()
            {
                return InitializeValidation();
            }


            public void Validate(string payload)
            {
                if (!InitializeValidation())
                    return;

                // Json exceptions are already handled by the PayloadEditorViewModel
                var deserialized = JsonConvert.DeserializeObject(payload, type);
                if (deserialized == null)
                    throw new PayloadValidationException(AssemblyParserStrings.JsonDeserializationNull, null);

                try
                {
                    var validationContext = new ValidationContext(deserialized);
                    Validator.ValidateObject(deserialized, validationContext, true);
                }
                catch (ValidationException e)
                {
                    var members = string.Join(", ", e.ValidationResult.MemberNames);
                    if (!string.IsNullOrEmpty(members))
                        throw new PayloadValidationException(string.Format(AssemblyParserStrings.ValidationErrorsMembers, members, e.ValidationResult.ErrorMessage), null);

                    throw new PayloadValidationException(string.Format(AssemblyParserStrings.ValidationErrors, e.ValidationResult.ErrorMessage), null);
                }
            }


            private bool InitializeValidation()
            {
                if (validationInitialized)
                    return validationAvailable;

                // Attempt to create an instance (only works if all dependencies are present, which is not yet
                // guaranteed because we aren't fetching NuGet dependencies yet). We're giving it a fighting chance
                // by referencing Tapeti.Annotations, System.ComponentModel.Annotations and Tapeti.DataAnnotations.Extensions in
                // this class library.
                try
                {
                    var instance = Activator.CreateInstance(type);
                    if (instance != null)
                    {
                        // Attributes are only evaluated when requested, so call validation once to give it a better chance to
                        // detect if we'll be able to validate the message
                        try
                        {
                            var validationContext = new ValidationContext(instance);
                            Validator.ValidateObject(instance, validationContext, true);

                            validationAvailable = true;
                        }
                        catch (ValidationException)
                        {
                            // The fact that it validated is good enough, this can be expected with an empty object
                            validationAvailable = true;
                        }
                        catch (Exception)
                        {
                            // ignored
                        }
                    }
                }
                catch (Exception)
                {
                    // No go, try to create an example without validation
                }

                validationInitialized = true;
                return validationAvailable;
            }
        }
    }
}
