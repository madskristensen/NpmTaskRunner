using System;
using System.Windows.Media;
using Microsoft.VisualStudio.TaskRunnerExplorer;

namespace AlfredTrx.Helpers
{
    public abstract class TaskRunnerConfigBase : ITaskRunnerConfig
    {
        private static ImageSource SharedIcon;

        private ITaskRunnerCommandContext _context;

        protected TaskRunnerConfigBase(ITaskRunnerCommandContext context, ITaskRunnerNode hierarchy)
        {
            TaskHierarchy = hierarchy;
            _context = context;
        }

        /// <summary>
        /// TaskRunner icon 
        /// </summary>
        public virtual ImageSource Icon => SharedIcon ?? (SharedIcon = LoadRootNodeIcon());

        public ITaskRunnerNode TaskHierarchy { get; }

        protected abstract IPersistTaskRunnerBindings BindingsPersister { get; }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public string LoadBindings(string configPath)
        {
            return BindingsPersister.Load(configPath);
        }

        public bool SaveBindings(string configPath, string bindingsXml)
        {
            return BindingsPersister.Save(configPath, bindingsXml);
        }

        protected virtual void Dispose(bool isDisposing)
        {
        }

        protected virtual ImageSource LoadRootNodeIcon()
        {
            return null;
        }
    }
}
