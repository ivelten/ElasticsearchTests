namespace ElasticsearchTests.Console

open System.Collections.Generic

type WorkQueueMessage =
    | Start of Async<unit>
    | Finished

type WorkQueueAgent = MailboxProcessor<WorkQueueMessage>

module WorkQueueAgent =
    let start maxDegreeOfParallelism : WorkQueueAgent = 
        MailboxProcessor.Start(fun inbox -> async {
            let queue = Queue<_>()
            let activeThreads = ref 0
            while true do
                let! msg = inbox.Receive()
                match msg with
                | Start work -> queue.Enqueue(work)
                | Finished -> decr activeThreads
                if activeThreads.Value < maxDegreeOfParallelism && queue.Count > 0 then
                    incr activeThreads
                    let work = queue.Dequeue
                    Async.Start(async {
                        do! work()
                        inbox.Post(Finished) }) })