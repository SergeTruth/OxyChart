using System;
using System.IO;
using System.Xml;
using System.Text;
using System.ComponentModel;
using System.Xml.Serialization;
using System.Collections.Generic;

namespace iChart
{
    public class XMLSettings
    {
        int _SplitterDistance;
        int _MainWidth;
        int _MainHeight;
        int _MainX;
        int _MainY;

        public XMLSettings()
        {
            SplitterDistance = 480;
            MainWidth = 0;
            MainHeight = 0;
            MainX = 0;
            MainY = 0;
        }


        [Category("Window Positions")]
        /// <summary>
        /// The Editor window width.
        /// </summary>
        public int SplitterDistance
        {
            get
            {
                return _SplitterDistance;
            }
            set
            {
                _SplitterDistance = value;
            }
        }

        [Category("Window Positions")]
        /// <summary>
        /// The Main window width.
        /// </summary>
        public int MainWidth
        {
            get
            {
                return _MainWidth;
            }
            set
            {
                _MainWidth = value;
            }
        }

        [Category("Window Positions")]
        /// <summary>
        /// The main window height.
        /// </summary>
        public int MainHeight
        {
            get
            {
                return _MainHeight;
            }
            set
            {
                _MainHeight = value;
            }
        }

        [Category("Window Positions")]
        /// <summary>
        /// The main window X coordinate.
        /// </summary>
        public int MainX
        {
            get
            {
                return _MainX;
            }
            set
            {
                _MainX = value;
            }
        }

        [Category("Window Positions")]
        /// <summary>
        /// The main window Y coordinate.
        /// </summary>
        public int MainY
        {
            get
            {
                return _MainY;
            }
            set
            {
                _MainY = value;
            }
        }
    }
}
