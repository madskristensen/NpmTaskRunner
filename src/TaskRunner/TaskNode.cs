using System.Threading.Tasks;
using Microsoft.VisualStudio.TaskRunnerExplorer;

namespace NpmTaskRunner
{
    public class TaskNode : TaskRunnerNode
    {
        private bool _isNpm;

        public TaskNode(string name, bool invokable, bool isNpm) : base(name, invokable)
        {
            _isNpm = isNpm;
        }

        public override Task<ITaskRunnerCommandResult> Invoke(ITaskRunnerCommandContext context)
        {
            // if the CLI is Yarn and the Verbose option is enabled, set the verbose option correctly
            if (!_isNpm && this.Command.Options?.Trim() == Constants.NPM_VERBOSE_OPTION)
                this.Command.Options = this.Command.Options.Replace(Constants.NPM_VERBOSE_OPTION, Constants.YARN_VERBOSE_OPTION).Trim();

            return base.Invoke(context);
        }
    }
}
