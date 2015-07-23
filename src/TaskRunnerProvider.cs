using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using AlfredTrx.Helpers;
using Microsoft.VisualStudio.TaskRunnerExplorer;

namespace NpmTaskRunner
{
    [TaskRunnerExport("Brocfile.js", "ember-cli-build.js")]
    class TaskRunnerProvider : ITaskRunner
    {
        private ImageSource _icon;

        public TaskRunnerProvider()
        {
            _icon = new BitmapImage(new Uri(@"pack://application:,,,/NpmTaskRunner;component/Resources/npm.png"));
        }

        public List<ITaskRunnerOption> Options
        {
            get { return null; }
        }

        public async Task<ITaskRunnerConfig> ParseConfig(ITaskRunnerCommandContext context, string configPath)
        {
            ITaskRunnerNode hierarchy = LoadHierarchy(configPath);

            if (!hierarchy.Children.Any() && !hierarchy.Children.First().Children.Any())
                return null;

            return await Task.Run(() =>
            {
                return new TaskRunnerConfig<JsonBindingsPersister>(context, hierarchy, _icon);
            });
        }

        private ITaskRunnerNode LoadHierarchy(string configPath)
        {
            ITaskRunnerNode root = new TaskRunnerNode("Broccoli Commands");

            string workingDirectory = Path.GetDirectoryName(configPath);

            Dictionary<string, string> scripts = TaskParser.LoadTasks(configPath);

            if (scripts == null)
                return root;

            TaskRunnerNode tasks = new TaskRunnerNode("Commands");
            tasks.Description = "Broccoli CLI commands.";
            root.Children.Add(tasks);

            foreach (var key in scripts.Keys.OrderBy(k => k))
            {
                TaskRunnerNode task = new TaskRunnerNode(key, true)
                {
                    Command = new TaskRunnerCommand(workingDirectory, "node.exe", scripts[key]),
                    Description = scripts[key],
                };

                tasks.Children.Add(task);
            }

            return root;
        }
    }
}
