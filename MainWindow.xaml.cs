using Gma.System.MouseKeyHook;
using SQLitePCL;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management.Instrumentation;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MouseHook
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        List<Process> processWithWindow = new List<Process>();
        WindowProcess windowProcess;
        public MainWindow()
        {
            InitializeComponent();

            GetSysProcessWithWindow();      // 获取当前有窗口的进程

            windowProcess = new WindowProcess();        // 定义一个对象
            windowProcess.ProcessWithWindow = processWithWindow;
            // 订阅事件
            windowProcess.MouseDownWinProcess += WindowProcess_MouseDownWinProcess;
            windowProcess.MouseMoveWinProcess += WindowProcess_MouseMoveWinProcess;

            windowProcess.StartGetWinProcess();         // 启用钩子  (先订阅事件)
        }

        private void WindowProcess_MouseMoveWinProcess(Process process)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append($"名称：{process.MainModule.FileVersionInfo.ProductName}{Environment.NewLine}");
            stringBuilder.Append($"版本：{process.MainModule.FileVersionInfo.FileVersion}{Environment.NewLine}");
            stringBuilder.Append($"描述：{process.MainModule.FileVersionInfo.FileDescription}{Environment.NewLine}");

            this.tb.Text = stringBuilder.ToString();
        }

        private void WindowProcess_MouseDownWinProcess(Process process)
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append($"名称：{process.MainModule.FileVersionInfo.ProductName}{Environment.NewLine}");
            stringBuilder.Append($"版本：{process.MainModule.FileVersionInfo.FileVersion}{Environment.NewLine}");
            stringBuilder.Append($"描述：{process.MainModule.FileVersionInfo.FileDescription}{Environment.NewLine}");

            this.tb.Text = stringBuilder.ToString();
        }

        private void GetSysProcessWithWindow()
        {
            Process[] processes = Process.GetProcesses();
            processWithWindow.Clear();

            foreach (var p in processes)
            {
                if (p.MainWindowHandle == IntPtr.Zero) continue;
                if (p.MainWindowTitle.Length == 0) continue;

                processWithWindow.Add(p);
            }
        }
    }
}
