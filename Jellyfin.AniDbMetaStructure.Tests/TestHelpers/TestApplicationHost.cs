using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Emby.AniDbMetaStructure.Tests.IntegrationTests;
using MediaBrowser.Common;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Net;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Model.Events;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.Serialization;
using MediaBrowser.Model.Updates;
using NSubstitute;
using NUnit.Framework;
using SimpleInjector;

namespace Emby.AniDbMetaStructure.Tests.TestHelpers
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
            this.Container.Register<IHttpClient>(() => new TestHttpClient());
            this.Container.Register<ILogManager>(() => new ConsoleLogManager());
            this.Container.Register<IApplicationHost>(() => this);
            this.Container.Register<IXmlSerializer>(() => new TestXmlSerializer());

            this.Container.GetInstance(typeof(ILogManager));
        }

        public void NotifyPendingRestart()
        {
            throw new NotImplementedException();
        }

        public void Restart()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<T> GetExports<T>(bool manageLiftime = true)
        {
            throw new NotImplementedException();
        }

        public Task<CheckForUpdateResult> CheckForApplicationUpdate(CancellationToken cancellationToken,
            IProgress<double> progress)
        {
            throw new NotImplementedException();
        }

        public Task UpdateApplication(PackageVersionInfo package, CancellationToken cancellationToken,
            IProgress<double> progress)
        {
            throw new NotImplementedException();
        }

        public T Resolve<T>()
        {
            return (T)this.Container.GetInstance(typeof(T));
        }

        public T TryResolve<T>()
        {
            throw new NotImplementedException();
        }

        public Task Shutdown()
        {
            throw new NotImplementedException();
        }

        public void RemovePlugin(IPlugin plugin)
        {
            throw new NotImplementedException();
        }

        public void Init()
        {
            throw new NotImplementedException();
        }

        public Task Init(IProgress<double> progress)
        {
            throw new NotImplementedException();
        }

        public object CreateInstance(Type type)
        {
            throw new NotImplementedException();
        }

        public string GetValue(string name)
        {
            throw new NotImplementedException();
        }

        public bool ContainsStartupOption(string name)
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
        public bool CanSelfUpdate { get; }
        public bool IsFirstRun { get; }
        public List<string> FailedAssemblies { get; }
        public Type[] AllConcreteTypes { get; }
        public IPlugin[] Plugins { get; }
        public PackageVersionClass SystemUpdateLevel { get; }

        public bool IsStartupComplete { get; } = true;

        public event EventHandler<GenericEventArgs<PackageVersionInfo>> ApplicationUpdated;
        public event EventHandler HasPendingRestartChanged;
    }
}