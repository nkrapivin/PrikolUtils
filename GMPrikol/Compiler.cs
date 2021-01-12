using PrikolLib;
using PrikolLib.Assets;
using PrikolLib.Base;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace GMPrikol
{
    class Compiler
    {
        public bool EnableDebugMode { get; set; }
        public bool EnableProMode { get; set; }
        public bool RunAfterBuild { get; set; }
        public string ProjectFilePath { get; set; }
        public string OutputExecutablePath { get; set; }
        public string GEXSearchPath { get; set; }

        private GMProject Project { get; set; }
        private string AppDir => AppDomain.CurrentDomain.BaseDirectory;

        public int Compile()
        {
            try
            {
                Console.Write("Loading project file... ");
                using (FileStream projectStream = new FileStream(ProjectFilePath, FileMode.Open, FileAccess.Read, FileShare.Read))

                // meh. yes, that many streams.
                using (MemoryStream gamedataBase = new MemoryStream())
                using (ProjectWriter gamedataStream = new ProjectWriter(gamedataBase))
                using (MemoryStream gmkryptBase = new MemoryStream())
                using (ProjectWriter gmkryptStream = new ProjectWriter(gmkryptBase))
                {
                    Project = new GMProject(projectStream);

                    Console.WriteLine("Done");

                    // Just like the Editor does.
                    if (Project.Rooms.Count <= 0)
                        throw new Exception("A game must have at least one room in order to run you dummy :)");

                    Console.Write("Unpacking rundata... ");
                    RundataManager.LoadRundata(Project.Version, OutputExecutablePath);
                    Console.WriteLine("Done");

                    ExeResInfo eri = new ExeResInfo(
                        Project.Options.GameVersion,
                        Project.Options.Company,
                        Project.Options.Product,
                        Project.Options.Copyright,
                        Project.Options.Description,
                        Project.Options.RawIcon
                    );

                    Console.Write("Setting exe information... ");
                    ExeResource.SetExeResourceInfo(OutputExecutablePath, eri);
                    Console.WriteLine("Done");

                    Console.Write("Writing game data... ");
                    gamedataStream.Write(GMProject.GMMagic);
                    gamedataStream.Write(Project.Version);

                    // dummy value 1
                    if (Project.Version == 810) gamedataStream.Write(1337);

                    // the debug mode flag, not encrypted...
                    gamedataStream.Write(EnableDebugMode);

                    // dummy value 2
                    if (Project.Version == 810) gamedataStream.Write(6969);

                    // options (not encrypted)
                    Options.WriteOptions(gamedataStream, Project.Options);

                    // embedded direct3d 8.0 dll
                    gamedataStream.Write("D3DX8.dll");
                    gamedataStream.Write(File.ReadAllBytes(Path.Combine(AppDir, "dxdata")));

                    // actual crypto fun starts.
                    gamedataStream.Write(0); // count1
                    gamedataStream.Write(0); // count2
                    // GMKrypt table, or in this case, a dummy table :p
                    for (int i = 0; i <= byte.MaxValue; i++)
                        gamedataStream.Write((byte)i);

                    // dummy ints...
                    gmkryptStream.Write(0); // count3

                    // header
                    gmkryptStream.Write(EnableProMode);
                    gmkryptStream.Write(Project.GameID);
                    gmkryptStream.Write(Project.DirectPlayGuid);

                    // extensions
                    Asset.WriteExtensions(gmkryptStream, Project.ExtensionPackageNames, GEXSearchPath);

                    // triggers
                    Asset.WriteAsset(gmkryptStream, Project.Triggers, AssetDelegates.Trigger);

                    // constants
                    Asset.WriteConstants(gmkryptStream, Project.Constants);

                    // sounds
                    Asset.WriteAsset(gmkryptStream, Project.Sounds, AssetDelegates.Sound);

                    // sprites
                    Asset.WriteAsset(gmkryptStream, Project.Sprites, AssetDelegates.Sprite);

                    // backgrounds
                    Asset.WriteAsset(gmkryptStream, Project.Backgrounds, AssetDelegates.Background);

                    // paths
                    Asset.WriteAsset(gmkryptStream, Project.Paths, AssetDelegates.Path);

                    // scripts
                    Asset.WriteAsset(gmkryptStream, Project.Scripts, AssetDelegates.Script);

                    // fonts
                    Asset.WriteAsset(gmkryptStream, Project.Fonts, AssetDelegates.Font);

                    // timelines
                    Asset.WriteAsset(gmkryptStream, Project.Timelines, AssetDelegates.Timeline, Project);

                    // objects
                    Asset.WriteAsset(gmkryptStream, Project.Objects, AssetDelegates.Object, Project);

                    // rooms
                    Asset.WriteAsset(gmkryptStream, Project.Rooms, AssetDelegates.Room, Project);
                    Asset.WriteMaxRoomIDs(gmkryptStream, Project.LastInstanceID, Project.LastTileID);

                    // included files
                    Asset.WriteAsset(gmkryptStream, Project.IncludedFiles, AssetDelegates.IncludedFile);

                    // game information
                    Asset.WriteGameInformation(gmkryptStream, Project.GameInformation);

                    // library creation code
                    Asset.WriteLibCC(gmkryptStream, Project.LibraryCreationCode);

                    // room execution order
                    Asset.WriteRoomExecOrder(gmkryptStream, Project.RoomExecutionOrder, Project);

                    // encryption
                    byte[] gmkryptData = Encryption.EncryptGM8(gmkryptBase);
                    gamedataStream.Write(gmkryptData.Length);
                    gamedataStream.Write(gmkryptData);

                    // encryption crc/xor (810 only)
                    if (Project.Version == 810)
                    {
                        // TODO: implement the thing.
                        Encryption.EncryptGM81(gamedataBase);
                    }

                    // finalization (rundata, icon, version, etc)
                    Console.WriteLine("Done");

                    // Write rundata.
                    Console.Write("Appending game data to rundata... ");
                    FileStream rundata = new FileStream(OutputExecutablePath, FileMode.Append, FileAccess.Write, FileShare.Read);
                    byte[] dataToWrite = gamedataBase.ToArray();

                    // GM8.1 STARTS AT A DIFFERENT POSITION, NEED TO CHANGE.
                    // Technically 2000000 should always be beyond rundata, but if you set a very large icon
                    // this position may be inside rundata, luckily GameMaker will also add 10000
                    // in a loop to the seek position until it finds the game data.
                    long pos = 2000000;
                    while (pos <= rundata.Length) pos += 10000;
                    rundata.Seek(pos, SeekOrigin.Begin);
                    
                    // Write gamedata to rundata.
                    rundata.Write(dataToWrite);

                    // Flush and close the stream safely.
                    rundata.Flush(true);
                    rundata.Dispose();

                    // yay!!!
                    Console.WriteLine("Done");
                }

                if (RunAfterBuild)
                {
                    try
                    {
                        Console.Write("Trying to run the game... ");
                        // TODO: on Linux maybe use `wine` as the filename, and full path as the argument?
                        Process game = Process.Start(OutputExecutablePath);
                        if (game == null) throw new Exception("No game process was started.");
                        game.Dispose();
                        Console.WriteLine("Done");
                    }
                    catch
                    {
                        Console.WriteLine("Warning: Unable to run the game process!");
                    }
                }

                Console.WriteLine("All Done!");
            }
            catch (Exception e)
            {
                Console.WriteLine();
                Console.WriteLine("-- BUILD FAILURE, EXCEPTION BEGIN --");
                Console.WriteLine(e.ToString());
                Console.WriteLine("-- EXCEPTION END --");
                Console.WriteLine();
                return Program.EXIT_FAILURE;
            }

            return Program.EXIT_SUCCESS;
        }
    }
}
