using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cassandra;
using Cassandra.Mapping;

namespace object_mapper_csharp
{
    class Program
    {
        private const string CqlQuery = "INSERT INTO tbl_sample_kv (id, value) VALUES (?, ?)";

        public ICluster Cluster { get; set; }
        public ISession Session { get; set; }
        public static Guid User0Guid { get; set; }

        private static void Main(string[] args)
        {
            new Program().MainAsync(args).GetAwaiter().GetResult();
        }

        private async Task MainAsync(string[] args)
        {
            User0Guid = Guid.NewGuid();
            // build cluster connection
            Cluster =
                Cassandra.Cluster.Builder()
                    .AddContactPoint("127.0.0.1")
                    .Build();

            //Set the Mapping Configuration
            MappingConfiguration.Global.Define(
               new Map<User>()
                  .TableName("users")
                  .PartitionKey(u => u.UserId)
                  .Column(u => u.UserId, cm => cm.WithName("id")));

            // create session
            Session = await Cluster.ConnectAsync().ConfigureAwait(false);

            // prepare schema
            await Session.ExecuteAsync(new SimpleStatement("CREATE KEYSPACE IF NOT EXISTS examples WITH replication = { 'class': 'SimpleStrategy', 'replication_factor': '1' }")).ConfigureAwait(false);
            await Session.ExecuteAsync(new SimpleStatement("USE examples")).ConfigureAwait(false);
            await Session.ExecuteAsync(new SimpleStatement("CREATE TABLE IF NOT EXISTS users(id uuid, name text, age int, PRIMARY KEY(id))")).ConfigureAwait(false);

            try
            {
                //Create an instance of a Mapper from the session
                IMapper mapper = new Mapper(Session);
                await InsertOperations(mapper);
                await QueryOperations(mapper);
                await UpdateOperations(mapper);
                await DeleteOperations(mapper);
            }
            finally
            {
                await Cluster.ShutdownAsync().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Demonstrates how to delete via CQL, and then delete via POCO's using a batch
        /// </summary>
        /// <param name="mapper">The mapper object</param>
        /// <returns></returns>
        private static async Task DeleteOperations(IMapper mapper)
        {
            var user = await mapper.SingleAsync<User>("WHERE id = ?", User0Guid);
            mapper.Delete<User>("WHERE id = ?", User0Guid);

            //Delete all the users in a batch
            IEnumerable<User> users = mapper.Fetch<User>();
            var batch = mapper.CreateBatch();
            foreach (var u in users)
            {
                batch.Delete(u);
            }
            await mapper.ExecuteAsync(batch);
            users = mapper.Fetch<User>();
            Console.WriteLine($"Retrieved {users.Count()} users");
        }

        /// <summary>
        /// Demonstrates the ability to update via a POCO or via CQL with the object mapper
        /// </summary>
        /// <param name="mapper">The object mapper</param>
        /// <returns></returns>
        private static async Task UpdateOperations(IMapper mapper)
        {
            var user = await mapper.SingleAsync<User>("WHERE id = ?", User0Guid);
            user.Name = "Update POCO";
            await mapper.UpdateAsync(user);
            user = await mapper.SingleAsync<User>("WHERE id = ?", User0Guid);
            Console.WriteLine($"Retrieved {user.ToString()}");
            await mapper.UpdateAsync<User>("SET name=? WHERE id=?", "Update CQL", User0Guid);
            user = await mapper.SingleAsync<User>("WHERE id = ?", User0Guid);
            Console.WriteLine($"Retrieved {user.ToString()}");
        }

        /// <summary>
        /// Demonstrates the different query methods allowed via the mapper. 
        /// </summary>
        /// <param name="mapper">The object mapper</param>
        /// <returns></returns>
        private static async Task QueryOperations(IMapper mapper)
        {
            IEnumerable<User> users = mapper.Fetch<User>();
            Console.WriteLine($"Retrieved {users.Count()} users");
            users = await mapper.FetchAsync<User>("FROM users WHERE id = ?", User0Guid);
            Console.WriteLine($"Retrieved {users.Count()} users");
            users = await mapper.FetchAsync<User>("WHERE id = ?", User0Guid);
            Console.WriteLine($"Retrieved {users.Count()} users");
            var user = await mapper.SingleAsync<User>("WHERE id = ?", User0Guid);
            Console.WriteLine($"Retrieved {user.ToString()}");
            user = await mapper.SingleOrDefaultAsync<User>("WHERE id = ?", User0Guid);
            Console.WriteLine($"Retrieved {user.ToString()}");
            user = await mapper.FirstAsync<User>("SELECT * FROM users");
            Console.WriteLine($"Retrieved {user.ToString()}");
            user = await mapper.FirstOrDefaultAsync<User>("SELECT * FROM users");
            Console.WriteLine($"Retrieved {user.ToString()}");
        }

        /// <summary>
        /// Demonstrates how to perform singular and batch insert operations using the object mapper
        /// </summary>
        /// <param name="mapper">The object mapper</param>
        /// <returns></returns>
        private static async Task InsertOperations(IMapper mapper)
        {
            //Insert a single record using a POCO
            mapper.Insert(new User() { UserId = User0Guid, Name = "User 0", Age = 0 });
            //Insert a group of records as a batch
            var batch = mapper.CreateBatch();
            for (int i = 0; i < 10; i++)
            {
                batch.Insert(new User() { UserId = Guid.NewGuid(), Name = "User " + (i + 1), Age = i + 1 });
            }
            await mapper.ExecuteAsync(batch);
        }
    }
}