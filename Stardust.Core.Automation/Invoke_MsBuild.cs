//
// invoke_msbuild.cs
// This file is part of Stardust.Core.Automation
//
// Author: Jonas Syrstad (jsyrstad2+StardustCore@gmail.com), http://no.linkedin.com/in/jonassyrstad/) 
// Copyright (c) 2014 Jonas Syrstad. All rights reserved.
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Management.Automation;
using Stardust.Particles;

namespace Stardust.Core.Automation
{

    [Cmdlet(VerbsLifecycle.Invoke, "MsBuild")]
    // ReSharper disable once InconsistentNaming
    public class Invoke_MsBuild : Cmdlet
    {
        [Parameter(Mandatory = true, ValueFromPipeline = true, HelpMessage = "The path to the file to build with MsBuild (e.g. a .sln or .csproj file).")]
        public string Path { get; set; }

        [Parameter(Mandatory = false)]
        public string MsBuildParameters { get; set; }

        [Parameter(Mandatory = false)]
        public string BuildLogDirectoryPath { get; set; }

        [Parameter(Mandatory = false)]
        public SwitchParameter AutoLaunchBuildLogOnFailure { get; set; }

        [Parameter(Mandatory = false)]
        public SwitchParameter KeepBuildLogOnSuccessfulBuilds { get; set; }

        [Parameter(Mandatory = false)]
        public SwitchParameter ShowBuildWindow { get; set; }

        [Parameter(Mandatory = false)]
        public SwitchParameter PassThru { get; set; }

        [Parameter(Mandatory = false)]
        public SwitchParameter GetLogPath { get; set; }

        protected override void ProcessRecord()
        {
            CheckPathKeyWord();

            var buildLogFile = GetBuildLogFilePath();
            
            try
            {
                var cmdArgumentsToRunMsBuild = CreateBuildCommand(buildLogFile);
                WriteDebug("Starting new cmd.exe process with arguments \"" + cmdArgumentsToRunMsBuild + "\".");
                
                var success = ExecuteMsBuild(cmdArgumentsToRunMsBuild);
                WriteObject(DoPostProsessing(buildLogFile, success));//Returns build state to client script
            }
            catch (Exception ex)
            {
                WriteObject(false);
                WriteError(new ErrorRecord(ex, "BuildError", ErrorCategory.FromStdErr, Path));
            }
        }

        private string GetBuildLogFilePath()
        {
            var solutionName = System.IO.Path.GetFileNameWithoutExtension(Path);
            WriteDebug(string.Format("Building solution: '{0}'", solutionName));
            var buildLogFile = BuildLogDirectoryPath.IsNullOrEmpty() ? "" : System.IO.Path.Combine(BuildLogDirectoryPath, solutionName) + ".msbuild.log";
            return buildLogFile;
        }

        private string CreateBuildCommand(string buildLogFile)
        {
            var buildArguments = CreateBuildArguments(buildLogFile);
            string cmdArgumentsToRunMsBuild;
            var vsCommandPrompt = GetVisualStudioComandPrompt();
            if (vsCommandPrompt.ContainsCharacters())
            {
                cmdArgumentsToRunMsBuild = "/k \" " + " \"" + vsCommandPrompt + "\" & msbuild ";
            }
            else
            {
                var msBuildPath = GetMsBuildPath();
                cmdArgumentsToRunMsBuild = "/k \" " + " \"" + msBuildPath + "\" ";
            }
            cmdArgumentsToRunMsBuild = cmdArgumentsToRunMsBuild + buildArguments + " & Exit\"";
            return cmdArgumentsToRunMsBuild;
        }

        private bool DoPostProsessing(string buildLogFile, bool success)
        {

            if (PassThru) return true;
            if (success)
            {
                if (!File.Exists(buildLogFile))
                {
                    WriteWarning(string.Format("Cannot find the build log file at '{0}', so unable to determine if build succeeded or not.",buildLogFile));
                    return false;
                }
                if (File.ReadAllText(buildLogFile).Contains("Build FAILED."))
                {
                    WriteWarning(string.Format("FAILED to build '{0}'. Please check the build log '{1}' for details.", Path,buildLogFile));
                    return false;
                }
                File.Delete(buildLogFile);
            }
            else
            {
                WriteWarning("Build failed");
                return false;
            }
            WriteDebug("Build completed");
            return true;
        }

        private bool ExecuteMsBuild(string cmdArgumentsToRunMsBuild)
        {
            var process = CreateBuildProcess(WindowStyle, cmdArgumentsToRunMsBuild);
            if (!PassThru)
                return WaitForBuild(process);
            process.Start();
            return true;
        }

        private ProcessWindowStyle WindowStyle
        {
            get { return ShowBuildWindow ? ProcessWindowStyle.Normal : ProcessWindowStyle.Hidden; }
        }

        private static bool WaitForBuild(Process process)
        {
            process.Start();
            process.WaitForExit();
            var success = process.ExitCode == 0;
            return success;
        }

        private static Process CreateBuildProcess(ProcessWindowStyle windowStyle, string cmdArgumentsToRunMsBuild)
        {
            var process = new Process();
            var startInfo = new ProcessStartInfo("cmd.exe")
            {
                WindowStyle = windowStyle,
                Arguments = cmdArgumentsToRunMsBuild
            };
            //startInfo.FileName = "cmd.exe";
            process.StartInfo = startInfo;
            return process;
        }

        public string GetMsBuildPath()
        {
            var versions = new[] { "12.0", "4.0", "3.5", "2.0" };
            foreach (var version in versions)
            {
                var regKey = @"HKLM:\SOFTWARE\Microsoft\MSBuild\ToolsVersions\" + version;
                var rk = Registry.LocalMachine.OpenSubKey(regKey, false);
                if (rk.IsInstance() && rk.GetValue("MSBuildToolsPath").IsInstance())
                {
                    return rk.GetValue("MSBuildToolsPath") + "MsBuild.exe";
                }
            }
            return null;
        }

        private string CreateBuildArguments(string buildLogFile)
        {
            return string.Format("\"{0}\" {1} /fileLoggerParameters:LogFile=\"{2}\"", Path, MsBuildParameters, buildLogFile);
        }

        private void CheckPathKeyWord()
        {

            if (BuildLogDirectoryPath.IsNullOrWhiteSpace() || BuildLogDirectoryPath.Equals("PathDirectory", StringComparison.InvariantCultureIgnoreCase))
            {
                BuildLogDirectoryPath = System.IO.Path.GetDirectoryName(Path);
            }

        }

        private string GetVisualStudioComandPrompt()
        {
            var vs2010CommandPrompt = Environment.GetEnvironmentVariable("VS100COMNTOOLS") + "vcvarsall.bat";
            var vs2012CommandPrompt = Environment.GetEnvironmentVariable("VS110COMNTOOLS") + "VsDevCmd.bat";
            var vs2013CommandPrompt = Environment.GetEnvironmentVariable("VS120COMNTOOLS") + "VsDevCmd.bat";
            if (File.Exists(vs2013CommandPrompt)) return vs2013CommandPrompt;
            if (File.Exists(vs2012CommandPrompt)) return vs2012CommandPrompt;
            if (File.Exists(vs2010CommandPrompt)) return vs2010CommandPrompt;
            return null;
        }
    }
}
