using DiscordBot.Handlers.Dialogue.Steps;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using System;
using System.Threading.Tasks;

namespace DiscordBot.Bots.Handlers.Dialogue.Steps
{
    public class itemPriceStep : DialogueStepBase
    {
        private readonly int? _minValue;
        private readonly int? _maxValue;

        private IDialogueStep _nextStep;

        public itemPriceStep(
            string content,
            IDialogueStep nextStep,
            int? minValue = null,
            int? maxValue = null) : base(content)
        {
            _nextStep = nextStep;
            _minValue = minValue;
            _maxValue = maxValue;
        }

        public Action<int> OnValidResult { get; set; } = delegate { };

        public override IDialogueStep NextStep => _nextStep;

        public void SetNextStep(IDialogueStep nextStep)
        {
            _nextStep = nextStep;
        }

        public override async Task<bool> ProcessStep(DiscordClient client, DiscordChannel channel, DiscordUser user)
        {
            var respondEmbed = new DiscordEmbedBuilder
            {
                Title = $"Item Creation Tool:",
                Description = $"{_content}",
                Color = DiscordColor.Blue,
            };

            respondEmbed.AddField("To cancel adding an item", "Use the !cancel command");

            if (_minValue.HasValue)
            {
                respondEmbed.AddField("Min Value:", $"{_minValue.Value}");
            }
            if (_maxValue.HasValue)
            {
                respondEmbed.AddField("Max Value:", $"{_maxValue.Value}");
            }

            var interactivity = client.GetInteractivity();

            while (true)
            {
                var embed = await channel.SendMessageAsync(embed: respondEmbed).ConfigureAwait(false);

                OnMessageAdded(embed);

                var messageResult = await interactivity.WaitForMessageAsync(x => x.ChannelId == channel.Id && x.Author.Id == user.Id).ConfigureAwait(false);

                OnMessageAdded(messageResult.Result);

                if (messageResult.Result.Content.Equals("!cancel", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }

                if(!int.TryParse(messageResult.Result.Content, out int inputValue))
                {
                    await TryAgain(channel, $"Your input is not an integer").ConfigureAwait(false);
                    continue;
                }

                if (_minValue.HasValue)
                {
                    if (inputValue < _minValue.Value)
                    {
                        await TryAgain(channel, $"Your input value: {inputValue} is smaller than: {_minValue}").ConfigureAwait(false);
                        continue;
                    }
                }
                if (_maxValue.HasValue)
                {
                    if (inputValue > _maxValue.Value)
                    {
                        await TryAgain(channel, $"Your input value: {inputValue} is larger than: {_maxValue}").ConfigureAwait(false);
                        continue;
                    }
                }

                OnValidResult(inputValue);

                return false;
            }
        }
    }
}