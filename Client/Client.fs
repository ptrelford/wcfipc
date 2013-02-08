open System
open System.ServiceModel
open System.Threading
open System.Windows.Forms

let createChannel() =
   let channelFactory = 
      let binding = NetNamedPipeBinding()        
      binding.MaxReceivedMessageSize <- 65536L * 4096L     
      let endpoint = EndpointAddress("net.pipe://localhost/joule/")      
      new ChannelFactory<IService>(binding, endpoint)
   let channel = channelFactory.CreateChannel()     
   channel, { 
      new IDisposable with 
         member __.Dispose() =
            channelFactory.Close()
            (channelFactory :> IDisposable).Dispose()
   }

module Async =
   let StartDisposable(task:_ Async) =
      let tokenSource = new CancellationTokenSource()
      Async.Start(task, tokenSource.Token)
      {  new IDisposable with
            member t.Dispose() =
               tokenSource.Cancel()
      }
  
[<EntryPoint;STAThread>]
let main argv = 
   Console.WriteLine("Starting Client")
   let timer = new Timer()   
   timer.Interval <- 800

   let pollCount = ref 0

   let poller = 
         async {
            while true do
               let! _ = Async.AwaitEvent timer.Tick
               incr pollCount
               try 
                  let channel, resources = createChannel()
                  use resources = resources
                  let keys = [|String('X', 40)|]
                  let xs = channel.GetValues { Topics = [|for i = 1 to 1000 do yield { Topic.Keys = keys } |] }                  
                  let text = 
                    xs.Values 
                    |> Array.map (fun x -> x.ToObject().ToString())
                    |> String.concat ","
                  Console.WriteLine(sprintf "%d %d %s" !pollCount xs.Values.Length text)
                  with ex -> Console.WriteLine(ex)                                     
         } |> Async.StartDisposable
  
   timer.Start()
   Console.WriteLine("Client started")

   while not Console.KeyAvailable do Application.DoEvents()

   timer.Stop()
   poller.Dispose()
   0
