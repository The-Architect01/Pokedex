using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using System.Runtime.InteropServices;

using Pokedex;

using PokeAPI;

namespace Summer_Pokemon_Game_C.Pkmn.Pokedex {

    public partial class PokedexData : Form {

        int PkmnNumber;
        readonly Random Rand = new();

        #region Graphic Imports
        [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn(
            int LeftRect,
            int TopRect,
            int RightRect,
            int BottomRect,
            int Width,
            int Height
            );
        #endregion

        #region Main Type Colors
        static readonly Color DEFAULT = Color.FromArgb(100,Color.White);
        static readonly Color CONTRAST_STEEL = Color.FromArgb(100,Color.LightSlateGray);
        static readonly Color CONTRAST_ELECTRIC = Color.FromArgb(100, Color.DarkKhaki);

        static readonly Color NORMAL = Color.Silver;
        static readonly Color FIRE = Color.Salmon;
        static readonly Color WATER = Color.Turquoise;
        static readonly Color GRASS = Color.LightGreen;
        static readonly Color ELECTRIC = Color.Khaki;
        static readonly Color ICE = Color.PowderBlue;
        static readonly Color FIGHTING = Color.DarkRed;
        static readonly Color POISON = Color.GreenYellow;
        static readonly Color GROUND = Color.Peru;
        static readonly Color FLYING = Color.SkyBlue;
        static readonly Color PSYCHIC = Color.Plum;
        static readonly Color BUG = Color.YellowGreen;
        static readonly Color ROCK = Color.SandyBrown;
        static readonly Color GHOST = Color.DarkSlateBlue;
        static readonly Color DARK = Color.DarkSlateGray;
        static readonly Color DRAGON = Color.IndianRed;
        static readonly Color STEEL = Color.Gainsboro;
        static readonly Color FAIRY = Color.LightPink;
        #endregion

        readonly Thread GetData;

        void GetPkmnDataAsync() {

            string numbervalue = PkmnNumber < 10 ? "00" + PkmnNumber : PkmnNumber < 100 ? "0" + PkmnNumber : PkmnNumber.ToString();

            PokeAPI.Pokemon pkmn = DataFetcher.GetApiObject<PokeAPI.Pokemon>(int.Parse(numbervalue)).Result;
            PokemonSpecies pkmnSpecies = DataFetcher.GetNamedApiObject<PokemonSpecies>(pkmn.Species.Name).Result;

            Text = pkmnSpecies.Name.ToUpper().Substring(0, 1) + pkmnSpecies.Name[1..].ToLower();
            Text = Text.Replace("-", " ");
            PkmnName.Text = Text;
            Genus.Text = "The " + pkmnSpecies.Genera[7].Name;
            label2.Text = "#" + numbervalue;

            PkmnWeight.Text = Math.Round((pkmn.Mass / 4.536d), 1) + " lbs.";
            int height = (int)Math.Round(pkmn.Height * 3.937d);
            PkmnHeight.Text = (height / 12) + "' ";
            PkmnHeight.Text += (int)(height % 12) + "\"";

            PkmnAbilities.Text = "";
            foreach (PokemonAbility ability in pkmn.Abilities) {
                PkmnAbilities.Text += ability.Ability.Name.Replace("-", " ").ToUpper() + "\n";
            }

            PkmnEggGroups.Text = "";
            foreach(NamedApiResource<EggGroup> eg in pkmnSpecies.EggGroups) {
                PkmnEggGroups.Text += eg.Name.Replace("-", " ").ToUpper() + "\n";
            }

            if(PkmnEggGroups.Text.SequenceEqual("")) { PkmnEggGroups.Text = "NO DATA"; }

            Type1.Image = (Image)UIElements.ResourceManager.GetObject("Type" + pkmn.Types[0].Type.Name.ToUpper().Substring(0,1) + pkmn.Types[0].Type.Name[1..]);
            
            SetBackgroundColor(pkmn.Types[0].Type.Name.ToUpper().Substring(0, 1) + pkmn.Types[0].Type.Name[1..]);
            
            try {
                Type2.Image = (Image)UIElements.ResourceManager.GetObject("Type" + pkmn.Types[1].Type.Name.ToUpper().Substring(0, 1) + pkmn.Types[1].Type.Name[1..]);
            } catch (IndexOutOfRangeException) {
                Type2.Image = null;
            }


            Info.Text = GetFlavorText(pkmnSpecies.FlavorTexts);
            GetStats(pkmn);

            CheckEvolution(pkmnSpecies.EvolutionChain.ID);
        }

        void GetStats(PokeAPI.Pokemon species) {
            HPValue.Text = species.Stats[0].BaseValue.ToString();
            HPStats.Width = (int)((int.Parse(HPValue.Text) / 255d) * 229);

            AttackValue.Text = species.Stats[1].BaseValue.ToString();
            AttackStat.Width = (int)((int.Parse(AttackValue.Text) / 255d) * 229);

            DefenseValue.Text = species.Stats[2].BaseValue.ToString();
            DefenseStat.Width = (int)((int.Parse(DefenseValue.Text) / 255d) * 229);

            SpAttackValue.Text = species.Stats[3].BaseValue.ToString();
            SpAttackStat.Width = (int)((int.Parse(SpAttackValue.Text) / 255d) * 229);

            SpDefenseValue.Text = species.Stats[4].BaseValue.ToString();
            SpDefenseStat.Width = (int)((int.Parse(SpDefenseValue.Text) / 255d) * 229);

            SpeedValue.Text = species.Stats[5].BaseValue.ToString();
            SpeedStat.Width = (int)((int.Parse(SpeedValue.Text) / 255d) * 229);

            foreach (PictureBox pb in statvalues) {
                pb.Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0, 0, pb.Width, pb.Height, 5, 5));
                double TopPercent = (pb.Width / 229d) * 100;
                if (TopPercent > 50) {
                    pb.BackColor = Color.FromArgb(255, 75, 255, 125);
                } else if (TopPercent > 10) {
                    pb.BackColor = Color.FromArgb(255, 250, 247, 124);
                } else {
                    pb.BackColor = Color.FromArgb(255, 255, 166, 165);
                }
            }

        }

        readonly PictureBox[] statvalues;

        void CheckEvolution(int pkmnEvolutionKey) {
            EvolutionLine el = new(pkmnEvolutionKey);
            int counter = 0;

            foreach (Control c in evolutionSnips) { c.Visible = false; }

            foreach (int num in el.Stages.Keys.OrderBy(x=>x)) {

                Label Stage = (Label)Controls.Find("Stage" + (counter + 1), true)[0];
                PictureBox StageImage = (PictureBox) Controls.Find("Stage" + (counter + 1) + "Image", true)[0];
                Label StageNumber = (Label)Controls.Find("Stage" + (counter + 1) + "Number", true)[0];
       
                string pkmnNumber = num < 10 ? "00" + num : num < 100 ? "0" + num: num.ToString();
                
                string webaddress = "https://assets.pokemon.com/assets/cms2/img/pokedex/full/" + pkmnNumber + ".png";

                Stage.Text = el.Stages[num].ToUpper().Substring(0, 1) + el.Stages[num][1..];
                StageImage.Load(webaddress);                            
                StageNumber.Text = "#"+ pkmnNumber;

                Stage.Visible = true;
                StageImage.Visible = true;
                StageNumber.Visible = true;

                counter++;
            }

            DoesNotEvolve.Visible = counter == 1;
        }

       static string GetFlavorText(PokemonSpeciesFlavorText[] values) {
            List<string> texts = new();
            foreach(PokemonSpeciesFlavorText value in values) {
                if (value.Language.Name != "en") { continue; }
                texts.Add(value.FlavorText.Replace("\n", " ").Replace("\f", " "));
            }
            return texts[0];
        }
        
        readonly Control[] evolutionSnips;

        public PokedexData(int number) {
            PkmnNumber = number;

            InitializeComponent();

            statvalues = new PictureBox[] { HPStats, AttackStat, DefenseStat,
            SpAttackStat, SpDefenseStat, SpeedStat};

            evolutionSnips = new Control[] {
                Stage1,Stage1Number,Stage1Image,
                Stage2,Stage2Number,Stage2Image,
                Stage3,Stage3Number,Stage3Image,
                Stage4,Stage4Number,Stage4Image,
                Stage5,Stage5Number,Stage5Image,
                Stage6,Stage6Number,Stage6Image,
                Stage7,Stage7Number,Stage7Image,
                Stage8,Stage8Number,Stage8Image,
                Stage9,Stage9Number,Stage9Image
            };

            CheckForIllegalCrossThreadCalls = false;
            GetData = new Thread(new ThreadStart(GetPkmnDataAsync)) { IsBackground = true };
            GetData.Start();

            panel1.BackColor = DEFAULT;
            panel1.Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0, 0, panel1.Width, panel1.Height, 50, 50));

            panel2.BackColor = DEFAULT;
            panel2.Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0, 0, panel2.Width, panel2.Height, 50, 50));

            panel3.BackColor = DEFAULT;
            panel3.Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0, 0, panel3.Width, panel3.Height, 50, 50));
        }

        private void PokedexV2_Load(object sender, EventArgs e) {
            LoadImages();
        }

        void LoadImages() {
            string numbervalue = PkmnNumber < 10 ? "00" + PkmnNumber : PkmnNumber < 100 ? "0" + PkmnNumber : PkmnNumber.ToString();
            string webaddress = "https://assets.pokemon.com/assets/cms2/img/pokedex/full/" + numbervalue + ".png";
            pictureBox1.Load(webaddress);
        }

        void Reset() {
            Info.Text = "Loading...";
            PkmnHeight.Text = "Loading...";
            PkmnWeight.Text = "Loading...";
            PkmnEggGroups.Text = "Loading...";
            PkmnAbilities.Text = "Loading...";
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData) {
            switch (keyData) {
                case Keys.Up: case Keys.W: case Keys.Left: case Keys.A:
                    if (keysLocked) { break; }
                    keysLocked = true;
                    Reset();
                    PkmnNumber = PkmnNumber - 1 == 0 ? 898 : PkmnNumber - 1;
                    LoadImages();
                    Thread UP = new(new ThreadStart(GetPkmnDataAsync)) { IsBackground = true };
                    UP.Start();
                    break;
                case Keys.Right: case Keys.D: case Keys.Down: case Keys.S:
                    if (keysLocked) { break; }
                    Reset();
                    keysLocked = true;
                    PkmnNumber = PkmnNumber + 1 == 899 ? 1 : PkmnNumber + 1;
                    LoadImages();
                    Thread DOWN = new(new ThreadStart(GetPkmnDataAsync)) { IsBackground = true };
                    DOWN.Start();
                    break;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        bool keysLocked = false;

        private void Timer1_Tick(object sender, EventArgs e) { keysLocked = false; }

        private void Stage_Click(object sender, EventArgs e) {
            string SenderName = ((Control)sender).Name;
            string Stage = "Stage" + SenderName.Split("Stage")[1][0]+"Number";
            PkmnNumber = int.Parse(Controls.Find(Stage, true)[0].Text.Split("#")[1]);
            Reset();
            LoadImages();
            Thread LoadNewPkmn = new(new ThreadStart(GetPkmnDataAsync)) { IsBackground = true };
            LoadNewPkmn.Start();
        }

        private void SetBackgroundColor(string Type) {
            if(Type == "Electric") {
                panel1.BackColor = CONTRAST_ELECTRIC;
                panel2.BackColor = CONTRAST_ELECTRIC;
                panel3.BackColor = CONTRAST_ELECTRIC;
            } else if(Type == "Steel"){
                panel1.BackColor = CONTRAST_STEEL;
                panel2.BackColor = CONTRAST_STEEL;
                panel3.BackColor = CONTRAST_STEEL;
            } else {
                panel1.BackColor = DEFAULT;
                panel2.BackColor = DEFAULT;
                panel3.BackColor = DEFAULT;
            }
            switch (Type) {
                case "Bug":
                    BackColor = BUG;
                    break;
                case "Dark":
                    BackColor = DARK;
                    break;
                case "Dragon":
                    BackColor = DRAGON;
                    break;
                case "Electric":
                    BackColor = ELECTRIC;
                    break;
                case "Fairy":
                    BackColor = FAIRY;
                    break;
                case "Fighting":
                    BackColor = FIGHTING;
                    break;
                case "Fire":
                    BackColor = FIRE;
                    break;
                case "Flying":
                    BackColor = FLYING;
                    break;
                case "Ghost":
                    BackColor = GHOST;
                    break;
                case "Grass":
                    BackColor = GRASS;
                    break;
                case "Ground":
                    BackColor = GROUND;
                    break;
                case "Ice":
                    BackColor = ICE;
                    break;
                case "Normal":
                    BackColor = NORMAL;
                    break;
                case "Poison":
                    BackColor = POISON;
                    break;
                case "Psychic":
                    BackColor = PSYCHIC;
                    break;
                case "Rock":
                    BackColor = ROCK;
                    break;
                case "Steel":
                    BackColor = STEEL;
                    break;
                case "Water":
                    BackColor = WATER;
                    break;
            }
        }
    
    }
}
