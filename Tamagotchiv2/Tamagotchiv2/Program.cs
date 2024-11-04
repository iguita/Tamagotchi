using System;
using System.IO;
using System.Threading;

namespace Tamagotchiv2
{
    internal class Program
    {
        static void Main(string[] args)
        {
            while (true)
            {
                ShowMainMenu();
                string choice = Console.ReadLine()?.ToLower();

                switch (choice)
                {
                    case "1": StartNewGame(); break;
                    case "2": LoadGame(); break;
                    case "3": RestartGame(); break;
                    case "4": return; // Sair do jogo
                    default:
                        Console.WriteLine("Opção inválida. Tente novamente.");
                        break;
                }
            }
        }

        static void ShowMainMenu()
        {
            Console.Clear();
            Console.WriteLine("===== Meu Bichinho Virtual =====");
            Console.WriteLine("1. Novo Jogo");
            Console.WriteLine("2. Carregar Jogo");
            Console.WriteLine("3. Reiniciar Jogo");
            Console.WriteLine("4. Sair");
            Console.Write("Escolha uma opção: ");
        }

        static void StartNewGame()
        {
            Console.Clear();
            Console.Write("Informe o nome do seu Bichinho: ");
            string petName = Console.ReadLine();
            Console.Write("Informe seu nome: ");
            string ownerName = Console.ReadLine();

            Tamagotchi pet = new Tamagotchi(petName);
            PlayGame(pet, ownerName);
        }

        static void LoadGame()
        {
            Console.Clear();
            Console.Write("Informe o nome do seu Bichinho para carregar: ");
            string petName = Console.ReadLine();
            Console.Write("Informe seu nome: ");
            string ownerName = Console.ReadLine();

            Tamagotchi pet = new Tamagotchi(petName);
            if (pet.LoadStatus(ownerName))
            {
                PlayGame(pet, ownerName);
            }
            else
            {
                Console.WriteLine("Jogo não encontrado. Verifique se o nome do bichinho e o seu nome estão corretos.");
                Console.WriteLine("Pressione qualquer tecla para voltar ao menu.");
                Console.ReadKey();
            }
        }

        static void RestartGame()
        {
            Console.Clear();
            Console.WriteLine("O jogo será reiniciado...");
            Console.ReadKey();
            StartNewGame();
        }

        static void PlayGame(Tamagotchi pet, string ownerName)
        {
            while (pet.IsAlive)
            {
                pet.ShowStatus();
                pet.ReduceRandomStatus();
                pet.ShowStatusWarnings();
                pet.DisplayRandomPhrase();

                Console.Write($"{ownerName}, o que vamos fazer hoje? (Brincar/Comer/Dormir/Banho/Salvar/Menu): ");
                string input = Console.ReadLine().ToLower();

                if (input == "menu")
                {
                    Console.WriteLine("Voltando ao menu principal...");
                    Thread.Sleep(1000);
                    return;
                }
                else if (input == "salvar")
                {
                    pet.SaveStatus(ownerName);
                    Console.WriteLine("Jogo salvo com sucesso!");
                    Thread.Sleep(1000);
                }
                else
                {
                    pet.Interact(input);
                    pet.ClampStatusValues();
                }
            }

            pet.ShowEndMessage();
        }
    }

    public class Tamagotchi
    {
        public string Name { get; private set; }
        private float Alimentado = 100;
        private float Limpo = 100;
        private float Feliz = 100;
        private float Energia = 100;

        private static readonly Random rand = new Random();
        private static readonly string[] Frases = {
            "Olá! Estou muito feliz de te ver! Como você está hoje?",
            "Ei, você pode me ajudar a encontrar algo para comer? Estou com fome!",
            "Vamos brincar? Que tal um joguinho para animar o dia?",
            "Estou me sentindo um pouco sujinho... Pode me dar um banho?",
            "Acho que preciso de uma sonequinha. Você cuida de mim enquanto eu descanso?",
            "Estou com um pouquinho de frio... Será que você pode me esquentar?"
        };

        public Tamagotchi(string name) => Name = name;

        public bool IsAlive => Alimentado > 0 && Limpo > 0 && Feliz > 0 && Energia > 0;

        public bool LoadStatus(string ownerName)
        {
            string file = GetFilePath(ownerName);
            if (File.Exists(file))
            {
                try
                {
                    string[] dados = File.ReadAllLines(file);
                    if (dados.Length == 5)
                    {
                        Alimentado = float.Parse(dados[1]);
                        Limpo = float.Parse(dados[2]);
                        Feliz = float.Parse(dados[3]);
                        Energia = float.Parse(dados[4]);
                        return IsAlive;
                    }
                    else
                    {
                        Console.WriteLine("O arquivo de status está corrompido ou no formato inesperado.");
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erro ao carregar o jogo: {ex.Message}");
                    return false;
                }
            }
            return false;
        }

        public void SaveStatus(string ownerName)
        {
            string fileContent = $"{Name}\n{Alimentado}\n{Limpo}\n{Feliz}\n{Energia}";
            File.WriteAllText(GetFilePath(ownerName), fileContent);
        }

        private string GetFilePath(string ownerName) => $"{Environment.CurrentDirectory}\\{Name}_{ownerName}.txt";

        public void ShowStatus()
        {
            Console.Clear();
            Console.WriteLine("Status do {0}:", Name);
            Console.WriteLine($"Alimentado: {Alimentado}");
            Console.WriteLine($"Limpo: {Limpo}");
            Console.WriteLine($"Feliz: {Feliz}");
            Console.WriteLine($"Energia: {Energia}");
        }

        public void ReduceRandomStatus()
        {
            switch (rand.Next(4))
            {
                case 0: Alimentado = Math.Max(0, Alimentado - rand.Next(10)); break;
                case 1: Limpo = Math.Max(0, Limpo - rand.Next(10)); break;
                case 2: Feliz = Math.Max(0, Feliz - rand.Next(10)); break;
                case 3: Energia = Math.Max(0, Energia - rand.Next(10)); break;
            }
        }

        public void ShowStatusWarnings()
        {
            if (Alimentado < 30) Console.WriteLine(Alimentado < 25 ? "Estou faminto!" : "Estou com fome.");
            if (Limpo < 30) Console.WriteLine(Limpo < 25 ? "Estou muito sujo!" : "Estou ficando sujo.");
            if (Feliz < 30) Console.WriteLine(Feliz < 25 ? "Estou triste!" : "Quer brincar comigo?");
            if (Energia < 30) Console.WriteLine(Energia < 25 ? "Estou exausto!" : "Estou ficando com sono.");
        }

        public void DisplayRandomPhrase()
        {
            Console.WriteLine(Frases[rand.Next(Frases.Length)]);
        }

        public void Interact(string input)
        {
            switch (input)
            {
                case "comer": Alimentado = Math.Min(100, Alimentado + rand.Next(30)); break;
                case "brincar": Feliz = Math.Min(100, Feliz + rand.Next(30)); break;
                case "dormir": Energia = Math.Min(100, Energia + rand.Next(30)); break;
                case "banho": Limpo = Math.Min(100, Limpo + rand.Next(30)); break;
                default: Console.WriteLine("Comando inválido. Tente novamente."); break;
            }
        }

        public void ClampStatusValues()
        {
            Alimentado = Math.Min(100, Math.Max(0, Alimentado));
            Limpo = Math.Min(100, Math.Max(0, Limpo));
            Feliz = Math.Min(100, Math.Max(0, Feliz));
            Energia = Math.Min(100, Math.Max(0, Energia));
        }

        public void ShowEndMessage()
        {
            if (!IsAlive)
            {
                Console.WriteLine("Seu bichinho morreu por falta de cuidados...");
            }
            else
            {
                Console.WriteLine("Obrigado por cuidar de mim! Até outro dia.");
            }
            Console.WriteLine("Pressione qualquer tecla para voltar ao menu.");
            Console.ReadKey();
        }
    }
}
