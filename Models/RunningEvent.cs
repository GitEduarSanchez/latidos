namespace Latidos.Models;

public class RunningEvent
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime EventDate { get; set; }
    public decimal Price { get; set; }
    public string PriceText => CurrencyFormatter.FormatCop(Price);
    public string City { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public int MaxParticipants { get; set; }
    public int CurrentParticipants { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public EventStatus Status { get; set; } = EventStatus.Active;

    public bool HasHappened => EventDate.Date < DateTime.Today;

    public bool IsCancelled => Status == EventStatus.Cancelled;

    public bool CanRegister => !IsCancelled && !HasHappened;

    public bool IsRegistrationClosed => !CanRegister;

    public int StartOrderGroup => HasHappened ? 1 : 0;

    public int SortPriority
    {
        get
        {
            if (CanRegister)
            {
                return 0;
            }

            return IsCancelled ? 1 : 2;
        }
    }

    public string RegistrationClosedText => IsCancelled ? "Inscripcion cancelada" : "Evento finalizado";

    public string StatusText
    {
        get
        {
            if (IsCancelled)
            {
                return "Cancelado";
            }

            return HasHappened ? "Finalizado" : "Activo";
        }
    }

    public string StatusBackgroundColor
    {
        get
        {
            if (IsCancelled)
            {
                return "#FDECEC";
            }

            return HasHappened ? "#EEEEEE" : "#EAF7EE";
        }
    }

    public string StatusTextColor
    {
        get
        {
            if (IsCancelled)
            {
                return "#B42318";
            }

            return HasHappened ? "#666666" : "#1F7A3A";
        }
    }

    public string CityLocationText
    {
        get
        {
            if (string.IsNullOrWhiteSpace(City))
            {
                return Location;
            }

            if (string.IsNullOrWhiteSpace(Location))
            {
                return City;
            }

            return $"{City} - {Location}";
        }
    }

    public double CapacityProgress
    {
        get
        {
            if (MaxParticipants <= 0)
            {
                return 0;
            }

            var ratio = (double)CurrentParticipants / MaxParticipants;
            return Math.Clamp(ratio, 0d, 1d);
        }
    }

    public int RemainingSpots => Math.Max(0, MaxParticipants - CurrentParticipants);

    public string RemainingSpotsText
    {
        get
        {
            if (RemainingSpots <= 0)
            {
                return "Cupo lleno";
            }

            return RemainingSpots == 1 ? "1 cupo" : $"{RemainingSpots} cupos";
        }
    }

    public bool IsAlmostFull => CapacityProgress >= 0.8 && RemainingSpots > 0;
}
