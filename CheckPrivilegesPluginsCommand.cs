using Greg.Xrm.Command;
using System.ComponentModel.DataAnnotations;

namespace CheckPrivilegesPlugins
{
    [Command("privileges", "get-entity-privileged-roles", HelpText = "Ottiene i ruoli con privilegi sull'entità specificata.")]
    [Alias("priv")]
    public class CheckPrivilegesPluginsCommand
    {
        [Option("entity", "e", HelpText = "Nome dell'entità da analizzare (es. 'lead').")]
        [Required]
        public string? EntityName { get; set; } 
    }
}