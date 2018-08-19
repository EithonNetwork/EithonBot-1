using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Interactive;
using Discord.Commands;

namespace EithonBot.Discord.Commands
{
    public class SampleModule : InteractiveBase
    {
        // DeleteAfterAsync will send a message and asynchronously delete it after the timeout has popped
        // This method will not block.
        [Command("delete")]
        public async Task<RuntimeResult> Test_DeleteAfterAsync()
        {
            await ReplyAndDeleteAsync("this message will delete in 10 seconds", timeout: TimeSpan.FromSeconds(10));
            return Ok();
        }

        // NextMessageAsync will wait for the next message to come in over the gateway, given certain criteria
        // By default, this will be limited to messages from the source user in the source channel
        // This method will block the gateway, so it should be ran in async mode.
        [Command("next", RunMode = RunMode.Async)]
        public async Task Test_NextMessageAsync()
        {
            await ReplyAsync("What is 2+2?");
            var response = await NextMessageAsync();
            if (response != null)
                await ReplyAsync($"You replied: {response.Content}");
            else
                await ReplyAsync("You did not reply before the timeout");
        }

        // PagedReplyAsync will send a paginated message to the channel
        // You can customize the paginator by creating a PaginatedMessage object
        // You can customize the criteria for the paginator as well, which defaults to restricting to the source user
        // This method will not block.
        [Command("paginator2")]
        public async Task Test_Paginator2()
        {
            var pages = new[] { "Page 1", "Page 2", "Page 3", "aaaaaa", "Page 5" };
            await PagedReplyAsync(null, null);
        }



        // PagedReplyAsync will send a paginated message to the channel
        // You can customize the paginator by creating a PaginatedMessage object
        // You can customize the criteria for the paginator as well, which defaults to restricting to the source user
        // This method will not block.
        [Command("paginator")]
        public async Task Test_Paginator()
        {
            var pages = new[]
            {
                new PaginatedMessage.Page
                {
                    Description = "Page1"
                },
                new PaginatedMessage.Page
                {
                    Author = new EmbedAuthorBuilder
                    {
                        IconUrl = Context.User.GetAvatarUrl(),
                        Name = Context.User.ToString(),
                        Url = Context.User.GetAvatarUrl()
                    },
                    Description = "Page 2 Description",
                    Title = "Page 2 Title",
                    Fields = new List<EmbedFieldBuilder>
                    {
                        new EmbedFieldBuilder
                        {
                            Name = "Field 1",
                            Value = "Field 1 Description"
                        }

                    },
                    ImageUrl = "https://discordapp.com/assets/9c38ca7c8efaed0c58149217515ea19f.png",
                    Color = Color.DarkMagenta,
                    FooterOverride = new EmbedFooterBuilder
                    {
                        IconUrl = Context.User.GetAvatarUrl(),
                        Text = Context.User.ToString()
                    },
                    ThumbnailUrl = Context.User.GetAvatarUrl(),
                    TimeStamp = DateTimeOffset.UtcNow,
                    Url = Context.User.GetAvatarUrl()
                },
                new PaginatedMessage.Page
                {
                    Author = new EmbedAuthorBuilder
                    {
                        IconUrl = Context.User.GetAvatarUrl(),
                        Name = Context.User.ToString(),
                        Url = Context.User.GetAvatarUrl()
                    },
                    Fields = new List<EmbedFieldBuilder>
                    {
                        new EmbedFieldBuilder
                        {
                            Name = "Field 1",
                            Value = "Field 1 Description"
                        }
                    },
                    Color = Color.DarkMagenta,
                    ThumbnailUrl = Context.User.GetAvatarUrl(),
                    TimeStamp = DateTimeOffset.UtcNow,
                    Url = Context.User.GetAvatarUrl()
                }
            };

            var pager = new PaginatedMessage
            {
                Pages = pages,
                Author = new EmbedAuthorBuilder
                {
                    IconUrl = Context.Client.CurrentUser.GetAvatarUrl(),
                    Name = Context.Client.CurrentUser.ToString(),
                    Url = Context.Client.CurrentUser.GetAvatarUrl()
                },
                Color = Color.DarkGreen,
                Content = "Default Message Content",
                Description = "Default Embed Description",
                Fields = new List<EmbedFieldBuilder>
                {
                    new EmbedFieldBuilder
                    {
                        Name = "Default Field 1",
                        Value = "Default Field Desc 1"
                    }
                },
                FooterOverride = null,
                ImageUrl = Context.Client.CurrentUser.GetAvatarUrl(),
                ThumbnailUrl = Context.Client.CurrentUser.GetAvatarUrl(),
                Options = PaginatedAppearanceOptions.Default,
                TimeStamp = DateTimeOffset.UtcNow
            };

            await PagedReplyAsync(pager, new ReactionList
            {
                Forward = true,
                Backward = true,
                Jump = true,
                Trash = true
            });
        }
    }
}
