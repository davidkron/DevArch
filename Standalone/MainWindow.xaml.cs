﻿using System.Collections.Generic;
using System.Runtime.InteropServices;
using Analysis;
using Analysis.SemanticTree;
using EnvDTE;
using Window = System.Windows.Window;

namespace Standalone
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            ArchControl.GenerateDiagram(GetDte());
        }


        private static DTE GetDte()
        {
            return (DTE)Marshal.
                GetActiveObject("VisualStudio.DTE.14.0");
        }
    }
}
