using System.Text.RegularExpressions;
using Clinix.Application.Interfaces;
using Clinix.Application.Utilities;
using Clinix.Domain.Entities;
using Clinix.Infrastructure.Data;
using Clinix.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Clinix.Application.Services;

/// <summary>
/// Suggest doctors based on symptom text. Features:
/// - tokenization + normalization
/// - scoring by keyword matches (configurable weights)
/// - DB fallback via LIKE
/// - fuzzy fallback using Levenshtein distance on keywords (small set)
/// - caching of recent suggestions
/// </summary>
public class DoctorMatchingService : IDoctorMatchingService
    {
    private readonly ClinixDbContext _db;
    private readonly ILogger<DoctorMatchingService> _log;
    private readonly IMemoryCache _cache;
    private readonly TimeSpan _cacheTtl = TimeSpan.FromMinutes(5);

    // tuning: how many top doctors to return
    private readonly int _maxSuggestions = 5;

    // weighting: exact token match gets more weight than contains/fuzzy
    private readonly double _exactTokenWeight = 2.0;
    private readonly double _containsWeight = 1.0;
    private readonly double _fuzzyWeight = 0.6;

    public DoctorMatchingService(ClinixDbContext db,
                                 ILogger<DoctorMatchingService> log,
                                 IMemoryCache cache)
        {
        _db = db;
        _log = log;
        _cache = cache;
        }

    public async Task<IEnumerable<Doctor>> SuggestDoctorsAsync(string reason, CancellationToken ct)
        {
        if (string.IsNullOrWhiteSpace(reason)) return Enumerable.Empty<Doctor>();

        var normalized = TextNormalizer.Normalize(reason);

        // cache key
        var key = $"suggest:{normalized}";
        if (_cache.TryGetValue(key, out List<Doctor>? cached)) return cached;

        // load symptom maps once (small table)
        var maps = await _db.SymptomSpecialtyMaps.AsNoTracking().ToListAsync(ct);
        if (!maps.Any())
            {
            _log.LogInformation("SymptomSpecialtyMap empty; skipping automated matching.");
            return Enumerable.Empty<Doctor>();
            }

        var tokens = Tokenize(normalized);

        // scoring: specialty => score
        var specialtyScores = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);

        // token-based scoring
        foreach (var map in maps)
            {
            var k = TextNormalizer.Normalize(map.Keyword);
            if (tokens.Contains(k))
                {
                specialtyScores.TryGetValue(map.Specialty, out var s);
                s += _exactTokenWeight;
                specialtyScores[map.Specialty] = s;
                }
            else if (normalized.Contains(k, StringComparison.OrdinalIgnoreCase))
                {
                specialtyScores.TryGetValue(map.Specialty, out var s);
                s += _containsWeight;
                specialtyScores[map.Specialty] = s;
                }
            else
                {
                // fuzzy: small Levenshtein check for short keywords (avoid heavy cost)
                if (k.Length <= 15)
                    {
                    var minDist = tokens.Select(t => LevenshteinDistance(t, k)).DefaultIfEmpty(int.MaxValue).Min();
                    if (minDist <= 2) // tolerant threshold; tune as needed
                        {
                        specialtyScores.TryGetValue(map.Specialty, out var s);
                        s += _fuzzyWeight * (3 - minDist); // closer distance greater weight
                        specialtyScores[map.Specialty] = s;
                        }
                    }
                }
            }

        // if no specialty scores yet, fallback to DB 'contains' search for keywords across map.Keyword
        if (!specialtyScores.Any())
            {
            _log.LogInformation("No specialty token scores; falling back to DB contains search.");
            var fallback = maps
                .Where(m => EF.Functions.Like(normalized, $"%{m.Keyword.ToLower()}%"))
                .GroupBy(m => m.Specialty)
                .Select(g => new { Specialty = g.Key, Score = g.Count() * 0.5 })
                .ToList();

            foreach (var f in fallback) specialtyScores[f.Specialty] = f.Score;
            }

        if (!specialtyScores.Any())
            return Enumerable.Empty<Doctor>();

        // pick top N specialties
        var topSpecialties = specialtyScores.OrderByDescending(kv => kv.Value)
                                            .Take(10) // take more specialties but we'll fetch doctors and score them
                                            .Select(kv => kv.Key)
                                            .ToList();

        // fetch doctors for these specialties (limit results)
        var doctors = await _db.Doctors.Where(d => topSpecialties.Contains(d.Specialty))
                                       .AsNoTracking()
                                       .ToListAsync(ct);

        // score doctors: base their specialty score, plus bonus if doctor's name contains tokens (prefer named specialists)
        var doctorScores = new Dictionary<int, double>();
        foreach (var doc in doctors)
            {
            var baseScore = specialtyScores.TryGetValue(doc.Specialty, out var s) ? s : 0.0;
            var nameNorm = TextNormalizer.Normalize(doc.Name);
            var nameTokens = Tokenize(nameNorm);
            var bonus = tokens.Intersect(nameTokens).Any() ? 0.3 : 0.0;
            doctorScores[doc.UserId] = baseScore + bonus;
            }

        // pick top doctors by score, stable order by specialty and name
        var topDoctorIds = doctorScores.OrderByDescending(kv => kv.Value)
                                       .Take(_maxSuggestions)
                                       .Select(kv => kv.Key)
                                       .ToHashSet();

        var result = doctors.Where(d => topDoctorIds.Contains(d.UserId))
                            .OrderByDescending(d => doctorScores[d.Id])
                            .ThenBy(d => d.Name)
                            .ToList();

        // cache
        _cache.Set(key, result, _cacheTtl);

        return result;
        }

    private static HashSet<string> Tokenize(string text)
        {
        return text.Split(new[] { ' ', ',', '.', ';', '/', '\\', '-', '_', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
                   .Select(t => TextNormalizer.Normalize(t))
                   .Where(s => s.Length > 1)
                   .ToHashSet();
        }

    // simple Levenshtein distance - small inputs only
    private static int LevenshteinDistance(string a, string b)
        {
        if (string.IsNullOrEmpty(a)) return b?.Length ?? 0;
        if (string.IsNullOrEmpty(b)) return a.Length;

        var n = a.Length;
        var m = b.Length;
        var d = new int[n + 1, m + 1];

        for (var i = 0; i <= n; i++) d[i, 0] = i;
        for (var j = 0; j <= m; j++) d[0, j] = j;

        for (var i = 1; i <= n; i++)
            {
            for (var j = 1; j <= m; j++)
                {
                var cost = a[i - 1] == b[j - 1] ? 0 : 1;
                d[i, j] = new[] {
                    d[i - 1, j] + 1,
                    d[i, j - 1] + 1,
                    d[i - 1, j - 1] + cost
                }.Min();
                }
            }
        return d[n, m];
        }
    }
