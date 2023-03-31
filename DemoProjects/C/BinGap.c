#include <stdarg.h>
#include <string.h>
#include <stdio.h>


enum State { Z, SecondOne, None };
char mybigarr[10];


char * IntToBinString(int n, char *ptr)
{
    
  
    for (unsigned long long mask = 0x8000000000000000; mask; mask >>= 1) 
    {
        if (mask & n)
        {
            *ptr++ = '1';
        }
        else
        {
            *ptr++ = '0';
        }

        //*ptr++ = (!!(mask & n) + '0');
      
    }
    *ptr = '\0';

    return ptr;

 
}

int solution0(int N)
{
    
    char arr[65];
    char* ptrarr = &arr[0];
    IntToBinString(N, ptrarr);

    int z = 0;
    int maxval = 0;
    
    enum State state = None;

    for(int i=0;i<65;i++)
    {
       
        if (state == None && arr[i] == '1')
        {
            state = Z;
            z = 0;
        }
        else
            if (state == Z && arr[i] == '0')
            {
                z++;

            }
            else
                if (state == Z && arr[i] == '1')
                {
                    if (z > maxval)
                    {
                        maxval = z;
                    }
                    z = 0;
                    state = Z;
                }

    }

    return maxval;
}


int main()
{

   int a =  solution0(-99999);
   printf("%d",a);

   
   return 0;
}

