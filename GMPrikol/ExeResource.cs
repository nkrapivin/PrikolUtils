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
        public static void SetExeResourceInfo(string fullPathToExe, ExeResInfo info)
        {
            // set version info
            VersionResource versionResource = new VersionResource();
            versionResource.Language = 1043;
            versionResource.LoadFrom(fullPathToExe);
            versionResource.FileVersion = info.ExeVersion.ToString();
            versionResource.ProductVersion = info.ExeVersion.ToString();
            StringFileInfo stringFileInfo = (StringFileInfo)versionResource["StringFileInfo"];
            string fVer = string.Format("{0}\0", info.ExeVersion.ToString());
            stringFileInfo["CompanyName"] = string.Format("{0}\0", info.Company);
            stringFileInfo["FileDescription"] = string.Format("{0}\0", info.Description);
            stringFileInfo["FileVersion"] = fVer;
            stringFileInfo["LegalCopyright"] = string.Format("{0}\0", info.Copyright);
            stringFileInfo["ProductName"] = string.Format("{0}\0", info.Product);
            stringFileInfo["ProductVersion"] = fVer;
            versionResource.SaveTo(fullPathToExe);

            // set icon info
            string AppDir = AppDomain.CurrentDomain.BaseDirectory;
            string iPath = Path.Combine(AppDir, "temp.ico");

            // I know, this is ugly, blame Vestris.
            File.WriteAllBytes(iPath, info.FileIcon);
            IconFile iconFile = new IconFile(iPath);
            File.Delete(iPath);

            IconDirectoryResource rc = new IconDirectoryResource();
            rc.Language = 1043;
            rc.Name = new ResourceId("MAINICON");

            rc.LoadFrom(fullPathToExe);
            //rc.SaveTo(fullPathToExe); // :(, this doesn't work due to a bug in Vestris's library.
            // TODO: Fix icon saving.
        }
    }
}
