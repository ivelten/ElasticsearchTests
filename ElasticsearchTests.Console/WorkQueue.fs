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
            let count = ref 0
            while true do
                let! msg = inbox.Receive()
                match msg with
                | Start work -> queue.Enqueue(work)
                | Finished -> decr count
                if count.Value < maxDegreeOfParallelism && queue.Count > 0 then
                    incr count
                    let work = queue.Dequeue
                    Async.Start(async {
                        do! work()
                        inbox.Post(Finished) }) })