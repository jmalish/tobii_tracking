using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace tobii_tracker
{
    class Parser
    {
        public string FileLocation { get; set; }
        public string FileName { get; set; }

        public Parser(string _fileLoc, string _fileName)
        {
            FileLocation = _fileLoc;
            FileName = _fileName;            
        }

        public bool ParseData()
        {
            int[,] coords = new int[1920,1080];

            string myFile = FileLocation + "\\eyetracks\\" + FileName + ".csv";

            Console.WriteLine("Reading file {0}", myFile);


            
            using (StreamReader sr = new StreamReader(myFile))
            {
                string line;
                sr.ReadLine(); // skip first line (contains headers)
                while ((line = sr.ReadLine()) != null)
                {
                    int x = Decimal.ToInt32(Math.Round(decimal.Parse(line.Split(',')[1]))); // just, uhh, look somewhere else
                    int y = Decimal.ToInt32(Math.Round(decimal.Parse(line.Split(',')[2]))); // seriously, please

                    // set floor and ceiling for both x and y
                    if (x < 1)
                    {
                        x = 1;
                    }
                    else if (x > 1920)
                    {
                        x = 1919;
                    }

                    if (y < 1)
                    {
                        y = 1;
                    }
                    else if (y > 1080)
                    {
                        y = 1079;
                    }

                    x = x-1;
                    y = y-1;



                    coords[x, y] = coords[x, y] + 1;
                }

                Console.WriteLine("Completed reading file");
            }

            int test = coords.GetLength(0);
            int test2 = coords.GetLength(1);


            string parsedFile = FileLocation + "\\eyetracks\\" + FileName + "_parsed.json";
            Console.WriteLine("File parsed, writing parsed data to {0}", parsedFile);

            StreamWriter sw = new StreamWriter(parsedFile);

            sw.Write("[");
            for (int x = 0; x < coords.GetLength(0); x++)
            {
                sw.Write("[");
                for (int y = 0; y < coords.GetLength(1); y++)
                {
                    sw.Write(coords[x, y]);
                    if (y != coords.GetLength(1) - 1)
                    {
                        sw.Write(",");
                    }
                }
                sw.Write("]");
                if (x != coords.GetLength(0) - 1)
                {
                    sw.Write(",");
                }
            }
            sw.Write("]");

            sw.Close();

            return true;
        }
    }
}





//sw.Write("[");
//            for (int y = 0; y<coords.GetLength(1); y++) // y
//            {
//                sw.Write("[");
//                for (int x = 0; x<coords.GetLength(1); x++) // x
//                {
//                    // Console.WriteLine("{0}, {1}: {2}", x, y, coords[x, y]);
//                    sw.Write(coords[x, y]);
//                    if (x != coords.GetLength(1) - 1)
//                    {
//                        sw.Write(",");
//                    }
//                }
//                sw.Write("]");
//                if (y != coords.GetLength(0) - 1)
//                {
//                    sw.Write(",");
//                }
//            }
//            sw.Write("]");