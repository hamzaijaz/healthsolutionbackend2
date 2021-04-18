using System;

namespace MyHealthSolution.Service.Application.Common.Exceptions
{
    public class DuplicateItemException : Exception
    {
        public DuplicateItemException(string entity)
            : base($"Duplicate items exist with the same {entity}.")
        {
        }
    }
}