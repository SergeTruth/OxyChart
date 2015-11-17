using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace iChart
{
    public partial class MainUI : Form
    {
        void LoadSettings()
        {            
            String settingsFile = Path.GetDirectoryName(Application.ExecutablePath) + "\\settings.xml";
            if (File.Exists(settingsFile))
            {
                try
                {
                    XMLSettings settings = (XMLSettings)LoadXML(settingsFile, typeof(XMLSettings));
                    Relocate(settings.MainX, settings.MainY, settings.MainWidth, settings.MainHeight);
                    MoveSplitter(settings.SplitterDistance);
                }
                catch(Exception error)
                {
                    MessageBox.Show(error.Message);
                }
            }
        }

        void SaveSettings()
        {
            try
            {
                String settingsFile = Path.GetDirectoryName(Application.ExecutablePath) + "\\settings.xml";
                XMLSettings settings = new XMLSettings();
                settings.MainX = this.Location.X;
                settings.MainY = this.Location.Y;
                settings.MainWidth = this.Width;
                settings.MainHeight = this.Height;
                settings.SplitterDistance = splitContainer1.SplitterDistance;
                SaveXML(settings, settingsFile, typeof(XMLSettings));
            }
            catch (Exception error)
            {
                MessageBox.Show(error.Message);
            }
        }

        /// <summary>
        /// Move the main UI window to the specified location.
        /// </summary>
        public void Relocate(int _X, int _Y, int _Width, int _Height)
        {
            this.Width = _Width;
            this.Height = _Height;
            Point newLocation = new Point(_X, _Y);
            this.Location = newLocation;
        }

        public void MoveSplitter(int SplitterDistance)
        {
            splitContainer1.SplitterDistance = SplitterDistance;
        }

    }
}
