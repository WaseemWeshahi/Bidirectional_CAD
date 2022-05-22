using Grasshopper;
using Grasshopper.Kernel;
using System;
using System.Drawing;

namespace MetaComp
{
    public class MetaCompInfo : GH_AssemblyInfo
    {
        public override string Name => "MetaComp";

        //Return a 24x24 pixel bitmap to represent this GHA library.
        public override Bitmap Icon => null;

        //Return a short string describing the purpose of this GHA library.
        public override string Description => "";

        public override Guid Id => new Guid("4AD97BF3-947F-4BF9-80D0-705551982C49");

        //Return a string identifying you or your company.
        public override string AuthorName => "";

        //Return a string representing your preferred contact details.
        public override string AuthorContact => "";
    }
}