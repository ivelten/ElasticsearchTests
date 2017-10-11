module ElasticsearchTests.Console.Tasks

open System.IO
open ElasticsearchTests.Data
open Nest

let private printResponse (response : IIndexResponse) =
    let audit = Seq.head response.ApiCall.AuditTrail
    let elapsed = audit.Ended - audit.Started
    if response.Created 
    then printfn "Document %s was indexed in %d ms." response.Id (int elapsed.TotalMilliseconds)
    else printfn "Error indexing document %s. %O" response.Id response.ApiCall

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

let runParallelIndexTest path =
    printfn "Indexing documents at \"%s\"..." path
    let agent = WorkQueueAgent.start 5
    let client = Storage.getClient()
    Directory.GetFiles(path, "*.docx", SearchOption.AllDirectories)
    |> Seq.map (fun f -> async { 
        let! response = Storage.uploadDocumentAsync client f
        printResponse response })
    |> Seq.iter (Start >> agent.Post)