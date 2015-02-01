using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Reactive.Linq;
using System.Text;
using System.Windows;
using System.Windows.Forms;
using Carnac.Logic.KeyMonitor;
using Carnac.Tests;
using Microsoft.CSharp;

namespace KeyStreamCapture
{
    public partial class MainWindow
    {
        readonly List<InterceptKeyEventArgs> keys = new List<InterceptKeyEventArgs>();
        readonly IDisposable subscription;
        bool capturing;

        public MainWindow()
        {
            InitializeComponent();
            subscription = InterceptKeys.Current.GetKeyStream().Subscribe(value =>
            {
                if (capturing)
                    keys.Add(value);
            });
        }

        private void StartCapture(object sender, RoutedEventArgs e)
        {
            keys.Clear();
            capturing = true;
        }

        private void StopCapture(object sender, RoutedEventArgs e)
        {
            capturing = false;
            GenerateCode();
        }

        private void GenerateCode()
        {
            keys.ToObservable();
            var cgo = new CodeGeneratorOptions
                          {
                              BracingStyle = "C",
                              BlankLinesBetweenMembers = false
                          };
            using (var provider = new CSharpCodeProvider())
            {
                var method = new CodeMemberMethod
                {
                    Name = "KeyStream",
                    ReturnType = new CodeTypeReference(typeof(IObservable<InterceptKeyEventArgs>))
                };

                var player =
                    new CodeVariableDeclarationStatement(new CodeTypeReference(typeof(KeyPlayer)), "keys",
                    new CodeObjectCreateExpression(typeof(KeyPlayer)));
                method.Statements.Add(player);
                foreach (var interceptKeyEventArgse in keys)
                {
                    var key = new CodeObjectCreateExpression(new CodeTypeReference(typeof(InterceptKeyEventArgs)),
                    new CodePropertyReferenceExpression(new CodeTypeReferenceExpression(typeof(Keys)), interceptKeyEventArgse.Key.ToString()),
                    new CodePropertyReferenceExpression(new CodeTypeReferenceExpression(typeof(KeyDirection)), interceptKeyEventArgse.KeyDirection.ToString()),
                    new CodePrimitiveExpression(interceptKeyEventArgse.AltPressed),
                    new CodePrimitiveExpression(interceptKeyEventArgse.ControlPressed),
                    new CodePrimitiveExpression(interceptKeyEventArgse.ShiftPressed));

                    var keyPress = new CodeMethodInvokeExpression(new CodeVariableReferenceExpression("keys"), "Add", key);
                    method.Statements.Add(keyPress);
                }
                method.Statements.Add(new CodeMethodReturnStatement(new CodeVariableReferenceExpression("keys")));

                var sb = new StringBuilder();
                using(var stringWriter = new StringWriter(sb))
                {
                    provider.GenerateCodeFromMember(method, stringWriter, cgo);
                }
                textBox.Text = sb.ToString();
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            subscription.Dispose();
        }
    }
}
