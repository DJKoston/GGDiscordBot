using DiscordBot.Core.Services.CustomCommands;
using DiscordBot.DAL;
using DiscordBot.DAL.Models.CustomCommands;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.CommandsNext.Entities;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DiscordBot.Bots.Handlers.HelpFormatters
{
    public class CustomHelpFormatter : BaseHelpFormatter
    {
        protected DiscordEmbedBuilder _embed;
        private readonly RPGContext _context;
        protected List<CustomCommand> CustomCommands;

        public CustomHelpFormatter(CommandContext ctx, RPGContext context) : base(ctx)
        {
            _context = context;
            _embed = new DiscordEmbedBuilder
            {
                Title = "GG-Bot Commands:",
                Color = DiscordColor.Gold,
            };

            var serverCommands = _context.CustomCommands.Where(x => x.GuildId == ctx.Guild.Id);
            CustomCommands = serverCommands.Where(x => x.Trigger.Contains("!")).OrderBy(x => x.Trigger).ToList();
        }

        public override BaseHelpFormatter WithCommand(Command command)
        {
            _embed.WithDescription("Please find detailed information for the command below.");

            _embed.AddField(command.Name, command.Description);

            return this;
        }

        public override BaseHelpFormatter WithSubcommands(IEnumerable<Command> cmds)
        {
            _embed.WithDescription("Please find a full list of commands that are avaliable to you!");

            List<string> configCommands = new List<string>();
            List<string> gameCommands = new List<string>();
            List<string> manageCommands = new List<string>();
            List<string> miscCommands = new List<string>();
            List<string> modCommands = new List<string>();
            List<string> nowLiveCommands = new List<string>();
            List<string> profileCommands = new List<string>();
            List<string> quoteCommands = new List<string>();
            List<string> reactionRoleCommands = new List<string>();
            List<string> setupCommands = new List<string>();
            List<string> streamerCommands = new List<string>();
            List<string> suggestionCommands = new List<string>();
            List<string> serverSpecificCommands = new List<string>();

            foreach (var cmd in cmds)
            {
                if (cmd.Module.ModuleType.Name == "ConfigCommands")
                {
                    if(cmd is CommandGroup commandGroup)
                    {
                        var childCommands = commandGroup.Children;

                        foreach (var childCommand in childCommands)
                        {
                            configCommands.Add($"`!{cmd.Name} {childCommand.Name}`");
                        }
                    }

                    else
                    {
                        configCommands.Add($"`!{cmd.Name}`");
                    }
                }

                if (cmd.Module.ModuleType.Name == "GameCommands")
                {
                    if (cmd is CommandGroup commandGroup)
                    {
                        var childCommands = commandGroup.Children;

                        foreach (var childCommand in childCommands)
                        {
                            gameCommands.Add($"`!{cmd.Name} {childCommand.Name}`");
                        }
                    }

                    else
                    {
                        gameCommands.Add($"`!{cmd.Name}`");
                    }
                }

                if (cmd.Module.ModuleType.Name == "ManageCommands")
                {
                    if (cmd is CommandGroup commandGroup)
                    {
                        var childCommands = commandGroup.Children;

                        foreach (var childCommand in childCommands)
                        {
                            manageCommands.Add($"`!{cmd.Name} {childCommand.Name}`");
                        }
                    }

                    else
                    {
                        manageCommands.Add($"`!{cmd.Name}`");
                    }
                }

                if (cmd.Module.ModuleType.Name == "MiscCommands")
                {
                    if (cmd is CommandGroup commandGroup)
                    {
                        var childCommands = commandGroup.Children;

                        foreach (var childCommand in childCommands)
                        {
                            miscCommands.Add($"`!{cmd.Name} {childCommand.Name}`");
                        }
                    }

                    else
                    {
                        miscCommands.Add($"`!{cmd.Name}`");
                    }
                }

                if (cmd.Module.ModuleType.Name == "ModCommands")
                {
                    if (cmd is CommandGroup commandGroup)
                    {
                        var childCommands = commandGroup.Children;

                        foreach (var childCommand in childCommands)
                        {
                            modCommands.Add($"`!{cmd.Name} {childCommand.Name}`");
                        }
                    }

                    else
                    {
                        modCommands.Add($"`!{cmd.Name}`");
                    }
                }

                if (cmd.Module.ModuleType.Name == "NowLiveCommands")
                {
                    if (cmd is CommandGroup commandGroup)
                    {
                        var childCommands = commandGroup.Children;

                        foreach (var childCommand in childCommands)
                        {
                            if (childCommand is CommandGroup childGroup)
                            {
                                var subchildCommands = childGroup.Children;

                                foreach (var subchildCommand in subchildCommands)
                                {
                                    nowLiveCommands.Add($"`!{cmd.Name} {childCommand.Name} {subchildCommand.Name}`");
                                }
                            }

                            else
                            {
                                nowLiveCommands.Add($"`!{cmd.Name} {childCommand.Name}");
                            }
                        }
                    }

                    else
                    {
                        nowLiveCommands.Add($"`!{cmd.Name}`");
                    }
                }

                if (cmd.Module.ModuleType.Name == "ProfileCommands")
                {
                    if (cmd is CommandGroup commandGroup)
                    {
                        var childCommands = commandGroup.Children;

                        foreach (var childCommand in childCommands)
                        {
                            profileCommands.Add($"`!{cmd.Name} {childCommand.Name}`");
                        }
                    }

                    else
                    {
                        profileCommands.Add($"`!{cmd.Name}`");
                    }
                }

                if (cmd.Module.ModuleType.Name == "QuoteCommands")
                {
                    if (cmd is CommandGroup commandGroup)
                    {
                        var childCommands = commandGroup.Children;

                        foreach (var childCommand in childCommands)
                        {
                            quoteCommands.Add($"`!{cmd.Name} {childCommand.Name}`");
                        }
                    }

                    else
                    {
                        quoteCommands.Add($"`!{cmd.Name}`");
                    }
                }

                if (cmd.Module.ModuleType.Name == "ReactionRoleCommands")
                {
                    if (cmd is CommandGroup commandGroup)
                    {
                        var childCommands = commandGroup.Children;

                        foreach (var childCommand in childCommands)
                        {
                            reactionRoleCommands.Add($"`!{cmd.Name} {childCommand.Name}`");
                        }
                    }

                    else
                    {
                        reactionRoleCommands.Add($"`!{cmd.Name}`");
                    }
                }

                if (cmd.Module.ModuleType.Name == "SetupCommands")
                {
                    if (cmd is CommandGroup commandGroup)
                    {
                        var childCommands = commandGroup.Children;

                        foreach (var childCommand in childCommands)
                        {
                            setupCommands.Add($"`!{cmd.Name} {childCommand.Name}`");
                        }
                    }

                    else
                    {
                        setupCommands.Add($"`!{cmd.Name}`");
                    }
                }

                if (cmd.Module.ModuleType.Name == "StreamerCommands")
                {
                    if (cmd is CommandGroup commandGroup)
                    {
                        var childCommands = commandGroup.Children;

                        foreach (var childCommand in childCommands)
                        {
                            streamerCommands.Add($"`!{cmd.Name} {childCommand.Name}`");
                        }
                    }

                    else
                    {
                        streamerCommands.Add($"`!{cmd.Name}`");
                    }
                }

                if (cmd.Module.ModuleType.Name == "SuggestionCommands")
                {
                    if (cmd is CommandGroup commandGroup)
                    {
                        var childCommands = commandGroup.Children;

                        foreach (var childCommand in childCommands)
                        {
                            suggestionCommands.Add($"`!{cmd.Name} {childCommand.Name}`");
                        }
                    }

                    else
                    {
                        suggestionCommands.Add($"`!{cmd.Name}`");
                    }
                }
            }

            foreach (var ccmd in CustomCommands)
            {
                serverSpecificCommands.Add(ccmd.Trigger.Replace($"{ccmd.Trigger}", $"`{ccmd.Trigger}`"));
            }

            if (configCommands.Count != 0)
            {
                _embed.AddField("Configuration Commands:", String.Join(", ", configCommands.ToArray()));
            }

            if (gameCommands.Count != 0)
            {
                _embed.AddField("Game Commands:", String.Join(", ", gameCommands.ToArray()));
            }

            if (manageCommands.Count != 0)
            {
                _embed.AddField("Manage Custom's Commands:", String.Join(", ", manageCommands.ToArray()));
            }

            if (miscCommands.Count != 0)
            {
                _embed.AddField("Misc Commands:", String.Join(", ", miscCommands.ToArray()));
            }

            if (modCommands.Count != 0)
            {
                _embed.AddField("Moderator Commands:", String.Join(", ", modCommands.ToArray()));
            }

            if (nowLiveCommands.Count != 0)
            {
                _embed.AddField("Now Live Management Commands:", String.Join(", ", nowLiveCommands.ToArray()));
            }

            if (profileCommands.Count != 0)
            {
                _embed.AddField("Profile Commands:", String.Join(", ", profileCommands.ToArray()));
            }

            if (quoteCommands.Count != 0)
            {
                _embed.AddField("Quote Commands:", String.Join(", ", quoteCommands.ToArray()));
            }

            if (reactionRoleCommands.Count != 0)
            {
                _embed.AddField("Reaction Role Commands:", String.Join(", ", reactionRoleCommands.ToArray()));
            }

            if (setupCommands.Count != 0)
            {
                _embed.AddField("Setup Commands:", String.Join(", ", setupCommands.ToArray()));
            }

            if (streamerCommands.Count != 0)
            {
                _embed.AddField("Streamer Commands:", String.Join(", ", streamerCommands.ToArray()));
            }

            if (suggestionCommands.Count != 0)
            {
                _embed.AddField("Suggestion Commands:", String.Join(", ", suggestionCommands.ToArray()));
            }

            if (serverSpecificCommands.Count != 0)
            {
                _embed.AddField("Server Specific Custom Commands:", String.Join(", ", serverSpecificCommands.ToArray()));
            }

            return this;
        }

        public override CommandHelpMessage Build()
        {
            return new CommandHelpMessage(embed: _embed);
        }
    }
}
