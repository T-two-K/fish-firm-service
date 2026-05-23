using Microsoft.EntityFrameworkCore;
using ProgrammingPractice_L19.Database;
using ProgrammingPractice_L19.Model;
using ProgrammingPractice_L19.Repositories;
using static ProgrammingPractice_L19.Repositories.FishermanRepository;

namespace ProgrammingPractice_L19.Service
{
    public class FishFirmService
    {
        public IDbContextFactory<FishFirmDbContext> _factory;
        public BoatRepository BoatRep { get; set; }
        public FishGroupRepository FishGroupRep { get; set; }
        public VoyageRepository VoyageRep { get; set; }
        public VoyageJarRepository VoyageJarRep { get; set; }
        public JarRepository JarRep { get; set; }
        public FishermanRepository FishermanRep { get; set; }
        public VoyageFishermanRepository VoyageFishermanRep { get; set; }

        public FishFirmService(IDbContextFactory<FishFirmDbContext> factory)
        {
            _factory = factory;
            BoatRep = new BoatRepository(factory);
            FishGroupRep = new FishGroupRepository(factory);
            VoyageJarRep = new VoyageJarRepository(factory);
            VoyageRep = new VoyageRepository(factory);
            JarRep = new JarRepository(factory);
            FishermanRep = new FishermanRepository(factory);
            VoyageFishermanRep = new VoyageFishermanRepository(factory);
        }

        //2. предоставить возможность добавления выхода катера в море с указанием команды; 
        public async Task AddVoyage(Voyage voyage, int boatId, List<Fisherman> fishermen)
        {
            if (fishermen == null || fishermen.Count == 0)
                throw new InvalidOperationException("Вы не выбрали ни одного рыбака");

            await using var db = await _factory.CreateDbContextAsync();
            await using var transaction = await db.Database.BeginTransactionAsync();

            try
            {
                if (await db.Voyages.AnyAsync(v => v.VoyageNumber == voyage.VoyageNumber && v.Id != voyage.Id))
                    throw new InvalidOperationException("Рейс с таким номером уже существует!");

                Boat? boat = await db.Boats.FindAsync(boatId);

                if (boat == null) throw new InvalidOperationException("Такого катера нет.");
                if (boat.IsBusy) throw new InvalidOperationException("Катер уже находится в рейсе.");

                foreach (var fisherman in fishermen)
                {
                    bool isBusy = await db.Set<VoyageFisherman>()
                                        .AnyAsync(vf => vf.FishermanId == fisherman.Id
                                                 && vf.Voyage.EndDate == null);

                    if (isBusy)
                        throw new InvalidOperationException($"Рыбак {fisherman.FullName} уже находится в рейсе");
                }

                voyage.BoatId = boatId;
                await db.Voyages.AddAsync(voyage);
                boat.IsBusy = true;
                db.SaveChanges();

                foreach (var fisherman in fishermen)
                {
                    VoyageFisherman newRelation = new VoyageFisherman()
                    {
                        VoyageId = voyage.Id,
                        FishermanId = fisherman.Id,
                    };

                    await db.Set<VoyageFisherman>().AddAsync(newRelation);
                }
                await db.SaveChangesAsync();

                voyage.CurrentBoat = boat;
                voyage.Fishermen = fishermen.Select(f => new VoyageFisherman()
                {
                    VoyageId = voyage.Id,
                    FishermanId = f.Id,
                    Fisherman = f,
                    Voyage = voyage,
                }).ToList();

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task UpdateVoyageAsync(
            Voyage voyage,
            int boatId,
            List<Fisherman> fishermen,
            DateTime startDate,
            string voyageNumber)
        {
            await using var db = await _factory.CreateDbContextAsync();
            var transaction = await db.Database.BeginTransactionAsync();

            try
            {
                if (await db.Voyages.AnyAsync(v => v.VoyageNumber == voyageNumber && v.Id != voyage.Id))
                    throw new InvalidOperationException("Рейс с таким номером уже существует!");

                if (boatId != voyage.BoatId)
                {
                    Boat newBoat = await db.Boats.FindAsync(boatId)
                                    ?? throw new InvalidOperationException("Такого катера нет.");

                    Boat oldBoat = await db.Boats.FindAsync(voyage.BoatId)
                                     ?? throw new InvalidOperationException("Катера нет...");

                    newBoat.IsBusy = true;
                    oldBoat.IsBusy = false;
                    voyage.BoatId = newBoat.Id;
                    voyage.CurrentBoat = newBoat;
                    db.SaveChanges();
                }

                var newFishermanIds = fishermen.Select(f => f.Id).ToHashSet();
                var oldFishermanIds = voyage.Fishermen.Select(vf => vf.FishermanId).ToHashSet();

                var deletedFishermen = oldFishermanIds.Except(newFishermanIds).ToList();
                var addedFishermen = newFishermanIds.Except(oldFishermanIds).ToList();

                if (addedFishermen.Any())
                    foreach (var fisherman in addedFishermen)
                    {
                        await db.Set<VoyageFisherman>().AddAsync(
                            new VoyageFisherman()
                            {
                                FishermanId = fisherman,
                                VoyageId = voyage.Id,
                            });
                    }

                if (deletedFishermen.Any())
                    foreach (var fisherman in deletedFishermen)
                    {
                        var relation = await db.Set<VoyageFisherman>()
                        .FindAsync(fisherman, voyage.Id);

                        if (relation != null)
                            db.Set<VoyageFisherman>().Remove(relation);
                    }

                await db.SaveChangesAsync();

                await db.Voyages.Where(v => v.Id == voyage.Id)
                    .ExecuteUpdateAsync(v => v
                        .SetProperty(v => v.VoyageNumber, voyageNumber)
                        .SetProperty(v => v.StartDate, startDate)
                        .SetProperty(v => v.BoatId, voyage.BoatId));

                voyage.VoyageNumber = voyageNumber;
                voyage.StartDate = startDate;

                voyage.Fishermen = fishermen.Select(f => new VoyageFisherman()
                {
                    VoyageId = voyage.Id,
                    FishermanId = f.Id,
                    Fisherman = f,
                    Voyage = voyage,
                }).ToList();

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task FinishVoyageAsync(Voyage voyage)
        {
            await using var db = await _factory.CreateDbContextAsync();
            var transaction = await db.Database.BeginTransactionAsync();

            try
            {
                await db.Voyages.ExecuteUpdateAsync(v => v
                        .SetProperty(v => v.EndDate, DateTime.Now));

                await db.SaveChangesAsync();

                Boat? boat = await db.Boats.FindAsync(voyage.BoatId);

                if (boat != null)
                {
                    boat.IsBusy = false;
                    await db.SaveChangesAsync();

                    voyage.CurrentBoat.IsBusy = false;
                }

                voyage.EndDate = DateTime.Now;
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task RemoveVoyageAsync(Voyage voyage)
        {
            await using var db = await _factory.CreateDbContextAsync();

            var transaction = await db.Database.BeginTransactionAsync();
            try
            {
                if (voyage.Fishermen.Any())
                {
                    var voyageFishermen = voyage.Fishermen;

                    db.Set<VoyageFisherman>().RemoveRange(voyageFishermen);
                    await db.SaveChangesAsync();
                }

                db.Attach(voyage);

                db.Remove(voyage);
                await db.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task UpdateVoyageJarAsync(
            VoyageJar voyageJar,
            int newVoyageId,
            int newJarId,
            DateTime startDate,
            DateTime? endDate)
        {
            await using var db = await _factory.CreateDbContextAsync();
            var transaction = await db.Database.BeginTransactionAsync();

            try
            {
                db.Attach(voyageJar);

                bool primaryKeyChanged = voyageJar.JarId != newJarId
                    || voyageJar.VoyageId != newVoyageId;

                if (primaryKeyChanged)
                {
                    var newPeriodId = 1;

                    while (await db.Set<VoyageJar>().AnyAsync(
                        vj => vj.VoyageId == newVoyageId
                        && vj.JarId == newJarId
                        && vj.PeriodId == newPeriodId))
                        newPeriodId++;

                    VoyageJar old = await db.Set<VoyageJar>().FindAsync(voyageJar.PeriodId,
                                voyageJar.JarId, voyageJar.VoyageId)
                            ?? throw new InvalidOperationException("Такой связи нет");

                    db.Set<VoyageJar>().Remove(old);
                    await db.SaveChangesAsync();

                    voyageJar = new VoyageJar()
                    {
                        VoyageId = newVoyageId,
                        JarId = newJarId,
                        PeriodId = newPeriodId,
                        BoatArrive = startDate,
                        BoatSillAway = endDate
                    };

                    await db.Set<VoyageJar>().AddAsync(voyageJar);
                    await db.SaveChangesAsync();
                }

                voyageJar.BoatArrive = startDate;
                voyageJar.BoatSillAway = endDate;

                await db.SaveChangesAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task AddVoyageJarAsync(
            VoyageJar addedVoyage,
            int voyageId,
            int jarId,
            DateTime boatArrive,
            DateTime? boatSillAway)
        {
            await using var db = await _factory.CreateDbContextAsync();

            int periodId = 1;
            while (await db.Set<VoyageJar>().AnyAsync(
                vj => vj.JarId == jarId
                && vj.VoyageId == voyageId
                && vj.PeriodId == periodId))
                periodId++;

            addedVoyage.JarId = jarId;
            addedVoyage.VoyageId = voyageId;
            addedVoyage.PeriodId = periodId;
            addedVoyage.BoatArrive = boatArrive;
            addedVoyage.BoatSillAway = boatSillAway;

            db.Set<VoyageJar>().Add(addedVoyage);
            db.SaveChanges();
        }

        //8. для выбранного пользователем рейса и банки добавить данные о сорте и количестве пойманной рыбы; 
        public async Task AddFishInfoBy(
            Voyage voyage,
            Jar jar,
            FishGroup fishGroup,
            string fishQuality,
            double fishWeight)
        {
            if (voyage.EndDate == null)
                throw new InvalidOperationException("Рейс ещё не был закончен.");

            await using var db = await _factory.CreateDbContextAsync();

            bool voyageJarExists = await db.Set<VoyageJar>()
                .AnyAsync(vj => vj.JarId == jar.Id && vj.VoyageId == voyage.Id);

            if (!voyageJarExists)
                throw new InvalidOperationException("Во время рейса корабль не посещал эту банку.");

            bool fishExists = await db.FishGroups
                .AnyAsync(fg => fg.Id == fishGroup.Id
                             && fg.JarId == jar.Id
                             && fg.VoyageId == voyage.Id);

            if (!fishExists)
                throw new InvalidOperationException("Соответствующий улов не был найден.");

            await db.FishGroups
                .Where(fg => fg.Id == fishGroup.Id)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(f => f.Quality, fishQuality)
                    .SetProperty(f => f.Weight, fishWeight));

            fishGroup.Quality = fishQuality;
            fishGroup.Weight = fishWeight;
        }
    }
}