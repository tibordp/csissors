using Csissors.Repository;
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
                .AddSingleton<IRepositoryFactory, PostgresRepositoryFactory>();
            return builder;
        }
    }
}