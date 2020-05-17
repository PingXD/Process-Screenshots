using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MouseHook
{
    class CaptrueImage
    {

        // 捕获指定进程的截图
        public static Bitmap Captuer(Process process)
        {
            if (process == null) return null;

            // 1) 获取设备上下文
            IntPtr windowDCHandle = GetWindowDC(IntPtr.Zero);       // 主窗口
            if (windowDCHandle == IntPtr.Zero)
                return null;

            // 2) 获取指定窗口边界和尺寸：GetWindowRect
            Rectangle rectangle = new Rectangle();
            if (GetWindowRect(process.MainWindowHandle, ref rectangle) == 0)
            {
                ReleaseDC(process.MainWindowHandle, windowDCHandle);        // 释放公用的和设备上下文环境
                return null;
            }

            // 3) 计算窗口大小
            int width = rectangle.Width - rectangle.X;
            int height = rectangle.Height - rectangle.Y;

            // 4) 创建一个设备上下文相关的位图，CreateCompatibleBitmap -> DeleteObject
            IntPtr compatibleBitmapHandle = CreateCompatibleBitmap(windowDCHandle, width, height);
            if (compatibleBitmapHandle == IntPtr.Zero)
            {
                DeleteObject(compatibleBitmapHandle);
                ReleaseDC(process.MainWindowHandle, windowDCHandle);
                return null;
            }

            // 5) 创建一个内存上下文兼容的句柄，CreateCompatibleDC -> DeleteDC 
            IntPtr compatibleDCHandle = CreateCompatibleDC(windowDCHandle);
            if (compatibleDCHandle == IntPtr.Zero)
            {
                DeleteObject(compatibleBitmapHandle);
                DeleteDC(compatibleDCHandle);
                ReleaseDC(process.MainWindowHandle, windowDCHandle);
                return null;
            }

            // 6) 选择一个设备上下文对象：SelectObject
            if (SelectObject(compatibleDCHandle, compatibleBitmapHandle) == IntPtr.Zero)
            {
                DeleteObject(compatibleBitmapHandle);
                DeleteDC(compatibleDCHandle);
                ReleaseDC(process.MainWindowHandle, windowDCHandle);
                return null;
            }

            // 7) 拷贝窗口到设备上下文：PrintWindow
            if (PrintWindow(process.MainWindowHandle, compatibleDCHandle, 0) == 0)
            {
                DeleteObject(compatibleBitmapHandle);
                DeleteDC(compatibleDCHandle);
                ReleaseDC(process.MainWindowHandle, windowDCHandle);
                return null;
            }

            Bitmap processImage = Image.FromHbitmap(compatibleBitmapHandle);

            // 8) 清理垃圾
            DeleteObject(compatibleBitmapHandle);
            DeleteDC(compatibleDCHandle);
            ReleaseDC(process.MainWindowHandle, windowDCHandle);

            return processImage;
        }


        #region Win32 Api

        [DllImport("User32.dll")]
        public static extern int PrintWindow(IntPtr hwnd, IntPtr hdcBlt, UInt32 nFlags);

        [DllImport("Gdi32.dll")]
        public static extern IntPtr SelectObject(IntPtr hdc, IntPtr h);

        [DllImport("Gdi32.dll")]
        public static extern IntPtr CreateCompatibleDC(IntPtr hdc);

        [DllImport("Gdi32.dll")]
        public static extern int DeleteDC(IntPtr hdc);

        [DllImport("Gdi32.dll")]
        public static extern IntPtr CreateCompatibleBitmap(IntPtr hdc, int cx, int cy);

        [DllImport("Gdi32.dll")]
        public static extern int DeleteObject(IntPtr ho);

        [DllImport("User32.dll")]
        public static extern int GetWindowRect(IntPtr hwnd, ref Rectangle lpRect);

        [DllImport("User32.dll")]
        public static extern IntPtr GetWindowDC(IntPtr hwnd);

        [DllImport("User32.dll")]
        public static extern int ReleaseDC(IntPtr hwnd, IntPtr hdc);

        #endregion

    }
} 
