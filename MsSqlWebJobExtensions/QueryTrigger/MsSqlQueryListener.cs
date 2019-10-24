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
        readonly string _configuration;
        readonly ITriggeredFunctionExecutor _triggerExecutor;
        readonly MsSqlQueryTriggerAttribute _attribute;
        CancellationToken _ct = default(CancellationToken);

        SqlConnection connection = null;
        SqlCommand command = null;

        public MsSqlQueryListener(string configuration, ITriggeredFunctionExecutor triggerExecutor, MsSqlQueryTriggerAttribute attribute)
        {
            _configuration = configuration;
            _triggerExecutor = triggerExecutor;
            _attribute = attribute;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            _ct = cancellationToken;


            /*
             * ALTER DATABASE AdvtDB SET ENABLE_BROKER;
             */

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
                var connectionString = _configuration;

                SqlDependency.Stop(connectionString);
                SqlDependency.Start(connectionString);

                if (connection == null)
                    connection = new SqlConnection(connectionString);

                if (command == null)
                    command = new SqlCommand(_attribute.Query, connection);

                await ReadDataAsync();
            }
            catch (Exception ex)
            {
                //TODO: Logging here
            }
        }

        private async Task ReadDataAsync()
        {
            // Ensure the command object does not have a notification object.
            command.Notification = null;

            // Create and bind the SqlDependency object to the command object.
            SqlDependency dependency = new SqlDependency(command);
            dependency.OnChange += new OnChangeEventHandler(new Action<object, SqlNotificationEventArgs>(dependency_OnChange));

            await connection.OpenAsync();
            //connection.BeginTransaction(IsolationLevel.ReadCommitted).Commit();
            //using (var setCommand = new SqlCommand("SET ARITHABORT ON", connection))
            //    setCommand.ExecuteNonQuery();
            await command.ExecuteNonQueryAsync();
            connection.Close();
        }

        delegate void UIDelegate();
        private void dependency_OnChange(object sender, SqlNotificationEventArgs e)
        {
            SqlDependency dependency = (SqlDependency)sender;
            dependency.OnChange -= dependency_OnChange;

            var msSqlInfo = new MsSqlInfo { /*TestMessage = "Some test message"*/ };//TODO: 
            var triggerValue = JsonConvert.SerializeObject(msSqlInfo, Constants.JsonSerializerSettings);

            TriggeredFunctionData input = new TriggeredFunctionData
            {
                TriggerValue = triggerValue
            };

            var task = _triggerExecutor.TryExecuteAsync(input, _ct);

            task.ContinueWith(t => ReadDataAsync()).GetAwaiter();

            var x = 0;
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
