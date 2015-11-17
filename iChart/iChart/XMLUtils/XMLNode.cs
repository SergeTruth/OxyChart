using System;
using System.Collections.Generic;
using System.Text;
using MindFusion.Diagramming;
using System.ComponentModel;
using System.Xml.Serialization;
using System.Xml;
using System.Xml.Schema;

namespace iChart
{
    public class XMLNode 
    {
        ShapeNode node;
        ProxyNode pnode;

        public XMLNode()
        {
        }

        public XMLNode(ShapeNode _node)
        {
            node = _node;
            pnode = new ProxyNode(node);
            Text = node.Text;
            HyperLink = node.HyperLink;
            Tooltip = node.ToolTip;

            NodeFont = XMLFont.FromFont(node.Font);
            TextColor = System.Drawing.ColorTranslator.ToHtml(pnode.TextColor);
            FillColor = System.Drawing.ColorTranslator.ToHtml(pnode.FillColor);
            FrameColor = System.Drawing.ColorTranslator.ToHtml(pnode.FrameColor);

            FrameThickness = (int)pnode.FrameThickness;
            Width = (int)pnode.Width;
            Height = (int)pnode.Height;
            X = (int)pnode.X;
            Y = (int)pnode.Y;
            ZIndex = pnode.ZIndex;
        }

        String _Text;
        public String Text
        {
            get
            {
                return _Text;
            }
            set
            {
                _Text = value;
            }
        }

        String _HyperLink;
        public String HyperLink
        {
            get
            {
                return _HyperLink;
            }
            set
            {
                _HyperLink = value;
            }
        }

        String _Tooltip;
        public String Tooltip
        {
            get
            {
                return _Tooltip;
            }
            set
            {
                _Tooltip = value;
            }
        }

        XMLFont _NodeFont;
        public XMLFont NodeFont
        {
            get
            {
                return _NodeFont;
            }
            set
            {
                _NodeFont = value;
            }
        }

        String _TextColor;
        public String TextColor
        {
            get
            {
                return _TextColor;
            }
            set
            {
                _TextColor = value;
            }
        }

        String _FillColor;
        public String FillColor
        {
            get
            {
                return _FillColor;
            }
            set
            {
                _FillColor = value;
            }
        }

        String _FrameColor;
        public String FrameColor
        {
            get
            {
                return _FrameColor;
            }
            set
            {
                _FrameColor = value;
            }
        }

        int _FrameThickness;
        public int FrameThickness
        {
            get
            {
                return _FrameThickness;
            }
            set
            {
                _FrameThickness = value;
            }
        }

        int _Width;
        public int Width
        {
            get
            {
                return _Width;
            }
            set
            {
                _Width = value;
            }
        }

        int _Height;
        public int Height
        {
            get
            {
                return _Height;
            }
            set
            {
                _Height = value;
            }
        }

        int _X;
        public int X
        {
            get
            {
                return _X;
            }
            set
            {
                _X = value;
            }
        }

        int _Y;
        public int Y
        {
            get
            {
                return _Y;
            }
            set
            {
                _Y = value;
            }
        }

        int _ZIndex;
        public int ZIndex
        {
            get
            {
                return _ZIndex;
            }
            set
            {
                _ZIndex = value;
            }
        }
    }
}
