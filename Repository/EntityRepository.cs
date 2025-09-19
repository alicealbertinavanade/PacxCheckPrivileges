using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace PrivilegesPlugins.Repository
{
    internal class EntityRepository
    {
        private readonly IOrganizationServiceAsync2 organizationService;

        public EntityRepository(IOrganizationServiceAsync2 organizationService)
        {
            this.organizationService = organizationService;
        }

        public Entity? GetEntityByName(string entityName)
        {
            var query = new QueryExpression("entity"); // usa EntityModel.Entity
            query.NoLock = true;
            query.TopCount = 1;
            query.ColumnSet.AddColumns("entityid", "name");
            query.Criteria.AddCondition("name", ConditionOperator.Equal, entityName);

            var retrievedEntity = organizationService.RetrieveMultiple(query).Entities.FirstOrDefault();

            return retrievedEntity; // restituisce direttamente Entity
        }
    }
}