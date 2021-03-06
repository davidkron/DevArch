﻿using System.Collections.Generic;
using System.Linq;
using Logic.Ordering;
using Logic.SemanticTree;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable InconsistentNaming

namespace Tests.Units.Logic.Filtering
{
    [TestClass]
    public class NotProduceDuplicateTests
    {
        [TestCategory("SiblingOrder.DuplicateTests")]
        [TestMethod]
        public void RemovesNodesFromToBeGroupedThatArePartOfNestedGroup()
        {
            //In the following test, diagramsymbol will be added in the nested call when grouping arrowView and archView
            //Allthough only arrowView and archView will be sent as input to the nested call, the nested call will also add diagramSymbol, since both of them depend on diagramSymbol
            Node node = new Node(nameof(node));
            Node diagramSymbol = new Node(nameof(diagramSymbol));
            Node CircularDependencyHolder = new Node(nameof(CircularDependencyHolder)) { SiblingDependencies = {node} };
            Node archView = new Node(nameof(archView)) {SiblingDependencies = {diagramSymbol}};
            Node arrowView = new Node(nameof(arrowView)) { SiblingDependencies = { diagramSymbol } };
            Node layerMapper = new Node(nameof(layerMapper)) { SiblingDependencies = {node,archView,diagramSymbol,arrowView}};
            Node bitmapRenderer = new Node(nameof(bitmapRenderer)) {SiblingDependencies = {node,archView,layerMapper}};
            
            var newList = SiblingReorderer.LayOutSiblingNodes(new HashSet<Node>
            {
                node,diagramSymbol,CircularDependencyHolder,archView,arrowView,layerMapper,bitmapRenderer
            }).ToList();

            // bitmaprender
            // layermapper
            // arrow arch
            // diagramsybol      circulardeps
            //              node
            
            TestExtesions.TreeAssert.DoesNotContainDuplicates(newList);
        }


        [TestCategory("SiblingOrder.DuplicateTests")]
        [TestMethod]
        public void RemovesNodesFromNextTargetGroupedThatArePartOfNestedGroup()
        {
            //Produes duplicate RootScope
            Node rootScope = new Node(nameof(rootScope));
            Node diagramSymbol = new Node(nameof(diagramSymbol));
            Node layerMapper = new Node(nameof(layerMapper)) { SiblingDependencies = { diagramSymbol } };
            Node diagramDefiniton = new Node(nameof(diagramDefiniton)) { SiblingDependencies = { rootScope } };
            Node diagramDefinitionParser = new Node(nameof(diagramDefinitionParser)) { SiblingDependencies = { diagramDefiniton,rootScope } };

            Node smallClassFilter= new Node(nameof(smallClassFilter));
            Node patternFinder = new Node(nameof(patternFinder));
            Node modelFilterer = new Node(nameof(modelFilterer)) { SiblingDependencies = { smallClassFilter,patternFinder }};
            Node diagramDefinitonGenerator = new Node(nameof(diagramDefinitonGenerator))
            {
                SiblingDependencies = { diagramDefinitionParser, diagramDefiniton,rootScope,modelFilterer }
            };

            var newList = SiblingReorderer.LayOutSiblingNodes(new HashSet<Node>
            {
                rootScope,patternFinder,smallClassFilter,diagramSymbol,diagramDefiniton,diagramDefinitonGenerator,diagramDefinitionParser,layerMapper,modelFilterer
            });

            TestExtesions.TreeAssert.DoesNotContainDuplicates(newList);
        }

        [TestCategory("SiblingOrder.DuplicateTests")]
        [TestMethod]
        public void UnknownErrorCause()
        {
            //Produes duplicate RootScope
            Node rootScope = new Node(nameof(rootScope));
            Node node = new Node(nameof(node));
            Node classNode = new Node(nameof(classNode)) { SiblingDependencies = { node } };
            Node childrenFilter = new Node(nameof(childrenFilter)) { SiblingDependencies = { classNode } };

            Node diagramDefiniton = new Node(nameof(diagramDefiniton)) { SiblingDependencies = { rootScope } };

            Node diagramDefinitionParser = new Node(nameof(diagramDefinitionParser)) { SiblingDependencies = { diagramDefiniton, rootScope } };
            
            Node smallClassFilter = new Node(nameof(smallClassFilter)) { SiblingDependencies = { childrenFilter } };
            
            Node modelFilterer = new Node(nameof(modelFilterer)) { SiblingDependencies = { node,classNode,smallClassFilter } };

            Node solutioNode = new Node(nameof(solutioNode)) { SiblingDependencies = { node } };

            Node diagramFromDiagramGenerator = new Node(nameof(diagramFromDiagramGenerator)) {SiblingDependencies = {diagramDefinitionParser, node,diagramDefiniton,rootScope,modelFilterer }};

            var newList = SiblingReorderer.LayOutSiblingNodes(new HashSet<Node>
            {
                rootScope, diagramDefiniton, diagramDefinitionParser, node, classNode, childrenFilter, smallClassFilter,modelFilterer,solutioNode,diagramFromDiagramGenerator
            });
            
            TestExtesions.TreeAssert.DoesNotContainDuplicates(newList);
        }


    }
}
