using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Globalization;
using System.Threading;

namespace SimpleDXF {

    #region DXF Document

    /// <summary>
    /// Encapsulates the entire DXF document, containing it's layers and entities.
    /// </summary>
    public class Document {

        //General declarations
        private StreamReader dxfReader;
        private int dxfLinesRead = 0;

        public List<Layer> Layers { get; set; }
        public List<Line> Lines { get; set; }
        public List<Polyline> Polylines { get; set; }
        public List<Circle> Circles { get; set; }
        public List<Arc> Arcs { get; set; }
        public List<Text> Texts { get; set; }
        public List<Point> Points { get; set; }

        /// <summary>
        /// Initializes a new instance of the <c>DXFDoc</c> class.
        /// </summary>
        /// <param name="dxfFile">The path of the DXF file to load</param>
        public Document(string dxfFile) {
            Layers = new List<Layer>();
            Lines = new List<Line>();
            Polylines = new List<Polyline>();
            Circles = new List<Circle>();
            Arcs = new List<Arc>();
            Texts = new List<Text>();
            Points = new List<Point>();

            //Make sure we read the DXF decimal separator (.) correctly
            CultureInfo cultureInfo = CultureInfo.CurrentCulture;
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

            if (File.Exists(dxfFile)) {
                dxfReader = new StreamReader(dxfFile);
            }
        }

        /// <summary>
        /// Read and parse the DXF file
        /// </summary>
        public void Read() {
            bool entitysection = false;

            CodePair code = this.ReadPair();
            while ((code.Value != "EOF") && (!dxfReader.EndOfStream)) {
                if (code.Code == 0) {
                    //Have we reached the entities section yet?
                    if (!entitysection) {
                        //No, so keep going until we find the ENTIIES section (and since we are here, let's try to read the layers)
                        switch (code.Value) {
                            case "SECTION":
                                string sec = ReadSection(ref code);
                                if (sec == "ENTITIES")
                                    entitysection = true;
                                break;
                            case "LAYER":
                                Layer layer = ReadLayer(ref code);
                                Layers.Add(layer);
                                break;
                            default:
                                code = this.ReadPair();
                                break;
                        }
                    } else {
                        //Yes, so let's read the entities
                        switch (code.Value) {
                            case "LINE":
                                Line line = ReadLine(ref code);
                                Lines.Add(line);
                                break;
                            case "CIRCLE":
                                Circle circle = ReadCircle(ref code);
                                Circles.Add(circle);
                                break;
                            case "ARC":
                                Arc arc = ReadArc(ref code);
                                Arcs.Add(arc);
                                break;
                            case "POINT":
                                Point point = ReadPoint(ref code);
                                Points.Add(point);
                                break;
                            case "TEXT":
                                Text text = ReadText(ref code);
                                Texts.Add(text);
                                break;
                            case "POLYLINE":
                                Polyline polyline = ReadPolyline(ref code);
                                Polylines.Add(polyline);
                                break;
                            case "LWPOLYLINE":
                                Polyline lwpolyline = ReadLwPolyline(ref code);
                                Polylines.Add(lwpolyline);
                                break;
                            default:
                                code = this.ReadPair();
                                break;
                        }
                    }
                } else {
                    code = this.ReadPair();
                }
            }
        }

        /// <summary>
        /// Reads a code/value pair at the current line from DXF file
        /// </summary>
        /// <returns>A CodePair object containing code and value for the current line pair</returns>
        private CodePair ReadPair() {
            string line, value;
            int code;

            line = dxfReader.ReadLine();
            dxfLinesRead++;

            //Only through an exepction if the code value is not numeric, indicating a corrupted file
            if (!int.TryParse(line, out code)) {
                throw new Exception("Invalid code (" + line + ") at line " + this.dxfLinesRead);
            } else {
                value = dxfReader.ReadLine();
                return new CodePair(code, value);
            }
        }

        /// <summary>
        /// Reads the SECTION name from the DXF file
        /// </summary>
        /// <param name="code">A reference to the current CodePair read</param>
        /// <returns>A string containing the section name</returns>
        private string ReadSection(ref CodePair code) {
            string returnval = "";

            code = this.ReadPair();
            while (code.Code != 0) {
                if (code.Code == 2) {
                    returnval = code.Value;
                    break;
                }
                code = this.ReadPair();
            }

            return returnval;
        }

        /// <summary>
        /// Reads the LINE data from the DXF file
        /// </summary>
        /// <param name="code">A reference to the current CodePair read</param>
        /// <returns>A Line object with layer and two point data</returns>
        private Line ReadLine(ref CodePair code) {
            Line returnval = new Line(Vector2d.Zero, Vector2d.Zero, "0");

            code = this.ReadPair();
            while (code.Code != 0) {
                switch (code.Code) {
                    case 8:
                        returnval.Layer = code.Value;
                        break;
                    case 10:
                        returnval.P1.X = double.Parse(code.Value);
                        break;
                    case 20:
                        returnval.P1.Y = double.Parse(code.Value);
                        break;
                    case 11:
                        returnval.P2.X = double.Parse(code.Value);
                        break;
                    case 21:
                        returnval.P2.Y = double.Parse(code.Value);
                        break;
                }
                code = this.ReadPair();
            }

            return returnval;
        }

        /// <summary>
        /// Reads the ARC data from the DXF file
        /// </summary>
        /// <param name="code">A reference to the current CodePair read</param>
        /// <returns>An Arc object with layer, center point, radius, start angle and end angle data</returns>
        private Arc ReadArc(ref CodePair code) {
            Arc returnval = new Arc(Vector2d.Zero, 0, 0, 0, "0");

            code = this.ReadPair();
            while (code.Code != 0) {
                switch (code.Code) {
                    case 8:
                        returnval.Layer = code.Value;
                        break;
                    case 10:
                        returnval.Center.X = double.Parse(code.Value);
                        break;
                    case 20:
                        returnval.Center.Y = double.Parse(code.Value);
                        break;
                    case 40:
                        returnval.Radius = double.Parse(code.Value);
                        break;
                    case 50:
                        returnval.StartAngle = double.Parse(code.Value);
                        break;
                    case 51:
                        returnval.EndAngle = double.Parse(code.Value);
                        break;
                }
                code = this.ReadPair();
            }
            
            return returnval;
        }

        /// <summary>
        /// Reads the LWPOLYLINE data from the DXF file
        /// </summary>
        /// <param name="code">A reference to the current CodePair read</param>
        /// <returns>A Polyline object with layer, closed flag and vertex list data</returns>
        private Polyline ReadLwPolyline(ref CodePair code) {
            Polyline returnval = new Polyline(new List<Vertex>(), "0", false);
            Vertex vtx = new Vertex(Vector2d.Zero);
            int flags = 0;

            code = this.ReadPair();
            while (code.Code != 0) {
                switch (code.Code) {
                    case 8:
                        returnval.Layer = code.Value;
                        break;
                    case 70:
                        flags = int.Parse(code.Value);
                        break;
                    case 10:
                        vtx = new Vertex(Vector2d.Zero);
                        vtx.Position.X = double.Parse(code.Value);
                        break;
                    case 20:
                        vtx.Position.Y = double.Parse(code.Value);
                        returnval.Vertexes.Add(vtx);
                        break;
                    case 42:
                        vtx.Bulge = double.Parse(code.Value);
                        break;
                }
                code = this.ReadPair();
            }

            if ((flags & 1) == 1)
                returnval.Closed = true;

            return returnval;
        }

        /// <summary>
        /// Reads the POLYLINE data from the DXF file
        /// </summary>
        /// <param name="code">A reference to the current CodePair read</param>
        /// <returns>A Polyline object with layer, closed flag and vertex list data</returns>
        private Polyline ReadPolyline(ref CodePair code) {
            Polyline returnval = new Polyline(new List<Vertex>(), "0", false);
            int flags = 0;

            code = this.ReadPair();
            while (code.Code != 0) {
                switch (code.Code) {
                    case 8:
                        returnval.Layer = code.Value;
                        break;
                    case 70:
                        flags = int.Parse(code.Value);
                        break;
                }
                code = this.ReadPair();
            }

            while (code.Value != "SEQEND") {
                if (code.Value == "VERTEX") {
                    Vertex vtx = ReadVertex(ref code);
                    returnval.Vertexes.Add(vtx);
                } else {
                    code = this.ReadPair();
                }
            }

            if ((flags & 1) == 1)
                returnval.Closed = true;

            return returnval;
        }

        /// <summary>
        /// Reads the VERTEX data from the DXF file
        /// </summary>
        /// <param name="code">A reference to the current CodePair read</param>
        /// <returns>A Vertex object with layer, position and bulge data</returns>
        private Vertex ReadVertex(ref CodePair code) {
            Vertex returnval = new Vertex(0, 0, 0, "0");

            code = this.ReadPair();
            while (code.Code != 0) {
                switch (code.Code) {
                    case 8:
                        returnval.Layer = code.Value;
                        break;
                    case 10:
                        returnval.Position.X = double.Parse(code.Value);
                        break;
                    case 20:
                        returnval.Position.Y = double.Parse(code.Value);
                        break;
                    case 42:
                        returnval.Bulge = double.Parse(code.Value);
                        break;
                }
                code = this.ReadPair();
            }

            return returnval;
        }

        /// <summary>
        /// Reads the CIRCLE data from the DXF file
        /// </summary>
        /// <param name="code">A reference to the current CodePair read</param>
        /// <returns>A Circle object with layer, center point and radius data</returns>
        private Circle ReadCircle(ref CodePair code) {
            Circle returnval = new Circle(Vector2d.Zero, 0, "0");

            code = this.ReadPair();
            while (code.Code != 0) {
                switch (code.Code) {
                    case 8:
                        returnval.Layer = code.Value;
                        break;
                    case 10:
                        returnval.Center.X = double.Parse(code.Value);
                        break;
                    case 20:
                        returnval.Center.Y = double.Parse(code.Value);
                        break;
                    case 40:
                        returnval.Radius = double.Parse(code.Value);
                        break;
                }
                code = this.ReadPair();
            }

            return returnval;
        }

        /// <summary>
        /// Reads the POINT data from the DXF file
        /// </summary>
        /// <param name="code">A reference to the current CodePair read</param>
        /// <returns>A Point object with layer and position data</returns>
        private Point ReadPoint(ref CodePair code) {
            Point returnval = new Point(Vector2d.Zero, "0");

            code = this.ReadPair();
            while (code.Code != 0) {
                switch (code.Code) {
                    case 8:
                        returnval.Layer = code.Value;
                        break;
                    case 10:
                        returnval.Position.X = double.Parse(code.Value);
                        break;
                    case 20:
                        returnval.Position.Y = double.Parse(code.Value);
                        break;
                }
                code = this.ReadPair();
            }

            return returnval;
        }

        /// <summary>
        /// Reads the TEXT data from the DXF file
        /// </summary>
        /// <param name="code">A reference to the current CodePair read</param>
        /// <returns>A Text object with layer, value (text) and position data</returns>
        private Text ReadText(ref CodePair code) {
            Text returnval = new Text(Vector2d.Zero, "", "0");

            code = this.ReadPair();
            while (code.Code != 0) {
                switch (code.Code) {
                    case 1:
                        returnval.Value = code.Value;
                        break;
                    case 8:
                        returnval.Layer = code.Value;
                        break;
                    case 10:
                        returnval.Position.X = double.Parse(code.Value);
                        break;
                    case 20:
                        returnval.Position.Y = double.Parse(code.Value);
                        break;
                }
                code = this.ReadPair();
            }

            return returnval;
        }

        /// <summary>
        /// Reads the LAYER data from the DXF file
        /// </summary>
        /// <param name="code">A reference to the current CodePair read</param>
        /// <returns>A Layer object with name and AciColor index</returns>
        private Layer ReadLayer(ref CodePair code) {
            Layer returnval = new Layer("0", 0);

            code = this.ReadPair();
            while (code.Code != 0) {
                switch (code.Code) {
                    case 2:
                        returnval.Name = code.Value;
                        break;
                    case 62:
                        returnval.ColorIndex = int.Parse(code.Value);
                        break;
                }
                code = this.ReadPair();
            }

            return returnval;
        }

    }

    /// <summary>
    /// CodePair class for storing the code/value read from the DXF file
    /// </summary>
    class CodePair {
        public int Code { get; set; }
        public string Value { get; set; }

        /// <summary>
        /// Initialize a new instance of the CodePair object. 
        /// </summary>
        /// <param name="Code">Numeric DXF code</param>
        /// <param name="Value">The value of the corresponding code</param>
        /// <returns>A DXF Vector2d object</returns>
        public CodePair(int Code, string Value) {
            this.Code = Code;
            this.Value = Value;
        }
    }

    #endregion

    #region DXF Entities

    /// <summary>
    /// Defines a 2D point in space. Contains a static method for an origin point.
    /// </summary>
    public class Vector2d {
        public double X { get; set; }
        public double Y { get; set; }

        /// <summary>
        /// Initialize a new instance of the Vector2d object. 
        /// </summary>
        /// <param name="X">X coordinate</param>
        /// <param name="Y">Y coordinate</param>
        /// <returns>A DXF Vector2d object</returns>
        public Vector2d(double X, double Y) {
            this.X = X;
            this.Y = Y;
        }

        /// <summary>
        /// Gets a Vector2d at the origin (0,0)
        /// </summary>
        /// <returns>A DXF Vector2d object</returns>
        public static Vector2d Zero {
            get { return new Vector2d(0, 0); }
        }
    }

    /// <summary>
    /// Defines a DXF vertex, with position, bulge and layer
    /// </summary>
    public class Vertex {
        public Vector2d Position { get; set; }
        public double Bulge { get; set; }
        public string Layer { get; set; }

        /// <summary>
        /// Initialize a new instance of the Vertex object. Bulge and Layer are optional (defaults to 0).
        /// </summary>
        /// <param name="Location">A Vector2d containg X and Y coordinates</param>
        /// <param name="Bulge">The tangent of 1/4 the included angle for an arc segment. Negative if the arc goes clockwise from the start point to the endpoint.</param>
        /// <param name="Layer">Layer name</param>
        /// <returns>A DXF Vertex object</returns>
        public Vertex(Vector2d Location, double Bulge = 0, string Layer = "0") {
            this.Position = Location;
            this.Bulge = Bulge;
            this.Layer = Layer;
        }

        /// <summary>
        /// Initialize a new instance of the Vertex object. Bulge and Layer are optional (defaults to 0).
        /// </summary>
        /// <param name="X">X coordinate</param>
        /// <param name="Y">Y coordinate</param>
        /// <param name="Bulge">The tangent of 1/4 the included angle for an arc segment. Negative if the arc goes clockwise from the start point to the endpoint.</param>
        /// <param name="Layer">Layer name</param>
        /// <returns>A DXF Vertex object</returns>
        public Vertex(double X, double Y, double Bulge = 0, string Layer = "0") {
            this.Position = new Vector2d(0, 0);
            this.Position.X = X;
            this.Position.Y = Y;
            this.Bulge = Bulge;
            this.Layer = Layer;
        }
    }

    /// <summary>
    /// Defines a DXF layer, with it's name and AciColor code
    /// </summary>
    public class Layer {
        public string Name { get; set; }
        public int ColorIndex { get; set; }

        /// <summary>
        /// Initialize a new instance of the Layer object
        /// </summary>
        /// <param name="Name">Layer name</param>
        /// <param name="ColorIndex">The AciColor index for the layer</param>
        /// <returns>A DXF Layer object</returns>
        public Layer(string Name, int ColorIndex) {
            this.Name = Name;
            this.ColorIndex = ColorIndex;
        }
    }

    /// <summary>
    /// Defines a DXF line, with starting and ending point
    /// </summary>
    public class Line {
        public Vector2d P1 { get; set; }
        public Vector2d P2  { get; set; }
        public string Layer { get; set; }

        /// <summary>
        /// Initialize a new instance of the Line object
        /// </summary>
        /// <param name="P1">A Vector2d containg X and Y coordinates of the first point</param>
        /// <param name="P2">A Vector2d containg X and Y coordinates of the second point</param>
        /// <param name="Layer">Layer name</param>
        /// <returns>A DXF Line object</returns>
        public Line(Vector2d P1, Vector2d P2, string Layer) {
            this.P1 = P1;
            this.P2 = P2;
            this.Layer = Layer;
        }

    }

    /// <summary>
    /// Defines a DXF polyline, with it's layer, vertex list and closed flag
    /// </summary>
    public class Polyline {
        public string Layer { get; set; }
        public List<Vertex> Vertexes { get; set; }
        public bool Closed { get; set; }

        /// <summary>
        /// Initialize a new instance of the Polyline object
        /// </summary>
        /// <param name="Vertexes">A Vertex list containg X and Y coordinates of each vertex</param>
        /// <param name="Layer">Layer name</param>
        /// <param name="Closed">Determine if the polyline is opened or closed</param>
        /// <returns>A DXF Polyline object</returns>
        public Polyline(List<Vertex> Vertexes, string Layer, bool Closed) {
            this.Vertexes = Vertexes;
            this.Layer = Layer;
            this.Closed = Closed;
        }
    }

    /// <summary>
    /// Defines a DXF circle, with it's layer, center point and radius
    /// </summary>
    public class Circle {
        public double Radius { get; set; }
        public Vector2d Center { get; set; }
        public string Layer { get; set; }

        /// <summary>
        /// Initialize a new instance of the Circle object
        /// </summary>
        /// <param name="Center">A Vector2d containg X and Y center coordinates</param>
        /// <param name="Radius">Circle radius</param>
        /// <param name="Layer">Layer name</param>
        /// <returns>A DXF Circle object</returns>
        public Circle(Vector2d Center, double Radius, string Layer) {
            this.Center = Center;
            this.Radius = Radius;
            this.Layer = Layer;
        }
    }

    /// <summary>
    /// Defines a DXF arc, with it's layer, center point, radius, start and end angle
    /// </summary>
    public class Arc {
        public string Layer { get; set; }
        public Vector2d Center { get; set; }
        public double Radius { get; set; }
        public double StartAngle { get; set; }
        public double EndAngle { get; set; }

        /// <summary>
        /// Initialize a new instance of the Arc object
        /// </summary>
        /// <param name="Center">A Vector2d containg X and Y center coordinates</param>
        /// <param name="Radius">Arc radius</param>
        /// <param name="StartAng">Starting angle, in degrees</param>
        /// <param name="EndAng">Ending angle, in degrees</param>
        /// <param name="Layer">Layer name</param>
        /// <returns>A DXF Arc object</returns>
        public Arc(Vector2d Center, double Radius, double StartAng, double EndAng, string Layer) {
            this.Center = Center;
            this.Radius = Radius;
            this.StartAngle = StartAng;
            this.EndAngle = EndAng;
            this.Layer = Layer;
        }
    }

    /// <summary>
    /// Defines a DXF text, with its layer, node point and text value
    /// </summary>
    public class Text {
        public string Value { get; set; }
        public string Layer { get; set; }
        public Vector2d Position { get; set; }

        /// <summary>
        /// Initialize a new instance of the Text object
        /// </summary>
        /// <param name="Position">A Vector2d containg  X and Y coordinates</param>
        /// <param name="Value">The text string itself</param>
        /// <param name="Layer">Layer name</param>
        /// <returns>A DXF Text object</returns>
        public Text(Vector2d Position, string Value, string Layer) {
            this.Position = Position;
            this.Value = Value;
            this.Layer = Layer;
        }
    }

    /// <summary>
    /// Defines a DXF point, with it's layer and position
    /// </summary>
    public class Point {
        public Vector2d Position { get; set; }
        public string Layer;

        /// <summary>
        /// Initialize a new instance of the Point object
        /// </summary>
        /// <param name="Position">A Vector2d containg X and Y coordinates</param>
        /// <param name="Layer">Layer name</param>
        /// <returns>A DXF Point object</returns>
        public Point(Vector2d Position, string Layer) {
            this.Position = Position;
            this.Layer = Layer;
        }
    }

    #endregion

    #region DXF Converters

    /// <summary>
    /// Helper class to create polygonal vertexes for circles, arcs and polylines.
    /// </summary>
    public class VertexConverter {

        /// <summary>
        /// Multiply this by an angle in degress to get the result in radians
        /// </summary>
        public const double DegToRad = Math.PI / 180.0;

        /// <summary>
        /// Multiply this by an angle in radians to get the result in degrees
        /// </summary>
        public const double RadToDeg = 180.0 / Math.PI;

        /// <summary>
        /// Get circle vertexes using a given precision. Higher the precision, smoother the circle shape, with an increase in vertex count.
        /// </summary>
        /// <param name="entity">The circle entity</param>
        /// <param name="precision">Shape precision (number of edges). Must be equal or higher than 3</param>
        /// <returns>A 2D vector list containing the circle shape</returns>
        public static List<Vector2d> GetCircleVertexes(Circle entity, int precision = 3) {
            List<Vector2d> coords = new List<Vector2d>();
            double X, Y, R, increment;

            X = entity.Center.X;
            Y = entity.Center.Y;
            R = entity.Radius;

            if (precision < 3)
                precision = 3;

            //High-school unit circle math ;)
            increment = Math.PI * 2 / precision;
            for (int i = 0; i < precision; i++) {
                double sin = Math.Sin(increment * i) * R;
                double cos = Math.Cos(increment * i) * R;

                coords.Add(new Vector2d(X + cos, Y + sin));
            }

            return coords;
        }

        /// <summary>
        /// Get arc vertexes using a given precision. Higher the precision, smoother the arc curve, with an increase in vertex count.
        /// </summary>
        /// <param name="entity">The arc entity</param>
        /// <param name="precision">Arc precision (number of segments). Must be equal or higher than 2</param>
        /// <returns>A 2D vector list containing the arc shape</returns>
        public static List<Vector2d> GetArcVertexes(Arc entity, int precision = 2) {
            List<Vector2d> coords = new List<Vector2d>();

            double start = (entity.StartAngle * DegToRad);
            double end = (entity.EndAngle * DegToRad);
            double angle;

            if (precision < 2)
                precision = 2;

            //Gets the angle increment for the given precision
            if (start > end)
                angle = (end + ((2 * Math.PI) - start)) / precision;
            else
                angle = (end - start) / precision;

            //Basic unit circle math to calculate arc vertex coordinate for a given angle and radius
            for (int i = 0; i <= precision; i++) {
                double sine = (entity.Radius * Math.Sin(start + angle * i));
                double cosine = (entity.Radius * Math.Cos(start + angle * i));
                coords.Add(new Vector2d(cosine + entity.Center.X, sine + entity.Center.Y));
            }

            return coords;
        }

        /// <summary>
        /// Get polyline vertexes using a given precision. Higher precision, smoother the polyline curves will be, with an increase in vertex count.
        /// </summary>
        /// <param name="entity">The polyline entity</param>
        /// <param name="precision">Curve precision (number of segments). Must be equal or higher than 2</param>
        /// <returns>A 2D vector list containing all the polyline vertexes, including straight and curved segments</returns>
        public static List<Vector2d> GetPolyVertexes(Polyline entity, int precision = 2) {
            List<Vector2d> coords = new List<Vector2d>();

            if (precision < 2)
                precision = 2;

            for (int i = 0; i < entity.Vertexes.Count; i++) {

                if (entity.Vertexes[i].Bulge == 0) {
                    coords.Add(new Vector2d(entity.Vertexes[i].Position.X, entity.Vertexes[i].Position.Y));
                } else {
                    if (i != entity.Vertexes.Count - 1) {
                        double bulge = entity.Vertexes[i].Bulge;
                        double p1x = entity.Vertexes[i].Position.X;
                        double p1y = entity.Vertexes[i].Position.Y;
                        double p2x = entity.Vertexes[i + 1].Position.X;
                        double p2y = entity.Vertexes[i + 1].Position.Y;

                        //Definition of bulge, from Autodesk DXF fileformat specs
                        double angulo = Math.Abs(Math.Atan(bulge) * 4);
                        bool girou = false;

                        //For my method, this angle should always be less than 180. 
                        if (angulo >= Math.PI) {
                            angulo = Math.PI * 2 - angulo;
                            girou = true;
                        }

                        //Distance between the two vertexes, the angle between Center-P1 and P1-P2 and the arc radius
                        double distancia = Math.Sqrt(Math.Pow(p1x - p2x, 2) + Math.Pow(p1y - p2y, 2));
                        double alfa = (Math.PI - angulo) / 2;
                        double raio = distancia * Math.Sin(alfa) / Math.Sin(angulo);

                        double xc, yc, angulo1, angulo2, multiplier, incr;

                        //Used to invert the signal of the calculations below
                        if (bulge < 0)
                            multiplier = 1;
                        else
                            multiplier = -1;

                        //Calculates the arc center
                        if (!girou) {
                            xc = ((p1x + p2x) / 2) - multiplier * ((p1y - p2y) / 2) * Math.Sqrt((Math.Pow(2 * raio / distancia, 2)) - 1);
                            yc = ((p1y + p2y) / 2) + multiplier * ((p1x - p2x) / 2) * Math.Sqrt((Math.Pow(2 * raio / distancia, 2)) - 1);
                        } else {
                            xc = ((p1x + p2x) / 2) + multiplier * ((p1y - p2y) / 2) * Math.Sqrt((Math.Pow(2 * raio / distancia, 2)) - 1);
                            yc = ((p1y + p2y) / 2) - multiplier * ((p1x - p2x) / 2) * Math.Sqrt((Math.Pow(2 * raio / distancia, 2)) - 1);
                        }

                        //Invert start and end angle, depending on the bulge (clockwise or counter-clockwise)
                        if (bulge < 0) {
                            angulo1 = Math.PI + Math.Atan2(yc - entity.Vertexes[i + 1].Position.Y, xc - entity.Vertexes[i + 1].Position.X);
                            angulo2 = Math.PI + Math.Atan2(yc - entity.Vertexes[i].Position.Y, xc - entity.Vertexes[i].Position.X);
                        } else {
                            angulo1 = Math.PI + Math.Atan2(yc - entity.Vertexes[i].Position.Y, xc - entity.Vertexes[i].Position.X);
                            angulo2 = Math.PI + Math.Atan2(yc - entity.Vertexes[i + 1].Position.Y, xc - entity.Vertexes[i + 1].Position.X);
                        }

                        //If it's more than 360, subtract 360 to keep it in the 0~359 range
                        if (angulo1 >= Math.PI * 2) angulo1 -= Math.PI * 2;
                        if (angulo2 >= Math.PI * 2) angulo2 -= Math.PI * 2;

                        //Calculate the angle increment for each vertex for the given precision
                        if (angulo1 > angulo2)
                            incr = (angulo2 + ((2 * Math.PI) - angulo1)) / precision;
                        else
                            incr = (angulo2 - angulo1) / precision;

                        //Gets the arc coordinates. If bulge is negative, invert the order
                        if (bulge > 0) {
                            for (int a = 0; a <= precision; a++) {
                                double sine = (Math.Abs(raio) * Math.Sin(angulo1 + incr * a));
                                double cosine = (Math.Abs(raio) * Math.Cos(angulo1 + incr * a));
                                coords.Add(new Vector2d(cosine + xc, sine + yc));
                            }
                        } else {
                            for (int a = precision; a >= 0; a--) {
                                double sine = (Math.Abs(raio) * Math.Sin(angulo1 + incr * a));
                                double cosine = (Math.Abs(raio) * Math.Cos(angulo1 + incr * a));
                                coords.Add(new Vector2d(cosine + xc, sine + yc));
                            }
                        }
                    }

                }
            }

            return coords;
        }

    }

    #endregion

    #region DXF Colors

    /// <summary>
    /// AciColor class, containing all the AciColor codes and ARGB converters
    /// </summary>
    public class AciColor {
        private byte index;

        //Hex values (ARGB) for all the 256 ACI Color indexes
        private static uint[] argb = new uint [256] {
            0xFF000000, 0xFFFF0000, 0xFFFFFF00, 0xFF00FF00, 0xFF00FFFF, 0xFF0000FF, 0xFFFF00FF, 0xFFFFFFFF, //007
            0xFF414141, 0xFF808080, 0xFFFF0000, 0xFFFFAAAA, 0xFFBD0000, 0xFFBD7E7E, 0xFF810000, 0xFF815656, //015
            0xFF680000, 0xFF684545, 0xFF4F0000, 0xFF4F3535, 0xFFFF3F00, 0xFFFFBFAA, 0xFFBD2E00, 0xFFBD8D7E, //023
            0xFF811F00, 0xFF816056, 0xFF681900, 0xFF684E45, 0xFF4F1300, 0xFF4F3B35, 0xFFFF7F00, 0xFFFFD4AA, //031
            0xFFBD5E00, 0xFFBD9D7E, 0xFF814000, 0xFF816B56, 0xFF683400, 0xFF685645, 0xFF4F2700, 0xFF4F4235, //039
            0xFFFFBF00, 0xFFFFEAAA, 0xFFBD8D00, 0xFFBDAD7E, 0xFF816000, 0xFF817656, 0xFF684E00, 0xFF685F45, //047
            0xFF4F3B00, 0xFF4F4935, 0xFFFFFF00, 0xFFFFFFAA, 0xFFBDBD00, 0xFFBDBD7E, 0xFF818100, 0xFF818156, //055
            0xFF686800, 0xFF686845, 0xFF4F4F00, 0xFF4F4F35, 0xFFBFFF00, 0xFFEAFFAA, 0xFF8DBD00, 0xFFADBD7E, //063
            0xFF608100, 0xFF768156, 0xFF4E6800, 0xFF5F6845, 0xFF3B4F00, 0xFF494F35, 0xFF7FFF00, 0xFFD4FFAA, //071
            0xFF5EBD00, 0xFF9DBD7E, 0xFF408100, 0xFF6B8156, 0xFF346800, 0xFF566845, 0xFF274F00, 0xFF424F35, //079
            0xFF3FFF00, 0xFFBFFFAA, 0xFF2EBD00, 0xFF8DBD7E, 0xFF1F8100, 0xFF608156, 0xFF196800, 0xFF4E6845, //087
            0xFF134F00, 0xFF3B4F35, 0xFF00FF00, 0xFFAAFFAA, 0xFF00BD00, 0xFF7EBD7E, 0xFF008100, 0xFF568156, //095
            0xFF006800, 0xFF456845, 0xFF004F00, 0xFF354F35, 0xFF00FF3F, 0xFFAAFFBF, 0xFF00BD2E, 0xFF7EBD8D, //103
            0xFF00811F, 0xFF568160, 0xFF006819, 0xFF45684E, 0xFF004F13, 0xFF354F3B, 0xFF00FF7F, 0xFFAAFFD4, //111
            0xFF00BD5E, 0xFF7EBD9D, 0xFF008140, 0xFF56816B, 0xFF006834, 0xFF456856, 0xFF004F27, 0xFF354F42, //119
            0xFF00FFBF, 0xFFAAFFEA, 0xFF00BD8D, 0xFF7EBDAD, 0xFF008160, 0xFF568176, 0xFF00684E, 0xFF45685F, //127
            0xFF004F3B, 0xFF354F49, 0xFF00FFFF, 0xFFAAFFFF, 0xFF00BDBD, 0xFF7EBDBD, 0xFF008181, 0xFF568181, //135
            0xFF006868, 0xFF456868, 0xFF004F4F, 0xFF354F4F, 0xFF00BFFF, 0xFFAAEAFF, 0xFF008DBD, 0xFF7EADBD, //143
            0xFF006081, 0xFF567681, 0xFF004E68, 0xFF455F68, 0xFF003B4F, 0xFF35494F, 0xFF007FFF, 0xFFAAD4FF, //151
            0xFF005EBD, 0xFF7E9DBD, 0xFF004081, 0xFF566B81, 0xFF003468, 0xFF455668, 0xFF00274F, 0xFF35424F, //159
            0xFF003FFF, 0xFFAABFFF, 0xFF002EBD, 0xFF7E8DBD, 0xFF001F81, 0xFF566081, 0xFF001968, 0xFF454E68, //167
            0xFF00134F, 0xFF353B4F, 0xFF0000FF, 0xFFAAAAFF, 0xFF0000BD, 0xFF7E7EBD, 0xFF000081, 0xFF565681, //175
            0xFF000068, 0xFF454568, 0xFF00004F, 0xFF35354F, 0xFF3F00FF, 0xFFBFAAFF, 0xFF2E00BD, 0xFF8D7EBD, //183
            0xFF1F0081, 0xFF605681, 0xFF190068, 0xFF4E4568, 0xFF13004F, 0xFF3B354F, 0xFF7F00FF, 0xFFD4AAFF, //191
            0xFF5E00BD, 0xFF9D7EBD, 0xFF400081, 0xFF6B5681, 0xFF340068, 0xFF564568, 0xFF27004F, 0xFF42354F, //199
            0xFFBF00FF, 0xFFEAAAFF, 0xFF8D00BD, 0xFFAD7EBD, 0xFF600081, 0xFF765681, 0xFF4E0068, 0xFF5F4568, //207
            0xFF3B004F, 0xFF49354F, 0xFFFF00FF, 0xFFFFAAFF, 0xFFBD00BD, 0xFFBD7EBD, 0xFF810081, 0xFF815681, //215
            0xFF680068, 0xFF684568, 0xFF4F004F, 0xFF4F354F, 0xFFFF00BF, 0xFFFFAAEA, 0xFFBD008D, 0xFFBD7EAD, //223
            0xFF810060, 0xFF815676, 0xFF68004E, 0xFF68455F, 0xFF4F003B, 0xFF4F3549, 0xFFFF007F, 0xFFFFAAD4, //231
            0xFFBD005E, 0xFFBD7E9D, 0xFF810040, 0xFF81566B, 0xFF680034, 0xFF684556, 0xFF4F0027, 0xFF4F3542, //239
            0xFFFF003F, 0xFFFFAABF, 0xFFBD002E, 0xFFBD7E8D, 0xFF81001F, 0xFF815660, 0xFF680019, 0xFF68454E, //247
            0xFF4F0013, 0xFF4F353B, 0xFF333333, 0xFF505050, 0xFF696969, 0xFF828282, 0xFFBEBEBE, 0xFFFFFFFF  //255
        };

        /// <summary>
        /// Initialize a new instance of the AciColor object
        /// </summary>
        /// <param name="index">The AciColor index</param>
        /// <returns>An AciColor object</returns>
        public AciColor(byte Index) {
                this.index = Index;
            
        }

        /// <summary>
        /// Convert current AciColor to ARGB
        /// </summary>
        /// <returns>32bit unsigned integer with the color in ARGB format</returns>
        public uint ToARGB() {
            if (index > 255 || index < 0)
                return 0;
            else
                return argb[index];
        }

        /// <summary>
        /// Convert any AciColor value to ARGB
        /// </summary>
        /// <param name="ACIColor">The AciColor index, from 0 to 2055</param>
        /// <returns>32bit unsigned integer with the color in ARGB format</returns>
        public static uint ACItoARGB(byte AciColor) {
            if (AciColor > 255 || AciColor < 0)
                return 0;
            else
                return argb[AciColor];
        }
    }

    #endregion
}
