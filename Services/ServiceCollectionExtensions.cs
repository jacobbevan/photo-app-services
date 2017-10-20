using System;
using Amazon.S3;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using photo_api.Services;

namespace Microsoft.Extensions.DependencyInjection
{
    public enum ImageProviderType
    {
        File,
        AWS
    }

    public static class ServiceCollectionExtensions
    {

        public static IServiceCollection AddFileBasedImageProvider(this IServiceCollection collection)
        {

            Func<IServiceProvider, object> factory = (s) => new FileImageProvider(s.GetService<ILoggerFactory>());
            var descriptor = new ServiceDescriptor(typeof(IImageProvider), factory, ServiceLifetime.Singleton);
            collection.Add(descriptor);
            return collection;            
        }

        public static IServiceCollection AddAWSBasedImageProvider(this IServiceCollection collection)
        {
            Func<IServiceProvider, object> factory = (s) => 
            {
                var opts = s.GetService<IOptions<BucketOptions>>();
                return new AWSImageProvider(opts.Value, s.GetService<IAmazonS3>(), s.GetService<ILoggerFactory>());
            };

            var descriptor = new ServiceDescriptor(typeof(IImageProvider), factory, ServiceLifetime.Singleton);
            collection.Add(descriptor);
            return collection;            
        }        
    }
}