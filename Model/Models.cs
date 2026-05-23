namespace ProgrammingPractice_L19.Model
{
    public class User
    {
        public int Id { get; set; }
        public string Login { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string Role { get; set; } = null!;
    }

    public class Boat
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Type { get; set; } = null!;
        public bool IsBusy { get; set; } = false;
        public double Displacement { get; set; }
        public DateTime ConstructionDate { get; set; }

        public override bool Equals(object? obj) =>
            obj is Boat other && other.Id == Id;

        public override int GetHashCode() =>
            Id.GetHashCode();
    }

    public class Voyage
    {
        public int Id { get; set; }
        public int BoatId { get; set; }
        public string VoyageNumber { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public Boat CurrentBoat { get; set; } = null!;
        public ICollection<VoyageFisherman> Fishermen { get; set; } = new List<VoyageFisherman>();
        public ICollection<VoyageJar> Jars { get; set; } = new List<VoyageJar>();
    }

    public class Jar
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Coordinate { get; set; } = null!;

        public ICollection<VoyageJar> Voyages { get; set; } = new List<VoyageJar>();
    }

    public class VoyageJar
    {
        public int VoyageId { get; set; }
        public Voyage Voyage { get; set; } = null!;

        public int JarId { get; set; }
        public Jar Jar { get; set; } = null!;

        //Катер может посетить одну и ту же банку за один и тот же рейс несколько раз
        //(захотелось ему).
        public int PeriodId { get; set; }
        public DateTime BoatArrive { get; set; }
        public DateTime? BoatSillAway { get; set; }

        public ICollection<FishGroup> Fishes { get; set; } = new List<FishGroup>();
    }

    public class Fisherman
    {
        public int Id { get; set; }
        public string FullName { get; set; } = null!;
        public string Address { get; set; } = null!;
        public string JobTitle { get; set; } = null!;
        public ICollection<VoyageFisherman> Voyages { get; set; } = new List<VoyageFisherman>();
    }

    public class VoyageFisherman
    {
        public int VoyageId { get; set; }
        public int FishermanId { get; set; }
        public Voyage Voyage { get; set; } = null!;
        public Fisherman Fisherman { get; set; } = null!;

        public override string ToString()
        {
            return $"{Fisherman.FullName} ({Fisherman.JobTitle})";
        }
    }

    public class FishGroup
    {
        public int Id { get; set; }
        public int JarId { get; set; }
        public int VoyageId { get; set; }
        public int PeriodId { get; set; }
        public string Name { get; set; } = null!;
        public string? Quality { get; set; } 
        public double? Weight { get; set; }
        public VoyageJar VoyageJar { get; set; } = null!;
    }
}
