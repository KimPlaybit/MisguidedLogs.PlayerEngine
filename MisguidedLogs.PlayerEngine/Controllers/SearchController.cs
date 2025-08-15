using Microsoft.AspNetCore.Mvc;
using MisguidedLogs.PlayerEngine.Models;
using MisguidedLogs.PlayerEngine.Services;

namespace MisguidedLogs.PlayerEngine.Controllers;

[ApiController]
[Route("[controller]")]
public class SearchController(Searcher searcher, ILogger<SearchController> logger) : ControllerBase
{
    [HttpGet("id/{query}")]
    public Player GetById(string query)
    {
        return searcher.GetPlayerById(query);
    }

}
