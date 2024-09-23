namespace CarSharing.Abstractions
{
    internal abstract class DataGeneratorConfiguration 
    { 
        public IEnumerable<string>? Data { get; set; }

        public int GeneratorResolutionSeconds { get; set; }
    }

    internal class DriversDataGeneratorConfiguration : DataGeneratorConfiguration
    { }

    internal class CarsDataGeneratorConfiguration : DataGeneratorConfiguration
    { }
}