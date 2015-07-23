## Broccoli and Ember-CLI Task Runner extension

Adds support for package.json's NPM scripts in Visual Studio 2015's
Task Runner Explorer.

### Execute scripts

When scripts are specified, the Task Runner Explorer
will show those scripts.

![Task list](art/task-list.png)

Each script can be executed by double-clicking the task.

### Bindings

Script bindings make it possible to associate individual scripts
with Visual Studio events such as "After build" etc.

![Visual Studio bindings](art/bindings.png)