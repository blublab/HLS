﻿#pragma checksum "..\..\..\Sendungsanfrage\Status.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "8902D7CF6FE18D482B6AD2E9DA78DAB8"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.18444
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Media.TextFormatting;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Shell;


namespace Client {
    
    
    /// <summary>
    /// Status
    /// </summary>
    public partial class Status : System.Windows.Window, System.Windows.Markup.IComponentConnector {
        
        
        #line 6 "..\..\..\Sendungsanfrage\Status.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBlock tb_id;
        
        #line default
        #line hidden
        
        
        #line 8 "..\..\..\Sendungsanfrage\Status.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button btn_Annehmen;
        
        #line default
        #line hidden
        
        
        #line 9 "..\..\..\Sendungsanfrage\Status.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button btn_Ablehnen;
        
        #line default
        #line hidden
        
        
        #line 12 "..\..\..\Sendungsanfrage\Status.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button btn_Cancel;
        
        #line default
        #line hidden
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Uri resourceLocater = new System.Uri("/HAW Logistics System;component/sendungsanfrage/status.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\Sendungsanfrage\Status.xaml"
            System.Windows.Application.LoadComponent(this, resourceLocater);
            
            #line default
            #line hidden
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        void System.Windows.Markup.IComponentConnector.Connect(int connectionId, object target) {
            switch (connectionId)
            {
            case 1:
            this.tb_id = ((System.Windows.Controls.TextBlock)(target));
            return;
            case 2:
            this.btn_Annehmen = ((System.Windows.Controls.Button)(target));
            
            #line 8 "..\..\..\Sendungsanfrage\Status.xaml"
            this.btn_Annehmen.Click += new System.Windows.RoutedEventHandler(this.Btn_Annehmen_Click);
            
            #line default
            #line hidden
            return;
            case 3:
            this.btn_Ablehnen = ((System.Windows.Controls.Button)(target));
            
            #line 9 "..\..\..\Sendungsanfrage\Status.xaml"
            this.btn_Ablehnen.Click += new System.Windows.RoutedEventHandler(this.Btn_Ablehnen_Click);
            
            #line default
            #line hidden
            return;
            case 4:
            this.btn_Cancel = ((System.Windows.Controls.Button)(target));
            
            #line 12 "..\..\..\Sendungsanfrage\Status.xaml"
            this.btn_Cancel.Click += new System.Windows.RoutedEventHandler(this.Btn_Cancel_Click);
            
            #line default
            #line hidden
            return;
            }
            this._contentLoaded = true;
        }
    }
}

