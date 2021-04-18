using Jellyfin.AniDbMetaStructure.Tests.IntegrationTests;
using MediaBrowser.Common;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Model.Serialization;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;
using SimpleInjector;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace Jellyfin.AniDbMetaStructure.Tests.TestHelpers
{
    public class TestApplicationHost : IApplicationHost
    {
        protected readonly Container Container;

        public TestApplicationHost()
        {
            var applicationPaths = Substitute.For<IApplicationPaths>();
            applicationPaths.CachePath.Returns(TestContext.CurrentContext.WorkDirectory + @"\" + Guid.NewGuid() +
                @"\CachePath");

            DependencyConfiguration.Reset();

            this.Container = new Container();

            this.Container.Register(() => applicationPaths);
            this.Container.Register<ILogger>(() => new ConsoleLogger());
            this.Container.Register<IApplicationHost>(() => this);
            this.Container.Register<IXmlSerializer>(() => new TestXmlSerializer());

            this.Container.GetInstance(typeof(ILogger));
        }

        public void NotifyPendingRestart()
        {
            throw new NotImplementedException();
        }

        public void Restart()
        {
            throw new NotImplementedException();
        }

        public IReadOnlyCollection<T> GetExports<T>(bool manageLifetime = true)
        {
            throw new NotImplementedException();
        }

        public IReadOnlyCollection<T> GetExports<T>(CreationDelegate defaultFunc, bool manageLifetime = true)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Assembly> GetApiPluginAssemblies()
        {
            throw new NotImplementedException();
        }

        public T Resolve<T>()
        {
            return (T)this.Container.GetInstance(typeof(T));
        }


        public Task Shutdown()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Type> GetExportTypes<T>()
        {
            throw new NotImplementedException();
        }

        public void Init()
        {
            throw new NotImplementedException();
        }

        public object CreateInstance(Type type)
        {
            throw new NotImplementedException();
        }

        public string OperatingSystemDisplayName { get; }
        public string Name { get; }
        public string SystemId { get; }
        public bool HasPendingRestart { get; }
        public bool IsShuttingDown { get; }
        public bool CanSelfRestart { get; }
        public Version ApplicationVersion { get; }

        public IServiceProvider ServiceProvider { get; set; }

        public string ApplicationVersionString { get; }

        public string ApplicationUserAgent { get; }

        public string ApplicationUserAgentAddress { get; }

        public event EventHandler HasPendingRestartChanged;
    }
}