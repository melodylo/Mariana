using System;
using System.IO;

namespace DataGeneration
{
    internal class Program
    {
        private readonly Random rand = new(0);

        private static void Main ()
        {
            Program program = new();
            program.GenerateData(10_000);
        }

        private void GenerateData (int pointCount)
        {
            _ = ScottPlot.DataGen.RandomWalk(rand, pointCount);
            _ = ScottPlot.DataGen.RandomWalk(rand, pointCount);
            _ = ScottPlot.DataGen.RandomWalk(rand, pointCount);
            double[] y = ScottPlot.DataGen.RandomWalk(rand, pointCount);
            double[] y2 = ScottPlot.DataGen.RandomWalk(rand, pointCount);

            string[] lines = new string[pointCount];
            string[] lines2 = new string[pointCount];

            lines[0] = "time, precipitation";
            lines2[0] = "time, wind";

            for (int i = 1; i < pointCount; i++)
            {
                lines[i] = i.ToString() + "," + y[i].ToString();
                lines2[i] = i.ToString() + "," + y2[i].ToString();
            }
            File.WriteAllLines("precipitation.csv", lines);
            File.WriteAllLines("wind.csv", lines2);

        }
    }
}