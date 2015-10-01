// -------------------------------------
//  Domain		: IBT / Realtime.co
//  Author		: Nicholas Ventimiglia
//  Product		: Messaging and Storage
//  Published	: 2014
//  -------------------------------------

using Foundation.Tasks;
using Realtime.Storage.Models;

namespace Realtime.Storage.DataAccess
{
    /// <summary>
    /// Data Access For Cloud Storage
    /// </summary>
    public interface IStorageRepository
    {
        #region Settings and Initialization
        
        /// <summary> 
        /// For registering item types. Alternative to using the  StorageKeyAttribute
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tableName">The table name</param>
        /// <param name="primaryKey">primary property name</param>
        /// <param name="secondaryKey">secondary property name</param>
        void RegisterType<T>(string tableName, string primaryKey, string secondaryKey);
        
        #endregion

        #region Security

        /// <summary>
        /// Authenticates the user for the storage access role(s)
        /// </summary>
        /// <returns></returns>
        UnityTask<StorageResponse<bool>> Authenticate(string authToken, string[] roles,int timeout = 1800 );

        /// <summary>
        /// Is the user authenticated ?
        /// </summary>
        /// <returns></returns>
        UnityTask<StorageResponse<bool>> IsAuthenticated();

        /// <summary>
        /// Retrieves a paginated list of the names of all the roles created by the user’s application.
        /// </summary>
        /// <returns></returns>
        UnityTask<StorageResponse<string[]>> ListRoles();

        /// <summary>
        /// Retrieves the all the roles associated with the subscription.
        /// </summary>
        /// <returns></returns>
        UnityTask<StorageResponse<Role[]>> GetRoles(string[] roles);

        /// <summary>
        /// Retrieves the policies that compose the role.
        /// </summary>
        /// <returns></returns>
        UnityTask<StorageResponse<Role>> GetRole(string role);

        #endregion

        #region Tables

        /// <summary>
        /// Gets a table's metadata
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        UnityTask<StorageResponse<TableMetadata>> GetTable(string table);
        
        /// <summary>
        /// Post a table's metadata
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        UnityTask<StorageResponse<TableMetadata>> CreateTable(TableMetadata table);

        /// <summary>
        /// Puts a table's metadata
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        UnityTask<StorageResponse<TableMetadata>> UpdateTable(TableMetadata table);

        /// <summary>
        /// Deletes a table
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        UnityTask<StorageResponse<TableMetadata>> DeleteTable(string table);

        /// <summary>
        /// Returns a listing of tables
        /// </summary>
        /// <param name="limit"></param>
        /// <param name="startTable"></param>
        /// <returns></returns>
        UnityTask<StorageResponse<TableList>> ListTables(int limit = 0, string startTable = null);

        #endregion

        #region Items

        /// <summary>
        /// Returns metadata about the Type
        /// </summary>
        /// <typeparam name="T">The data type</typeparam>
        /// <returns></returns>
        ItemMetadata GetMetadata<T>();

        /// <summary>
        /// Returns the item by Id
        /// </summary>
        /// <typeparam name="T">The type to fetch</typeparam>
        /// <param name="id">the primary id</param>
        /// <returns></returns>
        UnityTask<StorageResponse<T>> Get<T>(object id) where T : class;

        /// <summary>
        /// Returns the item by Id
        /// </summary>
        /// <typeparam name="T">The type to fetch</typeparam>
        /// <param name="id">the primary id</param>
        /// <param name="secondary">the secondary id</param>
        /// <returns></returns>
        UnityTask<StorageResponse<T>> Get<T>(object id, object secondary) where T : class;

        /// <summary>
        /// Returns the item by Id
        /// </summary>
        /// <typeparam name="T">The type to fetch</typeparam>
        /// <param name="key">the identities</param>
        /// <returns></returns>
        UnityTask<StorageResponse<T>> Get<T>(DataKey key) where T : class;

        /// <summary>
        /// Creates/saves a new item
        /// </summary>
        /// <typeparam name="T">The item type</typeparam>
        /// <param name="item">the item to save</param>
        /// <returns></returns>
        UnityTask<StorageResponse<T>> Create<T>(T item) where T : class;

        /// <summary>
        /// Update the item
        /// </summary>
        /// <typeparam name="T">The type to fetch</typeparam>
        /// <param name="item">the item to save</param>
        /// <returns></returns>
        UnityTask<StorageResponse<T>> Update<T>(T item) where T : class;

        /// <summary>
        /// Removes the item
        /// </summary>
        /// <typeparam name="T">The type to fetch</typeparam>
        /// <param name="item">the item to delete</param>
        /// <returns></returns>
        UnityTask<StorageResponse<T>> Delete<T>(T item) where T : class;

        /// <summary>
        /// Increments a numeric property
        /// </summary>
        /// <typeparam name="T">the item type</typeparam>
        /// <param name="item">the item</param>
        /// <param name="propertyName">the property</param>
        /// <param name="change">the incremental change</param>
        /// <returns></returns>
        UnityTask<StorageResponse<T>> Incr<T>(T item, string propertyName, int change = 1) where T : class;

        /// <summary>
        /// Decrements a numeric property
        /// </summary>
        /// <typeparam name="T">the item type</typeparam>
        /// <param name="item">the item</param>
        /// <param name="propertyName">the property</param>
        /// <param name="change">the incremental change</param>
        /// <returns></returns>
        UnityTask<StorageResponse<T>> Decr<T>(T item, string propertyName, int change = 1) where T : class;
        
        /// <summary>
        /// Returns an query ordered by the secondary key 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="request"></param>
        /// <returns></returns>
        UnityTask<StorageResponse<ItemList<T>>> Query<T>(ItemQueryRequest<T> request) where T : class;

        /// <summary>
        /// Returns a unordered item listing
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="request"></param>
        /// <returns></returns>
        UnityTask<StorageResponse<ItemList<T>>> List<T>(ItemListRequest<T> request) where T : class;

        #endregion
    }
}
