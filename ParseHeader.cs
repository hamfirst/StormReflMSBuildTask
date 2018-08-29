using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Build.Utilities;

namespace StormReflMsBuildTask
{
  public class ParseHeader : ToolTask
  {
    public string ExeFile { get; set; }
    public string DependencyDir { get; set; }
    public string[] HeaderFiles { get; set; }
    public string[] IncludeDirs { get; set; }

    private List<string> FilesToBuild = new List<string>();

    public ParseHeader()
    {
      LogStandardErrorAsError = true;
    }

    protected override string ToolName
    {
      get
      {
        return "StormRefl";
      }
    }

    protected override string GenerateFullPathToTool()
    {
      return ExeFile;
    }

    protected override bool ValidateParameters()
    {
      var exe_last_modified = GetLastWriteTime(ExeFile);

      if (HeaderFiles != null)
      {
        foreach (var header in HeaderFiles)
        {
          bool process_file = true;
          try
          {
            var file_name = System.IO.Path.GetFileNameWithoutExtension(header);
            var extension = System.IO.Path.GetExtension(header);
            var dir = System.IO.Path.GetDirectoryName(header);

            var meta_file = System.IO.Path.Combine(dir, file_name) + ".meta" + extension;
            var deps_file = System.IO.Path.Combine(DependencyDir, file_name) + extension + ".deps";


            process_file = CheckDependencies(meta_file, deps_file, exe_last_modified);
          }
          catch (Exception)
          {

          }

          //Log.LogWarning("Checking dependencies for {0}", header);
          if (process_file)
          {
            FilesToBuild.Add(header);
          }
        }
      }

      return base.ValidateParameters();
    }

    protected override bool SkipTaskExecution()
    {
      if(FilesToBuild.Count == 0)
      {
        return true;
      }

      return base.SkipTaskExecution();
    }

    public DateTime GetLastWriteTime(string path)
    {
      try
      {
        var last_write = System.IO.File.GetLastWriteTime(path);
        return last_write;
      }
      catch (Exception)
      {
        //Log.LogWarning("Couldn't determine last write time for {0}", path);
        return DateTime.Now;
      }
    }

    private bool CheckDependencies(string meta_file, string deps_file, DateTime exe_last_modified)
    {
      var last_modified = GetLastWriteTime(meta_file);
      if(exe_last_modified > last_modified)
      {
        //Log.LogWarning("Exe is more recent");
        return true;
      }

      try
      {
        var dep_files = System.IO.File.ReadAllLines(deps_file);

        foreach (var dep in dep_files)
        {
          var dep_last_modified = GetLastWriteTime(dep);
          if(dep_last_modified > last_modified)
          {
            //Log.LogWarning("Dep was more recent: {0}", dep);
            return true;
          }
        }   
      }
      catch(Exception)
      {
        //Log.LogWarning("Couldn't read deps file");
        return true;
      }

      return false;
    }

    protected override string GenerateCommandLineCommands()
    {
      CommandLineBuilder builder = new CommandLineBuilder();

      foreach(var header in FilesToBuild)
      {
        builder.AppendFileNameIfNotNull(header);
      }

      builder.AppendSwitch("-depsdir " + DependencyDir);
      builder.AppendSwitch("--");
      builder.AppendSwitch("-DSTORM_REFL_PARSE");
      builder.AppendSwitch("-D_CRT_SECURE_NO_WARNINGS");
      builder.AppendSwitch("-x c++");

      if (IncludeDirs != null)
      {
        foreach (var include in IncludeDirs)
        {
          builder.AppendSwitch("-I" + include);
        }
      }

      builder.AppendSwitch("-Wno-pragma-once-outside-header");
      return builder.ToString();
    }
  }
}
