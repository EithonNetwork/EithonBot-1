using Discord;
using Discord.Commands;
using Discord.WebSocket;
using EithonBot.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace EithonBot.Discord.Helpers
{
    class EmbedHelper
    {
        public static Embed UserGearEmbed(IUser user, MemberGear memberGear)
        {
            var guildUser = (SocketGuildUser)user;
            var classRole = MiscHelper.GetClassRoleOfUser(user);
            if (classRole == null) classRole = "N/A";

            var builder = new EmbedBuilder();
            
            builder.WithDescription($"" +
                $"**LVL**: {memberGear.LVL}\n" +
                $"**Renown Score:** {memberGear.Renown}\n" +
                $"**Gear Link:** {memberGear.GearLink}\n" +
                $"**Gear Comment:** {memberGear.GearComment}");

            var gearScoreFields = new List<EmbedFieldBuilder>{
                new EmbedFieldBuilder{ Name = "AP", Value = memberGear.AP, IsInline = true },
                new EmbedFieldBuilder{ Name = "AAP", Value = memberGear.AAP, IsInline = true },
                new EmbedFieldBuilder{ Name = "DP", Value = memberGear.DP, IsInline = true },

                new EmbedFieldBuilder{ Name = "Class", Value = classRole, IsInline = true },
                new EmbedFieldBuilder{ Name = "AlchStone", Value = memberGear.AlchStone, IsInline = true },
                new EmbedFieldBuilder{ Name = "Axe", Value = memberGear.Axe, IsInline = true },
            };
            builder.Fields = gearScoreFields;

            if (memberGear.GearLink == "N/A") memberGear.GearLink = "";
            builder.WithImageUrl(memberGear.GearLink);

            var footer = new EmbedFooterBuilder();
            footer.WithText($"Last updated: {memberGear.GearLastUpdated}");
            builder.WithFooter(footer);

            var author = new EmbedAuthorBuilder();
            author.WithIconUrl(guildUser.GetAvatarUrl());
            author.WithName(guildUser.Nickname);
            builder.WithAuthor(author);

            builder.WithColor(Color.Red);

            return builder.Build();
        }

        public static Embed UserActivityEmbed(IUser user, MemberActivity memberActivity)
        {
            var guildUser = (SocketGuildUser)user;

            var builder = new EmbedBuilder();

            builder.WithDescription($"" +
                $"**Tier**: {memberActivity.Tier}\n" +
                $"**Guild Activity:** {memberActivity.GA}\n" +
                $"**Inactivity notice:** {memberActivity.InactivityNotice}");

            var gearScoreFields = new List<EmbedFieldBuilder>{
                new EmbedFieldBuilder{ Name = "GQ", Value = memberActivity.GQ, IsInline = true },
                new EmbedFieldBuilder{ Name = "NW", Value = memberActivity.NW, IsInline = true },
                new EmbedFieldBuilder{ Name = "Villa", Value = memberActivity.Villa, IsInline = true },

                new EmbedFieldBuilder{ Name = "Militia", Value = memberActivity.Militia, IsInline = true },
                new EmbedFieldBuilder{ Name = "Seamonsters", Value = memberActivity.Seamonsters, IsInline = true },
                new EmbedFieldBuilder{ Name = "Placeholder", Value = "N/A", IsInline = true },
            };
            builder.Fields = gearScoreFields;

            var footer = new EmbedFooterBuilder();
            footer.WithText($"Last updated: {memberActivity.ActivityLastUpdated}");
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
