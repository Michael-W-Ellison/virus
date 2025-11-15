using BiochemSimulator.Models;
using System.Windows;
using System.Windows.Media;

namespace BiochemSimulator.Engine
{
    public class OrganismManager
    {
        private List<Organism> _organisms;
        private Random _random;
        private ChemistryEngine _chemistryEngine;
        private double _screenWidth;
        private double _screenHeight;
        private DateTime _outbreakStartTime;

        public List<Organism> Organisms => _organisms;
        public int TotalOrganismsCreated { get; private set; }
        public int GenerationsEvolved { get; private set; }

        public OrganismManager(ChemistryEngine chemistryEngine, double screenWidth, double screenHeight)
        {
            _organisms = new List<Organism>();
            _random = new Random();
            _chemistryEngine = chemistryEngine;
            _screenWidth = screenWidth;
            _screenHeight = screenHeight;
            TotalOrganismsCreated = 0;
            GenerationsEvolved = 0;
        }

        public void InitializeOutbreak(int initialCount = 10)
        {
            _outbreakStartTime = DateTime.Now;
            _organisms.Clear();

            for (int i = 0; i < initialCount; i++)
            {
                var organism = new Organism
                {
                    Position = new Point(
                        _random.NextDouble() * _screenWidth,
                        _random.NextDouble() * _screenHeight
                    ),
                    Size = 15 + _random.NextDouble() * 10,
                    Color = Color.FromRgb(
                        (byte)(100 + _random.Next(100)),
                        (byte)(150 + _random.Next(50)),
                        (byte)(50 + _random.Next(50))
                    ),
                    Type = OrganismType.Virus,
                    Generation = 1,
                    ReproductionRate = 0.05,
                    MutationRate = 0.15
                };

                _organisms.Add(organism);
                TotalOrganismsCreated++;
            }
        }

        public void Update(double deltaTime)
        {
            // Remove dead organisms
            _organisms.RemoveAll(o => !o.IsAlive);

            // Spread organisms
            SpreadOrganisms(deltaTime);

            // Reproduce organisms
            ReproduceOrganisms(deltaTime);

            // Move organisms slightly
            MoveOrganisms(deltaTime);
        }

        private void SpreadOrganisms(double deltaTime)
        {
            var organismsToAdd = new List<Organism>();

            foreach (var organism in _organisms.Where(o => o.IsAlive))
            {
                // Random chance to spread based on reproduction rate
                if (_random.NextDouble() < organism.ReproductionRate * deltaTime)
                {
                    var newOrganism = organism.Clone(mutate: true);

                    // Spread to nearby location
                    double angle = _random.NextDouble() * 2 * Math.PI;
                    double distance = 50 + _random.NextDouble() * 100;

                    newOrganism.Position = new Point(
                        Math.Max(0, Math.Min(_screenWidth, organism.Position.X + Math.Cos(angle) * distance)),
                        Math.Max(0, Math.Min(_screenHeight, organism.Position.Y + Math.Sin(angle) * distance))
                    );

                    organismsToAdd.Add(newOrganism);
                    TotalOrganismsCreated++;

                    if (newOrganism.Generation > GenerationsEvolved)
                    {
                        GenerationsEvolved = newOrganism.Generation;
                    }
                }
            }

            _organisms.AddRange(organismsToAdd);

            // Cap maximum organisms to prevent performance issues
            if (_organisms.Count > 500)
            {
                _organisms = _organisms.OrderByDescending(o => o.Generation)
                    .ThenByDescending(o => o.Health)
                    .Take(500)
                    .ToList();
            }
        }

        private void ReproduceOrganisms(double deltaTime)
        {
            // Organisms can reproduce if healthy enough
            var healthyOrganisms = _organisms.Where(o => o.IsAlive && o.Health > 70).ToList();

            if (healthyOrganisms.Count < 2) return;

            var organismsToAdd = new List<Organism>();

            for (int i = 0; i < Math.Min(5, healthyOrganisms.Count / 10); i++)
            {
                var parent = healthyOrganisms[_random.Next(healthyOrganisms.Count)];
                var offspring = parent.Clone(mutate: true);

                offspring.Position = new Point(
                    parent.Position.X + _random.Next(-30, 30),
                    parent.Position.Y + _random.Next(-30, 30)
                );

                organismsToAdd.Add(offspring);
            }

            _organisms.AddRange(organismsToAdd);
        }

        private void MoveOrganisms(double deltaTime)
        {
            foreach (var organism in _organisms.Where(o => o.IsAlive))
            {
                // Slight random movement
                double dx = (_random.NextDouble() - 0.5) * 20 * deltaTime;
                double dy = (_random.NextDouble() - 0.5) * 20 * deltaTime;

                organism.Position = new Point(
                    Math.Max(0, Math.Min(_screenWidth, organism.Position.X + dx)),
                    Math.Max(0, Math.Min(_screenHeight, organism.Position.Y + dy))
                );
            }
        }

        public List<Organism> ApplyChemical(Chemical chemical, Point location, double radius)
        {
            var affectedOrganisms = new List<Organism>();

            foreach (var organism in _organisms.Where(o => o.IsAlive))
            {
                double distance = Math.Sqrt(
                    Math.Pow(organism.Position.X - location.X, 2) +
                    Math.Pow(organism.Position.Y - location.Y, 2)
                );

                if (distance <= radius)
                {
                    double damage = _chemistryEngine.CalculateDamageToOrganism(chemical, organism);

                    // Damage decreases with distance
                    damage *= (1.0 - distance / radius);

                    organism.TakeDamage(damage, chemical.Name);
                    affectedOrganisms.Add(organism);

                    // Survivors develop resistance
                    if (organism.IsAlive && damage > 10)
                    {
                        organism.DevelopResistance(chemical.Name, 0.05);
                    }
                }
            }

            return affectedOrganisms;
        }

        public void SetScreenSize(double width, double height)
        {
            _screenWidth = width;
            _screenHeight = height;
        }

        public int GetAliveCount()
        {
            return _organisms.Count(o => o.IsAlive);
        }

        public double GetAverageResistance(string chemicalName)
        {
            var organismsWithResistance = _organisms
                .Where(o => o.IsAlive && o.Resistances.ContainsKey(chemicalName))
                .ToList();

            if (organismsWithResistance.Count == 0) return 0.0;

            return organismsWithResistance.Average(o => o.Resistances[chemicalName]);
        }

        public Dictionary<string, double> GetResistanceStats()
        {
            var stats = new Dictionary<string, double>();
            var aliveOrganisms = _organisms.Where(o => o.IsAlive).ToList();

            if (aliveOrganisms.Count == 0) return stats;

            var allResistanceKeys = aliveOrganisms
                .SelectMany(o => o.Resistances.Keys)
                .Distinct();

            foreach (var key in allResistanceKeys)
            {
                stats[key] = GetAverageResistance(key);
            }

            return stats;
        }
    }
}
