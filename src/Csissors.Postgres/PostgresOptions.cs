using Microsoft.Extensions.Options;

namespace Csissors.Postgres
{
    public class PostgresOptions : IOptions<PostgresOptions>
    {
        public string ConnectionString { get; set; }
        public string TableName { get; set; }
        PostgresOptions IOptions<PostgresOptions>.Value => this;
    }
}