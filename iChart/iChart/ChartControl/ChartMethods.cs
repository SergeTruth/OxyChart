using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using MindFusion.Diagramming;

namespace iChart
{
    public partial class MainUI : Form
    {
        public ShapeNode MakeNode(float x, float y)
        {
            ShapeNode note = diagramView1.Diagram.Factory.CreateShapeNode(x, y,
                NodeWidth,
                NodeHeight);
            note.Tag = "";
            note.HyperLink = "";
            note.Selected = true;
            //adjust the box properties
            ProxyNode node = new ProxyNode(note);
            node.FillColor = NodeFillColor;
            node.FrameColor = NodeFrameColor;
            node.FrameThickness = NodeFrameThickness;
            node.SnapToGrid();
            return note;
        }

        public ShapeNode MakeNode(float x, float y, float width, float height)
        {
            ShapeNode note = diagramView1.Diagram.Factory.CreateShapeNode(x, y,
                width,
                height);
            note.Tag = "";
            note.HyperLink = "";
            note.Selected = true;
            //adjust the box properties
            ProxyNode node = new ProxyNode(note);
            node.FillColor = NodeFillColor;
            node.FrameColor = NodeFrameColor;
            node.FrameThickness = NodeFrameThickness;
            node.SnapToGrid();
            return note;
        }

        public void CenterPoint(float midX, float midY)
        {
            diagramView1.ScrollX = midX - (diagramView1.Bounds.Width / 2) / (diagramView1.ZoomFactor / 100);
            diagramView1.ScrollY = midY - (diagramView1.Bounds.Height / 2) / (diagramView1.ZoomFactor / 100);
        }

        /// <summary>
        /// Move the visible space to show the specified note.
        /// </summary>
        public void FocusOn(ShapeNode node)
        {

            //float posX = node.Bounds.X;
            //float posY = node.Bounds.Y;
            float midX = node.Bounds.X + node.Bounds.Width / 2;
            float midY = node.Bounds.Y + node.Bounds.Height / 2;
            //datagramView.ScrollTo(posX, posY);
            CenterPoint(midX, midY);
            diagramView1.Diagram.Selection.Clear();
            diagramView1.Diagram.Selection.AddItem(node);
        }

        /// <summary>
        /// Cut selected object(s).
        /// </summary>
        public void Cut()
        {
            if (editing)
            {
                ((TextBox)editControl).Cut();
            }
            else
            {
                dX = 0;
                dY = 0;
                diagramView1.CutToClipboard(true, true);
            }
        }

        /// <summary>
        /// Copy selected object(s).
        /// </summary>
        public void Copy()
        {
            if (editing)
            {
                ((TextBox)editControl).Copy();
            }
            else
            {
                dX = 0;
                dY = 0;
                foreach (ShapeNode note in diagramView1.Diagram.Selection.Nodes)
                {
                    dY += (int)note.Bounds.Height;
                }
                diagramView1.CopyToClipboard(true, true);
            }
        }


        /// <summary>
        /// Paste object(s) from clipboard.
        /// </summary>
        public void Paste()
        {
            if (editing)
            {
                ((TextBox)editControl).Paste();
            }
            else if (diagramView1.Diagram.Selection.Nodes.Count == 1 && Clipboard.ContainsText())
            {
                if (diagramView1.Diagram.Selection.Nodes[0] is ShapeNode)
                {
                    ShapeNode node = (ShapeNode)diagramView1.Diagram.Selection.Nodes[0];
                    if (node.Text == "" && Clipboard.ContainsText()) node.Text = Clipboard.GetText();
                }
            }
            else
            {
                diagramView1.PasteFromClipboard(dX, dY);
                dX += 0;
                //Fix a strange font bug and increase the offsets by the size of the pasted object(s).
                foreach (ShapeNode note in diagramView1.Diagram.Selection.Nodes)
                {
                    note.Font = note.Font;
                    dY += (int)note.Bounds.Height;
                    ProxyNode node = new ProxyNode(note);
                    node.SnapToGrid();
                }
            }
        }

        /// <summary>
        /// Select all items in the datagram.
        /// </summary>
        public void SelectAll()
        {
            if (editing)
            {
                ((TextBox)editControl).SelectAll();
            }
            else if (splitContainer2.ActiveControl == richTextBox1)
            {
                richTextBox1.SelectAll();
            }
            else if (splitContainer2.ActiveControl == textBox1)
            {
                textBox1.SelectAll();
            }
            else
            {
                foreach (DiagramItem item in diagramView1.Diagram.Items)
                {
                    diagramView1.Diagram.Selection.AddItem(item);
                }
            }
        }

          public void Undo()
        {
            if (editing)
            {
                ((TextBox)editControl).Undo();
            }
            else
            {
                diagramView1.Diagram.UndoManager.Undo();
            }
        }

        public void Redo()
        {
            if (editing)
            {
                ((TextBox)editControl).Undo();
            }
            else
            {
                diagramView1.Diagram.UndoManager.Redo();
            }
        }
    }
}
