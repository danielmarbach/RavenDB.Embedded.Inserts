using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Raven.Client.Documents;
using Raven.Embedded;

namespace Raven41Embedded
{
    class Program
    {
        private static long counter;

        static async Task Main(string[] args)
        {
            var path = Path.Combine(Path.Combine(Path.GetTempPath(), "ravendb"), Path.GetFileNameWithoutExtension(Path.GetTempFileName()));
            Directory.CreateDirectory(path);
            var duration = TimeSpan.FromSeconds(120);
            try
            {
                EmbeddedServer.Instance.StartServer(new ServerOptions
                    {DataDirectory = path, AcceptEula = true });
                var documentStore =
                    await EmbeddedServer.Instance.GetDocumentStoreAsync(new DatabaseOptions("users")
                        {SkipCreatingDatabase = false});

                // warmup
                for (int i = 0; i < 10; i++)
                {
                    // 10 concurrent inserts
                    await Task.WhenAll(
                        InsertUser(documentStore),
                        InsertUser(documentStore),
                        InsertUser(documentStore),
                        InsertUser(documentStore),
                        InsertUser(documentStore),
                        InsertUser(documentStore),
                        InsertUser(documentStore),
                        InsertUser(documentStore),
                        InsertUser(documentStore),
                        InsertUser(documentStore));
                }

                var cts = new CancellationTokenSource(duration);
                while (!cts.IsCancellationRequested)
                {
                    // 10 concurrent inserts
                    await Task.WhenAll(
                        InsertUser(documentStore),
                        InsertUser(documentStore),
                        InsertUser(documentStore),
                        InsertUser(documentStore),
                        InsertUser(documentStore),
                        InsertUser(documentStore),
                        InsertUser(documentStore),
                        InsertUser(documentStore),
                        InsertUser(documentStore),
                        InsertUser(documentStore));
                }

                documentStore.Dispose();
                EmbeddedServer.Instance.Dispose();

                Console.WriteLine($"{Interlocked.Read(ref counter) / duration.TotalSeconds} docs/s");
                Console.ReadLine();
            }
            catch (Exception ex)
            {

            }
            finally
            {
                Directory.Delete(path, true);
            }
        }

        private static async Task InsertUser(IDocumentStore documentStore)
        {
            using (var session = documentStore.OpenAsyncSession())
            {
                await session.StoreAsync(new User());
                await session.SaveChangesAsync();
            }

            Interlocked.Increment(ref counter);
        }
    }

    class User
    {
        public string Id { get; set; }

        public string Content { get; set; } =
            "oTg0zJuHNPrDkCRSGWvLADIc53eumyfxv04D2pE12vZixe66Kk0zFLGFAsVPs9CtvTWM9ek5NL5iNeqpL1UYvtgWPgeSPZMMS2VUJHgrAztLVuI4CzYRoTX1FPs5gQOBzptUC1dJL5V019i434AVdHAZVHY3Z5wHYRqQ5uaLOC87eXSfuxbm7onIrDsjiZDTSfpB6TqbfzjGQJ49N6Smf3L3dxKJUMyxeW8Ei0KzwJBAMJOcsLcWXz5tTBM4NXk8CMNLfJ4oXmKJpDrnuCtxo4vdDmsNDAajleG0RH6sz8fFNUCVuIRMppHGfPtt5hxcmJkJBKlJP2yTISQJu4dfUeUdIBYpZHb1K3Crn5nN3Rb7pvf9cFq1iTfDxuhAUKoWvTSWz4I7OfXhIDiZcInT2rNn6zrTENQ5WezSOcr6Mu36NI9tsuzb4HQ0AxBqSURk94unHoVvKW7WwqacOaRd53o7Im8j3IWGwgKmaqilOuH6MiGRlL5OjbB4Mh87ApMsEFZciCPyBLUXtBfjYUnhTSyWD2TX8fgpePxQRnK2CPHoijWwF8xB67ZYI6d5mOQZz7uf1EgUa7v1ePbsWanGoxinyUeRKJSrjEwpxa83uM3wslgWSNWeuADu4g1DEb8r";
    }
}
