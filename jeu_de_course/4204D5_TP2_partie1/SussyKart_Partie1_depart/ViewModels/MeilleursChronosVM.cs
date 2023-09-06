namespace SussyKart_Partie1.ViewModels
{
    public class MeilleursChronosVM
    {

        public string Pseudo { get; set; } = null!;
        public string NomCourse { get; set; } = null!;
        public int Position { get; set; }
        public int Chrono { get; set; }
        public DateTime Date { get; set; }  
    }
}
