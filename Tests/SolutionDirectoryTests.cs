﻿using System.Runtime.InteropServices;
using EnvDTE;
using Logic.Building;
using Logic.Building.SemanticTree;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests
{
    [TestClass]
    public class SolutionDirectoryTests
    {
        [TestMethod]
        [TestCategory("ProjectModel")]
        public void ContainsSolutionDirectories()
        {
            var dte = (DTE) Marshal.
                GetActiveObject("VisualStudio.DTE.14.0");
            var tree = ProjectTreeBuilder.AddSolutionFoldersToTree(dte.Solution.Projects);
            Assert.IsTrue(tree.Childs.WithName("Clients") != null);
        }
    }
}
