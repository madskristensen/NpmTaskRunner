using System.Windows.Media;
using Microsoft.VisualStudio.TaskRunnerExplorer;

namespace NpmTaskRunner.Helpers
{
    public class TaskRunnerConfig : TaskRunnerConfigBase
    {
        private ImageSource _rootNodeIcon;

        public TaskRunnerConfig(ITaskRunnerCommandContext context, ITaskRunnerNode hierarchy)
            : base(context, hierarchy)
        {
        }

        public TaskRunnerConfig(ITaskRunnerCommandContext context, ITaskRunnerNode hierarchy, ImageSource rootNodeIcon)
            : this(context, hierarchy)
        {
            _rootNodeIcon = rootNodeIcon;
        }

        protected override ImageSource LoadRootNodeIcon()
        {
            return _rootNodeIcon;
        }
    }
}
