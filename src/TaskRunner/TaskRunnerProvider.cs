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
            ITaskRunnerNode root = new TaskRunnerNode(Vsix.Name);

            var scripts = TaskParser.LoadTasks(configPath);
            var hierarchy = GetHierarchy(scripts.Keys);

            var defaults = hierarchy.Where(h => Constants.ALL_DEFAULT_TASKS.Contains(h.Key));

            TaskRunnerNode defaultTasks = new TaskRunnerNode("Defaults");
            defaultTasks.Description = "Default predefined npm commands.";
            root.Children.Add(defaultTasks);
            AddCommands(configPath, scripts, defaults, defaultTasks);

            if (hierarchy.Count != defaults.Count())
            {
                var customs = hierarchy.Except(defaults);

                TaskRunnerNode customTasks = new TaskRunnerNode("Custom");
                customTasks.Description = "Custom npm commands.";
                root.Children.Add(customTasks);

                AddCommands(configPath, scripts, customs, customTasks);
            }

            return root;
        }

        private void AddCommands(string configPath, SortedList<string, string> scripts, IEnumerable<KeyValuePair<string, IEnumerable<string>>> commands, TaskRunnerNode tasks)
        {
            string cwd = Path.GetDirectoryName(configPath);

            foreach (var parent in commands)
            {
                TaskRunnerNode parentTask = CreateTask(cwd, parent.Key, scripts[parent.Key]);

                foreach (var child in parent.Value)
                {
                    TaskRunnerNode childTask = CreateTask(cwd, child, scripts[child]);
                    parentTask.Children.Add(childTask);
                }

                tasks.Children.Add(parentTask);
            }
        }

        private static TaskRunnerNode CreateTask(string cwd, string name, string cmd)
        {
            return new TaskRunnerNode(name, !string.IsNullOrEmpty(cmd))
            {
                Command = new TaskRunnerCommand(cwd, "cmd.exe", $"/c {cmd} --color=always"),
                Description = $"Runs the '{name}' command",
            };
        }

        private SortedList<string, IEnumerable<string>> GetHierarchy(IEnumerable<string> alltasks)
        {
            if (alltasks == null || !alltasks.Any())
                return null;

            var events = alltasks.Where(t => t.StartsWith(Constants.PRE_SCRIPT_PREFIX) || t.StartsWith(Constants.POST_SCRIPT_PREFIX));
            var parents = alltasks.Except(events).ToList();

            var hierarchy = new SortedList<string, IEnumerable<string>>();

            foreach (var parent in parents)
            {
                var children = GetChildScripts(parent, events).OrderByDescending(child => child);
                hierarchy.Add(parent, children);
                events = events.Except(children).ToList();
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
                if (candidate.StartsWith(Constants.POST_SCRIPT_PREFIX) && Constants.POST_SCRIPT_PREFIX + parent == candidate)
                    yield return candidate;

                if (candidate.StartsWith(Constants.PRE_SCRIPT_PREFIX) && Constants.PRE_SCRIPT_PREFIX + parent == candidate)
                    yield return candidate;
            }
        }
    }
}
