using System;
using System.Diagnostics.Contracts;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;

namespace Carnac.KeyMonitor
{
    namespace Common.Essentials
    {
        /// <summary>
        /// http://outcoldman.ru/en/blog/show/240
        /// </summary>
        public sealed class HotKey : IDisposable
        {
            public event Action<HotKey> HotKeyPressed;

            private readonly int id;
            private bool isKeyRegistered;
            readonly IntPtr handle;

            public HotKey(ModifierKeys modifierKeys, Keys key, Window window)
                : this(modifierKeys, key, new WindowInteropHelper(window))
            {
                Contract.Requires(window != null);
            }

            public HotKey(ModifierKeys modifierKeys, Keys key, WindowInteropHelper window)
                : this(modifierKeys, key, window.Handle)
            {
                Contract.Requires(window != null);
            }

            public HotKey(ModifierKeys modifierKeys, Keys key, IntPtr windowHandle)
            {
                Contract.Requires(modifierKeys != ModifierKeys.None || key != Keys.None);
                Contract.Requires(windowHandle != IntPtr.Zero);

                Key = key;
                KeyModifier = modifierKeys;
                id = GetHashCode();
                handle = windowHandle;
                RegisterHotKey();
                ComponentDispatcher.ThreadPreprocessMessage += ThreadPreprocessMessageMethod;
            }

            ~HotKey()
            {
                Dispose();
            }

            public Keys Key { get; private set; }

            public ModifierKeys KeyModifier { get; private set; }

            public void RegisterHotKey()
            {
                if (Key == Keys.None)
                    return;
                if (isKeyRegistered)
                    UnregisterHotKey();
                isKeyRegistered = HotKeyWinApi.RegisterHotKey(handle, id, KeyModifier, Key);
                if (!isKeyRegistered)
                    throw new ApplicationException("Hotkey already in use");
            }

            public void UnregisterHotKey()
            {
                isKeyRegistered = !HotKeyWinApi.UnregisterHotKey(handle, id);
            }

            public void Dispose()
            {
                ComponentDispatcher.ThreadPreprocessMessage -= ThreadPreprocessMessageMethod;
                UnregisterHotKey();
            }

            private void ThreadPreprocessMessageMethod(ref MSG msg, ref bool handled)
            {
                if (!handled)
                {
                    if (msg.message == HotKeyWinApi.WmHotKey
                        && (int)(msg.wParam) == id)
                    {
                        OnHotKeyPressed();
                        handled = true;
                    }
                }
            }

            private void OnHotKeyPressed()
            {
                if (HotKeyPressed != null)
                    HotKeyPressed(this);
            }
        }
    }

}
