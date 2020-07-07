using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace Csissors.Redis
{
    public class RedisOptions : IOptions<RedisOptions>
    {
        /// <summary>
        /// The configuration used to connect to Redis.
        /// </summary>
        public ConfigurationOptions ConfigurationOptions { get; set; }

        public string KeyPrefix { get; set; } = "";

        RedisOptions IOptions<RedisOptions>.Value => this;
    }
}