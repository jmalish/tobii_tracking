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
            bool parseOnly = false; // setting this to true skips the data recording, and goes straight to parsing a given file
            string desktopLocation = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory); // get desktop location

            var host = new Host(); // create tobii host
            var gazePointDataStream = host.Streams.CreateGazePointDataStream(); // create data stream from host
            
            Console.Write("Please enter name of file to be created\n" +
                "Leave empty to name as date\n" +
                "Or enter 'parse' to parse an existing file, skipping the data recording\n\n" +
                "File Name: "); 

            #region write full data to file
            string fileName = Console.ReadLine(); // get file name from user
            if (fileName == "") // if the file name is empty
            {
                fileName = DateTime.Now.ToString("dd_mm_yyyy_hh_mm_ss"); // just name the file the current date and time
            }
            else if (fileName.ToLower() == "parse")
            {
                parseOnly = true;
                Console.Write("\n\nEnter name of file to parse:");
                fileName = Console.ReadLine();
            } else if (File.Exists(desktopLocation + "\\eyetracks\\" + fileName + ".csv"))
            {
                Console.WriteLine("That file name already exists." +
                    "To overwrite that file, enter 'y', otherwise enter 'n' to parse the existing file:");
                if (Console.ReadLine() != "y") // if user enters 'y', they want to overwrite file
                {
                    parseOnly = true; // otherwise skip file writing and parse
                }
            }
            
            Directory.CreateDirectory(desktopLocation + "\\eyetracks"); // create the 'eyetracks' directory on the desktop, if it exists this is ignored
            if (!parseOnly) // if parseOnly is true, we don't need to record data, so skip this section
            {
                string myFile = desktopLocation + "\\eyetracks\\" + fileName + ".csv"; // create string to point to file

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
                Parser parser = new Parser(desktopLocation, fileName); // create parser
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
