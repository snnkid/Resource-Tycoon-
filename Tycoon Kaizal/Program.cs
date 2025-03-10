using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace ResourceTycoon
{
    class Program
    {
        static void Main()
        {
            Console.CursorVisible = false;
            Console.Title = "Neon Tycoon";

            Game game = new Game();
            game.Run();
        }
    }

    public class Game
    {
        private Player player = new Player();
        private DateTime lastUpdate = DateTime.Now;
        private bool isRunning = true;
        private string statusMessage = "";
        private int messageTimer = 0;
        private readonly string[] upgradeArt = { "▲▲▲", "⧈⧈⧈", "⚡⚡⚡" };

        public Game()
        {
            player.Upgrades.Add(new Upgrade("MINERS", 20, 1f, 0.5f));
            player.Upgrades.Add(new Upgrade("FACTORIES", 150, 5f, 1f));
            player.Upgrades.Add(new Upgrade("REACTORS", 1000, 25f, 2f));
        }

        public void Run()
        {
            while (isRunning)
            {
                Update();
                Render();
                Thread.Sleep(100); // 10 FPS
            }
        }

        private void Update()
        {
            HandleInput();
            UpdateResources();

            if (messageTimer > 0) messageTimer--;
        }

        private void HandleInput()
        {
            if (!Console.KeyAvailable) return;

            ConsoleKey key = Console.ReadKey(true).Key;

            if (key == ConsoleKey.Q) isRunning = false;

            if (key == ConsoleKey.Enter)
            {
                player.Resources += player.ClickPower;
                statusMessage = $"▲ +{player.ClickPower:N2}";
                messageTimer = 10;
            }

            if (key >= ConsoleKey.D1 && key <= ConsoleKey.D3)
            {
                int index = key - ConsoleKey.D1;
                if (player.BuyUpgrade(index))
                {
                    statusMessage = $"⚡ {player.Upgrades[index].Name} UPGRADED!";
                    messageTimer = 10;
                }
                else
                {
                    statusMessage = "⚠ NOT ENOUGH RESOURCES";
                    messageTimer = 10;
                }
            }
        }

        private void UpdateResources()
        {
            TimeSpan deltaTime = DateTime.Now - lastUpdate;
            lastUpdate = DateTime.Now;

            player.Resources += player.GetPassiveProduction()
                * (float)deltaTime.TotalSeconds;
        }

        private void Render()
        {
            var sb = new StringBuilder();

            // Header
            sb.AppendLine("░▒▓ RESOURCE TYCOON ▓▒░".PadLeft(45));
            sb.AppendLine("▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀");

            // Resources
            sb.AppendLine($"  -> RESOURCES: {player.Resources,10:N2} ▸ CLICK POWER: {player.ClickPower,6:N2}  ");
            sb.AppendLine($"  -> PASSIVE INCOME: {player.GetPassiveProduction(),6:N2}/SEC  ");
            sb.AppendLine("▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀");

            // Upgrades
            for (int i = 0; i < player.Upgrades.Count; i++)
            {
                var u = player.Upgrades[i];
                sb.AppendLine($"  {i + 1}  {upgradeArt[i]} {u.Name,-10} " +
                             $"LVL {u.Level,-2}  COST {u.CurrentCost,8:N0}  " +
                             $"PROD {u.GetProduction(),6:N2}/S  ");
            }

            // Status
            sb.AppendLine("▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀");
            sb.AppendLine($"  {statusMessage,-50}  ");
            sb.AppendLine("  -> ENTER - MINE  -> 1/2/3 - BUY  -> Q - QUIT  ");
            sb.AppendLine("▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀▄▀");

            // Neon rendering
            Console.SetCursorPosition(0, 0);
            Console.BackgroundColor = ConsoleColor.Black;

            for (int i = 0; i < sb.Length; i++)
            {
                // Header gradient
                if (i < 80)
                {
                    Console.ForegroundColor = (ConsoleColor)(i % 5 + 9);
                }
                // Resource numbers
                else if (sb[i] == ':')
                {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                }
                // Upgrade art
                else if (i > sb.ToString().IndexOf("UPGRADES") &&
                         (sb[i] == '▲' || sb[i] == '*' || sb[i] == '$'))
                {
                    Console.ForegroundColor = ConsoleColor.Magenta;
                }
                // Status messages
                else if (i > sb.ToString().IndexOf("STATUS"))
                {
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                }
                // Default
                else
                {
                    Console.ForegroundColor = ConsoleColor.Gray;
                }

                Console.Write(sb[i]);
            }
        }
    }

    public class Player
    {
        public float Resources { get; set; } = 10f;
        public float ClickPower { get; set; } = 1f;
        public List<Upgrade> Upgrades { get; set; } = new List<Upgrade>();

        public float GetPassiveProduction()
        {
            float total = 0;
            foreach (var upgrade in Upgrades)
                total += upgrade.GetProduction();
            return total;
        }

        public bool BuyUpgrade(int index)
        {
            if (index < 0 || index >= Upgrades.Count) return false;

            var upgrade = Upgrades[index];

            if (Resources >= upgrade.CurrentCost)
            {
                Resources -= upgrade.CurrentCost;
                upgrade.Level++;
                ClickPower += upgrade.Bonus;
                return true;
            }
            return false;
        }
    }

    public class Upgrade
    {
        public string Name { get; set; }
        public float BaseCost { get; set; }
        public float BaseProduction { get; set; }
        public float Bonus { get; set; }
        public int Level { get; set; } = 0;

        public float CurrentCost => BaseCost * (float)Math.Pow(1.5, Level);
        public float GetProduction() => BaseProduction * Level;

        public Upgrade(string name, float baseCost, float baseProduction, float bonus)
        {
            Name = name;
            BaseCost = baseCost;
            BaseProduction = baseProduction;
            Bonus = bonus;
        }
    }
}