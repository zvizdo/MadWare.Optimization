using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MadWare.Optimization.GeneticAlgorithm.Chromosome.Constraint
{
    public abstract class BaseConstraint<T, K> where K : BaseChromosome<K>
    {

        public abstract Task<T> ComputeConstraint( K chromosome );

    }
}
