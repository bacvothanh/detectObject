using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DetectObject.Test
{
    public static class StringExtension
    {
        public static int ParseToInt(this string value)
        {
            if (int.TryParse(value, out int result))
            {
                return result;
            }

            return default(int);
        }
    }
}
