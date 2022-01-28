using CommandLine;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Text;

namespace harReducer
{
    class Program
    {
        internal static bool Verbose = false;
        internal static string InFile = "";
        internal static string OutFile;
        internal static JObject entryInfo = new JObject();
        internal static string CSVFile = "";
        internal static bool CSV = false;
        internal static StringBuilder s = new StringBuilder();
        internal static string comma = "";

        static int  Main(string[] args)
        {
            Console.WriteLine("harReducer v1.0");
            try
            {


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

                          CSV = o.CSV;
                          if (CSV)
                              CSVFile = OutFile + ".csv";
                      }
                    );
            } catch (Exception ex)
            {
                log(ex.Message);
                return 1;

            }

            try
            {
                ProcessHar( InFile,  OutFile,  Verbose);
            }
            catch (Exception ex)
            {
                log(ex.Message);
                return 1;
            }

            if (Verbose)
                log("Exiting Normally");
            return 0;
        }

        private static void ProcessHar( string inFile,  string outFile,  bool verbose)
        {
            JObject i= new JObject();
            
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
            var c = 0;
            foreach(JObject j in i["log"]["entries"])
            {
                c++;
                
                entryInfo = new JObject();
                comma = "";
                AddEntry("entry", c, c);
                comma = ", ";
                AddEntry("url", "\"" + j["request"]["url"].ToString() + "\"", c);
                AddEntry("status", j["response"]["status"].ToString(), c);
                AddEntry("size", j["response"]["content"]["size"].ToString(), c);
                AddEntry("mimeType", j["response"]["content"]["mimeType"].ToString(), c);
                AddEntry("serverIPAddress", j["serverIPAddress"].ToString(), c);
                AddEntry("time", j["time"].ToString(), c);
                AddEntry("resourceType", j["_resourceType"].ToString(), c);
                if (j["_fromCache"] != null ) AddEntry("fromCache", j["_fromCache"].ToString(), c);

                if (CSV)
                    s.Append(Environment.NewLine);

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

            header.Add(new JProperty("TotalEntries", c));

            o.Add(new JProperty("info", header));
            o.Add(new JProperty("Fonts", Fonts));
            o.Add(new JProperty("Images", Images));
            o.Add(new JProperty("Scripts", Scripts));
            o.Add(new JProperty("Docs", Docs));
            o.Add(new JProperty("xhr", Xhr));
            o.Add(new JProperty("Other", Other));

            File.WriteAllText(OutFile, o.ToString());

            if (CSV)
                File.WriteAllText(CSVFile, s.ToString());
        }
        internal static void AddEntry(string name, int value, int c)
        {
            if (Verbose)
                log($"Adding Entry ${name} to entry ${c}");
            entryInfo.Add(name, value);
            if (CSV)
            {
                s.Append(comma);
                s.Append(value);

            }
        }
        internal static void AddEntry(string name, string value, int c )
        {
            log($"Adding Entry ${name} to entry ${c}");
            entryInfo.Add(name, value);
            if (CSV)
            {
                s.Append(comma);
                if (value.Contains(","))
                    value = value.Replace(",", ";");
                //if (value.Contains("\""))
                //    value = value.Replace("\"", "\"\"");
                if (value.Contains(","))
                    value =  value.Replace(",", ",,") ;


                s.Append(value);
                
            }
        }
        internal static void log(string msg) { Console.WriteLine(msg); }
        internal static void vlog(string msg)
        {
            if (Verbose) log(msg);
        }
    }
}
