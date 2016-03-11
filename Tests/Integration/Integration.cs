﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using EnvDTE;
using FluentAssertions;
using Lib;
using Logic;
using Logic.Building;
using Logic.Filtering;
using Logic.Filtering.Filters;
using Logic.Integration;
using Logic.Ordering;
using Logic.Scopes;
using Logic.SemanticTree;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VSSDK.Tools.VsIdeTesting;
using Presentation;
using ToolsMenu;
using static Tests.AssertionExtensions;
using static Tests.TestExtesions;

namespace Tests.Integration
{
    [TestClass]
    public class Integration
    {
        [TestCategory("Integration")]
        [TestMethod]
        public void FindsDependencies()
        {
            var modelGen = new DiagramGenerator(TestSolution);
            var tree = modelGen.GenerateDiagram(DiagramDefinition.RootDefault);
            var lib = tree.Childs.WithName("Lib");
            Assert.AreEqual(1,lib.DescendantNodes().Count(x => x.Name == "Node"));
            var clients = tree.Childs.WithName("Clients");
            var dependency = clients.AllSubDependencies().Any(x => x.Name == "DevArch");
            Assert.IsNotNull(dependency);
        }

        [TestCategory("Integration")]
        [TestMethod]
        public void SemanticTreeDoesNotContainDoubles()
        {
            var complete = SemanticTreeBuilder.AnalyseSolution(TestSolution);
            var tree = SemanticTreeBuilder.FindNamespace(complete, "Logic\\SemanticTree");
            tree.DescendantNodes().Count(x => x.Name == "Node").Should().Be(1);
            tree.RelayoutBasedOnDependencies();
            tree.DescendantNodes().Count(x => x.Name == "Node").Should().Be(1);
        }

        [TestCategory("Integration")]
        [TestMethod]
        public void LogicLayerIsVertical()
        {
            var complete = SemanticTreeBuilder.AnalyseSolution(TestSolution);
            var tree = SemanticTreeBuilder.FindNamespace(complete, "Logic\\Logic");
            tree.RemoveChild("DiagramDefinition");
            tree.RemoveChild("Filtering");
            tree.RemoveChild("DiagramGenerator");
            tree.RemoveChild("DiagramDefinitionParser");
            tree.RemoveChild("Common");
            tree.RemoveChild("OutputSettings");
            
            /*foreach (var child in tree.Childs)
            {
                //Remove those not in childs
                child.Dependencies =
                    child.Dependencies.Intersect(tree.Childs).ToList();
            }*/
            tree.RelayoutBasedOnDependencies();
            tree.Orientation.Should().Be(OrientationKind.Vertical);
        }

        [TestCategory("Integration")]
        [TestMethod]
        public void GeneratesWholeSolutionDiagramWithoutNamespacesWithoutCausingDuplicates()
        {
            var filters = DiagramDefinition.DefaultFilters;
            filters.Add(new RemoveContainers(true));
            var diagramGen = new DiagramGenerator(TestSolution);
            var diagramDef = new DiagramDefinition("",
                new RootScope(), new OutputSettings (SlnDir + "IntegrationTests\\NoContainers.png"), filters, true,false);
            var tree = diagramGen.GenerateDiagram(diagramDef);
            BitmapRenderer.RenderTreeToBitmap(tree, diagramDef.DependencyDown, diagramDef.Output, diagramDef.HideAnonymousLayers);
            TreeAssert.DoesNotContainDuplicates(tree);
        }

        [TestMethod]
        public void DeploysProjectSystem()
        {
            var projectFilesPath = ProjectDeployer.LocalProjectFilesPath;
            Directory.Delete(projectFilesPath,true);
            File(projectFilesPath).Should().NotExist();
            ProjectDeployer.EnsureDevArchProjectSupportExists();
            Dir(projectFilesPath).Should().Exist();
            Dir(projectFilesPath + "\\Rules").Should().Exist();
            File(projectFilesPath + "\\CustomProject.Default.props").Should().Exist();
            Directory.Delete(projectFilesPath,true);
        }
    }
}
