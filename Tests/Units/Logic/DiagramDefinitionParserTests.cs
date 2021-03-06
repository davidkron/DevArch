﻿using System.Linq;
using FluentAssertions;
using Logic;
using Logic.Filtering.Filters;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests.Units.Logic
{
    [TestClass]
    public class DiagramDefinitionParserTests
    {
        [TestCategory("Parsing")]
        [TestMethod]
        public void ParsesHideAnonymousLayer()
        {
            const string content = 
            @"<?xml version=""1.0"" encoding =""utf -8"" ?>
            <Diagram HideAnonymousLayers = ""false"">
              <Scope>
                <Root/>
              </Scope>
              <Output Path = ""Logic.png""/>
              <Filters>
              </Filters>
            </Diagram>";

            var result = DiagramDefinitionParser.ParseDiagramDefinition("", content);
            Assert.IsFalse(result.HideAnonymousLayers);
        }
        [TestCategory("Parsing.Filters")]
        [TestMethod]
        public void ParsesFilters()
        {
            const string content =
            @"<?xml version=""1.0"" encoding =""utf -8"" ?>
            <Diagram HideAnonymousLayers = ""false"">
              <Scope> <Root/> </Scope>
              <Output Path = ""Logic.png""/>
              <Filters>
                <RemoveTests>On</RemoveTests>
                <MaxDepth>3</MaxDepth>
                <RemoveExceptions>Off</RemoveExceptions>
              </Filters>
            </Diagram>";

            var result = DiagramDefinitionParser.ParseDiagramDefinition("", content).Filters;
            var removeTests = result.First(x => x.Name == "RemoveTests");
            var maxDepth = result.First(x => x.Name == "MaxDepth") as IntegralFilter;
            var removeExceptions = result.First(x => x.Name == "RemoveExceptions");;
            var removeDn = result.First(x => x.Name == "RemoveDefaultNamespaces");

            // Assert

            removeTests.ShouldBeApplied.Should().BeTrue();
            maxDepth.Parameter.Should().Be(3);
            removeExceptions.ShouldBeApplied.Should().BeFalse();
            //On by default
            removeDn.ShouldBeApplied.Should().BeTrue();
        }
    }
}
