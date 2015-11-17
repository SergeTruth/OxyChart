using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using MindFusion.Diagramming.WinForms;
using System.IO;
using MindFusion.Diagramming;
using FluentSharp.WinForms;
using FluentSharp.CoreLib;
using FluentSharp.CoreLib.API;
using FluentSharp.REPL;

namespace iChart
{
    public partial class MainUI : Form
    {
        public MainUI(String[] args)
        {
            InitializeComponent();
            ApplyHardcodedDefaults();
            LoadDefaults();
            LoadSettings();

            if (args.Length > 0)
            {
                String arg = args[0];
                if (File.Exists(arg))
                {
                    try
                    {
                        LoadFile(arg);
                    }
                    catch (Exception error)
                    {
                        MessageBox.Show(error.Message);
                    }
                }
            }
        }

        public DiagramView GetDiagramView()
        {
            return diagramView1;
        }

        private void undoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MenuUndo();
        }

        private void redoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MenuRedo();
        }

        private void cutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MenuCut();
        }

        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MenuCopy();
        }

        private void pasteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MenuPaste();
        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo(Application.ExecutablePath);
            System.Diagnostics.Process proc = new System.Diagnostics.Process();
            proc.StartInfo = startInfo;
            proc.Start();
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (AskToSaveData()) LoadTheDiagramSequence();
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveTheDiagramSequence();
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveAs();
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            SaveTheDiagramSequence();
        }

        private void findToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SearchForm search = new SearchForm(this);
            search.Show();
        }

        private void saveToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            SaveSettings();
        }

        private void reloadToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            LoadSettings();
        }

        private void saveToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            SaveDefaults();
        }

        private void reloadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LoadDefaults();
        }

        private void importXMLToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult result = dlgImportXML.ShowDialog();
            if (result == DialogResult.OK)
            {
                try
                {
                    XMLChart xmlChart = (XMLChart)LoadXML(dlgImportXML.FileName, typeof(XMLChart));
                    xmlChart.LoadChart(diagramView1.Diagram);
                }
                catch (Exception error)
                {
                    MessageBox.Show(error.Message);
                }
            }
        }

        private void exportXMLToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult result = dlgExportXML.ShowDialog();
            if (result == DialogResult.OK)
            {
                try
                {
                    XMLChart xmlChart = new XMLChart(diagramView1.Diagram);
                    xmlChart.SaveChart(dlgExportXML.FileName);
                }
                catch (Exception error)
                {
                    MessageBox.Show(error.Message, "Save XML");
                }
            }
        }

        private void exportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataExportForm dataExportForm = new DataExportForm(this);
            dataExportForm.Show();
        }

        private void importToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataImportForm dataImportForm = new DataImportForm(this);
            dataImportForm.Show();
        }


        public DiagramNodeCollection GetSelectedNodes()
        {
            return diagramView1.Diagram.Selection.Nodes;
        }

        private void selectAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MenuSelectAll();
        }

        private void savePNGToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult result = dlgSavePic.ShowDialog();

            if (result == DialogResult.OK)
            {
                diagramView1.Diagram.Selection.Clear();
                Bitmap image = diagramView1.Diagram.CreateImage();
                image.Save(dlgSavePic.FileName, System.Drawing.Imaging.ImageFormat.Png);
            }
        }

        private void undoToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            richTextBox1.Undo();
        }

        private void redoToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            richTextBox1.Redo();
        }

        private void cutToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            richTextBox1.Cut();
        }

        private void copyToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            richTextBox1.Copy();
        }

        private void pasteToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            richTextBox1.Paste();
        }

        private void selectAllToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            richTextBox1.SelectAll();
        }

        private void countToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int nodeCount = diagramView1.Diagram.Selection.Nodes.Count;
            MessageBox.Show("" + nodeCount + " nodes selected.");
        }

        private void menuNoteSize_Click(object sender, EventArgs e)
        {
            int nodeCount = diagramView1.Diagram.Selection.Nodes.Count;
            if (nodeCount == 1)
            {
                ShapeNode node = (ShapeNode)diagramView1.Diagram.Selection.Nodes[0];
                NodeWidth = (int)node.Bounds.Width;
                NodeHeight = (int)node.Bounds.Height;
            }
        }

        private void resizeToDefaultToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int nodeCount = diagramView1.Diagram.Selection.Nodes.Count;
            if (nodeCount > 0)
            {
                for (int i = 0; i < nodeCount; i++)
                {
                    ShapeNode node = (ShapeNode)diagramView1.Diagram.Selection.Nodes[i];
                    node.Resize((float)NodeWidth,(float)NodeHeight);
                }
            }
        }

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo(Application.ExecutablePath);
            System.Diagnostics.Process proc = new System.Diagnostics.Process();
            proc.StartInfo = startInfo;
            proc.Start();
        }

        private void MainUI_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!AskToSaveData()) e.Cancel = true;
        }

        private void launchScripterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.script_Me();
        }

        public void setTitle(String title)
        {
            this.invokeOnThread(
                () =>
                {
                    this.Text = title;
                });
        }

        public Diagram getDiagram()
        {
            return diagramView1.Diagram;
        }

        private void cutToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            MenuCut();
        }

        private void copyToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            MenuCopy();
        }

        private void cutToolStripMenuItem3_Click(object sender, EventArgs e)
        {
            MenuCut();
        }

        private void copyToolStripMenuItem3_Click(object sender, EventArgs e)
        {
            MenuCopy();
        }

        private void pasteToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            MenuPaste();
        }

        private void selectAllToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            MenuSelectAll();
        }
    }
}