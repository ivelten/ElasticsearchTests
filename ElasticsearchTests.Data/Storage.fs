namespace ElasticsearchTests.Data

open System
open System.IO
open Nest

type Policy = 
    { Id: Guid
      [<Text(Name = "filename")>]
      FileName: string
      Data: byte[] }

module Storage =
    let getClient() =
        let node = Uri("http://localhost:9200")
        use settings = new ConnectionSettings(node)
        ElasticClient(settings.DefaultIndex("policies"))

    let uploadDocumentAsync (client : ElasticClient) path = async { 
            let doc = { Id = Guid.NewGuid()
                        FileName = Path.GetFileName(path)
                        Data = File.ReadAllBytes(path) }
            return! client.IndexAsync(doc, (fun i -> i.Pipeline("attachment") :> IIndexRequest)) |> Async.AwaitTask
        }