using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdventOfCode2022.Day22
{
    public interface INavigatable
    {
        public Position Move(Position initialPosition, int distance);
    }
}
