﻿using System.Linq;

namespace Sam.JavaScriptNamespacer
{
    using System.Configuration;
    using System.IO;
    using System;
    using System.Text;

    class Program
    {
        static void Main(string[] args)
        {
            var path = ConfigurationManager.AppSettings["path"];
            var rootNameSpace = ConfigurationManager.AppSettings["rootNamespace"]; 
            const string JavaScriptFileExtension = "*js";
            const string FourSpaces = "    ";

            // With @ new lines and spaces are relevant.
            const string EndIIFE = @"
}(window || {}));";
            string StartIIFE =
@"(function (global) {

    'use strict';

    global.ROOT_NAMESPACE = global.ROOT_NAMESPACE || {};
";

            string NameSpacing = @"    global.ROOT_NAMESPACE.XXX = global.ROOT_NAMESPACE.XXX || {};
    global.ROOT_NAMESPACE.XXX.YYY = global.ROOT_NAMESPACE.XXX.YYY || {};
    var ZZZ = global.ROOT_NAMESPACE.XXX.YYY.ZZZ = global.ROOT_NAMESPACE.XXX.YYY.ZZZ || {};

";

            StartIIFE = StartIIFE.Replace("ROOT_NAMESPACE", rootNameSpace);
            NameSpacing = NameSpacing.Replace("ROOT_NAMESPACE", rootNameSpace);

            // WARNING: Possible infinate loop if directories have a loop
            string[] jsFiles = Directory.GetFiles(path, JavaScriptFileExtension, SearchOption.AllDirectories);

            const string EndFunc = " = function(";

            // This is probably very inefficient. Quick and dirty!
            foreach (var jsFile in jsFiles)
            {          
                string jsName = Path.GetFileNameWithoutExtension(jsFile);

                string NjsName = ToLowerCamelCase(jsName);

                string subFolder = new DirectoryInfo(Path.GetDirectoryName(jsFile)).Name;

                string NsubFolder = ToLowerCamelCase(subFolder);

                var foo = jsFile.Replace(subFolder + "\\" + Path.GetFileName(jsFile), "");

                string folder = new DirectoryInfo(foo).Name;

                string Nfolder = ToLowerCamelCase(folder);

                var newNs = NameSpacing
                    .Replace("XXX", Nfolder) 
                    .Replace("YYY", NsubFolder)
                    .Replace("ZZZ", NjsName);

                var lines = File.ReadAllLines(jsFile);

                var sb = new StringBuilder();

                var privateMethods = false;
                foreach (var line in lines)
                {
                    if (line.StartsWith("// METODOS AUXILIARES"))
                    {
                        privateMethods = true;
                        sb.Append(FourSpaces + line);
                        continue;
                    }

                    if (!privateMethods)
                    {
                        if (line.StartsWith("function "))
                        {
                            var newLine = line
                                .Replace("function ", jsName + ".")
                                .Replace("(", EndFunc);

                            sb.Append(FourSpaces + newLine + "\r");
                            continue;
                        }
                    }

                    var equal = line.Replace("==", "===")   
                                    .Replace("!=", "!==");

                    sb.Append(FourSpaces + equal + "\r");
                }

                var text = sb.ToString();
                var newFile = StartIIFE + newNs + text + EndIIFE;
                File.WriteAllText(jsFile + ".s", newFile);
            }

            Console.ReadKey();
        }

        private static string ToLowerCamelCase(string s)
        {
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < s.Length; i++)
            {
                if (i == 0)
                    sb.Append(s[i].ToString().ToLower());
                else
                    sb.Append(s[i]);
            }

            return sb.ToString();
        }
    }
}