using System;
using System.IO;
using System.Linq;
using GroupDocs.Parser.GUI.XmlStorage;
using System.Windows;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;

namespace GroupDocs.Parser.GUI
{
    class Settings
    {
        private const string WindowStateKey = "WindowState";
        private const string HeightKey = "Height";
        private const string WidthKey = "Width";
        private const string LeftKey = "Left";
        private const string TopKey = "Top";

        private const string LicensePathKey = "LicensePath1";

        private const WindowState WindowStateDefault = WindowState.Normal;
        private const double HeightDefault = 600;
        private const double WidthDefault = 800;
        private const double LeftDefault = 0;
        private const double TopDefault = 0;

        private string licensePathDefault = string.Empty;

        private IConfigurationRoot config = new ConfigurationBuilder()
            .AddJsonFile("config.json", optional: true)
            .Build();

        public Settings()
        {
            WindowState = WindowStateDefault;
            Height = HeightDefault;
            Width = WidthDefault;
            Left = LeftDefault;
            Top = TopDefault;

            licensePathDefault = config["LicensePath"] ??
                                 Directory.GetFiles(Directory.GetCurrentDirectory(), "*.lic").FirstOrDefault() ??
                                 string.Empty;
            LicensePath = licensePathDefault;
        }

        public WindowState WindowState { get; set; }
        public double Height { get; set; }
        public double Width { get; set; }
        public double Left { get; set; }
        public double Top { get; set; }

        public string LicensePath { get; set; }

        public void Save(XmlWriter writer)
        {
            writer.Write(WindowStateKey, (int)WindowState);
            writer.Write(HeightKey, Height);
            writer.Write(WidthKey, Width);
            writer.Write(LeftKey, Left);
            writer.Write(TopKey, Top);

            writer.Write(LicensePathKey, LicensePath);
        }

        public void Load(XmlReader reader)
        {
            WindowState = (WindowState)reader.Read(WindowStateKey, (int)WindowStateDefault);
            Height = reader.Read(HeightKey, HeightDefault);
            Width = reader.Read(WidthKey, WidthDefault);
            Left = reader.Read(LeftKey, LeftDefault);
            Top = reader.Read(TopKey, TopDefault);

            LicensePath = reader.Read(LicensePathKey, licensePathDefault);
        }
    }
}
