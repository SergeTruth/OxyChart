using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using MindFusion.Diagramming;

namespace iChart
{
    public partial class MainUI : Form
    {
        String projectFileName = "Project.ic";
        bool projectFileNameKnown = false;

        void SaveFile(String fileName)
        {
            diagramView1.Diagram.Selection.Clear();
            SaveToNode();
            diagramView1.Diagram.SaveToXml(fileName);
            projectFileName = fileName;
            projectFileNameKnown = true;
            this.Text = "iChart - " + System.IO.Path.GetFileNameWithoutExtension(fileName);
        }

        void LoadFile(String fileName)
        {
            ClearNodeEditor();
            diagramView1.Diagram.LoadFromXml(fileName);
            foreach (DiagramNode node in diagramView1.Diagram.Nodes)
            {
                if (node.Tag != null)
                {
                    if (node.Tag.ToString().Length > 1)
                    {
                        if (node.ToolTip == null | node.ToolTip.Length < 1)
                        {
                            node.ToolTip = node.Tag.ToString();
                        }
                    }
                }
            }
            this.Text = "iChart - " + System.IO.Path.GetFileNameWithoutExtension(fileName);
            projectFileName = fileName;
            projectFileNameKnown = true;
        }

        /// <summary>
        /// Ask the user to save the data.
        /// </summary>
        public bool AskToSaveData()
        {
            if (!diagramView1.Diagram.Dirty | diagramView1.Diagram.Nodes.Count == 0) return true;
            //Set the dialog message.
            String message = "Would you like to save the document ";
            message += projectFileName + "?";
            //Show dialog.
            DialogResult result =
                MessageBox.Show(message, "Save Project?", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
            //Quit if the answer is no.
            if (result == DialogResult.No) return true;
            //Cancel event if the answer is cancel.
            if (result == DialogResult.Cancel)
            {
                //Cancel
                return false;
            }
            //Save the data if the answer is yes.
            if (result == DialogResult.Yes)
            {
                //Check if the file name is known and save without prompting the user if it is.
                if (projectFileNameKnown)
                {
                    QuickSave();
                    return true;
                }
                //If the file name is not known, prompt the user for it, and save.
                else
                {
                    bool saved = SaveAs();
                    if (saved) return true;
                    else return false;
                }
            }
            return false;
        }

        /// <summary>
        /// Save the file without prompting the user.
        /// </summary>
        public void QuickSave()
        {
            if (projectFileNameKnown)
            {
                diagramView1.Diagram.SaveToXml(projectFileName);
            }
            else AskToSaveData();
        }

        /// <summary>
        /// Present the user with the save file dialog and save the file.
        /// </summary>
        public bool SaveAs()
        {
            dlgSave.FileName = projectFileName;

            DialogResult result = dlgSave.ShowDialog();
            if (result == DialogResult.OK)
            {
                projectFileName = dlgSave.FileName;
            }
            else return false;

            SaveFile(projectFileName);
            return true;
        }

        /// <summary>
        /// Present the user with the load file dialog and load the file.
        /// </summary>
        public void LoadTheDiagramSequence()
        {
            DialogResult result = dlgOpen.ShowDialog();
            if (result == DialogResult.OK)
            {
                projectFileName = dlgOpen.FileName;
            }
            else return;

            Diagram diagram = new Diagram();
            diagramView1.Diagram = diagram;
            ApplyHardcodedDefaults();
            LoadDefaults();
            LoadFile(projectFileName);
        }

        /// <summary>
        /// If the file name is known, save the datagram.
        /// If the file name is not known, show the save file dialog.
        /// </summary>        
        public bool SaveTheDiagramSequence()
        {
            if (projectFileNameKnown)
            {
                SaveFile(projectFileName);
                return true;
            }
            else return SaveAs();
        }
    }
}
