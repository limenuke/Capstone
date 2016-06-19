using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace PictureViewer
{
    class DCMset
    {
        public int numDcms;
        public float pixelSpacing;
        public string[] names;
        public Tuple<bool, Rectangle>[] rectSet;
        public Tuple<bool, Rectangle>[] scaledRectSet;
        public string[] zVals;
        public DCMset()
        {
            names = null;
            numDcms = 0;
        }

        public DCMset(string[] setDCM) {
            names = new string[setDCM.Length];
            setDCM.CopyTo(names, 0);
            numDcms = names.Count();
            rectSet = new Tuple<bool, Rectangle>[names.Count()];
            scaledRectSet = new Tuple<bool, Rectangle>[names.Count()];
            zVals = new string[names.Count()];
            for (int i = 0; i < names.Count(); i++ )
            {
                rectSet[i] = new Tuple<bool, Rectangle>(false, Rectangle.FromLTRB(0, 0, 0, 0));
                scaledRectSet[i] = new Tuple<bool, Rectangle>(false, Rectangle.FromLTRB(0, 0, 0, 0));
            }
        }
        
        public void add (string[] setDCM) {
            names = new string[setDCM.Length];
            setDCM.CopyTo(names, 0);
            numDcms = names.Count();
            zVals = new string[names.Count()];
            rectSet = new Tuple<bool, Rectangle>[names.Count()];
            scaledRectSet = new Tuple<bool, Rectangle>[names.Count()];
            for (int i = 0; i < names.Count(); i++)
            {
               rectSet[i] = new Tuple<bool, Rectangle>(false, Rectangle.FromLTRB(0, 0, 0, 0));
               scaledRectSet[i] = new Tuple<bool, Rectangle>(false, Rectangle.FromLTRB(0, 0, 0, 0));
            }
        }

    }
}
