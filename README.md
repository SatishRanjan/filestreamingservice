# File Streaming Service and Client
A sample multi threaded file streaming service written in C# dotnet6 using Berkely Socket API. The received bytes stream is written as its received from the server, completely avoiding the complete file memory buffering!

## Supported OS
Cross Platform, for Linux build targetting Linux

## Service Architecture

![alternativetext](/streaming_service_architecture.png)

## Dev Environment
- VS 2022: https://visualstudio.microsoft.com/vs/
- dotnet6: https://dotnet.microsoft.com/en-us/download/dotnet/6.0

## Info
- SocketClient: contains the socket client code and /downloadedfiles directory
- StreamingService: The file streaming service code and /samplefiles directory for the server dample files
- The file name to download from the server can be passed when running client using "SocketClient.exe" or "dotnet SocketClient.dll"
  
