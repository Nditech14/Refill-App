using Core.Entities;
using Infrastructure.ADLS.Abstraction;
using Infrastructure.ADLS.configuration;
using Infrastructure.ADLS.Implementation;

namespace Re_Fill_Web_API.Extensions.ADLSExtension
{
    public static class DataLakeServiceExtensions
    {
        public static void AddDataLakeServices(this IServiceCollection services, IConfiguration configuration)
        {
            var dataLakeSettings = configuration.GetSection("StorageSettings").Get<StorageSettings>();

            services.AddSingleton<IFileRepository<FileEntity>>(provider =>
                new AzureDataLakeFileRepository(dataLakeSettings.ConnectionString, dataLakeSettings.FileSystemName));


        }
    }
}
