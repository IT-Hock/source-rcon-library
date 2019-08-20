<p align="center">
    <img alt="source-rcon-logo" src="https://user-images.githubusercontent.com/20743379/35411973-0227102e-021b-11e8-9a1b-023e08c33c4e.png">
</p>

<p align="center">
    <img src="https://img.shields.io/maintenance/yes/2018.svg?style=flat-square">
</p>

<h3 align="center">
  Source RCON Library
</h3>

<p align="center">
  Source RCON Library is an easy to use, single-class solution to create a <a href="https://developer.valvesoftware.com/wiki/Source_RCON_Protocol">Valve source RCON</a> server
  which supports authentication, IP Whitelisting, a command manager and much more!
</p>

<p align="center">
    <a href="https://www.nuget.org/packages/source-rcon-server"><img src="https://img.shields.io/nuget/v/source-rcon-server.svg?style=flat-square"></a>
    <a href="https://www.nuget.org/packages/source-rcon-server"><img src="https://img.shields.io/nuget/dt/source-rcon-server.svg?style=flat-square"></a>
</p>

<p align="center">
    <a href="https://ci.appveyor.com/project/Subtixx/source-rcon-library"><img src="https://img.shields.io/appveyor/ci/subtixx/source-rcon-library.svg?logo=appveyor&style=flat-square"></a>
    <a href="https://ci.appveyor.com/project/Subtixx/source-rcon-library"><img src="https://img.shields.io/appveyor/tests/Subtixx/source-rcon-library.svg?logo=appveyor&style=flat-square"></a>
    <a href="https://codecov.io/gh/Subtixx/source-rcon-library"><img src="https://img.shields.io/codecov/c/github/subtixx/source-rcon-library.svg?style=flat-square"></a>
</p>

# Table of contents

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
        * [Debug](#debug)
        * [MaxConnectionsPerIp](#maxconnectionsperip)
        * [MaxConnections](#maxconnections)
        * [IpBanList](#ipbanlist)
        * [BanMinutes](#banminutes)
    * [Adding Commands](#adding-commands)
        * [LAMBDA](#lambda-expression)
        * [Class Method](#class-method)
* [ToDo List](#todo-list)
* [References](#references)
* [Contributing](#contributing)


# NuGet
```
Install-Package source-rcon-server
```

# Examples

## Hosted Examples

### Server

![2018-01-28_12-49-23](https://user-images.githubusercontent.com/20743379/35481729-bf9a8478-0429-11e8-97b4-4ab4787b9b7c.gif)

https://github.com/Subtixx/source-rcon-library/tree/master/Examples/ServerExample

### Client

![2018-01-28_12-52-10-e](https://user-images.githubusercontent.com/20743379/35481782-da71654a-042a-11e8-8345-d7ff9b47f749.gif)

https://github.com/Subtixx/source-rcon-library/tree/master/Examples/ClientExample

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

### Debug

This will sent debug messages to `System.Diagnostics.Debug.WriteLine` and `Console.WriteLine`

### MaxConnectionsPerIp

To disallow spamming the server with connections you can limit how many clients can connect from a single IP

### MaxConnections

With this setting you can limit how many total connections the server accepts at once

### IpBanList

Temporary list of IPs which are banned. You have to handle saving and loading yourself. Additionally this allows you
to make a command (For example called `rcon-ban` to ban specific clients from the RCON connections). The value
(since it's a dictionary) specifies the unix timestamp (Use `DateTimeExtensions` to get that value) when the ban is lifted.
To ban someone forever you can use `int.MaxValue`

### BanMinutes

This specifies how long a client will be banned when he fails to authenticate `MaxPasswordTries`.

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

## Contributing

Not much here yet. Feel free to fork and sent pull-requests.

* [TWaalen](https://github.com/TWaalen) for fixing the UnitTests

## ToDo List

* [ ] Split packets at 4096 bytes

## References

* [Valve Source RCON Protocol](https://developer.valvesoftware.com/wiki/Source_RCON_Protocol)
* [Project on NuGet](https://www.nuget.org/packages/source-rcon-server)
* [Project on AppVeyor](https://ci.appveyor.com/project/Subtixx/source-rcon-library)
* [Project on CodeCov](https://codecov.io/gh/Subtixx/source-rcon-library)