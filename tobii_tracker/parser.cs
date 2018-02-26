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
            int[,] coords = new int[1920,1080]; // create grid, 1920x1080, same as monitor size
            string myFile = FileLocation + "\\eyetracks\\" + FileName + ".csv"; // create string to access file location
            string parsedFile = FileLocation + "\\eyetracks\\" + FileName + "_parsed.json"; // string to write file, just file location with _parsed on the end
            int lineCount = 0; // holds number of lines, just informational
            double dataPointCount = 0; // holds number of data points, informational

            Console.WriteLine("Reading file {0}", myFile);

            #region reading file
            using (StreamReader sr = new StreamReader(myFile))  // create stream reader
            {
                var totalLineCount = sr.ReadToEnd().Split('\n').Count(); // get total number of lines in file
                sr.DiscardBufferedData();                                // doing this puts us at the end of the file, so we need to
                sr.BaseStream.Seek(0, SeekOrigin.Begin);                 // go back to start of file

                string line; // this will store the line that the loop below saves and reads from
                sr.ReadLine(); // skip first line (contains headers)
                while ((line = sr.ReadLine()) != null) // loop through all lines in the file
                {   // 0,0 is top left
                    int x = getXcoord(line); // get the x coordinate
                    int y = getYcoord(line); // get the y coord

                    if (x >= 0 && y >= 0)
                    {
                        addDataPoints(coords, x, y); // add datapoint to grid
                    }

                    lineCount++; // mark line as read, informational
                    if (lineCount % 1000 == 0) // in order to keep from refreshing console every millisecond and locking things up, just run every 1000 lines
                    {
                        Console.Clear(); // clear console to keep from spamming the window
                        decimal linesReadPercentage = Convert.ToDecimal(lineCount) / Convert.ToDecimal(totalLineCount) * 100; // basic math to get percentage
                        Console.WriteLine("Reading file: {0}%", Math.Round(linesReadPercentage, 0)); // write percentage to console
                    }

                }
                Console.Clear();
                Console.WriteLine("Reading file: 100%");
                Console.WriteLine("File parsed with {0} lines read, writing parsed data to {1}", lineCount, parsedFile);
            }
            #endregion reading file


            #region write to file
            StreamWriter sw = new StreamWriter(parsedFile); // create stream writer to create file
            sw.Write("["); // opening bracket for whole file
            for (int x = coords.GetLength(0) - 1; x >= 0; x--) // loop through all the columns, start from top and work down since plotly goes bottom up
            {
                sw.Write("["); // opening bracket for individual line
                for (int y = 0; y < coords.GetLength(1); y++) // loop through pixel in row, left to right
                {
                    sw.Write(coords[x, y]); // write coordinate info to file
                    dataPointCount++; // update data point count, this probably isn't really helpful anymore, informational
                    if (y != coords.GetLength(1) - 1)  // in order to keep from placing a comma after the last pixel
                    {
                        sw.Write(","); // place comma to seperate point
                    }
                }
                sw.Write("]"); // closing bracket for line
                if (x != 0) // keep from putting a comma at the end of the line
                {
                    sw.Write(","); // comma to seperate lines
                }
            }
            sw.Write("]"); // closing bracket for file
            sw.Close(); // close stream writer
            #endregion write to file

            Console.WriteLine("{0} data points created.", dataPointCount);

            return true;
        }

        int getXcoord(string line)
        {
            string xStr = line.Split(',')[1]; // get x coordinate from line
            decimal xDec = Decimal.Parse(xStr); // convert string to decimal
            xDec = Math.Round(xDec, 0); // round to nearest whole number

            if (xDec < 1 || xDec > 1920) // if a coordinate has a value that's out of range, just delete it
            {
                xDec = -1;
            }

            xDec = xDec - 1; // subtract 1 to get us in line with the 0 indexness of c#

            int x = Convert.ToInt32(xDec); // convert to integer
            
            return x;
        }

        int getYcoord(string line)
        {
            string yStr = line.Split(',')[2]; // get y coordinate from line
            decimal yDec = Decimal.Parse(yStr); // convert string to decimal
            yDec = Math.Round(yDec, 0); // round to nearest whole number

            if (yDec < 1 || yDec > 1080) // if a coordinate has a value that's out of range, just delete it
            {
                yDec = -1;
            }

            yDec = yDec - 1; // subtract 1 to get us in line with the 0 indexness of c#

            int y = Convert.ToInt32(yDec); // convert to integer
            
            return y;
        }

        void addDataPoints(int[,] _coords, int centerX, int centerY)
        {
            Coordinate centerPoint = new Coordinate(centerX, centerY); // this is the central point
            Coordinate pointToCheck = new Coordinate(); // this is used for the point we check against

            int circleRadius = 25;  // the larger the radius, the slower the program goes, see below for more info

            // this section pretty much looks at the surrounding SQUARE of pixels, we then use the pythagorean theorom to find if the given coordinate
            // is too far away, if it is, we ignore it, if not, give it the correct point value
            for (int x = centerX - circleRadius; x < centerX + circleRadius; x++)
            {
                for (int y = centerY - circleRadius; y < centerY + circleRadius; y++)
                {
                    pointToCheck.setCoords(x, y); // set coordinates using the for loop placement

                    if (pointToCheck.X >= 0 && pointToCheck.X < 1920) // make sure the point we're checking is even on the board
                    {
                        if (pointToCheck.Y >= 0 && pointToCheck.Y < 1080) // and again
                        {
                            int dist = centerPoint.getDistanceFromPoint(pointToCheck); // find the distance from the central point (pythagorean theorom

                            if (dist < circleRadius) // if it's farther than the radius, ignore it
                            {
                                // the first line will set the value of the center point to it's current value, plus the radius of the circle, doing down one point as it gets further away
                                // _coords[x, y] = _coords[x, y] + circleRadius - dist; // if it's within the radius, give it a point value depending on it's distance from central point
                                // this line just adds one point to the coordinates score, causing a perfectly "flat" circle instead of the "cone" of values
                                _coords[x, y]++; // if it's within the radius, give it a point value depending on it's distance from central point
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
            // distance formula is (x2-x1)^2+(y2-y1)^2, and then take the square root of that
            double distX = Math.Pow(_checkedPoint.X - this.X, 2); // x2 - x1, then squared
            double disty = Math.Pow(_checkedPoint.Y - this.Y, 2); // y2 - y1, then squared
            double distFinal = Math.Sqrt(distX + disty); // take the previous two lines and get the square root

            return Convert.ToInt32(Math.Round(distFinal, 0)); // conver to int and return
        }
    }
}