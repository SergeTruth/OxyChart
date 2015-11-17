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
    public partial class DataExportForm : Form
    {
        MainUI mainUI;

        public DataExportForm(MainUI _mainUI)
        {
            InitializeComponent();
            mainUI = _mainUI;
            Combine();
        }


        /// <summary>
        /// Combine the text from the the selected notes.
        /// </summary>
        public String CombineTextFromSelectedNotes()
        {
            Diagram datagram = mainUI.GetDiagramView().Diagram;

            String output = "";

            List<ShapeNode> nodes = new List<ShapeNode>();


            foreach (DiagramItem item in datagram.Selection.Nodes)
            {
                if (item is ShapeNode)
                {
                    nodes.Add((ShapeNode)item);
                }
            }

            nodes.Sort(new UtilNodeSort());

            foreach (ShapeNode item in nodes)
            {
                if (item is ShapeNode)
                {
                    ShapeNode note = (ShapeNode)item;
                    output += note.Text;
                    output += "\r\n";
                }
            }
            return output;
        }

        public String ProduceCSV()
        {
            Diagram datagram = mainUI.GetDiagramView().Diagram;

            String output = "";

            List<ShapeNode> nodes = new List<ShapeNode>();

            String[] separator = { "\r\n", "\n", "\r" };

            foreach (DiagramItem item in datagram.Selection.Nodes)
            {
                if (item is ShapeNode)
                {
                    nodes.Add((ShapeNode)item);
                }
            }

            nodes.Sort(new UtilNodeSort());

            foreach (ShapeNode item in nodes)
            {   
                MessageBox.Show(item.Text);
                ShapeNode note = (ShapeNode)item;
                String[] noteLines = note.Text.Split(separator,StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < noteLines.Length - 1; i++)
                {
                    output += noteLines[i] + ", ";
                }
                output += noteLines[noteLines.Length-1] + "\r\n";
            }
            return output;
        }

        /// <summary>
        /// Combine text from selected notes.
        /// </summary>
        private void Combine()
        {
            textBox.Clear();
            textBox.Text = CombineTextFromSelectedNotes();
        }

        private void reloadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Combine();
        }

        private void undoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            textBox.Undo();
        }

        private void redoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            textBox.Redo();
        }

        private void cutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            textBox.Cut();
        }

        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            textBox.Copy();
        }

        private void pasteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            textBox.Paste();
        }

        private void selectAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            textBox.SelectAll();
        }

        private void reloadCSVToolStripMenuItem_Click(object sender, EventArgs e)
        {
            textBox.Clear();
            textBox.Text = ProduceCSV();
        }
    }
}