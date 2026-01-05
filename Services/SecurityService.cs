using MindfulJournal.Models;
using System.Security.Cryptography;
using System.Text;

namespace MindfulJournal.Services;

/// <summary>
/// Provides security functionality, including PIN protection and app locking.
/// </summary>
public class SecurityService
{
    private readonly DatabaseService _databaseService;
    private AppSettings? _cachedSettings;
    
    /// <summary>
    /// Occurs when the application lock state changes.
    /// </summary>
    public event Action<bool>? LockStateChanged;
    
    /// <summary>
    /// Gets a value indicating whether the application is currently locked.
    /// </summary>
    public bool IsLocked { get; private set; } = true;
    
    /// <summary>
    /// Gets a value indicating whether PIN protection is enabled.
    /// </summary>
    public bool IsLockEnabled => _cachedSettings?.IsLockEnabled ?? false;

    /// <summary>
    /// Initializes a new instance of the <see cref="SecurityService"/> class.
    /// </summary>
    /// <param name="databaseService">The database service instance.</param>
    public SecurityService(DatabaseService databaseService)
    {
        _databaseService = databaseService;
    }

    /// <summary>
    /// Initializes the security service, ensuring the settings table exists and loading current settings.
    /// </summary>
    public async Task InitializeAsync()
    {
        await _databaseService.Database.CreateTableAsync<AppSettings>();
        await LoadSettingsAsync();
        
        // Set initial lock state based on settings
        IsLocked = _cachedSettings?.IsLockEnabled ?? false;
    }

    /// <summary>
    /// Loads the application settings from the database.
    /// </summary>
    private async Task LoadSettingsAsync()
    {
        _cachedSettings = await _databaseService.Database
            .Table<AppSettings>()
            .FirstOrDefaultAsync();
        
        if (_cachedSettings == null)
        {
            _cachedSettings = new AppSettings { Id = 1 };
            await _databaseService.Database.InsertAsync(_cachedSettings);
        }
    }

    /// <summary>
    /// Sets up a new PIN for the application.
    /// </summary>
    /// <param name="pin">The new PIN to set.</param>
    /// <returns>True if the PIN was successfully set; otherwise, false.</returns>
    public async Task<bool> SetupPinAsync(string pin)
    {
        if (string.IsNullOrWhiteSpace(pin) || pin.Length < 4)
            return false;

        var hash = HashPin(pin);
        
        if (_cachedSettings == null)
            await LoadSettingsAsync();
        
        _cachedSettings!.PinHash = hash;
        _cachedSettings.IsLockEnabled = true;
        
        await _databaseService.Database.UpdateAsync(_cachedSettings);
        
        IsLocked = false; // Unlock after setting up PIN
        LockStateChanged?.Invoke(false);
        
        return true;
    }

    /// <summary>
    /// Verifies if the provided PIN matches the stored PIN.
    /// </summary>
    /// <param name="pin">The PIN to verify.</param>
    /// <returns>True if the PIN is correct; otherwise, false.</returns>
    public bool VerifyPin(string pin)
    {
        if (_cachedSettings == null || string.IsNullOrWhiteSpace(_cachedSettings.PinHash))
            return false;

        var hash = HashPin(pin);
        return hash == _cachedSettings.PinHash;
    }

    /// <summary>
    /// Attempts to unlock the application with the provided PIN.
    /// </summary>
    /// <param name="pin">The PIN to try.</param>
    /// <returns>True if the application was unlocked; otherwise, false.</returns>
    public bool TryUnlock(string pin)
    {
        if (VerifyPin(pin))
        {
            IsLocked = false;
            LockStateChanged?.Invoke(false);
            return true;
        }
        return false;
    }

    /// <summary>
    /// Locks the application immediately.
    /// </summary>
    public async Task LockAsync()
    {
        if (_cachedSettings?.IsLockEnabled == true)
        {
            IsLocked = true;
            _cachedSettings.LastLockTime = DateTime.Now;
            await _databaseService.Database.UpdateAsync(_cachedSettings);
            LockStateChanged?.Invoke(true);
        }
    }

    /// <summary>
    /// Disables PIN protection.
    /// </summary>
    /// <param name="currentPin">The current PIN for verification.</param>
    /// <returns>True if the lock was disabled; otherwise, false.</returns>
    public async Task<bool> DisableLockAsync(string currentPin)
    {
        if (!VerifyPin(currentPin))
            return false;

        if (_cachedSettings == null)
            await LoadSettingsAsync();
        
        _cachedSettings!.IsLockEnabled = false;
        _cachedSettings.PinHash = string.Empty;
        
        await _databaseService.Database.UpdateAsync(_cachedSettings);
        
        IsLocked = false;
        LockStateChanged?.Invoke(false);
        
        return true;
    }

    /// <summary>
    /// Changes the current PIN to a new one.
    /// </summary>
    /// <param name="currentPin">The current PIN.</param>
    /// <param name="newPin">The new PIN.</param>
    /// <returns>True if the PIN was changed; otherwise, false.</returns>
    public async Task<bool> ChangePinAsync(string currentPin, string newPin)
    {
        if (!VerifyPin(currentPin))
            return false;

        if (string.IsNullOrWhiteSpace(newPin) || newPin.Length < 4)
            return false;

        _cachedSettings!.PinHash = HashPin(newPin);
        await _databaseService.Database.UpdateAsync(_cachedSettings);
        
        return true;
    }

    /// <summary>
    /// Checks if the application should be automatically locked based on the configured timeout.
    /// </summary>
    /// <returns>True if the app should auto-lock; otherwise, false.</returns>
    public bool ShouldAutoLock()
    {
        if (_cachedSettings?.IsLockEnabled != true)
            return false;
        
        if (_cachedSettings.AutoLockTimeoutMinutes < 0)
            return false; // Never auto-lock
        
        if (_cachedSettings.AutoLockTimeoutMinutes == 0)
            return true; // Always lock immediately
        
        if (_cachedSettings.LastLockTime == null)
            return true;
        
        var elapsed = DateTime.Now - _cachedSettings.LastLockTime.Value;
        return elapsed.TotalMinutes >= _cachedSettings.AutoLockTimeoutMinutes;
    }

    /// <summary>
    /// Hashes the provided PIN using SHA256 and a fixed salt.
    /// </summary>
    /// <param name="pin">The PIN to hash.</param>
    /// <returns>The hashed PIN string.</returns>
    private static string HashPin(string pin)
    {
        using var sha256 = SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(pin + "MindfulJournalSalt2024"));
        return Convert.ToBase64String(bytes);
    }
}
