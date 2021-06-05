using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace GetTTScontent
{
    class Program
    {
        public class Mini
        {
            public string name { get; set; }
            public string url { get; set; }
        }

        /// <summary>
        /// Downloads OBJ mesh content based on a TTS JSON Save file
        /// </summary>
        /// <param name="args">Unused</param>
        static void Main(string[] args)
        {
            // Create a webclient for download content from the cloud server
            WebClient wc = new WebClient();

            // Create the Assets sub-folder if it does not exist
            if (!System.IO.Directory.Exists("Assets")) { System.IO.Directory.CreateDirectory("Assets"); }

            // Copy default texture
            System.IO.File.Copy("Metal.bmp","Assets/Metal.bmp");

            // Setup key words used for finding mesh references
            string key1 = "\"Name\": \"Custom_Model\"";
            string key2 = "\"Nickname\":";
            string key3 = "\"MeshURL\":";

            // Create a list of minis. Minis are not processed immediately so that we can get a mini count and write progress to the screen
            List<Mini> minis = new List<Mini>();

            // Read the TTS JSON Save file (rename ot Minis.JSON or update the below line)
            string contents = System.IO.File.ReadAllText("minis.json");
            // Kepp processing the content while there are references left
            while(contents.IndexOf(key1)>=0)
            {
                // Find first key
                contents = contents.Substring(contents.IndexOf(key1) + key1.Length);
                // Find second key
                contents = contents.Substring(contents.IndexOf(key2)+key2.Length);
                contents = contents.Substring(contents.IndexOf("\"") + 1);
                // Extract nickname
                string miniName = contents.Substring(0,contents.IndexOf("\""));
                // Find third key
                contents = contents.Substring(contents.IndexOf(key3)+key3.Length);
                contents = contents.Substring(contents.IndexOf("\"") + 1);
                // Extract mesh URL
                string miniMesh = contents.Substring(0, contents.IndexOf("\""));
                contents = contents.Substring(contents.IndexOf("\"") + 1);
                // Add mini info to list
                minis.Add(new Mini() { name = miniName, url = miniMesh });
            }

            // Process each mini in the list
            int counter = 0;
            foreach (Mini mini in minis)
            {
                // Download the mini file (OBJ file) into a string 
                counter++;
                string[] obj = wc.DownloadString(mini.url).Split('\n');
                // Set the default file name based on the nickname
                string saveFile = mini.name + ".obj";
                string saveFileAlt = "";
                // Make corrections to the OBj file for to compensate for the fact that the file and and mesh name are not the same
                for (int i = 0; i < obj.Length; i++)
                {
                    // Change mesh name and material to the nickname
                    if (obj[i].StartsWith("mtllib ")) { obj[i] = "mtllib {MaterialFile}"; }
                    if (obj[i].StartsWith("o ")) { saveFileAlt = obj[i].Substring(2).Trim() + ".obj"; obj[i] = "o "+mini.name+"\n# Nickname: " + obj[i].Substring(2).Trim(); }
                    if (obj[i].StartsWith("usemtl ")) { obj[i] = "usemtl Common"; }
                }
                // Update the Blender file reference if present
                obj[0] = obj[0].Replace("File: ''", "File: '" + saveFile + "'");
                string content = String.Join("\n", obj);

                try
                {
                    // Avoid non-legal file characters when determining save file name
                    saveFile = saveFile.Replace(" ", "_");
                    saveFile = saveFile.Replace(":", "_");
                    saveFile = saveFile.Replace("#", "_");
                    saveFile = saveFile.Replace("__", "_");
                    // Display progress
                    Console.WriteLine(counter + " of " + minis.Count + ": (" + (100 * counter / minis.Count).ToString("0.00") + "%) : Writing " + saveFile);
                    // Check if nickname file already exists (nickname is shorter nicer name but alternate name is more unique)
                    if (!System.IO.File.Exists("Assets/" + saveFile))
                    {
                        // Use nickname for save file. Replace content material file reference with correct file name
                        content = content.Replace("{MaterialFile}", System.IO.Path.GetFileNameWithoutExtension(saveFile) + ".mtl");
                        // Save contents
                        System.IO.File.WriteAllText("Assets/" + saveFile, content);
                        // Copy common material file
                        System.IO.File.Copy("Common.mtl", "Assets/" + System.IO.Path.GetFileNameWithoutExtension(saveFile) + ".mtl");
                    }
                    else
                    {
                        // Use alternate name for save file. Replace content material file reference with correct file name
                        content = content.Replace("{MaterialFile}", System.IO.Path.GetFileNameWithoutExtension(saveFileAlt) + ".mtl");
                        // Save contents
                        System.IO.File.WriteAllText("Assets/" + saveFileAlt, content);
                        // Copy common material file
                        System.IO.File.Copy("Common.mtl", "Assets/" + System.IO.Path.GetFileNameWithoutExtension(saveFileAlt) + ".mtl");
                    }
                }
                catch(Exception)
                {
                }
            }
        }
    }
}
