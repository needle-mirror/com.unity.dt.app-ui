// This file draws upon the concepts found in the ServiceDescriptor implementation from the .NET Runtime library (dotnet/runtime),
// more information in Third Party Notices.md

using System;
using System.Collections.Concurrent;
using System.Reflection;

namespace Unity.AppUI.MVVM
{
    /// <summary>
    /// The default IServiceProvider.
    /// </summary>
    public sealed class ServiceProvider : IServiceProvider, IDisposable
    {
        readonly IServiceCollection m_Services;

        readonly ConcurrentDictionary<Type, Func<object>> m_RealizedServices;

        readonly ConcurrentDictionary<Type, object> m_Singletons;

        bool m_Disposed;
        
        /// <summary>
        /// Create a new ServiceProvider.
        /// </summary>
        /// <param name="serviceCollection"> The service collection to use. </param>
        public ServiceProvider(IServiceCollection serviceCollection)
        {
            m_Services = serviceCollection;
            m_RealizedServices = new ConcurrentDictionary<Type, Func<object>>();
            m_Singletons = new ConcurrentDictionary<Type, object>();
        }
        
        Func<object> RealizeService(Type serviceType)
        {
            ServiceDescriptor desc = null;

            foreach (var d in m_Services)
            {
                if (d.serviceType == serviceType)
                {
                    desc = d;
                    break;
                }
            }
            
            if (desc == null)
                throw new InvalidOperationException($"No service registered for type {serviceType.FullName}.");
            
            var constructors = desc.implementationType.GetConstructors(BindingFlags.Public | BindingFlags.Instance);
            if (constructors.Length == 0)
                throw new InvalidOperationException(
                    $"The type {desc.implementationType.FullName} doesn't contain any public constructor.");
            
            ConstructorInfo bestConstructor = null;
            
            foreach (var constructor in constructors)
            {
                if (IsValidConstructor(constructor))
                {
                    bestConstructor = constructor;
                    break;
                }
            }
            
            if (bestConstructor == null)
                throw new InvalidOperationException(
                    $"The type {desc.implementationType.FullName} doesn't contain any valid constructor.");

            return () =>
            {
                if (desc.lifetime == ServiceLifetime.Singleton)
                {
                    if (!m_Singletons.ContainsKey(serviceType))
                        m_Singletons[serviceType] = bestConstructor.Invoke(GetConstructorParameters(bestConstructor));
                    return m_Singletons[serviceType];
                }
                return bestConstructor.Invoke(GetConstructorParameters(bestConstructor));
            };
        }

        object[] GetConstructorParameters(ConstructorInfo info)
        {
            var parameters = info.GetParameters();
            var result = new object[parameters.Length];
            
            for (var i = 0; i < parameters.Length; i++)
            {
                var parameter = parameters[i];
                result[i] = GetService(parameter.ParameterType);
            }
            
            return result;
        }

        bool IsValidConstructor(ConstructorInfo info)
        {
            var res = true;
            
            foreach (var param in info.GetParameters())
            {
                res &= IsValidConstructorParameter(param);
            }
            
            return res;
        }

        bool IsValidConstructorParameter(ParameterInfo param)
        {
            foreach (var service in m_Services)
            {
                if (service.serviceType.IsAssignableFrom(param.ParameterType))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Get a service from the service provider.
        /// </summary>
        /// <param name="serviceType"> The type of the service to get. </param>
        /// <returns> The service instance. </returns>
        /// <exception cref="InvalidOperationException"> Thrown when the requested service is not registered. </exception>
        public object GetService(Type serviceType)
        {
            if (m_Disposed)
                throw new InvalidOperationException($"The {nameof(ServiceProvider)} object has already been disposed.");
            
            ServiceDescriptor desc = null;
            foreach (var d in m_Services)
            {
                if (d.serviceType == serviceType)
                {
                    desc = d;
                    break;
                }
            }
            
            if (desc == null)
                throw new InvalidOperationException($"Unable to find Service Descriptor for {serviceType.FullName}.");
            
            var realizedService = m_RealizedServices.GetOrAdd(serviceType, RealizeService);
            return realizedService?.Invoke();
        }
        
        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (m_Disposed)
                return;
            
            m_RealizedServices.Clear();
            m_Singletons.Clear();
            m_Disposed = true;
        }
    }
}
