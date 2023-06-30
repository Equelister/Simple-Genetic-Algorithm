using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace SimpleGeneticAlgorithm
{
    public static class ToFileClass
    {
        public static void SaveToFile(int best, double funcBest, string date)
        {
            //String date = DateTime.Now.ToString("yyyy-dd-M");
            using (StreamWriter stream = File.AppendText("SGA-Wyniki_MM_" + date +".txt"))
            {
                stream.WriteLine(/*"F(x) = " + */ funcBest.ToString()  + " " /*"; x = " */ + best.ToString() );
            }
        }

    }
}
