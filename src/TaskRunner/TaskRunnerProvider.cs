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
            var hierarchy = GetHierarchy(scripts.Keys);

            if (hierarchy == null)
                return root;

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

                //if (parent.Key == "uninstall")
                //    AddDependencies(parentTask, configPath, cwd);

                foreach (var child in parent.Value)
                {
                    TaskRunnerNode childTask = CreateTask(cwd, child, scripts[child]);
                    parentTask.Children.Add(childTask);
                }

                tasks.Children.Add(parentTask);
            }
        }

        //private void AddDependencies(TaskRunnerNode uninstallNode, string configPath, string cwd)
        //{
        //    var tasks = TaskParser.LoadDependencies(configPath);
        //    string allPackages = string.Join(" ", tasks.Values.SelectMany(t => t));

        //    uninstallNode.Command.Args = $"/c npm uninstall {allPackages} --color=always";

        //    foreach (var dep in tasks.Keys)
        //    {
        //        var depPackages = string.Join(" ", tasks[dep]);
        //        var depNode = CreateTask(cwd, dep, $"npm uninstall {depPackages}");

        //        foreach (var package in tasks[dep])
        //        {
        //            var child = CreateTask(cwd, $"uninstall {package}", $"npm uninstall {package}");
        //            depNode.Children.Add(child);
        //        }

        //        uninstallNode.Children.Add(depNode);
        //    }
        //}

        private static TaskRunnerNode CreateTask(string cwd, string name, string cmd)
        {
            bool isReserved = Constants.ALWAYS_TASKS.Contains(name, StringComparer.OrdinalIgnoreCase);

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

            var events = alltasks.Where(t => t.StartsWith("pre") || t.StartsWith("post"));
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
                if (candidate.StartsWith("post") && "post" + parent == candidate)
                    yield return candidate;

                if (candidate.StartsWith("pre") && "pre" + parent == candidate)
                    yield return candidate;
            }
        }
    }
}
