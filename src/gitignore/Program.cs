using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Mono.Options;
using Octokit;

namespace gitignore
{
    public class Program
    {
        private const string Dotgitignore = ".gitignore";
        private const string AppName = "gitignore-app";
        private const string Owner = "github";
        private const string Repository = "gitignore";

        private int _verbosity;
        private string _name;
        private bool _force;

        private async Task<IReadOnlyList<RepositoryContent>> GetGitHubContent()
        {
            var github = new GitHubClient(new ProductHeaderValue(AppName));
            var contents = await github.Repository.Content.GetAllContents(Owner, Repository);
            var contentsGlobal = await github.Repository.Content.GetAllContents(Owner, Repository, "/Global/");

            var result = new List<RepositoryContent>(contents);
            result.AddRange(contentsGlobal);
            return result;
        }

        private async Task<int> RunCreateAsync()
        {
            if (string.IsNullOrEmpty(_name))
            {
                Console.Error.WriteLine("Missing template name -n option."); return 1;
            }

            if (File.Exists(Dotgitignore) && !_force)
            {
                Console.Error.WriteLine(".gitignore already exsist. To overwrite, add -f."); return 1;
            }

            var contents = await GetGitHubContent();

            var url = contents.Where(content => content.Path.Equals(_name+Dotgitignore, StringComparison.OrdinalIgnoreCase))
                .Select(content => content.DownloadUrl).FirstOrDefault();

            if (url == null)
            {
                Console.Error.WriteLine("template name -n '{0}' dose not exsists.", _name);
                return 1;
            }

            using (var file = File.Create(Dotgitignore))
            {
                using (var client = new HttpClient())
                {
                    var msg = await client.GetAsync(url);
                    await msg.Content.CopyToAsync(file);
                }
            }

            return 1;
        }

        private async Task RunListAsync()
        {
            var contents = await GetGitHubContent();

            foreach (var content in contents.Where(content => content.Path.EndsWith(Dotgitignore)))
            {
                if (_verbosity > 0)
                {
                    Console.WriteLine("{0}, {1}, {2}", content.Path.Replace(Dotgitignore, ""), content.Path, content.DownloadUrl);
                }
                else
                {
                    Console.WriteLine("{0}", content.Path.Replace(Dotgitignore, ""));
                }
            }
        }

        private int Run(string[] args)
        {
            var commands = new CommandSet("commands")
            {
                "usage: gitignore COMMAND [OPTIONS]",
                "",
                "create .gitignore file from GitHub's github/gitignore repository app.",
                "",
                "Global options:",
                {
                    "v:",
                    "Output verbosity.",
                    (int? n) => _verbosity = n ?? _verbosity + 1
                },
                "",
                "Available commands:",
                new Command("list", "list available template names.")
                {
                    Run = ca => RunListAsync().Wait()
                },
                new Command("new", "create .gitignore from given name.")
                {
                    Options = new OptionSet()
                    {
                        {
                            "name|n=",
                            "{name} of .gitignore template.",
                            n =>
                            {
                                if(string.IsNullOrEmpty(n))
                                    throw new OptionException("Missing template name for option -n.", "-n");
                                _name = n;
                            }
                        },
                        {
                            "force|f",
                            "force exisiting .gitignore file.",
                            n => _force = n != null
                        }

                    },
                    Run = ca => RunCreateAsync().Wait()
                }
            };
            return commands.Run (args);
        }

        private static int Main(string[] args)
        {
            var exitCode = 1;
            try
            {
                exitCode = new Program().Run(args);
            }
            catch (OptionException e)
            {
                Console.Write ("error: ");
                Console.WriteLine (e.Message);
                Console.WriteLine ("Try `gitignore --help' for more information.");
            }

            return exitCode;
        }
    }
}
