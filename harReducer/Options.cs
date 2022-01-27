using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace harReducer
{
    public class Options
    {
         
        [Option('v', Required = false, HelpText = "sets verbose logging")]
        public bool Verbose { get; set; }
        [Option('i', Required = true, HelpText = "File to read")]
        public string InFile { get; set; }
        [Option('o', HelpText = "Output File - Defaults to harout.json")]
        public string OutFile { get; set; } = "harout.json";
        

    }
}
