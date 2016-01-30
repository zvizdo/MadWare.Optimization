using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MadWare.Optimization.GeneticAlgorithm.Chromosome.Constraint
{
    public abstract class BaseSoftConstraint<T> : BaseConstraint<double, T> where T : BaseChromosome<T>
    {
    }
}
