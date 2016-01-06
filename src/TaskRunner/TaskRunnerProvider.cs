using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.VisualStudio.TaskRunnerExplorer;
using NpmTaskRunner.Helpers;

namespace NpmTaskRunner
{
    [TaskRunnerExport(Constants.FILENAME)]
    class TaskRunnerProvider : ITaskRunner
    {
        private ImageSource _icon;
        private List<ITaskRunnerOption> _options = null;

        public TaskRunnerProvider()
        {
            _icon = new BitmapImage(new Uri(@"pack://application:,,,/NpmTaskRunner;component/Resources/npm.png"));
        }

        private void InitializeNpmTaskRunnerOptions()
        {
            _options = new List<ITaskRunnerOption>();
            _options.Add(new TaskRunnerOption("Verbose", PackageIds.cmdVerbose, PackageGuids.guidVSPackageCmdSet, false, "-d"));
        }

        public List<ITaskRunnerOption> Options
        {
            get
            {
                if (_options == null)
                {
                    InitializeNpmTaskRunnerOptions();
                }

                return _options;
            }
        }

        public async Task<ITaskRunnerConfig> ParseConfig(ITaskRunnerCommandContext context, string configPath)
        {
            ITaskRunnerNode hierarchy = LoadHierarchy(configPath);

            if (!hierarchy.Children.Any() || !hierarchy.Children.First().Children.Any())
                return null;

            return await Task.Run(() =>
            {
                return new TaskRunnerConfig(context, hierarchy, _icon);
            });
        }

        private ITaskRunnerNode LoadHierarchy(string configPath)
        {
            ITaskRunnerNode root = new TaskRunnerNode(Constants.TASK_CATEGORY);

            string workingDirectory = Path.GetDirectoryName(configPath);

            var scripts = TaskParser.LoadTasks(configPath);

            Telemetry.TrackEvent("Tasks loaded");

            if (scripts == null)
                return root;

            TaskRunnerNode tasks = new TaskRunnerNode("Scripts");
            tasks.Description = "Scripts specified in the \"scripts\" JSON element.";
            root.Children.Add(tasks);

            foreach (string script in scripts)
            {
                TaskRunnerNode task = new TaskRunnerNode(script, true)
                {
                    Command = new TaskRunnerCommand(workingDirectory, "cmd.exe", $"/c npm run {script} --color=always"),
                    Description = $"Runs the '{script}' script",
                };

                tasks.Children.Add(task);
            }

            return root;
        }
    }
}
