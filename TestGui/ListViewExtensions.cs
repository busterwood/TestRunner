using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TestGui
{
    static class ListViewExtensions
    {
        const int LVM_SETGROUPINFO = 0x1000 + 147;
        const int LVGF_STATE = 4;

        [DllImport("user32.dll")]
        static extern int SendMessage(IntPtr window, int message, int wParam, LvGroup lParam);

        public static void Collapse(this ListViewGroup lwgroup)
        {
            LvGroup group = new LvGroup();
            group.cbSize = Marshal.SizeOf(group);
            group.state = (int)GroupState.COLLAPSED;
            group.mask = LVGF_STATE;
            group.iGroupId = lwgroup.ListView.Groups.IndexOf(lwgroup);
            SendMessage(lwgroup.ListView.Handle, LVM_SETGROUPINFO, group.iGroupId, group);
        }

        public static void Expand(this ListViewGroup lwgroup)
        {
            LvGroup group = new LvGroup();
            group.cbSize = Marshal.SizeOf(group);
            group.state = (int)(GroupState.EXPANDED | GroupState.COLLAPSIBLE);
            group.mask = LVGF_STATE;
            group.iGroupId = lwgroup.ListView.Groups.IndexOf(lwgroup);
            SendMessage(lwgroup.ListView.Handle, LVM_SETGROUPINFO, group.iGroupId, group);
        }

        public static void Hide(this ListViewGroup lwgroup)
        {
            LvGroup group = new LvGroup();
            group.cbSize = Marshal.SizeOf(group);
            group.state = (int)GroupState.HIDDEN;
            group.mask = LVGF_STATE;
            group.iGroupId = lwgroup.ListView.Groups.IndexOf(lwgroup);
            SendMessage(lwgroup.ListView.Handle, LVM_SETGROUPINFO, group.iGroupId, group);
        }

        [StructLayout(LayoutKind.Sequential)]
        class LvGroup
        {
            public int cbSize;
            public int mask;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string pszHeader;
            public int cchHeader;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string pszFooter;
            public int cchFooter;
            public int iGroupId;
            public int stateMask;
            public int state;
            public int uAlign;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string pszSubtitle;
            public int cchSubtitle;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string pszTask;
            public int cchTask;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string pszDescriptionTop;
            public int cchDescriptionTop;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string pszDescriptionBottom;
            public int cchDescriptionBottom;
            public int iTitleImage;
            public int iExtendedImage;
            public int iFirstItem;
            public int cItems;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string pszSubsetTitle;
            public int cchSubsetTitle;
        }

        [Flags]
        enum GroupState : int
        {
            COLLAPSIBLE = 8,
            COLLAPSED = 1,
            EXPANDED = 0,
            HIDDEN = 2,
        }
    }
}
