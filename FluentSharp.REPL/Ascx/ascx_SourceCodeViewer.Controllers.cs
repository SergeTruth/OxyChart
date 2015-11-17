using FluentSharp.CoreLib.API;

namespace FluentSharp.REPL.Controls
{
    public partial class ascx_SourceCodeViewer
    {
        public SourceCodeEditor getSourceCodeEditor()
        {
            return sourceCodeEditor;
        }

        public void setDocumentContents(string documentContents)
        {            
         //   O2Thread.mtaThread(() =>
           //     {
                    sourceCodeEditor.setDocumentContents(documentContents, "xyz.cs");
         
             //   });
         
        }

        public void setDocumentContents(string documentContents, string file)
        {
            // ToCheckOutLater: I don't really understand why I need to run this of a different thread 
            // (but when developing ascx_O2_Command_Line there was a case where I had an
            // "In setDocumentContents: GapTextBufferStategy is not thread-safe!"
            // error
            O2Thread.mtaThread(() => sourceCodeEditor.setDocumentContents(documentContents, file));
        }
    }
}
