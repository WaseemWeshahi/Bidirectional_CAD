using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using Grasshopper.Plugin;
using System.Linq;

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
        protected static List<Point3d> last_points = new List<Point3d>();
        protected static List<List<Point3d>> gradient = new List<List<Point3d>>();
        protected static int iteration_num = 0;
        protected static decimal eps = new decimal(0.001);
        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Mesh", "mesh", "Target mesh", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Toggle", "toggle", "Toggle button", GH_ParamAccess.item);

        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Result", "result", "Reversed string", GH_ParamAccess.item);
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

            string log = "";
            if (!DA.GetData(0, ref mesh)) return;
            if (!DA.GetData(1, ref toggle)) return;

            if (!toggle)
            {
                DA.SetData(0, log_history);
                return;
            }

            GH_RhinoScriptInterface GH = new GH_RhinoScriptInterface();
            GH_Document GHdoc = Grasshopper.Instances.ActiveCanvas.Document;


            if (!GH.IsEditorLoaded() || GHdoc == null)
            {
                return;
            }

            List<Point3d> vertices = GetVertices(mesh);

            foreach (Point3d vertex in vertices)
            {
                log += vertex.ToString() + '\n';
            }

            List<Grasshopper.Kernel.Special.GH_NumberSlider> sliders = new List<Grasshopper.Kernel.Special.GH_NumberSlider>();
            List<string> names = new List<string>();
            List<decimal> vals = new List<decimal>();

            foreach (var element in GHdoc.Objects)
            {
                if (element.GetType().ToString() == "Grasshopper.Kernel.Special.GH_NumberSlider")
                {
                    var slider = element as Grasshopper.Kernel.Special.GH_NumberSlider;
                    names.Add(slider.NickName);
                    vals.Add(slider.CurrentValue);
                }
            }

            log += "******** Last saved points: *********" + '\n';
            for (int i = 0; i < vals.Count; i++)
            {
                UpdateSlider(names[i], vals[i] + 3);
            }

            List<Point3d> new_vert = new List<Point3d>();
            foreach (Point3d vertex in mesh.Vertices) // import vertices
            {
                new_vert.Add(vertex);
            }
            new_vert = new_vert.Union(new_vert).ToList(); // Dedpulicate
            foreach (Point3d vertex in last_points)
            {
                log += vertex.ToString() + '\n';
            }

            
            for (int i = 0; i < vals.Count; i++)
            {
                UpdateSlider(names[i], vals[i]);
            }
            
            // sliders - the original sliders and their values
            // vertices - the original vertices and their values
            log_history = log;
            last_points = vertices;
            DA.SetData(0, log);
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
            for(int i = 0; i < vertices.Count; i++)
            {
                vertices.Add(vertices[i]);
            }
            // Deduplicate
            vertices = vertices.Union(vertices).ToList();
            return vertices;
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