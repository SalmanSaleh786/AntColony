using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SBSE_Project_2.Models
{
    internal class BilFileReader
    {
        private short[,] elevationData;
        private int numRows;
        private int numCols;
        private double ulxmap; // Upper left corner X coordinate
        private double ulymap; // Upper left corner Y coordinate
        private double cellSize;  // Size of each cell in the raster data

        public BilFileReader(string filePath)
        {
            // Assuming the .bil file is a simple binary file with 16-bit signed integers
            using (BinaryReader bilReader = new BinaryReader(File.Open(filePath, FileMode.Open)))
            {

                    // Read the header information
                    bilReader.BaseStream.Seek(0, SeekOrigin.Begin);
                    numRows = 13248; // Read NROWS
                    numCols = 10388; // Read NCOLS
                    ulxmap = 61.3982088983582; // Read ULXMAP
                    ulymap = 41.7611978375398; // Read ULYMAP
                    cellSize = 0.001373291015625; // Read XDIM (assuming XDIM and YDIM are the same)

                    // Initialize the elevation data array
                    elevationData = new short[numRows, numCols];

                    // Read the elevation data from the file
                    for (int i = 0; i < numRows; i++)
                    {
                        for (int j = 0; j < numCols; j++)
                        {
                            elevationData[i, j] = bilReader.ReadInt16();
                        }
                    }
                
            }
        }

        public double GetAltitude(double latitude, double longitude)
        {
            // Convert latitude and longitude to row and column indices
            int row = (int)Math.Floor((ulymap - latitude) / cellSize);
            int col = (int)Math.Floor((longitude - ulxmap) / cellSize);

            // Check if the given latitude and longitude are within the bounds of the raster data
            if (row < 0 || row >= numRows || col < 0 || col >= numCols)
            {
                return 999999;
                //throw new ArgumentException("Coordinates are outside the bounds of the raster data.");
            }

            // Get altitude at the specified row and column
            double altitude = elevationData[row, col];

            // Check if altitude is the nodata value (-9999)
            if (altitude == -9999)
            {
                throw new ArgumentException("Altitude data not available for the specified coordinates.");
            }

            return altitude;
        }
    }


}
