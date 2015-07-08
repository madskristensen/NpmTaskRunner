using System.Windows.Media;
using Microsoft.VisualStudio.TaskRunnerExplorer;

namespace AlfredTrx.Helpers
{
    public class TaskRunnerConfig<TBindingsPersister> : TaskRunnerConfigBase
        where TBindingsPersister : class, IPersistTaskRunnerBindings, new()
    {
        private static TBindingsPersister SharedBindingsPersister = new TBindingsPersister();
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

        protected override IPersistTaskRunnerBindings BindingsPersister => SharedBindingsPersister;

        protected override ImageSource LoadRootNodeIcon()
        {
            return _rootNodeIcon;
        }
    }

    public class TaskRunnerConfig : TaskRunnerConfigBase
    {
        private ImageSource _rootNodeIcon;

        public TaskRunnerConfig(ITaskRunnerCommandContext context, ITaskRunnerNode hierarchy, IPersistTaskRunnerBindings persister)
            : base(context, hierarchy)
        {
            BindingsPersister = persister;
        }

        public TaskRunnerConfig(ITaskRunnerCommandContext context, ITaskRunnerNode hierarchy, IPersistTaskRunnerBindings persister, ImageSource rootNodeIcon)
            : this(context, hierarchy, persister)
        {
            _rootNodeIcon = rootNodeIcon;
        }

        protected override IPersistTaskRunnerBindings BindingsPersister { get; }

        protected override ImageSource LoadRootNodeIcon()
        {
            return _rootNodeIcon;
        }
    }
}
