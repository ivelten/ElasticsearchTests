module ElasticsearchTests.Console.Program

open System

[<EntryPoint>]
let main argv =
    //Tasks.runSingleDocumentIndexText @"E:\Files\testdoc0.docx"
    //Tasks.runSequentialIndexTest @"E:\Files"
    Tasks.runParallelIndexTest @"E:\Files"
    ExitCode.Ok