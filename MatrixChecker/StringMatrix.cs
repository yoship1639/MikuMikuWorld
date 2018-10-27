using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatrixChecker
{
    class StringMatrix
    {
        public string[,] M;

        public StringMatrix()
        {
            M = new string[4, 4];
            for (var y = 0; y < 4; y++)
            {
                for (var x = 0; x < 4; x++)
                {
                    if (y == x) M[y, x] = "1";
                    else M[y, x] = "";
                }
            }
        }

        public static StringMatrix operator *(StringMatrix m1, StringMatrix m2)
        {
            var m = new StringMatrix();
            for (var y = 0; y < 4; y++)
            {
                for (var x = 0; x < 4; x++)
                {
                    var str = "";
                    for (var i = 0; i < 4; i++)
                    {
                        var s1 = m1.M[y, i];
                        var s2 = m2.M[i, x];

                        if (s1 == "" || s2 == "") continue;
                        if (s1 == "1" && s2 == "1") str += "1";
                        else if (s1 == "1" && s2 != "1")
                        {
                            if (s2[0] == '-') str += s2;
                            else if (str.Length > 0) str += "+" + s2;
                            else str += s2;
                        } 
                        else if (s1 != "1" && s2 == "1")
                        {
                            if (s1[0] == '-') str += s1;
                            else if (str.Length > 0) str += "+" + s1;
                            else str += s1;
                        }
                        else str += string.Format("({0}*{1})", s1, s2);
                    }
                    m.M[y, x] = str;
                }
            }

            return m;
        }

        public string Row0 { get { return string.Format("{0}, {1}, {2}, {3}", M[0, 0], M[0, 1], M[0, 2], M[0, 3]); } }
        public string Row1 { get { return string.Format("{0}, {1}, {2}, {3}", M[1, 0], M[1, 1], M[1, 2], M[1, 3]); } }
        public string Row2 { get { return string.Format("{0}, {1}, {2}, {3}", M[2, 0], M[2, 1], M[2, 2], M[2, 3]); } }
        public string Row3 { get { return string.Format("{0}, {1}, {2}, {3}", M[3, 0], M[3, 1], M[3, 2], M[3, 3]); } }

        public override string ToString()
        {
            return Row0 + "\n" + Row1 + "\n" + Row2 + "\n" + Row3;
        }
    }
}
