using Microsoft.JSInterop;

namespace Web.Services;

/// <summary>
/// Interface for managing application theme state and persistence
/// </summary>
public interface IThemeService
{
	/// <summary>
	/// Gets the current theme ('light' or 'dark')
	/// </summary>
	string CurrentTheme { get; }

	/// <summary>
	/// Event triggered when the theme changes
	/// </summary>
	event Action? OnThemeChanged;

	/// <summary>
	/// Toggles between light and dark theme
	/// </summary>
	Task ToggleThemeAsync();

	/// <summary>
	/// Initializes the theme from localStorage or system preference
	/// </summary>
	Task InitializeAsync();
}

/// <summary>
/// Service for managing light/dark theme state with localStorage persistence
/// </summary>
public class ThemeService : IThemeService
{
	private readonly IJSRuntime _jsRuntime;
	private string _currentTheme = "dark"; // Default to dark mode

	public ThemeService(IJSRuntime jsRuntime)
	{
		_jsRuntime = jsRuntime;
	}

	public string CurrentTheme => _currentTheme;

	public event Action? OnThemeChanged;

	/// <summary>
	/// Initializes theme from localStorage, falls back to 'dark' if not set
	/// </summary>
	public async Task InitializeAsync()
	{
		try
		{
			_currentTheme = await _jsRuntime.InvokeAsync<string>("themeHelper.get");
			
			// Ensure valid theme value
			if (_currentTheme != "light" && _currentTheme != "dark")
			{
				_currentTheme = "dark";
			}

			await ApplyThemeAsync();
		}
		catch (Exception)
		{
			// If JavaScript interop fails (e.g., prerendering), use default
			_currentTheme = "dark";
		}
	}

	/// <summary>
	/// Toggles between light and dark theme
	/// </summary>
	public async Task ToggleThemeAsync()
	{
		_currentTheme = _currentTheme == "dark" ? "light" : "dark";
		
		await ApplyThemeAsync();
		
		OnThemeChanged?.Invoke();
	}

	/// <summary>
	/// Applies the current theme by updating localStorage and DOM
	/// </summary>
	private async Task ApplyThemeAsync()
	{
		try
		{
			await _jsRuntime.InvokeVoidAsync("themeHelper.set", _currentTheme);
			await _jsRuntime.InvokeVoidAsync("themeHelper.applyTheme", _currentTheme);
		}
		catch (Exception)
		{
			// Silently fail if JavaScript interop is not available
		}
	}
}
