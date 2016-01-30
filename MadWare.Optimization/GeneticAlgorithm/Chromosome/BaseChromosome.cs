using MadWare.Optimization.GeneticAlgorithm.Chromosome.Constraint;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MadWare.Optimization.GeneticAlgorithm.Chromosome
{

    public abstract class BaseChromosome<K> where K : BaseChromosome<K>
    {
        public double? FitnessValue { get; set; }

        public BaseChromosome()
        {
            this.FitnessValue = null;
        }

        public abstract void InitChromosome( Dictionary<string, object> initParams );

        public async Task MutateChromosome()
        {
            await this.Mutate();
            this.FitnessValue = null;
        }

        public async Task<K> CrossoverChromosome(K chromosome)
        {
            var c = await this.Crossover(chromosome);
            this.FitnessValue = null;
            return c;
        }

        public abstract Task InitializeRandom();

        public abstract Task Mutate();

        public abstract Task<K> Crossover(K chromosome);

        public async virtual Task Fitness( IEnumerable<BaseHardConstraint<K>> hardConstraints, IEnumerable<BaseSoftConstraint<K>> softConstraints)
        {
            if (this.FitnessValue.HasValue)
                return;
            
            if (hardConstraints != null && hardConstraints.Count() > 0)
            {
                var hcTasks = hardConstraints.Select(hc => hc.ComputeConstraint((K)this));
                var hcResults = await Task.WhenAll(hcTasks);
                if ( !hcResults.All( r => r ) )
                {
                    this.FitnessValue = new Random().NextDouble() * -1;
                    return;
                }
            }

            if (softConstraints != null && softConstraints.Count() > 0)
            {
                var scTasks = softConstraints.Select( sc => sc.ComputeConstraint((K)this) );
                this.FitnessValue = (await Task.WhenAll(scTasks)).Sum();
                return;
            }

            this.FitnessValue = 0;
        }

    }
}
