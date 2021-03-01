using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Host.Executors;
using Microsoft.Azure.WebJobs.Host.Listeners;
using Newtonsoft.Json;

namespace MsSqlWebJobExtensions
{
    public class MsSqlQueryListener : IListener
    {
        readonly string _connectionString;
        readonly ITriggeredFunctionExecutor _triggerExecutor;
        readonly MsSqlQueryTriggerAttribute _attribute;
        CancellationToken _ct = default(CancellationToken);

        readonly OnChangeEventHandler _onDependencyHandler;

        SqlConnection connection = null;
        SqlCommand command = null;

        public MsSqlQueryListener(string configuration, ITriggeredFunctionExecutor triggerExecutor, MsSqlQueryTriggerAttribute attribute)
        {
            _connectionString = configuration;
            _onDependencyHandler = new OnChangeEventHandler(async (sender, e) => await OnDependency(sender as SqlDependency, e));
            _triggerExecutor = triggerExecutor;
            _attribute = attribute;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            if (connection != null)
                throw new ApplicationException("The previous connection must be stopped before a new can be started!");

            _ct = cancellationToken;
            _ct.ThrowIfCancellationRequested();

            if (!EnoughPermission())
                throw new ApplicationException("User does not have database permission SUBSCRIBE QUERY NOTIFICATIONS.");

            await RegisterSqlDependencyOnServerAsync();
        }

        private bool EnoughPermission()
        {

            SqlClientPermission perm = new SqlClientPermission(System.Security.Permissions.PermissionState.Unrestricted);
            try
            {
                perm.Demand();
                return true;
            }
            catch (System.Exception)
            {
                return false;
            }
        }

        private async Task RegisterSqlDependencyOnServerAsync()
        {
            try
            {
                // Remove any existing dependency connection, then create a new one.
                SqlDependency.Stop(_connectionString);
                SqlDependency.Start(_connectionString);

                if (connection == null)
                    connection = new SqlConnection(_connectionString);

                if (command == null)
                    command = new SqlCommand(_attribute.Query, connection);

                await ReadDataAsync();
            }
            catch (InvalidOperationException ex) when ((ex.HResult & 0xffffffff) == 0x80131509)
            {
                throw new ApplicationException($"The service broker must be enabled. Please execute 'ALTER DATABASE {connection.Database} SET ENABLE_BROKER;'.");
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private async Task ReadDataAsync()
        {
            // Ensure the command object does not have a notification object.
            command.Notification = null;

            // Create and bind the SqlDependency object to the command object.
            SqlDependency dependency = new SqlDependency(command);
            dependency.OnChange += _onDependencyHandler;

            Console.WriteLine("Opening connection");
            await connection.OpenAsync();
            Console.WriteLine("Opened connection");
            //connection.BeginTransaction(IsolationLevel.ReadCommitted).Commit();
            //using (var setCommand = new SqlCommand("SET ARITHABORT ON", connection))
            //    setCommand.ExecuteNonQuery();

            Console.WriteLine("Executing non query: " + command.CommandText);
            await command.ExecuteNonQueryAsync();
            Console.WriteLine("Executed non query: " + command.CommandText);

            connection.Close();
            Console.WriteLine("Closed connection");
        }

        protected async Task OnDependency(SqlDependency dependency, SqlNotificationEventArgs e)
        {
            Console.WriteLine("OnDependency " + e.Info);
            if (e.Info == SqlNotificationInfo.Isolation)
                return;

            dependency.OnChange -= _onDependencyHandler;
            Console.WriteLine("OnDependency removed OnChange listener");

            if (e.Source == SqlNotificationSource.Data && e.Type == SqlNotificationType.Change)
            {
                Console.WriteLine("OnDependency Trigger setup");
                var msSqlInfo = new MsSqlInfo { Info = e.Info };
                var triggerValue = JsonConvert.SerializeObject(msSqlInfo, Constants.JsonSerializerSettings);
                var input = new TriggeredFunctionData { TriggerValue = triggerValue };

                Console.WriteLine("OnDependency Triggering");
                await _triggerExecutor.TryExecuteAsync(input, _ct);
                Console.WriteLine("OnDependency Trigger returned");
            }

            Console.WriteLine("OnDependency reading next data");
            _ = ReadDataAsync();
            Console.WriteLine("OnDependency Completed");
        }

        public void Cancel()
        {
            Console.WriteLine("Cancel()");
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            Console.WriteLine("Dispose()");
            throw new NotImplementedException();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("StopAsync()");
            throw new NotImplementedException();
        }
    }
}
