# Source RCON Library

|        |            |
| ------------- |-------------|
| NuGet      | [![Latest NuGet Version](https://img.shields.io/nuget/v/source-rcon-server.svg)](https://www.nuget.org/packages/source-rcon-server/) |
| Github      | [![Latest Github Version](https://img.shields.io/github/release/subtixx/source-rcon-library.svg?logo=github)](https://github.com/Subtixx/source-rcon-library/releases) |
| Status      | [![Maintenance](https://img.shields.io/maintenance/yes/2018.svg)]() |
| Downloads      | [![Github Releases](https://img.shields.io/github/downloads/subtixx/source-rcon-library/latest/total.svg?logo=github)](https://github.com/Subtixx/source-rcon-library/releases) |
| Open Issues      | [![GitHub issues](https://img.shields.io/github/issues/subtixx/source-rcon-library/.svg?logo=github)](https://github.com/subtixx/source-rcon-library/issues) |
| Tests      | [![AppVeyor tests](https://img.shields.io/appveyor/tests/Subtixx/source-rcon-library.svg?logo=appveyor)](https://ci.appveyor.com/project/Subtixx/source-rcon-library) |
| Builds      | [![AppVeyor](https://img.shields.io/appveyor/ci/subtixx/source-rcon-library.svg?logo=appveyor)](https://ci.appveyor.com/project/Subtixx/source-rcon-library) |
| Coverage      | [![Codecov](https://img.shields.io/codecov/c/github/subtixx/source-rcon-library.svg)](https://codecov.io/gh/Subtixx/source-rcon-library) |

Source RCON Library is a single class solution to create a [source](https://developer.valvesoftware.com/wiki/Source_RCON_Protocol) compatible RCON Server

# Examples

## Minimum setup server
```csharp
var server = new RemoteConServer(IPAddress.Any, 27015);
server.StartListening();
```

## Adding a command as LAMBDA expression

```csharp
server.CommandManager.Add("hello", "Echos back world", (command, arguments) => {
    return "world";
});
```

## Adding a command as method

```csharp
public static class Program
{
    public static int Main(string[] args)
    {
        var server = new RemoteConServer(IPAddress.Any, 27015) {SendAuthImmediately = true};
        server.CommandManager.Add("hello", "", Hello_Command);
        
        server.StartListening();
        while (true)
        {
        }
    }
    
    public string Hello_Command(string command, IList<string> args)
    {
        return "world";
    }
}
```