using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace BiochemSimulator.Models
{
    public class Organism
    {
        public Guid Id { get; set; }
        public Point Position { get; set; }
        public double Size { get; set; }
        public Color Color { get; set; }
        public int Generation { get; set; }
        public Dictionary<string, double> Resistances { get; set; }
        public double Health { get; set; }
        public double ReproductionRate { get; set; }
        public double MutationRate { get; set; }
        public bool IsAlive { get; set; }
        public DateTime CreatedAt { get; set; }
        public OrganismType Type { get; set; }

        public Organism()
        {
            Id = Guid.NewGuid();
            Resistances = new Dictionary<string, double>();
            Health = 100.0;
            ReproductionRate = 1.0;
            MutationRate = 0.1;
            IsAlive = true;
            CreatedAt = DateTime.Now;
            Size = 10.0;
            Color = Colors.Green;
            Type = OrganismType.SingleCell;
        }

        public Organism Clone(bool mutate = false)
        {
            var clone = new Organism
            {
                Position = Position,
                Size = Size,
                Color = Color,
                Generation = Generation + 1,
                Health = Health,
                ReproductionRate = ReproductionRate,
                MutationRate = MutationRate,
                Type = Type
            };

            foreach (var resistance in Resistances)
            {
                clone.Resistances[resistance.Key] = resistance.Value;
            }

            if (mutate)
            {
                ApplyMutation(clone);
            }

            return clone;
        }

        private void ApplyMutation(Organism organism)
        {
            Random rand = new Random();

            // Randomly increase resistance to a chemical
            if (organism.Resistances.Any() && rand.NextDouble() < organism.MutationRate)
            {
                var resistanceKey = organism.Resistances.Keys.ElementAt(rand.Next(organism.Resistances.Count));
                organism.Resistances[resistanceKey] = Math.Min(1.0, organism.Resistances[resistanceKey] + 0.1);
            }

            // Color mutation
            if (rand.NextDouble() < organism.MutationRate * 0.5)
            {
                byte r = (byte)Math.Min(255, organism.Color.R + rand.Next(-20, 20));
                byte g = (byte)Math.Min(255, organism.Color.G + rand.Next(-20, 20));
                byte b = (byte)Math.Min(255, organism.Color.B + rand.Next(-20, 20));
                organism.Color = Color.FromRgb(r, g, b);
            }

            // Size mutation
            if (rand.NextDouble() < organism.MutationRate * 0.3)
            {
                organism.Size *= 1 + (rand.NextDouble() - 0.5) * 0.2;
            }
        }

        public void TakeDamage(double damage, string chemicalName)
        {
            double resistance = Resistances.ContainsKey(chemicalName) ? Resistances[chemicalName] : 0.0;
            double actualDamage = damage * (1.0 - resistance);
            Health -= actualDamage;

            if (Health <= 0)
            {
                IsAlive = false;
            }
        }

        public void DevelopResistance(string chemicalName, double amount)
        {
            if (!Resistances.ContainsKey(chemicalName))
            {
                Resistances[chemicalName] = 0.0;
            }

            Resistances[chemicalName] = Math.Min(0.95, Resistances[chemicalName] + amount);
        }
    }

    public enum OrganismType
    {
        SingleCell,
        Bacteria,
        Virus,
        MutatedVirus,
        SuperVirus
    }
}
