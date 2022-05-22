using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;

using Grasshopper.Plugin;
namespace MetaComp
{
    public class MetaCompComponent : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public MetaCompComponent()
          : base("MetaComp", "Meta",
            "Controls the inputs in the canvas.",
            "Curve", "Primitive")
            //"Params", "Input")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("Value", "val", "the destination value", GH_ParamAccess.item, 0);
            pManager.AddGenericParameter("inputStr", "target", "Target slider's name", GH_ParamAccess.item);

        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            double val = 0.0;
            string compName = "";

            if (!DA.GetData(0, ref val)) return;
            if (!DA.GetData(1, ref compName)) return;

            if (val < 0.0)
            {
                AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Not dealing with negative values for now");
                return;
            }

            GH_RhinoScriptInterface GH = new GH_RhinoScriptInterface();
            GH_Document GHdoc = Grasshopper.Instances.ActiveCanvas.Document; //TODO Check if this has to be updated when a new document is opened


            foreach (var element in GHdoc.Objects)
            {
                //Print(element.GetType().ToString());
                if (element.GetType().ToString() == "Grasshopper.Kernel.Special.GH_NumberSlider")
                {
                    var slider = element as Grasshopper.Kernel.Special.GH_NumberSlider;
                    if (slider.NickName.Equals(compName, StringComparison.Ordinal))
                    {
                        slider.SetSliderValue((decimal)val);
                    }
                }
            }
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
        public override Guid ComponentGuid => new Guid("ED812A78-D4F9-4413-A2E5-2874FEA80066");
    }
}