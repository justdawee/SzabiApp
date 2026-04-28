using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using SzabiApp.Backend.Common;
using SzabiApp.Backend.Data.Context;
using SzabiApp.Backend.Models.Entities;
using SzabiApp.Backend.Models.Enums;

namespace SzabiApp.Backend.Data.Seeding;

/// <summary>
/// Populates the database with realistic Hungarian demo data for presentations.
/// Idempotent: only runs when no users exist yet, so it's safe to keep enabled
/// across restarts (it just becomes a no-op once data is in place).
/// </summary>
public sealed class DemoDataSeeder(
    AppDbContext context,
    IPasswordHasher<User> passwordHasher,
    IClock clock,
    DemoSeederOptions options,
    ILogger<DemoDataSeeder> logger)
{
    // Fixed seed → reproducible demo data across restarts.
    private const int RandomSeed = 20260428;

    private static readonly string[] FirstNames =
    [
        "Anna", "Bálint", "Csilla", "Dávid", "Eszter", "Ferenc", "Gabriella", "Hunor",
        "Ildikó", "József", "Katalin", "László", "Mónika", "Norbert", "Orsolya", "Péter",
        "Réka", "Szabolcs", "Tamás", "Ursula", "Viktor", "Zsófia", "Ádám", "Bence",
        "Cintia", "Dorina", "Endre", "Fanni", "Gergő", "Henrietta", "Imre", "Júlia",
        "Kinga", "Levente", "Margit", "Nándor", "Olivér", "Petra", "Roland", "Sára"
    ];

    private static readonly string[] LastNames =
    [
        "Nagy", "Kovács", "Tóth", "Szabó", "Horváth", "Varga", "Kiss", "Molnár",
        "Németh", "Farkas", "Balogh", "Papp", "Takács", "Juhász", "Lakatos", "Mészáros",
        "Oláh", "Simon", "Rácz", "Fekete", "Szilágyi", "Török", "Fehér", "Gál",
        "Balla", "Sándor", "Pintér", "Fodor", "Dudás", "Boros"
    ];

    private static readonly (LeaveCategory Category, string Note)[] LeaveTemplates =
    [
        (LeaveCategory.Annual, "Családi nyaralás a Balatonon."),
        (LeaveCategory.Annual, "Hosszú hétvége Bécsben."),
        (LeaveCategory.Annual, "Pihenés, feltöltődés."),
        (LeaveCategory.Annual, "Esküvőre megyek vidékre."),
        (LeaveCategory.Annual, "Síelés Ausztriában."),
        (LeaveCategory.Annual, "Költözés miatt szabadnap."),
        (LeaveCategory.Sick, "Megfáztam, orvoshoz kell mennem."),
        (LeaveCategory.Sick, "Influenzás vagyok."),
        (LeaveCategory.Sick, "Gyermek beteg, otthon kell maradnom."),
        (LeaveCategory.Sick, "Fogorvosi beavatkozás miatt."),
        (LeaveCategory.Unpaid, "Magánügy intézése."),
        (LeaveCategory.Unpaid, "Hosszabb külföldi út."),
        (LeaveCategory.Paternity, "Apasági szabadság a kisbaba érkezése miatt."),
        (LeaveCategory.Maternity, "Szülési szabadság."),
        (LeaveCategory.Other, "Hatósági ügyintézés.")
    ];

    private static readonly string[] ApprovalNotes =
    [
        "Jóváhagyva, jó pihenést!",
        "Rendben, mindenki tudja a helyettesítést.",
        "Elfogadva, kérlek a projekt átadást zárd le indulás előtt.",
        "OK, kellemes kikapcsolódást!"
    ];

    private static readonly string[] DenialNotes =
    [
        "Sajnos ebben az időszakban kritikus határidőnk van.",
        "Túl sokan lennének egyszerre szabadságon, kérlek tedd át.",
        "Kérlek, egyeztessük személyesen egy másik időpontot.",
        "Most nem fér bele, jövő hónapban újra kérheted."
    ];

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        if (!options.Enabled)
        {
            logger.LogDebug("Demo seeder disabled; skipping.");
            return;
        }

        if (options.ApplyMigrations)
        {
            logger.LogInformation("Demo seeder: applying pending migrations...");
            await context.Database.MigrateAsync(cancellationToken);
        }

        if (await context.Users.AnyAsync(cancellationToken))
        {
            logger.LogInformation("Demo seeder: users already present, skipping seeding.");
            return;
        }

        logger.LogInformation(
            "Demo seeder: populating database (managers={ManagerCount}, employees={EmployeeCount})...",
            options.ManagerCount, options.EmployeeCount);

        var random = new Random(RandomSeed);
        var nowLocal = clock.GetCurrentInstant().InUtc().LocalDateTime;
        var today = nowLocal.Date;

        var schedules = SeedWorkSchedules();
        var holidays = SeedHolidays();

        await context.WorkSchedules.AddRangeAsync(schedules, cancellationToken);
        await context.Holidays.AddRangeAsync(holidays, cancellationToken);

        var (admin, managers, employees) = SeedUsers(random, schedules, nowLocal);
        await context.Users.AddRangeAsync(new[] { admin }.Concat(managers).Concat(employees), cancellationToken);

        var allUsers = new List<User> { admin };
        allUsers.AddRange(managers);
        allUsers.AddRange(employees);

        var allowances = SeedAllowances(allUsers, today.Year);
        await context.LeaveAllowances.AddRangeAsync(allowances, cancellationToken);

        var requests = SeedLeaveRequests(random, employees, managers, allowances, holidays, today, nowLocal);
        await context.LeaveRequests.AddRangeAsync(requests, cancellationToken);

        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Demo seeder: done. Users={UserCount}, Allowances={AllowanceCount}, Requests={RequestCount}.",
            allUsers.Count, allowances.Count, requests.Count);
    }

    private static List<WorkSchedule> SeedWorkSchedules() =>
    [
        new() { Id = Guid.NewGuid(), Name = "Teljes munkaidő (40 óra)", WorkDaysPerWeek = 5, DailyWorkHours = 8 },
        new() { Id = Guid.NewGuid(), Name = "Részmunkaidő (20 óra)",   WorkDaysPerWeek = 5, DailyWorkHours = 4 },
        new() { Id = Guid.NewGuid(), Name = "Rugalmas (30 óra)",        WorkDaysPerWeek = 5, DailyWorkHours = 6 }
    ];

    private static List<Holiday> SeedHolidays() =>
    [
        new() { Id = Guid.NewGuid(), Name = "Újév",                              Date = new LocalDate(2026, 1, 1),   IsRecurringYearly = true },
        new() { Id = Guid.NewGuid(), Name = "Az 1848-as forradalom ünnepe",      Date = new LocalDate(2026, 3, 15),  IsRecurringYearly = true },
        new() { Id = Guid.NewGuid(), Name = "A munka ünnepe",                    Date = new LocalDate(2026, 5, 1),   IsRecurringYearly = true },
        new() { Id = Guid.NewGuid(), Name = "Államalapítás ünnepe",              Date = new LocalDate(2026, 8, 20),  IsRecurringYearly = true },
        new() { Id = Guid.NewGuid(), Name = "Az 1956-os forradalom ünnepe",      Date = new LocalDate(2026, 10, 23), IsRecurringYearly = true },
        new() { Id = Guid.NewGuid(), Name = "Mindenszentek",                     Date = new LocalDate(2026, 11, 1),  IsRecurringYearly = true },
        new() { Id = Guid.NewGuid(), Name = "Karácsony",                         Date = new LocalDate(2026, 12, 25), IsRecurringYearly = true },
        new() { Id = Guid.NewGuid(), Name = "Karácsony másnapja",                Date = new LocalDate(2026, 12, 26), IsRecurringYearly = true }
    ];

    private (User Admin, List<User> Managers, List<User> Employees) SeedUsers(
        Random random, List<WorkSchedule> schedules, LocalDateTime nowLocal)
    {
        var fullSchedule = schedules[0];
        var partSchedule = schedules[1];
        var flexSchedule = schedules[2];

        var admin = new User
        {
            Id = Guid.NewGuid(),
            FirstName = "Adminisztrátor",
            LastName = "SzabiApp",
            Email = "admin@szabiapp.hu",
            Role = UserRole.Admin,
            IsActive = true,
            BirthDate = new LocalDate(1985, 6, 12),
            CreatedAt = nowLocal,
            WorkScheduleId = fullSchedule.Id
        };
        admin.PasswordHash = passwordHasher.HashPassword(admin, options.DefaultPassword);

        var usedEmails = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { admin.Email };

        var managers = new List<User>();
        for (var i = 0; i < options.ManagerCount; i++)
        {
            var (first, last) = PickName(random);
            var email = UniqueEmail(first, last, "manager", usedEmails);

            var manager = new User
            {
                Id = Guid.NewGuid(),
                FirstName = first,
                LastName = last,
                Email = email,
                Role = UserRole.Manager,
                IsActive = true,
                BirthDate = RandomBirthDate(random, minAge: 32, maxAge: 55),
                CreatedAt = nowLocal,
                WorkScheduleId = fullSchedule.Id
            };
            manager.PasswordHash = passwordHasher.HashPassword(manager, options.DefaultPassword);
            managers.Add(manager);
        }

        var employees = new List<User>();
        for (var i = 0; i < options.EmployeeCount; i++)
        {
            var (first, last) = PickName(random);
            var email = UniqueEmail(first, last, "munkatars", usedEmails);

            var schedulePick = random.Next(10) switch
            {
                < 7 => fullSchedule,   // 70% full-time
                < 9 => flexSchedule,   // 20% flex
                _   => partSchedule    // 10% part-time
            };

            var employee = new User
            {
                Id = Guid.NewGuid(),
                FirstName = first,
                LastName = last,
                Email = email,
                Role = UserRole.Employee,
                IsActive = i % 18 != 17,  // ~5% inactive for realism
                BirthDate = RandomBirthDate(random, minAge: 22, maxAge: 60),
                CreatedAt = nowLocal,
                WorkScheduleId = schedulePick.Id,
                ManagerId = managers[i % managers.Count].Id
            };
            employee.PasswordHash = passwordHasher.HashPassword(employee, options.DefaultPassword);
            employees.Add(employee);
        }

        return (admin, managers, employees);
    }

    private static (string First, string Last) PickName(Random random) =>
        (FirstNames[random.Next(FirstNames.Length)], LastNames[random.Next(LastNames.Length)]);

    private static LocalDate RandomBirthDate(Random random, int minAge, int maxAge)
    {
        var year = 2026 - random.Next(minAge, maxAge + 1);
        var month = random.Next(1, 13);
        var day = random.Next(1, 28);
        return new LocalDate(year, month, day);
    }

    private static string UniqueEmail(string first, string last, string suffix, HashSet<string> used)
    {
        var baseLocal = $"{Normalize(first)}.{Normalize(last)}";
        var candidate = $"{baseLocal}@szabiapp.hu";
        var counter = 1;
        while (!used.Add(candidate))
        {
            candidate = $"{baseLocal}{counter}@szabiapp.hu";
            counter++;
        }

        // suffix unused on purpose — kept as future-proof differentiator hook
        _ = suffix;
        return candidate;
    }

    private static string Normalize(string value)
    {
        var lowered = value.ToLowerInvariant();
        var map = new Dictionary<char, char>
        {
            ['á'] = 'a', ['é'] = 'e', ['í'] = 'i', ['ó'] = 'o', ['ö'] = 'o',
            ['ő'] = 'o', ['ú'] = 'u', ['ü'] = 'u', ['ű'] = 'u'
        };
        var chars = lowered.Select(c => map.TryGetValue(c, out var replacement) ? replacement : c);
        return new string(chars.ToArray());
    }

    private static List<LeaveAllowance> SeedAllowances(List<User> users, int year)
    {
        var allowances = new List<LeaveAllowance>(users.Count * 2);

        foreach (var user in users)
        {
            allowances.Add(new LeaveAllowance
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Year = year,
                Category = LeaveCategory.Annual,
                TotalDays = user.Role == UserRole.Employee ? 25 : 30,
                UsedDays = 0
            });

            allowances.Add(new LeaveAllowance
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Year = year,
                Category = LeaveCategory.Sick,
                TotalDays = 15,
                UsedDays = 0
            });
        }

        return allowances;
    }

    private List<LeaveRequest> SeedLeaveRequests(
        Random random,
        List<User> employees,
        List<User> managers,
        List<LeaveAllowance> allowances,
        List<Holiday> holidays,
        LocalDate today,
        LocalDateTime nowLocal)
    {
        var requests = new List<LeaveRequest>();
        var allowanceLookup = allowances
            .GroupBy(a => a.UserId)
            .ToDictionary(g => g.Key, g => g.ToList());

        foreach (var employee in employees.Where(e => e.IsActive))
        {
            var requestCount = random.Next(2, 6); // 2–5 requests per employee
            var employeeManager = managers.FirstOrDefault(m => m.Id == employee.ManagerId);

            for (var i = 0; i < requestCount; i++)
            {
                var template = LeaveTemplates[random.Next(LeaveTemplates.Length)];
                var (start, end) = RandomRange(random, today, template.Category);
                var status = PickStatus(random, start, today);

                var workingDays = CountWorkingDays(start, end, holidays);
                if (workingDays == 0)
                    continue;

                var request = new LeaveRequest
                {
                    Id = Guid.NewGuid(),
                    UserId = employee.Id,
                    Category = template.Category,
                    StartDate = start,
                    EndDate = end,
                    Status = status,
                    RequestNote = template.Note,
                    CreatedAt = nowLocal.PlusDays(-random.Next(1, 60))
                };

                if (status is LeaveStatus.Approved or LeaveStatus.Denied)
                {
                    request.ReviewedById = employeeManager?.Id;
                    request.ReviewedAt = request.CreatedAt.PlusDays(random.Next(1, 4));
                    request.ReviewNote = status == LeaveStatus.Approved
                        ? ApprovalNotes[random.Next(ApprovalNotes.Length)]
                        : DenialNotes[random.Next(DenialNotes.Length)];
                }

                if (status == LeaveStatus.Approved
                    && allowanceLookup.TryGetValue(employee.Id, out var userAllowances))
                {
                    var allowance = userAllowances.FirstOrDefault(a =>
                        a.Year == start.Year && a.Category == template.Category);

                    if (allowance is not null && allowance.RemainingDays >= workingDays)
                    {
                        allowance.UsedDays += workingDays;
                    }
                    else
                    {
                        // Not enough balance left — flip to Pending so we don't exceed it.
                        request.Status = LeaveStatus.Pending;
                        request.ReviewedById = null;
                        request.ReviewedAt = null;
                        request.ReviewNote = null;
                    }
                }

                requests.Add(request);
            }
        }

        return requests;
    }

    private static (LocalDate Start, LocalDate End) RandomRange(Random random, LocalDate today, LeaveCategory category)
    {
        // Spread roughly half in the past (already handled), half in the future (pending/approved).
        var offset = random.Next(-90, 120);
        var start = today.PlusDays(offset);

        var length = category switch
        {
            LeaveCategory.Maternity => random.Next(60, 180),
            LeaveCategory.Paternity => random.Next(3, 8),
            LeaveCategory.Sick      => random.Next(1, 5),
            LeaveCategory.Annual    => random.Next(1, 10),
            LeaveCategory.Unpaid    => random.Next(1, 5),
            _                       => random.Next(1, 3)
        };

        return (start, start.PlusDays(length - 1));
    }

    private static LeaveStatus PickStatus(Random random, LocalDate start, LocalDate today)
    {
        // Past requests are mostly approved/denied; future requests are mostly pending/approved.
        if (start < today)
        {
            return random.Next(10) switch
            {
                < 6 => LeaveStatus.Approved,
                < 8 => LeaveStatus.Denied,
                < 9 => LeaveStatus.Cancelled,
                _   => LeaveStatus.Pending
            };
        }

        return random.Next(10) switch
        {
            < 5 => LeaveStatus.Pending,
            < 8 => LeaveStatus.Approved,
            < 9 => LeaveStatus.Denied,
            _   => LeaveStatus.Cancelled
        };
    }

    private static int CountWorkingDays(LocalDate start, LocalDate end, List<Holiday> holidays)
    {
        if (end < start) return 0;

        var workingDays = 0;
        var current = start;

        while (current <= end)
        {
            var isWeekend = current.DayOfWeek is IsoDayOfWeek.Saturday or IsoDayOfWeek.Sunday;
            var isHoliday = holidays.Any(h =>
                h.IsRecurringYearly
                    ? h.Date.Month == current.Month && h.Date.Day == current.Day
                    : h.Date == current);

            if (!isWeekend && !isHoliday)
                workingDays++;

            current = current.PlusDays(1);
        }

        return workingDays;
    }
}
