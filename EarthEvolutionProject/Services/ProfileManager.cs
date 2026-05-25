using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text.Json;
using EarthEvolutionProject.Models;

namespace EarthEvolutionProject.Services
{
    public class ProfileManager
    {
        private static readonly string ConfigPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "users_config.json");
        private AppConfiguration _config = new AppConfiguration();

        public UserProfile? CurrentProfile { get; private set; }

        public ProfileManager()
        {
            LoadConfiguration();
        }

        // Завантаження всіх профілів із файлу
        public void LoadConfiguration()
        {
            try
            {
                if (File.Exists(ConfigPath))
                {
                    string json = File.ReadAllText(ConfigPath);
                    _config = JsonSerializer.Deserialize<AppConfiguration>(json) ?? new AppConfiguration();
                }
                else
                {
                    _config = new AppConfiguration();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Помилка завантаження конфігурації: {ex.Message}");
                _config = new AppConfiguration();
            }
        }

        // Збереження поточного стану у файл
        public void SaveConfiguration()
        {
            try
            {
                if (CurrentProfile != null)
                {
                    var existing = _config.Profiles.FirstOrDefault(p => p.Username.Equals(CurrentProfile.Username, StringComparison.OrdinalIgnoreCase));
                    if (existing != null)
                    {
                        int index = _config.Profiles.IndexOf(existing);
                        _config.Profiles[index] = CurrentProfile;
                    }
                    else
                    {
                        _config.Profiles.Add(CurrentProfile);
                    }
                    _config.LastActiveUsername = CurrentProfile.Username;
                }

                var options = new JsonSerializerOptions { WriteIndented = true };
                string json = JsonSerializer.Serialize(_config, options);

                string? directory = Path.GetDirectoryName(ConfigPath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                File.WriteAllText(ConfigPath, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Помилка збереження конфігурації: {ex.Message}");
            }
        }

        public List<string> GetAllUsernames()
        {
            return _config.Profiles.Select(p => p.Username).ToList();
        }

        public void SelectOrCreateProfile(string username)
        {
            if (string.IsNullOrWhiteSpace(username)) return;

            username = username.Trim();
            var profile = _config.Profiles.FirstOrDefault(p => p.Username.Equals(username, StringComparison.OrdinalIgnoreCase));

            if (profile == null)
            {
                profile = new UserProfile { Username = username };
                _config.Profiles.Add(profile);
            }

            CurrentProfile = profile;
            _config.LastActiveUsername = username;
            SaveConfiguration();
        }

        public bool DeleteProfile(string username)
        {
            if (string.IsNullOrWhiteSpace(username)) return false;

            username = username.Trim();

            var profileToDelete = _config.Profiles.FirstOrDefault(p => p.Username.Equals(username, StringComparison.OrdinalIgnoreCase));

            if (profileToDelete != null)
            {
                _config.Profiles.Remove(profileToDelete);

                if (CurrentProfile != null && CurrentProfile.Username.Equals(username, StringComparison.OrdinalIgnoreCase))
                {
                    CurrentProfile = null;
                }

                if (_config.LastActiveUsername != null && _config.LastActiveUsername.Equals(username, StringComparison.OrdinalIgnoreCase))
                {
                    _config.LastActiveUsername = _config.Profiles.FirstOrDefault()?.Username;
                }

                SaveConfigDirectly();
                return true;
            }

            return false;
        }

        private void SaveConfigDirectly()
        {
            try
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                string json = JsonSerializer.Serialize(_config, options);
                File.WriteAllText(ConfigPath, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Помилка збереження конфігурації при видаленні: {ex.Message}");
            }
        }
    }
}