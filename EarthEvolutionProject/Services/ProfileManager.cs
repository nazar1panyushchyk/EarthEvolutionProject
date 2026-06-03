using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text.Json;
using EarthEvolutionProject.Models;

namespace EarthEvolutionProject.Services
{
    /// <summary>
    /// Сервісний клас для управління профілями користувачів та збереження налаштуваний програми.
    /// Забезпечує серіалізацію, десеріалізацію, створення, вибір та видалення облікових записів.
    /// </summary>
    public class ProfileManager
    {
        /// <summary>
        /// Статичний рядок, що визначає повний шлях до файлу конфігурації users_config.json у кореневій директорії застосунку.
        /// </summary>
        private static readonly string ConfigPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "users_config.json");

        /// <summary>
        /// Об'єкт конфігурації застосунку, що містить список усіх наявних профілів та ім'я останнього активного користувача.
        /// </summary>
        private AppConfiguration _config = new AppConfiguration();

        /// <summary>
        /// Повертає або встановлює об'єкт поточного активного профілю користувача в системі.
        /// </summary>
        public UserProfile? CurrentProfile { get; private set; }

        /// <summary>
        /// Конструктор менеджера профілів. При ініціалізації автоматично запускає процедуру завантаження конфігураційного файлу.
        /// </summary>
        public ProfileManager()
        {
            LoadConfiguration();
        }

        /// <summary>
        /// Завантажує та десеріалізує дані з файлу конфігурації. У разі відсутності файлу або помилки ініціалізує нову порожню конфігурацію.
        /// </summary>
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

        /// <summary>
        /// Синхронізує стан поточного активного профілю із загальним списком конфігурації та виконує запис структури у файл у форматованому вигляді JSON.
        /// </summary>
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

        /// <summary>
        /// Повертає список імен усіх зареєстрованих у системі користувачів для їх подальшого виведення в графічному інтерфейсі.
        /// </summary>
        /// <returns>Колекція текстових рядків з іменами користувачів.</returns>
        public List<string> GetAllUsernames()
        {
            return _config.Profiles.Select(p => p.Username).ToList();
        }

        /// <summary>
        /// Виконує пошук профілю за іменем та робить його активним. Якщо профіль із таким іменем відсутній, створює новий обліковий запис.
        /// </summary>
        /// <param name="username">Текстове ім'я користувача для вибору або створення.</param>
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

        /// <summary>
        /// Видаляє вказаний профіль із загальної конфігурації, скидає поточний активний профіль при збігу імен та безпосередньо перезаписує файл.
        /// </summary>
        /// <param name="username">Текстове ім'я користувача, профіль якого підлягає видаленню.</param>
        /// <returns>Значення true, якщо видалення пройшло успішно; інакше – false.</returns>
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

        /// <summary>
        /// Виконує пряму примусову серіалізацію поточного стану об'єкта конфігурації у файл без додаткових перевірок активного профілю.
        /// </summary>
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