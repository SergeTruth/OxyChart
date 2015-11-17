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
    public partial class DataImportForm : Form
    {
        MainUI mainUI;

        public DataImportForm(MainUI _mainUI)
        {
            InitializeComponent();
            mainUI = _mainUI;
        }

        /// <summary>
        /// Import each line of text as a separate note.
        /// </summary>
        public void Split(String input, String[] separator)
        {
            //Diagram to draw the notes in
            Diagram datagram = mainUI.GetDiagramView().Diagram;
            //Coordinates for drawing new notes
            float x = 32;
            float y = 32;

            //Split the text
            String[] buffer = input.Split(separator, StringSplitOptions.RemoveEmptyEntries);

            //Create a note for each line
            foreach (String line in buffer)
            {
                //ShapeNode note = UtilNotes.NewNote(x, y, datagram);
                ShapeNode node = mainUI.MakeNode(x, y);
                y += node.Bounds.Height;
                node.Text = line;
            }
        }

        private void splitByNewlineToolStripMenuItem_Click(object sender, EventArgs e)
        {
            String[] separator = { "\r\n", "\r", "\n" };
            Split(textBox.Text, separator);
        }

        private void splitByDoubleNewLineToolStripMenuItem_Click(object sender, EventArgs e)
        {
            String[] separator = { "\r\n\r\n", "\r\r", "\n\n" };
            Split(textBox.Text, separator);
        }

        private void splitByTabToolStripMenuItem_Click(object sender, EventArgs e)
        {
            String[] separator = { "\t", "\r\n", "\r", "\n" };
            Split(textBox.Text, separator);
        }

        private void parseAsCSVToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //Diagram to draw the notes in
            Diagram datagram = mainUI.GetDiagramView().Diagram;
            //Coordinates for drawing new notes
            float x = 32;
            float y = 32;

            String[] separator = { "\r\n", "\r", "\n" };
            String[] buffer = textBox.Text.Split(separator, StringSplitOptions.RemoveEmptyEntries);
            String[] separator2 = { ", ", " ,", " , ", "," };
            foreach (String text in buffer)
            {
                String[] nodebuffer = text.Split(separator2, StringSplitOptions.RemoveEmptyEntries);
                String nodetext = "";
                for (int i = 0; i < nodebuffer.Length; i++)
                {
                    nodetext += nodebuffer[i] + "\r\n";
                }
                ShapeNode node = mainUI.MakeNode(x, y);
                y += node.Bounds.Height;
                node.Text = nodetext;
            }
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

    }
}