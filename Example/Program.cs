using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using SimpleDXF;

namespace Example {
    class Program {
        static void Main(string[] args) {

            //User needs to specify a file
            if (args.Count() == 0) {
                Console.WriteLine("Example on how use Fergo SimpleDXF library\n" +
                                  "Usage: Example.exe [dxffile]\n\n" +
                                  "This application will output entity data to  a text to the same directory\n\n" +
                                  "Press any key to continue...");
                return;
            }

            //If it's only a filename, include the working directory path before it
            string path = args[0];
            string dir = Path.GetDirectoryName(path);
            string file = Path.GetFileName(path);

            if (dir.Length == 0) {
                dir = Environment.CurrentDirectory;
            }

            path = Path.Combine(dir, file);
            
            if (!File.Exists(path)) {
                Console.WriteLine("File not found\nPress any key to continue...");
                Console.ReadLine();
                return;
            }

            //Loads a file from the command line argument
            Document doc = new Document(path);
            List<Layer> layers;
            List<Line> lines;
            List<Polyline> polylines;
            List<Circle> circles;
            List<Arc> arcs;
            List<Point> points;
            List<Text> texts;

            //Parse the file
            doc.Read();

            //Easier access...
            layers = doc.Layers;
            lines = doc.Lines;
            polylines = doc.Polylines;
            circles = doc.Circles;
            arcs = doc.Arcs;
            points = doc.Points;
            texts = doc.Texts;

            Console.WriteLine("Loading '" + Path.GetFileNameWithoutExtension(path) + "'...\n\n");

            //Create the output path for the text file
            string txtpath = Path.Combine(Path.GetDirectoryName(path), Path.GetFileNameWithoutExtension(path) + ".txt");
            StreamWriter output = new StreamWriter(txtpath);

            //Print everything about each entity... code is very self explanatory
            Console.Write("Processing " + layers.Count + " layers...");
            output.WriteLine("LAYERS:");
            for (int i = 0; i < layers.Count; i++) {
                output.WriteLine("Nome: " + layers[i].Name + "\t - Color: " + layers[i].ColorIndex);
            }
            Console.WriteLine(" DONE!");

            Console.Write("Processing " + lines.Count + " lines...");
            output.WriteLine("\nLINES:");
            for (int i = 0; i < lines.Count; i++) {
                output.WriteLine("Layer: " + lines[i].Layer + "\t - P1: " + Math.Round(lines[i].P1.X, 4) + " " + Math.Round(lines[i].P1.Y, 4) + "\t P2: " + Math.Round(lines[i].P2.X, 4) + " " + Math.Round(lines[i].P2.Y, 4));
            }
            Console.WriteLine(" DONE!");

            Console.Write("Processing " + polylines.Count + " polylines and lwpolylines...");
            output.WriteLine("\nPOLYLINES:");
            for (int i = 0; i < polylines.Count; i++) {
                output.WriteLine("Layer: " + polylines[i].Layer + " - Vertex Count: " + polylines[i].Vertexes.Count() + "\t Closed: " + polylines[i].Closed);
                for (int a = 0; a < polylines[i].Vertexes.Count(); a++) {
                    output.WriteLine("Vertex " + a + ": " + Math.Round(polylines[i].Vertexes[a].Position.X, 4) + " " + Math.Round(polylines[i].Vertexes[a].Position.Y, 4) + "\t Bulge: " + Math.Round(polylines[i].Vertexes[a].Bulge, 5));
                }
                output.WriteLine("");
            }
            Console.WriteLine(" DONE!");

            Console.Write("Processing " + circles.Count + " circles...");
            output.WriteLine("\nCIRCLES:");
            for (int i = 0; i < circles.Count; i++) {
                output.WriteLine("Layer: " + circles[i].Layer + "\t Pos: " + Math.Round(circles[i].Center.X, 4) + " " + Math.Round(circles[i].Center.Y, 4) + "\t Radius: " + Math.Round(circles[i].Radius, 4));
            }
            Console.WriteLine(" DONE!");

            Console.Write("Processing " + arcs.Count + " arcs...");
            output.WriteLine("\nARCS:");
            for (int i = 0; i < arcs.Count; i++) {
                output.WriteLine("Layer: " + arcs[i].Layer + "\t Pos: " + Math.Round(arcs[i].Center.X, 4) + " " + Math.Round(arcs[i].Center.Y, 4) + "\t Angles: " + Math.Round(arcs[i].StartAngle, 0) + " " + Math.Round(arcs[i].EndAngle, 0) + " Rad: " + Math.Round(arcs[i].Radius, 2));
            }
            Console.WriteLine(" DONE!");

            Console.Write("Processing " + points.Count + " points...");
            output.WriteLine("\nPOINTS:");
            for (int i = 0; i < points.Count; i++) {
                output.WriteLine("Layer: " + points[i].Layer + "\t Pos: " + Math.Round(points[i].Position.X, 4) + " " + Math.Round(points[i].Position.Y, 4));
            }
            Console.WriteLine(" DONE!");

            Console.Write("Processing " + texts.Count + " texts...");
            output.WriteLine("\nTEXTS:");
            for (int i = 0; i < texts.Count; i++) {
                output.WriteLine("Layer: " + texts[i].Layer + "\t Pos: " + Math.Round(texts[i].Position.X, 4) + " " + Math.Round(texts[i].Position.Y, 4) + "\t Value: " + texts[i].Value);
            }
            Console.WriteLine(" DONE!\n");

            output.Close();
            Console.WriteLine("File '" + Path.GetFileNameWithoutExtension(txtpath) + "' successfully created!\nPress any key to continue...");
            Console.ReadLine();
        }
    }
}
