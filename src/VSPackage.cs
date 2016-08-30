using System;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.VisualStudio.Shell;

namespace NpmTaskRunner
{
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [InstalledProductRegistration("#110", "#112", Vsix.Version, IconResourceID = 400)]
    [Guid(PackageGuids.guidVSPackageString)]
    [ProvideAutoLoad("{349C5852-65DF-11dA-9384-00065B846F21}", PackageAutoLoadFlags.BackgroundLoad)] // WAP
    [ProvideAutoLoad("{E24C65DC-7377-472b-9ABA-BC803B73C61A}", PackageAutoLoadFlags.BackgroundLoad)] // WebSite
    [ProvideAutoLoad("{8BB2217D-0F2D-49D1-97BC-3654ED321F3B}", PackageAutoLoadFlags.BackgroundLoad)] // ProjectK
    [ProvideAutoLoad("{895DF540-DA8B-49B6-BD64-32FFD1410798}", PackageAutoLoadFlags.BackgroundLoad)] // Cordova
    [ProvideMenuResource("Menus.ctmenu", 1)]
    public sealed class VSPackage : AsyncPackage
    {
        protected override async System.Threading.Tasks.Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            await Logger.InitializeAsync(this, Vsix.Name);
        }
    }
}
