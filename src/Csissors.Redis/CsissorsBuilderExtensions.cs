using Csissors.Repository;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Csissors.Redis
{
    public static class CsissorsBuilderExtensions
    {
        public static CsissorsBuilder AddRedisRepository(this CsissorsBuilder builder, Action<RedisOptions> configure)
        {
            builder.Services.AddOptions()
                .Configure(configure)
                .AddSingleton<IRepositoryFactory, RedisRepositoryFactory>();
            return builder;
        }
    }
}