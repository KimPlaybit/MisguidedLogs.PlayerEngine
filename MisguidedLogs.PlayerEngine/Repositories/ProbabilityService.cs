using MisguidedLogs.PlayerEngine.Models;
using MisguidedLogs.PlayerEngine.Repositories.Bunnycdn;

namespace MisguidedLogs.PlayerEngine.Repositories;

public class ProbabilityService(BunnyCdnStorageLoader loader)
{
    public async Task<ProbabilityValues> GetProbabilityValuesAsync(int zone)
    {
        var path = $"misguided-logs-warcraftlogs/zones/{zone}_stripped.json.gz";
        return await loader.GetStorageObject<ProbabilityValues>(path) ?? throw new Exception();
    }
}

public record ProbabilityValues(int Zone, List<BossProbability> Bosses);

public record BossProbability(int BossId, List<ClassProbability> Tanks, List<ClassProbability> Dps, List<ClassProbability> Hps, List<ClassProbability> Hybrids)
{
    internal List<ClassProbability> GetProbabilityOfRole(Role role)
    {
        return role switch
        {
            Role.Tank => Tanks,
            Role.Dps => Dps,
            Role.Healer => Hps,
            Role.Hybrid => Hybrids,
            _ => throw new ArgumentOutOfRangeException(nameof(role), role, null)
        };
    }
}
public record ClassProbability(Class Class, List<SpecProbability> Specs, float Probability, float TotalProbability);


public record SpecProbability(TalentSpec Spec, float Probability, float TotalProbability);