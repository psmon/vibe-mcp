using Raven.Client.Documents;
using Raven.Client.Documents.Indexes;
using WebnoriMemory.Models;

namespace WebnoriMemory.Services;

public class RavenDbService
{
    private readonly IDocumentStore _documentStore;

    public RavenDbService()
    {
        _documentStore = new DocumentStore
        {
            Urls = new[] { "http://localhost:9000" },
            Database = "WebnoriMemory"
        };

        _documentStore.Initialize();
        
        // Ensure database exists
        try
        {
            _documentStore.Maintenance.Server.Send(new Raven.Client.ServerWide.Operations.CreateDatabaseOperation(new Raven.Client.ServerWide.DatabaseRecord
            {
                DatabaseName = "WebnoriMemory"
            }));
        }
        catch (Raven.Client.Exceptions.ConcurrencyException)
        {
            // Database already exists
        }

        // Deploy indexes
        IndexCreation.CreateIndexes(typeof(WebnoriNoteIndex).Assembly, _documentStore);
    }

    public IDocumentStore DocumentStore => _documentStore;
}