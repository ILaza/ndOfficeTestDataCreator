using System;
using System.Collections.Generic;

using NetDocuments.IoC.Interfaces;
using NetDocuments.Rest.Infrastructure.ContentFormatters;

namespace NetDocuments.Automation.RestClient.Infrastructure
{
    /// <summary>
    /// Wrapper for dependency injection container
    /// </summary>
    public class FormattersContainerWrapper : IInjectionContainer
    {
        private static readonly IReadOnlyDictionary<Type, object> handlers = new Dictionary<Type, object>
        {
            [typeof(FormUrlEncodedFormatter)] = new FormUrlEncodedFormatter(),
            [typeof(JsonMultiObjectFormatter)] = new JsonMultiObjectFormatter(),
            [typeof(JsonSingleObjectFormatter)] = new JsonSingleObjectFormatter(),
            [typeof(MultipartFormatter)] = new MultipartFormatter(),
            [typeof(SimpleStringFormatter)] = new SimpleStringFormatter(),
            [typeof(SingleByteArrayFormatter)] = new SingleByteArrayFormatter(),
            [typeof(SingleStreamFormatter)] = new SingleStreamFormatter(),
        };

        /// <summary>
        /// <see cref="IInjectionContainer.IsInitialized"/>
        /// </summary>
        public bool IsInitialized { get; set; }

        /// <summary>
        /// <see cref="IInjectionContainer.Parent"/>
        /// </summary>
        public IInjectionContainer Parent { get; set; }

        /// <summary>
        /// Creates new instance of the <see cref="FormattersContainerWrapper"/>
        /// </summary>
        public FormattersContainerWrapper()
        {
            IsInitialized = true;
        }

        /// <summary>
        /// <see cref="IInjectionContainer.AddModule(IModule)"/>
        /// </summary>
        public IInjectionContainer AddModule(IModule module)
        {
            return this;
        }

        /// <summary>
        /// <see cref="IInjectionContainer.AddRegistration(IRegistration)"/>
        /// </summary>
        public IInjectionContainer AddRegistration(IRegistration registration)
        {
            return this;
        }

        /// <summary>
        /// <see cref="IInjectionContainer.RegisterItself"/>
        /// </summary>
        public IInjectionContainer RegisterItself()
        {
            return this;
        }

        /// <summary>
        /// <see cref="IInjectionContainer.Resolve(Type)"/>
        /// </summary>
        public object Resolve(Type type)
        {
            return handlers.TryGetValue(type, out var handler)
                ? handler
                : null;
        }

        /// <summary>
        /// <see cref="IInjectionContainer.Resolve{TType}"/>
        /// </summary>
        public TType Resolve<TType>()
        {
            return handlers.TryGetValue(typeof(TType), out var handler)
                ? (TType)handler
                : default;
        }

        /// <summary>
        /// <see cref="IInjectionContainer.GetRegistrations()"/>
        /// </summary>
        public IEnumerable<IRegistration> GetRegistrations()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// <see cref="IInjectionContainer.ClearRegistrations()"/>
        /// </summary>
        public void ClearRegistrations()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// <see cref="IInjectionContainer.IsTypeRegistered{T}"/>
        /// </summary>
        public bool IsTypeRegistered<T>()
        {
            return handlers.TryGetValue(typeof(T), out _);
        }

        /// <summary>
        /// <see cref="IInjectionContainer.Resolve{TType}(Action{TType})"/>
        /// </summary>
        public TType Resolve<TType>(Action<TType> action = null) where TType : class
        {
            return Resolve<TType>();
        }

        /// <summary>
        /// <see cref="IDisposable.Dispose"/>
        /// </summary>
        public void Dispose() { }
    }
}
