using System.Text;
using Discord;
using Discord.Interactions;
using Oestus;

namespace Leinmachine.Modules; // All modules should be contained within this folder, and thus should be under the .Modules namespace!

public class General() : InteractionModuleBase // Modules with SlashCommands should be 1) Public for dependency injection and 2) Inherit InteractionModuleBase
{
    [SlashCommand("uptime", "Checks the uptime of the current shard")] // Example command. Shows how long the bot has been running in a hh:mm:ss format.
    public async Task Uptime()
    {
        await RespondAsync($"The current shard has been up for {(DateTime.Now - Leinmachine.startTime).ToString()}.", ephemeral: true);
    }

    [SlashCommand("ping", "Checks the ping to the current shard")] // Example command. Shows the difference between interaction creation time and handling time. 
    public async Task Ping()
    {
        await RespondAsync($"*Pong! {(DateTime.Now - Context.Interaction.CreatedAt).ToString("fff")}ms*", ephemeral: true);
    }

    public class RollAutocomplete : AutocompleteHandler
    {
        public override async Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context, IAutocompleteInteraction autocompleteInteraction, IParameterInfo parameter, IServiceProvider services)
        {
            List<AutocompleteResult> suggestions = new List<AutocompleteResult>() {
                new AutocompleteResult("d20", "1d20"),
                new AutocompleteResult("d12", "1d12"),
                new AutocompleteResult("d10", "1d10"),
                new AutocompleteResult("d8", "1d8"),
                new AutocompleteResult("d6", "1d6"),
                new AutocompleteResult("d4", "1d4")
            };

            return AutocompletionResult.FromSuccess(suggestions.Take(25));
        }
    }

    string ProcessQuery(string q) => q.Replace(" ", "").Replace("rt", "dm10").ToLower();

    public string Roll(string query, out string filepath)
    {
        query = ProcessQuery(query);
        filepath = "";
        int result = Dice.Parse(query, out var resultString);
        if (resultString.Length > 1900)
        {
            filepath = $"{Context.Interaction.Id}.txt";
            if (!File.Exists(filepath))
                File.WriteAllText(filepath, resultString);
            else
            {
                var writer = File.OpenWrite(filepath);
                writer.Write(Encoding.UTF8.GetBytes($"\n{resultString}"));
                writer.Close();
            }
        }
        return $"**{result}**! `{resultString}`";
    }

    [SlashCommand("roll", "Rolls Dice!")]
    public async Task RollDice([Autocomplete(typeof(RollAutocomplete))] string query)
    {
        var res = Roll(query, out string file);
        if (file != "")
            await RespondWithFileAsync(file, text: res);
        else await RespondAsync($"{Context.User.Mention} rolled `{query.Replace("dm10", "rt")}` for a total of {res}!");
    }

    public async Task FunctionNotImplemented()
    {
        await ReplyAsync("That function isn't implemented yet!");
    }
}