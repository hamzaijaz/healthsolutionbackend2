using System;

namespace CapitalRaising.RightsIssues.Service.Application.Common.Audit
{
    /// <summary>
    /// Audit Attribute
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class AuditAttribute  : Attribute
    {
        private string entityType;
        private Action action;

        public AuditAttribute(Type entityType, Action action)
        {
            this.entityType = entityType.Name;
            this.action = action;
        }
        public AuditAttribute(string entityType, Action action)
        {
            this.entityType = entityType;
            this.action = action;
        }
        
        /// <summary>
        /// Entity Type
        /// </summary>
        public string EntityType => entityType;
        /// <summary>
        /// Action
        /// </summary>
        public Action Action => action;
    }
}
