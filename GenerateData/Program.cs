using System;
using System.IO;
using System.Linq;

namespace DataGeneration
{
    internal class Program
    {
        private readonly Random rand = new();
        private static void Main ()
        {
            Program program = new();
            program.GenerateData(10_000);
        }

        private void GenerateData (int pointCount)
        {
            double[] gun_id = ScottPlot.DataGen.Consecutive(pointCount);

            DateTime currentDate = new DateTime(1993, 1, 1);
            DateTime[] dates = new DateTime[pointCount];
            for (int i = 0; i < pointCount; i++)
            {
                DateTime date = new DateTime(currentDate.Year, currentDate.Month, currentDate.Day, rand.Next(0, 24), rand.Next(0, 60), rand.Next(0, 60));
                dates[i] = date;
                currentDate = currentDate.AddDays(1);
            }

            double[] fi = ScottPlot.DataGen.RandomWalk(rand, pointCount);
            double[] fv = ScottPlot.DataGen.RandomWalk(rand, pointCount);
            double[] temp = ScottPlot.DataGen.RandomWalk(rand, pointCount);
            double[] emi = ScottPlot.DataGen.RandomWalk(rand, pointCount);

            string[] lines = new string[pointCount];
            lines[0] = "gunID, datetime, FI, FV, temp, emi";

            for (int i = 0; i < pointCount; i++)
            {
                lines[i] = gun_id[i].ToString() + "," + dates[i].ToString("yyyy-M-d H:m:s") + "," + fi[i].ToString() + "," + fv[i].ToString() + "," + temp[i].ToString() + "," + emi[i].ToString();
            }

            File.WriteAllLines("gun.csv", lines);
        }
    }
}  