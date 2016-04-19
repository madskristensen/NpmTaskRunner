using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace NpmTaskRunner
{
    [PackageRegistration(UseManagedResourcesOnly = true)]
    [InstalledProductRegistration("#110", "#112", Vsix.Version, IconResourceID = 400)]
    [Guid(PackageGuids.guidVSPackageString)]
    [ProvideAutoLoad(UIContextGuids80.SolutionExists)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    public sealed class VSPackage : Package
    {
        protected override void Initialize()
        {
            Logger.Initialize(this, Vsix.Name, Vsix.Version, "27e387d5-9428-4617-b79f-bcc80d4247b0");
            base.Initialize();
        }
    }
}
