using CommandLine;
using Newtonsoft.Json.Linq;
using System;
using System.IO;

namespace harReducer
{
    class Program
    {
        internal static bool Verbose = false;
        internal static string InFile = "";
        internal static string OutFile;
        static void Main(string[] args)
        {
            Console.WriteLine("harReducer v1.0");
            Parser.Default.ParseArguments<Options>(args)
                .WithParsed<Options>(
                  o =>
                  {
                      if (o.Verbose)
                      {
                          Verbose = true;
                          log("Verbose Mode Turned On");
                      }

                      InFile = o.InFile;
                      vlog($"Using ${InFile} as input file");

                      OutFile = o.OutFile;
                      vlog($"Using ${OutFile} as output file");

                  }
                );

            ProcessHar( InFile,  OutFile,  Verbose);
        }

        private static void ProcessHar( string inFile,  string outFile,  bool verbose)
        {
            JObject i= new JObject();
            JObject entryInfo = new JObject();
            JObject o = new JObject();
            
            JObject header = new JObject();
            JArray Images = new JArray(); 
            JArray Docs = new JArray();
            JArray Fonts =  new JArray();

            JArray StyleSheets = new JArray();
            JArray  Xhr =  new JArray();
            JArray Other =  new JArray();
            JArray Scripts = new JArray();

            vlog($"opening {inFile}");
            string dataIn = File.ReadAllText(inFile);
            vlog("Converting to JSON");
            try
            {
                i = JObject.Parse(dataIn);
            }
            catch (Exception ex)
            {
                vlog("exception caught while parsing file into json - " + ex.Message);
            }
            vlog($"url processed is ${i["log"]["pages"][0]["title"]}");
            vlog($"time collected: ${i["log"]["pages"][0]["startedDateTime"]}");
            header.Add(new JProperty("URL", i["log"]["pages"][0]["title"].ToString()));
            header.Add(new JProperty("collectionTime", i["log"]["pages"][0]["startedDateTime"]));

            foreach(JObject j in i["log"]["entries"])
            {
                entryInfo = new JObject();
                entryInfo.Add(new JProperty("url", j["request"]["url"]));
                entryInfo.Add(new JProperty("status", j["response"]["status"]));
                entryInfo.Add(new JProperty("size", j["response"]["content"]["size"]));
                entryInfo.Add(new JProperty("mimeType", j["response"]["content"]["mimeType"]));
                entryInfo.Add(new JProperty("serverIPAddress", j["serverIPAddress"]));
                entryInfo.Add(new JProperty("time", j["time"]));
                entryInfo.Add(new JProperty("resourceType", j["_resourceType"]));
                entryInfo.Add(new JProperty("fromCache", j["_fromCache"]));

                var resourceType = j.Value<string>("_resourceType");


                if (resourceType == "font")
                    Fonts.Add(entryInfo);
                else if (resourceType == "image")
                    Images.Add(entryInfo);
                else if (resourceType == "scripts")   // ping
                    Scripts.Add(entryInfo);
                else if (resourceType == "document" )
                    Docs.Add(entryInfo);
                else if (resourceType == "stylesheet")
                    StyleSheets.Add(entryInfo);
                else if (resourceType == "xhr")
                    Xhr.Add(entryInfo);
                else
                    Other.Add(entryInfo);

            }
            
            o.Add(new JProperty("info", header));
            o.Add(new JProperty("Fonts", Fonts));
            o.Add(new JProperty("Images", Images));
            o.Add(new JProperty("Scripts", Scripts));
            o.Add(new JProperty("Docs", Docs));
            o.Add(new JProperty("xhr", Xhr));
            o.Add(new JProperty("Other", Other));

            File.WriteAllText(OutFile, o.ToString());



        }

        internal static void log(string msg) { Console.WriteLine(msg); }
        internal static void vlog(string msg)
        {
            if (Verbose) log(msg);
        }
    }
}do you 
