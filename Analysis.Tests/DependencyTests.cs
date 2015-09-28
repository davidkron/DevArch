﻿using Analysis;
using System.Linq;
using Analysis.SemanticTree;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Analysis.Tests
{
    [TestClass]
    public class DependencyTests
    {
        [TestMethod]
        public void ContainsReferences()
        {
            using (var fakeWorkspace = new AdhocWorkspace())
            {
                var project = fakeWorkspace.AddProject("ProjectA", LanguageNames.CSharp);
                fakeWorkspace.AddDocument(project.Id, "DocumentA.cs", SourceText.From(
                    "class GuiFacade " +
                    "{" +
                    "public void Foo(){new Button().Bar();}" +
                    "public static void Main(){};" +
                    "}"
                ));
                fakeWorkspace.AddDocument(project.Id, "DocumentB.cs", SourceText.From("class Button {public void Bar(){}}"));
                var tree = new Tree();
                Analyser.AnalyzeSolutionToTree(fakeWorkspace.CurrentSolution, ref tree);
                var compile = project.GetCompilationAsync().Result;
                var diagnostics = compile.GetParseDiagnostics();
                Assert.IsTrue(!diagnostics.Any());

                var button = tree.Childs.FirstOrDefault(x => x.Name == "Button");
                Assert.IsNotNull(button);
                Assert.IsTrue(button.References.Any());
            }
        }

        [TestMethod]
        public void ContainsDependencies()
        {
            using (var fakeWorkspace = new AdhocWorkspace())
            {
                var project = fakeWorkspace.AddProject("ProjectA", LanguageNames.CSharp);
                fakeWorkspace.AddDocument(project.Id, "DocumentA.cs", SourceText.From(
                    "class GuiFacade " +
                    "{" +
                    "public void Foo(){new Button().Bar();}" +
                    "public static void Main(){};" +
                    "}"
                ));
                fakeWorkspace.AddDocument(project.Id, "DocumentB.cs", SourceText.From("class Button {public void Bar(){}}"));
                var tree = new Tree();
                Analyser.AnalyzeSolutionToTree(fakeWorkspace.CurrentSolution, ref tree);
                var button = tree.Childs.FirstOrDefault(x => x.Name == "Button");
                var guiFacade = tree.Childs.FirstOrDefault(x => x.Name == "GuiFacade");
                Assert.IsNotNull(button);
                Assert.IsNotNull(guiFacade);
                Assert.IsTrue(button.References.Any());
                Assert.IsTrue(guiFacade.Dependencies.Any());
            }
        }
    }
}
