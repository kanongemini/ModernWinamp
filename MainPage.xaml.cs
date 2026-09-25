using Microsoft.Maui.Controls.Shapes;
using Plugin.Maui.Audio;

namespace ModernWinamp;

public partial class MainPage : ContentPage
{
	private readonly IAudioManager _audioManager;
	private readonly List<(string Title, string Artist, string Album, string Genre, string Duration, Stream? Stream)> _playlist =
	[
		("Midnight Drive", "Neon Avenue", "Neon Nights", "Synthwave", "04:18", null),
		("Electric Bloom", "Chromatic State", "Chromatic Skies", "Synthwave", "03:52", null),
		("Afterglow", "Lunar Echo", "After Hours", "Synthwave", "05:06", null),
		("Signal Lost", "Night Circuit", "Static Dreams", "Synthwave", "03:44", null)
	];

	private IAudioPlayer? _audioPlayer;
	private int _currentTrackIndex;
	private IDispatcherTimer? _playbackTimer;
	private TimeSpan _elapsedTime;
	private double _volume = 1.0;

	public MainPage(IAudioManager audioManager)
	{
		_audioManager = audioManager;
		InitializeComponent();
		ConfigurePlaybackTimer();
		RenderCurrentTrack();
		RenderPlaylist();
	}

	private async void OnPlayClicked(object? sender, EventArgs e)
	{
		try
		{
			var stream = _playlist[_currentTrackIndex].Stream;
			if (stream is null)
			{
				await DisplayAlertAsync("Audio", "Importe um arquivo MP3 para reproduzir.", "OK");
				return;
			}

			_audioPlayer ??= _audioManager.CreatePlayer(stream);
			_audioPlayer.Volume = _volume;
			_audioPlayer.Play();
			_playbackTimer?.Start();
			PlayerStateLabel.Text = "●  PLAYING";
			PlayerStateLabel.TextColor = Color.FromArgb("#61F2A2");
		}
		catch (Exception exception)
		{
			await DisplayAlertAsync("Audio", $"Nao foi possivel iniciar a faixa: {exception.Message}", "OK");
		}
	}

	private async void OnImportMp3Clicked(object? sender, EventArgs e)
	{
		try
		{
			var result = await FilePicker.Default.PickAsync(new PickOptions
			{
				PickerTitle = "Selecione uma música",
				FileTypes = null
			});

			if (result is null)
				return;
			if (!result.FileName.EndsWith(".mp3", StringComparison.OrdinalIgnoreCase))
			{
				await DisplayAlertAsync("Importar MP3", "Selecione um arquivo .mp3.", "OK");
				return;
			}

			var metadata = ReadMetadata(result);
			var stream = await result.OpenReadAsync();
			var player = _audioManager.CreatePlayer(stream);
			StopCurrentPlayer();
			_playlist.Add((metadata.Title, metadata.Artist, metadata.Album, "MP3 file", "LOCAL MP3", stream));
			_audioPlayer = player;
			_audioPlayer.Volume = _volume;
			_currentTrackIndex = _playlist.Count - 1;
			ResetPlaybackCounter();
			TrackProgressBar.Progress = 0;
			CurrentTimeLabel.Text = "00:00";
			PlayerStateLabel.Text = "●  READY";
			PlayerStateLabel.TextColor = Color.FromArgb("#61F2A2");
			RenderCurrentTrack();
			RenderPlaylist();
		}
		catch (Exception exception)
		{
			await DisplayAlertAsync("Importar MP3", $"Nao foi possivel importar o arquivo: {exception.Message}", "OK");
		}
	}

	private void OnPauseClicked(object? sender, EventArgs e)
	{
		if (_audioPlayer is null)
			return;

		_audioPlayer?.Pause();
		_playbackTimer?.Stop();
		PlayerStateLabel.Text = "●  PAUSED";
		PlayerStateLabel.TextColor = Color.FromArgb("#FFCF66");
	}

	private void OnVolumeDownClicked(object? sender, EventArgs e)
	{
		SetVolume(_volume - 0.1);
	}

	private void OnVolumeUpClicked(object? sender, EventArgs e)
	{
		SetVolume(_volume + 0.1);
	}

	private void OnBackClicked(object? sender, EventArgs e)
	{
		SelectTrack((_currentTrackIndex - 1 + _playlist.Count) % _playlist.Count);
	}

	private void OnNextClicked(object? sender, EventArgs e)
	{
		SelectTrack((_currentTrackIndex + 1) % _playlist.Count);
	}

	private void SelectTrack(int index)
	{
		StopCurrentPlayer();
		_currentTrackIndex = index;
		ResetPlaybackCounter();
		TrackProgressBar.Progress = 0;
		CurrentTimeLabel.Text = "00:00";
		PlayerStateLabel.Text = "●  READY";
		PlayerStateLabel.TextColor = Color.FromArgb("#61F2A2");
		RenderCurrentTrack();
		RenderPlaylist();
	}

	private void StopCurrentPlayer()
	{
		_playbackTimer?.Stop();
		_audioPlayer?.Stop();
		_audioPlayer?.Dispose();
		_audioPlayer = null;
	}

	private void ConfigurePlaybackTimer()
	{
		_playbackTimer = Dispatcher.CreateTimer();
		_playbackTimer.Interval = TimeSpan.FromSeconds(1);
		_playbackTimer.Tick += (_, _) =>
		{
			_elapsedTime = _elapsedTime.Add(TimeSpan.FromSeconds(1));
			CurrentTimeLabel.Text = $"{(int)_elapsedTime.TotalMinutes:00}:{_elapsedTime.Seconds:00}";
		};
	}

	private void ResetPlaybackCounter()
	{
		_playbackTimer?.Stop();
		_elapsedTime = TimeSpan.Zero;
		CurrentTimeLabel.Text = "00:00";
	}

	private void SetVolume(double volume)
	{
		_volume = Math.Clamp(volume, 0.0, 1.0);
		if (_audioPlayer is not null)
			_audioPlayer.Volume = _volume;
		VolumeLabel.Text = $"{_volume:P0}";
	}

	private void RenderCurrentTrack()
	{
		var track = _playlist[_currentTrackIndex];
		TrackTitleLabel.Text = track.Title.ToUpperInvariant();
		TrackArtistLabel.Text = $"{track.Artist}  ·  {track.Genre}";
		TrackAlbumLabel.Text = $"Álbum: {track.Album}";
		DurationLabel.Text = track.Duration;
	}

	private static (string Title, string Artist, string Album) ReadMetadata(FileResult result)
	{
		var fallbackTitle = System.IO.Path.GetFileNameWithoutExtension(result.FileName);
		var fallback = (Title: fallbackTitle, Artist: "Artista desconhecido", Album: "Álbum desconhecido");

		try
		{
			if (string.IsNullOrWhiteSpace(result.FullPath))
				return fallback;

			using var metadataFile = TagLib.File.Create(result.FullPath);
			var title = string.IsNullOrWhiteSpace(metadataFile.Tag.Title) ? fallbackTitle : metadataFile.Tag.Title;
			var artist = metadataFile.Tag.Performers.FirstOrDefault();
			var album = metadataFile.Tag.Album;
			return (
				title,
				string.IsNullOrWhiteSpace(artist) ? fallback.Artist : artist,
				string.IsNullOrWhiteSpace(album) ? fallback.Album : album);
		}
		catch
		{
			return fallback;
		}
	}

	private void RenderPlaylist()
	{
		PlaylistContainer.Clear();
		PlaylistCountLabel.Text = $"{_playlist.Count:00} TRACKS";

		for (var index = 0; index < _playlist.Count; index++)
		{
			var track = _playlist[index];
			var isCurrent = index == _currentTrackIndex;
			var details = new VerticalStackLayout { Spacing = 2 };
			details.Children.Add(new Label { Text = track.Title, TextColor = isCurrent ? Colors.White : Color.FromArgb("#E9E5ED"), FontSize = 14, FontAttributes = FontAttributes.Bold });
			details.Children.Add(new Label { Text = track.Artist, TextColor = isCurrent ? Color.FromArgb("#A59BAF") : Color.FromArgb("#85808A"), FontSize = 11 });

			var row = new Grid
			{
				ColumnDefinitions =
				{
					new ColumnDefinition { Width = GridLength.Auto },
					new ColumnDefinition { Width = GridLength.Star },
					new ColumnDefinition { Width = GridLength.Auto }
				},
				ColumnSpacing = 12
			};
			row.Add(new Label { Text = $"{index + 1:00}", TextColor = isCurrent ? Color.FromArgb("#B96CFF") : Color.FromArgb("#77727E"), FontSize = 12, VerticalTextAlignment = TextAlignment.Center });
			row.Add(details, 1);
			row.Add(new Label { Text = track.Duration, TextColor = isCurrent ? Color.FromArgb("#B9A7C8") : Color.FromArgb("#77727E"), FontSize = 11, VerticalTextAlignment = TextAlignment.Center }, 2);

			var border = new Border
			{
				BackgroundColor = isCurrent ? Color.FromArgb("#2A1B39") : Color.FromArgb("#1B1B1F"),
				Stroke = isCurrent ? Color.FromArgb("#7336A8") : Color.FromArgb("#2D2D35"),
				StrokeThickness = 1,
				Padding = new Thickness(14, 12),
				Content = row,
				StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(10) }
			};
			border.GestureRecognizers.Add(new TapGestureRecognizer { Command = new Command(() => SelectTrack(index)) });
			PlaylistContainer.Children.Add(border);
		}
	}
}
