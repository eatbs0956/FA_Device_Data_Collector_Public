# Protocol Buffers（简称：ProtoBuf）
一种开源跨平台的序列化数据结构的协议。其对于存储资料或在网络上进行通信的程序是很有用的。这个方法包含一个接口描述语言，描述一些数据结构，并提供程序工具根据这些描述产生代码，这些代码将用来生成或解析代表这些数据结构的字节流。

## 文件头与语法
* syntax = “proto3”;
  * 指定使用 Protocol Buffers 第 3 版（proto3）语法。proto3 与 proto2 有一些差异（例如没有 required、默认值行为不同、字段 presence 支持有限等）。
* package daq;
  * 定义 protobuf 的逻辑包名为 daq。生成代码时常作为命名空间或包名的一部分，用于区分不同 proto 文件中的类型。
* import “google/protobuf/timestamp.proto”;
  * 引入了 Google 的标准类型 Timestamp（用于表示时间戳）。这不会自动把实现带入，需要在生成代码或运行时链路上有 protobuf runtime 支持该类型。

## message DataPoint（采样/数据点 的结构）
  * 定义：
        message DataPoint {
            string deviceId = 1;
            string pointId = 2;
            oneof value {
                double double_value = 3;
                string string_value = 4;
                int64 int_value = 5;
                bool bool_value = 6;
                bytes bytes_value = 7;
            }
            google.protobuf.Timestamp ts = 8;
        }
  * 字段解释：
    * deviceId (字段号 1, 类型 string)：设备标识符，通常用于标明哪台设备产生了该数据点（例如设备序列号或 UUID）。
    * pointId (字段号 2, 类型 string)：点 ID 或传感器/指标的标识（例如温度传感器的 channel id、信号名等）。
    * 直接在 DataPoint 中把 value 定义为一个 oneof，包含 double、string、int64、bool、bytes 等多个具体类型。oneof 在任何时候只会有一个字段被设置，类型明确、对下游代码友好、序列化高效。
    * ts (字段号 8, 类型 google.protobuf.Timestamp)：时间戳，使用 protobuf 的标准 Timestamp 表示（包含 seconds 和 nanos 两部分，表示自 Unix epoch 的时间）。

  * 语义与注意：
    * 在 proto3 中，这些字段默认是“可选”的（但没有 explicit presence，除非使用 optional 关键字或 wrapper types）。如果消息中省略某字段，序列化/反序列化后会得到该类型的默认值（string -> “”，double -> 0.0，message -> null/默认实例），因此服务端要区分“未提供”和“提供了默认值”时需要额外手段（如改用 optional 或 wrapper）。
    * 字段号（=1,2,3,4）很重要：用于二进制编码与向后兼容。不要随意更改已使用的字段号；新增字段用新的未用号，删除字段最好保留号为 reserved。
    * 使用 double 有精度/范围考虑：若数据严格为整数或需要高精度（比如金钱），应使用 int64 或 string 或 decimal 表示。时序/传感器数据通常用 double 合适，但注意浮点误差。

## message UploadAck（上传确认）
 * 定义：
   * message UploadAck {
        bool ok = 1;
        string message = 2;
    }
 * 字段解释：
   * ok (bool)：表示服务器是否成功接收并处理了上传（true/false）。
   * message (string)：可选的文本信息，通常用于错误描述或提示（例如 “ok” / “batch received” / “invalid timestamp at index 5”）。
 * 语义：
   * 这是服务端返回给客户端的单次确认响应消息结构（在你下面的 RPC 定义中，客户端会提交一个流，服务端最后返回一个 UploadAck）。

## service Ingestion 与 RPC 定义
* 定义：
  * service Ingestion {
        // 双向流或客户端流（这里示例为客户端流）
        rpc Upload(stream DataPoint) returns (UploadAck);
    }
* 解释 RPC 签名：
  * rpc Upload(stream DataPoint) returns (UploadAck); 表示这是一个 gRPC 的“客户端流”（client streaming）RPC：客户端会向服务器发送一连串 DataPoint 消息流（stream DataPoint），当客户端发送完并关闭流（half-close）后，服务器会基于接收到的全部数据返回一个单一的 UploadAck 响应给客户端。
  * 与之对应的其他 gRPC 模式：
    * Unary（单次请求-单次响应）：rpc Foo(Request) returns (Response);
    * Server streaming（服务端流）：rpc StreamSomething(Request) returns (stream Response);
    * Bidirectional streaming（双向流）：rpc Chat(stream Message) returns (stream Message);
  * 这个定义适合场景：客户端批量上传一系列采样点，服务器在接收完成后做批量写入/校验/聚合并返回结果/状态。

## 行为要点：
* 流的生命周期由 gRPC 框架管理，通常客户端可以在发送中间结果时进行窗口/流控。服务器可以在接收过程中做部分校验并在结束时决定成功/失败。
* 若在接收流期间发现严重错误，服务器可以直接返回错误 status（例如 INVALID_ARGUMENT、INTERNAL 等），这会导致 RPC 失败，客户端会收到相应错误并停止/处理。
* 如果希望边收边响应（例如实时确认每个点），应选择双向流或服务器流，当前定义只在结束时返回单个 ack。

## 与代码生成/使用相关
* 生成代码需要 protoc 与相应语言插件，例如：
  * Go: protoc --go_out=. --go-grpc_out=. device.proto （并通常需要在 proto 文件中指定 go_package 选项，推荐加上）
  * Java/Python/C# 等语言也有对应插件。
* Timestamp 类型映射：
  * google.protobuf.Timestamp 在各语言会映射为规范的时间类型（如 Go 的 time.Time、Java 的 com.google.protobuf.Timestamp 或转换到 java.time.Instant）。要注意序列化与本地时区/格式的转换。
* 推荐在 proto 文件中添加语言特定的 package 设置，例如 go_package、java_package、csharp_namespace，便于生成代码放到合适的命名空间/包中。

## 兼容性与版本控制提示
* 不要重用或更改已发布字段号；新增字段使用新的字段号。
* 如果要删除字段，推荐在 proto 中使用 reserved 关键字保留该字段号/名字，防止未来不小心复用。
* 若需要字段 presence（知道是否显式设置），可以：
  * 在 proto3 中使用 optional（从某些版本起支持），或
  * 使用 wrapper types（google.protobuf.DoubleValue / StringValue / BoolValue 等），这些都是 message，因此有 presence 信息。
* 对于时间戳，如果只需要整数 epoch（毫秒/纳秒），也可用 int64 存储，避免依赖标准 Timestamp 类型（但 Timestamp 语义更清晰）。

## 设计与实现建议（实用提示）
* 如果数据点很多并且有性能/吞吐需求，考虑：
  * 客户端将大量 DataPoint 打包成流发送（你当前的 client-stream 模式适合批量），但如果需要持续双向通讯或服务器侧推送处理进度，考虑双向流。
  * 在服务端对收到的流做分批写入（batch insert）以提高 DB 写入效率。
  * 在 UploadAck 中返回更多元信息（例如已写入条数、部分错误详情、处理耗时等），方便客户端重试/日志。
* 校验策略：
  * 服务器收到流后需验证必需字段（例如 deviceId/pointId 非空、ts 合理、value 非 NaN/Inf）。
  * 对时间戳的合法性（如未来时间或过旧时间）建立策略并在 message 字段中说明原因。
* 安全与认证：
  * gRPC 通常配合 TLS 与认证机制（token、mTLS）。上传数据常常需要鉴权，注意不要把匿名写入暴露到公网。

## 一个常见交互流程（举例）
  * 客户端建立 gRPC 连接 -> 调用 Upload() 并得到请求流句柄 -> 循环发送多个 DataPoint 到该流 -> 发送完毕后 half-close（告知服务器数据发完）-> 等待并接收服务器返回的单个 UploadAck -> 根据 UploadAck.ok 或错误状态决定重试/记录/报错。

## 总结（要点回顾）
* 这是一个非常直接的 proto 设计：DataPoint 表示单个采样点（设备 id、点 id、值、时间），UploadAck 表示整体上传结果，Ingestion.Upload 使用客户端流 RPC 模式，适用于客户端批量、连续地将多个 DataPoint 上传给服务端并在完成后获得一次性确认。
* 实际使用时注意 proto3 的 presence/default 行为、字段编号与兼容性、Timestamp 的映射与校验、以及 gRPC 流的错误/重试/安全策略。