using Csissors.Repository;
using Csissors.Serialization;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Csissors.Postgres
{
    public static class CsissorsBuilderExtensions
    {
        public static CsissorsBuilder AddPostgresRepository(this CsissorsBuilder builder, Action<PostgresOptions> configure)
        {
            builder.Services.AddOptions()
                .Configure(configure)
                .AddSingleton<IConfigurationSerializer, ConfigurationSerializer>()
                .AddSingleton<IRepositoryFactory, PostgresRepositoryFactory>();
            return builder;
        }
    }
}