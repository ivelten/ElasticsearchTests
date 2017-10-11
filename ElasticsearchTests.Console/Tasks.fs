module ElasticsearchTests.Console.Tasks

open System
open System.IO
open ElasticsearchTests.Data
open Nest

let private lockObject = obj()

let private printResponse (response : IIndexResponse) =
    let audit = Seq.head response.ApiCall.AuditTrail
    let elapsed = int (audit.Ended - audit.Started).TotalMilliseconds
    let timestamp = DateTime.Now.ToString("HH:mm:ss")
    lock lockObject (fun () ->
        if response.Created 
        then printfn "%s > Document %s was indexed in %d ms." timestamp response.Id elapsed
        else printfn "%s > Error indexing document %s. Request finished in %d ms." timestamp response.Id elapsed)

let runSingleDocumentIndexText path =
    printfn "Indexing document \"%s\"..." path
    let client = Storage.getClient()
    path
    |> Storage.uploadDocumentAsync client
    |> Async.RunSynchronously
    |> printResponse

let runSequentialIndexTest path =
    printfn "Indexing documents at \"%s\"..." path
    let client = Storage.getClient()
    Directory.GetFiles(path, "*.docx", SearchOption.AllDirectories)
    |> Seq.map ((Storage.uploadDocumentAsync client) >> Async.RunSynchronously)
    |> Seq.iter printResponse

let runParallelIndexTest (agent : WorkQueueAgent) path =
    printfn "Indexing documents at \"%s\"..." path
    let client = Storage.getClient()
    Directory.GetFiles(path, "*.docx", SearchOption.AllDirectories)
    |> Seq.map (fun f -> async { 
        let! response = Storage.uploadDocumentAsync client f
        printResponse response })
    |> Seq.iter (Start >> agent.Post)