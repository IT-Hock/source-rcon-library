# Source RCON Library [![Build status](https://ci.appveyor.com/api/projects/status/n9sygmqvugpvl7q6?svg=true)](https://ci.appveyor.com/project/Subtixx/source-rcon-library)

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

        return 0;
    }
    
    public string Hello_Command(string command, IList<string> args)
    {
        return "world";
    }
}
```