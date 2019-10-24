using System;

namespace MsSqlWebJobExtensions
{
    public sealed class MsSqlTableTriggerAttribute : Attribute
    {
        public string TableName { get; private set; }
        public int PollingInterval { get; private set; }

        public MsSqlTableTriggerAttribute(string tableName, int pollingInterval = 5000)
        {
            if (string.IsNullOrWhiteSpace(tableName))
                throw new ArgumentException(nameof(tableName));

            if (pollingInterval < 0)
                throw new ArgumentException(nameof(pollingInterval));

            this.TableName = tableName;
            this.PollingInterval = pollingInterval;
        }
    }
}
