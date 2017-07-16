using Carnac.Logic.KeyMonitor;
using System;
using System.Diagnostics;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows.Forms;
using Gma.System.MouseKeyHook;

namespace Carnac.Logic.MouseMonitor
{
    public class InterceptMouse : IInterceptKeys
    {

        public static readonly InterceptMouse Current = new InterceptMouse();
        readonly IKeyboardMouseEvents m_GlobalHook = Hook.GlobalEvents();
        readonly IObservable<InterceptKeyEventArgs> keyStream;
        private IObserver<InterceptKeyEventArgs> observer;
        private readonly KeysConverter kc = new KeysConverter();
        

        InterceptMouse()
        {
            keyStream = Observable.Create<InterceptKeyEventArgs>(observer =>
            {
                this.observer = observer;
                m_GlobalHook.MouseClick += OnMouseClick;
                m_GlobalHook.MouseDoubleClick += OnMouseDoubleClick;
                m_GlobalHook.MouseWheel += HookManager_MouseWheel;
                Debug.Write("Subscribed to mouse");

                return Disposable.Create(() =>
                {
                    m_GlobalHook.MouseClick -= OnMouseClick;
                    m_GlobalHook.MouseDoubleClick -= OnMouseDoubleClick;
                    m_GlobalHook.MouseWheel -= HookManager_MouseWheel;
                    m_GlobalHook.Dispose();
                    Debug.Write("Unsubscribed from mouse");
                });
            })
            .Publish().RefCount();

        }

        private Keys MouseButtonsToKeys(MouseButtons button)
        {
            switch(button)
            {
                case MouseButtons.Left:
                    return Keys.LButton;
                case MouseButtons.Middle:
                    return Keys.MButton;
                case MouseButtons.Right:
                    return Keys.RButton;
                case MouseButtons.XButton1:
                    return Keys.XButton1;
                case MouseButtons.XButton2:
                    return Keys.XButton2;
                default:
                    return Keys.None;
            }
        }

        private void OnMouseClick(object sender, MouseEventArgs e)
        {
            observer.OnNext(new InterceptKeyEventArgs(
                MouseButtonsToKeys(e.Button),
                KeyDirection.Down,
                Control.ModifierKeys == Keys.Alt,
                Control.ModifierKeys == Keys.Control,
                Control.ModifierKeys == Keys.Shift));
        }

        private void OnMouseDoubleClick(object sender, MouseEventArgs e)
        {
            observer.OnNext(new InterceptKeyEventArgs(
                MouseButtonsToKeys(e.Button),
                KeyDirection.Down,
                Control.ModifierKeys == Keys.Alt,
                Control.ModifierKeys == Keys.Control,
                Control.ModifierKeys == Keys.Shift));
        }

        private void HookManager_MouseWheel(object sender, MouseEventArgs e)
        {
            // for now using VolumeDown and Up as proxy could be refactored
            observer.OnNext(new InterceptKeyEventArgs(
                e.Delta > 0 ? Keys.VolumeUp : Keys.VolumeDown,
                KeyDirection.Down,
                Control.ModifierKeys == Keys.Alt,
                Control.ModifierKeys == Keys.Control,
                Control.ModifierKeys == Keys.Shift));
        }

        public IObservable<InterceptKeyEventArgs> GetKeyStream()
        {
            return keyStream;
        }

    }
}

