﻿using System.Collections.Generic;
using System.Linq;
using Logic.Building;
using Logic.Filtering;
using Logic.Integration;
using Logic.Scopes;
using Logic.SemanticTree;

namespace Logic
{
    public class DiagramFromDiagramDefinitionGenerator
    {
        private readonly AdvancedSolution _solution;
        public DiagramFromDiagramDefinitionGenerator(AdvancedSolution solution)
        {
            _solution = solution;
        }

        public IEnumerable<DiagramDefinitionParseResult> GetDiagramDefinitions()
        {
            return _solution.ArchProjects.SelectMany(
                project => project.GetDiagramDefinitionFiles().Select(
                    file => file.Parse(_solution.Directory)));
        }

        public Node GenerateDiagram(DiagramDefinition diagramDef)
        {
            Node tree = null;
            if (diagramDef.Scope is RootScope)
            {
                tree = SemanticTreeBuilder.AnalyseSolution(_solution);
            }
            if (diagramDef.Scope is DocumentScope)
            {
                tree = SemanticTreeBuilder.AnalyseDocument(_solution, ((DocumentScope) diagramDef.Scope).Name);
            }
            if (diagramDef.Scope is ClassScope)
            {
                tree = SemanticTreeBuilder.AnalyseClass(_solution, ((ClassScope)diagramDef.Scope).Name);
            }
            if (diagramDef.Scope is NamespaceScope)
            {
                tree = SemanticTreeBuilder.AnalyseNamespace(_solution, ((NamespaceScope) diagramDef.Scope).Name);
            }
            if (diagramDef.Scope is ProjectScope)
            {
                tree = SemanticTreeBuilder.AnalyseProject(_solution, ((ProjectScope) diagramDef.Scope).Name);
            }
            ModelFilterer.ApplyFilter(ref tree,diagramDef.Filters);

            return diagramDef.DependencyDown ? ReverseChildren(tree) : tree;
        }

        public static Node ReverseChildren(Node tree)
        {
            tree.SetChildren(tree.Orientation == OrientationKind.Horizontal
                ? tree.Childs.Select(ReverseChildren) // Horizontal layers are ordered alphabetically
                : tree.Childs.Select(ReverseChildren).Reverse());
            return tree;
        }
    }
}
