using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Unity.AppUI.MVVM;

namespace Unity.AppUI.Tests.MVVM
{
    [TestFixture]
    [TestOf(typeof(ServiceCollection))]
    class ServiceCollectionTests
    {
        interface IServiceTest { }
        class ServiceTest : IServiceTest { }
        class AlternateServiceTest : IServiceTest { }

        [Test]
        public void ServiceCollection_ShouldContainServices()
        {
            var services = new ServiceCollection();

            var serviceDescriptor =
                new ServiceDescriptor(
                    typeof(IServiceTest),
                    typeof(ServiceTest),
                    ServiceLifetime.Singleton);

            var serviceDescriptor2 =
                new ServiceDescriptor(
                    typeof(IServiceTest),
                    typeof(ServiceTest),
                    ServiceLifetime.Transient);

            services.Add(serviceDescriptor2);
            services.Insert(0, serviceDescriptor);

            Assert.AreEqual(2, services.Count);

            Assert.AreEqual(serviceDescriptor, services[0]);
            Assert.AreEqual(serviceDescriptor2, services[1]);

            Assert.AreEqual(0, services.IndexOf(serviceDescriptor));
            Assert.AreEqual(1, services.IndexOf(serviceDescriptor2));

            Assert.IsFalse(services.IsReadOnly);

            var array = new ServiceDescriptor[2];
            Assert.DoesNotThrow(() => services.CopyTo(array, 0));

            foreach (var desc in services)
            {
                Assert.IsTrue(array.Contains(desc));
            }

            services.Remove(serviceDescriptor);
            Assert.AreEqual(1, services.Count);

            services.RemoveAt(0);
            Assert.AreEqual(0, services.Count);

            services.Clear();
            Assert.AreEqual(0, services.Count);
        }

        [Test]
        public void AppUIServiceCollection_IsReadOnly_RejectsChanges()
        {
            var services = new AppUIServiceCollection();
            Assert.IsFalse(((IServiceCollection)services).IsReadOnly);

            var descriptor = ServiceDescriptor.Singleton(typeof(IServiceTest), typeof(ServiceTest));
            services.Add(descriptor);
            var provider = services.BuildServiceProvider();
            services.IsReadOnly = true;

            Assert.IsTrue(services.IsReadOnly);
            Assert.IsTrue(((ICollection<ServiceDescriptor>)services).IsReadOnly);
            Assert.IsTrue(((IServiceCollection)services).IsReadOnly);

            var other = ServiceDescriptor.Transient(typeof(AlternateServiceTest), typeof(AlternateServiceTest));
            Assert.Throws<InvalidOperationException>(() => services.Add(other));
            Assert.Throws<InvalidOperationException>(() => ((IServiceCollection)services).Add(other));
            Assert.Throws<InvalidOperationException>(() => services.Insert(0, other));
            Assert.Throws<InvalidOperationException>(() => services[0] = other);
            Assert.Throws<InvalidOperationException>(() => services.Remove(descriptor));
            Assert.Throws<InvalidOperationException>(() => services.RemoveAt(0));
            Assert.Throws<InvalidOperationException>(() => services.Clear());
            Assert.Throws<InvalidOperationException>(() => services.AddSingleton<AlternateServiceTest>());
            Assert.Throws<InvalidOperationException>(() => services.AddScoped<AlternateServiceTest>());
            Assert.Throws<InvalidOperationException>(() => services.AddTransient<AlternateServiceTest>());
            Assert.Throws<InvalidOperationException>(() => services.TryAddSingleton<AlternateServiceTest>());

            Assert.AreEqual(1, services.Count);
            Assert.AreSame(descriptor, services[0]);
            Assert.Throws<InvalidOperationException>(() => provider.GetService(typeof(AlternateServiceTest)));

            services.IsReadOnly = false;
            Assert.DoesNotThrow(() => services.Add(other));
            Assert.AreEqual(2, services.Count);
        }

        static IEnumerable<TestCaseData> TryAddCases()
        {
            yield return new TestCaseData(ServiceLifetime.Singleton, (Action<IServiceCollection>)(s => s.TryAddSingleton<IServiceTest, ServiceTest>()), typeof(IServiceTest)).SetName("TryAddSingleton<TService, TImplementation>");
            yield return new TestCaseData(ServiceLifetime.Singleton, (Action<IServiceCollection>)(s => s.TryAddSingleton<ServiceTest>()), typeof(ServiceTest)).SetName("TryAddSingleton<TService>");
            yield return new TestCaseData(ServiceLifetime.Singleton, (Action<IServiceCollection>)(s => s.TryAddSingleton(typeof(ServiceTest))), typeof(ServiceTest)).SetName("TryAddSingleton(Type)");
            yield return new TestCaseData(ServiceLifetime.Scoped, (Action<IServiceCollection>)(s => s.TryAddScoped<IServiceTest, ServiceTest>()), typeof(IServiceTest)).SetName("TryAddScoped<TService, TImplementation>");
            yield return new TestCaseData(ServiceLifetime.Scoped, (Action<IServiceCollection>)(s => s.TryAddScoped<ServiceTest>()), typeof(ServiceTest)).SetName("TryAddScoped<TService>");
            yield return new TestCaseData(ServiceLifetime.Scoped, (Action<IServiceCollection>)(s => s.TryAddScoped(typeof(ServiceTest))), typeof(ServiceTest)).SetName("TryAddScoped(Type)");
            yield return new TestCaseData(ServiceLifetime.Transient, (Action<IServiceCollection>)(s => s.TryAddTransient<IServiceTest, ServiceTest>()), typeof(IServiceTest)).SetName("TryAddTransient<TService, TImplementation>");
            yield return new TestCaseData(ServiceLifetime.Transient, (Action<IServiceCollection>)(s => s.TryAddTransient<ServiceTest>()), typeof(ServiceTest)).SetName("TryAddTransient<TService>");
            yield return new TestCaseData(ServiceLifetime.Transient, (Action<IServiceCollection>)(s => s.TryAddTransient(typeof(ServiceTest))), typeof(ServiceTest)).SetName("TryAddTransient(Type)");
        }

        [Test]
        [TestCaseSource(nameof(TryAddCases))]
        public void ServiceCollection_TryAdd_UsesLifetimeAndAddsOnce(ServiceLifetime lifetime, Action<IServiceCollection> tryAdd, Type serviceType)
        {
            var services = new ServiceCollection();

            tryAdd(services);
            tryAdd(services);

            Assert.AreEqual(1, services.Count);
            Assert.AreEqual(serviceType, services[0].serviceType);
            Assert.AreEqual(lifetime, services[0].lifetime);
        }
    }
}
