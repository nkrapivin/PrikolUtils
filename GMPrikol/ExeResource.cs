using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using Vestris.ResourceLib;

namespace GMPrikol
{
    public class ExeResInfo
    {
        public Version ExeVersion;
        public string Company;
        public string Product;
        public string Copyright;
        public string Description;
        public byte[] FileIcon;

        public ExeResInfo()
        {

        }

        public ExeResInfo(Version vv, string cc, string pp, string co, string de, byte[] icon)
        {
            ExeVersion = vv;
            Company = cc;
            Product = pp;
            Copyright = co;
            Description = de;
            FileIcon = icon;
        }
    }

    public static class ExeResource
    {
        /// <summary>
        /// Appends an extra null-terminator to the string. Needed by ResourceLib.
        /// </summary>
        /// <param name="s">the string</param>
        /// <returns>string with a null-terminator at the end.</returns>
        public static string n(string s) => string.Format("{0}\0", s);

        public static void SetExeResourceInfo(string fullPathToExe, ExeResInfo i)
        {
            VersionResource versionResource = new VersionResource();
            versionResource.Language = 1043;
            versionResource.LoadFrom(fullPathToExe);
            versionResource.FileVersion = i.ExeVersion.ToString();
            versionResource.ProductVersion = i.ExeVersion.ToString();
            StringFileInfo stringFileInfo = (StringFileInfo)versionResource["StringFileInfo"];
            stringFileInfo["CompanyName"] = n(i.Company);
            stringFileInfo["ProductName"] = n(i.Product);
            stringFileInfo["LegalCopyright"] = n(i.Copyright);
            stringFileInfo["ProductVersion"] = n(versionResource.ProductVersion);
            stringFileInfo["FileDescription"] = n(i.Description);
            stringFileInfo["Comments"] = n("Powered by GMPrikol.");

            versionResource.SaveTo(fullPathToExe);

            IconDirectoryResource rc = new IconDirectoryResource();
            rc.Name = new ResourceId("MAINICON");
            rc.Language = 1043;
            rc.LoadFrom(fullPathToExe);

            
            string AppDir = AppDomain.CurrentDomain.BaseDirectory;
            string Ico = Path.Combine(AppDir, "temp.ico");
            File.WriteAllBytes(Ico, i.FileIcon);
            IconFile iconfile = new IconFile(Ico);
            File.Delete(Ico);

            IconDirectoryResource iconDirectoryResource = new IconDirectoryResource(iconfile);
            rc.Icons.Clear();
            foreach (var ii in iconDirectoryResource.Icons)
                rc.Icons.Add(ii);
            
            rc.SaveTo(fullPathToExe);
        }
    }
}
