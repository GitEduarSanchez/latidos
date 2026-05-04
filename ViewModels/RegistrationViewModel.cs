using System.Windows.Input;
using Latidos.Models;
using Latidos.Services;

namespace Latidos.ViewModels;

public class RegistrationViewModel : BindableObject
{
    private readonly IEventService _eventService;
    private readonly ICartService _cartService;
    private RunningEvent? _event;

    public RunningEvent? Event
    {
        get => _event;
        set
        {
            if (_event != value)
            {
                _event = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(EventName));
                OnPropertyChanged(nameof(EventInfoText));
                OnPropertyChanged(nameof(EventPriceText));
                RegenerateCompetitorNumber();
            }
        }
    }

    public string EventName => Event?.Name ?? "Evento";
    public string EventInfoText => Event == null ? string.Empty : $"{Event.EventDate:dd MMM yyyy} - {Event.CityLocationText}";
    public string EventPriceText => Event == null ? string.Empty : $"${Event.Price:F2}";

    private string _fullName = string.Empty;
    public string FullName
    {
        get => _fullName;
        set
        {
            if (_fullName != value)
            {
                _fullName = value;
                OnPropertyChanged();
            }
        }
    }

    private string _selectedDocumentType = "Cedula";
    public string SelectedDocumentType
    {
        get => _selectedDocumentType;
        set
        {
            if (_selectedDocumentType != value)
            {
                _selectedDocumentType = value;
                OnPropertyChanged();
            }
        }
    }

    private string _documentNumber = string.Empty;
    public string DocumentNumber
    {
        get => _documentNumber;
        set
        {
            if (_documentNumber != value)
            {
                _documentNumber = value;
                OnPropertyChanged();
            }
        }
    }

    private DateTime _birthDate = DateTime.Today.AddYears(-18);
    public DateTime BirthDate
    {
        get => _birthDate;
        set
        {
            if (_birthDate != value)
            {
                _birthDate = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(AgeText));
            }
        }
    }

    private string _competitorNumber = string.Empty;
    public string CompetitorNumber
    {
        get => _competitorNumber;
        set
        {
            if (_competitorNumber != value)
            {
                _competitorNumber = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CompetitorNumberText));
            }
        }
    }

    private string _photoPath = string.Empty;
    public string PhotoPath
    {
        get => _photoPath;
        set
        {
            if (_photoPath != value)
            {
                _photoPath = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasPhoto));
            }
        }
    }

    private string _statusMessage = string.Empty;
    public string StatusMessage
    {
        get => _statusMessage;
        set
        {
            if (_statusMessage != value)
            {
                _statusMessage = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasStatusMessage));
            }
        }
    }

    public List<string> DocumentTypes { get; } = new()
    {
        "Cedula",
        "Tarjeta de identidad",
        "Pasaporte"
    };

    public bool HasPhoto => !string.IsNullOrWhiteSpace(PhotoPath);
    public bool HasStatusMessage => !string.IsNullOrWhiteSpace(StatusMessage);
    public string CompetitorNumberText => string.IsNullOrWhiteSpace(CompetitorNumber) ? "Pendiente" : CompetitorNumber;
    public string AgeText => $"{CalculateAge(BirthDate)} anos - {GetAgeCategory(BirthDate)}";

    public ICommand CapturePhotoCommand { get; }
    public ICommand PickPhotoCommand { get; }
    public ICommand AddToCartCommand { get; }
    public ICommand AddAnotherCommand { get; }
    public ICommand CancelCommand { get; }

    public RegistrationViewModel()
    {
        _eventService = IPlatformApplication.Current?.Services.GetService<IEventService>() ?? new EventService();
        _cartService = IPlatformApplication.Current?.Services.GetService<ICartService>() ?? new CartService();

        CapturePhotoCommand = new Command(async () => await CapturePhotoAsync());
        PickPhotoCommand = new Command(async () => await PickPhotoAsync());
        AddToCartCommand = new Command(async () => await AddRegistrationAsync(goToCart: true));
        AddAnotherCommand = new Command(async () => await AddRegistrationAsync(goToCart: false));
        CancelCommand = new Command(async () => await Shell.Current.GoToAsync("///events"));

        RegenerateCompetitorNumber();
    }

    public async Task LoadEventAsync(int eventId)
    {
        Event = await _eventService.GetEventByIdAsync(eventId);

        if (Event == null)
        {
            StatusMessage = "No se encontro el evento seleccionado.";
            return;
        }

        if (!Event.CanRegister)
        {
            StatusMessage = "Este evento no acepta nuevas inscripciones.";
        }
    }

    private async Task AddRegistrationAsync(bool goToCart)
    {
        StatusMessage = string.Empty;

        if (Event == null)
        {
            StatusMessage = "Selecciona un evento valido.";
            return;
        }

        if (!Event.CanRegister)
        {
            StatusMessage = "Este evento no acepta nuevas inscripciones.";
            return;
        }

        if (!TryBuildCompetitor(out var competitor, out var validationMessage))
        {
            StatusMessage = validationMessage;
            return;
        }

        var added = await _cartService.AddToCartAsync(new CartItem
        {
            EventId = Event.Id,
            Event = Event,
            Competitor = competitor,
            Quantity = 1,
            Price = Event.Price
        });

        if (!added)
        {
            StatusMessage = "No se pudo agregar la inscripcion al carrito.";
            return;
        }

        if (goToCart)
        {
            await Shell.Current.GoToAsync("///cart");
            return;
        }

        ClearCompetitorForm();
        StatusMessage = "Inscripcion agregada. Puedes registrar otro competidor para el mismo evento.";
    }

    private bool TryBuildCompetitor(out CompetitorProfile competitor, out string validationMessage)
    {
        competitor = new CompetitorProfile();
        validationMessage = string.Empty;

        if (string.IsNullOrWhiteSpace(FullName))
        {
            validationMessage = "Escribe el nombre completo del competidor.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(SelectedDocumentType))
        {
            validationMessage = "Selecciona el tipo de documento.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(DocumentNumber))
        {
            validationMessage = "Escribe el numero de documento.";
            return false;
        }

        if (BirthDate.Date > DateTime.Today)
        {
            validationMessage = "La fecha de nacimiento no puede estar en el futuro.";
            return false;
        }

        competitor = new CompetitorProfile
        {
            CompetitorNumber = CompetitorNumber,
            FullName = FullName.Trim(),
            DocumentType = SelectedDocumentType,
            DocumentNumber = DocumentNumber.Trim(),
            BirthDate = BirthDate.Date,
            PhotoPath = PhotoPath
        };

        return true;
    }

    private async Task CapturePhotoAsync()
    {
        try
        {
            if (!MediaPicker.Default.IsCaptureSupported)
            {
                StatusMessage = "La camara no esta disponible en este dispositivo.";
                return;
            }

            var photo = await MediaPicker.Default.CapturePhotoAsync();
            await SavePhotoAsync(photo);
        }
        catch (Exception ex)
        {
            StatusMessage = $"No se pudo capturar la foto: {ex.Message}";
        }
    }

    private async Task PickPhotoAsync()
    {
        try
        {
            var photo = await MediaPicker.Default.PickPhotoAsync();
            await SavePhotoAsync(photo);
        }
        catch (Exception ex)
        {
            StatusMessage = $"No se pudo seleccionar la foto: {ex.Message}";
        }
    }

    private async Task SavePhotoAsync(FileResult? photo)
    {
        if (photo == null)
        {
            return;
        }

        var extension = Path.GetExtension(photo.FileName);
        if (string.IsNullOrWhiteSpace(extension))
        {
            extension = ".jpg";
        }

        var fileName = $"competidor_{CompetitorNumber}_{DateTime.UtcNow:yyyyMMddHHmmss}{extension}";
        var targetPath = Path.Combine(FileSystem.AppDataDirectory, fileName);

        await using var sourceStream = await photo.OpenReadAsync();
        await using var targetStream = File.OpenWrite(targetPath);
        await sourceStream.CopyToAsync(targetStream);

        PhotoPath = targetPath;
        StatusMessage = "Foto agregada a la ficha del competidor.";
    }

    private void ClearCompetitorForm()
    {
        FullName = string.Empty;
        SelectedDocumentType = "Cedula";
        DocumentNumber = string.Empty;
        BirthDate = DateTime.Today.AddYears(-18);
        PhotoPath = string.Empty;
        RegenerateCompetitorNumber();
    }

    private void RegenerateCompetitorNumber()
    {
        var eventPrefix = Event?.Id ?? 0;
        var random = Random.Shared.Next(1000, 9999);
        CompetitorNumber = $"{eventPrefix:D2}{random}";
    }

    private static int CalculateAge(DateTime birthDate)
    {
        var today = DateTime.Today;
        var age = today.Year - birthDate.Year;

        if (birthDate.Date > today.AddYears(-age))
        {
            age--;
        }

        return Math.Max(age, 0);
    }

    private static string GetAgeCategory(DateTime birthDate)
    {
        var age = CalculateAge(birthDate);

        if (age <= 12)
        {
            return "Infantil";
        }

        if (age <= 17)
        {
            return "Juvenil";
        }

        if (age <= 39)
        {
            return "Adulto";
        }

        if (age <= 49)
        {
            return "Master A";
        }

        return "Master B";
    }
}
