using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.VisualStudio.TaskRunnerExplorer;

namespace NpmTaskRunner.Helpers
{
    public class TaskRunnerConfig : TaskRunnerConfigBase
    {
        private string _cliCommandName;

        public TaskRunnerConfig(ITaskRunnerCommandContext context, ITaskRunnerNode hierarchy)
            : base(context, hierarchy)
        {
        }

        public TaskRunnerConfig(ITaskRunnerCommandContext context, ITaskRunnerNode hierarchy, string cliCommandName)
            : this(context, hierarchy)
        {
            _cliCommandName = cliCommandName;
        }

        protected override ImageSource LoadRootNodeIcon()
        {
            return new BitmapImage(new Uri($@"pack://application:,,,/NpmTaskRunner;component/Resources/{_cliCommandName}.png")); ;
        }
    }
}
