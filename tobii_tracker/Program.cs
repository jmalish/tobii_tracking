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
            bool testing = false;

            var host = new Host();
            var gazePointDataStream = host.Streams.CreateGazePointDataStream();
            
            Console.Write("Please enter name of file to be created (Leave empty to name as date): "); 

            #region write full data to file
            string fileName = Console.ReadLine();
            if (fileName == "")
            {
                fileName = DateTime.Now.ToString("dd_mm_yyyy_hh_mm_ss");
            }

            string fileLocation = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            Directory.CreateDirectory(fileLocation + "\\eyetracks");
            if (!testing)
            {
                string myFile = fileLocation + "\\eyetracks\\" + fileName + ".csv";

                StreamWriter sw = new StreamWriter(myFile);

                Console.WriteLine($"Writing data to {myFile}");

                sw.WriteLine("Timestamp,X,Y");
                gazePointDataStream.GazePoint(
                (x, y, ts) =>
                {
                    //Console.WriteLine($"{ts},{x},{y}");
                    sw.WriteLine($"{ts},{x},{y}");
                });
                #endregion

                Console.WriteLine("\nPress any key to stop recording and parse data...");
                Console.ReadKey();

                sw.Close();
                host.DisableConnection();
            }

            try
            {
                Parser parser = new Parser(fileLocation, fileName);
                if (parser.ParseData())
                {
                    Console.WriteLine("\nData parsed, press any key to exit...");
                } else
                {
                    Console.WriteLine("\nSomething went wrong, data may not have been parsed" +
                        "\nPress any key to exit...");
                }
                Console.ReadKey();
            }
            catch (Exception exc)
            {
                Console.WriteLine("\nSomething went wrong: " + exc.Message +
                    "\nSome or all data may not have been parsed" +
                    "\nPress any key to exit...");
                Console.ReadKey();
            }
        }
    }
}
