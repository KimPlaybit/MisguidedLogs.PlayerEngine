using Lucene.Net.Analysis.Standard;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Lucene.Net.Util;
using MisguidedLogs.PlayerEngine.Repositories;
using MisguidedLogs.PlayerEngine.Repositories.Bunnycdn;
using MisguidedLogs.PlayerEngine.Services.Documents;
using LuceneDirectory = Lucene.Net.Store.Directory;

namespace MisguidedLogs.PlayerEngine.Services;

public class PlayerEngine
{
    private const LuceneVersion luceneVersion = LuceneVersion.LUCENE_48;
    private readonly BunnyCdnStorageLoader loader;
    private readonly ProbabilityService probabilityService;
    private readonly IndexWriter indexWriter;
    private readonly LuceneDirectory indexDir;
    private static bool IsUpdating;

    public PlayerEngine(BunnyCdnStorageLoader loader, ProbabilityService probabilityService)
    {
        string indexPath = Path.Combine("/app/data", "playerinfo");
        indexDir = FSDirectory.Open(indexPath);
        try
        {
            foreach (var item in indexDir.ListAll())
            {
                indexDir.DeleteFile(item);
            }
        }
        catch 
        {

        }



        // Create an analyzer to process the text 
        var standardAnalyzer = new StandardAnalyzer(luceneVersion);

        //Create an index writer
        var indexConfig = new IndexWriterConfig(luceneVersion, standardAnalyzer)
        {
            OpenMode = OpenMode.CREATE_OR_APPEND
        };

        indexWriter = new IndexWriter(indexDir, indexConfig);
        this.loader = loader;
        this.probabilityService = probabilityService;
        reader = indexWriter.GetReader(applyAllDeletes: true);
    }

    public async Task UpdateAllDocuments()
    {
        if (IsUpdating)
        {
            return;
        }

        IsUpdating = true;
        var naxx = await probabilityService.GetProbabilityValuesAsync(1036);
        var aq = await probabilityService.GetProbabilityValuesAsync(1035);
        var aqr = await probabilityService.GetProbabilityValuesAsync(1031); 
        var bwl = await probabilityService.GetProbabilityValuesAsync(1034); 
        var zg = await probabilityService.GetProbabilityValuesAsync(1030); 
        var mc = await probabilityService.GetProbabilityValuesAsync(1029); 
        var ony = await probabilityService.GetProbabilityValuesAsync(1028); 
        var bosses = naxx.Bosses.Union(aq.Bosses).Union(aqr.Bosses).Union(bwl.Bosses).Union(zg.Bosses).Union(mc.Bosses).Union(ony.Bosses).ToList();

        var playersInfo = await loader.TryGetStorageObject<HashSet<PlayerSearchIndex>>("misguided-logs-warcraftlogs/gold/players.json.gz");
        if (playersInfo is null)
        {
            return;
        }
        foreach (var playerInfo in playersInfo)
        {
            var term = new Term("id", playerInfo.Id);
            indexWriter.UpdateDocument(term, playerInfo.GetDocument(bosses));
        }
        indexWriter.Commit();

        IsUpdating = false;
    }

    private DirectoryReader reader;
    public IndexSearcher GetSearcher()
    {
        var newReader = DirectoryReader.OpenIfChanged(reader);

        if (newReader != null)
        {
            reader = newReader;
        }

        return new IndexSearcher(reader);
    }
}