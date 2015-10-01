// -------------------------------------
//  Domain		: IBT / Realtime.co
//  Author		: Nicholas Ventimiglia
//  Product		: Messaging and Storage
//  Published	: 2014
//  -------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using Foundation.Tasks;
using Realtime.LITJson;
using Realtime.Messaging.Internal;
using Realtime.Storage.Models;

namespace Realtime.Storage.DataAccess
{
    /// <summary>
    /// Data Access For Cloud Storage
    /// </summary>
    public class StorageRepository : IStorageRepository
    {
        #region props and fields
        /// <summary>
        /// Option for logging
        /// </summary>
        public bool VerbosLogging;

        protected UriPrototype PrototypeUri;
        protected Uri EnpointUri;
        protected Dictionary<Type, ItemMetadata> ItemMetadata;
        protected HttpTaskService Client;
        protected string ApplicationKey;
        protected string PrivateKey;
        protected string AuthenticationToken;
        #endregion

        #region Ctor / init

        /// <summary>
        /// new storage repository with custom settings
        /// </summary>
        /// <param name="appKey"></param>
        /// <param name="privateKey"></param>
        /// <param name="uri"></param>
        public StorageRepository(string appKey, string privateKey, UriPrototype uri)
        {
            Init(appKey, privateKey, uri);
        }

        /// <summary>
        /// new storage repository with default settings
        /// </summary>
        public StorageRepository()
        {
            Init(StorageSettings.Instance.ApplicationKey, StorageSettings.Instance.PrivateKey, new UriPrototype
            {
                Url = StorageSettings.Instance.StorageUrl,
                IsSecure = StorageSettings.Instance.StorageSSL,
                IsCluster = StorageSettings.Instance.StorageIsCluster,
            });
        }

        void Init(string appKey, string privateKey, UriPrototype uri)
        {
            ApplicationKey = appKey;
            PrivateKey = privateKey;
            PrototypeUri = uri;

            ItemMetadata = new Dictionary<Type, ItemMetadata>();

            Client = new HttpTaskService
            {
                Accept = "application/json",
                ContentType = "application/json",

            };

            StorageConverters.Initialize();
        }

        #endregion

        #region private methods methods

        /// <summary>
        /// Log
        /// </summary>
        /// <param name="s"></param>
        void Log(string s)
        {
            if (VerbosLogging)
                UnityEngine.Debug.Log(s);
        }

        /// <summary>
        /// Gets the Metadta for the type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public ItemMetadata GetMetadata<T>()
        {
            // todo make this thread safe
            var type = typeof(T);

            if (ItemMetadata.ContainsKey(type))
            {
                return ItemMetadata[type];
            }

            if (!Attribute.IsDefined(type, typeof(StorageKeyAttribute)))
            {
                throw new Exception("StorageItem Metadata is not defined. Either use the StorageItem attribute or configure the RIStorageRepository with your items metadata");
            }

            var attribute = (StorageKeyAttribute)Attribute.GetCustomAttribute(type, typeof(StorageKeyAttribute));
            var meta = new ItemMetadata(type, attribute);
            ItemMetadata.Add(type, meta);
            return meta;
        }

        /// <summary>
        /// Http Post
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="operation"></param>
        /// <param name="body"></param>
        /// <returns></returns>
        UnityTask<StorageResponse<T>> Post<T>(string operation, string body)
        {
            var task = new UnityTask<StorageResponse<T>>(TaskStrategy.Custom);

            Log(operation + " " + body);

            TaskManager.StartRoutine(PostAsync(task, operation, body));

            return task;
        }

        IEnumerator GetUrlFromCluster(UnityTask task)
        {
            if (PrototypeUri.IsCluster)
            {
                var request = new Uri(PrototypeUri.IsSecure ? "https://" + PrototypeUri.Url + "/server/ssl/1.0" : "http://" + PrototypeUri.Url + "/server/1.0");

                var www = Client.GetAsync(request.ToString());

                yield return TaskManager.StartRoutine(www.WaitRoutine());

                if (www.IsFaulted)
                {
                    task.Status = TaskStatus.Faulted;
                    task.Exception = www.Exception;
                    yield break;

                }

                var response = JsonMapper.ToObject<ClusterResponse>(www.Content).url;

                EnpointUri = new Uri(response);

                //var request = new Uri(PrototypeUri.IsSecure ? "https://" + PrototypeUri.Url + "/server/ssl/1.0" : "http://" + PrototypeUri.Url + "/server/1.0");
                //var response = ClusterClient.GetClusterServer(request.ToString(), ApplicationKey);
                //EnpointUri = new Uri(response);

            }
            else
            {
                EnpointUri = new Uri(PrototypeUri.IsSecure ? "https://" + PrototypeUri.Url : "http://" + PrototypeUri.Url);
                //EnpointUri = new Uri(PrototypeUri.IsSecure ? "https://" + PrototypeUri.Url + "/server/ssl/1.0" : "http://" + PrototypeUri.Url + "/server/1.0");
            }
        }

        IEnumerator PostAsync<T>(UnityTask<StorageResponse<T>> task, string operation, string body)
        {
            // Confirm Endpoint URI
            if (EnpointUri == null)
            {
                var urlTask = new UnityTask(TaskStrategy.Custom);


                yield return TaskManager.StartRoutine(GetUrlFromCluster(urlTask));

                if (EnpointUri == null)
                {
                    task.Exception = new Exception("Failed to get cluster url");
                    task.Status = TaskStatus.Faulted;
                    yield break;
                }
            }
            
            var url = EnpointUri.ToString().EndsWith("/") ? EnpointUri + operation : EnpointUri + "/" + operation;
            var www = Client.PostAsync(url, body);

            yield return TaskManager.StartRoutine(www.WaitRoutine());

            if (www.IsFaulted)
            {
                task.Exception = www.Exception;
                task.Status = TaskStatus.Faulted;
                yield break;
            }

            Log(www.Content);

            task.Result = JsonMapper.ToObject<StorageResponse<T>>(www.Content);
            task.Status = TaskStatus.Success;
        }


        /// <summary>
        /// Removes JSON fields from JToken.
        /// </summary>
        /// <param name="token"></param>
        /// <param name="fields"></param>
        void RemoveFields(JsonData token, string[] fields)
        {
            foreach (var field in fields)
            {
                token.Remove(field);
            }
        }



        #endregion

        #region Security

        //public StorageResponse<bool> Authenticate(string applicationKey, string privateKey, string authenticationToken, int timeout, IEnumerable<string> roles = null, IPolicy policies = null)
        //{
        //    dynamic body = new ExpandoObject();
        //    body.applicationKey = applicationKey;
        //    body.privateKey = privateKey;
        //    body.authenticationToken = authenticationToken;
        //    body.timeout = timeout;
        //    body.roles = roles;
        //    body.policies = policies;

        //    UnityTask<StorageResponse<bool>> result = request<bool>("authenticate", body);
        //    return result.Result;
        //}

        //public StorageResponse<bool> IsAuthenticated(string applicationKey, string authenticationToken)
        //{
        //    dynamic body = new ExpandoObject();
        //    body.applicationKey = applicationKey;
        //    body.authenticationToken = authenticationToken;

        //    UnityTask<StorageResponse<bool>> result = request<bool>("isAuthenticated", body);
        //    return result.Result;
        //}

        //public StorageResponse<IEnumerable<string>> ListRoles(string applicationKey, string privateKey)
        //{
        //    dynamic body = new ExpandoObject();
        //    body.applicationKey = applicationKey;
        //    body.privateKey = privateKey;

        //    UnityTask<StorageResponse<IEnumerable<string>>> result = request<IEnumerable<string>>("listRoles", body);
        //    return result.Result;
        //}

        //public StorageResponse<IEnumerable<Role>> GetRoles(string applicationKey, string privateKey, IEnumerable<string> roles)
        //{
        //    dynamic body = new ExpandoObject();
        //    body.applicationKey = applicationKey;
        //    body.privateKey = privateKey;
        //    body.roles = roles;

        //    UnityTask<StorageResponse<IEnumerable<Role>>> result = request<IEnumerable<Role>>("authenticate", body);
        //    return result.Result;
        //}


        #endregion

        #region Items

        /// <summary> 
        /// For registering item types. Alternative to using the  StorageKeyAttribute
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tableName">The table name</param>
        /// <param name="primary">primary property name</param>
        /// <param name="secondary">secondary property name</param>
        public void RegisterType<T>(string tableName, string primary, string secondary)
        {
            if (ItemMetadata.ContainsKey(typeof(T)))
                throw new Exception("Type already registered");

            var metadata = new ItemMetadata(typeof(T), tableName, primary, secondary);

            ItemMetadata.Add(typeof(T), metadata);
        }

        /// <summary>
        /// Returns the item by Id
        /// </summary>
        /// <code>
        /// 
        ///  //get task
        ///  var result1 = Repository.Create(score);
        ///  // wait for it
        ///  yield return StartCoroutine(result1.WaitRoutine());
        ///  // client error
        ///  result1.ThrowIfFaulted();
        ///  // server error
        ///  if (result1.Result.hasError)
        ///      throw new Exception(result1.Result.error.message);
        /// 
        /// </code>
        /// <typeparam name="T">The type to fetch</typeparam>
        /// <param name="id">the primary id</param>
        /// <returns></returns>
        public UnityTask<StorageResponse<T>> Get<T>(object id) where T : class
        {
            return Get<T>(new DataKey(id));
        }

        /// <summary>
        /// Returns the item by Id
        /// </summary>
        /// <typeparam name="T">The type to fetch</typeparam>
        /// <param name="id">the primary id</param>
        /// <param name="secondary">the secondary id</param>
        /// <returns></returns>
        public UnityTask<StorageResponse<T>> Get<T>(object id, object secondary) where T : class
        {
            return Get<T>(new DataKey(id, secondary));
        }

        /// <summary>
        /// Returns the item by Id
        /// </summary>
        /// <typeparam name="T">The type to fetch</typeparam>
        /// <param name="id">the identities</param>
        /// <returns></returns>
        public UnityTask<StorageResponse<T>> Get<T>(DataKey id) where T : class
        {
            var metadata = GetMetadata<T>();

            var request = new StorageRequest
            {
                applicationKey = ApplicationKey,
                privateKey = PrivateKey,
                authenticationToken = AuthenticationToken,
                table = metadata.Table,
                key = id,
            };

            var dto = JsonMapper.ToJson(request);

            return Post<T>("getItem", dto);
        }

        /// <summary>
        /// Creates/saves a new item
        /// </summary>
        /// <typeparam name="T">The item type</typeparam>
        /// <param name="item">the item to save</param>
        /// <returns></returns>
        public UnityTask<StorageResponse<T>> Create<T>(T item) where T : class
        {
            var metadata = GetMetadata<T>();

            var request = new StorageRequest
            {
                applicationKey = ApplicationKey,
                privateKey = PrivateKey,
                authenticationToken = AuthenticationToken,
                table = metadata.Table,
                //key = metadata.GetKey(item),
                item = item,
            };

            var dto = JsonMapper.ToJson(request);

            return Post<T>("putItem", dto);
        }

        /// <summary>
        /// Update the item
        /// </summary>
        /// <typeparam name="T">The type to fetch</typeparam>
        /// <param name="item">the item to save</param>
        /// <returns></returns>
        public UnityTask<StorageResponse<T>> Update<T>(T item) where T : class
        {
            var metadata = GetMetadata<T>();

            var itemDto = JsonMapper.ToObject(JsonMapper.ToJson(item));
            RemoveFields(itemDto, metadata.HasSecondary ? new[] { metadata.Primary, metadata.Secondary } : new[] { metadata.Primary });

            var request = new StorageRequest
            {
                applicationKey = ApplicationKey,
                privateKey = PrivateKey,
                authenticationToken = AuthenticationToken,
                table = metadata.Table,
                key = metadata.GetKey(item),
                item = itemDto.ToJson()
            };

            var dto = JsonMapper.ToJson(request);

            return Post<T>("updateItem", dto);
        }

        /// <summary>
        /// Removes the item
        /// </summary>
        /// <typeparam name="T">The type to fetch</typeparam>
        /// <param name="item">the item to delete</param>
        /// <returns></returns>
        public UnityTask<StorageResponse<T>> Delete<T>(T item) where T : class
        {
            var metadata = GetMetadata<T>();

            var request = new StorageRequest
            {
                applicationKey = ApplicationKey,
                privateKey = PrivateKey,
                authenticationToken = AuthenticationToken,
                table = metadata.Table,
                key = metadata.GetKey(item),
                item = item
            };

            var dto = JsonMapper.ToJson(request);

            return Post<T>("deleteItem", dto);
        }

        /// <summary>
        /// Increments a numeric property
        /// </summary>
        /// <typeparam name="T">the item type</typeparam>
        /// <param name="item">the item</param>
        /// <param name="propertyName">the property</param>
        /// <param name="change">the incremental change</param>
        /// <returns></returns>
        public UnityTask<StorageResponse<T>> Incr<T>(T item, string propertyName, int change = 1) where T : class
        {
            var metadata = GetMetadata<T>();

            var request = new StorageRequest
            {
                applicationKey = ApplicationKey,
                privateKey = PrivateKey,
                authenticationToken = AuthenticationToken,
                table = metadata.Table,
                key = metadata.GetKey(item),
                item = item,
                property = propertyName,
                value = change
            };

            var dto = JsonMapper.ToJson(request);

            return Post<T>("incr", dto);
        }

        /// <summary>
        /// Decrements a numeric property
        /// </summary>
        /// <typeparam name="T">the item type</typeparam>
        /// <param name="item">the item</param>
        /// <param name="propertyName">the property</param>
        /// <param name="change">the incremental change</param>
        /// <returns></returns>
        public UnityTask<StorageResponse<T>> Decr<T>(T item, string propertyName, int change = 1) where T : class
        {
            var metadata = GetMetadata<T>();

            var request = new StorageRequest
            {
                applicationKey = ApplicationKey,
                privateKey = PrivateKey,
                authenticationToken = AuthenticationToken,
                table = metadata.Table,
                key = metadata.GetKey(item),
                item = item,
                property = propertyName,
                value = change
            };

            var dto = JsonMapper.ToJson(request);

            return Post<T>("decr", dto);
        }

        /// <summary>
        /// Returns an query ordered by the secondary key 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <returns></returns>
        public UnityTask<StorageResponse<ItemList<T>>> Query<T>(ItemQueryRequest<T> query) where T : class
        {
            var metadata = GetMetadata<T>();

            var request = new StorageRequest
            {
                applicationKey = ApplicationKey,
                privateKey = PrivateKey,
                authenticationToken = AuthenticationToken,
                table = metadata.Table,
                key = query._datakey,
                properties = query._properties,
                startKey = query._startKey,
                limit = query._limit,
                filter = query._filter
            };


            var dto = JsonMapper.ToJson(request);

           return Post<ItemList<T>>("queryItems", dto);
        }

        /// <summary>
        /// Returns a unordered item listing
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <returns></returns>
        public UnityTask<StorageResponse<ItemList<T>>> List<T>(ItemListRequest<T> query) where T : class
        {
            var metadata = GetMetadata<T>();

            var request = new StorageRequest
            {
                applicationKey = ApplicationKey,
                privateKey = PrivateKey,
                authenticationToken = AuthenticationToken,
                table = metadata.Table,
                key = query._datakey,
                properties = query._properties,
                startKey = query._startKey,
                limit = query._limit,
                filter = query._filters
            };

            var dto = JsonMapper.ToJson(request);

            return Post<ItemList<T>>("listItems", dto);
        }

        #endregion

        #region tables

        /// <summary>
        /// Gets a table's metadata
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        public UnityTask<StorageResponse<TableMetadata>> GetTable(string table)
        {
            var request = new StorageRequest
            {
                applicationKey = ApplicationKey,
                privateKey = PrivateKey,
                authenticationToken = AuthenticationToken,
                table = table,
            };

            var dto = JsonMapper.ToJson(request);

            return Post<TableMetadata>("describeTable", dto);
        }

        /// <summary>
        /// Post a table's metadata
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        public UnityTask<StorageResponse<TableMetadata>> CreateTable(TableMetadata table)
        {
            var request = new StorageRequest
            {
                applicationKey = ApplicationKey,
                privateKey = PrivateKey,
                authenticationToken = AuthenticationToken,
                table = table.name,
                key = table.key,
                provisionType = table.provisionType,
                provisionLoad = table.provisionLoad,
                throughput = table.throughput,
            };

            var dto = JsonMapper.ToJson(request);

            return Post<TableMetadata>("createTable", dto);
        }

        /// <summary>
        /// Puts a table's metadata
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        public UnityTask<StorageResponse<TableMetadata>> UpdateTable(TableMetadata table)
        {
            var request = new StorageRequest
            {
                applicationKey = ApplicationKey,
                privateKey = PrivateKey,
                authenticationToken = AuthenticationToken,
                table = table.name,
                key = table.key,
                provisionType = table.provisionType,
                provisionLoad = table.provisionLoad,
                throughput = table.throughput,
            };

            var dto = JsonMapper.ToJson(request);

            return Post<TableMetadata>("updateTable", dto);
        }

        /// <summary>
        /// Deletes a table
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        public UnityTask<StorageResponse<TableMetadata>> DeleteTable(string table)
        {
            var request = new StorageRequest
            {
                applicationKey = ApplicationKey,
                privateKey = PrivateKey,
                authenticationToken = AuthenticationToken,
                table = table,
            };

            var dto = JsonMapper.ToJson(request);

            return Post<TableMetadata>("deleteTable", dto);
        }

        /// <summary>
        /// Returns a listing of tables
        /// </summary>
        /// <param name="limit"></param>
        /// <param name="startTable"></param>
        /// <returns></returns>
        public UnityTask<StorageResponse<TableList>> ListTables(int limit = 0, string startTable = null)
        {
            var request = new StorageRequest
            {
                applicationKey = ApplicationKey,
                privateKey = PrivateKey,
                authenticationToken = AuthenticationToken,
                startTable = startTable,
                limit = limit,
            };

            var dto = JsonMapper.ToJson(request);

            return Post<TableList>("listTables", dto);
        }
        #endregion

        #region auth


        /// <summary>
        /// Authenticates the user for the storage access role(s)
        /// </summary>
        /// <code>
        /// 
        ///IEnumerator AuthenticateAsync()
        ///{
        ///    Terminal.LogImportant("AuthenticateAsync");

        ///    /// get task
        ///    var result1 = Repository.Authenticate(AuthKey, Roles);
        ///    /// wait for it
        ///    yield return StartCoroutine(result1.WaitRoutine());
        ///    /// client error
        ///    result1.ThrowIfFaulted();
        ///    /// server error
        ///    if (result1.Result.hasError)
        ///        throw new Exception(result1.Result.error.message);

        ///    Terminal.LogSuccess("Authenticated");
        ///}
        /// </code>
        /// <returns></returns>
        public UnityTask<StorageResponse<bool>> Authenticate(string authToken, string[] roles, int timeout = 1800)
        {
            AuthenticationToken = authToken;

            if (roles == null || roles.Length == 0)
                return new UnityTask<StorageResponse<bool>>(new Exception("roles is required"));

            if (string.IsNullOrEmpty(AuthenticationToken))
                return new UnityTask<StorageResponse<bool>>(new Exception("authToken is required"));

            var body = new StorageRequest();
            body.applicationKey = ApplicationKey;
            body.privateKey = PrivateKey;
            body.authenticationToken = AuthenticationToken;
            body.timeout = timeout;
            body.roles = roles;
            //body.policies = policies;

            var json = JsonMapper.ToJson(body);

            return Post<bool>("authenticate", json);
        }

        /// <summary>
        /// Is the user authenticated ?
        /// </summary>
        /// <returns></returns>
        public UnityTask<StorageResponse<bool>> IsAuthenticated()
        {
            if (string.IsNullOrEmpty(AuthenticationToken))
                return new UnityTask<StorageResponse<bool>>(new StorageResponse<bool>(false));

            var body = new StorageRequest
            {
                applicationKey = ApplicationKey,
                authenticationToken = AuthenticationToken
            };

            var json = JsonMapper.ToJson(body);

            return Post<bool>("isAuthenticated", json);
        }

        /// <summary>
        /// Retrieves a paginated list of the names of all the roles created by the user�s application.
        /// </summary>
        /// <returns></returns>
        public UnityTask<StorageResponse<string[]>> ListRoles()
        {
            var body = new StorageRequest
            {
                applicationKey = ApplicationKey,
                privateKey = PrivateKey
            };
            var json = JsonMapper.ToJson(body);

            return Post<string[]>("listRoles", json);
        }

        /// <summary>
        /// Retrieves the all the roles associated with the subscription.
        /// </summary>
        /// <returns></returns>
        public UnityTask<StorageResponse<Role[]>> GetRoles(string[] roles)
        {

            var body = new StorageRequest
            {
                applicationKey = ApplicationKey,
                privateKey = PrivateKey,
                roles = roles,
            };

            var json = JsonMapper.ToJson(body);

            return Post<Role[]>("getRoles", json);
        }

        /// <summary>
        /// Retrieves the policies that compose the role.
        /// </summary>
        /// <returns></returns>
        public UnityTask<StorageResponse<Role>> GetRole(string role)
        {
            var body = new StorageRequest
            {
                applicationKey = ApplicationKey,
                privateKey = PrivateKey,
                role = role
            };

            var json = JsonMapper.ToJson(body);

            return Post<Role>("getRole", json);
        }
        #endregion



    }
}