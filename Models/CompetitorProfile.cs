namespace Latidos.Models;

public class CompetitorProfile
{
    public string CompetitorNumber { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string DocumentType { get; set; } = string.Empty;
    public string DocumentNumber { get; set; } = string.Empty;
    public DateTime BirthDate { get; set; }
    public string PhotoPath { get; set; } = string.Empty;

    public int Age
    {
        get
        {
            var today = DateTime.Today;
            var age = today.Year - BirthDate.Year;

            if (BirthDate.Date > today.AddYears(-age))
            {
                age--;
            }

            return Math.Max(age, 0);
        }
    }

    public string AgeCategory
    {
        get
        {
            if (Age <= 12)
            {
                return "Infantil";
            }

            if (Age <= 17)
            {
                return "Juvenil";
            }

            if (Age <= 39)
            {
                return "Adulto";
            }

            if (Age <= 49)
            {
                return "Master A";
            }

            return "Master B";
        }
    }

    public bool HasPhoto => !string.IsNullOrWhiteSpace(PhotoPath);
}
