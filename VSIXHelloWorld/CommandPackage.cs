using System;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.Win32;


using Task = System.Threading.Tasks.Task;

namespace VSIXHelloWorld
{
    //SolutionExistsAndFullyLoadedContext
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the
    /// IVsPackage interface and uses the registration attributes defined in the framework to
    /// register itself and its components with the shell. These attributes tell the pkgdef creation
    /// utility what data to put into .pkgdef file.
    /// </para>
    /// <para>
    /// To get loaded into VS, the package must be referred by &lt;Asset Type="Microsoft.VisualStudio.VsPackage" ...&gt; in .vsixmanifest file.
    /// </para>
    /// </remarks>
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)] // Info on this package for Help/About
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [Guid(CommandPackage.PackageGuidString)]
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "pkgdef, VS and vsixmanifest are valid VS terms")]
    [ProvideAutoLoad(UIContextGuids80.SolutionExists)]
    public sealed class CommandPackage : AsyncPackage, IVsSolutionEvents
    {
        /// <summary>
        /// CommandPackage GUID string.
        /// </summary>



        public const string PackageGuidString = "2ba708bc-05dd-4bad-81af-a87bed483a7d";
        private DTE _dte;
        private uint _hSolutionEvents = uint.MaxValue;
        private IVsSolution _solution;
        SolutionBuild sb;
        SolutionConfiguration sc;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="Command"/> class.
        /// </summary>
        public CommandPackage()
        {

            // Show a message box to prove we were here

            // Inside this method you can place any initialization code that does not require
            // any Visual Studio service because at this point the package object is created but
            // not sited yet inside Visual Studio environment. The place to do all the other
            // initialization is the Initialize method.
        }

        #region Package Members

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token to monitor for initialization cancellation, which can occur when VS is shutting down.</param>
        /// <param name="progress">A provider for progress updates.</param>
        /// <returns>A task representing the async work of package initialization, or an already completed task if there is none. Do not return null from this method.</returns>
        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            _dte = (DTE)GetService(typeof(DTE));
            sb = _dte.Solution.SolutionBuild;
            sc = sb.ActiveConfiguration;

            // When initialized asynchronously, the current thread may be a background thread at this point.
            // Do any initialization that requires the UI thread after switching to the UI thread.

            await this.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
            await Command.InitializeAsync(this);
            AdviseSolutionEvents();
        }

        protected override void Dispose(bool disposing)
        {
            UnadviseSolutionEvents();

            base.Dispose(disposing);
        }
        private void AdviseSolutionEvents()
        {
            UnadviseSolutionEvents();

            _solution = GetService(typeof(SVsSolution)) as IVsSolution;

            _solution?.AdviseSolutionEvents(this, out _hSolutionEvents);
        }

        private void UnadviseSolutionEvents()
        {
            if (_solution == null) return;
            if (_hSolutionEvents != uint.MaxValue)
            {
                _solution.UnadviseSolutionEvents(_hSolutionEvents);
                _hSolutionEvents = uint.MaxValue;
            }

            _solution = null;
        }

        public int OnAfterOpenProject(IVsHierarchy pHierarchy, int fAdded)
        {

            
            return VSConstants.S_OK;

        }

        public int OnQueryCloseProject(IVsHierarchy pHierarchy, int fRemoving, ref int pfCancel)
        {
            System.Windows.Forms.MessageBox.Show("Query Close Project");
            return VSConstants.S_OK;
            throw new NotImplementedException();
        }

        public int OnBeforeCloseProject(IVsHierarchy pHierarchy, int fRemoved)
        {
            System.Windows.Forms.MessageBox.Show("Before Close Project");
            return VSConstants.S_OK;
            throw new NotImplementedException();
        }

        public int OnAfterLoadProject(IVsHierarchy pStubHierarchy, IVsHierarchy pRealHierarchy)
        {
            System.Windows.Forms.MessageBox.Show("After load project ");
            return VSConstants.S_OK;

        }

        public int OnQueryUnloadProject(IVsHierarchy pRealHierarchy, ref int pfCancel)
        {
            System.Windows.Forms.MessageBox.Show("Query unload Project");
            return VSConstants.S_OK;
            throw new NotImplementedException();
        }

        public int OnBeforeUnloadProject(IVsHierarchy pRealHierarchy, IVsHierarchy pStubHierarchy)
        {
            System.Windows.Forms.MessageBox.Show("before unload Project");
            return VSConstants.S_OK;

        }

        public int OnAfterOpenSolution(object pUnkReserved, int fNewSolution)
        {
            
            
            var msg = System.IO.Path.GetDirectoryName(_dte.Solution.FullName);
            System.Windows.Forms.MessageBox.Show("solution opened : " +msg);
            LiveXaml xml = new LiveXaml();
            xml.Main(msg);
            return VSConstants.S_OK;

        }


        public int OnQueryCloseSolution(object pUnkReserved, ref int pfCancel)
        {
            System.Windows.Forms.MessageBox.Show("query close  Solution");
            return VSConstants.S_OK;

        }

        public int OnBeforeCloseSolution(object pUnkReserved)
        {
            return VSConstants.S_OK;
            throw new NotImplementedException();
        }

        public int OnAfterCloseSolution(object pUnkReserved)
        {
            return VSConstants.S_OK;
            throw new NotImplementedException();
        }

        #endregion
     
       
}
}
