using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static AdventOfCode2022.Day17.Day17;

namespace AdventOfCode2022.Day22
{
    public class Landscape : INavigatable
    {
        private readonly List<char[]> landscape;

        public Landscape(List<char[]> landscape)
        {
            this.landscape = landscape;
        }

        public Position Move(Position initialPosition, int distance)
        {
            Vector movementVector;
            switch (initialPosition.d)
            {
                case Direction.Up:
                    movementVector = new(0, -1);
                    break;
                case Direction.Down:
                    movementVector = new(0, 1);
                    break;
                case Direction.Left:
                    movementVector = new(-1, 0);
                    break;
                case Direction.Right:
                    movementVector = new(1, 0);
                    break;
                default:
                    throw new Exception();
            }

            int positionMoved = 0;
            Vector lastValidPosition = initialPosition.v;
            Vector lastPosition = lastValidPosition;
            while (positionMoved < distance)
            {
                var nextProposedPosition = lastPosition.Add(movementVector);
                if (nextProposedPosition.y < 0)
                {
                    nextProposedPosition = nextProposedPosition with { y = landscape.Count - 1 };
                }
                if (nextProposedPosition.y > landscape.Count - 1)
                {
                    nextProposedPosition = nextProposedPosition with { y = 0 };
                }
                if (nextProposedPosition.x < 0)
                {
                    nextProposedPosition = nextProposedPosition with { x = landscape[0].Length - 1 };
                }
                if (nextProposedPosition.x > landscape[0].Length - 1)
                {
                    nextProposedPosition = nextProposedPosition with { x = 0 };
                }
                var row = landscape[nextProposedPosition.y];
                var coordinateValue = ' ';
                if (nextProposedPosition.x < row.Length)
                {
                    coordinateValue = row[nextProposedPosition.x];
                }

                if (coordinateValue == '.')
                {
                    positionMoved += 1;
                    lastValidPosition = nextProposedPosition;
                }
                else if (coordinateValue == '#')
                {
                    break;
                }
                lastPosition = nextProposedPosition;
            }
            return initialPosition with { v = lastValidPosition };
        }
    }

}
