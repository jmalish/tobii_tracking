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
            string parsedFile = FileLocation + "\\eyetracks\\" + FileName + "_parsed.json";
            int lineCount = 0;
            int dataPointCount = 0;

            Console.WriteLine("Reading file {0}", myFile);

            #region reading file
            using (StreamReader sr = new StreamReader(myFile))
            {
                string line;
                sr.ReadLine(); // skip first line (contains headers)
                while ((line = sr.ReadLine()) != null)
                {   // 0,0 is top left
                    lineCount++;
                    //int x = Decimal.ToInt32(Math.Round(decimal.Parse(line.Split(',')[1]))); // just, uhh, look somewhere else
                    //int y = Decimal.ToInt32(Math.Round(decimal.Parse(line.Split(',')[2]))); // seriously, please

                    int x = getXcoord(line);
                    int y = getYcoord(line);

                    //Console.WriteLine("x: {0}, y:{1}", x, y);

                    try
                    {
                        coords[x, y]++;
                    }
                    catch (Exception exc)
                    {
                        Console.WriteLine(exc.Message);
                        Console.WriteLine("x: {0}, y:{1}", x, y);
                    }
                    
                }
                Console.WriteLine("File parsed with {0} lines read, writing parsed data to {1}", lineCount, parsedFile);
            }
            #endregion reading file


            #region write to file
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
                        dataPointCount = dataPointCount + coords[x, y];
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
            #endregion write to file

            Console.WriteLine("{0} data points created.", dataPointCount);

            return true;
        }

        int getXcoord(string line)
        {
            string xStr = line.Split(',')[1];
            decimal xDec = Decimal.Parse(xStr);
            xDec = Math.Round(xDec, 0);

            if (xDec < 1)
            {
                xDec = 1;
            }
            else if (xDec > 1920)
            {
                xDec = 1920;
            }

            xDec = xDec - 1;

            int x = Convert.ToInt32(xDec);

            //Console.WriteLine("x: {0}", x);
            return x;
        }

        int getYcoord(string line)
        {
            string yStr = line.Split(',')[2];
            decimal yDec = Decimal.Parse(yStr);
            yDec = Math.Round(yDec, 0);

            if (yDec < 1)
            {
                yDec = 1;
            }
            else if (yDec > 1080)
            {
                yDec = 1080;
            }

            yDec = yDec - 1;

            int y = Convert.ToInt32(yDec);

            //Console.WriteLine("y: {0}", y);
            return y;
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