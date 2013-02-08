open System
open System.ServiceModel
open System.ServiceModel.Channels
open System.ServiceModel.Description
open System.ServiceModel.Dispatcher

[<AutoOpen>]
module Operators =
   let ( *. ) xs n = [|for i=1 to n do yield! xs|]

[<ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)>]
type Service () =      
   interface IService with
      member this.GetValues(xs) = 
         Console.WriteLine("GetValues called")         
         { Values = 
            [|for i = 1 to xs.Topics.Length do 
                yield ExcelValue.Of(String('a', 50)) 
                yield ExcelValue.Of(99.99M)
                yield ExcelValue.Of(0.001)
                yield ExcelValue.Of(true)
                yield ExcelValue.Of(DateTime.Now)
            |] 
        }

type ConsoleMessageTracer() =
   let trace (msg:Message) = Console.WriteLine(msg.ToString().Length)      
   interface IDispatchMessageInspector with
      member __.AfterReceiveRequest(request:byref<Message>, channel:IClientChannel, instanceContext:InstanceContext) =
         let buffer = request.CreateBufferedCopy(Int32.MaxValue)
         let msg = buffer.CreateMessage()
         trace msg
         request <- msg
         null
      member __.BeforeSendReply(reply:byref<Message>, correlationState:obj) =
         let msg = reply.CreateBufferedCopy(Int32.MaxValue).CreateMessage()
         trace msg
         reply <- msg         
   interface IClientMessageInspector with
      member __.AfterReceiveReply(reply:byref<Message>, correlationState:obj) =
         let msg = reply.CreateBufferedCopy(Int32.MaxValue).CreateMessage()
         trace msg
         reply <- msg
      member __.BeforeSendRequest(request:byref<Message>, channel:IClientChannel) =
         let msg = request.CreateBufferedCopy(Int32.MaxValue).CreateMessage()
         trace msg
         request <- msg
         null

type ConsoleMessageTracing () =
   inherit Attribute()
   interface IEndpointBehavior with
      member __.AddBindingParameters(_,_) = ()
      member __.ApplyClientBehavior(endpoint:ServiceEndpoint, clientRuntime:ClientRuntime) =
         clientRuntime.MessageInspectors.Add( ConsoleMessageTracer())
      member __.ApplyDispatchBehavior(endpoint:ServiceEndpoint, endpointDispatcher:EndpointDispatcher) =
         endpointDispatcher.DispatchRuntime.MessageInspectors.Add( ConsoleMessageTracer() )
      member __.Validate(_) = ()
   interface IServiceBehavior with
      member __.AddBindingParameters(_,_,_,_) = ()
      member __.ApplyDispatchBehavior(desc:ServiceDescription, host:ServiceHostBase) =
         for ch in host.ChannelDispatchers do
            let ch = ch :?> ChannelDispatcher
            for e in ch.Endpoints do
               e.DispatchRuntime.MessageInspectors.Add( ConsoleMessageTracer() )
      member __.Validate(_,_) = ()

[<EntryPoint>]
let main argv = 
   Console.WriteLine("Starting Server")

   let service = Service()   
    
   let pipe = Uri "net.pipe://localhost"
   let serviceHost = new ServiceHost(service, pipe)   
  
   let binding = NetNamedPipeBinding()      
   //binding.MaxReceivedMessageSize <- 65536L * 4096L   

   let endpoint =
      serviceHost.AddServiceEndpoint(typeof<IService>, binding, "joule")      

   serviceHost.Description.Behaviors.Add( new ConsoleMessageTracing());

   let mutable opened = false

   do try serviceHost.Open(); opened <- true
      with 
      | :? System.ServiceModel.AddressAlreadyInUseException as e -> 
         System.Diagnostics.Debug.WriteLine(e.Message)
      | ex -> 
         System.Diagnostics.Debug.WriteLine(ex)
         reraise ()

   Console.WriteLine("Server Started")
   Console.ReadLine() |> ignore

   if opened then 
      serviceHost.Close()
      (serviceHost :> IDisposable).Dispose()
   
   0
