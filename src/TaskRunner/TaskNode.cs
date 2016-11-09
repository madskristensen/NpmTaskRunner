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
            // if the CLI is Yarn and the Verbose option is enabled, strip off the Verbose option prior to executing
            if (!_isNpm)
                this.Command.Options = this.Command.Options?.Replace(Constants.NPM_VERBOSE_OPTION, string.Empty).Trim();

            return base.Invoke(context);
        }
    }
}
