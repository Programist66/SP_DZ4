using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SP_DZ4
{
    public class CopiedFile
    {
        public string FileName = "";
        public double Progress = 0;

        public CopiedFile(string fileName, double progress)
        {
            FileName = fileName;
            Progress = progress;
        }
    }
}
