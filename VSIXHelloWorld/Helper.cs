using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextManager.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.VisualStudio.VSConstants;
using Microsoft.VisualStudio.TextManager;

namespace VSIXHelloWorld
{
    public static class Helper
    {
        public static string main()
        {
            IVsTextManager txtMgr = (IVsTextManager)Microsoft.VisualStudio.Shell.Package.GetGlobalService(typeof(SVsTextManager));
            IVsTextView vTextView = null;
            int mustHaveFocus = 1;
            txtMgr.GetActiveView(mustHaveFocus, null, out vTextView);
            return GetFullPath(vTextView);
            

            // IVsTextManager.GetUserPreferences(VIEWPREFERENCES[]pViewPrefs, FRAMEPREFERENCES[], LANGPREFERENCES[], FONTCOLORPREFERENCES[]);
            //return GetFullPath(GetActiveView());
        }
        
        public static string GetFullPath(this IVsTextView textView)
        {
            ErrorHandler.ThrowOnFailure(textView.GetBuffer(out IVsTextLines buffer));
            var userData = buffer as IVsUserData;
            ErrorHandler.ThrowOnFailure(userData.GetData(typeof(IVsUserData).GUID, out object data));
            return data as string;
        }
        public static IVsWindowFrame GetDocumentFrame(IVsMonitorSelection monitorSelection)
        {
            ErrorHandler.Succeeded(monitorSelection.GetCurrentElementValue((uint)VSSELELEMID.SEID_DocumentFrame, out object element));
            return element as IVsWindowFrame;
        }

        public static IVsTextView GetActiveView(this IVsTextManager textManager)
        {
            ErrorHandler.ThrowOnFailure(textManager.GetActiveView(1, null, out IVsTextView textView));
            return textView;
        }


        private static EnvDTE80.DTE2 GetDTE2()
        {
            return Microsoft.VisualStudio.Shell.Package.GetGlobalService(typeof(DTE)) as EnvDTE80.DTE2;
        }

        public static string GetSourceFilePath()
        {
            EnvDTE80.DTE2 _applicationObject = GetDTE2();
            UIHierarchy uih = _applicationObject.ToolWindows.SolutionExplorer;
            Array selectedItems = (Array)uih.SelectedItems;
            if (null != selectedItems)
            {
                foreach (UIHierarchyItem selItem in selectedItems)
                {
                    ProjectItem prjItem = selItem.Object as ProjectItem;
                    string filePath = prjItem?.Properties.Item("FullPath").Value.ToString();
                    //System.Windows.Forms.MessageBox.Show(selItem.Name + filePath);
                    return filePath;
                }
            }
            return string.Empty;
        }
    }
}
