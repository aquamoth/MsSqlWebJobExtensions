using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Host.Executors;
using Microsoft.Azure.WebJobs.Host.Listeners;
using Newtonsoft.Json;

namespace MsSqlWebJobExtensions
{
    public class MsSqlListener : IListener
    {
        readonly string _configuration;
        readonly ITriggeredFunctionExecutor _triggerExecutor;
        readonly MsSqlTriggerAttribute _attribute;
        Timer _timer = null;
        CancellationToken _ct = default(CancellationToken);

        public MsSqlListener(string configuration, ITriggeredFunctionExecutor triggerExecutor, MsSqlTriggerAttribute attribute)
        {
            _configuration = configuration;
            _triggerExecutor = triggerExecutor;
            _attribute = attribute;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            _ct = cancellationToken;

            _timer = new Timer(new TimerCallback(Timer_Callback), null, 1000, 5000);

            return Task.CompletedTask;
        }

        private void Timer_Callback(object state)
        {
            if (_ct.IsCancellationRequested)
            {
                _timer.Dispose();
                return;
            }

            _timer.Change(Timeout.Infinite, Timeout.Infinite);

            Timer_CallbackAsync(state)
                .ContinueWith(new Action<Task>(t =>
                {
                    _timer.Change(1000, 5000);
                }));
        }

        private Task Timer_CallbackAsync(object state)
        {
            var hasNewData = CheckForRows();
            if (!hasNewData)
                return Task.CompletedTask;

            var msSqlInfo = new MsSqlInfo { TestMessage = "Some test message" };
            var triggerValue = JsonConvert.SerializeObject(msSqlInfo, Constants.JsonSerializerSettings);

            TriggeredFunctionData input = new TriggeredFunctionData
            {
                TriggerValue = triggerValue
            };

            var task = _triggerExecutor.TryExecuteAsync(input, _ct);

            return task;
        }

        private bool CheckForRows()
        {
            try
            {
                using (var connection = System.Data.SqlClient.SqlClientFactory.Instance.CreateConnection())
                {
                    connection.ConnectionString = _configuration;
                    connection.Open();

                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "SELECT COUNT(*) FROM UserHooks_References";
                        object result = command.ExecuteScalar();
                        return (int)result > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                //TODO: Logging here
                return false;
            }
        }

        public void Cancel()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
