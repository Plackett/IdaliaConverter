using System.Diagnostics;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace IdaliaPlot
{
    class Idalia
    {
        static string docPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

        static void Main()
        {
            Console.WriteLine("Welcome to Plackett's KML to GPS Coordinate/Plot converter");
            Console.WriteLine("Type the filepath to your NOAA™️ KML file");
            string filePath = Console.ReadLine();
            if(filePath != "" && filePath != null) {
                try
                {
                    ReadFile(filePath);
                }
                catch(Exception e)
                {
                    Console.WriteLine("Exception: " + e.Message);
                }
                finally
                {
                    Console.WriteLine("Finished! Check your Documents folder for the resulting files.");
                    Console.WriteLine("You can go to https://www.gpsvisualizer.com/ and use IdaliaCombined.txt to map it visually.");
                }
                return;
            }
            else {
                Console.WriteLine("No Valid Input Provided, exiting...");
                return;
            }
        }

        static void ReadFile(string Path)
        {
            Console.WriteLine("Path chosen: " + Path);
            string[] times = Array.Empty<string>();
            string[] winds = Array.Empty<string>();
            string[] coords = Array.Empty<string>();
            int coordgrabbed = 0;
            StreamReader sr = new StreamReader(Path);
            string line;
            while((line = sr.ReadLine()) != null)
            {
                string timetest = getBetween(line,"Valid at:","</td>");
                string windtest = getBetween(line,"Maximum Wind:","</td>");
                string coordtest = getBetween(line,"<coordinates>","</coordinates>");
                if(timetest != "")
                {
                    timetest = timetest.Replace(',', ' ');
                    Array.Resize(ref times, times.Length + 1);
                    times[times.Length - 1] = timetest;
                }
                else if(windtest != "")
                {
                    Array.Resize(ref winds, winds.Length + 1);
                    winds[winds.Length - 1] = windtest;
                }
                else if(coordtest != "" && coordgrabbed == 1)
                {
                    string fixedline = coordtest.Replace(" ", "");
                    string[] coordArray = fixedline.Split(',');
                    coords = coordArray;
                    coordgrabbed = 2;
                } else if(coordtest != "" && coordgrabbed == 0)
                {
                    coordgrabbed = 1;
                }
            }
            writeFiles(times, winds, coords);
        }

        static void writeFiles(string[] times, string[] winds, string[] coords)
        {
            using (StreamWriter outputFile = new StreamWriter(Path.Combine(docPath, "IdaliaTimes.txt")))
            {
                foreach (string line in times)
                {
                    outputFile.WriteLine(line);
                }
                outputFile.Close();
            }
            using (StreamWriter outputFile = new StreamWriter(Path.Combine(docPath, "IdaliaWinds.txt")))
            {
                foreach (string line in winds)
                {
                    outputFile.WriteLine(line);
                }
                outputFile.Close();
            }
            using (StreamWriter outputFile = new StreamWriter(Path.Combine(docPath, "IdaliaCoordinates.txt")))
            {
                outputFile.WriteLine("latitude,longitude");
                string[] fixedcoords = FixCoordinates(coords);
                foreach (string line in fixedcoords)
                {
                    outputFile.WriteLine(line);
                }
                outputFile.Close();
            }
            using (StreamWriter outputFile = new StreamWriter(Path.Combine(docPath, "IdaliaCombined.txt")))
            {
                outputFile.WriteLine("type,name,description,latitude,longitude");
                string[] fixedcoords = FixCoordinates(coords);
                for(int i = 0; i < times.Length; i++)
                {
                    outputFile.Write("T,");
                    outputFile.Write(times[i] + ",");
                    outputFile.Write(winds[i] + ",");
                    outputFile.Write(fixedcoords[i] + "\n");
                }
                outputFile.Close();
            }
            using (StreamWriter outputFile = new StreamWriter(Path.Combine(docPath, "IdaliaCombined.csv")))
            {
                outputFile.WriteLine("type,name,description,latitude,longitude");
                string[] fixedcoords = FixCoordinates(coords);
                for(int i = 0; i < times.Length; i++)
                {
                    outputFile.Write("T,");
                    outputFile.Write(times[i] + ",");
                    outputFile.Write(winds[i] + ",");
                    outputFile.Write(fixedcoords[i] + "\n");
                }
                for(int i = 0; i < times.Length; i++)
                {
                    outputFile.Write("W,");
                    outputFile.Write(times[i] + ",");
                    outputFile.Write(winds[i] + ",");
                    outputFile.Write(fixedcoords[i] + "\n");
                }
                outputFile.Close();
            }
        }

        public static string getBetween(string strSource, string strStart, string strEnd)
        {
            if (strSource.Contains(strStart) && strSource.Contains(strEnd))
            {
                int Start, End;
                Start = strSource.IndexOf(strStart, 0) + strStart.Length;
                End = strSource.IndexOf(strEnd, Start);
                return strSource.Substring(Start, End - Start);
            }

            return "";
        }

        static string[] FixCoordinates(string[] coords)
        {
            string[] newcoords = Array.Empty<string>();
            int index = 0;
            string pair = "";
            foreach(string coordRaw in coords)
            {
                string coord = coordRaw;
                if(coordRaw[0] == '0')
                {
                    coord = coordRaw.Remove(0,1);
                }
                index++;
                switch(index)
                {
                    case 1:
                        pair = coord;
                        break;
                    default:
                        Array.Resize(ref newcoords, newcoords.Length + 1);
                        newcoords[newcoords.Length - 1] = coord + "," + pair;
                        pair = "";
                        index = 0;
                        break;
                }
            }
            return newcoords;
        }
    }
}