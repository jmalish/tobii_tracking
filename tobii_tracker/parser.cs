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
                var totalLineCount = sr.ReadToEnd().Split('\n').Count(); // get total number of lines in file
                sr.DiscardBufferedData();                                // doing this puts us at the end of the file, so we need to
                sr.BaseStream.Seek(0, SeekOrigin.Begin);                 // go back to start of file

                string line;
                sr.ReadLine(); // skip first line (contains headers)
                while ((line = sr.ReadLine()) != null)
                {   // 0,0 is top left
                    int x = getXcoord(line);
                    int y = getYcoord(line);

                    //Console.WriteLine("x: {0}, y:{1}", x, y);

                    addDataPoints(coords, x, y);

                    lineCount++;
                    if (lineCount % 1000 == 0)
                    {
                        Console.Clear();
                        decimal linesReadPercentage = Convert.ToDecimal(lineCount) / Convert.ToDecimal(totalLineCount) * 100;
                        Console.WriteLine("Reading file: {0}%", Math.Round(linesReadPercentage, 0));
                    }

                }
                Console.WriteLine("File parsed with {0} lines read, writing parsed data to {1}", lineCount, parsedFile);
            }
            #endregion reading file


            #region write to file
            StreamWriter sw = new StreamWriter(parsedFile);
            sw.Write("[");
            for (int x = coords.GetLength(0) - 1; x >= 0; x--) // TODO: test
            {
                sw.Write("[");
                for (int y = 0; y < coords.GetLength(1); y++)
                {
                    sw.Write(coords[x, y]);
                    dataPointCount = dataPointCount + coords[x, y];
                    if (y != coords.GetLength(1) - 1) 
                    {
                        sw.Write(",");
                    }
                }
                sw.Write("]");
                if (x != 0) // keep from putting a comma at the end of the line
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

        void addDataPoints(int[,] _coords, int centerX, int centerY)
        {
            Coordinate centerPoint = new Coordinate(centerX, centerY);
            Coordinate pointToCheck = new Coordinate();

            int circleRadius = 25;  // radius is also used for the "score" of the point

            for (int x = centerX - circleRadius; x < centerX + circleRadius; x++)
            {
                for (int y = centerY - circleRadius; y < centerY + circleRadius; y++)
                {
                    pointToCheck.setCoords(x, y);

                    if (pointToCheck.X >= 0 && pointToCheck.X < 1920)
                    {
                        if (pointToCheck.Y >= 0 && pointToCheck.Y < 1080)
                        {
                            int dist = centerPoint.getDistanceFromPoint(pointToCheck);

                            if (dist < circleRadius)
                            {
                                _coords[x, y] = _coords[x, y] + circleRadius - dist;
                            }
                        }
                    }
                }
            }
        }
    }

    public struct Coordinate
    {
        public int X { get; set; }
        public int Y { get; set; }

        public Coordinate(int _x, int _y)
        {
            X = _x;
            Y = _y;
        }

        public void setCoords(int _x, int _y)
        {
            X = _x;
            Y = _y;
        }

        public int getDistanceFromPoint(Coordinate _checkedPoint)
        {
            double distX = Math.Pow(_checkedPoint.X - this.X, 2);
            double disty = Math.Pow(_checkedPoint.Y - this.Y, 2);
            double distFinal = Math.Sqrt(distX + disty);

            return Convert.ToInt32(Math.Round(distFinal, 0));
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