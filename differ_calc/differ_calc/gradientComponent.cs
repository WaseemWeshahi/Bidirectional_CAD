using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using Grasshopper.Plugin;
using System.Linq;
using System;
using System.IO;

namespace differ_calc
{
    public class gradientComponent : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public gradientComponent()
          : base("Gradient Calculator", "Grad",
            "Calculates the gradient on each input slider",
            "Maths", "Util")
        {
        }
        protected static string log_history = "";
        protected static string last_vert_log = "";
        protected static List<Point3d> last_points = new List<Point3d>();
        protected static List<List<Point3d>> gradient = new List<List<Point3d>>(); // #S x #V
        protected static int iteration_num = 0;
        protected static decimal eps = new decimal(0.001);
        protected static bool in_eps_mode = false;
        protected static List<string> names = new List<string>();
        protected static List<string> Names = new List<string>();
        protected static List<decimal> vals = new List<decimal>();
        protected static List<Grasshopper.Kernel.Special.GH_NumberSlider> sliders = new List<Grasshopper.Kernel.Special.GH_NumberSlider>();
        
        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Mesh", "mesh", "Target mesh", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Toggle", "toggle", "Toggle button", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Reset", "reset", "Panic button", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Result", "result", "Gradient of canvas sliders", GH_ParamAccess.item);
            pManager.AddTextParameter("Vertices", "verts", "Vertices of mesh", GH_ParamAccess.item);

        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Mesh mesh = new Mesh();
            bool toggle = false;
            bool reset = false;
            string log = "";
            if (!DA.GetData(0, ref mesh)) return;
            if (!DA.GetData(1, ref toggle)) return;
            if (!DA.GetData(2, ref reset)) return;

            GH_RhinoScriptInterface GH = new GH_RhinoScriptInterface();
            GH_Document GHdoc = Grasshopper.Instances.ActiveCanvas.Document;

            if (reset)
            {
                in_eps_mode = false;
                return;
            }
            if (!(toggle ^ in_eps_mode))
            {
                DA.SetData(0, log_history);
                DA.SetData(1, last_vert_log);
                return;
            }

            if (!GH.IsEditorLoaded() || GHdoc == null)
            {
                return;
            }

            List<Point3d> vertices = GetVertices(mesh);

            // if we have altered the sliders in previous iteration, we should calculate
            // the difference in vertex positions
            if (in_eps_mode)
            {
                var diff = new List<Point3d>();
                for (int i=0; i < vertices.Count; i++)
                {
                    Point3d dd = (Point3d)(vertices[i] - last_points[i]);
                    dd = Point3d.Divide(dd, (double)eps);
                    dd.X = Math.Round(dd.X, 2);
                    dd.Y = Math.Round(dd.Y, 2);
                    dd.Z = Math.Round(dd.Z, 2);

                    diff.Add(dd);
                } 
                gradient.Add(diff);
                iteration_num--;
                if (GHdoc != null)
                    this.ExpireSolution(true);
            }

            if (!in_eps_mode)
            {
                foreach (var element in GHdoc.Objects)
                {
                    if (IsSlider(element))
                    {
                        var slider = element as Grasshopper.Kernel.Special.GH_NumberSlider;
                        names.Add(slider.NickName);
                        Names.Add(slider.Name);
                        vals.Add(slider.CurrentValue);
                        sliders.Add(slider);
                    }
                }
                in_eps_mode = true;
                iteration_num = names.Count;
                log_history = log;
                last_points = vertices;
            }

            if (iteration_num > 0)
            {
                int i = names.Count - iteration_num;
                if (i > 0)
                    UpdateSlider(names[i - 1], vals[i - 1]); // Reset previous slider
                UpdateSlider(names[i], vals[i] + eps);
                sliders[i].ExpireSolution(true);

            }
            else if (iteration_num == 0)
            {
                // Reset sliders
                for (int i = 0; i < names.Count; i++)
                {
                    UpdateSlider(names[i], vals[i]);
                }
                last_vert_log = "";
                foreach (Point3d vertex in last_points)
                {
                    last_vert_log += '(' + vertex.ToString() + ')' + "\n";
                }
                log = "";
                // Print Result!
                for (int s = 0; s < names.Count; s++)
                {
                    log += names[s] + " : ";
                    for (int v = 0; v < vertices.Count; v++)
                    {
                        log += '(' + gradient[s][v].ToString() + ')' + "\t\t\t";
                    }
                    log += '\n';
                }
                in_eps_mode = false;
                last_points = new List<Point3d>();
                gradient = new List<List<Point3d>>(); // #S x (#Vx3)
                names = new List<string>();
                vals = new List<decimal>();
                sliders = new List<Grasshopper.Kernel.Special.GH_NumberSlider>();
                log_history = log;
            }

            // sliders - the original sliders and their values
            // last_points - the original vertices and their values
            ExportResult("C:/Users/waemw/Desktop/University/Thesis/Bidirectional_CAD/tests/logs/result.txt", log);
            DA.SetData(0, log);
            DA.SetData(1, last_vert_log);
        }
        protected void UpdateSlider(string name, decimal val)
        {
            GH_RhinoScriptInterface GH = new GH_RhinoScriptInterface();
            GH_Document GHdoc = Grasshopper.Instances.ActiveCanvas.Document; //TODO Check if this has to be updated when a new document is opened

            foreach (var element in GHdoc.Objects)
            {
                if (element.GetType().ToString() == "Grasshopper.Kernel.Special.GH_NumberSlider")
                {
                    var slider = element as Grasshopper.Kernel.Special.GH_NumberSlider;

                    if (slider.NickName.Equals(name, StringComparison.Ordinal))
                    {
                        slider.SetSliderValue(val);
                    }
                }
            }
        }
        protected List<Point3d> GetVertices(Mesh mesh)
        {
            List<Point3d> vertices = new List<Point3d>();
            // import vertices
            for(int i = 0; i < mesh.Vertices.Count; i++)
            {
                vertices.Add(mesh.Vertices[i]);
            }
            // Deduplicate
            vertices = vertices.Union(vertices).ToList();
            return vertices;
        }
        protected List<Point3d> GetVerticesV(Mesh mesh)
        {
            List<Point3d> vertices = new List<Point3d>();
            // import vertices
            for (int i = 0; i < mesh.Vertices.Count; i++)
            {
                vertices.Add(mesh.Vertices[i]);
            }
            // Deduplicate
            vertices = vertices.Union(vertices).ToList();
            return vertices;
        }
        protected bool IsSlider(IGH_DocumentObject obj)
        {
            if (obj.GetType().ToString() == "Grasshopper.Kernel.Special.GH_NumberSlider")
            {
                // Filter out sliders here
                var slider = obj as Grasshopper.Kernel.Special.GH_NumberSlider;
                return slider.NickName != "";
            }
            return false;
        }
        private void ScheduleCallback(GH_Document document)
        {
            //this.Component.ExpireSolution(false);
            return;
        }
        protected void ExportResult(string path, string data)
        {
            StreamWriter file = new StreamWriter(path);
            //data.Replace("\t\t\t", "\n");
            file.Write(data);
            file.Close();
            return;

        }
        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// You can add image files to your project resources and access them like this:
        /// return Resources.IconForThisComponent;
        /// </summary>
        protected override System.Drawing.Bitmap Icon => null;

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid => new Guid("EFAD59A0-00D6-426C-8392-A6B2E17912A8");
    }
}