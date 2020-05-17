using Gma.System.MouseKeyHook;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MouseHook
{
    public class WindowProcess
    {
        // 定义代理  用来定义事件
        public delegate void WinProcessHandler(Process process);

        // 定义鼠标移动时，获取的窗口所在进程
        public event WinProcessHandler MouseMoveWinProcess;

        // 定义鼠标左击时，获取的窗口所在进程
        public event WinProcessHandler MouseDownWinProcess;

        // 鼠标钩子
        private IKeyboardMouseEvents m_GlobalHook;

        // 存放一些有窗口的进程
        List<Process> processWithWindow = new List<Process>();
        public List<Process> ProcessWithWindow
        {
            get => processWithWindow;
            set => processWithWindow = value;
        }

        public void StartGetWinProcess()    // 开始使用钩子
        {
            m_GlobalHook = Hook.GlobalEvents();
            m_GlobalHook.MouseDownExt += M_GlobalHook_MouseDownExt;
            m_GlobalHook.MouseMoveExt += M_GlobalHook_MouseMoveExt;
        }

        // 鼠标移动，发送事件
        private void M_GlobalHook_MouseMoveExt(object sender, MouseEventExtArgs e)
        {
            IntPtr handle = WindowFromPoint(e.X, e.Y);      // 获取鼠标移到的进程
            if (handle == IntPtr.Zero) return;              // 没有窗口的进程

            foreach (var p in processWithWindow)
            {
                if (p.MainWindowHandle == handle)
                {
                    MouseMoveWinProcess?.Invoke(p);         // 调用事件  显示进程信息
                    return;
                }
            }
        }

        // 鼠标按下，发送事件
        private void M_GlobalHook_MouseDownExt(object sender, MouseEventExtArgs e)
        {
            if (e.Button == MouseButtons.Left)       // 左键按下
            {
                IntPtr handle = WindowFromPoint(e.X, e.Y);
                if (handle == IntPtr.Zero) return;

                foreach (var p in processWithWindow)
                {
                    if (p.MainWindowHandle == handle)
                    {
                        MouseDownWinProcess?.Invoke(p);

                        // 左键按下  获取截图
                        Bitmap bitmap = CaptrueImage.Captuer(p);
                        if (bitmap == null) return;

                        bitmap.Save("a.bmp");
                        Process.Start("mspaint", "a.bmp");
                        return;
                    }
                }
            }

            if(e.Button == MouseButtons.Right)
            {
                m_GlobalHook.MouseDownExt -= M_GlobalHook_MouseDownExt;
                m_GlobalHook.MouseMoveExt -= M_GlobalHook_MouseMoveExt;

                m_GlobalHook.Dispose();     // 释放钩子
            }
        }

        // 导入 Win32 Api
        [DllImport("user32.dll", EntryPoint = "WindowFromPoint")]
        public static extern IntPtr WindowFromPoint(int xPoint, int yPoint);

    }
}
