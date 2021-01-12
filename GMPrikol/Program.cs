using System;
using System.IO;

namespace GMPrikol
{
    class Program
    {
        public const int EXIT_SUCCESS = 0;
        public const int EXIT_FAILURE = 1;

        static void PrintUsage()
        {
            Console.WriteLine("GMPrikol - A tool to compile GameMaker 8.0/8.1 games.");

            Console.WriteLine("Usage:");
            Console.WriteLine("<program> [arguments]");
            Console.WriteLine("Arguments:");
            Console.WriteLine("-debug/-d - Build with debug mode enabled.");
            Console.WriteLine("-run/-r - Run exe after building (Windows only).");
            Console.WriteLine("-pro - Enable Pro mode? (only enable if you own a GM license)");
            Console.WriteLine("-output/-o [path] - Specify custom output path.");
            Console.WriteLine("-project/-p [path] - Path to the gmk/gm81 project file.");
            Console.WriteLine("-gex/-g [path] - Set path to a folder containing GEX extension files.");
            Console.WriteLine("-help/-h - Print this menu.");

            Console.WriteLine();
            Console.WriteLine("Credits:");
            Console.WriteLine("AngelCode - BMFont generator.");
            Console.WriteLine("Zhilemann - helped with YoYo's gamedata encryption.");
            Console.WriteLine("YoYo Games - not obfuscating an old GMAC build.");

            Console.WriteLine();
            Console.WriteLine("Note: This program DOES NOT support Game Maker 7.0 .gmk files! (and most likely never will)");
            Console.WriteLine("Note 2: You have to provide 'dxdata' and 'rundata(project version)' files yourself:");
            Console.WriteLine("rundata800 - Game Maker 8.0, rundata810 - GameMaker 8.1.xx");
            Console.WriteLine("dxdata file should be the same across GM8 and GM8.1");
            Console.WriteLine("dxdata SHA256: 34011DA8374E4C4E6671F79BA33937BB46EE52C75D3570AB6B0EE6D61176CC17");

            Console.WriteLine("Press any key to quit...");
            Console.ReadKey(true);
        }

        static int Main(string[] args)
        {
            bool enableDebugMode = false;
            bool runAfterBuild = false;
            bool enableProMode = false;
            string outputPath = null;
            string projectFilePath = null;
            string gexPath = null;

            if (args.Length == 0 || (args.Length > 0 && (args[0] == "-help" || args[0] == "-h")))
            {
                PrintUsage();
                return EXIT_SUCCESS;
            }

            for (int arg = 0; arg < args.Length; arg++)
            {
                try
                {
                    switch (args[arg])
                    {
                        case "-debug":
                        case "-d":
                            {
                                enableDebugMode = true;
                                Console.WriteLine("Info: Debug mode enabled.");
                                break;
                            }

                        case "-run":
                        case "-r":
                            {
                                runAfterBuild = true;
                                Console.WriteLine("Info: Will try to run game after build.");
                                break;
                            }

                        case "-pro":
                            {
                                enableProMode = true;
                                Console.WriteLine("Info: Pro mode enabled.");
                                break;
                            }

                        case "-output":
                        case "-o":
                            {
                                outputPath = args[arg + 1];
                                //Console.WriteLine("Output path redefined: " + outputPath);
                                break;
                            }

                        case "-project":
                        case "-p":
                            {
                                projectFilePath = args[arg + 1];
                                Console.WriteLine("Info: Project file " + projectFilePath);
                                break;
                            }

                        case "-gex":
                        case "-g":
                            {
                                gexPath = args[arg + 1];
                                Console.WriteLine("Info: Will search for GEX files in " + gexPath);
                                break;
                            }

                        default: break;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error: Malformed arguments. " + e.Message);
                    return EXIT_FAILURE;
                }
            }

            if (projectFilePath == null)
            {
                Console.WriteLine("Error: No project file given!");
                return EXIT_FAILURE;
            }

            if (!File.Exists(projectFilePath))
            {
                Console.WriteLine("Error: Project file does not exist!");
                return EXIT_FAILURE;
            }

            if (gexPath == null)
            {
                string AppDir = AppDomain.CurrentDomain.BaseDirectory;
                gexPath = Path.Combine(AppDir, "extensions");
                Console.WriteLine("Info: No GEX path set, will look for GEX files in " + gexPath);
            }

            if (outputPath == null)
            {
                outputPath = Path.ChangeExtension(projectFilePath, "exe");
                Console.WriteLine("Warning: No output path given, will write to " + outputPath);
            }

            if (File.Exists(outputPath))
            {
                Console.WriteLine("Warning: Output file already exists, it will be overwritten!");
            }

            // run the actual compiler.
            Console.WriteLine();
            int compilerExitCode = new Compiler()
            {
                EnableDebugMode = enableDebugMode,
                EnableProMode = enableProMode,
                RunAfterBuild = runAfterBuild,
                ProjectFilePath = projectFilePath,
                OutputExecutablePath = outputPath,
                GEXSearchPath = gexPath
            }.Compile();

            // return with the compiler's exit code :/
            return compilerExitCode;
        }
    }
}
