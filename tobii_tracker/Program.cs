using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Tobii.Interaction;

namespace tobii_tracker
{
    class Program
    {
        static void Main(string[] args)
        {
            bool testing = false; // setting this to true skips the eye recording part, useful for testing the parser

            var host = new Host(); // create tobii host
            var gazePointDataStream = host.Streams.CreateGazePointDataStream(); // create data stream from host
            
            Console.Write("Please enter name of file to be created (Leave empty to name as date): "); 

            #region write full data to file
            string fileName = Console.ReadLine(); // get file name from user
            if (fileName == "") // if the file name is empty
            {
                fileName = DateTime.Now.ToString("dd_mm_yyyy_hh_mm_ss"); // just name the file the current date and time
            }

            string fileLocation = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory); // get desktop
            Directory.CreateDirectory(fileLocation + "\\eyetracks"); // create the 'eyetracks' directory on the desktop, if it exists this is ignored
            if (!testing)
            {
                string myFile = fileLocation + "\\eyetracks\\" + fileName + ".csv"; // create string to point to file

                StreamWriter sw = new StreamWriter(myFile); // create stream writer

                Console.WriteLine($"Writing data to {myFile}"); // notify user of file name

                sw.WriteLine("Timestamp,X,Y"); // set header in file
                gazePointDataStream.GazePoint(
                (x, y, ts) =>
                {
                    sw.WriteLine($"{ts},{x},{y}"); // this takes the current time, the x and y coords, and writes them to the file
                });
                #endregion

                Console.WriteLine("\nPress any key to stop recording and parse data..."); // tell user how to stop data recording
                Console.ReadKey(); // when a key is pressed stop reading

                sw.Close(); // close the stream writer
                host.DisableConnection(); // close the tobii host
            }

            try
            {
                Parser parser = new Parser(fileLocation, fileName); // create parser
                if (parser.ParseData()) // tell the parser to do it's thing, it returns a bool that basically says if it was succesful or not
                {
                    Console.WriteLine("\nData parsed, press any key to exit...");
                } else
                {
                    Console.WriteLine("\nSomething went wrong, data may not have been parsed" +
                        "\nPress any key to exit...");
                }
                Console.ReadKey();
            }
            catch (Exception exc) // basic error handling
            {
                Console.WriteLine("\nSomething went wrong: " + exc.Message +
                    "\nSome or all data may not have been parsed" +
                    "\nPress any key to exit...");
                Console.ReadKey();
            }
        }
    }
}
