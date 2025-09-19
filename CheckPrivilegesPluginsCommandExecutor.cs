using Greg.Xrm.Command;
using Greg.Xrm.Command.Services.Connection;
using Greg.Xrm.Command.Services.Output;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System.Text;

namespace CheckPrivilegesPlugins
{
    public class CheckPrivilegesPluginsCommandExecutor : ICommandExecutor<CheckPrivilegesPluginsCommand>
    {
        private readonly IOutput output;
        private readonly IOrganizationServiceRepository organizationServiceRepository;

        public CheckPrivilegesPluginsCommandExecutor(
            IOutput output,
            IOrganizationServiceRepository organizationServiceRepository)
        {
            this.output = output;
            this.organizationServiceRepository = organizationServiceRepository;
        }

        public async Task<CommandResult> ExecuteAsync(CheckPrivilegesPluginsCommand command, CancellationToken cancellationToken)
        {
            var entityName = command.EntityName?.Trim();
            if (string.IsNullOrEmpty(entityName))
            {
                return CommandResult.Fail("Entity name is required. Use --entity [name]");
            }

            this.output.Write($"Connecting to the current dataverse environment...");
            var crm = await this.organizationServiceRepository.GetCurrentConnectionAsync();
            this.output.WriteLine("Done", ConsoleColor.Green);

            // Step 1: Recupera i privilegi disponibili per l'entità
            output.WriteLine($"Recupero privilegi per l'entità '{entityName}'...");
            var entityDefQuery = new QueryExpression("entitydefinition")
            {
                ColumnSet = new ColumnSet("privileges"),
                Criteria = new FilterExpression
                {
                    Conditions =
                    {
                        new ConditionExpression("logicalname", ConditionOperator.Equal, entityName)
                    }
                }
            };
            entityDefQuery.TopCount = 1;
            var entityDefResult = crm.RetrieveMultiple(entityDefQuery);
            if (entityDefResult.Entities.Count == 0)
                return CommandResult.Fail($"Entity '{entityName}' not found.");

            var privileges = entityDefResult.Entities[0].GetAttributeValue<EntityCollection>("privileges")?.Entities;
            if (privileges == null || privileges.Count == 0)
                return CommandResult.Fail($"No privileges found for entity '{entityName}'.");

            // Step 2: Interroga RolePrivileges per ciascun PrivilegeId
            output.WriteLine("Recupero ruoli con privilegi...");
            var rolePrivileges = new Dictionary<Guid, List<Entity>>();
            foreach (var privilege in privileges)
            {
                var privilegeId = privilege.GetAttributeValue<Guid>("privilegeid");
                var rolePrivQuery = new QueryExpression("roleprivileges")
                {
                    ColumnSet = new ColumnSet("roleid"),
                    Criteria = new FilterExpression
                    {
                        Conditions =
                        {
                            new ConditionExpression("privilegeid", ConditionOperator.Equal, privilegeId)
                        }
                    }
                };
                var rolePrivResult = crm.RetrieveMultiple(rolePrivQuery);
                foreach (var rp in rolePrivResult.Entities)
                {
                    var roleId = rp.GetAttributeValue<Guid>("roleid");
                    if (!rolePrivileges.ContainsKey(roleId))
                        rolePrivileges[roleId] = new List<Entity>();
                    rolePrivileges[roleId].Add(privilege);
                }
            }

            // Step 3: Recupera i nomi dei ruoli
            output.WriteLine("Recupero nomi dei ruoli...");
            var roleNames = new Dictionary<Guid, string>();
            if (rolePrivileges.Count == 0)
            {
                output.WriteLine("Nessun ruolo trovato con privilegi su questa entità.", ConsoleColor.Yellow);
                return CommandResult.Success();
            }
            var roleIds = rolePrivileges.Keys.ToList();
            var roleQuery = new QueryExpression("role")
            {
                ColumnSet = new ColumnSet("roleid", "name"),
                Criteria = new FilterExpression
                {
                    Conditions =
                    {
                        new ConditionExpression("roleid", ConditionOperator.In, roleIds.ToArray())
                    }
                }
            };
            var roleResult = crm.RetrieveMultiple(roleQuery);
            foreach (var role in roleResult.Entities)
            {
                var roleId = role.GetAttributeValue<Guid>("roleid");
                var name = role.GetAttributeValue<string>("name");
                roleNames[roleId] = name;
            }

            // Step 4: Recupera gli egl_userprofile associati ai ruoli tramite egl_roles
            output.WriteLine("Recupero userprofile associati ai ruoli...");
            var userProfilesByRole = new Dictionary<string, List<string>>();
            var userProfileQuery = new QueryExpression("egl_userprofile")
            {
                ColumnSet = new ColumnSet("egl_name", "egl_roles")
            };
            var userProfiles = crm.RetrieveMultiple(userProfileQuery).Entities;
            foreach (var roleId in rolePrivileges.Keys)
            {
                var roleName = roleNames.ContainsKey(roleId) ? roleNames[roleId] : roleId.ToString();
                var profiles = userProfiles
                    .Where(up => (up.GetAttributeValue<string>("egl_roles") ?? "")
                        .Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
                        .Any(r => r.Trim().Equals(roleName, StringComparison.OrdinalIgnoreCase)))
                    .Select(up => up.GetAttributeValue<string>("egl_name"))
                    .Where(n => !string.IsNullOrEmpty(n))
                    .ToList();
                userProfilesByRole[roleName] = profiles;
            }

            // Output tabella finale
            output.WriteLine("Ruoli, privilegi e userprofile associati:");
            var sb = new StringBuilder();
            sb.AppendLine("| Role | Privileges | UserProfiles |");
            sb.AppendLine("|------|------------|--------------|");
            foreach (var kvp in rolePrivileges)
            {
                var roleId = kvp.Key;
                var roleName = roleNames.ContainsKey(roleId) ? roleNames[roleId] : roleId.ToString();
                var privs = kvp.Value.Select(priv =>
                {
                    var privName = priv.GetAttributeValue<string>("name");
                    var accessRight = priv.GetAttributeValue<OptionSetValue>("accessright")?.Value.ToString() ?? "";
                    return $"{privName} {accessRight}".Trim();
                });
                var profiles = userProfilesByRole.ContainsKey(roleName) ? string.Join(", ", userProfilesByRole[roleName]) : "";
                sb.AppendLine($"| {roleName} | {string.Join("; ", privs)} | {profiles} |");
            }
            output.WriteLine(sb.ToString());

            return CommandResult.Success();
        }
    }
}