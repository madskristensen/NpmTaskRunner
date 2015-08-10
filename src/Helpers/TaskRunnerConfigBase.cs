using System;
using System.Windows.Media;
using Microsoft.VisualStudio.TaskRunnerExplorer;

namespace NpmTaskRunner.Helpers
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
        
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public string LoadBindings(string configPath)
        {
            try
            {
                return BindingsPersister.Load(configPath);
            }
            catch
            {
                return "<binding />";
            }
        }

        public bool SaveBindings(string configPath, string bindingsXml)
        {
            try
            {
                return BindingsPersister.Save(configPath, bindingsXml);
            }
            catch
            {
                return false;
            }
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
