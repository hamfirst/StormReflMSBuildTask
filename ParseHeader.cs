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
    public string[] HeaderFiles { get; set; }
    public string[] IncludeDirs { get; set; }

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

    protected override string GenerateCommandLineCommands()
    {
      CommandLineBuilder builder = new CommandLineBuilder();

      if (HeaderFiles != null)
      {
        foreach (var header in HeaderFiles)
        {
          builder.AppendFileNameIfNotNull(header);
        }
      }

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
