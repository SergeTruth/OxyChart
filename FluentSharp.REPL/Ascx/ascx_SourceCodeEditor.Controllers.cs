// This file is part of the OWASP O2 Platform (http://www.owasp.org/index.php/OWASP_O2_Platform) and is released under the Apache 2.0 License (http://www.apache.org/licenses/LICENSE-2.0)
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using FluentSharp.WinForms;
using FluentSharp.WinForms.Utils;
using FluentSharp.CSharpAST.Utils;
using FluentSharp.CoreLib.API;
using FluentSharp.REPL.Utils;
using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Document;
using FluentSharp.CoreLib;
using O2.External.SharpDevelop.Ascx;
using O2.External.SharpDevelop.ScriptSamples;
using System.Threading;
using ICSharpCode.NRefactory.Ast;
using ICSharpCode.TextEditor.Gui.CompletionWindow;

namespace FluentSharp.REPL.Controls
{
    public partial class SourceCodeEditor
    {        
        public event Action<string>         eFileOpen;
        public event Action<Assembly>       eCompile;
        public event Action<string>         eDocumentDataChanged;
        public event Action<string,string>  eDocumentSelectionChanged_WordAndLine;
        public event MethodInvoker          eEnterInSource_Event;
        //public delegate void documentDataChanged_EventHandler(String sDocumentText);

        // external compilers
        public static Dictionary<string, Func<string, DataReceivedEventHandler, Process>> externalExecutionEngines = new Dictionary<string, Func<string, DataReceivedEventHandler, Process>>();
        
        public bool     runOnLoad = true;
        public int      iLastFoundPosition;
        public long     iMaxFileSize = 500; //  200k
        public String   sDirectoryOfFileLoaded = "";
        public String   sFileToOpen = "";
        public String   sPathToFileLoaded = "";        
        public bool     partialFileViewMode;
        public int      startLine;
        public int      endLine;
        public Assembly compiledAssembly;
        public Thread   autoCompileThread;        
        public bool     allowCodeCompilation = true;        
        public bool     autoBackUpOnCompileSuccess = true;
        public bool     checkForDebugger = false;        
        public INode    currentINode = null;
        public Ast_CSharp_ShowDetailsInViewer   showAstDetails;
        public O2CodeCompletion                 o2CodeCompletion;          
        public O2MappedAstData                  compiledFileAstData = null;
        public Dictionary<string, string>       sampleScripts;
        public List<string>                     partialFileContents = new List<string>();

        public void     setDefaultValues()
        {
            eCompile =  (assembly) => { };
            eFileOpen = (file) => { };
        }
        public void     onLoad()
        {
            if (false == DesignMode && runOnLoad)
            {                
                tbMaxLoadSize.Text = iMaxFileSize.str();
                if (File.Exists(sFileToOpen))
                    loadSourceCodeFile(sFileToOpen);
                else
                {
                    if (sFileToOpen != "")
                        PublicDI.log.error("in SourceCodeEditor .ctor: File provided does not exist: {0}", sFileToOpen);
                }
                configureOnLoadMenuOptions();
                configureDefaultSettingsForTextEditor(tecSourceCode);
                tecSourceCode.ActiveTextAreaControl.TextArea.IconBarMargin.MouseDown += IconBarMargin_MouseDown;
                //        tecSourceCode.ActiveTextAreaControl.TextArea.IconBarMargin.MouseMove += new MarginMouseEventHandler(IconBarMargin_MouseMove);
                //        tecSourceCode.ActiveTextAreaControl.TextArea.IconBarMargin.MouseLeave += new EventHandler(IconBarMargin_MouseLeave);
                //        tecSourceCode.ActiveTextAreaControl.TextArea.KeyPress += TextArea_KeyPress;
                tecSourceCode.ActiveTextAreaControl.TextArea.KeyDown += TextArea_KeyDown;
                tecSourceCode.ActiveTextAreaControl.TextArea.KeyPress += TextArea_KeyPress;
                tecSourceCode.ActiveTextAreaControl.TextArea.KeyUp += TextArea_KeyUp;
                tecSourceCode.ActiveTextAreaControl.TextArea.KeyEventHandler += TextArea_KeyEventHandler;
                tecSourceCode.ActiveTextAreaControl.TextArea.DragEnter += TextArea_DragEnter;
                tecSourceCode.ActiveTextAreaControl.TextArea.DragOver += TextArea_DragOver;
                tecSourceCode.ActiveTextAreaControl.TextArea.DragDrop += TextArea_DragDrop;
                tecSourceCode.ActiveTextAreaControl.SelectionManager.SelectionChanged += SelectionManager_SelectionChanged;
                tecSourceCode.ActiveTextAreaControl.Caret.PositionChanged += SelectionManager_SelectionChanged;

                tecSourceCode.TextEditorProperties.Font = new Font("Courier New", 9, FontStyle.Regular);

                mapExternalExecutionEngines();
                //this.onCaretMove(caretMoved);
                extraGuiChanges();
                runOnLoad = false;
            }
        }
        public void     extraGuiChanges() // starting to move this code into O2's more explicit code
        {
            menuStripForSourceEdition.add_MenuItem("Script Editor")
                                     .add_MenuItem("Open C# REPL Script", () => open.scriptEditor())
                                     .add_MenuItem("REPL this Editor", () => this.script_Me());
            menuStripForSourceEdition.add_MenuItem("Open Parent Folder of current script", () => this.sPathToFileLoaded.parentFolder().startProcess());

            toolStripWithSourceCodeActions.add_Button("Sample C#", OpenSampleCSharpFile);

            
        }

        public void OpenSampleCSharpFile()
        {
            var sampleCode = @"
            ""Hello world"".alert();
            return ""Hello World"";";
            var sampleFile = sampleCode.createCSharpFileFromCodeSnippet();
            loadSourceCodeFile(sampleFile);            
        }
        public void     TextArea_KeyUp(object sender, KeyEventArgs e)
        {
            handlePressedKeys(e);
        }
        public void     handlePressedKeys(KeyEventArgs e)
        {            
           
            if (e.Modifiers == Keys.Control && e.KeyValue == 'B')           // Ctrl+B compiles code
            {
                compileSourceCode();
                PublicDI.log.debug("Control B was pressed");
            }
            else if (e.Modifiers == Keys.Control && e.KeyValue == 'S')           // Ctrl+B saves code
            {
                saveSourceCode();
            }
            else if (e.KeyValue == 116)                                     // F5 (key 116) executes it
            {
                executeMethod();
            }
            else if (o2CodeCompletion.isNull() || o2CodeCompletion.codeCompletionWindow.isNull())                                   // trigger on " " (space)
                this.focus();  //  to deal with the issue that sometimes happened where the cursor would disapear (from the currently GUI under edit)
        }
        public void     TextArea_KeyDown(object sender, KeyEventArgs e)
        {
            //PublicDI.log.debug("KeyDown: " + e.KeyValue.ToString()); ;
        }
        public void     TextArea_KeyPress(object sender, KeyPressEventArgs e)
        {
            //PublicDI.log.debug("KeyPress: " + e.KeyChar.ToString()); ;
        }
        public void     IconBarMargin_MouseDown(AbstractMargin sender, Point mousepos, MouseButtons mouseButtons)
        {
            /*    IconBarMargin margin = (IconBarMargin)sender;
                Graphics g = margin.TextArea.CreateGraphics();
                //g.Clear(Color.Red);
                //g.DrawString("B", new Font("Arial", 16), new SolidBrush(Color.Black), 0, mousepos.Y);
                //margin.DrawBookmark(g, mousepos.Y, true);
                margin.DrawBreakpoint(g, mousepos.Y, true,true);
    */
            //tecSourceCode.Document.BookmarkManager.AddMark(bookmark);
        }
        public void     TextArea_DragOver(object sender, DragEventArgs e)
        {
            Dnd.setEffect(e);
        }
        public void     TextArea_DragDrop(object sender, DragEventArgs e)
        {
            var fileOrFolder = Dnd.tryToGetFileOrDirectoryFromDroppedObject(e);
            if (fileOrFolder.fileExists())
            {
                loadSourceCodeFile(fileOrFolder);
                return;
            }
            var data = Dnd.tryToGetObjectFromDroppedObject(e);
            if (data != null)
            {
                //var dsa = data.GetType().FullName;
                var methods = data as List<MethodInfo>;
                if (methods != null)
                {                    
                    if (methods.Count > 0)
                    {
                        var filteredSignature = new FilteredSignature(methods[0]);
                        tecSourceCode.ActiveTextAreaControl.TextArea.InsertString(filteredSignature.sFunctionNameAndParams);
                        //  var functionSignature = new FilteredSignature
                    }
                }
                else
                    tecSourceCode.ActiveTextAreaControl.TextArea.InsertString(data.ToString());
            }
        }
        public void     TextArea_DragEnter(object sender, DragEventArgs e)
        {
            Dnd.setEffect(e);

        }
        public void     mapExternalExecutionEngines()
        {
            this.invokeOnThread(
                () =>
                {
                    cbExternalEngineToUse.Items.Clear();
                    foreach (var externalEngine in externalExecutionEngines.Keys)
                        cbExternalEngineToUse.Items.Add(externalEngine);
                    if (cbExternalEngineToUse.Items.Count > 0)
                        cbExternalEngineToUse.SelectedIndex = 0;
                });
        }
        public void     configureOnLoadMenuOptions()
        {
            //if (O2Messages.IsGuiLoaded())
            //{
            //btShowLogs.Visible = O2Messages.isGuiLoaded();
            //}
        }
        public void     configureDefaultSettingsForTextEditor(TextEditorControlBase tecTargetTextEditor)
        {
            tecTargetTextEditor.Document.DocumentChanged += Document_DocumentChanged;

            cboxLineNumbers.Checked = true;

            cboxInvalidLines.Checked = true;
            cboxInvalidLines_CheckedChanged(null, null);

            cboxEOLMarkers.Checked = false;
            cboxEOLMarkers_CheckedChanged(null, null);

            cboxHRuler.Checked = false;
            cboxHRuler_CheckedChanged(null, null);

            cboxSpaces.Checked = false;
            cboxSpaces_CheckedChanged(null, null);

            cboxTabs.Checked = false;
            cboxTabs_CheckedChanged(null, null);

            cboxVRuler.Checked = false;
            cboxVRuler_CheckedChanged(null, null);
        }
        public void     loadSourceCodeFileIntoTextEditor(String fileToLoad, TextEditorControlBase tecTargetTextEditor)
        {
            fileToLoad = tryToResolveFileLocation(fileToLoad, this);
            //this.okThreadSync(delegate
            //ExtensionMethods.invokeOnThread((Control)this, () =>
            tecSourceCode.invokeOnThread(
                () =>
                    {
                        try
                        {
                            partialFileViewMode = false;
                            lbPartialFileView.Visible = false;
                            tecSourceCode.Visible = true;
                            long iCurrentFileSize = Files_WinForms.getFileSize(fileToLoad);
                            if (iCurrentFileSize > (iMaxFileSize*1024))
                            {
                                PublicDI.log.error("File to load is too big: max is {0}k, this file is {1}k : {2}",
                                             iMaxFileSize,
                                             iCurrentFileSize/1024, fileToLoad);
                                loadPartialFileView(fileToLoad);
                            }
                            else
                            {                                
                                if (fileToLoad.extension(".h2"))
                                {                                    
                                    setPathToFileLoaded(fileToLoad);
                                    setDocumentContents(H2.load(fileToLoad).SourceCode, fileToLoad, false); 
                                    setDocumentHighlightingStrategy("aa.cs");
                                }
                                else
                                {
                                    tecTargetTextEditor.LoadFile(fileToLoad);
                                }
                                if (fileToLoad.extension(".o2"))
                                {
                                    var realFileTypeToload = Path.GetFileNameWithoutExtension(fileToLoad);
                                    tecSourceCode.Document.HighlightingStrategy =
                                        HighlightingStrategyFactory.CreateHighlightingStrategyForFile(realFileTypeToload);
                                }
                                lbSourceCode_UnsavedChanges.Visible = false;
                                btSaveFile.Enabled = false;
                                eFileOpen(fileToLoad);
                            }
                        }
                        catch (Exception ex)
                        {
                            PublicDI.log.error("in loadSourceCodeFileIntoTextEditor: {0}", ex.Message);
                        }
                        return default(object);
                    });

        }
        public void     loadPartialFileView(string fileToLoad)
        {
            fileToLoad = tryToResolveFileLocation(fileToLoad, this);
            this.invokeOnThread(() =>
                    {
                        partialFileViewMode = true;
                        lbPartialFileView.Visible = true;
                        tecSourceCode.Visible = false;
                        partialFileContents = Files_WinForms.loadLargeSourceFileIntoList(fileToLoad, true);
                        showLinesFromPartialFileContents(0, 1000);
                    });
        }
        public void     showLinesFromPartialFileContents(int start_Line, int end_Line)
        {
            startLine = start_Line;
            endLine = end_Line;
            this.invokeOnThread(() =>
                    {
                        var numberOflinesInCurrentFile = partialFileContents.Count;
                        if (startLine > numberOflinesInCurrentFile)
                        {
                            startLine = numberOflinesInCurrentFile - 1000;
                            if (startLine < 0)
                                startLine = 0;
                        }
                        if (endLine < startLine)
                            endLine = startLine + 1000;
                        if (endLine > numberOflinesInCurrentFile)
                            endLine = numberOflinesInCurrentFile - 1;
                        lbPartialFileView.Items.Clear();
                        for (int i = startLine; i < endLine; i++)
                            lbPartialFileView.Items.Add(partialFileContents[i]);


                    });
        }
        public void     selectLineFromPartialFileContents(uint lineToSelect)
        {
            // handle the special case where 0 is provided as the line to select 
            //  if (lineToSelect == 0)
            //      lineToSelect = 1;
            this.invokeOnThread(() =>
                    {
                        if (startLine < lineToSelect && lineToSelect < endLine)
                        {
                            var selectedIndex = (int)lineToSelect - startLine - 1;
                            if (lbPartialFileView.Items.Count > selectedIndex)
                                lbPartialFileView.SelectedIndex = selectedIndex;
                        }
                        else
                        {
                            var newStartLine = (int)lineToSelect - 500;
                            var newEndLine = (int)lineToSelect + 500;
                            if (newStartLine < 0)
                            {
                                newEndLine += -newStartLine;    // add it to the end
                                newStartLine = 0;               // make it start at 0
                            }
                            showLinesFromPartialFileContents(newStartLine, newEndLine);
                            if (lineToSelect - startLine > 0 && lineToSelect - startLine < endLine)
                                lbPartialFileView.SelectedIndex = (int)lineToSelect - startLine;
                        }
                        tbExecutionHistoryOrLog.Text = string.Format("{0}: {1}{2}", lbPartialFileView.SelectedIndex + 1, lbPartialFileView.Text, Environment.NewLine) +
                                                       tbExecutionHistoryOrLog.Text;

                    });
        }
        public SourceCodeEditor     saveSourceCodeFile(String sTargetLocation)
        {
            return this.invokeOnThread(
                () =>{
                        try
                        {
                            tecSourceCode.SaveFile(sTargetLocation);
                            if (sPathToFileLoaded != null && sPathToFileLoaded != sTargetLocation)
                                sPathToFileLoaded = sTargetLocation;

                            PublicDI.log.info("Source code saved to: {0}", sTargetLocation);
                            tbSourceCode_FileLoaded.Text = Path.GetFileName(sTargetLocation);
                            lbSource_CodeFileSaved.Visible = true;
                            lbSourceCode_UnsavedChanges.Visible = false;
                            btSaveFile.Enabled = false;
                            tecSourceCode.LineViewerStyle = LineViewerStyle.None;
                        }
                        catch (Exception ex)
                        {
                            PublicDI.log.error("in saveSourceCodeFile {0}", ex.Message);
                        }
                        return this;
                    });
        }
        public void     newFile()
        {
            "Creating a new Script".info();
            setPathToFileLoaded(null);
            setDocumentContents("",".cs");
        }
        public bool     loadSourceCodeFile(String pathToSourceCodeFileToLoad)
        {
            if (pathToSourceCodeFileToLoad.notValid())
            {
                newFile();
                return false;
            }

            pathToSourceCodeFileToLoad = tryToResolveFileLocation(pathToSourceCodeFileToLoad, this);

            var fileExtension = Path.GetExtension(pathToSourceCodeFileToLoad).lower();
            if (fileExtension == ".dll" || fileExtension == ".exe" || fileExtension == ".class" || hasNonRendredChars(pathToSourceCodeFileToLoad))
            {
                PublicDI.log.error("Skipping file load due to its extension or contents: {0}", Path.GetFileName(pathToSourceCodeFileToLoad));
                return false;
            }
            return (this.invokeOnThread(() =>
                                 {
                                     try
                                     {
                                         if (File.Exists(pathToSourceCodeFileToLoad))
                                         {
                                             if (sPathToFileLoaded != pathToSourceCodeFileToLoad)
                                             {
                                                 PublicDI.log.debug("Loading File: {0}",
                                                              pathToSourceCodeFileToLoad);
                                                 if (pathToSourceCodeFileToLoad != "")
                                                 {

                                                     //tbSourceCode.Text = files.GetFileContent(config.getDefaultSourceCodeCompilationExampleFile());

                                                     //  tbSourceCode_PathToFileLoaded.Text = sPathToSourceCodeFileToLoad;
                                                     setPathToFileLoaded(pathToSourceCodeFileToLoad);
                                                     
                                                     loadSourceCodeFileIntoTextEditor(pathToSourceCodeFileToLoad, tecSourceCode);

                                                     //compileSourceCode();
                                                     lbSource_CodeFileSaved.Visible = false;
                                                     lbSourceCode_UnsavedChanges.Visible = false;
                                                     btSaveFile.Enabled = false;


           //                                          PublicDI.log.debug("Source code file loaded: {0}", pathToSourceCodeFileToLoad);
                                                     setCompileAndInvokeButtonsState(sPathToFileLoaded);
                                                 }
                                                 return true;
                                             }
                                         }
                                         else
                                             tecSourceCode.Text = "";
                                     }
                                     catch (Exception ex)
                                     {
                                         PublicDI.log.ex(ex, "in loadSourceCodeFile");
                                     }
                                     return false;
                                 }));
        }
        public void     setPathToFileLoaded(string pathToSourceCodeFileToLoad)
        {
            if (pathToSourceCodeFileToLoad.notValid())
            {
                sDirectoryOfFileLoaded = "";
                tbSourceCode_DirectoryOfFileLoaded.set_Text("");
                tbSourceCode_FileLoaded.set_Text("");
                sPathToFileLoaded ="";
            }
            else
                tbSourceCode_DirectoryOfFileLoaded.invokeOnThread(
                    () =>{
                            sDirectoryOfFileLoaded = Path.GetDirectoryName(pathToSourceCodeFileToLoad);
                            tbSourceCode_DirectoryOfFileLoaded.Text = sDirectoryOfFileLoaded;
                            tbSourceCode_FileLoaded.Text = Path.GetFileName(pathToSourceCodeFileToLoad);
                            sPathToFileLoaded = pathToSourceCodeFileToLoad;
                         });
        }
        public bool     hasNonRendredChars(string pathToSourceCodeFileToLoad)
        {
            var fileContents = Files.getFileContents(pathToSourceCodeFileToLoad);
            if (fileContents.Contains("\0"))
                return true;
            return false;
        }
        public void     setCompileAndInvokeButtonsState(string pathToFileLoaded)
        {
            bool supportCSharpCompileAndExecute = true;
            if (pathToFileLoaded.extension().lower() == ".o2")
                pathToFileLoaded = Path.GetFileNameWithoutExtension(pathToFileLoaded);
            switch (Path.GetExtension(pathToFileLoaded))
            {
                case ".h2":
                case ".cs":
                    btExecuteOnExternalEngine.Visible = false;
                    lbExecuteOnEngine.Visible = false;
                    cbExternalEngineToUse.Visible = false;
                    btShowHidePythonLogExecutionOutputData.Visible = false;
                    break;
                default:
                    btExecuteOnExternalEngine.Visible = true;
                    lbExecuteOnEngine.Visible = true;
                    cbExternalEngineToUse.Visible = true;
                    btShowHidePythonLogExecutionOutputData.Visible = true;
                    supportCSharpCompileAndExecute = false;
                    break;

            }
            lbCompileCode.Visible = supportCSharpCompileAndExecute;
            btCompileCode.Visible = supportCSharpCompileAndExecute;
            btDragAssemblyCreated.Visible = supportCSharpCompileAndExecute;
            btSelectedLineHistory.Visible = supportCSharpCompileAndExecute;
            tsCompileStripSeparator.Visible = supportCSharpCompileAndExecute;
        }     
        public String   getSourceCode()
        {
            return tecSourceCode.invokeOnThread(() => tecSourceCode.Text);
            //return (string)this.tecSourceCode.ActiveTextAreaControl.TextArea.invokeOnThread(() => tecSourceCode.Text);

            /*try
            {
                string sourceCode = "";
                var sync = new AutoResetEvent(false);
                if (tecSourceCode.InvokeRequired)
                    tecSourceCode.Invoke(new EventHandler((sender,e) =>
                                                    {
                                                        sourceCode = tecSourceCode.Text;
                                                        sync.Set();
                                                    }));
                else
                    return tecSourceCode.Text;
                sync.WaitOne();
                return sourceCode;
            }

            catch (Exception)
            {
                return "";                
            }  */
        }        
        public string   getFullPathTOCurrentSourceCodeFile()
        {
            var directory = tbSourceCode_DirectoryOfFileLoaded.get_Text();
            var file = tbSourceCode_FileLoaded.get_Text();
            if (directory.valid() && file.valid())
            {
                Files.checkIfDirectoryExistsAndCreateIfNot(directory);
                //return Path.Combine(sDirectoryOfFileLoaded, tbSourceCode_FileLoaded.Text);            
                return Path.Combine(directory, file);
            }
            return "";
        }
        public SourceCodeEditor     saveSourceCode()
        {
            String sFilePath = getFullPathTOCurrentSourceCodeFile();
            PublicDI.CurrentScript = sFilePath;
            if (partialFileViewMode == false && sFilePath.extension(".h2").isFalse())
            {
                // if (lbSourceCode_UnsavedChanges.Visible)
                // {
                
                saveSourceCodeFile(sFilePath);
                if (File.Exists(sFilePath))
                    sPathToFileLoaded = sFilePath;
            }
            else
                new H2(getSourceCode()).save(sFilePath);
            return this;
        }
        public void     highlightLineWithColor(Int32 iLineToSelect, Color cColor)
        {
            //TextEditorControl tecControl = tecSourceCode.ActiveTextAreaControl;            
            //ICSharpCode.TextEditor.Document.LineSegment lsLineSegment = tecSourceCode.Document.GetLineSegment(iLineToSelect);
            //lsLineSegment.
        }
        public void     cleanHighLights()
        {
        }
        public void     gotoLine(Int32 iLineToSelect)
        {
            try
            {
                if (partialFileViewMode)
                    selectLineFromPartialFileContents((uint)iLineToSelect);
                else
                    this.okThreadSync(delegate
                                                            {
                                                                tecSourceCode.LineViewerStyle = LineViewerStyle.FullRow;
                                                                TextAreaControl teaControl =
                                                                    tecSourceCode.ActiveTextAreaControl;
                                                                if (iLineToSelect < 1)
                                                                    teaControl.JumpTo(0);
                                                                else
                                                                    teaControl.JumpTo(iLineToSelect - 1);

                                                                tbExecutionHistoryOrLog.Text =
                                                                    string.Format("{0}: {1}{2}", getSelectedLineNumber(),
                                                                                  getSelectedLineText(),
                                                                                  Environment.NewLine) +
                                                                    tbExecutionHistoryOrLog.Text;
                                                            });
            }
            catch (Exception ex)
            {
                PublicDI.log.error("in SourceCodeEditor.gotoLine: {0}", ex.Message);
            }
        }
        public void     gotoLine(String sPathSourceCodeFile, string lineToSelect)
        {
            try
            {
                gotoLine(sPathSourceCodeFile, Int32.Parse(lineToSelect));
            }
            catch (Exception ex)
            {
                PublicDI.log.error("in gotoLine: {0}", ex.Message);
            }
        }
        public void     gotoLine(String sPathSourceCodeFile, Int32 iLineToSelect)
        {

            if (sPathToFileLoaded != sPathSourceCodeFile)
            // only load if the current file is different from the one already loaded
            {
                // tecSourceCode.Visible = false;   
                loadSourceCodeFile(sPathSourceCodeFile);
                //  tecSourceCode.Visible = true;
            }
            gotoLine(iLineToSelect);

        }       
        public void     searchForTextInTextEditor(string sTextToSearch)
        {
            tecSourceCode.invokeOnThread(
                ()=>{
                        // code snippet from http://owasp-code-central.googlecode.com/svn/trunk/labs/ReportGenerator/ascx/ascxXsltEditor.cs (which I wrote a while back of OWASP Report Generator tool)
                        lbSearch_textNotFound.Visible = false;
                        //int iOriginalTextCaret = tecSourceCode.ActiveTextAreaControl.TextArea.Caret.Offset;
                        var iOffsetOfEndOfCurrentSelection = tecSourceCode.ActiveTextAreaControl.TextArea.Caret.Offset +
                                                             tecSourceCode.ActiveTextAreaControl.TextArea.SelectionManager.
                                                                 SelectedText.Length;
                        var text = tecSourceCode.Text;
                        int iFoundPos = text.lower().index(sTextToSearch.lower(), iOffsetOfEndOfCurrentSelection);
                        //start from the cursor position
                        if (iFoundPos == -1) // didn't find anything
                            iFoundPos = tecSourceCode.Text.lower().index(sTextToSearch.lower(), 0); // start from the top
                        if (iFoundPos > -1) // if there is a match process it
                        {
                            tecSourceCode.ActiveTextAreaControl.TextArea.SelectionManager.ClearSelection();
                            tecSourceCode.ActiveTextAreaControl.TextArea.SelectionManager.SetSelection(
                                new DefaultSelection(tecSourceCode.Document,
                                                     tecSourceCode.Document.OffsetToPosition(iFoundPos),
                                                     tecSourceCode.Document.OffsetToPosition(iFoundPos + sTextToSearch.Length)));

                            tecSourceCode.ActiveTextAreaControl.Caret.Position =
                                tecSourceCode.ActiveTextAreaControl.TextArea.SelectionManager.SelectionCollection[0].StartPosition;
                            tecSourceCode.ActiveTextAreaControl.TextArea.ScrollToCaret();


                            tecSourceCode.ActiveTextAreaControl.TextArea.SelectionManager.FireSelectionChanged();
                        }
                        else
                        {
                            lbSearch_textNotFound.Visible = true;
                            lbSearch_textNotFound.Text = string.Format("Provided search string not found: '{0}'", sTextToSearch);
                        }
                    });
        }       
        public void     searchForTextInTextEditor_findNext(String sTextToSearch)
        {
            tecSourceCode.invokeOnThread(
                ()=>{
                        // code snippet from http://owasp-code-central.googlecode.com/svn/trunk/labs/ReportGenerator/ascx/ascxXsltEditor.cs (which I wrote a while back of OWASP Report Generator tool) 
                        if (iLastFoundPosition > tecSourceCode.Text.Length)
                            iLastFoundPosition = 0;
                        var iFoundPos = tecSourceCode.Text.lower().index(sTextToSearch.lower(), iLastFoundPosition);
                        if (iFoundPos == -1) // try from the begginig
                            iFoundPos = tecSourceCode.Text.lower().index(sTextToSearch.lower(), 0);
                        if (iFoundPos > -1)// & iLastFoundPosition != iFoundPos)
                        {
                            lbSearch_textNotFound.Visible = false;
                            tecSourceCode.ActiveTextAreaControl.TextArea.SelectionManager.ClearSelection();
                            tecSourceCode.ActiveTextAreaControl.TextArea.SelectionManager.SetSelection(
                                new DefaultSelection(tecSourceCode.Document,
                                                     tecSourceCode.Document.OffsetToPosition(iFoundPos),
                                                     tecSourceCode.Document.OffsetToPosition(iFoundPos + sTextToSearch.Length)));

                            tecSourceCode.ActiveTextAreaControl.Caret.Position =
                                tecSourceCode.ActiveTextAreaControl.TextArea.SelectionManager.SelectionCollection[0].StartPosition;

                            tecSourceCode.ActiveTextAreaControl.TextArea.ScrollToCaret();
                            //tecSourceCode.ActiveTextAreaControl.TextArea.SelectionManager.FireSelectionChanged();

                            iLastFoundPosition = iFoundPos + sTextToSearch.Length;
                        }
                        else
                        {
                            lbSearch_textNotFound.Visible = true;
                        }
                    });
        }
        public void     setDirectoryOfFileLoaded(string newPath)
        {
            if (((tbSourceCode_DirectoryOfFileLoaded)).okThread(delegate { setDirectoryOfFileLoaded(newPath); }))
                if (newPath != tbSourceCode_DirectoryOfFileLoaded.Text)
                {
                    tbSourceCode_DirectoryOfFileLoaded.Text = newPath;
                    sDirectoryOfFileLoaded = newPath;
                    lbSourceCode_UnsavedChanges.Visible = true;
                    btSaveFile.Enabled = true;
                    lbSource_CodeFileSaved.Visible = false;
                }
        }
        public void     getAtCaret_WordAndObject(ref String sWord, ref String sObject)
        {
            Int32 iCurrentWord = 0;
            Caret cCaret = tecSourceCode.ActiveTextAreaControl.TextArea.Caret;
            LineSegment lsLineSegment =
                tecSourceCode.ActiveTextAreaControl.TextArea.Document.GetLineSegment(cCaret.Position.Y);
            if (lsLineSegment.Words != null)
            {
                iCurrentWord += lsLineSegment.Words.TakeWhile(twWord => twWord.Offset >= cCaret.Position.X ||
                                                                        cCaret.Position.X >= (twWord.Offset + twWord.Length))
                                                    .Count();

                if (iCurrentWord < lsLineSegment.Words.Count)
                {
                    sWord = lsLineSegment.Words[iCurrentWord].Word;

                    // look before
                    int iIndex = iCurrentWord;
                    while (iIndex > 0)
                        if (lsLineSegment.Words[iIndex].Word == "." ||
                            lsLineSegment.Words[iIndex].Type == TextWordType.Word)
                            sObject = lsLineSegment.Words[iIndex--].Word + sObject;
                        else
                            break;
                    //look after
                    // sObject += "   --  ";
                    iIndex = iCurrentWord + 1;
                    while (iIndex < lsLineSegment.Words.Count)
                        if (lsLineSegment.Words[iIndex].Word != ";" &&
                            (lsLineSegment.Words[iIndex].Word == "." ||
                             lsLineSegment.Words[iIndex].Type == TextWordType.Word))
                            sObject += lsLineSegment.Words[iIndex++].Word;
                        else
                            break;

                    PublicDI.log.debug("Word: {0}     Object:{1}", sWord, sObject);
                }
            }
        }
        public void     setSelectedLineNumber(string fileName, int lineNumber)
        {
            gotoLine(fileName, lineNumber);
        }
        public void     setSelectedLineNumber(int lineNumber)
        {
            gotoLine(lineNumber);
        }
        public int      getSelectedLineNumber()
        {
            return this.invokeOnThread(() => tecSourceCode.ActiveTextAreaControl.Caret.Line + 1);
        }
        public string   getSelectedLineText()
        {
            return  this.invokeOnThread(
                                () =>
                                    {
                                        var currentLine = tecSourceCode.ActiveTextAreaControl.Caret.Line;
                                        var lineSegment = tecSourceCode.ActiveTextAreaControl.TextArea.TextView.Document.GetLineSegment(currentLine);
                                        return
                                            tecSourceCode.ActiveTextAreaControl.TextArea.TextView.Document.GetText(lineSegment.Offset, lineSegment.Length);
                                    });
        }
        public void     executeMethod()
        {
            this.invokeOnThread(
                () => {
                           if (cboxCompliledSourceCodeMethods.SelectedItem != null)
                           {
                               var method = (Reflection_MethodInfo)cboxCompliledSourceCodeMethods.SelectedItem;
                               method.invokeMTA(new object[0]);
                           }
                           return default(object);
                       });
        }
        public void     compileSourceCode()
        {
            if (getSourceCode() != "" && allowCodeCompilation)
                if (partialFileViewMode == false)
                    this.invokeOnThread(() =>
                            {
                                var fileExtention = sPathToFileLoaded.extension().ToLower();
                                if (fileExtention == ".o2")
                                    fileExtention = Path.GetExtension(Path.GetFileNameWithoutExtension(sPathToFileLoaded));
                                //PublicDI.log.info("in compileSourceCode");
                                switch (fileExtention)
                                {
                                    case ".h2":
                                    case ".cs":
                                        compileDotNetCode();
                                        break;
                                    default:
                                        var currentExternalEngine = cbExternalEngineToUse.Text;
                                        //PublicDI.log.info("Going to execute PY");
                                        executeOnExternalEngine(currentExternalEngine);
                                        break;

                                }
                            }
                        );

        }
        public Assembly compileH2File()
        {
            var sourceCode = getSourceCode();
            if (sourceCode != "")
            {				
                saveSourceCode(); // always save before compiling  
                var csharpCompiler = new CSharp_FastCompiler();
                csharpCompiler.set_OnAstFail(()=> "AST Creation for provided source code failed".error());   
                //csharpCompiler.generateDebugSymbols = true;
                csharpCompiler.compileSnippet(sourceCode);
                csharpCompiler.waitForCompilationComplete();
                if (csharpCompiler.compilerResults() != null && csharpCompiler.compilerResults().Errors.Count ==0)
                    return csharpCompiler.compilerResults().CompiledAssembly;
                if (csharpCompiler.compiledAssembly().notNull())
                    return csharpCompiler.compiledAssembly();
            }
            return null;
        }

        public Assembly compileCSSharpFile()
        {
            enableCodeComplete();
            compiledAssembly = null;
            var compileEngine = new CompileEngine();            
            if (getSourceCode() != "")
            {
                saveSourceCode();
                // always save before compiling                                                
                compileEngine.compileSourceFile(sPathToFileLoaded);
                compiledAssembly = compileEngine.compiledAssembly;
                if (compiledAssembly.notNull() && o2CodeCompletion.notNull() && compileEngine.cpCompilerParameters.notNull())
                    o2CodeCompletion.addReferences(compileEngine.cpCompilerParameters.ReferencedAssemblies.toList());  
            }

            var state = compiledAssembly == null && compileEngine.sbErrorMessage != null;
            //btShowHideCompilationErrors.visible(state);
            //btShowHideCompilationErrors.prop("Visible",state);
            btShowHideCompilationErrors.visible(state);
            tvCompilationErrors.visible(state);
            lbCompilationErrors.visible(state);
            //lbCompilationErrors.prop("Visible", state);
                
            

            clearBookmarksAndMarkers();
            // if there isn't a compiledAssembly, show errors
            if (compiledAssembly == null)
            {
                CompileEngine_WinForms.addErrorsListToTreeView(tvCompilationErrors, compileEngine.sbErrorMessage);
                showErrorsInSourceCodeEditor(compileEngine.sbErrorMessage.str());
            }
         /*   else
            {
                if (compiledFileAstData.notNull())
                    compiledFileAstData.Dispose();
                compiledFileAstData = new O2MappedAstData(sPathToFileLoaded);
            }*/
            
            return compiledAssembly;
        }
        public void     compileDotNetCode()
        {
            cboxCompliledSourceCodeMethods.enabled(false);	
            O2Thread.mtaThread (
                () =>
                {
                    try
                    {													
                        //start the compilation in a separate thread
                        compiledAssembly = sPathToFileLoaded.extension(".h2") ? compileH2File() : compileCSSharpFile();
                        // then continue on the gui thread

                        this.invokeOnThread(() =>
                           {
                               // set gui options depending on compilation result
                               var assemblyCreated = compiledAssembly != null;

                               //btDebugMethod.Visible = 
                               executeSelectedMethodToolStripMenuItem.Visible =
                                               btDragAssemblyCreated.Visible = 
                                               btExecuteSelectedMethod.Visible =
                                               lbExecuteCode.Visible =
                                               cboxCompliledSourceCodeMethods.Visible = 
                                               assemblyCreated;

                               cboxCompliledSourceCodeMethods.Items.Clear();
                               if (compiledAssembly != null)
                               {
                                   cboxCompliledSourceCodeMethods.Enabled = true;
                                   autoBackup();
                                   var previousExecutedMethod = cboxCompliledSourceCodeMethods.Text;
                                   eCompile(compiledAssembly);
                                   O2Messages.dotNetAssemblyAvailable(compiledAssembly.Location);
                                   //only show the first ten
                                   foreach (var method in PublicDI.reflection.getMethods(compiledAssembly)
                                                                             .Where((method) => (false == method.IsAbstract && false == method.IsSpecialName))
                                                                             .Take(5))
                                   {
                                       cboxCompliledSourceCodeMethods.Items.Add(new Reflection_MethodInfo(method));
                                   }
                                   // remap the previously executed method
                                   if (cboxCompliledSourceCodeMethods.Items.Count > 0)
                                   {
                                       foreach (var method in cboxCompliledSourceCodeMethods.Items)
                                           if (method.ToString() == previousExecutedMethod)
                                           {
                                               cboxCompliledSourceCodeMethods.SelectedItem = method;
                                           }
                                       cboxCompliledSourceCodeMethods.SelectedIndex = 0;
                                   }
                                   cboxCompliledSourceCodeMethods.enabled();
                               }
                               refresh();
                           });
                        //tecSourceCode.Refresh();
                    }
                    catch (Exception ex)
                    {
                        PublicDI.log.error("in compileDotNetCode:{0}", ex.Message);
                    }
                });
        }        
        public void     autoBackup()
        {
            if (autoBackUpOnCompileSuccess)
            {
                var code = getSourceCode();
                PublicDI.config.AutoSavedScripts.createDir();    // make sure it exits
                var extension = sPathToFileLoaded.extension();
                if (extension.valid().isFalse())
                    extension = ".cs";
                var targetFile = PublicDI.config.AutoSavedScripts.pathCombine("code_Editor_" + Files.getFileSaveDateTime_Now().trim() + extension);
                if (extension == ".h2")
                    new H2(code).save(targetFile);
                else
                    targetFile.fileWrite(code);
            }
        }
        public void     showErrorsInSourceCodeEditor(string errorMessages)
        {
            String[] sSplitedErrorMessage = errorMessages.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string sSplitMessage in sSplitedErrorMessage)
            {
                string[] sSplitedLine = sSplitMessage.Split(new[] { "::" }, StringSplitOptions.None);
                if (sSplitedLine.Length == 5)
                {
                    if (sPathToFileLoaded.ToLower() == sSplitedLine[4].ToLower())            // only show errors for the loaded file
                    {
                        int iLine = Int32.Parse(sSplitedLine[0]);
                        int iColumn = Int32.Parse(sSplitedLine[1]);
                        setSelectedText(iLine, iColumn, true);
                    }
                    //O2Messages.fileErrorHighlight(sSplitedLine[4], iLine, iColumn);                    
                }
            }
        }        
        public  void    executeOnExternalEngine(string engineToUse)
        {
            //PublicDI.log.info("executing PY");
            /*if (!File.Exists(ironPhytonExe))
                PublicDI.log.error("Count not find Iron Phyton executable: {0}", ironPhytonExe);
            else
            {*/
            saveSourceCode();
            // make the log visible
            tbExecutionHistoryOrLog.Visible = true;
            tbExecutionHistoryOrLog.Text =
                "****************************************************************************************".line() +
                "O2 Message: Executing External Engine script: {0}".format(sPathToFileLoaded.fileName()).line() +
                "****************************************************************************************".line().line();
            // execute script

            if (externalExecutionEngines.ContainsKey(engineToUse))
            {
                if (externalExecutionEngines[engineToUse] != null)
                    externalExecutionEngines[engineToUse](sPathToFileLoaded, externalEngineExecutionLogCallback);
            }
            else
                writeMessageTo_ExecutionHistoryOrLog(string.Format("COULD NOT EXECUTE SCRIPT. Engine Not supported: {0}", engineToUse));
            /*switch(engineToUse)
            {
                case "IronPython":
                    IronPythonExec.executePythonFile(sPathToFileLoaded, pythonExecutionLogCallback);
                    break;
                case "Jython":
                    JythonExec.executePythonFile(sPathToFileLoaded, pythonExecutionLogCallback);
                    break;
                case "CPython":
                    CPythonExec.executePythonFile(sPathToFileLoaded, pythonExecutionLogCallback);
                    break;
                default:
                        
                    break;
            }*/
            //Processes.startProcessAsConsoleApplication(ironPhytonExe, sPathToFileLoaded, pythonExecutionLogCallback);
            //}

        }
        public  void    externalEngineExecutionLogCallback(object sender, DataReceivedEventArgs e)
        {
            writeMessageTo_ExecutionHistoryOrLog(e.Data);
        }
        public void     writeMessageTo_ExecutionHistoryOrLog(string logMessage)
        {
            if (!string.IsNullOrEmpty(logMessage))
            {
                this.invokeOnThread(() => tbExecutionHistoryOrLog.AppendText(logMessage + Environment.NewLine));
                PublicDI.log.info("\t:{0}:", logMessage);
            }
        }
        public void     showLogViewerControl()
        {
            var tabControl = this.parent<TabControl>();
            if (tabControl.notNull())
                tabControl.insert_LogViewer();
            else
                this.insert_LogViewer();
            /*int splitterLocation = (int)(150); 
            var logViewer = this.add_Control<ascx_LogViewer>();
            this.insert_Right(logViewer, splitterLocation);
            btShowLogs.visible(false);            */
        }
        public void     hideButton(string buttonId)
        {
            (this.field(buttonId) as ToolStripButton).visible(false);
        }        
        public void     loadSampleScripts()
        {
            loadSampleScripts(typeof(O2SampleScripts));
        }
        public void     loadSampleScripts(object resourcesObjectWithSampleScripts)
        {
            this.invokeOnThread(
                () => {
                           sampleScripts = SampleScripts.getDictionaryWithSampleScripts(resourcesObjectWithSampleScripts);
                           cBoxSampleScripts.Items.Clear();
                           foreach (var scriptName in sampleScripts.Keys)
                               cBoxSampleScripts.Items.Add(scriptName);
                           if (cBoxSampleScripts.Items.Count > 0)
                           {
                               cBoxSampleScripts.SelectedIndex = 0;
                               lbSampleScripts.Visible = true;
                               cBoxSampleScripts.Visible = true;
                           }
                           return default(object);
                       });

            /*    O2Forms.newTreeNode(tvSampleScripts.Nodes, scriptName, 1, sampleScripts[scriptName]);

            if (tvSampleScripts.Nodes.Count > 0)
                tvSampleScripts.SelectedNode = tvSampleScripts.Nodes[0];*/
        }
        public void     loadSampleScript(string scriptToLoad)
        {
            if (scriptToLoad != "" && sampleScripts.ContainsKey(scriptToLoad))
            {
                var tempScriptFileName = PublicDI.config.TempFileNameInTempDirectory + "_" + Path.GetFileName(scriptToLoad);
                Files.WriteFileContent(tempScriptFileName, sampleScripts[scriptToLoad]);
                loadSourceCodeFile(tempScriptFileName);
                tecSourceCode.Focus();
                //compileSourceCode();
            }
        }
        public void     setMaxLoadSize(string newMaxLoadSize)
        {
            try
            {
                iMaxFileSize = Int32.Parse(newMaxLoadSize);
            }
            catch (Exception ex)
            {
                PublicDI.log.ex(ex, "in setMaxLoadSize");
            }

        }
        public void     showSelectedErrorOnSourceCodeFile()
        {
            var selectedError = getSelectedError();
            if (selectedError == "")
                return;
            PublicDI.log.info(selectedError);

            string[] sSplitedLine = selectedError.Split(new[] { "::" }, StringSplitOptions.None);
            if (sSplitedLine.Length == 5)
            {
                int iLine = Int32.Parse(sSplitedLine[0]);
                gotoLine(iLine);
            }
        }
        public string   getSelectedError()
        {
            if (tvCompilationErrors.SelectedNode != null)
                return tvCompilationErrors.SelectedNode.Text;
            return "";
        }
        public ToolStrip getToolStrip()
        {
            return toolStripWithSourceCodeActions;
        }
        public object   GetCurrentCSharpObjectModel()
        {
            var timer = new O2Timer("Calculated O2 Object Model for referencedAssesmblies").start();
            var signatures = new List<string>();
            var referencedAssemblies = new CompileEngine().getListOfReferencedAssembliesToUse();

            //compileEngine.lsGACExtraReferencesToAdd();
            foreach (var referencedAssesmbly in referencedAssemblies)
                if (File.Exists(referencedAssesmbly))
                    signatures.AddRange(PublicDI.reflection.getMethods(referencedAssesmbly)
                              .Select(method => new FilteredSignature(method).sSignature));
            timer.stop();
            return signatures;
        }
        public void     reloadCurrentFile()
        {
            var currentLoadedFile = sPathToFileLoaded;// getFullPathTOCurrentSourceCodeFile();
            if (File.Exists(currentLoadedFile))
            {
                PublicDI.log.info("reloading file: {0}", currentLoadedFile);
                sPathToFileLoaded = ""; // reset this value (since that would prevent this file from being opened again
                loadSourceCodeFile(currentLoadedFile);
            }
        }
        public void     openFile()
        {
            PublicDI.log.info("Select file to open");
            var openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                PublicDI.log.info("Loading file: {0}", openFileDialog.FileName);
                loadSourceCodeFile(openFileDialog.FileName);
            }
        }
        public void     setDocumentContents(string documentContents)
        {
            setDocumentContents(documentContents, "", true);
        }
        public void     setDocumentContents(string documentContents, string fileNameOrExtension)
        {
            setDocumentContents(documentContents, fileNameOrExtension, true);
        }        
        public void     setDocumentHighlightingStrategy(string fileNameOrExtension)
        {            
            this.invokeOnThread(
                    () => tecSourceCode.Document.HighlightingStrategy =
                                HighlightingStrategyFactory.CreateHighlightingStrategyForFile(fileNameOrExtension));

        }
        public void     setDocumentContents(string documentContents, string fileNameOrExtension, bool clearFileLocationValues)
        {

            tecSourceCode.ActiveTextAreaControl.TextArea.invokeOnThread(
                    () =>
                    {
                        try
                        {
                            tecSourceCode.Document.TextContent = documentContents;
                            tecSourceCode.Document.HighlightingStrategy = HighlightingStrategyFactory.CreateHighlightingStrategyForFile(fileNameOrExtension);
                            if (clearFileLocationValues)
                            {
                                sPathToFileLoaded = "";
                                tbSourceCode_DirectoryOfFileLoaded.Text = "";
                                tbSourceCode_FileLoaded.Text = "";
                            }
                            tecSourceCode.ActiveTextAreaControl.Refresh();
                        }
                        catch (Exception ex)
                        {
                            PublicDI.log.error("In setDocumentContents: {0}", ex.Message);
                        }
                    });
        }
        public void     setSelectedText(int line, int column, bool showAsError)
        {
            setSelectedText(line, column, showAsError, true);
        }
        public void     setSelectedText(int line, int column, bool showAsError, bool showAsBookMark)
        {
            setSelectedText(line, column, showAsError, showAsBookMark, true);
        }
        public void     setSelectedText(int line, int column, bool showAsError, bool showAsBookMark, bool decrementLineAndColumn)
        {
            this.invokeOnThread(
                () =>
                    {
                        if (decrementLineAndColumn)
                        {
                            line--; // since the first line is at 0
                            column--; // same for column
                        }
                        //var text = tecSourceCode.ActiveTextAreaControl.TextArea.Text;
                        var location = new TextLocation(column, line);
                        var bookmark = new Bookmark(tecSourceCode.Document, location);
                        if (showAsBookMark)
                            tecSourceCode.Document.BookmarkManager.AddMark(bookmark);

                        if (showAsError)
                        {
                            var offset = bookmark.Anchor.Offset;
                            var length = bookmark.Anchor.Line.Length - column;
                            var newMarker = new TextMarker(offset, length, TextMarkerType.WaveLine);
                            tecSourceCode.Document.MarkerStrategy.AddMarker(newMarker);
                        }
                        refresh();                        
                    });
            // tecSourceCode.ActiveTextAreaControl.Refresh();
        }
        public void     refresh()
        {
            this.invokeOnThread(
                () =>
                    {
                        try
                        {
                            tecSourceCode.Document.RequestUpdate(new TextAreaUpdate(TextAreaUpdateType.WholeTextArea));
                            tecSourceCode.Document.CommitUpdate();
                        }
                        catch (Exception ex)
                        {
                            ex.log("in SourceCodeEditor.refresh");
                        }
                    });
        }
        public void     clearBookmarksAndMarkers()
        {
            tecSourceCode.invokeOnThread(() =>
            {
                tecSourceCode.Document.BookmarkManager.Clear();
                clearMarkers();
            });
        }
        public void     clearMarkers()
        {
            tecSourceCode.invokeOnThread(() =>
            {
                var textMarkers = new List<TextMarker>(tecSourceCode.Document.MarkerStrategy.TextMarker);
                foreach (var textMarker in textMarkers)
                    tecSourceCode.Document.MarkerStrategy.RemoveMarker(textMarker);
                refresh();
            });
        }
        public void     openO2ObjectModel()
        {
            //O2Messages.openControlInGUI(typeof(O2ObjectModel), O2DockState.Float, "O2 Object Model");
            O2Thread.mtaThread(()=>open.o2ObjectModel());
        }
        public void     setAutoCompileStatus(bool state)
        {
            if (autoCompileThread != null && state == false)
                autoCompileThread.Abort();
            else
                if (autoCompileThread == null && state)
                {
                    autoCompileThread = O2Thread.mtaThread(
                        () =>{
                                PublicDI.log.debug("Starting auto compile loop");
                                while (true)
                                {
                                    compileSourceCode();
                                    Processes.Sleep(10000, true);
                                }                                
                            // ReSharper disable FunctionNeverReturns
                            });
                            // ReSharper restore FunctionNeverReturns

                    autoCompileThread.Priority = ThreadPriority.Lowest;
                }

        }
        public void     addBreakpointOnCurrentLine()
        {
            var currentLine = tecSourceCode.ActiveTextAreaControl.Caret.Line;
            var lineSegment = tecSourceCode.ActiveTextAreaControl.TextArea.TextView.Document.GetLineSegment(currentLine);
            O2Messages.raiseO2MDbg_SetBreakPointOnFile(sPathToFileLoaded, lineSegment.LineNumber + 1);
        }
        public void     createStandAloneExeAndDebugMethod(string loadDllsFrom)
        {
            this.invokeOnThread(
                () =>{
                        var selectedItem = cboxCompliledSourceCodeMethods.SelectedItem as Reflection_MethodInfo;
                        if (selectedItem != null)                    
                        selectedItem.raiseO2MDbgDebugMethodInfoRequest(loadDllsFrom);                    
                     });
        }
        public string   tryToResolveFileLocation(string fileToMap, Control hostControl)
        {            
            if (autoTryToFixSourceCodeFileReferences && false == File.Exists(fileToMap) && false == Directory.Exists(fileToMap))
            {
                return SourceCodeMappingsUtils.mapFile(fileToMap, hostControl);
            }
            return fileToMap;
        }

        public List<String>             getSampleScriptsNames()
        {
            return (sampleScripts != null) ? new List<string>(sampleScripts.Keys) : new List<string>();
        }
        public TextEditorControl        getObject_TextEditorControl()
        {
            return tecSourceCode;
        }
        public ICompletionDataProvider enableCodeComplete()
        {
            return o2CodeCompletion ?? (o2CodeCompletion = new O2CodeCompletion(tecSourceCode));
        }

        public static void  addExternalEngine(string engineName, Func<string, DataReceivedEventHandler, Process> engineExecuteFunction)
        {
            externalExecutionEngines.Add(engineName, engineExecuteFunction);
        }
        public static void  listinLogViewCurrentAssemblyReferencesAutomaticallyAdded()
        {
            PublicDI.log.debug(StringsAndLists.fromStringList_getText(new CompileEngine().getListOfReferencedAssembliesToUse()));
        }                
        public static bool  autoTryToFixSourceCodeFileReferences = true;        

                // reason from commment:
        // the idea with this method was to show information about the current carret position, but as it is 
        // it is expensive and doesn't add a lot of value 
        /*        void caretMoved(Caret caret)
                {
        

                    // same code as the one in "public static INode iNode(this O2MappedAstData o2MappedAstData, string file, Caret caret)" in O2MappedAstData_ExtensionMethods.cs (O2 Script)
                    var o2MappedAstData = compiledFileAstData;
                    var file = sPathToFileLoaded;

                    if (o2MappedAstData != null && file != null && caret != null)
                    {
                        if (o2MappedAstData.FileToINodes.hasKey(file))
                        {
                            var allINodes = o2MappedAstData.FileToINodes[file];
                            var adjustedLine = caret.Line + 1;
                            var adjustedColumn = caret.Column + 1;
                            CurrentINode = allINodes.getINodeAt(adjustedLine, adjustedColumn);
                            if (CurrentINode != null)                    
                                lbCurrentAstNode.set_Text("Current Ast Node: {0}".format(CurrentINode.typeName()));                                                                    
                        }
                
                    }
                    if (compiledFileAstData.notNull())
                    {
                        //var iNode = compiledFileAstData.iNode(sPathToFileLoaded, caret);
                        //if (iNode != null)	
                        //{					
                            //CurrentINode = iNode;
                            //lbCurrentAstNode.set_Text("Current Ast Node: {0}".format(iNode.typeName()))
                        //}
                    }
  
                }
                */

    }
}
