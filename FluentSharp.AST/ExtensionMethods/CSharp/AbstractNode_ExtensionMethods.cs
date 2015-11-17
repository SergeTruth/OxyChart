using System;
using System.Collections.Generic;
using System.Linq;
using ICSharpCode.NRefactory.Ast;

using FluentSharp.CoreLib;

namespace FluentSharp.CSharpAST
{
    public static class AbstractNode_ExtensionMethods
    {
        public static bool hasReturnStatement(this AbstractNode abstractNode)
        {
			//return abstractNode.iNodes<ReturnStatement>().size() > 0;
            foreach (var returnStatement in abstractNode.iNodes<ReturnStatement>())     //only return true when the return has a value (i.e. on return xyz; and not on just return; )
                if (returnStatement.Expression != Expression.Null)
                    return true;
            return false;
        }

        public static INode lastChild(this AbstractNode abstractNode)
        {
            var childrenCount = abstractNode.Children.Count;
            if (childrenCount > 0)
                return abstractNode.Children[childrenCount - 1];
            return null;
        }

        public static bool isLastChild(this AbstractNode abstractNode, Type type)
        {
            var lastChild = abstractNode.lastChild();

            return (lastChild != null) && lastChild.GetType() == type;
        }

        public static object getLastReturnValue(this AbstractNode abstractNode)
        {
			var returnStatements = abstractNode.iNodes<ReturnStatement>();
			if (returnStatements.size() > 0)
            {
				var returnStatement = returnStatements.Last();
                if (returnStatement.Expression is PrimitiveExpression)
                {
                    var primitiveExpression = (PrimitiveExpression)returnStatement.Expression;
                    return primitiveExpression.Value;
                }
                return new object();
            }
            return null;
        }

        public static List<ReturnStatement> returnStatements(this AbstractNode abstractNode)
        {
            return abstractNode.notNull() ? abstractNode.iNodes<ReturnStatement>() 
                                          : new List<ReturnStatement> ();
        }
    }
}
