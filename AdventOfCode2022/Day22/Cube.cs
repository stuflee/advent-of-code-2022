using Microsoft.Diagnostics.Runtime.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdventOfCode2022.Day22
{
    public record struct Coordinate(int x, int y, int z);

    public class Cube
    {
        private Dictionary<Coordinate, Vector> surfaceToMapTranslator;

        public Cube(List<char[]> pointData)
        {
            var cubeSize = 0;
            //Point data is either 3/4 or 4/3 sides, lets work out the dimensions first.
            var maxRowLength = pointData.Max(row => row.Length);
            var height = pointData.Count;
            if (maxRowLength / 3 == height / 4)
            {
                cubeSize = maxRowLength / 3;
            }
            else if (maxRowLength / 4 == height / 3)
            {
                cubeSize = maxRowLength / 4;
            }

            //The starting position for the cube.
            var coordinate = new Coordinate(1, 1, 1);
            //The movement vector for the first face.
            var vector = new Coordinate(1, 1, 0);

            var startPosition = new Vector(0, 0);
            while (TryParseFace())


        }
    }

    public class Face
    {
        public Face(List<char[]> facePointData, Vector horizontalVector, Vector verticalVector)
        {

        }
    }
}
