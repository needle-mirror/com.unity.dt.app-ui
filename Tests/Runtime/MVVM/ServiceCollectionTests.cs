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
        public void ServiceDescriptor_When_ShouldCreateConditionalDescriptor()
        {
            var originalDescriptor = ServiceDescriptor.Singleton(typeof(IServiceTest), typeof(ServiceTest));
            Assert.IsNull(originalDescriptor.condition);

            var conditionalDescriptor = originalDescriptor.When(ctx => ctx.RequestingType == typeof(ServiceTest));

            Assert.IsNotNull(conditionalDescriptor.condition);
            Assert.AreEqual(originalDescriptor.serviceType, conditionalDescriptor.serviceType);
            Assert.AreEqual(originalDescriptor.implementationType, conditionalDescriptor.implementationType);
            Assert.AreEqual(originalDescriptor.lifetime, conditionalDescriptor.lifetime);
        }

        [Test]
        public void ServiceCollection_AddSingletonWhen_ShouldAddConditionalService()
        {
            var services = new ServiceCollection();

            services.AddSingletonWhen(typeof(IServiceTest), typeof(ServiceTest),
                ctx => ctx.RequestingType == typeof(ServiceTest));

            Assert.AreEqual(1, services.Count);
            var descriptor = services[0];

            Assert.AreEqual(typeof(IServiceTest), descriptor.serviceType);
            Assert.AreEqual(typeof(ServiceTest), descriptor.implementationType);
            Assert.AreEqual(ServiceLifetime.Singleton, descriptor.lifetime);
            Assert.IsNotNull(descriptor.condition);
        }

        [Test]
        public void ServiceCollection_AddScopedWhen_ShouldAddConditionalService()
        {
            var services = new ServiceCollection();

            services.AddScopedWhen(typeof(IServiceTest), typeof(ServiceTest),
                ctx => ctx.IsScoped);

            Assert.AreEqual(1, services.Count);
            var descriptor = services[0];

            Assert.AreEqual(ServiceLifetime.Scoped, descriptor.lifetime);
            Assert.IsNotNull(descriptor.condition);
        }

        [Test]
        public void ServiceCollection_AddTransientWhen_ShouldAddConditionalService()
        {
            var services = new ServiceCollection();

            services.AddTransientWhen(typeof(IServiceTest), typeof(ServiceTest),
                ctx => ctx.ServiceType == typeof(IServiceTest));

            Assert.AreEqual(1, services.Count);
            var descriptor = services[0];

            Assert.AreEqual(ServiceLifetime.Transient, descriptor.lifetime);
            Assert.IsNotNull(descriptor.condition);
        }

        [Test]
        public void ServiceDescriptor_ExtensionWhen_ShouldCreateConditionalDescriptor()
        {
            var services = new ServiceCollection();
            var descriptor = ServiceDescriptor.Singleton(typeof(IServiceTest), typeof(ServiceTest));

            var conditionalDescriptor = descriptor.When(ctx => ctx.RequestingType != null);
            services.Add(conditionalDescriptor);

            Assert.AreEqual(1, services.Count);
            Assert.IsNotNull(services[0].condition);
        }

        [Test]
        public void ServiceCollection_AddSingletonInstance_StoresDescriptorWithInstance()
        {
            var services = new ServiceCollection();
            var instance = new ServiceTest();

            services.AddSingleton<IServiceTest>(instance);

            Assert.AreEqual(1, services.Count);
            var descriptor = services[0];
            Assert.AreEqual(typeof(IServiceTest), descriptor.serviceType);
            Assert.AreEqual(ServiceLifetime.Singleton, descriptor.lifetime);
            Assert.AreSame(instance, descriptor.implementationInstance);
            Assert.AreEqual(typeof(ServiceTest), descriptor.implementationType);
        }

        [Test]
        public void ServiceCollection_AddSingleton_NullInstance_Throws()
        {
            var services = new ServiceCollection();

            Assert.Throws<System.ArgumentNullException>(
                () => services.AddSingleton<IServiceTest>((IServiceTest)null));
            Assert.Throws<System.ArgumentNullException>(
                () => services.AddSingleton(typeof(IServiceTest), (object)null));
        }

        [Test]
        public void ServiceCollection_AddSingleton_InstanceTypeMismatch_Throws()
        {
            var services = new ServiceCollection();

            // Object literal is not assignable to IServiceTest.
            Assert.Throws<System.ArgumentException>(
                () => services.AddSingleton(typeof(IServiceTest), new object()));
        }

        [Test]
        public void ServiceDescriptor_InstanceWhen_PreservesInstance()
        {
            var instance = new ServiceTest();
            var descriptor = ServiceDescriptor.Singleton(typeof(IServiceTest), instance);

            var conditional = descriptor.When(ctx => true);

            Assert.AreSame(instance, conditional.implementationInstance);
            Assert.IsNotNull(conditional.condition);
            Assert.AreEqual(ServiceLifetime.Singleton, conditional.lifetime);
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

        [Test]
        public void AppUIServiceCollection_IsReadOnly_RejectsInstanceRegistrations()
        {
            var services = new AppUIServiceCollection { IsReadOnly = true };

            Assert.Throws<InvalidOperationException>(() => services.AddSingleton<IServiceTest>(new ServiceTest()));
            Assert.Throws<InvalidOperationException>(() => services.AddSingleton(typeof(IServiceTest), new ServiceTest()));
            Assert.AreEqual(0, services.Count);
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
