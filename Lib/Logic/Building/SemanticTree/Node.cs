using System.Collections.Generic;
using System.Linq;
using EnvDTE;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.FindSymbols;
using Document = Microsoft.CodeAnalysis.Document;
using Project = Microsoft.CodeAnalysis.Project;

namespace Logic.Building.SemanticTree
{
    public class Node : Tree
    {
        public Node(ISymbol symbol)
        {
            Symbol = symbol;
        }
        public Node(string name)
        {
            _name = name;
        }

        private readonly string _name;
        public string Name => _name ?? Symbol.Name;

        public readonly ISymbol Symbol;
        public List<Node> Dependencies = new List<Node>();
        public readonly HashSet<Node> SiblingDependencies = new HashSet<Node>();
        public Tree Parent;

        public override string ToString()
        {
            return Childs.Any() ? $"({Name} = {base.ToString()})" : Name;
        }
        
        public new Node  FindNodeWithSymbol(ISymbol symbol)
        {
            return Equals(Symbol, symbol) ? this : Childs.Select(x => x.FindNodeWithSymbol(symbol)).FirstOrDefault(x => x != null);
        }
    }

    public class ClassNode : Node
    {
        public readonly IEnumerable<ReferencedSymbol> References;
        public readonly IEnumerable<TypeSyntax> BaseClasses;
        public IEnumerable<INamedTypeSymbol> SymbolDependencies; 
        public ClassNode(ISymbol symbol, IEnumerable<ReferencedSymbol> references,IEnumerable<TypeSyntax> baseClasses) : base(symbol)
        {
            References = references;
            BaseClasses = baseClasses;
        }

        public override bool Equals(object obj)
        {
            if (obj is ClassNode)
                return (obj as ClassNode).Symbol.Equals(Symbol);

            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return Symbol?.GetHashCode() ?? 0;
            }
        }
    }
    
    public class ProjectNode : Node
    {
        public ProjectNode(ProjectItem p) : base(p.Name)
        { }
        public IEnumerable<Document> Documents = new List<Document>();

        public ProjectNode(Project project) : base(project.Name)
        {
            Documents = project.Documents;
        }
    }

    public class SiblingHolderNode : Node
    {
        public SiblingHolderNode(IEnumerable<Node> siblingNodes) : base("")
        {
            SetChildren(siblingNodes);
            Horizontal = true;
        }
    }


    public class CircularDependencyHolderNode : SiblingHolderNode
    {
        public CircularDependencyHolderNode(IEnumerable<Node> siblingNodes) : base(siblingNodes)
        {
        }
    }
}
 