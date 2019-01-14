using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DocDbRepo.Sample
{
    public class Program
    {
        private static async Task Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables()
                .AddUserSecrets<DocumentClientSettings>()
                .Build();

            var services = new ServiceCollection()
               .Configure<DocumentClientSettings>(configuration.GetSection("DocumentClientSettings"))
               .AddOptions()
               .BuildServiceProvider();

            var clientSettings = services.GetRequiredService<IOptions<DocumentClientSettings>>().Value;

            // get the Azure DocumentDB client
            var client = new DocumentClient(new Uri(clientSettings.EndpointUrl), clientSettings.AuthorizationKey, clientSettings.ConnectionPolicy);

            // Run demo
            var documentDb = new DocumentDbBuilder()
                .WithId("Demo")
                .AddCollection<Person>(func: cb =>
                {
                    cb
                        .IncludeIndexPath("/*", Index.Range(DataType.Number), Index.Hash(DataType.String, 3), Index.Spatial(DataType.Point))
                        .IncludeIndexPath("/Birthday/?", Index.Range(DataType.Number))
                        .ExcludeIndexPath("/FirstName/?", "/LastName/?");
                })
                .Build(client);

            // create repository for persons and set Person.FullName property as identity field (overriding default Id property name)
            var repo = documentDb.Repository<Person>();

            // output all persons in our database, nothing there yet
            PrintPersonCollection(await repo.FindAsync());

            // create a new person
            Person matt = new Person
            {
                FirstName = "Matt",
                LastName = "TBA",
                Birthday = new DateTime(1990, 10, 10),
                PhoneNumbers =
                    new Collection<PhoneNumber>
                    {
                        new PhoneNumber {Number = "555", Type = "Mobile"},
                        new PhoneNumber {Number = "777", Type = "Landline"}
                    }
            };

            // add person to database's collection (if collection doesn't exist it will be created and named as class name -it's a convenction, that can be configured during initialization of the repository)
            matt = await repo.UpsertAsync(matt);

            matt = await repo.GetAsync(matt);

            matt = await repo.ReplaceAsync(matt);
            await repo.RemoveAsync(matt);

            // create another person
            Person jack = new Person
            {
                FirstName = "Jack",
                LastName = "Smith",
                Birthday = new DateTime(1990, 10, 10),
                PhoneNumbers = new Collection<PhoneNumber>()
            };

            // add jack to collection
            jack = await repo.UpsertAsync(jack);

            // should output person and his two phone numbers
            PrintPersonCollection(await repo.FindAsync());

            // change birth date
            matt.Birthday -= new TimeSpan(500, 0, 0, 0);

            // remove landline phone number
            matt.PhoneNumbers.RemoveAt(1);

            // should update person
            matt = await repo.UpsertAsync(matt);

            // should output Matt with just one phone number
            PrintPersonCollection(await repo.FindAsync());

            // get Matt by his Id
            Person justMatt = await repo.GetAsync(matt.FullName);
            Console.WriteLine("GetByIdAsync result: " + justMatt);

            // ... or by his first name
            Person firstMatt = await repo.FirstOrDefaultAsync(p => p.FirstName.ToLower() == "matt");
            Console.WriteLine("First: " + firstMatt);

            // query all the smiths
            var smiths = (await repo.FindAsync(p => p.LastName.Equals("Smith"))).ToList();
            Console.WriteLine(smiths.Count);

            // use IQueryable, as for now supported expressions are 'Queryable.Where', 'Queryable.Select' & 'Queryable.SelectMany'
            var allSmithsPhones =
                (await repo.FindAsync()).SelectMany(p => p.PhoneNumbers).Select(p => p.Type);

            foreach (var phone in allSmithsPhones)
            {
                Console.WriteLine(phone);
            }

            // count all persons
            var personsCount = await repo.FindAsync();

            // count all jacks
            var jacksCount = await repo.FindAsync(p => p.FirstName == "Jack");

            Console.ReadKey(true);

            // remove matt from collection
            await repo.RemoveAsync(matt.FullName);

            // remove jack from collection
            await repo.RemoveAsync(jack.FullName);

            // should output nothing
            PrintPersonCollection(await repo.FindAsync());

            // remove collection
            await repo.DeleteAsync();

            Console.ReadKey(true);
        }

        private static void PrintPersonCollection(IEnumerable<Person> people)
        {
            foreach (var person in people)
            {
                Console.WriteLine(person);
            }
        }
    }
}
