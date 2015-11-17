using System.Collections.Generic;
using ICSharpCode.NRefactory.Visitors;
using ICSharpCode.NRefactory.Ast;
using FluentSharp.CoreLib;


namespace FluentSharp.CSharpAST.Utils
{
    public class GetAllINodes : NodeTrackingAstVisitor
    {
        public List<INode> AllNodes { get; set; }
        public Dictionary<string, List<INode>> NodesByType { get; set; }

        public GetAllINodes()
        {
            AllNodes = new List<INode>();
            NodesByType = new Dictionary<string, List<INode>>();
        }
        
        public GetAllINodes(CompilationUnit compilationUnit) : this()
        {
        	compilationUnit.AcceptVisitor(this,null);
        }

        protected override void BeginVisit(INode node)
        {
            AllNodes.add(node);
            NodesByType.add(node.typeName(), node);
        }

    }
}
