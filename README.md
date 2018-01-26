# ![Logo](https://user-images.githubusercontent.com/20743379/35411973-0227102e-021b-11e8-9a1b-023e08c33c4e.png) Source RCON Library [![Maintenance](https://img.shields.io/maintenance/yes/2018.svg)]()

Source RCON Library is an easy to use, single-class solution to create a [Valve source RCON](https://developer.valvesoftware.com/wiki/Source_RCON_Protocol) server which supports authentication,
IP Whitelisting and a command manager

* [NuGet](#nuget)
* [Examples](#examples)
    * [Setup](#setup)
        * [Minimum setup server](#minimum-setup-server)
    * [Configuration](#configuration)
        * [EmptyPayloadKick](#emptypayloadkick)
        * [EnableIpWhitelist / IpWhitelist](#enableipwhitelist-/-ipwhitelist)
        * [InvalidPacketKick](#invalidpacketkick)
        * [MaxPasswordTries](#maxpasswordtries)
        * [Password](#password)
        * [SendAuthImmediately](#sendauthimmediately)
        * [UseCustomCommandHandler / OnCommandReceived](#usecustomcommandhandler-/-oncommandreceived)
    * [Adding Commands](#adding-commands)
        * [LAMBDA](#lambda-expression)
        * [Class Method](#class-method)
* [ToDo List](#todo-list)
* [References](#references)
* [Contributing](#contributing)

# Build and Tests Status

[![AppVeyor](https://img.shields.io/appveyor/ci/subtixx/source-rcon-library.svg?logo=appveyor)](https://ci.appveyor.com/project/Subtixx/source-rcon-library)
[![AppVeyor tests](https://img.shields.io/appveyor/tests/Subtixx/source-rcon-library.svg?logo=appveyor)](https://ci.appveyor.com/project/Subtixx/source-rcon-library)

# Coverage

[![Codecov](https://img.shields.io/codecov/c/github/subtixx/source-rcon-library.svg)](https://codecov.io/gh/Subtixx/source-rcon-library)

# NuGet
```
Install-Package source-rcon-server
```

# Examples

## Setup

### Minimum setup server
```csharp
var server = new RemoteConServer(IPAddress.Any, 27015);
server.StartListening();
```

## Configuration

This library allows you to configure certain things, here I'll explain what you can configure and
how it affects the server.

### EmptyPayloadKick

When the client is authenticated (already entered his password), the server checks the payload
(or body as it's called on the wiki) before processing the packet.
If the payload is empty and `EmptyPayloadKick` is true, the client gets disconnected from the RCON server.

### EnableIpWhitelist / IpWhitelist

Sometimes it's necessary to protect the RCON server from intruders. This setting will allow you to
specify a wildcard based IP Whitelist to check against when a new client connects.

### InvalidPacketKick

The server will ensure that the client will only sent packets (after authentication) that are of type
ExecCommand. If this is not the case, and the setting is true, the client will be disconnected if he
sents a packet with a different type. 

### MaxPasswordTries

To protect the server against brute force attacks it checks if the client has exceeded the maximum
allowed of tries specified by this setting. If he has he will be disconnected immediately

### Password

This is the password that has to be entered before any commands can be executed

### SendAuthImmediately

Sometimes client libraries don't match the specification, this was the case when testing the library.
If you've trouble using a specific client, and haven't tried setting this to true this might help.
(If not please open an issue!)

### UseCustomCommandHandler / OnCommandReceived

Sometimes you just want to do it yourself. Setting `UseCustomCommandHandler` to true and adding an eventhandler
to `OnCommandReceived` will ignore the built-in CommandManager and sent all commands to the eventhandler instead

## Adding Commands

There are two main ways to create commands, either as [LAMBDA expression](#lambda-expression)
or as [Class Method](#class-method). You can optionally specify a usage and description to in a custom
help command (Example is in RCONServerLibExample), if you don't specify it it'll be empty by default.

```csharp
public void Add(string name, CommandHandler handler)
public void Add(string name, string description, CommandHandler handler)
public void Add(string name, string usage, string description, CommandHandler handler)
```

### LAMBDA expression

```csharp
public static class Program
{
    public static int Main(string[] args)
    {
        var server = new RemoteConServer(IPAddress.Any, 27015);
        server.CommandManager.Add("hello", "Echos back world", (command, arguments) => {
            return "world";
        });
    }
}
```

### Class Method

```csharp
public static class Program
{
    public static int Main(string[] args)
    {
        var server = new RemoteConServer(IPAddress.Any, 27015);
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

## ToDo List

* [ ] Ban List
* [ ] Split packets at 4096 bytes

## References

* [Valve Source RCON Protocol](https://developer.valvesoftware.com/wiki/Source_RCON_Protocol)
* [Project on NuGet](https://www.nuget.org/packages/source-rcon-server)
* [Project on AppVeyor](https://ci.appveyor.com/project/Subtixx/source-rcon-library)
* [Project on CodeCov](https://codecov.io/gh/Subtixx/source-rcon-library)