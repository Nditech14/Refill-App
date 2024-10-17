using Application.Services;
using Application.Services.Abstraction;
using Application.Services.Implementation;
using Core.Abstraction;
using Core.Entities;
using Core.Implementation;
using Core.Services;
using Infrastructure.Mapping;
using Microsoft.Azure.Cosmos;

namespace Re_Fill_Web_API.Extensions
{
    public static class GeneralExtension
    {
        public static IServiceCollection RegisterApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Register CosmosClient with DI (Singleton)
            services.AddSingleton<CosmosClient>(provider =>
            {
                var cosmosDbConfig = configuration.GetSection("CosmosDb");
                var account = cosmosDbConfig["Account"];
                var key = cosmosDbConfig["Key"];
                return new CosmosClient(account, key);
            });

            // Register ICosmosDbRepository and ICosmosDbService as generic services
            services.AddScoped(typeof(ICosmosDbRepository<>), typeof(CosmosDbRepository<>));
            services.AddScoped(typeof(ICosmosDbService<>), typeof(CosmosDbService<>));

            // If you want to register a specific instance of ICosmosDbService for Inventory
            services.AddScoped<ICosmosDbService<Inventory>>(provider =>
            {
                var cosmosDbRepository = provider.GetRequiredService<ICosmosDbRepository<Inventory>>();
                return new CosmosDbService<Inventory>(cosmosDbRepository);
            });

            services.AddScoped<ICosmosDbService<PurchaseRequest>>(provider =>
            {
                var cosmosDbRepository = provider.GetRequiredService<ICosmosDbRepository<PurchaseRequest>>();
                return new CosmosDbService<PurchaseRequest>(cosmosDbRepository);
            });

            services.AddScoped<ICosmosDbService<RequestItem>>(provider =>
            {
                var cosmosDbRepository = provider.GetRequiredService<ICosmosDbRepository<RequestItem>>();
                return new CosmosDbService<RequestItem>(cosmosDbRepository);
            });

            services.AddScoped<ICosmosDbService<UserDetails>>(provider =>
            {
                var cosmosDbRepository = provider.GetRequiredService<ICosmosDbRepository<UserDetails>>();
                return new CosmosDbService<UserDetails>(cosmosDbRepository);
            });

            // Register other services
            services.AddScoped<IInventoryService, InventoryService>();
            services.AddScoped<IPurchaseRequestService, PurchaseRequestService>();
            services.AddScoped<IFileService<FileEntity>, FileService>();
            services.AddSingleton<IEmailService, EmailService>();
            services.AddScoped<IUserDetailService, UserDetailService>();
            services.AddScoped<IServiceBusProducer, ServiceBusProducer>();
            services.AddHostedService<NotificationWorker>();
            services.AddScoped<IRequestItemService, RequestItemService>();
            services.AddScoped<IEmailTemplateService, EmailTemplateService>();


            // Register AutoMapper profile
            services.AddAutoMapper(typeof(AutoMapperProfile));

            // Add HttpContextAccessor (useful for accessing HttpContext in services)
            services.AddHttpContextAccessor();

            // Add logging services (Console and Debug)
            services.AddLogging(config =>
            {
                config.AddConsole();
                config.AddDebug();
            });

            return services;
        }
    }
}
