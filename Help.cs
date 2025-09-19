using Greg.Xrm.Command.Parsing;
using Greg.Xrm.Command.Services;

namespace PrivilegesPlugins
{
    public class Help : INamespaceHelper
    {
        private readonly string help = "Get all user profiles associated with the corresponding role from an entity";

        public string[] Verbs { get; } = new string[1] { "roleuserprofile" };

        public Help() { }

        public string GetHelp()
        {
            return this.help;
        }

        public void WriteHelp(MarkdownWriter writer)
        {
            writer.WriteParagraph(this.GetHelp());
        }
    }
}