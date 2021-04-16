using System;
using System.Collections.Generic;
using System.Linq;

namespace CapitalRaising.RightsIssues.Service.Application.Common.Exceptions
{
    public class DuplicateItemException : Exception
    {
        public DuplicateItemException(string entity)
            : base($"Duplicate items exist with the same {entity}.")
        {
        }
    }
}