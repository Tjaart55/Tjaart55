using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WinFormsTest
{
    class Program
    {
         enum State {Z, SecondOne, None }
         State state = State.None;

        public int solution0(int N)
        {
           string s =   Convert.ToString(N, 2);
           int z = 0;
           int maxval = 0;
            string tst = "";
          
            foreach(char c in s)
            {
                tst += c;
                if (state == State.None && c == '1')
                {
                    state = State.Z;
                    //z = 0;
                } 
                else
                if (state == State.Z && c=='0') 
                {
                    z++;
                   
                }else
                if (state == State.Z && c == '1')
                {
                    if(z>maxval)
                    {
                        maxval = z;
                    }
                    z = 0;
                    state = State.Z;
                }
               
            }

            return maxval;
           
        }

    }
}
