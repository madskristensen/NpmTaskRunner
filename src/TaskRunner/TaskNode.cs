using System.Threading.Tasks;
using Microsoft.VisualStudio.TaskRunnerExplorer;
using NpmTaskRunner.Helpers;

namespace NpmTaskRunner
{
    public class TaskNode : TaskRunnerNode
    {
        private readonly PackageManager _packageManager;

        public TaskNode(string name, bool invokable, PackageManager packageManager) : base(name, invokable)
        {
            _packageManager = packageManager;
        }

        public override Task<ITaskRunnerCommandResult> Invoke(ITaskRunnerCommandContext context)
        {
            // if the CLI is not NPM and the Verbose option is enabled, set the verbose option correctly
            if (_packageManager != PackageManager.NPM && this.Command.Options?.Trim() == PackageManager.NPM.VerboseOption)
            {
                this.Command.Options = this.Command.Options.Replace(PackageManager.NPM.VerboseOption, _packageManager.VerboseOption).Trim();
            }

            return base.Invoke(context);
        }
    }
}
