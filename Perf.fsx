    #r "System.ServiceModel"
    #r "System.Runtime.Serialization"
    #r "protobuf-net.dll"
    //#r "System.Xml.dll"
    //#r "System.Net.Http.dll"
    #time "on"

    open System.ServiceModel
    open System.Runtime.Serialization

    [<DataContract; ProtoBuf.ProtoContract; CLIMutable>]
    type Vector<'T> = { [<DataMember; ProtoBuf.ProtoMember(1)>] Values : 'T[] }

    [<ServiceContract>]
    type IService =
      [<OperationContract>]
      [<ProtoBuf.ServiceModel.ProtoBehavior>]
      abstract Test: Vector<Vector<Vector<float>>> -> string

    type Service () =
      interface IService with
        member o.Test data = sprintf "Hello, %A" data

    let server = System.Threading.Thread (fun () ->
      let svh = new ServiceHost (typeof<Service>)
      let binding = NetNamedPipeBinding()
      binding.MaxReceivedMessageSize <- binding.MaxReceivedMessageSize * 4L
      svh.AddServiceEndpoint (typeof<IService>, binding, "net.pipe://localhost/123") |> ignore
      svh.Open () )

    server.IsBackground <- true
    server.Start()

    let scf: IService = 
       let binding = NetNamedPipeBinding()
       binding.MaxReceivedMessageSize <- binding.MaxReceivedMessageSize * 4L
       ChannelFactory.CreateChannel (binding, EndpointAddress "net.pipe://localhost/123")
    let rnd = System.Random ()
    let arr =
      { Values = Array.init 100 (fun i ->
       { Values =
          Array.init 10 (fun j ->
             { Values =Array.init 10 (fun k -> rnd.NextDouble()) }
          )}
       )}

    printfn "%s" (scf.Test arr)