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
        string indexPath = Path.Combine(Environment.CurrentDirectory, "playerinfo");
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
        var probability = await probabilityService.GetProbabilityValuesAsync(2018); // set in config to begin with, can't support all zones yet
        var playersInfo = await loader.GetStorageObject<HashSet<PlayerSearchIndex>>("misguided-logs-warcraftlogs/gold/players.json.gz");
        if (playersInfo is null)
        {
            return;
        }
        foreach (var playerInfo in playersInfo)
        {
            var term = new Term("id", playerInfo.Id);
            indexWriter.UpdateDocument(term, playerInfo.GetDocument(probability));
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