namespace global

open System.ServiceModel

[<ServiceContract>]
type IService =
   [<OperationContract>]
   abstract GetValues : args : string[] -> obj[][]