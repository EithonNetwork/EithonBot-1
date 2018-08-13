using Discord;
using Discord.Commands;
using Discord.WebSocket;
using EithonBot.Models;
using EithonBot.Spreadsheet.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace EithonBot.Discord.Helpers
{
    class EmbedHelper
    {
        private static string CreateDescription(List<DatabaseField> databaseFields)
        {
            string description = "";
            foreach (var field in databaseFields)
            {
                var name = field.Column.ColumnHeader;
                var value = field.CellValue;
                if (value == null || value == "") value = "N/A";

                description = $"{description}\n**{name}:** {value}";
            }
            return description;
        }

        private static List<EmbedFieldBuilder> CreateInlineFields(List<DatabaseField> databaseFields)
        {
            var discordFields = new List<EmbedFieldBuilder>();
            foreach(var field in databaseFields)
            {
                if(field == null)
                {
                    var discordField = new EmbedFieldBuilder { Name = "Placeholder", Value = "N/A", IsInline = true };
                    discordFields.Add(discordField);
                }
                else
                {
                    var name = field.Column.ColumnHeader;
                    var value = field.CellValue;
                    if (value == null || value == "") value = "N/A";
                    var discordField = new EmbedFieldBuilder { Name = name, Value = value, IsInline = true };
                    discordFields.Add(discordField);
                }

            }
            return discordFields;
        }

        internal static Embed SignupProfileEmbed(IUser user, MemberSignupData memberSignupData)
        {
            var guildUser = (SocketGuildUser)user;
            var classRole = MiscHelper.GetClassRoleOfUser(user);
            if (classRole == null) classRole = "N/A";

            var builder = new EmbedBuilder();
            
            var description = CreateDescription(new List<DatabaseField> { memberSignupData.SignupComment });
            builder.WithDescription(description);

            var fields = CreateInlineFields(new List<DatabaseField> {
                memberSignupData.FirstEvent,
                memberSignupData.SecondEvent,
                memberSignupData.ThirdEvent,
                memberSignupData.FourthEvent,
            });
            builder.Fields = fields;

            //builder.WithImageUrl(memberSignupData.GearLink);

            //var footer = new EmbedFooterBuilder();
            //footer.WithText($"Last updated: {memberSignupData.GearLastUpdated}");
            //builder.WithFooter(footer);

            var author = new EmbedAuthorBuilder();
            author.WithIconUrl(guildUser.GetAvatarUrl());
            author.WithName(guildUser.Nickname);
            builder.WithAuthor(author);

            builder.WithColor(Color.Red);

            return builder.Build();
        }

        public static Embed GearProfileEmbed(IUser user, Dictionary<string, DatabaseField> databaseFields)
        {
            var guildUser = (SocketGuildUser)user;
            var classRole = MiscHelper.GetClassRoleOfUser(user);
            if (classRole == null) classRole = "N/A";

            var builder = new EmbedBuilder();

            var description = CreateDescription(new List<DatabaseField> {
                databaseFields.GetValueOrDefault("LVL"),
                databaseFields.GetValueOrDefault("Renown"),
                databaseFields.GetValueOrDefault("GearLink"),
                databaseFields.GetValueOrDefault("GearComment")
            });
            builder.WithDescription(description);

            var fields = CreateInlineFields(new List<DatabaseField> {
                databaseFields.GetValueOrDefault("AP"),
                databaseFields.GetValueOrDefault("AAP"),
                databaseFields.GetValueOrDefault("DP"),
                databaseFields.GetValueOrDefault("Class"),
                databaseFields.GetValueOrDefault("AlchStone"),
                databaseFields.GetValueOrDefault("Axe")
            });
            builder.Fields = fields;

            if (databaseFields.GetValueOrDefault("GearLink").CellValue == "N/A") databaseFields.GetValueOrDefault("GearLink").CellValue = "";
            builder.WithImageUrl(databaseFields.GetValueOrDefault("GearLink").CellValue);

            var footer = new EmbedFooterBuilder();
            footer.WithText($"Last updated: {databaseFields.GetValueOrDefault("Gear last updated").CellValue}");
            builder.WithFooter(footer);

            var author = new EmbedAuthorBuilder();
            author.WithIconUrl(guildUser.GetAvatarUrl());
            author.WithName(guildUser.Nickname);
            builder.WithAuthor(author);

            builder.WithColor(Color.Red);

            return builder.Build();
        }

        public static Embed ActivityProfileEmbed(IUser user, Dictionary<string, DatabaseField> databaseFields)
        {
            var guildUser = (SocketGuildUser)user;

            var builder = new EmbedBuilder();

            var description = CreateDescription(new List<DatabaseField> {
                databaseFields.GetValueOrDefault("Tier"),
                databaseFields.GetValueOrDefault("GA"),
                databaseFields.GetValueOrDefault("InactivityNotice")
            });
            builder.WithDescription(description);

            var fields = CreateInlineFields(new List<DatabaseField> {
                databaseFields.GetValueOrDefault("GQ"),
                databaseFields.GetValueOrDefault("NW"),
                databaseFields.GetValueOrDefault("Villa"),
                databaseFields.GetValueOrDefault("Militia"),
                databaseFields.GetValueOrDefault("Seamonsters"),
                databaseFields.GetValueOrDefault("Placeholder")
            });
            builder.Fields = fields;

            //builder.WithDescription($"" +
            //    $"**Tier**: {memberActivity.Tier}\n" +
            //    $"**Guild Activity:** {memberActivity.GA}\n" +
            //    $"**Inactivity notice:** {memberActivity.InactivityNotice}");

            //var gearScoreFields = new List<EmbedFieldBuilder>{
            //    new EmbedFieldBuilder{ Name = "GQ", Value = memberActivity.GQ, IsInline = true },
            //    new EmbedFieldBuilder{ Name = "NW", Value = memberActivity.NW, IsInline = true },
            //    new EmbedFieldBuilder{ Name = "Villa", Value = memberActivity.Villa, IsInline = true },

            //    new EmbedFieldBuilder{ Name = "Militia", Value = memberActivity.Militia, IsInline = true },
            //    new EmbedFieldBuilder{ Name = "Seamonsters", Value = memberActivity.Seamonsters, IsInline = true },
            //    new EmbedFieldBuilder{ Name = "Placeholder", Value = "N/A", IsInline = true },
            //};
            //builder.Fields = gearScoreFields;

            var footer = new EmbedFooterBuilder();
            footer.WithText($"Last updated: {databaseFields.GetValueOrDefault("Activity last updated").CellValue}");
            builder.WithFooter(footer);

            var author = new EmbedAuthorBuilder();
            author.WithIconUrl(guildUser.GetAvatarUrl());
            author.WithName(guildUser.Nickname);
            builder.WithAuthor(author);

            builder.WithColor(Color.Red);

            return builder.Build();
        }
    }
}
