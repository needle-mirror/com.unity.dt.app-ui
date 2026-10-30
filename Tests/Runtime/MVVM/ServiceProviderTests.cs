using NUnit.Framework;
using Unity.AppUI.MVVM;

namespace Unity.AppUI.Tests.MVVM
{
    [TestFixture]
    [TestOf(typeof(ServiceProvider))]
    class ServiceProviderTests
    {
        [Test]
        public void ServiceProvider_ShouldCreateServiceProvider()
        {
            var services = new ServiceCollection();
            var serviceProvider = new ServiceProvider(services);
            Assert.IsNotNull(serviceProvider);
        }

        [Test]
        public void ServiceProvider_WithNullCollection_ShouldThrow()
        {
            Assert.Throws<System.ArgumentNullException>(() => new ServiceProvider(null));
        }

        [Test]
        public void ServiceProvider_ShouldDispose()
        {
            var services = new ServiceCollection();
            var serviceProvider = new ServiceProvider(services);
            serviceProvider.Dispose();
            Assert.IsTrue(serviceProvider.disposed);
            Assert.Throws<System.ObjectDisposedException>(() => serviceProvider.GetService<object>());
        }

        [Test]
        public void ServiceProvider_WithValidService_ShouldRealizeService()
        {
            var services = new ServiceCollection();
            services.AddSingleton<ServiceWithValidConstructor>();
            services.AddSingleton<ServiceWithValidConstructor2>();
            var serviceProvider = new ServiceProvider(services);
            var service = serviceProvider.GetService<ServiceWithValidConstructor>();
            Assert.IsNotNull(service);

            var service2 = serviceProvider.GetService<ServiceWithValidConstructor2>();
            Assert.IsNotNull(service2);
            Assert.IsNotNull(service2.dep);
        }

        [Test]
        public void ServiceProvider_ThatNotContainServiceDesc_ShouldThrow()
        {
            var services = new ServiceCollection();
            var serviceProvider = new ServiceProvider(services);
            Assert.Throws<System.InvalidOperationException>(() => serviceProvider.GetService<ServiceWithValidConstructor>());
        }

        [Test]
        public void ServiceProvider_WithInvalidService_ShouldThrow()
        {
            var services = new ServiceCollection();
            services.AddSingleton<ServiceWithInvalidConstructor>();
            var serviceProvider = new ServiceProvider(services);
            Assert.Throws<System.InvalidOperationException>(() => serviceProvider.GetService<ServiceWithInvalidConstructor>());
        }

        [Test]
        public void ServiceProvider_WithServiceUsingAttributes_ShouldRealizeService()
        {
            var services = new ServiceCollection();
            services.AddSingleton<ServiceWithValidConstructor>();
            services.AddSingleton<ServiceWithValidConstructor2>();
            services.AddSingleton<ServiceWithAttributes>();
            var serviceProvider = new ServiceProvider(services);
            var service = serviceProvider.GetService<ServiceWithAttributes>();
            Assert.IsNotNull(service);
            Assert.IsNotNull(service.dependency1);
            Assert.IsNotNull(service.dependency2);
            Assert.IsNotNull(service.dependency2.dep);
            Assert.AreEqual(1, service.onDependenciesInjectedCalled);
        }

        [Test]
        public void ServiceProvider_ShouldCreateScope()
        {
            var services = new ServiceCollection();
            services.AddSingleton<ServiceWithValidConstructor>();
            services.AddSingleton<ServiceWithValidConstructor2>();
            services.AddScoped<ServiceWithAttributes>();
            var serviceProvider = services.BuildServiceProvider();

            var service1 = serviceProvider.GetService<ServiceWithValidConstructor>();
            Assert.IsNotNull(service1);
            var service2 = serviceProvider.GetService<ServiceWithValidConstructor2>();
            Assert.IsNotNull(service2);

            var service3 = serviceProvider.GetService<ServiceWithAttributes>();
            Assert.IsNotNull(service3);

            using (var scope = serviceProvider.CreateScope())
            {
                var scoped1 = scope.ServiceProvider.GetService<ServiceWithValidConstructor>();
                Assert.IsNotNull(scoped1);
                Assert.AreSame(service1, scoped1);

                var scoped2 = scope.ServiceProvider.GetService<ServiceWithValidConstructor2>();
                Assert.IsNotNull(scoped2);
                Assert.AreSame(service2, scoped2);

                var scoped3 = scope.ServiceProvider.GetService<ServiceWithAttributes>();
                Assert.IsNotNull(scoped3);
                Assert.IsNotNull(scoped3.dependency1);
                Assert.IsNotNull(scoped3.dependency2);
                Assert.IsNotNull(scoped3.dependency2.dep);
                Assert.AreEqual(1, scoped3.onDependenciesInjectedCalled);

                Assert.AreNotSame(service3, scoped3);
            }
        }

        public class ServiceWithValidConstructor
        {
            public ServiceWithValidConstructor() { }
        }

        public class ServiceWithValidConstructor2
        {
            public ServiceWithValidConstructor dep { get; }

            public ServiceWithValidConstructor2(ServiceWithValidConstructor s)
            {
                dep = s;
            }
        }

        public class ServiceWithInvalidConstructor
        {
            public ServiceWithInvalidConstructor(int i) { }
        }

        public class ServiceWithAttributes : IDependencyInjectionListener
        {
            [Service]
            public ServiceWithValidConstructor dependency1;

            [Service]
            public ServiceWithValidConstructor2 dependency2 { get; private set; }

            public int onDependenciesInjectedCalled { get; private set; }

            public void OnDependenciesInjected()
            {
                onDependenciesInjectedCalled++;
            }
        }

        [Test]
        public void ServiceProvider_ConditionalResolution_ShouldResolveBasedOnContext()
        {
            var services = new ServiceCollection();

            // Add conditional services for ITestInterface
            services.AddSingletonWhen(typeof(ITestInterface), typeof(TestImplementation1),
                ctx => ctx.RequestingType == typeof(ServiceConsumer1));

            services.AddSingletonWhen(typeof(ITestInterface), typeof(TestImplementation2),
                ctx => ctx.RequestingType == typeof(ServiceConsumer2));

            services.AddSingleton<ServiceConsumer1>();
            services.AddSingleton<ServiceConsumer2>();

            var provider = services.BuildServiceProvider();

            var consumer1 = provider.GetService<ServiceConsumer1>();
            var consumer2 = provider.GetService<ServiceConsumer2>();

            Assert.IsNotNull(consumer1);
            Assert.IsNotNull(consumer2);
            Assert.IsNotNull(consumer1.dependency);
            Assert.IsNotNull(consumer2.dependency);

            Assert.IsInstanceOf<TestImplementation1>(consumer1.dependency);
            Assert.IsInstanceOf<TestImplementation2>(consumer2.dependency);
        }

        [Test]
        public void ServiceProvider_ConditionalResolution_WithNoMatchingCondition_ShouldThrow()
        {
            var services = new ServiceCollection();

            services.AddSingletonWhen(typeof(ITestInterface), typeof(TestImplementation1),
                ctx => ctx.RequestingType == typeof(ServiceConsumer1));

            services.AddSingleton<ServiceConsumer2>();

            var provider = services.BuildServiceProvider();

            Assert.Throws<System.InvalidOperationException>(() => provider.GetService<ServiceConsumer2>());
        }

        [Test]
        public void ServiceProvider_ConditionalResolution_WithScopeContext_ShouldWork()
        {
            var services = new ServiceCollection();

            services.AddSingletonWhen(typeof(ITestInterface), typeof(TestImplementation1),
                ctx => !ctx.IsScoped);

            services.AddScopedWhen(typeof(ITestInterface), typeof(TestImplementation2),
                ctx => ctx.IsScoped);

            services.AddSingleton<ServiceConsumer1>();
            services.AddScoped<ServiceConsumer2>();

            var provider = services.BuildServiceProvider();

            var rootConsumer = provider.GetService<ServiceConsumer1>();
            Assert.IsInstanceOf<TestImplementation1>(rootConsumer.dependency);

            using var scope = provider.CreateScope();
            var scopedConsumer = scope.ServiceProvider.GetService<ServiceConsumer2>();
            Assert.IsInstanceOf<TestImplementation2>(scopedConsumer.dependency);
        }

        [Test]
        public void ServiceProvider_ConditionalResolution_WithAttributeInjection_ShouldWork()
        {
            var services = new ServiceCollection();

            services.AddSingletonWhen(typeof(ITestInterface), typeof(TestImplementation1),
                ctx => ctx.RequestingType == typeof(ServiceWithConditionalAttributes));

            services.AddSingleton<ServiceWithConditionalAttributes>();

            var provider = services.BuildServiceProvider();
            var service = provider.GetService<ServiceWithConditionalAttributes>();

            Assert.IsNotNull(service);
            Assert.IsNotNull(service.dependency);
            Assert.IsInstanceOf<TestImplementation1>(service.dependency);
        }

        [Test]
        public void ServiceProvider_ConditionalResolution_MultipleConditions_ShouldMatchFirst()
        {
            var services = new ServiceCollection();

            // First condition should match
            services.AddSingletonWhen(typeof(ITestInterface), typeof(TestImplementation1),
                ctx => ctx.ServiceType == typeof(ITestInterface));

            // Second condition would also match but shouldn't be reached
            services.AddSingletonWhen(typeof(ITestInterface), typeof(TestImplementation2),
                ctx => ctx.ServiceType == typeof(ITestInterface));

            services.AddSingleton<ServiceConsumer1>();

            var provider = services.BuildServiceProvider();
            var consumer = provider.GetService<ServiceConsumer1>();

            Assert.IsInstanceOf<TestImplementation1>(consumer.dependency);
        }

        // Test interfaces and implementations for conditional resolution
        public interface ITestInterface { }

        public class TestImplementation1 : ITestInterface { }

        public class TestImplementation2 : ITestInterface { }

        public class ServiceConsumer1
        {
            public ITestInterface dependency { get; }

            public ServiceConsumer1(ITestInterface dep)
            {
                dependency = dep;
            }
        }

        public class ServiceConsumer2
        {
            public ITestInterface dependency { get; }

            public ServiceConsumer2(ITestInterface dep)
            {
                dependency = dep;
            }
        }

        public class ServiceWithConditionalAttributes
        {
            [Service]
            public ITestInterface dependency;
        }

        [Test]
        public void ServiceProvider_WithInstance_ReturnsSameInstance()
        {
            var services = new ServiceCollection();
            var instance = new InstanceService();
            services.AddSingleton<IInstanceService>(instance);

            var provider = services.BuildServiceProvider();

            var resolved1 = provider.GetService<IInstanceService>();
            var resolved2 = provider.GetService<IInstanceService>();

            Assert.AreSame(instance, resolved1);
            Assert.AreSame(instance, resolved2);
        }

        [Test]
        public void ServiceProvider_WithInstance_AcrossScopes_ReturnsSameInstance()
        {
            var services = new ServiceCollection();
            var instance = new InstanceService();
            services.AddSingleton<IInstanceService>(instance);

            var provider = services.BuildServiceProvider();
            var root = provider.GetService<IInstanceService>();

            using var scope = provider.CreateScope();
            var scoped = scope.ServiceProvider.GetService<IInstanceService>();

            Assert.AreSame(instance, root);
            Assert.AreSame(instance, scoped);
        }

        [Test]
        public void ServiceProvider_WithInstance_DoesNotInvokeConstructor()
        {
            var services = new ServiceCollection();
            // ThrowingService's only public ctor throws — registering an instance must bypass it.
            var instance = ThrowingService.CreatePreBuilt();
            services.AddSingleton<ThrowingService>(instance);

            var provider = services.BuildServiceProvider();

            Assert.DoesNotThrow(() => provider.GetService<ThrowingService>());
            Assert.AreSame(instance, provider.GetService<ThrowingService>());
        }

        [Test]
        public void ServiceProvider_WithInstance_AsDependency_InjectsIntoOtherServices()
        {
            var services = new ServiceCollection();
            var instance = new InstanceService();
            services.AddSingleton<IInstanceService>(instance);
            services.AddSingleton<ConsumerOfInstance>();

            var provider = services.BuildServiceProvider();
            var consumer = provider.GetService<ConsumerOfInstance>();

            Assert.IsNotNull(consumer);
            Assert.AreSame(instance, consumer.dep);
        }

        [Test]
        public void ServiceProvider_WithInstance_DoesNotRunFieldInjection()
        {
            var services = new ServiceCollection();
            services.AddSingleton<ServiceWithValidConstructor>();
            // Register a pre-built instance whose [Service] field should remain null.
            var instance = new ServiceWithFieldInjection();
            services.AddSingleton<ServiceWithFieldInjection>(instance);

            var provider = services.BuildServiceProvider();
            var resolved = provider.GetService<ServiceWithFieldInjection>();

            Assert.AreSame(instance, resolved);
            Assert.IsNull(resolved.dependency);
        }

        [Test]
        public void ServiceProvider_WithFactory_ForwardsToSameSingleton()
        {
            var services = new ServiceCollection();
            services.AddSingleton<IInstanceService, InstanceService>();
            services.Add(ServiceDescriptor.Singleton<IFactoryConsumer>(sp => (IFactoryConsumer)sp.GetService(typeof(IFactoryLifecycle))));
            services.AddSingleton<IFactoryLifecycle, FactoryService>();

            var provider = services.BuildServiceProvider();

            var lifecycle = provider.GetService<IFactoryLifecycle>();
            var consumer = provider.GetService<IFactoryConsumer>();
            Assert.AreSame(lifecycle, consumer);
        }

        [Test]
        public void ServiceProvider_WithFactory_RespectsLifetimes()
        {
            var services = new ServiceCollection();
            var calls = 0;
            services.AddSingleton<IFactoryLifecycle>(sp => { calls++; return new FactoryService(); });
            services.AddScoped<IFactoryConsumer>(sp => new FactoryService());
            services.AddTransient<IInstanceService>(sp => new InstanceService());

            var provider = services.BuildServiceProvider();

            Assert.AreSame(provider.GetService<IFactoryLifecycle>(), provider.GetService<IFactoryLifecycle>());
            Assert.AreEqual(1, calls);
            Assert.AreNotSame(provider.GetService<IInstanceService>(), provider.GetService<IInstanceService>());
            Assert.AreSame(provider.GetService<IFactoryConsumer>(), provider.GetService<IFactoryConsumer>());
            using var scope = provider.CreateScope();
            Assert.AreNotSame(provider.GetService<IFactoryConsumer>(), scope.ServiceProvider.GetService<IFactoryConsumer>());
        }

        [Test]
        public void ServiceDescriptor_WithNullFactory_Throws()
        {
            Assert.Throws<System.ArgumentNullException>(() => ServiceDescriptor.Singleton(typeof(IInstanceService), (System.Func<System.IServiceProvider, object>)null));
            Assert.Throws<System.ArgumentNullException>(() => new ServiceCollection().AddSingleton<IInstanceService>((System.Func<System.IServiceProvider, IInstanceService>)null));
        }

        [Test]
        public void ServiceProvider_WithSingletonFactory_RunsOnceForConcurrentFirstResolve()
        {
            var calls = 0;
            var services = new ServiceCollection();
            services.AddSingleton<IFactoryLifecycle>(sp => CountedFactory(ref calls));
            var provider = services.BuildServiceProvider();

            var results = ResolveFromTwoThreads(() => provider.GetService<IFactoryLifecycle>());

            Assert.AreEqual(1, calls);
            Assert.IsNotNull(results[0]);
            Assert.AreSame(results[0], results[1]);
        }

        [Test]
        public void ServiceProvider_WithScopedFactory_RunsOnceForConcurrentFirstResolveInAScope()
        {
            var calls = 0;
            var services = new ServiceCollection();
            services.AddScoped<IFactoryLifecycle>(sp => CountedFactory(ref calls));
            var provider = services.BuildServiceProvider();
            using var scope = provider.CreateScope();

            var results = ResolveFromTwoThreads(() => scope.ServiceProvider.GetService<IFactoryLifecycle>());

            Assert.AreEqual(1, calls);
            Assert.IsNotNull(results[0]);
            Assert.AreSame(results[0], results[1]);
        }

        [Test]
        public void ServiceProvider_WithSingletonFactory_RetriesAfterTheFactoryFails()
        {
            var calls = 0;
            var services = new ServiceCollection();
            services.AddSingleton<IFactoryLifecycle>(sp => calls++ == 0 ? null : new FactoryService());
            var provider = services.BuildServiceProvider();

            Assert.Throws<System.InvalidOperationException>(() => provider.GetService<IFactoryLifecycle>());
            Assert.IsNotNull(provider.GetService<IFactoryLifecycle>());
            Assert.AreEqual(2, calls);
        }

        [Test]
        public void ServiceProvider_WithConditionalScopedFactories_ResolvesEachConsumersOwnFactory()
        {
            var services = new ServiceCollection();
            services.Add(ServiceDescriptor.Scoped<ITestInterface>(sp => new TestImplementation1())
                .When(ctx => ctx.RequestingType == typeof(ServiceConsumer1)));
            services.Add(ServiceDescriptor.Scoped<ITestInterface>(sp => new TestImplementation2())
                .When(ctx => ctx.RequestingType == typeof(ServiceConsumer2)));
            services.AddScoped<ServiceConsumer1>();
            services.AddScoped<ServiceConsumer2>();
            var provider = services.BuildServiceProvider();

            using var scope = provider.CreateScope();
            Assert.IsInstanceOf<TestImplementation1>(scope.ServiceProvider.GetService<ServiceConsumer1>().dependency);
            Assert.IsInstanceOf<TestImplementation2>(scope.ServiceProvider.GetService<ServiceConsumer2>().dependency);
        }

        [Test]
        public void ServiceProvider_WithConditionalSingletonFactory_SharesOneInstanceAcrossConsumers()
        {
            var calls = 0;
            var services = new ServiceCollection();
            services.Add(ServiceDescriptor.Singleton<ITestInterface>(sp => { calls++; return new TestImplementation1(); })
                .When(ctx => ctx.RequestingType == typeof(ServiceConsumer1) || ctx.RequestingType == typeof(ServiceConsumer2)));
            services.AddSingleton<ServiceConsumer1>();
            services.AddSingleton<ServiceConsumer2>();
            var provider = services.BuildServiceProvider();

            Assert.AreSame(provider.GetService<ServiceConsumer1>().dependency, provider.GetService<ServiceConsumer2>().dependency);
            Assert.AreEqual(1, calls);
        }

        // The first caller waits inside the factory for a second caller. Without a once-only guarantee the
        // second caller enters the factory too. With it, the wait times out and the second caller reuses the result.
        static readonly System.Threading.ManualResetEventSlim s_SecondFactoryCall = new System.Threading.ManualResetEventSlim();

        static FactoryService CountedFactory(ref int calls)
        {
            if (System.Threading.Interlocked.Increment(ref calls) > 1)
                s_SecondFactoryCall.Set();
            else
                s_SecondFactoryCall.Wait(500);
            return new FactoryService();
        }

        static object[] ResolveFromTwoThreads(System.Func<object> resolve)
        {
            s_SecondFactoryCall.Reset();
            var results = new object[2];
            var errors = new System.Exception[2];
            using var start = new System.Threading.Barrier(2);
            var threads = new System.Threading.Thread[2];
            for (var i = 0; i < threads.Length; i++)
            {
                var index = i;
                threads[i] = new System.Threading.Thread(() =>
                {
                    try
                    {
                        start.SignalAndWait();
                        results[index] = resolve();
                    }
                    catch (System.Exception e)
                    {
                        errors[index] = e;
                    }
                });
                threads[i].Start();
            }

            foreach (var thread in threads)
                Assert.IsTrue(thread.Join(5000), "A resolving thread did not finish.");
            foreach (var error in errors)
                Assert.IsNull(error, error?.ToString());
            return results;
        }

        public interface IFactoryConsumer { }

        public interface IFactoryLifecycle : IFactoryConsumer { }

        public class FactoryService : IFactoryLifecycle { }

        public interface IInstanceService { }

        public class InstanceService : IInstanceService { }

        public class ConsumerOfInstance
        {
            public IInstanceService dep { get; }

            public ConsumerOfInstance(IInstanceService dep)
            {
                this.dep = dep;
            }
        }

        public class ThrowingService
        {
            public ThrowingService()
            {
                throw new System.InvalidOperationException("Constructor must not be invoked for instance-registered services.");
            }

            ThrowingService(bool _) { }

            public static ThrowingService CreatePreBuilt() => new ThrowingService(true);
        }

        public class ServiceWithFieldInjection
        {
            [Service]
            public ServiceWithValidConstructor dependency;
        }
    }
}
