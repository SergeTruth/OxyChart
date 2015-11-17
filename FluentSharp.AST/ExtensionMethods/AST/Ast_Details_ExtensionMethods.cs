using System.Collections.Generic;
using System.Linq;
using FluentSharp.CoreLib;
using FluentSharp.CSharpAST.Utils;
using ICSharpCode.NRefactory.Ast;

namespace FluentSharp.AST
{
    public static class Ast_Details_ExtensionMethods
    {
        public static List<MethodDeclaration> methodDeclarations(this AstDetails astDetails)
        {
            return (from astValue in astDetails.methods()
                select astValue.OriginalObject).toList();
        }
        public static List<AstValue<MethodDeclaration>> methods(this AstDetails astDetails)
        {
            return (astDetails.notNull()) ?  astDetails.Methods : new List<AstValue<MethodDeclaration>>();
                
        }
        public static List<string> comments(this AstDetails astDetails)
        {            
            if(astDetails.isNotNull() && astDetails.Comments.notNull())
                return (from comment in astDetails.Comments
                        select comment.Text).toList();
            return new List<string>();;
        }
    }
}