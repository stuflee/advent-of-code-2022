using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static AdventOfCode2022.Day17.Day17;

namespace AdventOfCode2022.Day22
{
    public record struct Vector(int x, int y)
    {
        public Vector Add(Vector v)
        {
            return this with { x = this.x + v.x, y = this.y + v.y };
        }
    }

    public record struct Position(Vector v, Direction d);

}
