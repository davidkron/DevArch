﻿using System;
using System.Linq;
using System.Runtime.InteropServices;
using EnvDTE;
using Logic;
using Logic.Building;
using Logic.Filtering;
using Logic.Integration;
using Logic.SemanticTree;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Presentation;
using static Tests.TestExtesions;

namespace Tests.Integration
{
    [TestClass]
    public class Integration
    {
        [TestMethod]
        public void FindsDependencies()
        {
            var modelGen = new DiagramFromDiagramDefinitionGenerator(TestSolution);
            var tree = modelGen.GenerateDiagram(DiagramDefinition.RootDefault);
            var lib = tree.Childs.WithName("Lib");
            Assert.AreEqual(1,lib.DescendantNodes().Count(x => x.Name == "Node"));
            var clients = tree.Childs.WithName("Clients");
            var dependency = clients.AllSubDependencies().Any(x => x.Name == "DevArch");
            Assert.IsNotNull(dependency);
        }

        [TestMethod]
        public void SemanticTreeDoesNotContainDoubles()
        {
            var tree = SemanticTreeBuilder.AnalyseNamespace(TestSolution, "Logic\\SemanticTree");
            Assert.AreEqual(1, tree.DescendantNodes().Count(x => x.Name == "Node"));
            ModelFilterer.ApplyFilter(ref tree, new Filters());
            Assert.AreEqual(1, tree.DescendantNodes().Count(x => x.Name == "Node"));
        }

        [TestMethod]
        public void LogicLayerIsVertical()
        {
            var tree = SemanticTreeBuilder.AnalyseNamespace(TestSolution, "Logic");
            tree = tree.Childs.First(); tree = tree.Childs.First();
            tree.RemoveChild("DiagramDefinition");
            tree.RemoveChild("Filters");
            tree.RemoveChild("DiagramFromDiagramDefinitionGenerator");
            tree.RemoveChild("DiagramDefinitionParser");
            tree.RemoveChild("Common");
            tree.RemoveChild("OutputSettings");
            tree.RemoveChild("NamedScope");
            tree.RemoveChild("DocumentScope");
            tree.RemoveChild("ProjectScope");
            tree.RemoveChild("NamespaceScope");
            tree.RemoveChild("ClassScope");
            tree.RemoveChild("NoArchProjectsFound");
            
            foreach (var child in tree.Childs)
            {
                //Remove those not in childs
                child.Dependencies =
                    child.Dependencies.Intersect(tree.Childs).ToList();
            }
            ModelFilterer.ApplyFilter(ref tree, new Filters());
            Assert.AreEqual(OrientationKind.Vertical,tree.Orientation);
        }

        [TestMethod]
        public void GeneratesWholeSolutionDiagramWithoutNamespacesWithoutCausingDuplicates()
        {
            var diagramGen = new DiagramFromDiagramDefinitionGenerator(TestSolution);
            var diagramDef = new DiagramDefinition("",
                new RootScope(), new OutputSettings(), new Filters { RemoveContainers = true }, true, false);
            var tree = diagramGen.GenerateDiagram(diagramDef);
            TreeAssert.DoesNotContainDuplicates(tree);
        }

        [TestMethod]
        public void TestCurArchNoNspaces()
        {
            var diagramDef = new DiagramDefinition("",
                new RootScope(), new OutputSettings
                {
                    Path = SlnDir + "IntegrationTests\\WithoutNspaces.png"
                }, new Filters {RemoveContainers = true}, true, false);
            var tree = SemanticTreeBuilder.AnalyseSolution(TestSolution) as Node;
            var lib = tree.Childs.WithName("Lib");
            lib.RemoveChild("Logic");
            lib.RemoveChild("Presentation");
            var clients = tree.Childs.WithName("Clients");
            var vsclients = clients.Childs.WithName("VisualStudio");
            vsclients.RemoveChild("DevArchProject.ProjectType");
            ModelFilterer.ApplyFilter(ref tree, new Filters {RemoveContainers = true});
            DiagramFromDiagramDefinitionGenerator.ReverseTree(tree);
            BitmapRenderer.RenderTreeToBitmap(tree, diagramDef.DependencyDown, diagramDef.Output,diagramDef.HideAnonymousLayers);
        }
    }
}
