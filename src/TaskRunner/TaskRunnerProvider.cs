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

            var scripts = TaskParser.LoadTasks(configPath);
            var hierarchy = GetHierarchy(scripts);

            if (hierarchy == null)
                return root;

            TaskRunnerNode tasks = new TaskRunnerNode("Scripts");
            tasks.Description = "Scripts specified in the \"scripts\" JSON element.";
            root.Children.Add(tasks);

            string cwd = Path.GetDirectoryName(configPath);

            foreach (var parent in hierarchy.Keys)
            {
                TaskRunnerNode parentTask = CreateTask(cwd, parent);

                foreach (var child in hierarchy[parent])
                {
                    TaskRunnerNode childTask = CreateTask(cwd, child);
                    parentTask.Children.Add(childTask);
                }

                tasks.Children.Add(parentTask);
            }

            Telemetry.TrackEvent("Tasks loaded");

            return root;
        }

        private static TaskRunnerNode CreateTask(string cwd, string name)
        {
            return new TaskRunnerNode(name, true)
            {
                Command = new TaskRunnerCommand(cwd, "cmd.exe", $"/c npm run {name} --color=always"),
                Description = $"Runs the '{name}' script",
            };
        }

        private SortedList<string, IEnumerable<string>> GetHierarchy(IEnumerable<string> alltasks)
        {
            if (alltasks == null)
                return null;

            var events = alltasks.Where(t => t.StartsWith("pre") || t.StartsWith("post"));
            var parents = alltasks.Except(events);

            var hierarchy = new SortedList<string, IEnumerable<string>>();

            foreach (var parent in parents)
            {
                var children = GetChildScripts(parent, events);
                hierarchy.Add(parent, children);
                events = events.Except(children);
            }

            foreach (var child in events)
            {
                hierarchy.Add(child, Enumerable.Empty<string>());
            }

            return hierarchy;
        }

        private static IEnumerable<string> GetChildScripts(string parent, IEnumerable<string> events)
        {
            var candidates = events.Where(e => e.EndsWith(parent, StringComparison.OrdinalIgnoreCase));

            foreach (var candidate in candidates)
            {
                if (candidate.StartsWith("post") && "post" + parent == candidate)
                    yield return candidate;

                if (candidate.StartsWith("pre") && "pre" + parent == candidate)
                    yield return candidate;
            }
        }
    }
}
