using MadWare.Optimization.GeneticAlgorithm.Chromosome;
using MadWare.Optimization.GeneticAlgorithm.Chromosome.Constraint;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MadWare.Optimization.GeneticAlgorithm
{
    public abstract class BaseGeneticAlgorithm<T> where T : BaseChromosome<T>
    {

        protected int MaxNumGenerations { get; set; }

        protected int PopulationSize { get; set; }

        protected int NumCrossoversPerGeneration { get; set; }

        protected double MutationRate { get; set; }

        protected int? ProtectTop { get; set; }

        protected IEnumerable<BaseHardConstraint<T>> HardConstraints { get; set; }

        protected IEnumerable<BaseSoftConstraint<T>> SoftConstraints { get; set; }

        protected Dictionary<string, object> ChromosomeInitParams { get; set; }

        protected Random Rnd { get; set; }

        public BaseGeneticAlgorithm(int maxNumGenerations, int populationSize, int numCrossoversPerGeneration, double mutationRate, int? protectTop, IEnumerable<BaseHardConstraint<T>> hardConstraints, IEnumerable<BaseSoftConstraint<T>> softConstraints, Dictionary<string, object> chromosomeInitParams = null)
        {
            this.MaxNumGenerations = maxNumGenerations;
            this.PopulationSize = populationSize;
            this.NumCrossoversPerGeneration = numCrossoversPerGeneration;
            this.MutationRate = mutationRate;
            this.ProtectTop = protectTop;

            this.HardConstraints = hardConstraints;
            this.SoftConstraints = softConstraints;
            this.ChromosomeInitParams = chromosomeInitParams;

            this.Rnd = new Random();
        }

        protected T CreateRandomChromosome()
        {
            T c = Activator.CreateInstance<T>();
            c.InitChromosome(this.ChromosomeInitParams);
            c.InitializeRandom();

            return c;
        }

        protected virtual Tuple<int, int> PickParents<T>(List<T> population) 
        {
            int p1 = this.Rnd.Next(0, population.Count);
            int p2 = this.Rnd.Next(0, population.Count);

            while (p1 == p2)
            {
                p1 = this.Rnd.Next(0, population.Count);
                p2 = this.Rnd.Next(0, population.Count);
            }

            return new Tuple<int, int>(p1, p2);
        }

        public async virtual Task<T> Optimize()
        {
            List<T> population;

            //create population
            List<Task<T>> populationCreateTasks = new List<Task<T>>();
            for (int i = 0; i < this.PopulationSize; i++)
            {
                var t = Task.Run<T>(() => this.CreateRandomChromosome());
                populationCreateTasks.Add(t);
            }

            population = (await Task.WhenAll(populationCreateTasks)).ToList();

            int g = 0;
            while (g <= this.MaxNumGenerations)
            {
                g++;

                //create new generation
                List<Task<T>> crossoverTasks = new List<Task<T>>();

                Random r = new Random();
                for (int i = 0; i < this.NumCrossoversPerGeneration; i++)
                {
                    var p = this.PickParents(population);

                    crossoverTasks.Add(population[p.Item1].CrossoverChromosome(population[p.Item2]));
                }

                var children = await Task.WhenAll(crossoverTasks);

                //mutate children
                List<Task> mutationTasks = new List<Task>();

                foreach (T bc in children)
                {
                    if (r.NextDouble() > this.MutationRate)
                        continue;

                    mutationTasks.Add(bc.MutateChromosome());
                }

                //mutate population
                var mutationPopulation = this.ProtectTop.HasValue ? population.Skip(this.ProtectTop.Value).ToList() : population;
                var randomIndexes = RandomSelection((int)(this.MutationRate * (double)mutationPopulation.Count), mutationPopulation.Count);

                foreach (int rndIdx in randomIndexes)
                    mutationTasks.Add(mutationPopulation[rndIdx].MutateChromosome());

                await Task.WhenAll(mutationTasks);

                //add children to existing population
                population.AddRange(children.Select(c => (T)c));

                //calculate fitness
                var fitnessTasks = population.Select(c => c.Fitness(this.HardConstraints, this.SoftConstraints));
                await Task.WhenAll(fitnessTasks);

                //make selection
                population = population.OrderByDescending(c => c.FitnessValue).Take(this.PopulationSize).ToList();

                Console.WriteLine("Best fitness: " + population.First().FitnessValue.ToString());
            }

            return population.First();
        }

        public static int[] RandomSelection(int select, int outOf)
        {
            int[] selection = new int[outOf];
            for (int i = 0; i < outOf; i++)
                selection[i] = i;

            Shuffle(selection.ToArray());

            return selection.Take(select).ToArray();
        }

        public static void Shuffle<T>(T[] array)
        {
            Random _random = new Random();

            int n = array.Length;
            for (int i = 0; i < n; i++)
            {
                // NextDouble returns a random number between 0 and 1.
                // ... It is equivalent to Math.random() in Java.
                int r = i + (int)(_random.NextDouble() * (n - i));
                T t = array[r];
                array[r] = array[i];
                array[i] = t;
            }
        }

    }
}
