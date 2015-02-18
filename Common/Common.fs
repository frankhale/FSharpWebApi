namespace Common

module Helpers = 

  let awaitPlainTask(t) =
    (async { return! Async.AwaitIAsyncResult <| t() |> Async.Ignore } |> Async.StartAsTask).Result

  let await(t) =
    (async { return! Async.AwaitTask <| t() } |> Async.StartAsTask).Result
