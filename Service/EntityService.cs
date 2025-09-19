using Greg.Xrm.Command.Services.Connection;
using Greg.Xrm.Command.Services.Output;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk; // aggiunto per Entity
using PrivilegesPlugins.Repository;
using CheckPrivilegesPlugins; // aggiunto per CheckPrivilegesPluginsCommand

namespace PrivilegesPlugins.Service
{
    internal class EntityCheckExist
    {
        public bool Exists { get; set; } = true;
        public string ErrorMessage { get; set; } = string.Empty;
        public Entity? Entity { get; set; } // aggiunto '?'
    }

    internal class EntityService
    {
        private readonly CheckPrivilegesPluginsCommand command;
        private readonly IOrganizationServiceRepository organizationServiceRepository;
        private readonly EntityRepository entityRepository;
        private readonly IOutput output;

        public EntityService(CheckPrivilegesPluginsCommand command, IOrganizationServiceAsync2 organizationService, IOutput output, IOrganizationServiceRepository organizationServiceRepository)
        {
            this.command = command;
            this.entityRepository = new EntityRepository(organizationService); // corretto nome
            this.output = output;
            this.organizationServiceRepository = organizationServiceRepository;
        }

        public EntityCheckExist CheckIfEntityExists()
        {
            EntityCheckExist entityExistsResponse = new EntityCheckExist();
            entityExistsResponse.Entity = null; // già corretto

            var errorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(command.EntityName))
            {
                output.WriteLine($"Entity name parameter not given, use the default entity 'systemuser'.", ConsoleColor.Yellow);
                command.EntityName = "systemuser";
            }

            if (string.IsNullOrWhiteSpace(command.EntityName))
            {
                entityExistsResponse.ErrorMessage = "Cannot get current default entity, exiting...";
                entityExistsResponse.Exists = false;
                return entityExistsResponse;
            }

            var entity = entityRepository.GetEntityByName(command.EntityName);

            if (entity is null)
            {
                entityExistsResponse.ErrorMessage = $"Cannot find an entity with name <{command.EntityName}>";
                entityExistsResponse.Exists = false;
                return entityExistsResponse;
            }

            entityExistsResponse.Entity = entity;
            output.WriteLine(
                $"Entity <{entity.GetAttributeValue<string>("name")}> found with Id {entity.GetAttributeValue<Guid>("entityid")}",

                ConsoleColor.Green);

            return entityExistsResponse;
        }
    }
}