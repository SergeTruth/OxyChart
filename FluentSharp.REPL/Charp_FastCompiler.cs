using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Threading;
using FluentSharp.AST;
using FluentSharp.CSharpAST.Utils;
using FluentSharp.CoreLib.API;
using ICSharpCode.NRefactory.Ast;
using ICSharpCode.NRefactory;
using FluentSharp.CoreLib;
using FluentSharp.CSharpAST;
using System.Reflection;

namespace FluentSharp.REPL.Utils
{
    
    public class CSharp_FastCompiler
    {
        public CSharp_FastCompiler_CompilerOptions   CompilerOptions   {  get; set; }
        public CSharp_FastCompiler_CompilerArtifacts CompilerArtifacts {  get; set; }
        public CSharp_FastCompiler_ExecutionOptions  ExecutionOptions  {  get; set; }
        public CSharp_FastCompiler_Events            Events            {  get; set; }

        public int forceAstBuildDelay = 0;
        
		public bool   DebugMode                 { get ; set;}
		
		public Stack<string> _createAstStack = new Stack<string>();
        public Stack<string> _compileStack = new Stack<string>();		

		public bool          _creatingAst;
		public bool          _compiling;
                
        public ManualResetEvent FinishedCompilingCode { get;set;}

        static CSharp_FastCompiler()
        {
            setDefaultUsingStatements();
            setDefaultReferencedAssemblies();
            
        }
        public CSharp_FastCompiler()
        {        
        	DebugMode             = false;				        // set to true to see details about each AstCreation and Compilation stage
            FinishedCompilingCode = new ManualResetEvent(true);            

            CompilerOptions       = new CSharp_FastCompiler_CompilerOptions ();
            ExecutionOptions      = new CSharp_FastCompiler_ExecutionOptions();
            CompilerArtifacts     = new CSharp_FastCompiler_CompilerArtifacts();
            Events                = new CSharp_FastCompiler_Events           (); 
        }
        
        public static void setDefaultUsingStatements()
        {
            CompileEngine.DefaultUsingStatements
                            .add_OnlyNewItems("FluentSharp.WinForms",
                                              "FluentSharp.WinForms.Controls",
                                              "FluentSharp.WinForms.Interfaces",
                                              "FluentSharp.WinForms.Utils",
                                              "FluentSharp.REPL",
                                              "FluentSharp.REPL.Controls",
                                              "FluentSharp.REPL.Utils");                                     
        }
        public static void setDefaultReferencedAssemblies()
        {
            CompileEngine.DefaultReferencedAssemblies
                            .add_OnlyNewItems("FluentSharp.WinForms.dll",
                                              "FluentSharp.REPL.exe",
											  "FluentSharp.SharpDevelopEditor.dll");          // required or some scripts that don't need this will still need it to compile (due to FluentSharp.REPL use of it)
                                              
        }    

		public Dictionary<string,object> getDefaultInvocationParameters()
		{
			return new Dictionary<string, object>();
		}              
        public void compileSnippet(string codeSnippet)
        {
            try
            {
                if (codeSnippet.valid())
                {
			//		if (getCachedAssemblyForCode_and_RaiseEvents(codeSnippet))
			//			return;
					
                    FinishedCompilingCode.Reset();
                    _createAstStack = new Stack<string>();
                    //      createAstStack.Clear();
                    if (_createAstStack.Count == 0)
                        _creatingAst = false;
                    _createAstStack.Push(codeSnippet);
                    compileSnippet();
                }
            }
            catch (Exception ex)
            {
                ex.log("in compileSnippet");
            }
        }
        public bool removeCachedAssemblyForCode(string codeSnippet)
        {
            if (codeSnippet.notValid())
            {
                "in removeCachedAssemblyforCode, there was no codeSnippet passed".error();
                return false;
            }
            var filesMd5 = codeSnippet.md5Hash();
            return CompileEngine.removeCachedAssemblyForCode_MD5(filesMd5);
        }
		public bool getCachedAssemblyForCode_and_RaiseEvents(string codeSnippet)
		{
            if (codeSnippet.notValid())
                return false;
			var filesMd5 = codeSnippet.md5Hash();
			var cachedCompilation = CompileEngine.getCachedCompiledAssembly_MD5(filesMd5);
			if (cachedCompilation.notNull())
			{
				this.compiledAssembly(cachedCompilation);

				if (CompileEngine.loadReferencedAssembliesIntoMemory(this.compiledAssembly()))
				{
					this.raise_OnCompileOK();				
					FinishedCompilingCode.Set();
					return true;
				}
			}
			return false;
		}
        public void compileSnippet()
        {
            O2Thread.mtaThread(
                () =>
                {
                    if (_creatingAst == false && _createAstStack.Count > 0)
                    {
                        _creatingAst = true;
                        var codeSnippet = _createAstStack.Pop();
                        this.sleep(forceAstBuildDelay, DebugMode);            // wait a bit to allow more entries to be cleared from the stack
                        if (_createAstStack.Count > 0)
                            codeSnippet = _createAstStack.Pop();

                        _createAstStack.Clear();

                        this.invocationParameters(getDefaultInvocationParameters());
                        this.raise_BeforeSnippetAst();
                        DebugMode.ifInfo("Compiling Source Snippet (Size: {0})", codeSnippet.size());
                        var sourceCode = createCSharpCodeWith_Class_Method_WithMethodText(codeSnippet);
                        if (this.useCachedAssemblyIfAvailable() && getCachedAssemblyForCode_and_RaiseEvents(sourceCode))   // see if we have already compiled this snippet before
                            return;
                        if (sourceCode != null)
                            compileSourceCode(sourceCode, this.createdFromSnippet());
                        else
                            FinishedCompilingCode.Set();
                        _creatingAst = false;
                        //compileSnippet();                 // this was there to try to see if the current in the editor was compiled (this should be detected in a different way)
                    }
                });
        }
        private void compileSourceCode(string sourceCode, bool createdFromSnippet)
        {
            this.createdFromSnippet(createdFromSnippet);
            _compileStack.Push(sourceCode);
            compileSourceCode();
        }       
        public void compileSourceCode(string sourceCode)
        {
            if (sourceCode.valid().isFalse())
            {
                "in CSharp_FastCompiler,compileSourceCode, provided sourceCode code was empty".error();
                this.raise_OnCompileFail();
            }
            else
            {
				if (getCachedAssemblyForCode_and_RaiseEvents(sourceCode))
					return;
                // we need to do make sure we include any extra references included in the code
                var astCSharp = new Ast_CSharp(sourceCode);
                astCSharp.mapCodeO2References(CompilerOptions);
                compileSourceCode(sourceCode, false);
            }
       	}       	
       	public void compileSourceCode()
       	{
       	    O2Thread.mtaThread(compileSourceCode_Thread );
        }

        //so that we can do EnC
        public void compileSourceCode_Thread()
        {
            try
            {
                if (_compiling == false && _compileStack.Count > 0)
                {
                    _compiling = true;                    
                    FinishedCompilingCode.Reset();
                    compileExtraSourceCodeReferencesAndUpdateReferencedAssemblies();                    
                    var sourceCode = _compileStack.Pop();
                    _compileStack.Clear();                                  // remove all previous compile requests (since their source code is now out of date

                    compileSourceCode_Sync(sourceCode);

                    _compiling = false;
                    FinishedCompilingCode.Set();
                    compileSourceCode();                    
                }
            }
            catch (Exception ex)
            {
                ex.log("in compileSourceCode");
                _compiling = false;
                FinishedCompilingCode.Set();                
            }
        }

        public Assembly compileSourceCode_Sync(string sourceCode)
        {
            if (sourceCode.notValid())
                return null;
            try
            {
                Environment.CurrentDirectory = PublicDI.config.CurrentExecutableDirectory;
                this.compiledAssembly(null);
                this.raise_BeforeCompile();
                DebugMode.ifInfo("Compiling Source Code (Size: {0})", sourceCode.size());
                this.sourceCode(sourceCode);

                if (sourceCode.lines().starting("//CLR_3.5").notEmpty()) // allow setting compilation into 2.0 CLR
                {
                    this.compilationVersion("v3.5");
                }

                var providerOptions = new Dictionary<string, string>().add("CompilerVersion", this.compilationVersion());

                var csharpCodeProvider = new Microsoft.CSharp.CSharpCodeProvider(providerOptions);
                var compilerParams = new CompilerParameters
                    {
                        OutputAssembly = "_o2_Script.dll".tempFile(),
                        IncludeDebugInformation = CompilerOptions.generateDebugSymbols,
                        GenerateInMemory = !CompilerOptions.generateDebugSymbols
                    };

                foreach (var referencedAssembly in CompilerOptions.ReferencedAssemblies)
                    compilerParams.ReferencedAssemblies.Add(referencedAssembly);

                this.compilerResults((this.generateDebugSymbols()) ? csharpCodeProvider.CompileAssemblyFromFile  (compilerParams,sourceCode.saveWithExtension(".cs"))
                                                                   : csharpCodeProvider.CompileAssemblyFromSource(compilerParams, sourceCode));

                if (this.compilerResults().Errors.Count > 0 || this.compilerResults().CompiledAssembly == null)
                {
                    this.compilationErrors("");
                    foreach (CompilerError error in this.compilerResults().Errors)
                    {
                        //CompilationErrors.Add(CompilationErrors.line(error.ToString());
                        var errorMessage = String.Format("{0}::{1}::{2}::{3}::{4}", error.Line,
                                                         error.Column, error.ErrorNumber,
                                                         error.ErrorText, error.FileName);
                        this.compilationErrors(this.compilationErrors().line(errorMessage));
                        "[CSharp_FastCompiler] Compilation Error: {0}".error(errorMessage);
                    }
                    DebugMode.ifError("Compilation failed");
                    this.raise_OnCompileFail();
                }
                else
                {
                    this.compiledAssembly(this.compilerResults().CompiledAssembly);
                    if (this.compiledAssembly().Location.fileExists())
                        CompileEngine.setCachedCompiledAssembly_toMD5(sourceCode, this.compiledAssembly());
                    DebugMode.ifDebug("Compilation was OK");
                    this.raise_OnCompileOK();                    
                }
                return this.compiledAssembly();
            }
            catch (Exception ex)
            {
                ex.log("[compileSourceCode_Sync");
                return null;
            }
        }
        // we need to use CompileEngine (which is slower but supports multiple file compilation 
        public void compileExtraSourceCodeReferencesAndUpdateReferencedAssemblies()
        {            
            if (CompilerOptions.ExtraSourceCodeFilesToCompile.size() > 0)
            {
                "[CSharp Compiler] Compiling provided {0} external source code references".info(CompilerOptions.ExtraSourceCodeFilesToCompile.size());
                var assembly = new CompileEngine(this.useCachedAssemblyIfAvailable()).compileSourceFiles(CompilerOptions.ExtraSourceCodeFilesToCompile);                
                if (assembly != null)
                {
                    
                    CompilerOptions.ReferencedAssemblies.Add(assembly.Location);
                    CompileEngine.setCachedCompiledAssembly(CompilerOptions.ExtraSourceCodeFilesToCompile, assembly);
                    CompilerOptions.generateDebugSymbols = true;                // if there are extra assemblies we can't generate the assembly in memory                    
                }
            }
        }
        /*public string getAstErrors(string sourceCode)
        {
            return new Ast_CSharp(sourceCode).Errors;
        }*/
        public string createCSharpCodeWith_Class_Method_WithMethodText(string code)
        {                        
            //var parsedCode = TextToCodeMappings.tryToFixRawCode(code, tryToCreateCSharpCodeWith_Class_Method_WithMethodText);            

            var parsedCode = tryToCreateCSharpCodeWith_Class_Method_WithMethodText(code);
        
            if (parsedCode == null)
            {
                DebugMode.ifError("Ast parsing Failed");
                this.raise_OnAstFail();
            }
            return parsedCode;
        }
        ///                
        /// <summary>
        /// Returns a compilable C# file from and Snippet
        /// 
        /// Dev Note:this code needs to be refactored (since it is too big and complex)
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public string tryToCreateCSharpCodeWith_Class_Method_WithMethodText(string code)
        {
            if (code.empty())
                return null;
			code = code.line();	// make sure there is an empty line at the end            
                      
            try
            {
                //handle special incudes in source code
                var lines = code.fix_CRLF().lines();
                foreach (var originalLine in lines)
                {
                    var line = originalLine;                    
                    if(originalLine.starts("//O2Include:"))
                    {
                        var includeText = line.subString_After("//O2Include:");
                        var file = includeText;
                        var baseFile = CompilerOptions.SourceCodeFile ?? PublicDI.CurrentScript;
                        var parentFolder = baseFile.parentFolder();
                        if (parentFolder.notValid())
                            "[CSharpFastCompiled] in O2Include mapping, could not get parent folder of current script".error();
                        var resolvedFile = CompileEngine.findFileOnLocalScriptFolder(file,parentFolder);
                        if (resolvedFile.fileExists())
                        {
                            var fileContents = resolvedFile.contents();
                            code = code.Replace(line, line.line().add(fileContents).line());
                        }
                        else
                            "[CSharpFastCompiled] in O2Include mapping, could not a mapping for: {0}".error(includeText);
                    };
                }

                var snippetParser = new SnippetParser(SupportedLanguage.CSharp);
                
                var parsedCode = snippetParser.Parse(code);
				this.astErrors(snippetParser.errors());

                this.compilationUnit(new CompilationUnit());

                if (parsedCode is BlockStatement || parsedCode is CompilationUnit)
                {
                    Ast_CSharp astCSharp;
                    if (parsedCode is BlockStatement)
                    {
                        // map parsedCode into a new type and method 
                        var blockStatement = (BlockStatement)parsedCode;

                        var csharpCode = blockStatement.ast_CSharp_CreateCompilableClass(snippetParser, code,
                                                                                         CompilerOptions, CompilerArtifacts, ExecutionOptions);                        

                        this.astDetails(new Ast_CSharp(csharpCode).AstDetails);
                        this.createdFromSnippet(true);
                        DebugMode.ifDebug("Ast parsing was OK");
                        this.sourceCode(csharpCode);
                        this.raise_OnAstOK();                        
                        
                        return csharpCode;
                    }
                    
                    
                    this.compilationUnit((CompilationUnit)parsedCode);
                    if (this.compilationUnit().Children.Count == 0)
                        return null;

                    astCSharp = new Ast_CSharp(this.compilationUnit(), snippetParser.Specials);
                    // add the comments from the original code                        

                    astCSharp.mapCodeO2References(CompilerOptions);
                    this.createdFromSnippet(false);
                    
                    // create sourceCode using Ast_CSharp & AstDetails		
                    if(this.compilationUnit().Children.Count > 0)
                    {                        
                        // reset the astCSharp.AstDetails object        	
                        astCSharp.mapAstDetails();
                        // add the comments from the original code
                        astCSharp.ExtraSpecials.AddRange(snippetParser.Specials);	                 

	                    this.sourceCode(astCSharp.AstDetails.CSharpCode);

                        //once we have the created SourceCode we need to create a new AST with it
                        var tempAstDetails = new Ast_CSharp(this.sourceCode()).AstDetails;
                        //note we should try to add back the specials here (so that comments make it to the generated code
                        this.astDetails(tempAstDetails);
	                    DebugMode.ifDebug("Ast parsing was OK");
                        this.raise_OnAstOK();	                    
	                    return this.sourceCode();
                    }
                }            
            }
            catch (Exception ex)
            {                            	
				DebugMode.ifError("in createCSharpCodeWith_Class_Method_WithMethodText:{0}", ex.Message);                
            }      			
			return null;                
        }        
        /*public void mapCodeO2References(Ast_CSharp astCSharp)
        {            
            bool onlyAddReferencedAssemblies = false;			
            ExtraSourceCodeFilesToCompile = new List<string>();                                
        	var compilationUnit = astCSharp.CompilationUnit;
            ReferencedAssemblies = new List<string>();
            var filesToDownload = new List<string>();

        	var currentUsingDeclarations = new List<string>();
        	foreach(var usingDeclaration in astCSharp.AstDetails.UsingDeclarations)
        		currentUsingDeclarations.Add(usingDeclaration.Text);        	
        	
            foreach (var comment in astCSharp.AstDetails.Comments)
            {
                comment.Text.eq    ("O2Tag_OnlyAddReferencedAssemblies", () => onlyAddReferencedAssemblies = true);				
                comment.Text.starts("using ", false, value => astCSharp.CompilationUnit.add_Using(value));
                comment.Text.starts(new [] {"ref ", "O2Ref:"}, false,  value => ReferencedAssemblies.Add(value));
                comment.Text.starts(new[]  { "Download:","download:", "O2Download:" }, false, value => filesToDownload.Add(value));
                comment.Text.starts(new[]  { "include", "file ", "O2File:" }, false, value => ExtraSourceCodeFilesToCompile.Add(value));
                comment.Text.starts(new[]  { "dir ", "O2Dir:" }, false, value => ExtraSourceCodeFilesToCompile.AddRange(value.files("*.cs",true))); 
               
                comment.Text.starts(new[]  { "O2:debugSymbols",
                                            "generateDebugSymbols", 
                                            "debugSymbols"}, true, (value) => generateDebugSymbols = true);
                comment.Text.starts(new[]  {"SetInvocationParametersToDynamic"}, (value) => ResolveInvocationParametersType = false);
                comment.Text.starts(new[]  { "DontSetInvocationParametersToDynamic" }, (value) => ResolveInvocationParametersType = true);                    
                comment.Text.eq("StaThread", () => { ExecuteInStaThread = true; });
                comment.Text.eq("MtaThread", () => { ExecuteInMtaThread = true; });
                comment.Text.eq("WorkOffline", () => { WorkOffline = true; });                
            }

            //resolve location of ExtraSourceCodeFilesToCompile
            resolveFileLocationsOfExtraSourceCodeFilesToCompile();

            CompileEngine.handleReferencedAssembliesInstallRequirements(astCSharp.AstDetails.CSharpCode);

            //use the same technique to download files that are needed for this script (for example *.zip files or other unmanaged/support files)
            CompileEngine.tryToResolveReferencesForCompilation(filesToDownload, WorkOffline);            

            if (onlyAddReferencedAssemblies.isFalse())
            {
                foreach (var defaultRefAssembly in CompileEngine.DefaultReferencedAssemblies)
                    if (ReferencedAssemblies.Contains(defaultRefAssembly).isFalse())
                        ReferencedAssemblies.add(defaultRefAssembly);
                foreach (var usingStatement in CompileEngine.DefaultUsingStatements)
                    if (false == currentUsingDeclarations.Contains(usingStatement))
                        compilationUnit.add_Using(usingStatement);
            }

			//make sure the referenced assemblies are in the current execution directory
			CompileEngine.tryToResolveReferencesForCompilation(ReferencedAssemblies, WorkOffline);            

        }*/
/*        public void resolveFileLocationsOfExtraSourceCodeFilesToCompile()
        {
            if (ExtraSourceCodeFilesToCompile.size() > 0)
            {                
                // try to resolve local file references
                try
                {                    
                    if (this.SourceCodeFile.isNull())           // in case this is not set
                        SourceCodeFile = PublicDI.CurrentScript;
                    for (int i = 0; i < ExtraSourceCodeFilesToCompile.size(); i++)
                    {
                        var fileToResolve = ExtraSourceCodeFilesToCompile[i].trim();

                        var resolved = false;
                        // try using SourceCodeFile.directoryName()
                        if (fileToResolve.fileExists().isFalse())
                            if (SourceCodeFile.valid() && SourceCodeFile.isFile())
                            {
                                var resolvedFile = SourceCodeFile.directoryName().pathCombine(fileToResolve);
                                if (resolvedFile.fileExists())
                                {
                                    ExtraSourceCodeFilesToCompile[i] = resolvedFile;
                                    resolved = true;
                                }
                            }    
                        // try using the localscripts folders                                            
                        var localMapping = fileToResolve.local();
                        if (localMapping.valid())
                        {
                            ExtraSourceCodeFilesToCompile[i] = localMapping;
                            resolved = true;
                        }
                        
                        if (resolved.isFalse() && fileToResolve.fileExists().isFalse())
                            ExtraSourceCodeFilesToCompile[i] = ExtraSourceCodeFilesToCompile[i].fullPath();                        
                    }
                }
                catch (Exception ex)
                {
                    ex.log("in compileExtraSourceCodeReferencesAndUpdateReferencedAssemblies while resolving ExtraSourceCodeFilesToCompile");
                }
            }
        }*/
        public object executeFirstMethod()
        {
			if (this.compiledAssembly().notNull())
			{
				var parametersValues = this.invocationParameters().valuesArray();
				return this.compiledAssembly().executeFirstMethod(CompilerOptions.ExecuteInStaThread, CompilerOptions.ExecuteInMtaThread, parametersValues);
			}
			return null;
        }                       
        public Location getGeneratedSourceCodeMethodLineOffset()
        {
            if (this.createdFromSnippet() && this.sourceCode().valid())                
                    if (this.astDetails().Methods.size() > 0)
                    {
                        return this.astDetails().Methods[0].OriginalObject.firstLineOfCode();
                    }
            return new Location(0, 0) ;
        }
        public void waitForCompilationComplete()
        {            
            if (FinishedCompilingCode.WaitOne(20 * 1000).isFalse())
                "in CSharp_FastCompiler, the compilation lasted more than 20 seconds".error();
            FinishedCompilingCode.WaitOne();
        }
    }
}
