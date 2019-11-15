using Pnnl.Api.Approvals.Data.Interfaces;
using Pnnl.Api.Operations;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Pnnl.Api.Approvals.Data
{
    public class ContextPersonStoreBase : IContextPersonStore
    {
        private const string User = "system-user";

        public Person Get(CancellationToken cancellationToken = default(CancellationToken), IDictionary<object, object> context = null)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            cancellationToken.ThrowIfCancellationRequested();

            if (!context.TryGetValue(User, out object value))
            {
                return null;
            }

            return (Person)value;
        }
    }
}
