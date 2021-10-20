using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using ShellProgressBar;
using CommandLine;
namespace adf2mp3
{
    class Program
    {
        public class Options
        {
            [Option('d', "directory", Required = false, Default = false, HelpText = "Process all ADF files in a directory")]
            public bool dir { get; set; }

            [Option('i', "input", Required = true, HelpText = "What file or directory to load.")]
            public string source { get; set; }
            
            [Option('o', "output", Required = true, HelpText = "Where to put File(s).")]
            public string destination { get; set; }
        }

        static void Main(string[] args)
        {
            
            Parser.Default.ParseArguments<Options>(args)
                .WithParsed<Options>(async o =>
                {
                    var l = new Program();
                    if (o.dir)
                    { 
                        l.DoDirectory(o.source, o.destination).Wait();
                    }
                    else
                    {
                        l.DoSingleFile(o.source, o.destination, true).Wait();
                    }
                });
        }

        private async Task DoSingleFile(string input, string output,bool initPGB = false)
        {
            if(initPGB) pbar = new ProgressBar(1, "Converting", options);
            var Music = await File.ReadAllBytesAsync(input);
            await File.WriteAllBytesAsync(output, Conv(Music, output));
            pbar.Tick();
        }

        private async Task DoDirectory(string input, string output)
        {
            Console.WriteLine("Reading Directory...");
            List<string> toProcess = new List<string>();
            foreach (var fi in Directory.GetFiles(input))
            {
                if (fi.ToLower().Contains("adf"))
                {
                    toProcess.Add(fi);
                }
            }

            pbar = new ProgressBar(toProcess.Count, "Converting Music...", options);
            foreach(var fi in toProcess)
            {
                if (fi.ToLower().EndsWith("adf"))
                {
                    var fileName = Path.GetFileNameWithoutExtension(fi) + ".mp3";
                    var filePath = Path.Combine(output, fileName);
                    await DoSingleFile(fi, filePath);
                }
            }
        }
        ProgressBarOptions options = new ProgressBarOptions
        {
            ProgressCharacter = '─',
            ProgressBarOnBottom = true
        };

        private ProgressBarOptions childOptions = new ProgressBarOptions
        {
            ProgressCharacter = '─',
            ProgressBarOnBottom = true,
            ForegroundColor = ConsoleColor.DarkYellow,
            CollapseWhenFinished = true
        };
        //ProgressBar pbar = new ProgressBar(Decoded.Length, $"Converting {Name} from ADF to MP3", options)
        private ProgressBar pbar;
        private byte[] Conv(byte[] ADF, string Name)
        {
            
            byte[] Decoded = new byte[ADF.Length];
            int address = 0;
            var Child = pbar.Spawn(ADF.Length, Name, childOptions);
            foreach (var b in ADF)
            {
                Decoded[address]=(BitConverter.GetBytes(b ^ 34)[0]); 
                address++;
                Child.Tick();
            }
            return Decoded;
        }
    }
}