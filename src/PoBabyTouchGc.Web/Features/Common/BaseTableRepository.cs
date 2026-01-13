using Azure;
using Azure.Data.Tables;
using Microsoft.Extensions.Logging;

namespace PoBabyTouchGc.Web.Features.Common;

/// <summary>
/// Base repository for Azure Table Storage operations
/// Applying Template Method Pattern to eliminate duplicate initialization code
/// </summary>
public abstract class BaseTableRepository
{
    protected readonly TableClient TableClient;
    protected readonly ILogger Logger;

    protected BaseTableRepository(
        TableServiceClient tableServiceClient,
        string tableName,
        ILogger logger)
    {
        Logger = logger;
        TableClient = tableServiceClient.GetTableClient(tableName);
        
        try
        {
            TableClient.CreateIfNotExists();
            Logger.LogInformation("Table initialized: {TableName}", tableName);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to initialize table: {TableName}", tableName);
            throw;
        }
    }

    /// <summary>
    /// Get entity by partition and row key with proper error handling
    /// </summary>
    protected async Task<T?> GetEntityAsync<T>(string partitionKey, string rowKey)
        where T : class, ITableEntity
    {
        try
        {
            var response = await TableClient.GetEntityAsync<T>(partitionKey, rowKey);
            return response.Value;
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            Logger.LogDebug("Entity not found: {PartitionKey}/{RowKey}", partitionKey, rowKey);
            return null;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error retrieving entity: {PartitionKey}/{RowKey}", partitionKey, rowKey);
            throw;
        }
    }

    /// <summary>
    /// Upsert entity with proper error handling
    /// </summary>
    protected async Task<bool> UpsertEntityAsync<T>(T entity) where T : ITableEntity
    {
        try
        {
            await TableClient.UpsertEntityAsync(entity, TableUpdateMode.Replace);
            Logger.LogDebug("Entity upserted: {PartitionKey}/{RowKey}", 
                entity.PartitionKey, entity.RowKey);
            return true;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error upserting entity: {PartitionKey}/{RowKey}", 
                entity.PartitionKey, entity.RowKey);
            return false;
        }
    }

    /// <summary>
    /// Delete entity with proper error handling
    /// </summary>
    protected async Task<bool> DeleteEntityAsync(string partitionKey, string rowKey)
    {
        try
        {
            await TableClient.DeleteEntityAsync(partitionKey, rowKey);
            Logger.LogDebug("Entity deleted: {PartitionKey}/{RowKey}", partitionKey, rowKey);
            return true;
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            Logger.LogWarning("Entity not found for deletion: {PartitionKey}/{RowKey}", 
                partitionKey, rowKey);
            return false;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error deleting entity: {PartitionKey}/{RowKey}", 
                partitionKey, rowKey);
            return false;
        }
    }

    /// <summary>
    /// Query entities with proper error handling
    /// </summary>
    protected async Task<List<T>> QueryEntitiesAsync<T>(string filter) where T : class, ITableEntity, new()
    {
        var results = new List<T>();

        try
        {
            await foreach (var entity in TableClient.QueryAsync<T>(filter))
            {
                results.Add(entity);
            }

            Logger.LogDebug("Query returned {Count} entities", results.Count);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error querying entities with filter: {Filter}", filter);
            throw;
        }

        return results;
    }
}
