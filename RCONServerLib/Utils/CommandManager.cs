using System.Collections.Generic;

namespace RCONServerLib.Utils
{
    /// <summary>
    ///     Command manager
    /// </summary>
    public class CommandManager
    {
        protected Dictionary<string, Command> Commands;

        public CommandManager()
        {
            Commands = new Dictionary<string, Command>();
        }

        /// <summary>
        ///     Adds command to list of command handlers.
        /// </summary>
        /// <param name="command"></param>
        public void Add(Command command)
        {
            Commands[command.Name] = command;
        }

        /// <summary>
        ///     Adds new command handler.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="description"></param>
        /// <param name="handler"></param>
        public void Add(string name, string description, CommandFunc handler)
        {
            Add(name, "", description, handler);
        }

        /// <summary>
        ///     Adds new command handler.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="usage"></param>
        /// <param name="description"></param>
        /// <param name="handler"></param>
        public void Add(string name, string usage, string description, CommandFunc handler)
        {
            Commands[name] = new Command(name, usage, description, handler);
        }

        /// <summary>
        ///     Returns command or null, if the command doesn't exist.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public Command GetCommand(string name)
        {
            Command command;
            Commands.TryGetValue(name, out command);
            return command;
        }
    }

    /// <summary>
    ///     Generalized command holder
    /// </summary>
    public class Command
    {
        public Command(string name, string usage, string description, CommandFunc func)
        {
            Name = name;
            Usage = usage;
            Description = description;
            Func = func;
        }

        public string Name { get; protected set; }
        public string Usage { get; protected set; }
        public string Description { get; protected set; }
        public CommandFunc Func { get; protected set; }
    }
    
    public delegate string CommandFunc(string command, IList<string> args);
}