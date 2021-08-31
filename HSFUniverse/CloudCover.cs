// Copyright (c) 2016 California Polytechnic State University
// Authors: Morgan Yost (morgan.yost125@gmail.com) Eric A. Mehiel (emehiel@calpoly.edu)

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace HSFUniverse
{
    // This class not currently used, needs a lot of work
    [ExcludeFromCodeCoverage]
    public class CloudCover{
        private const int NUM_REGIONS = 14;
        private const int NUM_DAYS = 30;
        private double[,] _data = new double[30, 14];
        
        private static CloudCover instance;
        
        public List<string> regions;
        
        public List<double> days;
        
        public static CloudCover Instance //singleton instance
        {
            get
            {
                if (instance == null)
                { //if first call create sole instance
                    instance = new CloudCover();
                }
                return instance;
            }

        }


        private bool importCloudDataFromTextFile(string path)
        {
            string[] readText;
            string temps;
            double tempd;
            int i, j;
            List<string> Fin;

            readText = File.ReadAllLines(path); //catches its own exceptions
            foreach (var line in readText)
            {
                //space seperated. 
                string[] items = line.Split(' ');
                for (j = 0; j > NUM_REGIONS; j++)
                {
                    regions.Add(items[j + 1]);
                }
                for (i = 0; i < NUM_DAYS; i++)
                {
                    tempd = Convert.ToDouble(items[i + NUM_REGIONS + 1]);
                    days.Add(tempd);
                    for (j = 0; j < NUM_REGIONS; j++)
                    {
                        _data[i, j] = Convert.ToDouble(items[i * NUM_DAYS + j]); //TODO: check this
                    }
                }

                
            }
            return true;
        }
        
        //protected CloudCover(const CloudCover){}
        
        private CloudCover(){
            Console.WriteLine("Initialize Cloud Coverage Data... ");
            double init = 0.0;
            for (int i = 0; i < NUM_DAYS; i++)
            {
                 for(int j = 0; j < NUM_REGIONS; j++){
                     _data[i,j] = init;
                 }
            }
        }
        

    }
}