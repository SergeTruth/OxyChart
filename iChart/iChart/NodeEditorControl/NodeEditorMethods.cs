using System;
using System.Collections.Generic;
using System.Text;
using MindFusion.Diagramming;
using MindFusion.Diagramming.WinForms;
using System.Windows.Forms;

namespace iChart
{
    public partial class MainUI : Form
    {
        public void LoadFromNode(ShapeNode node)
        {
            richTextBox1.Enabled = true;
            richTextBox1.Text = node.ToolTip;
            richTextBox1.Tag = node;

            textBox1.Enabled = true;
            textBox1.Text = node.HyperLink;
        }

        public void SaveToNode(ShapeNode node)
        {
            node.ToolTip = richTextBox1.Text;
            node.HyperLink = textBox1.Text;
        }

        public void SaveToNode()
        {
            if (richTextBox1.Tag is ShapeNode)
            {
                ShapeNode node = (ShapeNode)richTextBox1.Tag;
                node.ToolTip = richTextBox1.Text;
                node.HyperLink = textBox1.Text;
            }
        }

        public void ClearNodeEditor()
        {
            richTextBox1.Text = "";
            richTextBox1.Enabled = false;
            richTextBox1.Tag = null;

            textBox1.Text = "";
            textBox1.Enabled = false;
        }
    }
}
