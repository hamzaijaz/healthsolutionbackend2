using System;

namespace MyHealthSolution.Service.Application.Common.Exceptions
{
    public class NotFoundException : Exception
    {
        public NotFoundException(string name, object key)
            : base($"{name} ({key}) was not found.")
        {
        }
    }
}
