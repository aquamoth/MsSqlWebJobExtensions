using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Host.Executors;

namespace MsSqlWebJobExtensions.Tests.Mocks
{
    public class MockExecutor : ITriggeredFunctionExecutor
    {
        bool _succeed;
        public MockExecutor(bool succeed)
        {
            _succeed = succeed;
        }

        public Task<FunctionResult> TryExecuteAsync(TriggeredFunctionData input, CancellationToken cancellationToken)
        {
            return Task.FromResult(new FunctionResult(_succeed));
        }
    }
}
