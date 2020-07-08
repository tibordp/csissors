using Microsoft.Extensions.Options;
using System;

namespace Csissors
{
    public class CsissorsOptions : IOptions<CsissorsOptions>
    {
        public TimeSpan PollInterval { get; set; } = TimeSpan.FromSeconds(1);
        public TimeSpan DefaultLeaseDuration { get; set; } = TimeSpan.FromSeconds(60);
        public int MaxExecutionSlots { get; set; } = 100;
        public CsissorsOptions Value => this;
    }
}