using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Host.Executors;
using Microsoft.Azure.WebJobs.Host.Listeners;
using Newtonsoft.Json;

namespace MsSqlWebJobExtensions
{
    public class MsSqlTableListener : IListener
    {
        readonly string _configuration;
        readonly ITriggeredFunctionExecutor _triggerExecutor;
        readonly MsSqlTableTriggerAttribute _attribute;
        Timer _timer = null;
        CancellationToken _ct = default;


#if DEBUG
        internal bool __timer_callback_triggered = false;
#endif

        public MsSqlTableListener(string configuration, ITriggeredFunctionExecutor triggerExecutor, MsSqlTableTriggerAttribute attribute)
        {
            _configuration = configuration;
            _triggerExecutor = triggerExecutor;
            _attribute = attribute;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            _ct = cancellationToken;

            _timer = new Timer(new TimerCallback(Timer_Callback), null, _attribute.PollingInterval, _attribute.PollingInterval);

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer.Dispose();
            return Task.CompletedTask;
        }

        public void Cancel()
        {
        }

        public void Dispose()
        {
        }

        private void Timer_Callback(object state)
        {
#if DEBUG
        __timer_callback_triggered = true;
#endif

            if (_ct.IsCancellationRequested)
            {
                _timer.Dispose();
                return;
            }

            _timer.Change(Timeout.Infinite, Timeout.Infinite);

            var timerTask = Timer_CallbackAsync(state);

            timerTask.ContinueWith(new Action<Task>(t =>
            {
                _timer.Change(_attribute.PollingInterval, _attribute.PollingInterval);
            }));
        }

        private Task Timer_CallbackAsync(object state)
        {
            var hasNewData = CheckForRows();
            if (!hasNewData)
                return Task.CompletedTask;

            var msSqlInfo = new MsSqlInfo { /*TestMessage = "Some test message"*/ };//TODO: 
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
                        command.CommandText = $"SELECT COUNT(*) FROM {_attribute.TableName}";
                        var result = (int)command.ExecuteScalar();
                        return result > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                //TODO: Logging here
                return false;
            }
        }
    }
}
