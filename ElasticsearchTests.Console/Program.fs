module ElasticsearchTests.Console.Program

open System

[<EntryPoint>]
let main argv =
    printfn "Starting Elasticsearch tests..."
    //Tasks.runSingleDocumentIndexText @"E:\Files\testdoc0.docx"
    //Tasks.runSequentialIndexTest @"E:\Files"
    let agent = WorkQueueAgent.start 5
    Tasks.runParallelIndexTest agent @"E:\Files"
    printfn "Tests are running in background. Press any key to stop them."
    Console.ReadKey() |> ignore
    printfn "Finished testing Elasticsearch."
    ExitCode.Ok