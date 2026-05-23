using Microsoft.EntityFrameworkCore;
using ProgrammingPractice_L19.Database;
using ProgrammingPractice_L19.DTO;
using ProgrammingPractice_L19.Model;
using System.Data;
using System.Linq.Expressions;
using System.Windows.Documents;

namespace ProgrammingPractice_L19.Repositories;

public abstract class BaseRepository<TElement> where TElement : class
{
    protected IDbContextFactory<FishFirmDbContext> _factory;

    public BaseRepository(IDbContextFactory<FishFirmDbContext> factory) =>
        _factory = factory;

    public async Task<List<TElement>> GetAllAsync(params Expression<Func<TElement, object>>[]? includes)
    {
        await using var db = await _factory.CreateDbContextAsync();

        IQueryable<TElement> query = db.Set<TElement>();

        if (includes != null)
            foreach (var include in includes)
                query = query.Include(include);

        return await query.ToListAsync();
    }

    public async Task<bool> CheckExist(Expression<Func<TElement, bool>> operation)
    {
        await using var db = await _factory.CreateDbContextAsync();

        return db.Set<TElement>().Any(operation);
    }

    public async Task<TElement?> GetByIdAsync(int id)
    {
        await using var db = await _factory.CreateDbContextAsync();

        return await db.Set<TElement>().FindAsync(id);
    }

    public async Task AddAsync(TElement element)
    {
        await using var db = await _factory.CreateDbContextAsync();

        db.Attach(element);

        db.Entry(element).State = EntityState.Added;
        await db.SaveChangesAsync();
    }

    public async Task UpdateAsync(TElement element)
    {
        await using var db = await _factory.CreateDbContextAsync();

        db.Attach(element);

        db.Entry(element).State = EntityState.Modified;
        await db.SaveChangesAsync();
    }

    public async Task DeleteAsync(TElement element)
    {
        await using var db = await _factory.CreateDbContextAsync();

        db.Attach(element);

        db.Entry(element).State = EntityState.Deleted;
        await db.SaveChangesAsync();
    }
}

public class BoatRepository : BaseRepository<Boat>
{
    public BoatRepository(IDbContextFactory<FishFirmDbContext> factory) : base(factory) { }

    public async Task<List<Boat>> GetFreeAsync()
    {
        await using var db = await _factory.CreateDbContextAsync();

        return await db.Boats.Where(b => b.IsBusy == false).ToListAsync();
    }
}

public class VoyageRepository : BaseRepository<Voyage>
{
    public VoyageRepository(IDbContextFactory<FishFirmDbContext> factory) : base(factory) { }

    //1. для каждого катера вывести даты выхода в море с указанием улова; 
    public async Task<List<Voyage>> GetAllAsync()
    {
        await using var db = await _factory.CreateDbContextAsync();

        return await db.Voyages.Include(v => v.CurrentBoat)
                               .Include(v => v.Jars).ThenInclude(j => j.Fishes)
                               .Include(v => v.Fishermen).ThenInclude(f => f.Fisherman)
                               .OrderBy(v => v.CurrentBoat.Name).ThenBy(v => v.StartDate)
                               .ToListAsync();
    }

    public async Task<List<Voyage>> GetVoyageWithJarsAsync()
    {
        await using var db = await _factory.CreateDbContextAsync();

        return await db.Voyages.Include(v => v.Jars).ThenInclude(j => j.Jar)
                               .Include(v => v.Jars).ThenInclude(vj => vj.Fishes)
                               .ToListAsync();
    }

    /// <summary>
    /// Получаем все законченые рейсы
    /// </summary>
    public async Task<List<Voyage>> GetFinishedAsync()
    {
        await using var db = await _factory.CreateDbContextAsync();

        return await db.Voyages.Include(v => v.Jars).ThenInclude(j => j.Jar)
            .Include(v => v.Jars).ThenInclude(j => j.Fishes)
            .Where(v => v.EndDate != null).ToListAsync();
    }
}

public class FishGroupRepository : BaseRepository<FishGroup>
{
    public FishGroupRepository(IDbContextFactory<FishFirmDbContext> factory) : base(factory) { }

    //3. для указанного интервала дат вывести для каждого сорта рыбы список катеров с наибольшим уловом;
    public async Task<List<FishCatchResultDto>> GetMaxFishByBoat(DateTime? dateFrom, DateTime? dateTo)
    {
        await using var db = await _factory.CreateDbContextAsync();

        List<FishGroup> fishGroups = await db.FishGroups
            .Include(fg => fg.VoyageJar)
                .ThenInclude(j => j.Voyage)
                    .ThenInclude(v => v.CurrentBoat)
                        .Where(fg => fg.VoyageJar.Voyage.StartDate >= dateFrom
                                 && fg.VoyageJar.Voyage.EndDate <= dateTo
                                 && fg.Quality != null)
            .ToListAsync();

        return fishGroups.GroupBy(fg => new { fg.Name, fg.Quality })
                                .Select(group => group
                                    .GroupBy(fg => fg.VoyageJar.Voyage.CurrentBoat.Name)
                                    .Select(boatGroup => new FishCatchResultDto
                                    {
                                        FishName = group.Key.Name,
                                        Quality = group.Key.Quality!,
                                        BoatName = boatGroup.Key,
                                        TotalCatch = boatGroup.Sum(fg => fg.Weight ?? 0)
                                    })
                                    .OrderBy(fg => fg.TotalCatch)
                                    .First())
                                .OrderBy(f => f.FishName).ThenBy(f => f.Quality)
                                .ToList();
    }

    //11. для указанного сорта рыбы и банки вывести список рейсов с указанием количества пойманной рыбы.
    public async Task<List<VoyageByJarAndQuality>> GetVoyageBy(string jarName, string qualityName)
    {
        await using var db = await _factory.CreateDbContextAsync();

        // Фильтруем через FishGroups напрямую — надёжнее чем Include с Where
        List<VoyageByJarAndQuality> result = await db.FishGroups
            .Where(f => f.Quality == qualityName
                     && f.VoyageJar.Jar.Name == jarName)
            .GroupBy(f => new
            {
                f.VoyageJar.Voyage.VoyageNumber,
                f.Name
            })
            .Select(group => new VoyageByJarAndQuality
            {
                VoyageName = group.Key.VoyageNumber,
                FishName = group.Key.Name,
                Weight = group.Sum(f => f.Weight ?? 0)
            })
            .OrderBy(r => r.VoyageName)
            .ToListAsync();

        return result;
    }

    public async Task<List<FishGroup>> GetAllFishAsync()
    {
        await using var db = await _factory.CreateDbContextAsync();

        return await db.FishGroups.Include(fg => fg.VoyageJar).ThenInclude(fg => fg.Jar)
                                  .Include(fg => fg.VoyageJar).ThenInclude(fg => fg.Voyage)
                                  .ToListAsync();
    }       

    public async Task<List<string?>> GetAllQualitiesAsync()
    {
        await using var db = await _factory.CreateDbContextAsync();

        return db.FishGroups.Select(f => f.Quality).Distinct().ToList();
    }

    public async Task<List<FishGroup>> GetByKeys(int jarId, int voyageId)
    {
        await using var db = await _factory.CreateDbContextAsync();

        return await db.FishGroups
            .Where(fg => fg.JarId == jarId && fg.VoyageId == voyageId)
            .OrderBy(fg => fg.Name)
            .ToListAsync();
    }
}

public class VoyageJarRepository : BaseRepository<VoyageJar>
{
    public VoyageJarRepository(IDbContextFactory<FishFirmDbContext> factory) : base(factory)
    {
    }

    //4. для указанного интервала дат вывести список банок, с указанием среднего улова за этот период; 
    public async Task<List<JarsAverageCatchDto>> GetAverageCatch(DateTime? dateFrom, DateTime? dateTo)
    {
        await using var db = await _factory.CreateDbContextAsync();

        List<VoyageJar> data = await db.Set<VoyageJar>()
                                       .Include(vj => vj.Jar)
                                       .Include(vj => vj.Fishes)
                                       .Where(vj => vj.BoatArrive >= dateFrom
                                                 && vj.BoatSillAway <= dateTo)
                                       .ToListAsync();

        return data.GroupBy(j => j.Jar.Name)
                    .Select(group => new JarsAverageCatchDto()
                    {
                        JarName = group.Key,
                        AverageCatch = group
                        .Average(vj => vj.Fishes.Sum(fg => fg.Weight ?? 0))
                    })
                    .OrderBy(j => j.AverageCatch)
                    .ToList();
    }

    //6. для заданной банки вывести список катеров, которые получили улов выше среднего;
    public async Task<List<BoatMoreThenAverageCatchDto>> GetAverageCatchBy(string jarName)
    {
        await using var db = await _factory.CreateDbContextAsync();

        List<VoyageJar> data = await db.Set<VoyageJar>()
                           .Include(vj => vj.Jar)
                           .Include(vj => vj.Voyage).ThenInclude(v => v.CurrentBoat)
                           .Include(vj => vj.Fishes)
                           .Where(vj => vj.Jar.Name == jarName)
                           .ToListAsync();

        double averageJarCatch = data.Average(j =>
                                    (j.Fishes).Sum(f => f.Weight)) ?? 0;

        return data.GroupBy(vj => vj.Voyage.CurrentBoat)
                   .Where(group => group.Sum(j => (j.Fishes)
                                .Sum(f => f.Weight)) > averageJarCatch)
                   .Select(group => new BoatMoreThenAverageCatchDto
                   {
                       Boat = group.Key,
                       TotalCatch = group.Sum(j => (j.Fishes)
                                            .Sum(f => f.Weight)) ?? 0
                   })
                   .OrderBy(b => b.TotalCatch)
                   .ToList();
    }

    //7. вывести список сортов рыбы и для каждого сорта список рейсов с указанием даты выхода и возвращения, количества улова;
    public async Task<List<QualityCatchByVoyagesDto>> GetQualityCatch(string qualityName)
    {
        await using var db = await _factory.CreateDbContextAsync();

        return await db.FishGroups
            .Where(fg => fg.Quality == qualityName)
            .GroupBy(fg => new
            {
                fg.VoyageJar.Voyage.VoyageNumber,
                fg.VoyageJar.Voyage.StartDate,
                fg.VoyageJar.Voyage.EndDate
            })
            .Select(group => new QualityCatchByVoyagesDto
            {
                Quality = qualityName,
                VoyageNumber = group.Key.VoyageNumber,
                StartDate = group.Key.StartDate,
                EndDate = group.Key.EndDate,
                TotalCatch = group.Sum(fg => fg.Weight ?? 0)
            })
            .OrderBy(vj => vj.Quality)
            .ThenBy(vj => vj.TotalCatch)
            .ToListAsync();
    }

    public async Task<List<VoyageJarCatchReportDto>> GetDataDtoAsync()
    {
        await using var db = await _factory.CreateDbContextAsync();

        var data = await db.Set<VoyageJar>()
                        .Include(vj => vj.Jar)
                        .Include(vj => vj.Voyage)
                        .Include(vj => vj.Fishes)
                        .ToListAsync();

        List<VoyageJarCatchReportDto> result = new();

        foreach (var dat in data)
            if (dat.Fishes != null)
            {
                result.AddRange(dat.Fishes.Select(f => new VoyageJarCatchReportDto()
                {
                    FishName = f.Name,
                    FishQuality = f.Quality,
                    FishWeight = f.Weight,
                    JarName = dat.Jar.Name,
                    VoyageNumber = dat.Voyage.VoyageNumber
                }).ToList());
            }

        return result;
    }
}

public class JarRepository : BaseRepository<Jar>
{
    public JarRepository(IDbContextFactory<FishFirmDbContext> factory) : base(factory) { }

    // Среднее количество улова по всей банке
    public async Task<double> GetAverageCatch(Jar jar)
    {
        await using FishFirmDbContext db = await _factory.CreateDbContextAsync();

        Jar? data = await db.Jars.Include(j => j.Voyages).ThenInclude(v => v.Fishes)
                                      .FirstOrDefaultAsync(j => j.Id == jar.Id);

        if (data == null)
            throw new NullReferenceException("Такой банки не существует!");

        return data.Voyages
                   .Average(v => (v.Fishes)
                           .Sum(f => f.Weight ?? 0));
    }
}

public class FishermanRepository : BaseRepository<Fisherman>
{
    public FishermanRepository(IDbContextFactory<FishFirmDbContext> factory) : base(factory) { }

    public async Task<List<Fisherman>> GetFreeAsync()
    {
        await using var db = await _factory.CreateDbContextAsync();

        return await db.Fishermen.Include(f => f.Voyages)
                                 .Where(f => f.Voyages.All(v => v.Voyage.EndDate != null))
                                 .OrderBy(f => f.JobTitle).ThenBy(f => f.FullName)
                                 .ToListAsync();
    }

    public void Update(Fisherman fisherman)
    {
        using var db = _factory.CreateDbContext();

        db.Attach(fisherman);

        db.Update(fisherman);
        db.SaveChanges();
    }

    public void Remove(Fisherman fisherman)
    {
        using var db = _factory.CreateDbContext();

        db.Attach(fisherman);

        db.Remove(fisherman);
        db.SaveChanges();
    }

    public bool CheckBy(Expression<Func<Fisherman, bool>> condition)
    {
        using var db = _factory.CreateDbContext();

        return db.Fishermen.Any(condition);
    }

    public void Add(Fisherman fisherman)
    {
        using var db = _factory.CreateDbContext();

        db.Add(fisherman);
        db.SaveChanges();
    }

    public List<Fisherman> GetAll()
    {
        using var db = _factory.CreateDbContext();

        return db.Fishermen.Include(f => f.Voyages).ThenInclude(f => f.Voyage)
                           .OrderBy(f => f.JobTitle)
                           .ThenBy(f => f.FullName)
                           .ToList();
    }

    public class VoyageFishermanRepository : BaseRepository<VoyageFisherman>
    {
        public VoyageFishermanRepository(
            IDbContextFactory<FishFirmDbContext> factory) : base(factory){}
    }
}