using System.Diagnostics.CodeAnalysis;
using System.Reactive;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Steeltoe.CircuitBreaker.Hystrix;

namespace DHaven.Faux.HttpSupport
{
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public class HystrixTask : HystrixCommand
    {
        private readonly Task run;
        private readonly Task fallback;
        
        public HystrixTask(IHystrixCommandOptions options, Task run, Task fallback, ILogger logger)
            : base(options, null, null, logger)
        {
            this.run = run;
            this.fallback = fallback;
        }

        protected override async Task<Unit> RunAsync()
        {
            await run;
            return Unit.Default;
        }

        protected override async Task<Unit> RunFallbackAsync()
        {
            await fallback;
            return Unit.Default;
        }
    }
}