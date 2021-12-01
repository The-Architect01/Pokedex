using System;
using System.Windows.Forms;

using Summer_Pokemon_Game_C.Pkmn.Pokedex;

namespace Pokedex {
    static class Program {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main() {
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new PokedexData(1));
        }
    }
}
