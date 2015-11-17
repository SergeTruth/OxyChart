using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace iChart
{
    public partial class MainUI : Form
    {
        /// <summary>
        /// Save an object as an XML file.
        /// </summary>
        public static void SaveXML(Object obj, String file, Type type)
        {
            XmlSerializer xmlSer = new XmlSerializer(type);
            try
            {
                TextWriter writer = new StreamWriter(file);
                xmlSer.Serialize(writer, obj);
                writer.Close();
            }
            catch (Exception error)
            {

                MainUI.ShowError("UtilIO.SaveXML", "Cannot save XML to " + file + "\r\n\r\n" + error.Message);
            }
        }


        /// <summary>
        /// Load an object from an XML file.
        /// </summary>
        public static Object LoadXML(String file, Type type)
        {
            Object obj = null;
            XmlSerializer xmlSer = new XmlSerializer(type);
            try
            {
                TextReader reader = new StreamReader(file);
                obj = xmlSer.Deserialize(reader);
                reader.Close();
                reader.Dispose();
            }
            catch (Exception error)
            {
                MainUI.ShowError("UtilIO.LoadXML", "Cannot read XML from " + file + "\r\n\r\n" + error.Message);
            }
            return obj;
        }


        /// <summary>
        /// Save an array to XML.
        /// </summary>
        public static void SaveXMLArray(ArrayList array, String fileName, Type type)
        {
            Type[] extraTypes = new Type[1];
            extraTypes[0] = type;
            XmlSerializer XmlSer = new XmlSerializer(typeof(ArrayList), extraTypes);
            try
            {
                TextWriter writer = new StreamWriter(fileName);
                XmlSer.Serialize(writer, array);
                writer.Close();
                writer.Dispose();
            }
            catch (Exception error)
            {
                MainUI.ShowError("UtilIO.SaveXMLArray", error.Message);
            }
        }


        /// <summary>
        /// Load an array from XML.
        /// </summary>
        public static ArrayList LoadXMLArray(String fileName, Type type)
        {
            Object obj = null;
            Type[] extraTypes = new Type[1];
            extraTypes[0] = type;
            XmlSerializer XmlSer = new XmlSerializer(typeof(ArrayList), extraTypes);
            try
            {
                TextReader reader = new StreamReader(fileName);
                obj = XmlSer.Deserialize(reader);
                reader.Close();
                reader.Dispose();
            }
            catch (Exception error)
            {
                ShowError("UtilIO.LoadXMLArray", error.Message);
            }
            if (obj != null)
            {
                ArrayList arrayList = (ArrayList)obj;
                return arrayList;
            }
            else return null;
        }

        /// <summary>
        /// Show an error message.
        /// </summary>
        public static DialogResult ShowError(String title, String message)
        {
            return MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
