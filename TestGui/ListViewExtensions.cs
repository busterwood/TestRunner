using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TestGui
{
    static class ListViewExtensions
    {
        static Func<ListViewGroup, int> GetGroupId;
        const int LVM_SETGROUPINFO = 0x1000 + 147;
        const int LVGF_STATE = 0x4;
        const int LVGF_GROUPID = 0x10;

        [DllImport("user32.dll")]
        static extern int SendMessage(IntPtr window, int message, int wParam, LvGroup lParam);

        static ListViewExtensions()
        {
            var propertyInfo = typeof(ListViewGroup).GetProperty("ID", BindingFlags.Instance | BindingFlags.NonPublic);
            GetGroupId = lvg => (int) propertyInfo.GetMethod.Invoke(lvg, null);
            Debug.Assert(GetGroupId != null);
        }

        public static void SetDoubleBuffer(this ListView listView)
        {
            listView
               .GetType()
               .GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic)
               .SetValue(listView, true, null);
        }

        public static void Collapse(this ListViewGroup lwgroup)
        {
            SetGroupState(lwgroup, GroupState.COLLAPSED);
        }

        public static void Expand(this ListViewGroup lwgroup)
        {
            SetGroupState(lwgroup, GroupState.NORMAL);
        }

        public static void Hide(this ListViewGroup lwgroup)
        {
            SetGroupState(lwgroup, GroupState.NOHEADER | GroupState.COLLAPSED);
        }

        private static void SetGroupState(ListViewGroup lvgroup, GroupState state)
        {
            var group = new LvGroup
            {
                state = (uint)state,
                mask = LVGF_STATE | LVGF_GROUPID,
                iGroupId = GetGroupId(lvgroup),
            };
            int result = SendMessage(lvgroup.ListView.Handle, LVM_SETGROUPINFO, group.iGroupId, group);
            //if (result == -1)
            //    throw new Win32Exception("failed to set list view group");
        }

        [StructLayout(LayoutKind.Sequential)]
        class LvGroup
        {
            public uint cbSize = (uint)Marshal.SizeOf(typeof(LvGroup));
            public uint mask;
            public IntPtr pszHeader;
            public int cchHeader;
            public IntPtr pszFooter = IntPtr.Zero;
            public int cchFooter = 0;
            public int iGroupId;
            public uint stateMask = 0;
            public uint state = 0;
            public uint uAlign;
        }

        [Flags]
        enum GroupState
        {
            COLLAPSIBLE = 8,
            COLLAPSED = 1,
            NORMAL = 0,
            HIDDEN = 2,
            NOHEADER = 4
        }
    }
}
