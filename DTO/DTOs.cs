using ProgrammingPractice_L19.Model;

namespace ProgrammingPractice_L19.DTO;

public class FishCatchResultDto
{
    public string FishName { get; set; } = string.Empty;
    public string Quality { get; set; } = string.Empty;
    public string BoatName { get; set; } = string.Empty;
    public double TotalCatch { get; set; }
}

public class JarsAverageCatchDto
{
    public string JarName { get; set; } = string.Empty;
    public double AverageCatch { get; set; }
}

public class BoatMoreThenAverageCatchDto
{
    public Boat Boat { get; set; } = null!;
    public double TotalCatch { get; set; }
}

public class QualityCatchByVoyagesDto
{
    public string Quality { get; set;  } = string.Empty;
    public string VoyageNumber { get; set; } = string.Empty;
    public double TotalCatch { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}

public class VoyageJarCatchReportDto
{
    public string VoyageNumber { get; set;  } = string.Empty;
    public string JarName { get; set;  } = string.Empty;
    public string FishName { get; set; } = string.Empty;
    public string? FishQuality { get; set; }
    public double? FishWeight { get; set; }
}

public class VoyageByJarAndQuality
{
    public string VoyageName { get; set; } = string.Empty;
    public string FishName { get; set; } = string.Empty;
    public double Weight { get; set; }
}
