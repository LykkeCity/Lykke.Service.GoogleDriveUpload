using Autofac;
using Autofac.Extensions.DependencyInjection;
using Common.Log;
using Lykke.Service.GoogleDriveUpload.Core;
using Lykke.Service.GoogleDriveUpload.Core.Services;
using Lykke.Service.GoogleDriveUpload.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Lykke.Service.GoogleDriveUpload.Modules
{
    public class ServiceModule : Module
    {
        private readonly GoogleDriveUploadSettings _settings;
        private readonly ILog _log;
        // NOTE: you can remove it if you don't need to use IServiceCollection extensions to register service specific dependencies
        private readonly IServiceCollection _services;

        public ServiceModule(GoogleDriveUploadSettings settings, ILog log)
        {
            _settings = settings;
            _log = log;

            _services = new ServiceCollection();
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterInstance(_settings)
                .SingleInstance();

            builder.RegisterInstance(_log)
                .As<ILog>()
                .SingleInstance();

            builder.RegisterType<HealthService>()
                .As<IHealthService>()
                .SingleInstance();

            // TODO: Add your dependencies here

            builder.RegisterType<GoogleDriveService>()
                .As<IGoogleDriveService>()
                .SingleInstance();

            builder.Populate(_services);
        }
    }
}
