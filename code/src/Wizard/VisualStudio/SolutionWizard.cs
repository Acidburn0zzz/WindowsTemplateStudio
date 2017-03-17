﻿using EnvDTE;
using Microsoft.Internal.VisualStudio.PlatformUI;
using Microsoft.Templates.Core;
using Microsoft.Templates.Core.Diagnostics;
using Microsoft.Templates.Core.Extensions;
using Microsoft.Templates.Core.Gen;
using Microsoft.Templates.Core.Locations;
using Microsoft.Templates.Wizard;
using Microsoft.Templates.Wizard.Host;
using Microsoft.Templates.Wizard.Resources;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TemplateWizard;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Microsoft.Templates.Wizard.VisualStudio
{
    public class SolutionWizard : IWizard, IDisposable
    {
        private WizardState _userSelection;
        private GenContext _context;

        public SolutionWizard()
        {
            //TODO: LOCK THIS?
            if (!GenContext.IsInitialized)
            {
#if DEBUG
                GenContext.Bootstrap(new LocalTemplatesSource(), new VsGenShell());
#else
                GenContext.Bootstrap(new RemoteTemplatesSource(), new VsGenShell());
#endif
            }
        }

        public void BeforeOpeningFile(ProjectItem projectItem)
        {
        }

        public void Dispose()
        {
            _context?.Dispose();
        }

        public void ProjectFinishedGenerating(Project project)
        {
        }

        public void ProjectItemFinishedGenerating(ProjectItem projectItem)
        {
        }

        public async void RunFinished()
        {
            AppHealth.Current.Verbose.TrackAsync("Creating UWP Community Templates project...").FireAndForget();
            await GenController.GenerateAsync(_userSelection);
            AppHealth.Current.Verbose.TrackAsync("Generation finished").FireAndForget();

            GenContext.ToolBox.Shell.ShowStatusBarMessage(StringRes.RestoringMessage);
            GenContext.ToolBox.Shell.RestorePackages();

            GenContext.ToolBox.Shell.CollapseSolutionItems();
        }

        public void RunStarted(object automationObject, Dictionary<string, string> replacementsDictionary, WizardRunKind runKind, object[] customParams)
        {
            if (runKind == WizardRunKind.AsNewProject || runKind == WizardRunKind.AsMultiProject)
            {
                _context = GenContext.CreateNew(replacementsDictionary);
                _userSelection = GenController.GetUserSelection(WizardSteps.Project);
            }
        }

        public bool ShouldAddProjectItem(string filePath)
        {
            return true;
        }
    }
}
