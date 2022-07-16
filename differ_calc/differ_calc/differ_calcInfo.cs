using Grasshopper;
using Grasshopper.Kernel;
using System;
using System.Drawing;

namespace differ_calc
{
    public class differ_calcInfo : GH_AssemblyInfo
    {
        public override string Name => "gradient";

        //Return a 24x24 pixel bitmap to represent this GHA library.
        public override Bitmap Icon => null;

        //Return a short string describing the purpose of this GHA library.
        public override string Description => "";

        public override Guid Id => new Guid("83C1C4D3-4268-4719-B695-26BCE038FD93");

        //Return a string identifying you or your company.
        public override string AuthorName => "";

        //Return a string representing your preferred contact details.
        public override string AuthorContact => "";
    }
}