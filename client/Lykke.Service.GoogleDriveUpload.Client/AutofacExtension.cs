using System;
using Autofac;
using Common.Log;
using Lykke.Common.Log;

namespace Lykke.Service.GoogleDriveUpload.Client
{
    public static class AutofacExtension
    {
        /// <summary>
        /// Adds Google Drive Upload client to the ContainerBuilder.
        /// </summary>
        /// <param name="builder">ContainerBuilder instance.</param>
        /// <param name="serviceUrl">Effective Google Drive service location.</param>
        /// <param name="log">Logger.</param>
        [Obsolete("Please, use the overload without explicitly passed logger.")]
        public static void RegisterGoogleDriveUploadClient(this ContainerBuilder builder, string serviceUrl, ILog log)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            if (log == null)
                throw new ArgumentNullException(nameof(log));

            if (string.IsNullOrWhiteSpace(serviceUrl))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(serviceUrl));

            builder.RegisterInstance(new GoogleDriveUploadClient(
                serviceUrl, 
                log))
                .As<IGoogleDriveUploadClient>()
                .SingleInstance();
        }

        /// <summary>
        /// Adds Google Drive Upload client to the ContainerBuilder.
        /// </summary>
        /// <param name="builder">ContainerBuilder instance. The implementation of ILogFactory should be already injected.</param>
        /// <param name="serviceUrl">Effective Google Drive service location.</param>
        public static void RegisterGoogleDriveUploadClient(this ContainerBuilder builder, string serviceUrl)
        {
            if (string.IsNullOrWhiteSpace(serviceUrl))
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(serviceUrl));

            builder.Register(ctx => new GoogleDriveUploadClient(
                serviceUrl,
                ctx.Resolve<ILogFactory>()))
                .As<IGoogleDriveUploadClient>()
                .SingleInstance();
        }
    }
}
