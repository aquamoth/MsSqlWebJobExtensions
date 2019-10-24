using System;

namespace MsSqlWebJobExtensions
{
    public sealed class MsSqlQueryTriggerAttribute : Attribute
    {
        public string Query { get; private set; }

        public MsSqlQueryTriggerAttribute(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                throw new ArgumentException(nameof(query));

            this.Query = query;
        }
    }
}
