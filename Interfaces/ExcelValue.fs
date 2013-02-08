namespace global

open System
open System.Runtime.Serialization
open ProtoBuf

type ValueCode = Text = 0 | Number = 1 | Currency = 2 | Logical = 3 | DateTime = 4

[<DataContract; ProtoContract; CLIMutable>]
type ExcelValue = {
        [<ProtoMember(1)>]
        Code : ValueCode
        [<ProtoMember(2, IsRequired=false)>]
        Text : string
        [<ProtoMember(3, IsRequired=false)>]
        Number : float
        [<ProtoMember(4, IsRequired=false)>]
        Currency : decimal
        [<ProtoMember(5, IsRequired=false)>]
        Logical : bool
        [<ProtoMember(6, IsRequired=false)>]
        DateTime : DateTime
    }
    with
    static member Empty = { 
        Code = ValueCode.Text; 
        Text = Unchecked.defaultof<string>
        Number = Unchecked.defaultof<float> 
        Currency = Unchecked.defaultof<decimal>
        Logical = Unchecked.defaultof<bool>
        DateTime = Unchecked.defaultof<DateTime>
        }
    static member Of(text:string) = 
        { ExcelValue.Empty with Code = ValueCode.Text; Text = text }
    static member Of(number:float) = 
        { ExcelValue.Empty with Code = ValueCode.Number; Number = number }
    static member Of(currency:decimal) = 
        { ExcelValue.Empty with Code = ValueCode.Currency; Currency = currency }
    static member Of(logical:bool) = 
        { ExcelValue.Empty with Code = ValueCode.Logical; Logical = logical }
    static member Of(datetime:DateTime) = 
        { ExcelValue.Empty with Code = ValueCode.DateTime; DateTime = datetime }
    static member Of(value:obj) =
        match value with
        | :? string as text -> ExcelValue.Of(text)
        | :? float as number -> ExcelValue.Of(number)
        | :? decimal as currency -> ExcelValue.Of(currency)
        | :? bool as logical -> ExcelValue.Of(logical)
        | :? DateTime as datetime -> ExcelValue.Of(datetime)
        | _ -> ExcelValue.Of(value.ToString())
    member value.ToObject() =
        match value.Code with
        | ValueCode.Text  -> box value.Text
        | ValueCode.Number -> box value.Number
        | ValueCode.Currency -> box value.Currency
        | ValueCode.Logical -> box value.Logical
        | ValueCode.DateTime -> box value.DateTime
        | _ -> invalidOp "Unknown value type"
    override value.ToString() =
        value.ToObject().ToString()