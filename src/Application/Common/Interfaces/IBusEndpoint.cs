﻿using System.Threading.Tasks;

namespace CapitalRaising.RightsIssues.Service.Application.Common.Interfaces
{
    public interface IBusEndpoint 
    {
        /// <summary>
        /// Send command
        /// </summary>
        /// <typeparam name="TPayload"></typeparam>
        Task SendAsync<TPayload>(TPayload payload) where TPayload: class;
    }
}