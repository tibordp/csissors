using Microsoft.Extensions.Options;
using System;

namespace Csissors
{
    public class CsissorsOptions : IOptions<CsissorsOptions>
    {
        public TimeSpan PollInterval { get; set; } = TimeSpan.FromSeconds(1);
        public CsissorsOptions Value => this;
    }
}