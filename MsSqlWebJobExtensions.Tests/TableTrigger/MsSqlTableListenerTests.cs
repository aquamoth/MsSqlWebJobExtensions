using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Host.Listeners;
using Xunit;

namespace MsSqlWebJobExtensions.Tests.TableTrigger
{
    public partial class MsSqlTableListenerTests
    {
        const string IRRELEVANT = "IRRELEVANT";
        const int POLLING_INTERVAL = 10;

        [Fact]
        public async Task Starts_stops_and_disposes()
        {
            var attribute = new MsSqlTableTriggerAttribute(IRRELEVANT, POLLING_INTERVAL);
            var executor = new Mocks.MockExecutor(true);
            var cts = new CancellationTokenSource();

            var sut = new MsSqlTableListener(IRRELEVANT, executor, attribute);
            await sut.StartAsync(cts.Token);
            await Task.Delay(POLLING_INTERVAL + 5);
            await sut.StopAsync(cts.Token);

            Assert.True(sut.__timer_callback_triggered);

            sut.Dispose();
        }

        [Fact]
        public async Task Stop_disables_timer()
        {
            var attribute = new MsSqlTableTriggerAttribute(IRRELEVANT, POLLING_INTERVAL);
            var executor = new Mocks.MockExecutor(true);
            var cts = new CancellationTokenSource();

            var sut = new MsSqlTableListener(IRRELEVANT, executor, attribute);
            await sut.StartAsync(cts.Token);
            await sut.StopAsync(cts.Token);

            Assert.False(sut.__timer_callback_triggered);
            await Task.Delay(POLLING_INTERVAL + 5);
            Assert.False(sut.__timer_callback_triggered);

            sut.Dispose();
        }

        [Fact]
        public async Task Cancel_does_no_harm()
        {
            var attribute = new MsSqlTableTriggerAttribute(IRRELEVANT);
            var executor = new Mocks.MockExecutor(true);

            var sut = new MsSqlTableListener(IRRELEVANT, executor, attribute);
            sut.Cancel();
            sut.Dispose();
        }
    }
}
