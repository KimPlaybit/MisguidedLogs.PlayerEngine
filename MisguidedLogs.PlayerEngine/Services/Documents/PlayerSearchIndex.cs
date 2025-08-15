using Lucene.Net.Documents;
using MisguidedLogs.PlayerEngine.Models;
using MisguidedLogs.PlayerEngine.Repositories;

namespace MisguidedLogs.PlayerEngine.Services.Documents;

public record PlayerSearchIndex(string Id, string Name, int Guid, Class Class, HashSet<PlayedCombinations> Combinations, HashSet<Achivement>? AchivedAchivements)
{
    public Document GetDocument(ProbabilityValues probability)
    {
        var doc = new Document
        {
            new TextField("playername", Name, Field.Store.YES),
            new TextField("id", Id, Field.Store.YES),
            new Int32Field("guid", Guid, Field.Store.YES),
            new Int32Field("class", (int)Class, Field.Store.YES),
            new TextField("playersearchinfo", Name.ToLowerInvariant().Replace("-",""), Field.Store.NO)
        };

        foreach (var combination in Combinations)
        {
            var probabilityOfCombination = probability.Bosses
                .FirstOrDefault(b => b.BossId == combination.BossId)?.GetProbabilityOfRole(combination.Role)
                .FirstOrDefault(x => x.Class == Class)?.Specs
                .FirstOrDefault(x => x.Spec == combination.Spec)?.TotalProbability ?? 0f;


            doc.Add(new StringField("playedCombination", $"{combination.BossId}:{combination.Role}:{combination.Spec}", Field.Store.YES));
            doc.Add(new Int32Field($"playedCombination:{combination.BossId}:{combination.Role}:{combination.Spec}", (int)(probabilityOfCombination * 1000), Field.Store.NO));
        }

        if (AchivedAchivements is not null)
        {
            foreach (var achivement in AchivedAchivements)
            {
                doc.Add(new StringField("achivement:", $"{achivement.Name}:{achivement.Boss}:{achivement.ReportCode}:{achivement.AchivedAt}", Field.Store.YES));
            }
        }

        return doc;
    }
}

public record PlayedCombinations(short BossId, Role Role, TalentSpec Spec);

public record Achivement(AchivementEnum Name, int Boss, string ReportCode, DateTime AchivedAt);

public enum AchivementEnum
{
    ForTheLight,
    BruteForce,
    TheBigHunt,
    StormEarthFire,
    EarthMotherIsWatching,
    Assassination,
    ArcanePower,
    LightWillGuideUs,
    EmbraceTheShadows,
    HeyNoTaunt,
    TheAntiMeta
}