using Lucene.Net.Index;
using Lucene.Net.Search;
using MisguidedLogs.PlayerEngine.Models;

namespace MisguidedLogs.PlayerEngine.Services;

public class Searcher(PlayerEngine PlayerEngine)
{
    public Player GetPlayerById(string id)
    {
        var searcher = PlayerEngine.GetSearcher();
        var query = new TermQuery(new Term("id", id));

        var topDocs = searcher.Search(query, 2).ScoreDocs.First();
        var docHit = searcher.Doc(topDocs.Doc);

        var combinations = docHit.GetFields("playedCombination").Select(combination => combination.GetStringValue());
        var achivements = docHit.GetFields("achivements").Select(achive => achive.GetStringValue());
     
        return new Player(
            docHit.Get("id"),
            docHit.Get("playername"),
            int.Parse(docHit.Get("guid")),
            (Class)Enum.Parse(typeof(Class), docHit.Get("class")),
            [ ..combinations],
            [ ..achivements]
        );
    }
}
