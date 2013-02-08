namespace global

open System
open System.ServiceModel
open System.Runtime.Serialization
open ProtoBuf.ServiceModel
open ProtoBuf



[<DataContract; ProtoContract; CLIMutable>]
type Values = {
   [<DataMember; ProtoMember(1)>]
   Values : ExcelValue []
   }

[<DataContract; ProtoContract; CLIMutable>]
type Topic = {
   [<DataMember; ProtoMember(1)>]
   Keys : string []
   }

[<DataContract; ProtoContract; CLIMutable>]
type Topics = {
   [<DataMember; ProtoMember(1)>]
   Topics : Topic[]
   }

[<ServiceContract>]
type IService =
   [<OperationContract; ProtoBehavior>]
   abstract GetValues : args : Topics -> Values
